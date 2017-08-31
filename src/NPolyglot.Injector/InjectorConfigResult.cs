using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPolyglot.Injector
{
    public class InjectorConfigResult
    {
        public static InjectorConfigResult Incorrect(string error) =>
            new InjectorConfigResult(InjectorConfigResultType.IncorrectConfig, null, null, error);

        public static InjectorConfigResult DontInject =
            new InjectorConfigResult(InjectorConfigResultType.DontInject, null, null, null);

        public static InjectorConfigResult Inject(string parser, string transform) =>
            new InjectorConfigResult(InjectorConfigResultType.Inject, parser, transform, null);

        private InjectorConfigResult(InjectorConfigResultType type, string parser, string transform, string error)
        {
            Type = type;
            Parser = parser;
            Transform = transform;
            Error = error;
        }

        public InjectorConfigResultType Type { get; }

        public string Parser { get; }

        public string Transform { get; }

        public string Error { get; }
    }
}
