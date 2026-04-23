using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctFace : OcctShape
{
    public OcctFace(OcctWire outerWire)
        : base(Create(outerWire))
    {
        OuterWire = outerWire;
    }

    public OcctWire OuterWire { get; }

    private static IntPtr Create(OcctWire outerWire)
    {
        ArgumentNullException.ThrowIfNull(outerWire);
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_face(outerWire.DangerousGetHandle(), out var shape));
        return shape;
    }
}
