using LateBoundDetective.Analyzers;
using LateBoundDetective.CacheObjects;
using LateBoundDetective.Helpers;
using Serilog;
using XSharp.VsParser.Helpers.Cache;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;

namespace LateBoundDetective;

public class SolutionAnalyzer : IDisposable
{
    private readonly ClassHierarchy _classHierarchy;

    private readonly string _solutionPath;

    private readonly CacheHelper _cacheHelper;

    public SolutionAnalyzer(string solutionPath, ClassHierarchy classHierarchy, string outputPath)
    {
        _solutionPath = solutionPath;
        _classHierarchy = classHierarchy;
        _cacheHelper = new CacheHelper(Path.Combine(outputPath, "SolutionAnalyzer.cache"), "3");
    }


    public void Analyze()
    {
        var projectFiles = SolutionProjectHelper.GetProjectFiles(_solutionPath);
        foreach (var projectFile in projectFiles)
            AnalyzeProject(projectFile);
    }

    private static bool IsDesignerGenerated(string filePath)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var designerFilePath =
            Path.Combine(Path.GetDirectoryName(filePath)!, $"{nameWithoutExt}.{nameWithoutExt}.xsfrm");
        return File.Exists(designerFilePath);
    }

    private bool ProjectReferencesChanged(string projectPath)
    {
        var projectSourceCode = File.ReadAllText(projectPath);
        var references = ReferenceHelper.GetReferences(projectPath).OrderBy(q => q).ToList();

        if (!_cacheHelper.TryGetValue(projectPath, projectSourceCode,
                out AnalyzerProjectResult analyzerProjectResult))
        {
            _cacheHelper.Add(projectPath, projectSourceCode, new AnalyzerProjectResult { References = references });
            return true;
        }

        return !analyzerProjectResult.References.SequenceEqual(references);
    }

    private void AnalyzeProject(string projectPath)
    {
        Console.WriteLine($"Analyzing Project {projectPath}");
        var projectHelper = new ProjectHelper(projectPath);
        var parser = ParserHelper.BuildWithOptionsList(projectHelper.GetOptions());

        var analyzers = new IAnalyzer[]
        {
            new SafeCreateInstanceAnalyzer(_classHierarchy, projectPath),
            new RegServerOpenAnalyzer(_classHierarchy, projectPath),
            new AssignLocalObjectVarAnalyzer(_classHierarchy, projectPath),
        };
        // Check in the cache, if the source
        var isProjectReferencesChanged = ProjectReferencesChanged(projectPath);
        if (isProjectReferencesChanged)
            Log.Information("References in project-file {project} changed. Ignoring cached results for this project.",
                Path.GetFileName(projectPath));
        foreach (var sourceFile in projectHelper.GetSourceFiles(true).Where(q => !IsDesignerGenerated(q)))
        {
            var sourceCode = File.ReadAllText(sourceFile);
            if (isProjectReferencesChanged ||
                !_cacheHelper.TryGetValue(sourceFile, sourceCode, out AnalyzerFileResult analyzerFileResult))
            {
                var result = parser.ParseText(sourceCode, sourceFile);
                if (!result.OK)
                {
                    Console.WriteLine($"Parse Error: {sourceFile}");
                    continue;
                }

                analyzerFileResult = new() { FilePath = sourceFile };
                foreach (var analyzer in analyzers)
                    analyzer.Execute(sourceFile, parser.Tree, analyzerFileResult);

                _cacheHelper.Add(sourceFile, sourceCode, analyzerFileResult);
            }

            LogHelper.Error(analyzerFileResult);
        }
    }

    public void Dispose()
    {
        _cacheHelper.Dispose();
        GC.SuppressFinalize(this);
    }
}