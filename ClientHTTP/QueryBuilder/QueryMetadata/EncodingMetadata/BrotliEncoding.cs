using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder.QueryMetadata.EncodingMetadata
{
    public class BrotliEncoding : EncodingDecorator
    {
        public BrotliEncoding(SystemInfo encoding) : base(encoding) { }

        public override string Stringify()
        {
            string encoding = base.Stringify();

            return encoding switch
            {
                "Accept-Encoding:" => encoding + " br",
                _ => encoding + ", br",
            };
        }
    }
}
