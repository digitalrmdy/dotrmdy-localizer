using Microsoft.CodeAnalysis;

namespace dotRMDY.Localizer.SourceGenerator.Diagnostics;

internal class DiagnosticDescriptors
{
	internal static readonly DiagnosticDescriptor NoNameSpaceDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0001",
#pragma warning restore RS2008
		title: "No namespace provided for translation file",
		messageFormat: "No namespace found for translation file: {0}",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	internal static readonly DiagnosticDescriptor NoHeaderDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0002",
#pragma warning restore RS2008
		title: "No translation header defined",
		messageFormat: "There's no translation header defined on the first line for translation file",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	internal static readonly DiagnosticDescriptor TranslationEntryTooShortDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0003",
#pragma warning restore RS2008
		title: "Translation entry too short for correct format",
		messageFormat: "The translation entry is too short for the correct format",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	internal static readonly DiagnosticDescriptor TranslationEntryInvalidStartDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0004",
#pragma warning restore RS2008
		title: "Translation entry has invalid start character",
		messageFormat: "Translation entry has invalid start character as it doesn't start with a double quote (\")",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	internal static readonly DiagnosticDescriptor TranslationEntryInvalidEndDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0005",
#pragma warning restore RS2008
		title: "Translation entry has invalid end character",
		messageFormat: "Translation entry has invalid end character as it doesn't end with a double quote (\")",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);


	internal static readonly DiagnosticDescriptor IncorrectTranslationEntryDiagnosticDescriptor = new(
#pragma warning disable RS2008
		id: "LOCALIZER0006",
#pragma warning restore RS2008
		title: "Translation entry has invalid format",
		messageFormat: "Incorrectly formatted translation entry: {0}",
		category: typeof(Generator).FullName,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

}