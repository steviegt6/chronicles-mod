using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Hjson;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace DarknessUnbound.CodeAssist.SourceGenerators;

[Generator]
public sealed class LocalizationGenerator : ISourceGenerator {
    private sealed class LocalizationNode {
        public string Name { get; }

        public Dictionary<string, LocalizationNode> Nodes { get; }

        public List<string> Keys { get; set; }

        public LocalizationNode(string name, Dictionary<string, LocalizationNode> nodes, List<string> keys) {
            Name = name;
            Nodes = nodes;
            Keys = keys;
        }
    }

    void ISourceGenerator.Initialize(GeneratorInitializationContext context) { }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        var files = context.AdditionalFiles.Where(x => Path.GetExtension(x.Path) == ".hjson");

        context.AddSource("localization.g.cs", GenerateLocalization(files.ToList(), context.Compilation.AssemblyName!));
    }

    private static string GenerateLocalization(List<AdditionalText> hjsonFiles, string assemblyName) {
        var sb = new StringBuilder();

        var keys = new HashSet<string>();

        foreach (var file in hjsonFiles)
        foreach (var key in GetKeysFromFile(file)) {
            keys.Add(key);
            // if (!keys.Add(key))
            //     Debug.WriteLine($"Duplicate key: {key}");
        }

        var root = new LocalizationNode("", new Dictionary<string, LocalizationNode>(), new List<string>());

        foreach (var key in keys) {
            var parts = key.Split('.');
            var current = root;

            for (var i = 0; i < parts.Length; i++) {
                var part = parts[i];

                if (i == parts.Length - 1) {
                    current.Keys.Add(key);
                }
                else {
                    if (!current.Nodes.TryGetValue(part, out var node)) {
                        node = new LocalizationNode(part, new Dictionary<string, LocalizationNode>(), new List<string>());
                        current.Nodes.Add(part, node);
                    }

                    current = node;
                }
            }
        }

        sb.AppendLine($"using {assemblyName}.Common.Localization;");
        sb.AppendLine();
        sb.AppendLine($"namespace {assemblyName}.Core;");
        sb.AppendLine();
        sb.AppendLine($"internal static class {assemblyName}Localization {{");

        foreach (var child in root.Nodes.Values)
            sb.Append(GenerateTextFromLocalizationNode(child, 1));

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateTextFromLocalizationNode(LocalizationNode node, int depth = 0) {
        var sb = new StringBuilder();
        var indent = new string(' ', depth * 4);

        sb.AppendLine($"{indent}public static class {node.Name} {{");
        foreach (var key in node.Keys)
            sb.AppendLine($"{indent}    public static readonly LocalizableText {key.Split('.').Last()} = LocalizableText.FromKey(\"{key}\");");

        foreach (var child in node.Nodes.Values)
            sb.Append(GenerateTextFromLocalizationNode(child, depth + 1));

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    private static List<string> GetKeysFromFile(AdditionalText file) {
        var keys = new List<string>();
        var prefix = GetPrefixFromPath(file.Path);
        var text = File.ReadAllText(file.Path);
        var json = HjsonValue.Parse(text).ToString();
        var jsonObject = JObject.Parse(json);

        foreach (var t in jsonObject.SelectTokens("$..*")) {
            if (t.HasValues)
                continue;

            if (t is JObject { Count: 0 })
                continue;

            var path = "";
            var current = t;

            for (var parent = t.Parent; parent is not null; parent = parent.Parent) {
                path = parent switch {
                    JProperty property => property.Name + (path == string.Empty ? string.Empty : '.' + path),
                    JArray array => array.IndexOf(current) + (path == string.Empty ? string.Empty : '.' + path),
                    _ => path,
                };
                current = parent;
            }

            path = path.Replace(".$parentVal", "");
            if (!string.IsNullOrWhiteSpace(prefix))
                path = prefix + '.' + path;

            keys.Add(path);
        }

        return keys;
    }

    public static string? GetPrefixFromPath(string path) {
        path = Path.GetFileNameWithoutExtension(path);
        var splitByUnderscore = path.Split('_');

        return splitByUnderscore.Length switch {
            0 => null,
            1 => splitByUnderscore[0],
            2 => splitByUnderscore[1],
            _ => throw new ArgumentException("Invalid path format", nameof(path))
        };
    }
}
