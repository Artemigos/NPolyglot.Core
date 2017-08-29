using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPolyglot.Core
{
    public class MetadataReader
    {
        public MetadataEntries ReadMetadata(string input, out string trimmed) =>
            ReadMetadata(input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries), out trimmed);

        public MetadataEntries ReadMetadata(string[] lines, out string trimmed)
        {
            var result = new MetadataEntries();
            int i = 0;

            for (i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                if (IsParserDirective(line))
                {
                    var parser = ExtractParser(line);
                    result.SetParser(parser);
                }
                else if (IsTransformDirective(line))
                {
                    var transform = ExtractTransform(line);
                    result.SetTransform(transform);
                }
                else if (IsLangDirective(line))
                {
                    var lang = ExtractLang(line);
                    result.SetParser(lang + "Parser");
                    result.SetTransform(lang + "Transform");
                }
                else
                {
                    break;
                }
            }

            trimmed = string.Join(Environment.NewLine, lines.Skip(i));
            return result;
        }

        public bool IsParserDirective(string line) =>
            Regex.IsMatch(line, @"@parser [^\s]+");

        public string ExtractParser(string line) =>
            Regex.Match(line, @"@parser ([^\s]+)").Groups[1].Value;

        public bool IsTransformDirective(string line) =>
            Regex.IsMatch(line, @"@transform [^\s]+");

        public string ExtractTransform(string line) =>
            Regex.Match(line, @"@transform ([^\s]+)").Groups[1].Value;

        public bool IsLangDirective(string line) =>
            Regex.IsMatch(line, @"@[^\s]+");

        public string ExtractLang(string line) =>
            Regex.Match(line, @"@([^\s]+)").Groups[1].Value;
    }
}
