using ClientHTTP.QueryBuilder;
using ClientHTTP.QueryBuilder.QueryMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP;

internal class Program
{
    public static void Main(string[] args)
    {
        string host = "";
        HTTPQueryBuilder queryBuilder;

        while (true)
        {
            queryBuilder = new HTTPQueryBuilder()
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

            queryBuilder.OnServerResponse += (body) =>
            {
                Console.WriteLine("Response received:");
                Console.WriteLine(body);

                host = "";
            };

            queryBuilder.OnRedirect += async (location) =>
            {
                Console.WriteLine($"Redirecting to {location} - Proceed?\nYes | No");

                string? shouldRedirect = Console.ReadLine()?.Trim().ToUpper();

                if (shouldRedirect == "YES") 
                {
                    queryBuilder.DefineHost(location);
                    await queryBuilder.SendAsync();
                }
            };

            if (host == "")
            {
                Console.WriteLine("Search HTTP/HTTPS URL:");

                host = Console.ReadLine()!.Trim();
            }

            if (host == "exit") 
            {
                break;
            }

            queryBuilder.DefineHost(host);
            _ = queryBuilder.SendAsync();

            Console.ReadLine();

            Console.Clear();
            host = "";
        }
    }
}
