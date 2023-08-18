using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using XSharpSafeCreateInstanceAnalzyer.models;
using static LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser;

namespace XSharpSafeCreateInstanceAnalzyer.analysis
{

    public class SafeCreateInstanceAnalysis : IAnalysis<SafeCreateInstanceInfo>
    {
        private readonly ParserHelper _parser;

        public SafeCreateInstanceAnalysis(ParserHelper parser)
        {
            _parser = parser;
        }


        public IEnumerable<SafeCreateInstanceInfo> GetAnalyses()
        {
            var tree = _parser.Tree;

            foreach (var methodCallMember in tree.WhereType<MethodCallContext>())
            {
                var values = methodCallMember.ToValues();
                if ("SafeCreateInstance".EqualsIgnoreCase(values.NameExpression?.Name))
                {
                    yield return new SafeCreateInstanceInfo()
                    {
                        ClassName = values.Arguments.FirstOrDefault()?.Value.Substring(1),
                        StartLine = methodCallMember.Start.Line,
                        StartLineOffset = methodCallMember.Start.Column,
                        EndLine = methodCallMember.Stop.Line,
                        EndLineOffset = methodCallMember.Stop.Column
                    };
                }
            }            
        }
    }
}
