using System.Xml.Linq;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Utilities;
using XSharpCreateInstanceChecker.Helpers;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;

namespace XSharpCreateInstanceChecker.Analyzers;

public class ClassReferencedAnalyzer
{
    private readonly ClassHierarchy ClassHierarchy;
    private readonly string ProjectName;
    private readonly NameHashset AvailableReferences;

    static (string? localVar, string? methodName, string? type) GetAssignmentLocalVarName(MethodCallContext methodCallContext)
    {
        if (methodCallContext.parent is AssignmentExpressionContext assignment)
        {
            if (assignment.Left is not PrimaryExpressionContext primaryContext)
                return (null, null, null);

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
                                return (primary, methodName, variable.Type);
                }
            }
        }
        return (null, null, null);
    }

    public ClassReferencedAnalyzer(ClassHierarchy classHierarchy, string projectPath)
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

    protected (bool isReferenced, string shortCode, string msg) IsClassNameAvailableAsTyped(MethodCallContext methodCallContext, string className)
    {
        if (!className.StartsWith("#"))
            return (false, "", "");

        className = className[1..];
        var classProjectFileName = ReferenceHelper.ExtractProjectName(ClassHierarchy.GetProjectFileName(className));
        var prj = ProjectName.EqualsIgnoreCase(classProjectFileName);
        var refs = AvailableReferences.Contains(classProjectFileName);
        if (!(prj || refs))
            return (false, "", "");

        var shortCode = prj ? "Pj" : "Ref";
        var refTypeMsg = prj ? "project" : "referenced";
        var msg = $"{refTypeMsg} type \"{className}\" used";
        var (localVar, methodName, localVarType) = GetAssignmentLocalVarName(methodCallContext);
        if (!string.IsNullOrEmpty(localVar))
        {
            shortCode += "Loc";
            if (className.EqualsIgnoreCase(localVarType))
            {
                shortCode += "Typed";
                msg += $", assigned to local, already typed variable \"{localVar}\" in method \"{methodName}\"";
            }
            else
                msg += $", assigned to local variable \"{localVar}\" in method \"{methodName}\"";
        }

        return (true, shortCode, msg);
    }

}