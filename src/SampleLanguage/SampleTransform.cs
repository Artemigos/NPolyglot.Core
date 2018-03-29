namespace SampleLanguage
{
    public class SampleTransform
    {
        public string Transform(object data)
        {
            dynamic d = data;
            return string.Format(
@"namespace SampleNamespace
{{
    public class SampleClass
    {{
        public string A {{ get {{ return ""{0}""; }} }}
        public int B {{ get {{ return {1}; }} }}
    }}
}}", d.A, d.B);
        }
    }
}
