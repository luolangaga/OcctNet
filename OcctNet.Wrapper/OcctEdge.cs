using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctEdge : OcctShape
{
    public OcctEdge(OcctPointCoordinates start, OcctPointCoordinates end)
        : base(Create(start, end))
    {
        Start = start;
        End = end;
    }

    public OcctPointCoordinates Start { get; }

    public OcctPointCoordinates End { get; }

    private static IntPtr Create(OcctPointCoordinates start, OcctPointCoordinates end)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_edge(
            start.X,
            start.Y,
            start.Z,
            end.X,
            end.Y,
            end.Z,
            out var shape));

        return shape;
    }
}
