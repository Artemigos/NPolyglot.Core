using System;
using NPolyglot.LanguageDesign;

namespace SampleLanguage
{
    public class SubstSampleParser : ICodedParser
    {
        public string ExportName => nameof(SubstSampleParser);

        public object Parse(string input)
        {
            return input.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}