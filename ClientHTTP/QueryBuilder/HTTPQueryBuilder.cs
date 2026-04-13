using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientHTTP.QueryBuilder
{
    public class HTTPQueryBuilder(string method = "GET", string index = "/", string protocolVersion = "HTTP/1.1")
    {
        public string ProtocolVersion { get; init; } = protocolVersion;
        public string Index { get; init; } = index;
        public string Method { get; init; } = method;
        private readonly Query _query = new();
        public event Action<string>? OnServerResponse;

        public async Task SendAsync(bool autoRedirect = false)
        {
            if (!TryBuildQuery(out string? query)) 
                return;

            Console.WriteLine("Sending query:\n" + query);

            using TcpClient client = new(_query.Host!.Domain.Host, _query.Host!.Domain.Port);
            Stream stream = client.GetStream();

            string response;

            if (_query.Host!.Domain.Scheme == Uri.UriSchemeHttps)
            {
                await HandleHttpsAsync(stream, _query.Host!.Domain, query!);
            }
            else
            {
                await HandleHttpAsync(stream, query!);
            }

            // -- MAKE METHODS --
            // OnServerResponse?.Invoke(splitResponse[1]);
            // await HandleResponseAsync(splitResponse[0], autoRedirect);
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

        private static bool TryGetHeader(string header, out string? value)
        {
            value = null;

            string pattern = @"^Location:\s*(.+)$";
            Match match = Regex.Match(header, pattern);

            if (match.Success) 
            {
                value = match.Value;
                return true;
            }

            return false;
        }

        private async Task HandleResponseAsync(string header, bool autoRedirect)
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

                    if (!autoRedirect) 
                    {
                        string location = GetRedirectLocation(header)!;
                        Console.WriteLine($"Redirecting to {location} - Proceed?\nYes | No");

                        string? shouldRedirect = Console.ReadLine()?.Trim().ToUpper();

                        if (shouldRedirect == "YES")
                        {
                            DefineHost(location);
                            await SendAsync();
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

        private static async Task HandleHttpAsync(Stream stream, string query)
        {
            using StreamWriter writer = new(stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteAsync(query);

            using StreamReader reader = new(stream, new UTF8Encoding(false));
            byte[] response = await GetResponseAsync(stream);

            await HandleResponseAsync(response);
        }

        private static async Task HandleHttpsAsync(Stream stream, Uri uri, string query)
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

        private static async Task HandleResponseAsync(byte[] response) 
        {
            (byte[] header, byte[] body) = GetSplitResponse(response);


        }

        private static (byte[], byte[]) GetSplitResponse(byte[] repsonse) 
        {
            byte[] delimiter = Encoding.UTF8.GetBytes("\r\n\r\n");

            // temporary return value
            return ([], []);
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

        private static byte[] OrganizeResponseBody(byte[] chunkedBody) 
        {
            using MemoryStream memStream = new();
            int index = 0;

            while (index < chunkedBody.Length) 
            {
                int chunkSizeEnd = Array.IndexOf(chunkedBody, (byte)'\r', index);
                if (chunkSizeEnd == -1) break;

                string chunkSizeHex = Encoding.UTF8.GetString(chunkedBody, index, chunkSizeEnd - index);
                if (!int.TryParse(chunkSizeHex, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize) || chunkSize == 0)
                    break;

                index = chunkSizeEnd + 2;
                memStream.Write(chunkedBody, index, chunkSize);

                index += chunkSize + 2;
            }

            return memStream.ToArray();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static string? GetRedirectLocation(string header) 
        {
            string[] headerLine = header.Split("\r\n");

            foreach (string line in headerLine) 
            {
                string[] splitLine = line.Split(' ');

                if (splitLine[0] == "Location:") 
                {
                    string location = splitLine[1];

                    if (location[^1] == '/')
                    {
                        location = location[..^1];
                    }

                    return location;
                }
            }

            return null;
        }
    }
}
