using System;
using System.Reflection;

namespace NPolyglot.LanguageDesign
{
    public static class ConventionReflection
    {
        public static MethodInfo GetParseMethod(this Type t)
        {
            var method = t.GetMethod("Parse", new[] { typeof(string) });
            if (method == null || method.ReturnType != typeof(object))
            {
                return null;
            }

            return method;
        }

        public static MethodInfo GetTransformMethod(this Type t)
        {
            var method = t.GetMethod("Transform", new[] { typeof(object) });
            if (method == null || method.ReturnType != typeof(string))
            {
                return null;
            }

            return method;
        }

        public static PropertyInfo GetExportNameProperty(this Type t) =>
            t.GetProperty("ExportName", typeof(string));

        public static ConstructorInfo GetDefaultConstructor(this Type t) =>
            t.GetConstructor(new Type[0]);

        public static bool IsParser(this Type t) =>
            t.GetDefaultConstructor() != null &&
            t.GetExportNameProperty() != null &&
            t.GetParseMethod() != null;

        public static bool IsTransform(this Type t) =>
            t.GetDefaultConstructor() != null &&
            t.GetExportNameProperty() != null &&
            t.GetTransformMethod() != null;
    }
}