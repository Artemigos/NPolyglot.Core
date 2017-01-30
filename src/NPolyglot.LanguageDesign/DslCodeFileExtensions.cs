using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NPolyglot.LanguageDesign
{
    public static class DslCodeFileExtensions
    {
        public static ITaskItem Clone(this ITaskItem item) =>
            new TaskItem(item);

        public static void SetContent(this ITaskItem item, string content) =>
            item.SetMetadata(MetadataNames.Content, content);

        public static string GetContent(this ITaskItem item) =>
            item.GetMetadata(MetadataNames.Content);

        public static void SetParser(this ITaskItem item, string parser) =>
            item.SetMetadata(MetadataNames.Parser, parser);

        public static string GetParser(this ITaskItem item) =>
            item.GetMetadata(MetadataNames.Parser);

        public static void SetTemplate(this ITaskItem item, string template) =>
            item.SetMetadata(MetadataNames.Template, template);

        public static string GetTemplate(this ITaskItem item) =>
            item.GetMetadata(MetadataNames.Template);

        public static void SetSerializedObject(this ITaskItem item, string @object) =>
            item.SetMetadata(MetadataNames.Object, @object);

        public static string GetSerializedObject(this ITaskItem item) =>
            item.GetMetadata(MetadataNames.Object);

        public static void SetObject(this ITaskItem item, object @object)
        {
            var serialized = JsonConvert.SerializeObject(@object);
            item.SetSerializedObject(serialized);
        }

        public static object GetObject(this ITaskItem item)
        {
            var serialized = item.GetSerializedObject();
            var wrap = "{\"root\":" + serialized + "}";
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(wrap, new ExpandoObjectConverter());

            return obj.root;
        }
    }
}
