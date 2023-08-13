using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XSharpCreateInstanceChecker;

public class Config
{
    public string? SolutionPath { get; set; }
    public string? OutputPath { get; set; }

    public static Config FromYaml(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<Config>(File.ReadAllText(filePath));
    }

}


