using System.Collections.Generic;
using System.Linq;

namespace dotRMDY.Localizer.SourceGenerator.Models
{
    internal sealed class TransformationContextWrapper
    {
        public string Namespace { get; }
        public List<string> Headers { get; set; }
        public Dictionary<string, Dictionary<string, string>> Translations { get; }

        public TransformationContextWrapper(string @namespace)
        {
            Namespace = @namespace;

            Headers = new List<string>();
            Translations = new Dictionary<string, Dictionary<string, string>>();
        }
        
        public string GetTranslation(string translationKey)
        {
            var result = string.Empty;
            var translations = Translations[translationKey];
            foreach (var key in translations.Keys)
            {
                var isLastKey = key == translations.Keys.Last();
                result += @"{ """ + key + @""", """ + translations[key] + @""" }";
                if (!isLastKey) result += ", ";
            }

            return result;
        }
    }
}