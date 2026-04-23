using System.Runtime.InteropServices;

namespace OcctNet.Wrapper.Native;

internal sealed class OcctShapeHandle : SafeHandle
{
    private OcctShapeHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    internal OcctShapeHandle(IntPtr handle)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        return NativeMethods.occtnet_shape_destroy(handle) == OcctStatus.Ok;
    }
}
