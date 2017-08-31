using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NPolyglot.Core
{
    public abstract class NPolyglotBaseTask : AppDomainIsolatedTask
    {
        public override bool Execute()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                return ExecuteInternal();
            }
            catch (Exception e)
            {
                Log.LogError("Failed to execute task '{0}': ", TaskName, e);
                return false;
            }
        }

        protected abstract string TaskName { get; }

        protected abstract bool ExecuteInternal();

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