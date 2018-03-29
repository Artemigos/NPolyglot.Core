using System.Text;

namespace ProjectForTesting
{
    public class SubstitutionTest
    {
        public string Test()
        {
#if INJECT_DSL || SubstSample
            test with lang definition
#else
            return null;
#endif
        }
    }
}
