using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using dotRMDY.Localizer.SourceGenerator.Diagnostics;
using dotRMDY.Localizer.SourceGenerator.Helpers;
using dotRMDY.Localizer.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace dotRMDY.Localizer.SourceGenerator;

partial class Generator
{
	internal class Parser
	{
		private readonly AdditionalText _additionalText;

		private string AdditionalTextPath => _additionalText.Path ?? throw new InvalidOperationException();

		private SourceText? _sourceText;

		public Parser(AdditionalText additionalText)
		{
			_additionalText = additionalText;
		}

		public bool Execute(
			CancellationToken ct,
			[NotNullWhen(true)] out List<string>? languageCodes,
			[NotNullWhen(true)] out List<TranslationEntry>? translations,
			out ImmutableArray<Diagnostic> diagnostics)
		{
			using var immutableDiagnosticArrayBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

			var sourceText = _additionalText.GetText(ct);
			if (sourceText == null)
			{
				goto failure;
			}

			_sourceText = sourceText;

			var textLineCollection = _sourceText.Lines;

			ct.ThrowIfCancellationRequested();

			// Parse the header
			var headerLine = textLineCollection.First();
			if (!TryParseHeader(headerLine, out languageCodes, out var diagnostic))
			{
				immutableDiagnosticArrayBuilder.Add(diagnostic);

				goto failure;
			}

			ct.ThrowIfCancellationRequested();

			// Parse the translation entries
			var translationEntryLines = textLineCollection
				.Skip(1)
				.Where(x => !x.Span.IsEmpty)
				.ToList();
			translations = new List<TranslationEntry>(translationEntryLines.Count);

			foreach (var translationEntryLine in translationEntryLines)
			{
				if (TryParseLine(translationEntryLine, languageCodes, out var translationEntry, out diagnostic))
				{
					translations.Add(translationEntry);
				}
				else
				{
					immutableDiagnosticArrayBuilder.Add(diagnostic);
				}

				ct.ThrowIfCancellationRequested();
			}

			diagnostics = immutableDiagnosticArrayBuilder.ToImmutable();

			return true;


			failure:
			languageCodes = null;
			translations = null;

			diagnostics = immutableDiagnosticArrayBuilder.ToImmutable();

			return false;
		}

		private bool TryParseHeader(
			TextLine textLine,
			[NotNullWhen(true)] out List<string>? languageCodes,
			[NotNullWhen(false)] out Diagnostic? diagnostic)
		{
			if (textLine.Span.IsEmpty)
			{
				languageCodes = null;

				diagnostic = CreateDiagnostic(DiagnosticDescriptors.NoHeaderDiagnosticDescriptor, textLine);
				return false;
			}

			if (!TryParseLineInternal(textLine, out languageCodes, out diagnostic))
			{
				return false;
			}

			languageCodes = languageCodes
				.Skip(1)
				.Select(codeRaw => codeRaw[^5..])
				.ToList();
			return true;
		}

		private bool TryParseLine(
			TextLine textLine,
			IReadOnlyCollection<string> languageCodes,
			[NotNullWhen(true)] out TranslationEntry? translationEntry,
			[NotNullWhen(false)] out Diagnostic? diagnostic)
		{
			if (!TryParseLineInternal(textLine, out var values, out diagnostic))
			{
				translationEntry = null;
				return false;
			}

			// Check if the number of translation entries matches the amount of language codes specified in the header
			if (values.Count - 1 != languageCodes.Count)
			{
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.IncorrectTranslationEntryDiagnosticDescriptor,
					textLine,
					$"TranslationEntry has incorrect amount of values compared to language codes in header. Expected {languageCodes.Count}, got {values.Count - 1}");

				translationEntry = null;
				return false;
			}

			// Check if the translationKey is not empty
			if (string.IsNullOrWhiteSpace(values[0]))
			{
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.IncorrectTranslationEntryDiagnosticDescriptor,
					textLine,
					"TranslationKey is empty");

				translationEntry = null;
				return false;
			}

			translationEntry = new TranslationEntry(values[0], values
				.Skip(1)
				.Select(EscapeString)
				.ToArray());
			return true;
		}

		private bool TryParseLineInternal(
			TextLine textLine,
			[NotNullWhen(true)] out List<string>? values,
			[NotNullWhen(false)] out Diagnostic? diagnostic)
		{
			var textLineString = textLine.ToString();

			// Check if the line is fine, 6 is the minimum length of a line which would look like this "a",""
			if (textLineString.Length < 6)
			{
				values = null;
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.TranslationEntryTooShortDiagnosticDescriptor,
					textLine);
				return false;
			}

			// Check if the line starts with "
			if (textLineString[0] != '"')
			{
				values = null;
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.TranslationEntryInvalidStartDiagnosticDescriptor,
					textLine);
				return false;
			}

			// Check if the line ends with "
			if (textLineString[^1] != '"')
			{
				values = null;
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.TranslationEntryInvalidEndDiagnosticDescriptor,
					textLine);
				return false;
			}

			var textLineSpan = textLineString[1..^1].AsSpan();

			var separatorRefSpan = "\",\"".AsSpan();

			var translationValues = new List<string>();

			var separatorIndex = textLineSpan.IndexOf(separatorRefSpan);
			if (separatorIndex < 0)
			{
				values = null;
				diagnostic = CreateDiagnostic(
					DiagnosticDescriptors.IncorrectTranslationEntryDiagnosticDescriptor,
					textLine,
					"Separator not found");
				return false;
			}

			do
			{
				translationValues.Add(textLineSpan[..separatorIndex].ToString());

				textLineSpan = textLineSpan[(separatorIndex + separatorRefSpan.Length)..];
				separatorIndex = textLineSpan.IndexOf(separatorRefSpan);
			} while (separatorIndex >= 0);

			translationValues.Add(textLineSpan.ToString());

			values = translationValues;
			diagnostic = null;
			return true;
		}

		private static string EscapeString(string input)
		{
			return input.Replace(@"""""", @"\""");
		}

		private Diagnostic CreateDiagnostic(
			DiagnosticDescriptor diagnosticDescriptor,
			TextLine textLine,
			params object?[]? messageArgs)
		{
			var linePositionStart = new LinePosition(textLine.LineNumber, 0);
			var linePositionEnd = new LinePosition(textLine.LineNumber, textLine.End - textLine.Start);

			return Diagnostic.Create(
				diagnosticDescriptor,
				Location.Create(
					AdditionalTextPath,
					textLine.Span,
					new LinePositionSpan(linePositionStart, linePositionEnd)),
				messageArgs);
		}
	}
}