namespace OcctNet.Models;

public readonly record struct SketchPoint(double X, double Y, double Z)
{
    public static SketchPoint Origin { get; } = new(0, 0, 0);

    public SketchPoint WithZ(double z)
    {
        return new SketchPoint(X, Y, z);
    }
}
