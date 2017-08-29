using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NPolyglot.LanguageDesign;

namespace NPolyglot.Core
{
    public class PreprocessDslCode : Task
    {
        [Required]
        public ITaskItem[] DslCodeFiles { get; set; }

        [Output]
        public ITaskItem[] DslCodeWithMetadata { get; set; }

        public override bool Execute()
        {
            try
            {
                var reader = new MetadataReader();
                DslCodeWithMetadata = DslCodeFiles.Select(x => x.Clone()).ToArray();

                foreach (var code in DslCodeWithMetadata)
                {
                    var fullPath = Path.GetFullPath(code.ItemSpec);
                    var content = File.ReadAllText(fullPath);

                    var result = reader.ReadMetadata(content, out var codeContent);

                    if (result.HasParser)
                    {
                        code.SetParser(result.Parser);
                    }

                    if (result.HasTransform)
                    {
                        code.SetTemplate(result.Transform);
                    }

                    code.SetContent(codeContent);
                }

                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Failed to preprocess DSL files: {0}", e);
                return false;
            }
        }
    }
}
