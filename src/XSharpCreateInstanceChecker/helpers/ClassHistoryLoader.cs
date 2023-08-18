using Microsoft.Build.Construction;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharpSafeCreateInstanceAnalzyer.models;

namespace XSharpSafeCreateInstanceAnalzyer.analyzer
{
    public class ClassHistoryLoader
    {
        public ClassHierarchy Load(string?[] solutionPaths, string cachePath)
        {
            var result = new ClassHierarchy(cachePath);
            var solutionLoader = new SolutionLoader();

            foreach (var solutionPath in solutionPaths.Where(q => !string.IsNullOrWhiteSpace(q)))
            {
                foreach (var projectPath in solutionLoader.GetProjectFiles(solutionPath))
                {
                    Console.WriteLine($"Extracting class hierachy for project {projectPath}");
                    result.AnalyzeProject(projectPath);
                }
            }

            return result;
        }
    }
}
