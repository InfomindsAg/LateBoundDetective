using XSharp.VsParser.Helpers.Extensions;

namespace XSharpCreateInstanceChecker.Helpers;

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
