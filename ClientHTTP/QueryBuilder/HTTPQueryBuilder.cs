using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;
using System.IO.Compression;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientHTTP.QueryBuilder
{
    public class HTTPQueryBuilder(string method = "GET", string index = "/", string protocolVersion = "HTTP/1.1", bool autoRedirect = false)
    {
        public string ProtocolVersion { get; init; } = protocolVersion;
        public string Index { get; init; } = index;
        public string Method { get; init; } = method;
        private readonly Query _query = new();
        private readonly bool _autoRedirect = autoRedirect;
        public event Action<string>? OnServerResponse;

        public async Task SendAsync()
        {
            if (!TryBuildQuery(out string? query)) 
                return;

            Console.WriteLine("Sending query:\n" + query);

            using TcpClient client = new(_query.Host!.Domain.Host, _query.Host!.Domain.Port);
            Stream stream = client.GetStream();

            if (_query.Host!.Domain.Scheme == Uri.UriSchemeHttps)
            {
                await HandleHttpsAsync(stream, _query.Host!.Domain, query!);
            }
            else
            {
                await HandleHttpAsync(stream, query!);
            }
        }

        public HTTPQueryBuilder DefineHost(string host)
        {
            if (!Uri.TryCreate(host, UriKind.Absolute, out Uri? uri)) 
            {
                Console.WriteLine("Invalid URL.");
            }

            this._query.SetHost(uri!);
            return this;
        }

        public HTTPQueryBuilder DefineConnection(string behaviour)
        {
            this._query.SetConnection(behaviour);
            return this;
        }

        public HTTPQueryBuilder DefineUserAgent(SystemInfo info)
        {
            this._query.SetUserAgent(info);
            return this;
        }

        public HTTPQueryBuilder DefineEncoding(SystemInfo info) 
        {
            this._query.SetEncoding(info);
            return this;
        }

        private bool IsQueryValid()
        {
            if (String.IsNullOrEmpty(Method))
            {
                Console.WriteLine("Query dropped - Method unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(Index))
            {
                Console.WriteLine("Query dropped - Index path unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(ProtocolVersion))
            {
                Console.WriteLine("Query dropped - Protocol version unspecified.");
                return false;
            }

            if (_query.Host?.Domain == null
                || (_query.Host!.Domain.Scheme != Uri.UriSchemeHttp
                && _query.Host!.Domain.Scheme != Uri.UriSchemeHttps))
            {
                Console.WriteLine("Query dropped - Host domain unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.Connection?.Behaviour))
            {
                Console.WriteLine("Query dropped - Connection cleanup behaviour unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.UserAgent?.Stringify())
                || _query.UserAgent?.Stringify() == "User-Agent:") 
            {
                Console.WriteLine("Query dropped - User agents unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.Encoding?.Stringify())) 
            {
                Console.WriteLine("Query dropped - Encoding unspecified.");
                return false;
            }

            return true;
        }

        private bool TryBuildQuery(out string? queryString)
        {
            queryString = null;

            if (!IsQueryValid())
                return false;

            queryString += $"{Method} {Index} {ProtocolVersion}\r\n";
            queryString += $"{_query.Host?.Stringify()}\r\n";
            queryString += $"{_query.Connection?.Stringify()}\r\n";
            queryString += $"{_query.UserAgent?.Stringify()}\r\n";
            queryString += $"{_query.Encoding?.Stringify()}\r\n";
            queryString += "\r\n";

            return true;
        }

        private static bool TryGetHeader(string headers, string header, out string? value)
        {
            value = null;

            string pattern = @$"^{Regex.Escape(header)}:\s*([^\r\n]+)";
            Match match = Regex.Match(headers, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (!match.Success)
                return false;

            value = match.Groups[1].Value.Trim();

            return true;
        }

        private async Task HandleStatusAsync(string header)
        {
            string responseCode = header.Split("\r\n")[0].Split(' ')[1];

            Console.Write(responseCode + " - ");
            
            switch (responseCode[0]) 
            {
                case '2':
                    Console.WriteLine("Success\n");
                    break;
                case '3':
                    Console.WriteLine("Redirect\n");

                    if (!_autoRedirect) 
                    {
                        if (TryGetHeader(header, "Location", out string? location)) 
                        {
                            Console.WriteLine($"Redirecting to {location} - Proceed?\nYes | No");

                            string? shouldRedirect = Console.ReadLine()?.Trim().ToUpper();

                            if (shouldRedirect == "YES")
                            {
                                DefineHost(location!);
                                await SendAsync();
                            }
                        }
                    }
                    break;
                case '4':
                    Console.WriteLine("Client error\n");
                    break;
                case '5':
                    Console.WriteLine("Server error\n");
                    break;
            }
        }

        private async Task HandleHttpAsync(Stream stream, string query)
        {
            using StreamWriter writer = new(stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteAsync(query);

            using StreamReader reader = new(stream, new UTF8Encoding(false));
            byte[] response = await GetResponseAsync(stream);

            await HandleResponseAsync(response);
        }

        private async Task HandleHttpsAsync(Stream stream, Uri uri, string query)
        {
            SslStream sslStream = new(stream, false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate!), null);

            sslStream.AuthenticateAsClient(uri.Host);
            stream = sslStream;

            using StreamWriter writer = new(stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteAsync(query);

            using StreamReader reader = new(stream, new UTF8Encoding(false));
            byte[] response = await GetResponseAsync(stream);

            await HandleResponseAsync(response);
        }

        private async Task HandleResponseAsync(byte[] response)
        {
            Dictionary<string, Func<byte[], Task<string>>> decompressionDispatcher = new Dictionary<string, Func<byte[], Task<string>>>()
            {
                { "gzip", DecompressGzipAsync },
                { "br", DecompressBrotliAsync }
            };

            if (TryGetSplitResponse(response, out (byte[] Header, byte[] Body)? splitResponse))
            {
                byte[] header = splitResponse!.Value.Header;
                byte[] body = splitResponse!.Value.Body;

                string headerString = Encoding.UTF8.GetString(header);
                string bodyString = Encoding.UTF8.GetString(body);

                if (TryGetHeader(headerString, "Transfer-Encoding", out string? transferEncoding)
                    && transferEncoding!.Contains("chunked"))
                {
                    body = await OrganizeResponseBodyAsync(body);
                }

                if (TryGetHeader(headerString, "Content-Encoding", out string? contentEncoding))
                {
                    bodyString = await decompressionDispatcher[contentEncoding!.ToLower()](body);
                }

                OnServerResponse?.Invoke(bodyString);
                await HandleStatusAsync(headerString);
            }
        }

        private static async Task<string> DecompressGzipAsync(byte[] bodyBytes) 
        {
            using MemoryStream compressedStream = new(bodyBytes);
            using GZipStream decompressor = new(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new(decompressor, Encoding.UTF8);

            return await reader.ReadToEndAsync();
        }

        private static async Task<string> DecompressBrotliAsync(byte[] bodyBytes) 
        {
            using MemoryStream compressedStream = new(bodyBytes);
            using BrotliStream decompressor = new(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new(decompressor, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        private static bool TryGetSplitResponse(byte[] response, out (byte[] Header, byte[] Body)? splitResponse) 
        {
            splitResponse = null;

            byte[] delimiter = Encoding.UTF8.GetBytes("\r\n\r\n");

            ReadOnlySpan<byte> responseSpan = response;
            int index = responseSpan.IndexOf(delimiter);

            if (index != -1)
            {
                splitResponse = (
                    responseSpan[..index].ToArray(), 
                    responseSpan[(index + 4)..].ToArray()
                );

                return true;
            }
            else 
            {
                Console.WriteLine("Parsing error - Failed to find header delimiter.\n");
                return false;
            }
        }

        private static async Task<byte[]> GetResponseAsync(Stream stream)
        {
            using MemoryStream memStream = new();
            byte[] buffer = new byte[4096];

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0) 
            {
                await memStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            byte[] responseBytes = memStream.ToArray();
            return responseBytes;
        }

        private static async Task<byte[]> OrganizeResponseBodyAsync(byte[] chunkedBody) 
        {
            using MemoryStream memStream = new();
            int index = 0;
            while (index < chunkedBody.Length)
            {
                int chunkSizeEnd = Array.IndexOf(chunkedBody, (byte)'\r', index);
                if (chunkSizeEnd == -1)
                    break;

                string chunkSizeHex = Encoding.UTF8.GetString(chunkedBody, index, chunkSizeEnd - index);

                if (!int.TryParse(chunkSizeHex, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize) || chunkSize == 0) 
                {
                    Console.WriteLine("Parsing error - Failed to obtain chunk size.");
                    break;
                }
                
                index = chunkSizeEnd + 2;
                await memStream.WriteAsync(chunkedBody.AsMemory(index, chunkSize));

                index += chunkSize + 2;
            }

            return memStream.ToArray();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
