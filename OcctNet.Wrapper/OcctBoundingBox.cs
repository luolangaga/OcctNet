namespace OcctNet.Wrapper;

public readonly record struct OcctBoundingBox(
    double MinX,
    double MinY,
    double MinZ,
    double MaxX,
    double MaxY,
    double MaxZ)
{
    public double SizeX => MaxX - MinX;

    public double SizeY => MaxY - MinY;

    public double SizeZ => MaxZ - MinZ;
}
