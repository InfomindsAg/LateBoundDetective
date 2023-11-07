namespace LateBoundDetective.Analyzers;

public class AnalyzerFileResult
{
    public string FilePath { get; set; } = "";
    public List<AnalyzerFileResultItem> Items { get; set; } = new();
}

public class AnalyzerFileResultItem
{
    public int Line { get; set; }
    public string ShortCode { get; set; } = "";
    public string Message { get; set; } = "";

}