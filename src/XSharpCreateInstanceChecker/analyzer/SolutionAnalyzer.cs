﻿using System.Xml.Linq;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;
using XSharpSafeCreateInstanceAnalzyer.analysis;

namespace XSharpSafeCreateInstanceAnalzyer.analyzer;

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
        var projectFiles = new SolutionLoader().GetProjectFiles(SolutionPath);
        foreach (var projectFile in projectFiles)
            AnalyzeProject(projectFile);
    }

    void AnalyzeProject(string projectPath)
    {
        Console.WriteLine($"Analyzing Project {projectPath}");
        var projectHelper = new ProjectHelper(projectPath);
        var parser = ParserHelper.BuildWithOptionsList(projectHelper.GetOptions());

        var safeCreateInstanceAnalyzer = new SafeCreateInstanceAnalyzer(ClassHierarchy, projectPath);

        foreach (var sourceFile in projectHelper.GetSourceFiles(true))
        {
            var sourceCode = File.ReadAllText(sourceFile);
            var result = parser.ParseText(sourceCode, sourceFile);
            if (!result.OK)
            {
                Console.WriteLine($"Parse Error: {sourceFile}");
                continue;
            }

            safeCreateInstanceAnalyzer.Execute(sourceFile, parser.Tree);
        }
    }
}