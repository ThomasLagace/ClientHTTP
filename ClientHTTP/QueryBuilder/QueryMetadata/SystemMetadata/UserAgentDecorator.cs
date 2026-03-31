namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public abstract class UserAgentDecorator : SystemInfo
    {
        private SystemInfo browser;

        public UserAgentDecorator(SystemInfo browser)
        {
            this.browser = browser;
        }

        public virtual string Stringify()
        {
            return browser.Stringify();
        }
    }
}
