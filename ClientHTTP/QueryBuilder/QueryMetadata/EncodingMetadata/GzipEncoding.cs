using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder.QueryMetadata.EncodingMetadata
{
    public class GzipEncoding : EncodingDecorator
    {
        public GzipEncoding(SystemInfo encoding) : base(encoding) { }

        public override string Stringify()
        {
            string encoding = base.Stringify();

            return encoding switch
            {
                "Accept-Encoding:" => encoding + " gzip",
                _ => encoding + ", gzip",
            };
        }
    }
}
