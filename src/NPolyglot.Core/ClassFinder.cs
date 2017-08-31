using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NPolyglot.LanguageDesign;

namespace NPolyglot.Core
{
    public static class ClassFinder
    {
        public static IEnumerable<Type> FindParserTypes(this Assembly assembly) =>
            assembly.GetTypes().Where(x => x.IsParser());

        public static IEnumerable<Type> FindTransformTypes(this Assembly assembly) =>
            assembly.GetTypes().Where(x => x.IsTransform());

        public static object InstanceFromDefaultConstructor(this Type type) =>
            type.GetDefaultConstructor().Invoke(new object[0]);

        public static ICodedParser CreateParser(this Type type) =>
            new ReflectionBasedParserWrapper(type.InstanceFromDefaultConstructor());

        public static ICodedTransform CreateTransform(this Type type) =>
            new ReflectionBasedTransformWrapper(type.InstanceFromDefaultConstructor());

        public static IDictionary<string, ICodedParser> GetParsers(this Assembly assembly) =>
            assembly
                .FindParserTypes()
                .Select(x => x.CreateParser())
                .ToDictionary(x => x.ExportName, x => x);

        public static IDictionary<string, ICodedTransform> GetTransforms(this Assembly assembly) =>
            assembly
                .FindTransformTypes()
                .Select(x => x.CreateTransform())
                .ToDictionary(x => x.ExportName, x => x);

        public static void FindParsersAndTransforms(
            this Assembly assembly,
            out IDictionary<string, ICodedParser> parsers,
            out IDictionary<string, ICodedTransform> transforms)
        {
            parsers = new Dictionary<string, ICodedParser>();
            transforms = new Dictionary<string, ICodedTransform>();
            
            foreach (var t in assembly.GetTypes())
            {
                if (t.IsParser())
                {
                    var parser = t.CreateParser();
                    parsers.Add(parser.ExportName, parser);
                }
                else if (t.IsTransform())
                {
                    var transform = t.CreateTransform();
                    transforms.Add(transform.ExportName, transform);
                }
            }
        }
    }
}