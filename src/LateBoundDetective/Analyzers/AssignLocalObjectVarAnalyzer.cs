using LateBoundDetective.CacheObjects;
using LateBoundDetective.Helpers;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Utilities;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;

namespace LateBoundDetective.Analyzers;

public class AssignLocalObjectVarAnalyzer : ClassReferencedAnalyzer
{
    public AssignLocalObjectVarAnalyzer(ClassHierarchy classHierarchy, string projectPath) : base(classHierarchy,
        projectPath)
    {
    }

    private static NameHashset _NullValues = new NameHashset() { "nil", "null", "null_object" };

    static bool IsNullValue(string value) => string.IsNullOrEmpty(value) || _NullValues.Contains(value);

    static (string? localVar, string? methodName, string? type) GetAssignmentLocalVarName(
        CtorCallContext ctorCallContext)
    {
        if (ctorCallContext.Parent is PrimaryExpressionContext primary &&
            primary.parent is AssignmentExpressionContext assignment)
            return GetAssignmentLocalVarName(assignment);
        return (null, null, null);
    }

    class LocalAssignments
    {
        public NameHashset Constructors { get; set; } = new();
        public NameHashset OtherAssignments { get; set; } = new();
    }

    Dictionary<string, LocalAssignments> GetLocalAssignments(ClsmethodContext clsMethodContext)
    {
        var localVarsObjUsual = new Dictionary<string, LocalAssignments>();
        // get all local variables
        foreach (var localVarValues in clsMethodContext.AsEnumerable()
                     .WhereType<CommonLocalDeclContext>()
                     .Select(q => q.ToValues())
                     .SelectMany(q => q.Variables)
                     .Where(q => IsObjectOrUsual(q.Type)))
        {
            if (!localVarsObjUsual.TryGetValue(localVarValues.Name, out var localAssignments))
            {
                localAssignments = new LocalAssignments();
                localVarsObjUsual.Add(localVarValues.Name, localAssignments);
            }

            if (!IsNullValue(localVarValues.InitExpression))
                localAssignments.OtherAssignments.Add(localVarValues.InitExpression);
        }

        // Get all assignments for the stored local vars
        foreach (var assignmentExpressionContext in clsMethodContext.AsEnumerable()
                     .WhereType<AssignmentExpressionContext>())
        {
            var (localVar, _, localVarType) = GetAssignmentLocalVarName(assignmentExpressionContext);
            if (string.IsNullOrEmpty(localVar) || !localVarsObjUsual.TryGetValue(localVar, out var localAssignments))
                continue;

            var ctorCallContext = assignmentExpressionContext.Right.AsEnumerable()
                .FirstOrDefaultType<CtorCallContext>();
            if (ctorCallContext != null)
            {
                var className = ctorCallContext.Type?.GetText();
                if (string.IsNullOrEmpty(className))
                    continue;

                localAssignments.Constructors.Add(className);
            }
            else
            {
                var assignmentValues = assignmentExpressionContext.ToValues();
                if (!string.IsNullOrEmpty(assignmentValues.ValueExpression))
                {
                    if (!IsNullValue(assignmentValues.ValueExpression))
                        localAssignments.OtherAssignments.Add(assignmentValues.ValueExpression);
                }
            }
        }

        return localVarsObjUsual;
    }

    public override void Execute(string filePath, AbstractSyntaxTree tree, AnalyzerFileResult result)
    {
        foreach (var clsMethodContext in tree.WhereType<ClsmethodContext>())
        {
            var localAssignmentsDict = GetLocalAssignments(clsMethodContext);

            foreach (var ctorCallContext in clsMethodContext.AsEnumerable().WhereType<CtorCallContext>())
            {
                var (localVar, _, localVarType) = GetAssignmentLocalVarName(ctorCallContext);
                if (string.IsNullOrEmpty(localVar) ||
                    !localAssignmentsDict.TryGetValue(localVar, out var localAssignments))
                    continue;

                var className = ctorCallContext.Type?.GetText();
                if (string.IsNullOrEmpty(className))
                    continue;

                var shortCode = "LocAs";
                var msg =
                    $"Instance of class {className} assigned to local variable {localVar} of type {localVarType}";

                if (localAssignments.Constructors.Count == 1 && localAssignments.OtherAssignments.Count == 0)
                {
                    shortCode += "1";
                    msg += " (single assignment)";
                }
                else
                {
                    shortCode += "Mix";
                    var mixedAssignments = new List<string>();
                    if (localAssignments.Constructors.Count > 0)
                        mixedAssignments.Add($"{localAssignments.Constructors.Count} ctors");
                    if (localAssignments.OtherAssignments.Count > 0)
                        mixedAssignments.Add($"{localAssignments.OtherAssignments.Count} other");

                    msg += $" (mixed, {string.Join(" and ", mixedAssignments)})";
                }

                result.Items.Add(new AnalyzerFileResultItem
                    { Line = ctorCallContext.Start.Line, ShortCode = shortCode, Message = msg });
            }
        }
    }
}