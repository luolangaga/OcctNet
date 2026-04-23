using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public class OcctShape : IDisposable
{
    private readonly OcctShapeHandle handle;
    private bool disposed;

    internal OcctShape(IntPtr nativeHandle)
    {
        NativeMethods.EnsureLoaded();
        handle = new OcctShapeHandle(nativeHandle);
    }

    public bool IsNull
    {
        get
        {
            ThrowIfDisposed();
            NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_is_null(handle.DangerousGetHandle(), out var isNull));
            return isNull != 0;
        }
    }

    public OcctBoundingBox BoundingBox
    {
        get
        {
            ThrowIfDisposed();
            NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_get_bounding_box(
                handle.DangerousGetHandle(),
                out var minX,
                out var minY,
                out var minZ,
                out var maxX,
                out var maxY,
                out var maxZ));

            return new OcctBoundingBox(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }

    internal IntPtr DangerousGetHandle()
    {
        ThrowIfDisposed();
        return handle.DangerousGetHandle();
    }

    public OcctShape Translate(OcctVector3d vector)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_translate(
            handle.DangerousGetHandle(),
            vector.X,
            vector.Y,
            vector.Z,
            out var translatedShape));

        return new OcctShape(translatedShape);
    }

    public OcctShape Extrude(OcctVector3d vector)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_extrude(
            handle.DangerousGetHandle(),
            vector.X,
            vector.Y,
            vector.Z,
            out var extrudedShape));

        return new OcctShape(extrudedShape);
    }

    public OcctShape Revolve(OcctAxis1d axis, double angleRadians = Math.Tau)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_revolve(
            handle.DangerousGetHandle(),
            axis.Origin.X,
            axis.Origin.Y,
            axis.Origin.Z,
            axis.Direction.X,
            axis.Direction.Y,
            axis.Direction.Z,
            angleRadians,
            out var revolvedShape));

        return new OcctShape(revolvedShape);
    }

    public OcctShape Fuse(OcctShape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_fuse(
            handle.DangerousGetHandle(),
            other.handle.DangerousGetHandle(),
            out var resultShape));

        return new OcctShape(resultShape);
    }

    public OcctShape Cut(OcctShape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_cut(
            handle.DangerousGetHandle(),
            other.handle.DangerousGetHandle(),
            out var resultShape));

        return new OcctShape(resultShape);
    }

    public OcctShape Common(OcctShape other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_common(
            handle.DangerousGetHandle(),
            other.handle.DangerousGetHandle(),
            out var resultShape));

        return new OcctShape(resultShape);
    }

    public void ExportStl(string filePath, bool ascii = false)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_export_stl(
            handle.DangerousGetHandle(),
            NativeMethods.EncodePath(filePath),
            ascii ? 1 : 0));
    }

    public void ExportStep(string filePath)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_export_step(
            handle.DangerousGetHandle(),
            NativeMethods.EncodePath(filePath)));
    }

    public void ExportIges(string filePath)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_export_iges(
            handle.DangerousGetHandle(),
            NativeMethods.EncodePath(filePath)));
    }

    public static OcctShape ImportStl(string filePath)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_import_stl(NativeMethods.EncodePath(filePath), out var shape));
        return new OcctShape(shape);
    }

    public static OcctShape ImportStep(string filePath)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_import_step(NativeMethods.EncodePath(filePath), out var shape));
        return new OcctShape(shape);
    }

    public static OcctShape ImportIges(string filePath)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_shape_import_iges(NativeMethods.EncodePath(filePath), out var shape));
        return new OcctShape(shape);
    }

    public OcctMesh Triangulate(double linearDeflection = 0.1, double angularDeflection = 0.5)
    {
        ThrowIfDisposed();
        return OcctMesh.FromShape(this, linearDeflection, angularDeflection);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        handle.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
