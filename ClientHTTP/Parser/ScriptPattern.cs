
namespace ClientHTTP.Parser;

public class ScriptPattern : ParserPattern
{
    public override string Pattern => @"<script.*src=[""']([^""']+)[""'].*?>";
}
