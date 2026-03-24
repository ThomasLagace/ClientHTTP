using ClientHTTP.Parser;

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
        List<string> imageLinks = _imageParser.ParseHTML(html);
        List<Uri> imageUris = new();
        foreach (string imageLink in imageLinks)
        {
            if (imageLink.StartsWith("http"))
                imageUris.Add(new(imageLink));
            else
                imageUris.Add(new(uri, imageLink));
        }
        await ImageDownloader.DownloadImagesAsync(imageUris, downloadPath);
        // TODO: Add the download of other resources
    }
}
