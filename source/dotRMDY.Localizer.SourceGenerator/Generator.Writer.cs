using System.Collections.Generic;
using System.Text;
using dotRMDY.Localizer.SourceGenerator.Helpers;
using dotRMDY.Localizer.SourceGenerator.Models;

namespace dotRMDY.Localizer.SourceGenerator;

partial class Generator
{
	private static SourceWriter WriteTranslations(TranslationSourceContext translationSourceContext)
	{
		var sourceWriter = new SourceWriter();
		sourceWriter.WriteLine("// <auto-generated/>");
		sourceWriter.WriteLine("using System.Collections.Generic;");
		sourceWriter.WriteLine();
		sourceWriter.WriteLine($"namespace {translationSourceContext.Namespace};");
		sourceWriter.WriteLine();
		sourceWriter.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
		sourceWriter.WriteLine("public static partial class Translations");
		sourceWriter.WriteLine("{");
		sourceWriter.Indentation++;

		sourceWriter.WriteLine("public static string[] Languages =");
		sourceWriter.WriteLine("{");
		sourceWriter.Indentation++;

		foreach (var languageKey in translationSourceContext.LanguageCodes)
		{
			sourceWriter.WriteLine($"\"{languageKey}\",");
		}

		sourceWriter.Indentation--;
		sourceWriter.WriteLine("};");

		sourceWriter.WriteLine();

		foreach (var translationEntry in translationSourceContext.Translations)
		{
			sourceWriter.WriteLine($"public static string {translationEntry.TranslationKey} => GetString(TranslationKey.{translationEntry.TranslationKey});");
		}

		sourceWriter.WriteLine();

		sourceWriter.WriteLine("static void Load()");
		sourceWriter.WriteLine("{");
		sourceWriter.Indentation++;

		foreach (var translationEntry in translationSourceContext.Translations)
		{
			sourceWriter.WriteLine(FormatTranslationLoadStatement(translationEntry, translationSourceContext.LanguageCodes));
		}

		sourceWriter.Indentation--;
		sourceWriter.WriteLine("}");

		sourceWriter.WriteLine();

		sourceWriter.WriteLine("public enum TranslationKey");
		sourceWriter.WriteLine("{");
		sourceWriter.Indentation++;

		foreach (var translationEntry in translationSourceContext.Translations)
		{
			sourceWriter.WriteLine($"{translationEntry.TranslationKey},");
		}

		sourceWriter.Indentation--;
		sourceWriter.WriteLine("}");


		sourceWriter.Indentation--;
		sourceWriter.WriteLine("}");

		return sourceWriter;

		string FormatTranslationLoadStatement(TranslationEntry translationEntry, List<string> languageCodes)
		{
			var statementBuilder = new StringBuilder($"TranslationItems.Add(TranslationKey.{translationEntry.TranslationKey}, new Dictionary<string, string> {{ ");

			for (var i = 0; i < languageCodes.Count; i++)
			{
				statementBuilder
					.Append("{ \"")
					.Append(languageCodes[i])
					.Append("\", \"")
					.Append(translationEntry.Translations[i])
					.Append("\" }");

				if (i < languageCodes.Count - 1)
				{
					statementBuilder.Append(", ");
				}
			}

			statementBuilder.Append(" });");

			return statementBuilder.ToString();
		}
	}

	private static SourceWriter WriteITranslationProvider(TranslationSourceContext translationSourceContext)
	{
		var sourceWriter = new SourceWriter();
		sourceWriter.WriteLine("// <auto-generated/>");
		sourceWriter.WriteLine($"namespace {translationSourceContext.Namespace};");
		sourceWriter.WriteLine();
		sourceWriter.WriteLine("public partial interface ITranslationProvider");
		sourceWriter.WriteLine("{");
		sourceWriter.Indentation++;

		foreach (var translationEntry in translationSourceContext.Translations)
		{
			sourceWriter.WriteLine($"string {translationEntry.TranslationKey} {{ get; }}");
		}

		sourceWriter.Indentation--;
		sourceWriter.WriteLine("}");

		return sourceWriter;
	}

	private static SourceWriter WriteTranslationProviderImplementation(TranslationSourceContext translationSourceContext)
	{
		var sourceWriter = new SourceWriter();
		sourceWriter.WriteLine("// <auto-generated/>");
		sourceWriter.WriteLine($"namespace {translationSourceContext.Namespace};");
		sourceWriter.WriteLine();
		sourceWriter.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
		sourceWriter.WriteLine("public partial class TranslationProvider : ITranslationProvider");
		sourceWriter.WriteLine("{");

		sourceWriter.Indentation++;
		foreach (var translationEntry in translationSourceContext.Translations)
		{
			sourceWriter.WriteLine($"public string {translationEntry.TranslationKey} => Translations.{translationEntry.TranslationKey};");
		}
		sourceWriter.Indentation--;

		sourceWriter.WriteLine("}");

		return sourceWriter;
	}
}