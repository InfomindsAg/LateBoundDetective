﻿using LateBoundDetective.Analyzers;
using LateBoundDetective.CacheObjects;
using Serilog;
using Serilog.Events;

namespace LateBoundDetective.Helpers;

public static class LogHelper
{
    public static void Config(Config? config)
    {

        var loggerconfig = new LoggerConfiguration();

        loggerconfig = loggerconfig.MinimumLevel.Verbose();

        loggerconfig = loggerconfig.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);

        if (!string.IsNullOrEmpty(config?.OutputPath))
        {
            var infoLogFile = Path.Combine(config.OutputPath, "info.txt");
            if (File.Exists(infoLogFile))
                File.Delete(infoLogFile);

            const string outputTemplate = "[{Level:u3}] {Message:lj}{NewLine}{Exception}";
            loggerconfig = loggerconfig
                                .WriteTo.File(infoLogFile, outputTemplate: outputTemplate);
        }

        Log.Logger = loggerconfig.CreateLogger();
    }

    public static void Error(string filePath, int line, string shortCode, string message) => Log.Error("{filename}({line},1): {shortcode}: {message}", filePath, line, shortCode, message);

    public static void Error(AnalyzerFileResult analyzerFileResult)
    {
        foreach (var item in analyzerFileResult.Items)
            Error(analyzerFileResult.FilePath, item.Line, item.ShortCode, item.Message);
    }
}
