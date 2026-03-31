namespace ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata
{
    public class Mozilla : UserAgentDecorator
    {
        public Mozilla(SystemInfo browser) : base(browser) { }

        public override string Stringify()
        {
            return base.Stringify() + " Mozilla/5.0";
        }
    }
}
