using Serilog;
using XSharp.VsParser.Helpers.Extensions;
using XSharpCreateInstanceChecker.Helpers;

namespace XSharpCreateInstanceChecker
{
    internal class Program
    {
        private static readonly string configName = "config.yaml";

        static void Main(string[] args)
        {
            LogHelper.Config(null);

            var configPath = args.ElementAtOrDefault(0) ?? configName;
            if (!string.IsNullOrEmpty(configPath) && !configPath.EndsWithIgnoreCase(".yaml"))
                configPath += ".yaml";

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

            var classHierachy = ClassHistoryLoader.Load(new[] { config.SolutionPath, config.LibrarySolutionPath }, Path.Combine(config.OutputPath, "ClassHierarchy.cache"));

            var solutionAnalyzer = new SolutionAnalyzer(config.SolutionPath, classHierachy);
            solutionAnalyzer.Analyze();
        }

    }
}