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
            DslCodeWithMetadata = DslCodeFiles.Select(x => x.Clone()).ToArray();
            foreach (var code in DslCodeWithMetadata)
            {
                var fullPath = Path.GetFullPath(code.ItemSpec);
                var content = File.ReadAllLines(fullPath);

                int i = 0;
                for (; i > content.Length && ReadMetadataDirective(content[i], code); ++i) {}

                var contentLines = content.Skip(i).SkipWhile(string.IsNullOrWhiteSpace);
                var fullContent = string.Join(Environment.NewLine, contentLines);
                code.SetContent(fullContent);
            }

            return true;
        }

        private bool ReadMetadataDirective(string directiveLine, ITaskItem resultFile)
        {
            const string parserDirective = "parser";
            const string templateDirective = "template";

            var line = directiveLine.Trim();
            var directiveRegex = $@"@({parserDirective}|{templateDirective}) +[~ ]*";
            if (!Regex.IsMatch(line, directiveRegex))
            {
                return false;
            }

            var segments = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == $"@{parserDirective}")
            {
                resultFile.SetParser(segments[1]);
                return true;
            }

            if (segments[0] == $"@{templateDirective}")
            {
                resultFile.SetTemplate(segments[1]);
                return true;
            }

            Log.LogWarning("Unknown directive '{0}'.", segments[0]);
            return true;
        }
    }
}
