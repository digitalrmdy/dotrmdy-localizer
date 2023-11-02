using System;
using System.Collections.Immutable;
using dotRMDY.Localizer.SourceGenerator.Diagnostics;
using dotRMDY.Localizer.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace dotRMDY.Localizer.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class Generator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// get the analyser config options provider
		var analyzerConfigOptionsProvider = context.AnalyzerConfigOptionsProvider;

		var additionalTextsValuesProvider = context.AdditionalTextsProvider
			.Where(static x => x.Path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

		var translationSourceContextValuesProviderWithDiagnostics = additionalTextsValuesProvider.Combine(analyzerConfigOptionsProvider)
			.Select(static (kvp, ct) =>
			{
				var (translationsFile, analyzerConfigOptionsProvider) = kvp;

				var options = analyzerConfigOptionsProvider.GetOptions(translationsFile);
				if (!options.TryGetValue("build_metadata.additionalfiles.Namespace", out var @namespace))
				{
					var noNamespaceDiagnostic =
						Diagnostic.Create(DiagnosticDescriptors.NoNameSpaceDiagnosticDescriptor, Location.None, translationsFile.Path);
					return new Result<TranslationSourceContext?>(null, new ImmutableArray<Diagnostic> { noNamespaceDiagnostic });
				}

				ct.ThrowIfCancellationRequested();

				var parser = new Parser(translationsFile);
				if (!parser.Execute(
					    ct,
					    out var languageCodes,
					    out var translationEntries,
					    out var diagnostics))
				{
					return new Result<TranslationSourceContext?>(null, diagnostics);
				}

				var translationSourceContext = new TranslationSourceContext(
					@namespace,
					languageCodes,
					translationEntries);
				return new Result<TranslationSourceContext?>(translationSourceContext, diagnostics);
			});

		// Output the diagnostics
		context.RegisterSourceOutput(
			translationSourceContextValuesProviderWithDiagnostics.Select((result, _) => result.Errors),
			static (productionContext, diagnostics) =>
			{
				foreach (var diagnostic in diagnostics)
				{
					productionContext.ReportDiagnostic(diagnostic);
				}
			});

		// Enable pipeline caching
		var translationSourceContextValuesProvider = translationSourceContextValuesProviderWithDiagnostics
			.Where(static x => x.Value is not null)
			.Select(static (x, _) => x.Value!);

		context.RegisterSourceOutput(translationSourceContextValuesProvider, (productionContext, sourceContext) =>
		{
			productionContext.AddSource(
				sourceContext.Namespace + ".Translations.g.cs",
				WriteTranslations(sourceContext).ToSourceText());

			productionContext.AddSource(
				sourceContext.Namespace + ".TranslationKey.g.cs",
				WriteTranslationKeyEnum(sourceContext).ToSourceText());

			productionContext.AddSource(
				sourceContext.Namespace + ".ITranslationProvider.g.cs",
				WriteITranslationProvider(sourceContext).ToSourceText());

			productionContext.AddSource(
				sourceContext.Namespace + ".TranslationProvider.g.cs",
				WriteTranslationProviderImplementation(sourceContext).ToSourceText());
		});
	}
}