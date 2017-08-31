using System.Text;
using NPolyglot.LanguageDesign;

namespace SampleLanguage
{
    public class SubstSampleTransform : ICodedTransform
    {
        public string ExportName => nameof(SubstSampleTransform);

        public string Transform(object data)
        {
            var words = (string[])data;
            var b = new StringBuilder();
            b.AppendLine("StringBuilder b = new StringBuilder();");

            foreach (var word in words)
            {
                b.AppendFormat("b.AppendLine(\"{0}\");\n", word);
            }

            b.AppendLine("return b.ToString();");
            return b.ToString();
        }
    }
}