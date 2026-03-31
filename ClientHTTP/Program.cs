using ClientHTTP.QueryBuilder;
using ClientHTTP.QueryBuilder.QueryMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP;

internal class Program
{
    public static async Task Main(string[] args)
    {
        while (true) 
        {
            Console.Clear();

            Console.WriteLine("Search HTTP/HTTPS URL:");

            string host = Console.ReadLine()!.Trim();

            HTTPQueryBuilder queryBuilder = new HTTPQueryBuilder()
            .DefineConnection("close")
            .DefineHost(host)
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

            queryBuilder.OnServerResponse += (body) =>
            {
                Console.WriteLine("Response received:\n");
                Console.WriteLine(body);
            };

            await queryBuilder.SendAsync();

            Console.ReadLine();
        }
    }
}
