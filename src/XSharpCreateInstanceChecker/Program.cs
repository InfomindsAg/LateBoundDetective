using Serilog;
using System.CommandLine;
using System.Diagnostics;
using XSharp.VsParser.Helpers.Parser;
using XSharp.VsParser.Helpers.Project;
using XSharpCreateInstanceChecker;
using XSharpCreateInstanceChecker.helpers;
using XSharpSafeCreateInstanceAnalzyer.analyzer;

namespace XSharpSafeCreateInstanceAnalzyer
{
    internal class Program
    {
        private static string configName = "config.yaml";

        static void Main(string[] args)
        {
            LogHelper.Config(null);


            var configPath = args.ElementAtOrDefault(0) ?? configName;
            if (!File.Exists(configPath))
            {
                Log.Error($"Config file {configPath} does not exist.");
                return;
            }

            var config = Config.FromYaml(configPath);
            if (string.IsNullOrEmpty(config.SolutionPath) || string.IsNullOrEmpty(config.OutputPath))
            {
                Log.Error($"Config file: SolutionPath and OutputPath can not be empty.");
                return;
            }

            LogHelper.Config(config);

            var classHierachy = new ClassHistoryLoader().Load(new[] { config.SolutionPath, config.LibrarySolutionPath }, Path.Combine(config.OutputPath, "ClassHierarchy.cache"));

            var solutionAnalyzer = new SolutionAnalyzer(config.SolutionPath, classHierachy);
            solutionAnalyzer.Analyze();

            //var solutionAnalyzer = new SolutionAnalyzer(config.SolutionPath);
            //var analysis = solutionAnalyzer.Analyze();
            //var safeCreateInstance = string.Join(Environment.NewLine, analysis.SafeCreateInstanceInfos.Where(x => x != null));
            //var getRegServerReal = string.Join(Environment.NewLine, analysis.GetRegServerRealInfos.Where(x => x != null));

            //Directory.CreateDirectory(config.OutputPath);
            //File.WriteAllText(Path.Combine(config.OutputPath, "dataSafeCreateInstance.txt"), safeCreateInstance);
            //File.WriteAllText(Path.Combine(config.OutputPath, "dataGetRegServerReal.txt"), getRegServerReal);
        }





    }
}