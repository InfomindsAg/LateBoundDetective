using LateBoundDetective.CacheObjects;
using LateBoundDetective.Helpers;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;

namespace LateBoundDetective.Analyzers;

public class RegServerOpenAnalyzer : ClassReferencedAnalyzer
{

    public RegServerOpenAnalyzer(ClassHierarchy classHierarchy, string projectPath) : base(classHierarchy, projectPath)
    { }


    public override void Execute(string filePath, AbstractSyntaxTree tree, AnalyzerFileResult result)
    {
        foreach (var methodCallContext in tree.WhereType<MethodCallContext>())
        {
            var values = methodCallContext.ToValues();
            if (!"open".EqualsIgnoreCase(values.AccessMember?.MemberName))
                continue;

            var accessExpression = values.AccessMember!.AccessExpression.TrimEnd(' ', '(', ')');
            var classNameArgument = values.Arguments.ElementAtOrDefault(1)?.Value?.Trim() ?? "";
            if (("GetRegServer".EqualsIgnoreCase(accessExpression) || "GetRegServerReal".EqualsIgnoreCase(accessExpression)) && classNameArgument.StartsWith('#'))
            {
                var (isReferenced, shortCode, msg) = IsClassNameAvailableAsTyped(methodCallContext, classNameArgument);
                if (!isReferenced)
                    continue;

                shortCode = $"Rs{shortCode}";
                msg = $"GetRegServer Open with {msg}";

                result.Items.Add(new AnalyzerFileResultItem { Line = methodCallContext.Start.Line, ShortCode = shortCode, Message = msg});
            }
        }
    }
}
