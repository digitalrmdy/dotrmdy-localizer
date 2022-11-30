using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Localizer.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Localizer.SourceGenerator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            // get the additional text provider
            var additionalTexts = initContext.AdditionalTextsProvider
                .Where(a => a.Path.EndsWith(".csv"));

            // get the analyser config options provider
            var analyzerConfigOptionsProvider = initContext.AnalyzerConfigOptionsProvider;

            var combinePipeline = additionalTexts.Combine(analyzerConfigOptionsProvider);

            var transformPipeline = combinePipeline
                .Select((ctx, _) =>
                {
                    var options = ctx.Right.GetOptions(ctx.Left);

                    if (!options.TryGetValue("build_metadata.additionalfiles.Namespace", out var @namespace))
                    {
                        throw new InvalidOperationException("Could not find namespace");
                    }

                    return (@namespace, additionalFile: ctx.Left);
                })
                .Where(t => t.@namespace != null);

            var transformPipeline2 = transformPipeline
                .Select(static (ctx, ct) => (@namespace: ctx.@namespace!, sourceText: ctx.additionalFile.GetText(ct)))
                .Where(t => t.sourceText != null);

            var transformPipeline3 = transformPipeline2
                .Select(static (ctx, ct) => Process(ctx.@namespace, ctx.sourceText!, ct));

            initContext.RegisterSourceOutput(transformPipeline3, (productionContext, sourceContext) =>
            {
                productionContext.AddSource("Translations.g.cs", 
                    $@"using System.Collections.Generic;

namespace {sourceContext.Namespace}
{{
    public static partial class Translations
	{{
        public static string[] Languages =
        {{
{string.Join(",\n", sourceContext.Headers.Skip(1).Select(languageKey => $"{Indent(3)}\"{languageKey}\""))}
        }};

{string.Join("\n", sourceContext.Translations.Keys.Select(translationKey => $"{Indent(2)}public static string {translationKey} => GetString(TranslationKey.{translationKey});"))}

        static void Load() {{
{string.Join("\n", sourceContext.Translations.Keys.Select(translationKey => $"{Indent(3)}TranslationItems.Add(TranslationKey.{translationKey}, new Dictionary<string, string> {{ {sourceContext.GetTranslation(translationKey)} }});"))}
        }}
    }}

	public enum TranslationKey
	{{
{string.Join(",\n", sourceContext.Translations.Keys.Select(translationKey => $"{Indent(2)}{translationKey}"))}
    }}
}}");

                productionContext.AddSource("ITranslationProvider.g.cs", 
                    $@"namespace {sourceContext.Namespace}
{{
    public partial interface ITranslationProvider
    {{
{string.Join("\n", sourceContext.Translations.Keys.Select(translationKey => $"{Indent(2)}string {translationKey} {{ get; }}"))}
    }}
}}");

                productionContext.AddSource("TranslationProvider.g.cs", 
                    $@"namespace {sourceContext.Namespace}
{{
    public partial class TranslationProvider : ITranslationProvider
    {{
{string.Join("\n", sourceContext.Translations.Keys.Select(translationKey => $"{Indent(2)}public string {translationKey} => Translations.{translationKey};"))}
    }}
}}");
            });
        }

        private static TransformationContextWrapper Process(string @namespace, SourceText sourceText, CancellationToken ct)
        {
            var transformationContext = new TransformationContextWrapper(@namespace);
            var lines = sourceText.Lines.Select(x => x.ToString()).Where(s => !s.StartsWith("\"'")).ToArray();

            // first line contains key followed by translations
            transformationContext.Headers = ReadLine(lines[0], true);
            for (var i = 1; i < lines.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var data = ReadLine(lines[i]);
                    var key = data[0];
                    var translation = new Dictionary<string, string>();
                    for (var y = 1; y < data.Count; y++)
                    {
                        translation.Add(transformationContext.Headers[y], data[y]);
                    }

                    transformationContext.Translations.Add(key, translation);
                }
                catch (Exception exc)
                {
                    throw new Exception($"Exception while parsing line {i}", exc);
                }
            }

            return transformationContext;
        }

        private static List<string> ReadLine(string line, bool isHeader = false)
        {
            var splits = line.Split(new[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
            var resultList = new List<string>();
            foreach (var split in splits)
            {
                var toAdd = split;
                if (toAdd[0] == '"')
                {
                    if (toAdd.Length == 1)
                    {
                        continue;
                    }

                    toAdd = toAdd.Substring(1, toAdd.Length - 1);
                }

                if (toAdd[toAdd.Length - 1] == '"')
                {
                    if (toAdd.Length == 1)
                    {
                        continue;
                    }

                    toAdd = toAdd.Substring(0, toAdd.Length - 1);
                }

                resultList.Add(EscapeString(toAdd));
            }

            if (isHeader)
            {
                resultList = resultList.Select(r => r.Substring(r.Length - 5)).ToList();
            }

            return resultList;
        }

        private static string EscapeString(string input)
        {
            return input.Replace(@"""""", @"\""");
        }

        private static string Indent(uint depth = 0) => string.Empty.PadLeft((int)depth * 4).Replace("    ", "	");
    }
}