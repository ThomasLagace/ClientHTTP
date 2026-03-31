using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class UserAgent : SystemInfo
    {
        public string Stringify()
        {
            return "User-Agent:";
        }
    }
}
