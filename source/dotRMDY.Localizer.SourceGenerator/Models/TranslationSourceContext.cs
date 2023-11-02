using System.Collections.Generic;

namespace dotRMDY.Localizer.SourceGenerator.Models;

internal sealed record TranslationSourceContext(string Namespace, List<string> LanguageCodes, List<TranslationEntry> Translations);