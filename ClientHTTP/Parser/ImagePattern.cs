namespace ClientHTTP.Parser;

internal class ImagePattern : ParserPattern
{
    public override string Pattern => @"<img.*src=[""']([^""']+)[""'].*?>";
}
