using LateBoundDetective.CacheObjects;
using XSharp.VsParser.Helpers.Parser;

namespace LateBoundDetective.Analyzers;

public interface IAnalyzer
{
    void Execute(string filePath, AbstractSyntaxTree tree, AnalyzerFileResult result);
}