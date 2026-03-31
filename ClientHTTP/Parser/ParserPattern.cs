using System.Text.RegularExpressions;

namespace ClientHTTP.Parser;

public abstract class ParserPattern
{
    public abstract string Pattern { get; }

    public List<string> Parse(string html)
    {
        MatchCollection stylesheetTags = Regex.Matches(html, Pattern, RegexOptions.IgnoreCase);
        List<string> stylesheetLinks = new();
        foreach (Match match in stylesheetTags)
        {
            stylesheetLinks.Add(match.Groups[1].Value);
        }
        return stylesheetLinks;
    }
}
