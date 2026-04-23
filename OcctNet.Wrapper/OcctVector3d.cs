namespace OcctNet.Wrapper;

public readonly record struct OcctVector3d(double X, double Y, double Z)
{
    public static OcctVector3d Zero { get; } = new(0, 0, 0);

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
}
