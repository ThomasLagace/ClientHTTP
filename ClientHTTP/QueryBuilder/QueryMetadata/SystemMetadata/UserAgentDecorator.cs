namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public abstract class UserAgentDecorator : SystemInfo
    {
        private readonly SystemInfo _browser;

        public UserAgentDecorator(SystemInfo browser)
        {
            _browser = browser;
        }

        public virtual string Stringify()
        {
            return _browser.Stringify();
        }
    }
}
