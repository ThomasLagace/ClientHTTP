namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public class OperatingSystem : UserAgentDecorator
    {
        public OperatingSystem(SystemInfo info) : base(info) { }

        public override string Stringify()
        {
            return base.Stringify() + " (Windows NT 10.0; Win64; x64)";
        }
    }
}
