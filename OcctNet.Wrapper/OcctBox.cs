using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctBox : OcctShape
{
    public OcctBox(double sizeX, double sizeY, double sizeZ)
        : base(Create(sizeX, sizeY, sizeZ))
    {
        SizeX = sizeX;
        SizeY = sizeY;
        SizeZ = sizeZ;
    }

    public double SizeX { get; }

    public double SizeY { get; }

    public double SizeZ { get; }

    private static IntPtr Create(double sizeX, double sizeY, double sizeZ)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_box(sizeX, sizeY, sizeZ, out var shape));
        return shape;
    }
}
