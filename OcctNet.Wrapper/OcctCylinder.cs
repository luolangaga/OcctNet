using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctCylinder : OcctShape
{
    public OcctCylinder(double radius, double height)
        : base(Create(radius, height))
    {
        Radius = radius;
        Height = height;
    }

    public double Radius { get; }

    public double Height { get; }

    private static IntPtr Create(double radius, double height)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_cylinder(radius, height, out var shape));
        return shape;
    }
}
