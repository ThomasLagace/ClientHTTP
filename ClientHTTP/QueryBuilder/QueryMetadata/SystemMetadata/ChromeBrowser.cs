namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public class ChromeBrowser : UserAgentDecorator
    {
        public ChromeBrowser(SystemInfo browser) : base(browser) { }

        public override string Stringify()
        {
            return base.Stringify() + " Chrome/89.0.4389.90";
        }
    }
}
