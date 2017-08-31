using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NPolyglot.Injector
{
    public class IfPreprocSubstitutor
    {
        public string Substitute(SyntaxNode node, Func<DirectiveData, SubstitutionDecision> decider)
        {
            var result = node.ToFullString();

            var ifs = node.DescendantTrivia().Where(x => x.HasStructure && x.GetStructure() is IfDirectiveTriviaSyntax).ToList();
            ifs.Reverse();

            foreach (var i in ifs)
            {
                var token = i.Token;
                var directive = i.GetStructure() as IfDirectiveTriviaSyntax;
                var related = directive.GetRelatedDirectives();
                var end = related.Last();
                var delimiter = related[1];

                var condition = directive.Condition.ToFullString().Trim();
                var currentContent = result.Substring(i.Span.End, delimiter.SpanStart - i.Span.End);
                var decision = decider(new DirectiveData(condition, currentContent));

                if (decision.ShouldSubstitute)
                {
                    result = Splice(result, i.SpanStart, end.Span.End - 1, decision.NewContent);
                }
            }

            return result;
        }

        private string Splice(string source, int startPos, int endPos, string newVal)
        {
            var before = source.Substring(0, startPos);
            var after = source.Substring(endPos + 1);

            return string.Concat(before, newVal, after);
        }
    }
}
