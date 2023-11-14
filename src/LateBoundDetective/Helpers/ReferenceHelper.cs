using System.Xml.Linq;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Utilities;

namespace LateBoundDetective.Helpers;

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

    public static NameHashset GetReferences(string projectPath)
    {
        var root = XDocument.Load(projectPath).Root!;
        var tagNames = new string[] { "Reference", "ProjectReference" };

        return new NameHashset(root.Descendants()
            .Where(p => tagNames.Contains(p.Name.LocalName))
            .Select(q => q.Attribute("Include")?.Value)
            .Where(q => !string.IsNullOrEmpty(q))
            .Select(ReferenceHelper.ExtractProjectName!));
    }
}