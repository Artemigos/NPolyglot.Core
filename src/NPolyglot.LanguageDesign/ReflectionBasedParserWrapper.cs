using System;
using System.Reflection;

namespace NPolyglot.LanguageDesign
{
    public class ReflectionBasedParserWrapper : ICodedParser
    {
        private readonly object _wrapped;

        private readonly MethodInfo _parse;

        private readonly PropertyInfo _exportName;

        public ReflectionBasedParserWrapper(object wrapped)
        {
            Type t = wrapped.GetType();
            System.Diagnostics.Debug.Assert(
                ConventionReflection.IsParser(t));

            _wrapped = wrapped;
            _parse = ConventionReflection.GetParseMethod(t);
            _exportName = ConventionReflection.GetExportNameProperty(t);
        }

        public string ExportName =>
            (string)_exportName.GetValue(_wrapped);

        public object Parse(string input) =>
            _parse.Invoke(_wrapped, new[] { input });
    }
}