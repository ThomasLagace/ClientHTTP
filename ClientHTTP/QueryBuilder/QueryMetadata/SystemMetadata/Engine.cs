namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public class Engine : UserAgentDecorator
    {
        public Engine(SystemInfo browser) : base(browser) { }

        public override string Stringify()
        {
            return base.Stringify() + " AppleWebKit/537.36 (KHTML, like Gecko)";
        }
    }
}
