namespace ClientHTTP.Parser;

internal class StylesheetPattern : ParserPattern
{
    public override string Pattern => @"<link.*rel=[""']stylesheet[""'].*href=[""']([^""']+)[""'].*?>";
}
