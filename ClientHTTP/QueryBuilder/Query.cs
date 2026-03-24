using ClientHTTP.QueryBuilder.QueryMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder
{
    public class Query
    {
        public Method? Method { get; private set; }
        public QueryMetadata.Index? Index { get; private set; }
        public Host? Host { get; private set; }
        public Connection? Connection { get; private set; }
        public UserAgent? UserAgent { get; private set; }

        public void SetMethod(Method method) 
        {
            Method = method;
        }

        public void SetIndex(string path) 
        {
            Index = new QueryMetadata.Index(path);
        }

        public void SetConnection(string behaviour) 
        {
            Connection = new Connection(behaviour);
        }

        public void SetHost(string domain) 
        {
            Host = new Host(domain);
        }

        public void SetUserAgent(string browser)
        { 
            UserAgent = new UserAgent(browser);
        }
    }
}
