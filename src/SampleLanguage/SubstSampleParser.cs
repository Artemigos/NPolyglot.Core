using System;

namespace SampleLanguage
{
    public class SubstSampleParser
    {
        public object Parse(string input)
        {
            return input.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
