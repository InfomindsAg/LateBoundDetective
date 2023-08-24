using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;
using XSharpSafeCreateInstanceAnalzyer.models;
using XSharpCreateInstanceChecker.helpers;
using XSharp.VsParser.Helpers.Utilities;
using System.Xml.Linq;

namespace XSharpSafeCreateInstanceAnalzyer.analysis
{
    public class SafeCreateInstanceAnalyzer
    {
        private readonly ClassHierarchy ClassHierarchy;
        private readonly string ProjectName;
        private readonly NameHashset AvailableReferences;

        public SafeCreateInstanceAnalyzer(ClassHierarchy classHierarchy, string projectPath)
        {
            ClassHierarchy = classHierarchy;
            ProjectName = ReferenceHelper.ExtractProjectName(projectPath);

            AvailableReferences = new();

            var root = XDocument.Load(projectPath).Root!;
            var tagNames = new string[] { "Reference", "ProjectReference" };

            AvailableReferences = new NameHashset(root.Descendants()
                .Where(p => tagNames.Contains(p.Name.LocalName))
                .Select(q => q.Attribute("Include")?.Value)
                .Where(q => !string.IsNullOrEmpty(q))
                .Select(ReferenceHelper.ExtractProjectName!));
        }

        (string? localVar, string? methodName) GetAssignmentLocalVarName(MethodCallContext methodCallContext)
        {
            if (methodCallContext.parent is AssignmentExpressionContext assignment)
            {
                if (assignment.Left is not PrimaryExpressionContext primaryContext)
                    return (null, null);

                var primary = primaryContext.GetText();

                if (!string.IsNullOrEmpty(primary))
                {
                    var clsMethodContext = methodCallContext.FirstParentOrDefault<ClsmethodContext>();
                    if (clsMethodContext != null)
                    {
                        var methodName = clsMethodContext.AsEnumerable().FirstOrDefaultType<SignatureContext>()?.ToValues().Name;

                        foreach (var localDecl in clsMethodContext.AsEnumerable().WhereType<CommonLocalDeclContext>())
                            foreach (var variable in localDecl.ToValues().Variables)
                                if (primary.EqualsIgnoreCase(variable.Name))
                                    return (primary, methodName);

                    }
                }
            }
            return (null, null);
        }

        public void Execute(string filePath, AbstractSyntaxTree tree)
        {
            foreach (var methodCallContext in tree.WhereType<MethodCallContext>())
            {
                var values = methodCallContext.ToValues();
                var name = values.NameExpression?.Name;
                if ("SafeCreateInstance".EqualsIgnoreCase(name) || "CreateInstance".EqualsIgnoreCase(name))
                {
                    var ClassName = values.Arguments.FirstOrDefault()?.Value.Substring(1);
                    var StartLine = methodCallContext.Start.Line;

                    var classProjectFileName = ReferenceHelper.ExtractProjectName(ClassHierarchy.GetProjectFileName(ClassName));
                    var prj = ProjectName.EqualsIgnoreCase(classProjectFileName);
                    var refs = AvailableReferences.Contains(classProjectFileName);
                    if (!(prj || refs))
                        continue;

                    var shortCode = prj ? "CrPj" : "CrRef";
                    var refTypeMsg = prj ? "project" : "referenced";
                    var msg = $"(Safe)CreateInstance of {refTypeMsg} type \"{ClassName}\" used";
                    var (localVar, methodName) = GetAssignmentLocalVarName(methodCallContext);
                    if (!string.IsNullOrEmpty(localVar))
                    {
                        msg += $", assigned to local variable \"{localVar}\" in method \"{methodName}\"";
                        shortCode += "Loc";
                    }

                    if (values.Arguments.Length == 1)
                    {
                        msg += " with no additional arguments";
                        shortCode += "0P";
                    }

                    LogHelper.Error(filePath, StartLine, shortCode, msg);
                }
            }
        }
    }
}
