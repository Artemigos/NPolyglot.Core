using System;
using System.Reflection;

namespace NPolyglot.LanguageDesign
{
    public class ReflectionBasedTransformWrapper : ICodedTransform
    {
        private readonly object _wrapped;

        private readonly MethodInfo _transform;

        private readonly PropertyInfo _exportName;

        public ReflectionBasedTransformWrapper(object wrapped)
        {
            var t = wrapped.GetType();
            System.Diagnostics.Debug.Assert(
                ConventionReflection.IsTransform(t));

            _wrapped = wrapped;
            _transform = ConventionReflection.GetTransformMethod(t);
            _exportName = ConventionReflection.GetExportNameProperty(t);
        }

        public string ExportName =>
            (string)_exportName.GetValue(_wrapped);

        public string Transform(object data) =>
            (string)_transform.Invoke(_wrapped, new[] { data });
    }
}