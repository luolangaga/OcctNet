namespace OcctNet.Models;

public sealed class SketchFace
{
    public SketchFace(SketchPoint firstCorner, SketchPoint oppositeCorner)
    {
        FirstCorner = firstCorner;
        OppositeCorner = oppositeCorner;
    }

    public SketchPoint FirstCorner { get; }

    public SketchPoint OppositeCorner { get; }

    public double Height { get; private set; }

    public double SizeX => Math.Abs(OppositeCorner.X - FirstCorner.X);

    public double SizeY => Math.Abs(OppositeCorner.Y - FirstCorner.Y);

    public IReadOnlyList<SketchPoint> BaseCorners
    {
        get
        {
            var z = FirstCorner.Z;
            return
            [
                new SketchPoint(FirstCorner.X, FirstCorner.Y, z),
                new SketchPoint(OppositeCorner.X, FirstCorner.Y, z),
                new SketchPoint(OppositeCorner.X, OppositeCorner.Y, z),
                new SketchPoint(FirstCorner.X, OppositeCorner.Y, z)
            ];
        }
    }

    public IReadOnlyList<SketchPoint> TopCorners
    {
        get
        {
            return BaseCorners.Select(point => point.WithZ(point.Z + Height)).ToArray();
        }
    }

    public bool ContainsGroundPoint(SketchPoint point)
    {
        var minX = Math.Min(FirstCorner.X, OppositeCorner.X);
        var maxX = Math.Max(FirstCorner.X, OppositeCorner.X);
        var minY = Math.Min(FirstCorner.Y, OppositeCorner.Y);
        var maxY = Math.Max(FirstCorner.Y, OppositeCorner.Y);

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
    }

    public void PushPull(double delta)
    {
        Height = Math.Max(0, Height + delta);
    }
}
