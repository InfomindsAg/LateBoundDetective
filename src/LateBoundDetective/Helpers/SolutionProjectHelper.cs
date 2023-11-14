using Microsoft.Build.Construction;

namespace LateBoundDetective.Helpers;

public class SolutionProjectHelper
{
    private const string ProjectExtension = ".xsproj";

    public string[] GetProjectFiles(string solutionPath)
    {
        var solutionFile = SolutionFile.Parse(solutionPath);

        return solutionFile.ProjectsInOrder
            .Select(x => x.AbsolutePath)
            .Where(x => x.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
