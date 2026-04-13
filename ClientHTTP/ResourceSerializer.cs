using ClientHTTP.Parser;
using ClientHTTP.QueryBuilder;
using ClientHTTP.QueryBuilder.QueryMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.EncodingMetadata;
using ClientHTTP.QueryBuilder.QueryMetadata.SystemMetadata;

namespace ClientHTTP;
public static class ResourceSerializer
{
    private static HTMLParser _imageParser = new([new ImagePattern()]);
    private static HTMLParser _allResourceParser = new([
        new ScriptPattern(),
        new StylesheetPattern(),
    ]);

    public static async Task SerializeResourcesAsync(Uri uri, string html)
    {
        try
        {
            Directory.CreateDirectory(uri.Host);
            await Task.WhenAll(
                File.WriteAllTextAsync(uri.Host + "/page.html", html),
                DownloadRelatedResourcesAsync(uri, html, uri.Host)
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static async Task DownloadRelatedResourcesAsync(Uri uri, string html, string downloadPath)
    {
        Console.WriteLine("Downloading all resources related to " + uri.ToString() + " on local " + downloadPath);
        List<string> imageLinks = _imageParser.ParseHTML(html);
        List<Uri> imageUris = new();
        foreach (string imageLink in imageLinks)
        {
            if (imageLink.StartsWith("http"))
                imageUris.Add(new(imageLink));
            else
                imageUris.Add(new(uri, imageLink));
        }

        List<string> resourceLinks = _allResourceParser.ParseHTML(html);
        List<Uri> resourceUris = new();
        foreach (string resourceLink in resourceLinks)
        {
            if (resourceLink.StartsWith("http"))
                resourceUris.Add(new(resourceLink));
            else
                resourceUris.Add(new(uri, resourceLink));
        }

        var fetchAllResources = async () =>
        {
            List<Task> tasks = new();
            foreach (Uri uri in resourceUris)
            {
                HTTPQueryBuilder httpQueryBuilder = new HTTPQueryBuilder(index: uri.AbsolutePath.ToString(), autoRedirect: true)
                    .DefineHost(uri.Scheme + "://" + uri.Host)
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
                    )
                    .DefineEncoding(
                        new BrotliEncoding(
                            new GzipEncoding(
                                new Encoding()
                            )
                        )
                    );

                httpQueryBuilder.OnServerResponse += async (data) =>
                {
                    await File.WriteAllTextAsync(downloadPath + uri.Segments.Last(), data);
                };

                tasks.Add(httpQueryBuilder.SendAsync());
            }
            await Task.WhenAll(tasks);
        };

        await Task.WhenAll(
            ImageDownloader.DownloadImagesAsync(imageUris, downloadPath),
            fetchAllResources()
        );
    }

    public static async Task SerializeAllResources(Uri uri, string html, string downloadPath)
    {
        Console.WriteLine("Downloading all at " + uri.ToString());
        Directory.CreateDirectory(downloadPath);
        await Task.WhenAll(
            File.WriteAllTextAsync(downloadPath + "index.html", html),
            DownloadRelatedResourcesAsync(uri, html, downloadPath)
        );
    }
}
