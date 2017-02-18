using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLanguage
{
    public class SampleParser : NPolyglot.LanguageDesign.ICodedParser
    {
        public string ExportName => nameof(SampleParser);

        public object Parse(string input) => new { A = "a", B = 42 };
    }
}
