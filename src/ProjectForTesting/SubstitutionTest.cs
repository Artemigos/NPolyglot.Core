using System.Text;

namespace ProjectForTesting
{
    public class SubstitutionTest
    {
        public string TestWithSeparateDefinitions()
        {
#if INJECT_DSL || SubstSampleParser || SubstSampleTransform
            test with separate definitions
#else
            return null;
#endif
        }

        public string TestWithLangDefinition()
        {
#if INJECT_DSL || SubstSample
            test with lang definition
#else
            return null;
#endif
        }
    }
}
