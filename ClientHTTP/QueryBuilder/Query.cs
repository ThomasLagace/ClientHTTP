using ClientHTTP.QueryBuilder.QueryMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder
{
    public class Query
    {
        public Host? Host { get; private set; }
        public Connection? Connection { get; private set; }
        public SystemInfo? UserAgent { get; private set; }

        public void SetConnection(string behaviour) 
        {
            Connection = new Connection(behaviour);
        }

        public void SetHost(Uri domain) 
        {
            Host = new Host(domain);
        }

        public void SetUserAgent(SystemInfo info)
        {
            UserAgent = info;
        }
    }
}
