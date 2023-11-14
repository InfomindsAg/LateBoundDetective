using XSharp.VsParser.Helpers.ClassHierarchy;

namespace LateBoundDetective.Helpers;

public class ClassHistoryLoader
{
    public static ClassHierarchy Load(string?[] solutionPaths, string cachePath)
    {
        var result = new ClassHierarchy(cachePath);
        var solutionLoader = new SolutionProjectHelper();

        foreach (var solutionPath in solutionPaths.Where(q => !string.IsNullOrWhiteSpace(q)))
        {
            foreach (var projectPath in solutionLoader.GetProjectFiles(solutionPath!))
            {
                Console.WriteLine($"Extracting class hierachy for project {projectPath}");
                result.AnalyzeProject(projectPath);
            }
        }

        return result;
    }
}
