using ClientHTTP.QueryBuilder;
using ClientHTTP.QueryBuilder.QueryMetadata.QueryMethod;

namespace ClientHTTP;

internal class Program
{
    static void Main(string[] args)
    {
        HTTPQueryBuilder queryBuilder = new HTTPQueryBuilder()
            .DefineMethod(new GetMethod())
            .DefineIndex("/")
            .DefineHost("www.example.com")
            .DefineConnection("close");

        _ = queryBuilder.SendAsync();
    }
}
