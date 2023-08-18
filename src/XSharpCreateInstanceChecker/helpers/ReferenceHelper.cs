using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XSharp.VsParser.Helpers.Parser.Result;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Extensions;

namespace XSharpCreateInstanceChecker.helpers
{
    internal static class ReferenceHelper
    {

        public static string ExtractProjectName(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                return string.Empty;

            var firstComma = reference.IndexOf(",");
            if (reference.EndsWithIgnoreCase(".xsproj"))
                return Path.GetFileNameWithoutExtension(reference);
            if (firstComma > 0 && reference.IndexOf("=") > 0)
                return reference.Substring(0, firstComma);

            return reference;
        }
    }
}
