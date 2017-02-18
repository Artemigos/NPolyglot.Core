using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLanguage
{
    public class SampleTransform : NPolyglot.LanguageDesign.ICodedTransform
    {
        public string ExportName => nameof(SampleTransform);

        public string Transform(object data)
        {
            dynamic d = data;
            return string.Format(
@"namespace SampleNamespace
{
    public class SampleClass
    {
        public string A => ""{0}"";
        public int B => {1};
    }
}", d.A, d.B);
        }
    }
}
