using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public static class OcctRuntime
{
    public static string NativeVersion => NativeMethods.GetVersion();

    public static bool TryGetNativeVersion(out string version, out string? error)
    {
        return NativeMethods.TryGetVersion(out version, out error);
    }
}
