namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class Connection
    {
        public string Behaviour { get; init; }

        public Connection(string behaviour)
        {
            Behaviour = behaviour;
        }

        public string Stringify() 
        {
            return "Connection: " + Behaviour;
        }
    }
}
