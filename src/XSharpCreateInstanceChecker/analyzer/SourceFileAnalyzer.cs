using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.VsParser.Helpers.Cache;
using XSharp.VsParser.Helpers.Parser;
using XSharpSafeCreateInstanceAnalzyer.analysis;
using XSharpSafeCreateInstanceAnalzyer.models;

namespace XSharpSafeCreateInstanceAnalzyer.analyzer
{
    public class SourceFileAnalyzer
    {
        private readonly SafeCreateInstanceAnalysis _SafeCreateInstanceAnalysis;
        private readonly GetRegServerRealAnalysis _GetRegServerRealAnalysis;
        private readonly ParserHelper _Parser;
        public static CacheHelper Cache { get; set; } = null;


        public SourceFileAnalyzer(ParserHelper parser)
        {
            _Parser = parser;
            _SafeCreateInstanceAnalysis = new SafeCreateInstanceAnalysis(parser);
            _GetRegServerRealAnalysis = new GetRegServerRealAnalysis(parser);
        }

        public (Analysis, bool fromCache) Anaylze(string sourceFile)
        {
            if (!sourceFile.EndsWith(".prg"))
            {
                return (null, false);
            }

            string sourceCode = File.ReadAllText(sourceFile);
            var result = _Parser.ParseFile(sourceFile, false);

            if (!result.OK)
            {
                Debug.Print(result.Errors.ToString());
                return (null, false);
            }

            return (GetAnalysis(), false);
        }

        private Analysis GetAnalysis()
        {
            return new()
            {
                SafeCreateInstanceInfos = _SafeCreateInstanceAnalysis.GetAnalyses(),
                GetRegServerRealInfos = _GetRegServerRealAnalysis.GetAnalyses(),
            };
        }

    }
}
