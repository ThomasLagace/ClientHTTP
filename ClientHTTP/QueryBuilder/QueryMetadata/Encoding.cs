using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    internal class Encoding : SystemInfo
    {
        public string Stringify()
        {
            return "Accept-Encoding:";
        }
    }
}
