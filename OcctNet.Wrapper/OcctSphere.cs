using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctSphere : OcctShape
{
    public OcctSphere(double radius)
        : base(Create(radius))
    {
        Radius = radius;
    }

    public double Radius { get; }

    private static IntPtr Create(double radius)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_sphere(radius, out var shape));
        return shape;
    }
}
