namespace OcctNet.Wrapper;

public readonly record struct OcctAxis1d(OcctPointCoordinates Origin, OcctVector3d Direction)
{
    public static OcctAxis1d XAxis { get; } = new(new OcctPointCoordinates(0, 0, 0), new OcctVector3d(1, 0, 0));

    public static OcctAxis1d YAxis { get; } = new(new OcctPointCoordinates(0, 0, 0), new OcctVector3d(0, 1, 0));

    public static OcctAxis1d ZAxis { get; } = new(new OcctPointCoordinates(0, 0, 0), new OcctVector3d(0, 0, 1));
}
