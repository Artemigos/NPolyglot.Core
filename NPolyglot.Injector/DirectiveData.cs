namespace NPolyglot.Injector
{
    public class DirectiveData
    {
        public DirectiveData(string condition, string content)
        {
            Condition = condition;
            Content = content;
        }

        public string Condition { get; }

        public string Content { get; }
    }
}
