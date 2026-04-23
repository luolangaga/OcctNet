using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctMesh
{
    public OcctMesh(IReadOnlyList<OcctMeshVertex> vertices, IReadOnlyList<int> triangleIndices)
    {
        Vertices = vertices;
        TriangleIndices = triangleIndices;
    }

    public IReadOnlyList<OcctMeshVertex> Vertices { get; }

    public IReadOnlyList<int> TriangleIndices { get; }

    public int TriangleCount => TriangleIndices.Count / 3;

    public static OcctMesh FromShape(OcctShape shape, double linearDeflection = 0.1, double angularDeflection = 0.5)
    {
        ArgumentNullException.ThrowIfNull(shape);

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_mesh_create(
            shape.DangerousGetHandle(),
            linearDeflection,
            angularDeflection,
            out var meshHandle));

        using var mesh = new OcctMeshHandle(meshHandle);
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_mesh_get_counts(mesh.DangerousGetHandle(), out var vertexCount, out var triangleIndexCount));

        var rawVertices = new double[vertexCount * 3];
        var triangleIndices = new int[triangleIndexCount];

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_mesh_copy(
            mesh.DangerousGetHandle(),
            rawVertices,
            rawVertices.Length,
            triangleIndices,
            triangleIndices.Length));

        var vertices = new OcctMeshVertex[vertexCount];
        for (var i = 0; i < vertexCount; i++)
        {
            var offset = i * 3;
            vertices[i] = new OcctMeshVertex(rawVertices[offset], rawVertices[offset + 1], rawVertices[offset + 2]);
        }

        return new OcctMesh(vertices, triangleIndices);
    }
}
