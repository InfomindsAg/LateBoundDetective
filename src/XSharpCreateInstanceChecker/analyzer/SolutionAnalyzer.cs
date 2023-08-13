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
using XSharpSafeCreateInstanceAnalzyer.models;

namespace XSharpSafeCreateInstanceAnalzyer.analyzer
{
    public class SolutionAnalyzer
    {
        private const string ProjectExtension = ".xsproj";

        public string SolutionPath { get; }

        public SolutionAnalyzer(string solutionPath)
        {
            SolutionPath = solutionPath;
        }

        private string[] GetProjectFiles(string solutionPath)
        {
            var solutionFile = SolutionFile.Parse(solutionPath);

            return solutionFile.ProjectsInOrder
                .Select(x => x.AbsolutePath)
                .Where(x => x.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        public Analysis Analyze()
        {
            var projectFiles = GetProjectFiles(SolutionPath);

            var analysis = new Analysis()
            {
                SafeCreateInstanceInfos = new List<SafeCreateInstanceInfo>(),
                GetRegServerRealInfos = new List<GetRegServerRealInfo>(),
            };

            var sCIList = new List<SafeCreateInstanceInfo>();
            var gRSRList = new List<GetRegServerRealInfo>();

            foreach (var projectFile in projectFiles)
            {
                //Analyze Projects siehe SonarAnalyzer Solution
                var projectAnalyzer = new ProjectAnalyzer(projectFile);
                Console.WriteLine("Analyzing " + projectFile);
                var result = projectAnalyzer.Anaylze();

                sCIList.AddRange(result.SafeCreateInstanceInfos);
                gRSRList.AddRange(result.GetRegServerRealInfos);
            }

            return new()
            {
                SafeCreateInstanceInfos = sCIList,
                GetRegServerRealInfos = gRSRList,
            };
        }



    }
}
