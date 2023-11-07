using LateBoundDetective.Analyzers;
using LateBoundDetective.helpers;
using XSharp.VsParser.Helpers.Cache;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;

namespace LateBoundDetective;

public class SolutionAnalyzer
{
    private readonly ClassHierarchy _classHierarchy;

    private readonly string _solutionPath;

    private readonly CacheHelper _cacheHelper;

    public SolutionAnalyzer(string solutionPath, ClassHierarchy classHierarchy, string outputPath)
    {
        _solutionPath = solutionPath;
        _classHierarchy = classHierarchy;
        _cacheHelper = new CacheHelper(Path.Combine(outputPath, "SolutionAnalyzer.cache"), "1");
    }


    public void Analyze()
    {
        var projectFiles = new SolutionProjectHelper().GetProjectFiles(_solutionPath);
        foreach (var projectFile in projectFiles)
            AnalyzeProject(projectFile);
    }

    bool IsDesignerGenerated(string filePath)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var designerFilePath =
            Path.Combine(Path.GetDirectoryName(filePath)!, $"{nameWithoutExt}.{nameWithoutExt}.xsfrm");
        return File.Exists(designerFilePath);
    }


    void AnalyzeProject(string projectPath)
    {
        Console.WriteLine($"Analyzing Project {projectPath}");
        var projectHelper = new ProjectHelper(projectPath);
        var parser = ParserHelper.BuildWithOptionsList(projectHelper.GetOptions());

        var safeCreateInstanceAnalyzer = new SafeCreateInstanceAnalyzer(_classHierarchy, projectPath);
        var regServerAnalyzer = new RegServerOpenAnalyzer(_classHierarchy, projectPath);

        foreach (var sourceFile in projectHelper.GetSourceFiles(true).Where(q => !IsDesignerGenerated(q)))
        {
            var sourceCode = File.ReadAllText(sourceFile);
            if (!_cacheHelper.TryGetValue(sourceFile, sourceCode, out AnalyzerFileResult analyzerFileResult))
            {
                var result = parser.ParseText(sourceCode, sourceFile);
                if (!result.OK)
                {
                    Console.WriteLine($"Parse Error: {sourceFile}");
                    continue;
                }

                analyzerFileResult = new() { FilePath = sourceFile };
                safeCreateInstanceAnalyzer.Execute(sourceFile, parser.Tree, analyzerFileResult);
                regServerAnalyzer.Execute(sourceFile, parser.Tree, analyzerFileResult);
                
                _cacheHelper.Add(sourceFile, sourceCode, analyzerFileResult);
            }

            LogHelper.Error(analyzerFileResult);
        }
    }
}