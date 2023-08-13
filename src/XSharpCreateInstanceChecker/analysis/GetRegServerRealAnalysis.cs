using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;
using XSharpSafeCreateInstanceAnalzyer.models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using LanguageService.SyntaxTree.Tree;
using XSharp.VsParser.Helpers.Parser.Values;

namespace XSharpSafeCreateInstanceAnalzyer.analysis
{
    public class GetRegServerRealAnalysis : IAnalysis<GetRegServerRealInfo>
    {

        private readonly ParserHelper _parser;

        public GetRegServerRealAnalysis(ParserHelper parser)
        {
            _parser = parser;
        }

        

        public IEnumerable<GetRegServerRealInfo> GetAnalyses()
        {
            var tree = _parser.Tree;

            //var methods = GetAllMethods();
            var localVariables = GetAllLocalObjectVariables();
            var classVariables = GetAllClassObjectVariables();
            var (assignments, classNames) = GetAllAssignmentsAndClassNames();

            var infos = new List<GetRegServerRealInfo>();

            for ( var i = 0; i<assignments.Count(); i++ )
            {
                var assignment = assignments.ElementAt(i);
                var className = classNames.ElementAt(i);

                if (localVariables.Any(v => v.EqualsIgnoreCase(assignment)))
                {
                    infos.Add(className);
                }else if(classVariables.Any(v => v.EqualsIgnoreCase(assignment)))
                {
                    infos.Add(className);
                }

            }
            return infos;
        }

        private IEnumerable<(GetRegServerRealInfo info, int startLine, int endLine)> GetAllMethods()
        {
            var tree = _parser.Tree;
            var methodContexts = tree.WhereType<MethodContext>();
            foreach (var methodContext in methodContexts)
            {
                var start = methodContext.Start.Line;
                var end = methodContext.Stop.Line;
                var values = methodContext.ToValues();

                yield return (
                        new GetRegServerRealInfo()
                        {
                            
                        },
                        start,
                        end
                    );
            }
        }

        private IEnumerable<string> GetAllLocalObjectVariables()
        {
            var tree = _parser.Tree;

            var localVarContexts = tree.WhereType<LocalvarContext>();

            foreach (var localvarContext in localVarContexts)
            {
                var values = localvarContext.ToValues();
                if (values.Type.EqualsIgnoreCase("object"))
                {
                    yield return values.Name;
                }
            }

        }

        private IEnumerable<string> GetAllClassObjectVariables()
        {
            var tree = _parser.Tree;

            var classVarMembers = tree.WhereType<ClassvarContext>();
            var objectVariables = new List<string>();
            int variablesInLine = 0;

            foreach (var classVarMember in classVarMembers)
            {
                var dataType = classVarMember.DataType?.GetText();
                if (dataType == null)
                {
                    objectVariables.Add(classVarMember.Start.Text);
                    variablesInLine++;
                }
                else if ("object".EqualsIgnoreCase(dataType))
                {
                    objectVariables.Add(classVarMember.Start.Text);
                    variablesInLine = 0;
                }
                else
                {
                    objectVariables.RemoveRange(objectVariables.Count() - variablesInLine, variablesInLine);
                    variablesInLine = 0;
                }

            }
            return objectVariables;
        }

        private (IEnumerable<string>, IEnumerable<GetRegServerRealInfo>) GetAllAssignmentsAndClassNames()
        {
            var tree = _parser.Tree;

            var classNames = new List<GetRegServerRealInfo>();
            var assignments = new List<string>();

            var methodCallMembers = tree.WhereType<MethodCallContext>();
            foreach (var methodCallMember in methodCallMembers)
            {
                var values = methodCallMember.ToValues();

                if ("Open".EqualsIgnoreCase(values.AccessMember?.MemberName) &&
                    ("GetRegServer()".EqualsIgnoreCase(values.AccessMember?.AccessExpression) ||
                    "GetRegServerReal()".EqualsIgnoreCase(values.AccessMember?.AccessExpression)))
                {
                    classNames.Add(new()
                    {
                        ClassName = values.Arguments.ElementAt(1).Value.Substring(1),
                        StartLine = methodCallMember.Start.Line,
                        EndLine = methodCallMember.Stop.Line,
                        StartLineOffset = methodCallMember.Start.Column,
                        EndLineOffset = methodCallMember.Stop.Column,
                    });
                    
                    var parent = methodCallMember.Parent;
                    var expression = parent.GetChild(0).GetText();
                    assignments.Add(expression);
                }

            }

            return (assignments, classNames);
        }
    }
}
