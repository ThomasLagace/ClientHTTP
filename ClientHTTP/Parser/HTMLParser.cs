namespace ClientHTTP.Parser;

public class HTMLParser
{
    List<ParserPattern> _patterns;

    public HTMLParser(List<ParserPattern> patterns)
    {
        _patterns = patterns;
    }

    public List<string> ParseHTML(string html)
    {
        List<string> result = new();
        foreach (ParserPattern pattern in _patterns)
        {
            result.AddRange(pattern.Parse(html));
        }
        return result;
    }
}
