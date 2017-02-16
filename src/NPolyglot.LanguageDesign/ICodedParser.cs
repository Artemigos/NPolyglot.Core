namespace NPolyglot.LanguageDesign
{
    public interface ICodedParser
    {
        string ExportName { get; }

        object Parse(string input);
    }
}