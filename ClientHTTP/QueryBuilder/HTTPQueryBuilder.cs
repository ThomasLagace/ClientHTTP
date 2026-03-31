using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ClientHTTP.QueryBuilder
{
    public class HTTPQueryBuilder(string method = "GET", string index = "/", string protocolVersion = "HTTP/1.1")
    {
        public string ProtocolVersion { get; init; } = protocolVersion;
        public string Index { get; init; } = index;
        public string Method { get; init; } = method;
        private readonly Query _query = new();
        public event Action<string>? OnServerResponse;

        public async Task SendAsync()
        {
            if (!TryBuildQuery(out string query)) 
                return;

            Console.WriteLine("Sending query:\n" + query);

            using TcpClient client = new TcpClient(_query.Host!.Domain.Host, _query.Host!.Domain.Port);
            Stream stream = client.GetStream();

            string response;

            if (_query.Host!.Domain.Scheme == Uri.UriSchemeHttps)
            {
                response = await HandleHttpsAsync(stream, _query.Host!.Domain, query);
            }
            else
            {
                response = await HandleHttpAsync(stream, query);
            }

            OnServerResponse?.Invoke(response.Split("\r\n\r\n")[1]);
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

        private bool TryBuildQuery(out string queryString)
        {
            queryString = string.Empty;

            if (!IsQueryValid())
                return false;

            queryString += $"{Method} {Index} {ProtocolVersion}\r\n";
            queryString += $"{_query.Host?.Stringify()}\r\n";
            queryString += $"{_query.Connection?.Stringify()}\r\n";
            queryString += $"{_query.UserAgent?.Stringify()}\r\n";
            queryString += "\r\n";

            return true;
        }

        private static async Task<string> HandleHttpAsync(Stream stream, string query)
        {
            using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteAsync(query);

            using StreamReader reader = new StreamReader(stream, new UTF8Encoding(false));
            string response = await reader.ReadToEndAsync();

            return response;
        }

        private static async Task<string> HandleHttpsAsync(Stream stream, Uri uri, string query)
        {
            SslStream sslStream = new SslStream(stream, false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate!), null);

            sslStream.AuthenticateAsClient(uri.Host);
            stream = sslStream;

            using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteAsync(query);

            using StreamReader reader = new StreamReader(stream, new UTF8Encoding(false));
            string response = await reader.ReadToEndAsync();

            return response;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
