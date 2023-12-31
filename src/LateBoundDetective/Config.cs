﻿using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LateBoundDetective;

public class Config
{
    public string? SolutionPath { get; set; }
    public string? OutputPath { get; set; }
    public string? LibrarySolutionPath { get; set; }

    

    public static Config FromYaml(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<Config>(File.ReadAllText(filePath));
    }

}


