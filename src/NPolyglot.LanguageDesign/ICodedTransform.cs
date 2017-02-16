namespace NPolyglot.LanguageDesign
{
    public interface ICodedTransform
    {
        string ExportName { get; }

        string Transform(object data);
    }
}
