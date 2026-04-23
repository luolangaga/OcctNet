using CommunityToolkit.Mvvm.ComponentModel;

namespace OcctNet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string nativeStatus = CreateNativeStatus();

    [ObservableProperty]
    private string activeTool = "选择";

    [ObservableProperty]
    private string statusMessage = "选择一个工具开始建模";

    [ObservableProperty]
    private int entityCount;

    private static string CreateNativeStatus()
    {
        return Wrapper.OcctRuntime.TryGetNativeVersion(out var version, out var error)
            ? $"OCCT {version}"
            : $"OCCT 未加载: {error}";
    }
}
