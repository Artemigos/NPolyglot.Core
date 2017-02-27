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
                Log.LogMessage(MessageImportance.Low, "Starting task");
                Log.LogMessage(MessageImportance.Low, "Assemblies to load: {0}", string.Join("; ", ParsersDlls.Select(x => Path.GetFullPath(x.ItemSpec))));

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                var assemblies =
                    from dll in ParsersDlls
                    let path = Path.GetFullPath(dll.ItemSpec)
                    select Assembly.LoadFile(path);

                Log.LogMessage(MessageImportance.Low, "Loaded assemblies");
                Log.LogMessage(MessageImportance.Low, "Parser candidates: {0}", string.Join(", ", assemblies.SelectMany(x => x.GetTypes())));

                var parserTypes =
                    from a in assemblies
                    from t in a.GetTypes()
                    where IsValidParser(t)
                    select t;

                var parsers = parserTypes
                    .Select(x => GetDefaultConstructor(x).Invoke(new object[0]))
                    .Cast<ICodedParser>()
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
            catch (Exception e)
            {
                Log.LogError("Failed to parse DSL files: {0}", e);
                return false;
            }
        }

        private bool IsValidParser(Type t)
        {
            if (!ImplementsCodedParser(t))
            {
                Log.LogMessage(MessageImportance.Low, "Validating whether '{0}' is a parser - doesn't implement parser interface", t.ToString());
                return false;
            }

            if (GetDefaultConstructor(t) == null)
            {
                Log.LogMessage(MessageImportance.Low, "Validating whether '{0}' is a parser - doesn't have a default constructor", t.ToString());
                return false;
            }

            Log.LogMessage(MessageImportance.Low, "Found that '{0}' is a valid parser", t.ToString());
            return true;
        }

        private ConstructorInfo GetDefaultConstructor(Type t) =>
            t.GetConstructor(new Type[0]);

        private bool ImplementsCodedParser(Type t) =>
            t.GetInterfaces().Contains(typeof(ICodedParser));

        private bool IsFileValidForParsing(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Parser) &&
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Content);

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);
            if (alreadyLoaded != null)
            {
                return alreadyLoaded;
            }

            var name = args.Name.Split(',')[0];
            var path = Path.GetDirectoryName(args.RequestingAssembly.Location);
            var assemblies = Directory.GetFiles(path, $"{name}.dll");
            var found = assemblies.FirstOrDefault();

            if (found == null)
            {
                Log.LogMessage(MessageImportance.Low, "Failed to find assembly '{0}' for '{1}'", args.Name, args.RequestingAssembly.FullName);
                return Assembly.Load(args.Name);
            }

            Log.LogMessage(MessageImportance.Low, "Found assembly '{0}' for '{1}'", found, args.RequestingAssembly.FullName);
            return Assembly.LoadFile(found);
        }
    }
}