using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.CSharp;

namespace NPolyglot.Injector
{
    public class ProcessDSLsInCodebase : Task
    {
        private InjectorConfigReader _configReader;

        private IDictionary<string, string> _injectors;

        [Required]
        public ITaskItem[] Injectors { get; set; }

        [Required]
        public ITaskItem[] CodeFiles { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Output]
        public ITaskItem[] GeneratedOutput { get; set; }

        [Output]
        public ITaskItem[] SubstitutedItems { get; set; }

        public string TaskName => nameof(ProcessDSLsInCodebase);

        public override bool Execute()
        {
            try
            {
                return ExecuteInternal();
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to execute task '{TaskName}': {e}");
                throw;
            }
        }

        private bool ExecuteInternal()
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

            var result = TransformContent(data.Content, config.InjectorName);
            return SubstitutionDecision.Substitute(result);
        }

        private string TransformContent(string content, string injectorName)
        {
            if (!_injectors.TryGetValue(injectorName, out var command))
            {
                throw new ArgumentOutOfRangeException(nameof(injectorName), "Could not find injector: " + injectorName);
            }

            var success = RunCommand(command, content, out var output);

            if (!success)
            {
                throw new Exception("Failed to transform DSL into code.");
            }

            return output;
        }

        private bool RunCommand(string command, string input, out string output)
        {
            var proc = new Process();
            var result = new StringBuilder();
            proc.OutputDataReceived += (sender, args) => result.Append(args.Data);
            proc.ErrorDataReceived += (sender, args) => Log.LogError(args.Data);
            proc.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                CreateNoWindow = true
            };

            proc.Start();
            proc.StandardInput.Write(input);
            proc.WaitForExit();

            output = result.ToString();
            return proc.ExitCode == 0;
        }

        private void Initialize()
        {
            _configReader = new InjectorConfigReader();
            _injectors = Injectors.ToDictionary(
                i => i.ItemSpec,
                i => i.GetMetadata("Command"));
        }
    }
}
