using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctWire : OcctShape
{
    public OcctWire(params OcctEdge[] edges)
        : base(Create(edges))
    {
        Edges = edges.ToArray();
    }

    public IReadOnlyList<OcctEdge> Edges { get; }

    private static IntPtr Create(IReadOnlyCollection<OcctEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);
        if (edges.Count == 0)
        {
            throw new ArgumentException("Wire requires at least one edge.", nameof(edges));
        }

        NativeMethods.EnsureLoaded();

        var handles = edges.Select(edge =>
        {
            ArgumentNullException.ThrowIfNull(edge);
            return edge.DangerousGetHandle();
        }).ToArray();

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_make_wire(handles, handles.Length, out var shape));
        return shape;
    }
}
