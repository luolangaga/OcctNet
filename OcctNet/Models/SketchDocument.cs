using OcctNet.Wrapper;

namespace OcctNet.Models;

public sealed class SketchDocument
{
    private readonly List<SketchEdge> edges = [];
    private readonly List<SketchFace> faces = [];

    public IReadOnlyList<SketchEdge> Edges => edges;

    public IReadOnlyList<SketchFace> Faces => faces;

    public int EntityCount => edges.Count + faces.Count;

    public event EventHandler? Changed;

    public void Clear()
    {
        edges.Clear();
        faces.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void AddLine(SketchPoint start, SketchPoint end)
    {
        edges.Add(new SketchEdge(start, end));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public SketchFace AddRectangle(SketchPoint firstCorner, SketchPoint oppositeCorner)
    {
        var face = new SketchFace(firstCorner, oppositeCorner);
        faces.Add(face);
        Changed?.Invoke(this, EventArgs.Empty);
        return face;
    }

    public void PushPull(SketchFace face, double delta)
    {
        face.PushPull(delta);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public static double Distance(SketchPoint start, SketchPoint end)
    {
        using var nativeStart = new OcctPoint3d(start.X, start.Y, start.Z);
        using var nativeEnd = new OcctPoint3d(end.X, end.Y, end.Z);

        return nativeStart.DistanceTo(nativeEnd);
    }
}
