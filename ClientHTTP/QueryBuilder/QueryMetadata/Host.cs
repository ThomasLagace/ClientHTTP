namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class Host
    {
        public Uri Domain { get; init; }

        public Host(Uri domain) 
        {
            Domain = domain;
        }

        public string Stringify()
        {
            return "Host: " + Domain.Host;
        }
    }
}
