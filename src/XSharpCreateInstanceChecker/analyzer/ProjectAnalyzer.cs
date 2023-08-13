using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Extensions;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;
using XSharpSafeCreateInstanceAnalzyer.models;

namespace XSharpSafeCreateInstanceAnalzyer.analyzer
{
    public class ProjectAnalyzer
    {

        private ProjectHelper _projectHelper;
        private string _projectPath;
        private ParserHelper _parser;
        private string _projectFile;

        public ProjectAnalyzer(string projectFile)
        {
            _projectPath = Path.GetDirectoryName(projectFile);
            _projectHelper = new ProjectHelper(projectFile);
            _parser = ParserHelper.BuildWithOptionsList(_projectHelper.GetOptions());
            _projectFile = projectFile;
        }

        public Analysis Anaylze()
        {
            var sourceFiles = _projectHelper.GetSourceFiles(true);
            
            var analyzer = new SourceFileAnalyzer(_parser);

            var sCIList = new List<SafeCreateInstanceInfo>();
            var gRSRList = new List<GetRegServerRealInfo>();

            foreach (var sourceFile in sourceFiles)
            {

                var (analysis, fromCache) = analyzer.Anaylze(sourceFile);
                var safeCreateInstanceInfos = analysis.SafeCreateInstanceInfos;
                var getRegServerRealInfos = analysis.GetRegServerRealInfos;

                if (safeCreateInstanceInfos?.Count() > 0) 
                {
                    var safeCreateInstClasses = FindClassNames(safeCreateInstanceInfos);
                    sCIList.AddRange(safeCreateInstClasses);
                }

                if (getRegServerRealInfos != null)
                {
                    var getRegServerRealClasses = FindClassNames(getRegServerRealInfos);
                    gRSRList.AddRange(getRegServerRealClasses);
                }

                
                
            }

            return new()
            {
                SafeCreateInstanceInfos = sCIList,
                GetRegServerRealInfos = gRSRList,
            };
        }

        private IEnumerable<T> FindClassNames<T>(IEnumerable<T> infos) where T : IInfo
        {
            var classHierarchy = new ClassHierarchy(_projectFile + ".cached");
            classHierarchy.AnalyzeProject(_projectFile);
            foreach (var info in infos)
            {
                var project = classHierarchy.GetProjectFileName(info.ClassName);

                if (project != null)
                {
                    if (_projectFile.EndsWith(project))
                    {
                        yield return info;
                    }
                }
                
                
            }
        }
    }
}
