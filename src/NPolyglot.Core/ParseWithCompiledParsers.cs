using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NPolyglot.LanguageDesign;

namespace NPolyglot.Core
{
    public class ParseWithCompiledParsers : AppDomainIsolatedTask
    {
        [Required]
        public ITaskItem[] ParsersDlls { get; set; }

        [Required]
        public ITaskItem[] DslCodeFiles { get; set; }

        [Output]
        public ITaskItem[] DslCodeWithMetadata { get; set; }

        public override bool Execute()
        {
            try
            {
                var assemblies =
                    from dll in ParsersDlls
                    let path = Path.GetFullPath(dll.ItemSpec)
                    select Assembly.Load(path);

                var parserTypes =
                    from a in assemblies
                    from t in a.GetTypes()
                    where IsValidParser(t)
                    select t;

                var parsers = parserTypes
                    .Select(x => GetDefaultConstructor(x).Invoke(new object[0]))
                    .Cast<ICodedParser>()
                    .ToDictionary(x => x.ExportName, x => x);

                DslCodeWithMetadata = DslCodeFiles.Select(x => new TaskItem(x)).ToArray();
                foreach (var file in DslCodeWithMetadata.Where(IsFileValidForParsing))
                {
                    var parserName = file.GetParser();
                    ICodedParser parser;
                    if (!parsers.TryGetValue(parserName, out parser))
                    {
                        continue;
                    }

                    var content = file.GetContent();
                    var result = parser.Parse(content);
                    file.SetObject(result);
                }

                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Failed to parse DSL files: {0}", e);
                return false;
            }
        }

        private bool IsValidParser(Type t) =>
            ImplementsCodedParser(t) && GetDefaultConstructor(t) != null;

        private ConstructorInfo GetDefaultConstructor(Type t) =>
            t.GetConstructor(new Type[0]);

        private bool ImplementsCodedParser(Type t) =>
            t.GetInterfaces().Contains(typeof(ICodedParser));

        private bool IsFileValidForParsing(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Parser) &&
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Content);
    }
}