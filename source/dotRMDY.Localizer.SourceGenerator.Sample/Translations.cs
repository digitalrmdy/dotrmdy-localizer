using System.Globalization;

namespace dotRMDY.Localizer.SourceGenerator.Sample
{
	public static partial class Translations
	{
		private static Dictionary<TranslationKey, Dictionary<string, string>> TranslationItems { get; }

		static Translations()
		{
			TranslationItems = new Dictionary<TranslationKey, Dictionary<string, string>>();
			Load();
		}

		internal static string GetStringByCulture(TranslationKey key, CultureInfo? culture = null)
		{
			var found = TranslationItems.TryGetValue(key, out var translations);
			if (!found)
			{
				return $"[{culture}]{key}";
			}

			found = translations.TryGetValue(culture.ToString(), out var translation);
			if (!found)
			{
				return $"[{culture}]{key}";
			}

			if (string.IsNullOrEmpty(translation)
			    && translation != "|")
			{
				return $"[{culture}]{key}";
			}

			return translation;
		}

		public static string GetString(TranslationKey key)
		{
			return GetStringByCulture(key, null);
		}
	}
}