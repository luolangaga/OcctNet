using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OcctNet.Wrapper.Native;

internal static class NativeMethods
{
    private const string LibraryName = "OcctNetNative";
    private const int ErrorBufferSize = 4096;

    private static readonly bool ResolverInstalled = InstallResolver();

    internal static void EnsureLoaded()
    {
        _ = ResolverInstalled;
    }

    internal static string GetVersion()
    {
        EnsureLoaded();

        var buffer = new byte[256];
        ThrowIfFailed(occtnet_get_version(buffer, buffer.Length));
        return DecodeUtf8(buffer);
    }

    internal static bool TryGetVersion(out string version, out string? error)
    {
        try
        {
            version = GetVersion();
            error = null;
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or OcctException)
        {
            version = string.Empty;
            error = ex.Message;
            return false;
        }
    }

    internal static void ThrowIfFailed(int status)
    {
        if (status == OcctStatus.Ok)
        {
            return;
        }

        throw new OcctException(ReadLastError(), status);
    }

    internal static string ReadLastError()
    {
        var buffer = new byte[ErrorBufferSize];

        try
        {
            _ = occtnet_get_last_error(buffer, buffer.Length);
            var message = DecodeUtf8(buffer);
            return string.IsNullOrWhiteSpace(message)
                ? "OCCT native call failed without an error message."
                : message;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            return ex.Message;
        }
    }

    private static bool InstallResolver()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ResolveNativeLibrary);
        return true;
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        foreach (var candidate in EnumerateLibraryCandidates(assembly))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> EnumerateLibraryCandidates(Assembly assembly)
    {
        var mappedName = NativeLibraryName();
        var rid = RuntimeIdentifier();

        var explicitLibrary = Environment.GetEnvironmentVariable("OCCTNET_NATIVE_LIBRARY");
        if (!string.IsNullOrWhiteSpace(explicitLibrary))
        {
            yield return explicitLibrary;
        }

        var explicitDirectory = Environment.GetEnvironmentVariable("OCCTNET_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(explicitDirectory))
        {
            yield return Path.Combine(explicitDirectory, mappedName);
        }

        yield return Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", mappedName);
        yield return Path.Combine(AppContext.BaseDirectory, mappedName);

        var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
        if (!string.IsNullOrWhiteSpace(assemblyDirectory))
        {
            yield return Path.Combine(assemblyDirectory, "runtimes", rid, "native", mappedName);
            yield return Path.Combine(assemblyDirectory, mappedName);
        }

        yield return LibraryName;
    }

    private static string NativeLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{LibraryName}.dll";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"lib{LibraryName}.dylib";
        }

        return $"lib{LibraryName}.so";
    }

    private static string RuntimeIdentifier()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "osx"
                : "linux";

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "arm64",
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
        };

        return $"{os}-{architecture}";
    }

    private static string DecodeUtf8(byte[] buffer)
    {
        var length = Array.IndexOf(buffer, (byte)0);
        if (length < 0)
        {
            length = buffer.Length;
        }

        return Encoding.UTF8.GetString(buffer, 0, length);
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_get_version([Out] byte[] buffer, int bufferLength);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_get_last_error([Out] byte[] buffer, int bufferLength);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_point_create(double x, double y, double z, out IntPtr point);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_point_destroy(IntPtr point);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_point_get_coordinates(IntPtr point, out double x, out double y, out double z);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_point_set_coordinates(IntPtr point, double x, double y, double z);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occtnet_point_distance(IntPtr left, IntPtr right, out double distance);
}
