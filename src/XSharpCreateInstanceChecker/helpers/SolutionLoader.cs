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
    public class SolutionLoader
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
}
