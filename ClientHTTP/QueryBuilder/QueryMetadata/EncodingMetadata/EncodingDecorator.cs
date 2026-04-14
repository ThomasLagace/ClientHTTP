using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP.QueryBuilder.QueryMetadata.EncodingMetadata
{
    public class EncodingDecorator : SystemInfo
    {
        private readonly SystemInfo _encoding;

        public EncodingDecorator(SystemInfo encoding)
        {
            _encoding = encoding;
        }

        public virtual string Stringify()
        {
            return _encoding.Stringify();
        }
    }
}
