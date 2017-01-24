using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            DslCodeWithMetadata = DslCodeFiles.Select(x => new TaskItem(x)).ToArray();
            for (int i = 0; i < DslCodeWithMetadata.Length; ++i)
            {
                var code = DslCodeWithMetadata[i];
                var filename = Path.GetFileName(code.ItemSpec);
                var fullPath = Path.GetFullPath(code.ItemSpec);
                var content = File.ReadAllLines(fullPath);

                int j = 0;
                for (; ReadMetadataDirective(content[j], code); ++j) {}

                var contentLines = content.Skip(j).SkipWhile(string.IsNullOrWhiteSpace);
                code.SetMetadata("Content", string.Join(Environment.NewLine, contentLines));
            }

            return true;
        }

        private bool ReadMetadataDirective(string directiveLine, ITaskItem resultFile)
        {
            var line = directiveLine.Trim();
            var directiveRegex = @"@(parser|template) +[~ ]*";
            if (!Regex.IsMatch(line, directiveRegex))
            {
                return false;
            }

            var segments = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "@parser")
            {
                resultFile.SetMetadata("Parser", segments[1]);
                return true;
            }

            if (segments[0] == "@template")
            {
                resultFile.SetMetadata("Template", segments[1]);
                return true;
            }

            Log.LogWarning("Unknown directive '{0}'.", segments[0]);
            return true;
        }
    }
}
