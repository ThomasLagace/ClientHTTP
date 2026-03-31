using ClientHTTP.QueryBuilder;
using ClientHTTP.QueryBuilder.QueryMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP;

internal class Program
{
    static void Main(string[] args)
    {
        HTTPQueryBuilder queryBuilder = new HTTPQueryBuilder()
            .DefineConnection("close")
            .DefineUserAgent(
                new SafariBrowser(
                    new ChromeBrowser(
                        new Engine(
                            new QueryBuilder.QueryMetadata.SystemMetadata.OperatingSystem(
                                new Mozilla(
                                    new UserAgent()
                                    )
                                )
                            )
                        )
                    )
                );

        queryBuilder.OnServerResponse += (response) =>
        {
            Console.WriteLine("Response received:");
            Console.WriteLine(response);
        };

        Console.WriteLine("Enter HTTP/HTTPS URL:");

        string host = Console.ReadLine()!.Trim();
        queryBuilder.DefineHost(host);

        queryBuilder.OnServerResponse += async (body) =>
        {
            if (Uri.TryCreate(host, UriKind.Absolute, out Uri? uri))
                await ResourceSerializer.SerializeAllResources(uri!, body, uri!.Host.ToString() + "/");
        };

        _ = queryBuilder.SendAsync();

        Console.ReadLine();
    }
}
