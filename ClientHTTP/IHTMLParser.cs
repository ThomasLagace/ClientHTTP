namespace ClientHTTP;

public interface IHTMLParser
{
    public Task ParseHTMLAsync(string html, Uri uri);
}
