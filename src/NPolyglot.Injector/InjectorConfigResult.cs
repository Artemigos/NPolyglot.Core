namespace NPolyglot.Injector
{
    public class InjectorConfigResult
    {
        public static InjectorConfigResult Incorrect(string error) =>
            new InjectorConfigResult(InjectorConfigResultType.IncorrectConfig, null, error);

        public static readonly InjectorConfigResult DontInject =
            new InjectorConfigResult(InjectorConfigResultType.DontInject, null, null);

        public static InjectorConfigResult Inject(string injectorName) =>
            new InjectorConfigResult(InjectorConfigResultType.Inject, injectorName, null);

        private InjectorConfigResult(InjectorConfigResultType type, string injectorName, string error)
        {
            Type = type;
            InjectorName = injectorName;
            Error = error;
        }

        public InjectorConfigResultType Type { get; }

        public string InjectorName { get; }

        public string Error { get; }
    }
}
