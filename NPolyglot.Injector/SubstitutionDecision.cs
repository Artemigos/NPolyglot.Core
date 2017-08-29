namespace NPolyglot.Injector
{
    public class SubstitutionDecision
    {
        public static SubstitutionDecision DoNotSubstitute = new SubstitutionDecision(false, null);

        public static SubstitutionDecision Substitute(string content) =>
            new SubstitutionDecision(true, content);

        private SubstitutionDecision(bool should, string content)
        {
            ShouldSubstitute = should;
            NewContent = content;
        }

        public bool ShouldSubstitute { get; }

        public string NewContent { get; }
    }
}
