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
    public class TransformWithCompiledTransforms : AppDomainIsolatedTask
    {
        [Required]
        public ITaskItem[] TransformsDlls { get; set; }

        [Required]
        public ITaskItem[] ParsedFiles { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Output]
        public ITaskItem[] GeneratedOutput { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.Low, "Starting task");
                Log.LogMessage(MessageImportance.Low, "Assemblies to load: {0}", string.Join("; ", TransformsDlls.Select(x => Path.GetFullPath(x.ItemSpec))));

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                var assemblies =
                    from dll in TransformsDlls
                    let path = Path.GetFullPath(dll.ItemSpec)
                    select Assembly.LoadFile(path);

                Log.LogMessage(MessageImportance.Low, "Loaded assemblies");
                Log.LogMessage(MessageImportance.Low, "Transform candidates: {0}", string.Join(", ", assemblies.SelectMany(x => x.GetTypes())));

                var transformTypes =
                    from a in assemblies
                    from t in a.GetTypes()
                    where t.IsTransform()
                    select t;

                var transforms = transformTypes
                    .Select(x => x.GetDefaultConstructor().Invoke(new object[0]))
                    .Select(x => (ICodedTransform)new ReflectionBasedTransformWrapper(x))
                    .ToDictionary(x => x.ExportName, x => x);

                Log.LogMessage(MessageImportance.Low, "Found tarnsforms: {0}", string.Join(", ", transforms.Select(x => x.Key)));

                var output = new List<ITaskItem>();
                foreach (var file in ParsedFiles.Where(IsFileValidForTransform))
                {
                    var templateKey = file.GetTemplate();
                    ICodedTransform transform;
                    if (!transforms.TryGetValue(templateKey, out transform))
                    {
                        continue;
                    }

                    var obj = file.GetObject();
                    var result = transform.Transform(obj);

                    var outputFilename = Path.GetFileNameWithoutExtension(file.ItemSpec) + ".cs";
                    var outputPath = Path.Combine(OutputDir, outputFilename);
                    File.WriteAllText(outputPath, result);

                    output.Add(new TaskItem(outputPath));
                }

                GeneratedOutput = output.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Failed to transform DSL files: {0}", e);
                return false;
            }
        }

        private bool IsFileValidForTransform(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Object) &&
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Template);

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
