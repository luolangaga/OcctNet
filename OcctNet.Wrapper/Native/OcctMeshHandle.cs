using System.Runtime.InteropServices;

namespace OcctNet.Wrapper.Native;

internal sealed class OcctMeshHandle : SafeHandle
{
    private OcctMeshHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    internal OcctMeshHandle(IntPtr handle)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        return NativeMethods.occtnet_mesh_destroy(handle) == OcctStatus.Ok;
    }
}
