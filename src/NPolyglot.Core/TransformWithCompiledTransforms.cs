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
    public class TransformWithCompiledTransforms : NPolyglotBaseTask
    {
        [Required]
        public ITaskItem[] TransformsDlls { get; set; }

        [Required]
        public ITaskItem[] ParsedFiles { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Output]
        public ITaskItem[] GeneratedOutput { get; set; }

        protected override string TaskName => nameof(TransformWithCompiledTransforms);

        protected override bool ExecuteInternal()
        {
            Log.LogMessage(MessageImportance.Low, "Starting task");
            Log.LogMessage(MessageImportance.Low, "Assemblies to load: {0}", string.Join("; ", TransformsDlls.Select(x => Path.GetFullPath(x.ItemSpec))));

            var assemblies =
                from dll in TransformsDlls
                let path = Path.GetFullPath(dll.ItemSpec)
                select Assembly.LoadFile(path);

            Log.LogMessage(MessageImportance.Low, "Loaded assemblies");
            Log.LogMessage(MessageImportance.Low, "Transform candidates: {0}", string.Join(", ", assemblies.SelectMany(x => x.GetTypes())));

            var transformTypes = assemblies.SelectMany(x => x.FindTransformTypes());
            var transforms = transformTypes
                .Select(x => x.CreateTransform())
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

        private bool IsFileValidForTransform(ITaskItem item) =>
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Object) &&
            item.MetadataNames.Cast<string>().Contains(MetadataNames.Template);
    }
}
