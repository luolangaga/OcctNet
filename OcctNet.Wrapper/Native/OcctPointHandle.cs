using System.Runtime.InteropServices;

namespace OcctNet.Wrapper.Native;

internal sealed class OcctPointHandle : SafeHandle
{
    private OcctPointHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    internal OcctPointHandle(IntPtr handle)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        return NativeMethods.occtnet_point_destroy(handle) == OcctStatus.Ok;
    }
}
