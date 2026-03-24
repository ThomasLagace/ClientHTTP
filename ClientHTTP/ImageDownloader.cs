namespace ClientHTTP;

public static class ImageDownloader
{
    public static async Task DownloadImagesAsync(List<Uri> imageUris, string downloadPath)
    {
        List<Task> downloadImagesTasks = new();
        foreach (Uri uri in imageUris)
        {
            downloadImagesTasks.Add(DownloadImageAsync(uri, downloadPath));
        }

        await Task.WhenAll(downloadImagesTasks);
    }

    public static async Task DownloadImageAsync(Uri imageUri, string downloadPath)
    {
        using (HttpClient client = new())
        {
            try
            {
                byte[] imageBytes = await client.GetByteArrayAsync(imageUri);
                File.WriteAllBytes(downloadPath + imageUri.Segments.Last(), imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
