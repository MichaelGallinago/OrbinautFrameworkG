using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AudioStorageSourceGenerator;

[Generator]
public class AudioStorageGenerator : IIncrementalGenerator
{
    private static readonly Regex SnakeRegex = new Regex("_([a-z])");
    private static readonly Regex AudioRegex = new(@"\.(mp3|ogg|wav)$");
    
    private const string ProjectPathProperty = "build_property.projectdir";
    private const string AttributeName = nameof(AudioStorageAttribute);
    private const string FullAttributeName = $"{nameof(AudioStorageSourceGenerator)}.{AttributeName}";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            FullAttributeName,
            static (_, _) => true,
            static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Collect();
        
        var pathProvider = context.AnalyzerConfigOptionsProvider.Select(
            static (optionsProvider, _) => optionsProvider.GlobalOptions);
        
        var filesProvider = context.AdditionalTextsProvider
            .Where(a => AudioRegex.IsMatch(a.Path))
            .Select((a, _) => (Path.GetFileName(a.Path), a.Path.Replace('\\', '/')))
            .Collect();
        
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider).Combine(filesProvider.Combine(pathProvider)), Build);
    }
            
    private static void Build(
        SourceProductionContext context,
        ((Compilation compilation, ImmutableArray<AudioStorageData?> storages),
        (ImmutableArray<(string, string)> names, AnalyzerConfigOptions config)) source)
    {
        foreach (AudioStorageData? storage in source.Item1.storages)
        {
            if (storage == null) continue;
            var data = (AudioStorageData)storage;
            if (data.Namespace == null) continue;
            if (!source.Item2.config.TryGetValue(ProjectPathProperty, out string? projectPath)) continue;
            
            var sb = new StringBuilder();
            sb.Append("using Godot;\n\n");
            sb.Append($"namespace {data.Namespace}\n");
            sb.Append("{\n");
            sb.Append($"   public static class {data.Name}\n");
            sb.Append("   {\n");
            
            HashSet<string> existingNames = [];
            string folderPath = data.Folder.Remove(0, 6);
            foreach ((string file, string path) in source.Item2.names)
            {
                string localPath = path.Remove(0, projectPath.Length);
                if (localPath.Remove(localPath.Length - file.Length, file.Length) != folderPath) continue;
                string audioStream = GetAudioStreamName(file);
                sb.Append($"      public static readonly {audioStream} {
                    GetNumberedName(file, existingNames)} = GD.Load<{audioStream}>(\"{data.Folder}{file}\");\n");
            }
            
            sb.Append("   }\n");
            sb.Append("}\n");
            context.AddSource($"{data.Name}.g.cs", sb.ToString());
        }
    }

    private static string GetNumberedName(string file, ISet<string> existingNames)
    {
        string name = Path.GetFileNameWithoutExtension(file);
        if (name == string.Empty)
        {
            name = "NoName";
        }

        name = SnakeRegex.Replace(name, m => m.Groups[1].Value.ToUpper());
        name = char.IsLetter(name[0]) ? char.ToUpper(name[0]) + name.Substring(1) : '_' + name;
        
        var counter = 1;
        string numberedName = name;
        while (!existingNames.Add(numberedName))
        {
            numberedName = $"{name}_{++counter}";
        }

        return numberedName;
    }
    
    private static string GetAudioStreamName(string file) => "AudioStream" + Path.GetExtension(file) switch
    {
        ".wav" => "Wav",
        ".ogg" => "OggVorbis",
        ".mp3" => "MP3",
        _ => string.Empty
    };
        
    private static AudioStorageData? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        AttributeData? attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == AttributeName);

        if (attribute == null) return null;
        
        var name = (string)attribute.ConstructorArguments[0].Value!;
        var namespaceName = (string)attribute.ConstructorArguments[1].Value!;
        var folder = (string)attribute.ConstructorArguments[2].Value!;

        return new AudioStorageData(name, namespaceName, folder);
    }
    
    private readonly record struct AudioStorageData(string Name, string? Namespace, string Folder)
    {
        public readonly string Name = Name;
        public readonly string Folder = Folder;
        public readonly string? Namespace = Namespace;
    }
}
