using LateBoundDetective.Analyzers;
using LateBoundDetective.helpers;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;

namespace LateBoundDetective;

public class SolutionAnalyzer
{
    private readonly ClassHierarchy ClassHierarchy;

    private readonly string SolutionPath;

    public SolutionAnalyzer(string solutionPath, ClassHierarchy classHierarchy)
    {
        SolutionPath = solutionPath;
        ClassHierarchy = classHierarchy;
    }


    public void Analyze()
    {
        var projectFiles = new SolutionProjectHelper().GetProjectFiles(SolutionPath);
        foreach (var projectFile in projectFiles)
            AnalyzeProject(projectFile);
    }

    bool IsDesignerGenerated(string filePath)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var designerFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, $"{nameWithoutExt}.{nameWithoutExt}.xsfrm");
        return File.Exists(designerFilePath);
    }


    void AnalyzeProject(string projectPath)
    {
        Console.WriteLine($"Analyzing Project {projectPath}");
        var projectHelper = new ProjectHelper(projectPath);
        var parser = ParserHelper.BuildWithOptionsList(projectHelper.GetOptions());

        var safeCreateInstanceAnalyzer = new SafeCreateInstanceAnalyzer(ClassHierarchy, projectPath);
        var regServerAnalyzer = new RegServerOpenAnalyzer(ClassHierarchy, projectPath);

        foreach (var sourceFile in projectHelper.GetSourceFiles(true).Where(q => !IsDesignerGenerated(q)))
        {
            var sourceCode = File.ReadAllText(sourceFile);
            var result = parser.ParseText(sourceCode, sourceFile);
            if (!result.OK)
            {
                Console.WriteLine($"Parse Error: {sourceFile}");
                continue;
            }

            safeCreateInstanceAnalyzer.Execute(sourceFile, parser.Tree);
            regServerAnalyzer.Execute(sourceFile, parser.Tree);
        }
    }
}
