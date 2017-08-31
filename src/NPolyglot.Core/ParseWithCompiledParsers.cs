using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NPolyglot.LanguageDesign;

namespace NPolyglot.Core
{
    public class ParseWithCompiledParsers : NPolyglotBaseTask
    {
        [Required]
        public ITaskItem[] ParsersDlls { get; set; }

        [Required]
        public ITaskItem[] DslCodeFiles { get; set; }

        [Output]
        public ITaskItem[] DslCodeWithMetadata { get; set; }

        protected override string TaskName => nameof(ParseWithCompiledParsers);

        protected override bool ExecuteInternal()
        {
            Log.LogMessage(MessageImportance.Low, "Starting task");
            Log.LogMessage(MessageImportance.Low, "Assemblies to load: {0}", string.Join("; ", ParsersDlls.Select(x => Path.GetFullPath(x.ItemSpec))));

            var assemblies =
                from dll in ParsersDlls
                let path = Path.GetFullPath(dll.ItemSpec)
                select Assembly.LoadFile(path);

            Log.LogMessage(MessageImportance.Low, "Loaded assemblies");
            Log.LogMessage(MessageImportance.Low, "Parser candidates: {0}", string.Join(", ", assemblies.SelectMany(x => x.GetTypes())));

            var parserTypes = assemblies.SelectMany(x => x.FindParserTypes());
            var parsers = parserTypes
                .Select(x => x.CreateParser())
                .ToDictionary(x => x.ExportName, x => x);

            Log.LogMessage(MessageImportance.Low, "Found parsers: {0}", string.Join(", ", parsers.Select(x => x.Key)));

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

        private bool IsFileValidForParsing(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Parser) &&
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Content);
    }
}