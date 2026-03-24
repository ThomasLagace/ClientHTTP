using ClientHTTP.QueryBuilder.QueryMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder
{
    public class HTTPQueryBuilder
    {
        public string ProtocolVersion { get; init; }
        private readonly Query _query = new();

        public HTTPQueryBuilder(string protocolVersion = "HTTP/1.1")
        {
            ProtocolVersion = protocolVersion;
        }

        public async Task SendAsync()
        {
            if (!TryBuildQuery(out string query)) return;


        }

        public HTTPQueryBuilder DefineMethod(Method method)
        {
            this._query.SetMethod(method);
            return this;
        }

        public HTTPQueryBuilder DefineIndex(string path)
        {
            this._query.SetIndex(path);
            return this;
        }

        public HTTPQueryBuilder DefineHost(string host)
        {
            this._query.SetHost(host);
            return this;
        }

        public HTTPQueryBuilder DefineConnection(string behaviour)
        {
            this._query.SetConnection(behaviour);
            return this;
        }

        public HTTPQueryBuilder DefineUserAgent(string browser)
        {
            //make builder via SupportedBrowsers

            this._query.SetUserAgent(browser);
            return this;
        }

        private bool TryBuildQuery(out string queryString) 
        {
            queryString = string.Empty;

            if (String.IsNullOrEmpty(_query.Method?.GetCommand()))
            {
                Console.WriteLine("Query dropped - Method unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.Index?.Path))
            {
                Console.WriteLine("Query dropped - Index path unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.Host?.Domain))
            {
                Console.WriteLine("Query dropped - Host domain unspecified.");
                return false;
            }

            if (String.IsNullOrEmpty(_query.Connection?.Behaviour))
            {
                Console.WriteLine("Query dropped - Connection cleanup behaviour unspecified.");
                return false;
            }

            //format full query in queryString

            return true;
        }

        private async Task HandleResponseAsync(string response) 
        {
            
        }

        private async Task ParseHttpAsync() 
        {

        }
        private async Task ParseHttpsAsync() 
        {
        
        }
    }
}
