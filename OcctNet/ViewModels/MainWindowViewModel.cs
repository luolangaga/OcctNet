namespace OcctNet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = CreateGreeting();

    private static string CreateGreeting()
    {
        return Wrapper.OcctRuntime.TryGetNativeVersion(out var version, out var error)
            ? $"OCCT native wrapper loaded. Version: {version}"
            : $"OCCT native wrapper project is ready. Native library not loaded: {error}";
    }
}
