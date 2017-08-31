using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using NPolyglot.Core;
using NPolyglot.LanguageDesign;

namespace NPolyglot.Injector
{
    public class ProcessDSLsInCodebase : NPolyglotBaseTask
    {
        private InjectorConfigReader _configReader;

        private IDictionary<string, ICodedParser> _parsers;

        private IDictionary<string, ICodedTransform> _transforms;

        [Required]
        public ITaskItem[] ParsersDlls { get; set; }

        [Required]
        public ITaskItem[] TransformsDlls { get; set; }

        [Required]
        public ITaskItem[] CodeFiles { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Output]
        public ITaskItem[] GeneratedOutput { get; set; }

        [Output]
        public ITaskItem[] SubstitutedItems { get; set; }

        protected override string TaskName => nameof(ProcessDSLsInCodebase);

        protected override bool ExecuteInternal()
        {
            Initialize();

            var substitutor = new IfPreprocSubstitutor();
            var generated = new List<ITaskItem>();
            var substituted = new List<ITaskItem>();

            foreach (var file in CodeFiles)
            {
                var path = Path.GetFullPath(file.ItemSpec);
                var code = File.ReadAllText(path);
                var tree = CSharpSyntaxTree.ParseText(code).GetRoot();
                var resultCode = substitutor.Substitute(tree, HandleSubstitutableSegment);

                if (code != resultCode)
                {
                    var fileName = Path.GetFileName(path);
                    var fullPath = Path.Combine(OutputDir, fileName);
                    File.WriteAllText(fullPath, resultCode);

                    generated.Add(new TaskItem(fullPath));
                    substituted.Add(file);
                }
            }

            GeneratedOutput = generated.ToArray();
            SubstitutedItems = substituted.ToArray();

            return true;
        }

        private SubstitutionDecision HandleSubstitutableSegment(DirectiveData data)
        {
            var config = _configReader.Read(data.Condition);

            if (config.Type == InjectorConfigResultType.DontInject)
            {
                return SubstitutionDecision.DoNotSubstitute;
            }

            if (config.Type == InjectorConfigResultType.IncorrectConfig)
            {
                Log.LogWarning(config.Error);
                return SubstitutionDecision.DoNotSubstitute;
            }

            var result = ParseDsl(data.Content, config.Parser, config.Transform);
            return SubstitutionDecision.Substitute(result);
        }

        private string ParseDsl(string content, string parserKey, string transformKey)
        {
            if (!_parsers.TryGetValue(parserKey, out var parser))
            {
                throw new ArgumentOutOfRangeException(nameof(parserKey), "Could not find parser: " + parserKey);
            }

            if (!_transforms.TryGetValue(transformKey, out var transform))
            {
                throw new ArgumentOutOfRangeException(nameof(transformKey), "Could not find transform: " + transformKey);
            }

            var obj = parser.Parse(content);
            var newContent = transform.Transform(obj);
            return newContent;
        }

        private void Initialize()
        {
            _configReader = new InjectorConfigReader();

            var assemblies = ParsersDlls
                .Union(TransformsDlls)
                .Select(dll => Path.GetFullPath(dll.ItemSpec))
                .Distinct()
                .Select(Assembly.LoadFile);

            assemblies
                .SelectMany(x => x.GetTypes())
                .FindParsersAndTransforms(out _parsers, out _transforms);
        }
    }
}