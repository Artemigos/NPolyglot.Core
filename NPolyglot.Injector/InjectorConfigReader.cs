using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPolyglot.Injector
{
    public class InjectorConfigReader
    {
        public InjectorConfigResult Read(string config)
        {
            var parts = config
                .Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.ToUpperInvariant() != "TRUE")
                .ToArray();

            if (parts.Length == 0 || parts[0] != "INJECT_DSL")
            {
                return InjectorConfigResult.DontInject;
            }

            if (parts.Length == 2)
            {
                return InjectorConfigResult.Inject(
                    parts[1] + "Parser",
                    parts[1] + "Transform");
            }

            if (parts.Length == 3)
            {
                return InjectorConfigResult.Inject(parts[1], parts[2]);
            }

            return InjectorConfigResult.Incorrect("Incorrect number of injection configuration elements.");
        }
    }
}
