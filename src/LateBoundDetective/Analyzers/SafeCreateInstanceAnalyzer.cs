using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;
using XSharpCreateInstanceChecker.Helpers;

namespace XSharpCreateInstanceChecker.Analyzers;

public class SafeCreateInstanceAnalyzer : ClassReferencedAnalyzer
{
    public SafeCreateInstanceAnalyzer(ClassHierarchy classHierarchy, string projectPath) : base(classHierarchy, projectPath)
    { }

    public void Execute(string filePath, AbstractSyntaxTree tree)
    {
        foreach (var methodCallContext in tree.WhereType<MethodCallContext>())
        {
            var values = methodCallContext.ToValues();
            var name = values.NameExpression?.Name;
            var firstArgument = values.Arguments.FirstOrDefault()?.Value?.Trim() ?? "";
            if (("SafeCreateInstance".EqualsIgnoreCase(name) || "CreateInstance".EqualsIgnoreCase(name)) && firstArgument.StartsWith('#'))
            {
                var (isRefernced, shortCode, msg) = IsClassNameAvailableAsTyped(methodCallContext, firstArgument);
                if (!isRefernced)
                    continue;

                shortCode = $"Cr{shortCode}";
                msg = $"(Safe)CreateInstance of {msg}";

                if (values.Arguments.Length == 1)
                {
                    msg += " with no additional arguments";
                    shortCode += "0P";
                }

                LogHelper.Error(filePath, methodCallContext.Start.Line, shortCode, msg);
            }
        }
    }
}
