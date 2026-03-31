namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public class SafariBrowser : UserAgentDecorator
    {
        public SafariBrowser(SystemInfo browser) : base(browser) { }

        public override string Stringify()
        {
            return base.Stringify() + " Safari/537.36";
        }
    }
}
