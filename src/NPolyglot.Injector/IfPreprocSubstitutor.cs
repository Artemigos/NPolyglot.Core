using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPolyglot.Injector
{
    public class IfPreprocSubstitutor
    {
        public string Substitute(string node, Func<DirectiveData, SubstitutionDecision> decider)
        {
            var result = node;
            var ifs = FindIfs(node);
            ifs.Reverse();

            foreach (var i in ifs)
            {
                var decision = decider(new DirectiveData(i.condition, i.content));

                if (decision.ShouldSubstitute)
                {
                    result = Splice(result, i.start, i.end - 1, decision.NewContent);
                }
            }

            return result;
        }

        private IEnumerable<(int start, int end, string condition, string content)> FindIfs(string code)
        {
            const string re = @"[\r\n]\s*(#if\s+(\S.*?)[\r\n](.*?)([\r\n]\s*#else\s*[\r\n].*?)?[\r\n]\s*#endif)\s*[\r\n]";
            var matches = Regex.Matches(code, re, RegexOptions.Singleline);

            foreach (Match m in matches)
            {
                Group entire = m.Groups[1];
                Group cond = m.Groups[2];
                Group val = m.Groups[3];
                yield return (entire.Index, entire.Index + entire.Length, cond.Value, val.Value);
            }
        }

        private string Splice(string source, int startPos, int endPos, string newVal)
        {
            var before = source.Substring(0, startPos);
            var after = source.Substring(endPos + 1);

            return string.Concat(before, newVal, after);
        }
    }
}
