using OcctNet.Wrapper.Native;

namespace OcctNet.Wrapper;

public sealed class OcctPoint3d : IDisposable
{
    private readonly OcctPointHandle handle;
    private bool disposed;

    public OcctPoint3d(double x, double y, double z)
    {
        NativeMethods.EnsureLoaded();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_point_create(x, y, z, out var point));
        handle = new OcctPointHandle(point);
    }

    public double X
    {
        get
        {
            var coordinates = Coordinates;
            return coordinates.X;
        }
        set
        {
            var coordinates = Coordinates;
            SetCoordinates(value, coordinates.Y, coordinates.Z);
        }
    }

    public double Y
    {
        get
        {
            var coordinates = Coordinates;
            return coordinates.Y;
        }
        set
        {
            var coordinates = Coordinates;
            SetCoordinates(coordinates.X, value, coordinates.Z);
        }
    }

    public double Z
    {
        get
        {
            var coordinates = Coordinates;
            return coordinates.Z;
        }
        set
        {
            var coordinates = Coordinates;
            SetCoordinates(coordinates.X, coordinates.Y, value);
        }
    }

    public OcctPointCoordinates Coordinates
    {
        get
        {
            ThrowIfDisposed();
            NativeMethods.ThrowIfFailed(NativeMethods.occtnet_point_get_coordinates(handle.DangerousGetHandle(), out var x, out var y, out var z));
            return new OcctPointCoordinates(x, y, z);
        }
    }

    public static OcctPoint3d Origin()
    {
        return new OcctPoint3d(0, 0, 0);
    }

    public void SetCoordinates(double x, double y, double z)
    {
        ThrowIfDisposed();
        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_point_set_coordinates(handle.DangerousGetHandle(), x, y, z));
    }

    public double DistanceTo(OcctPoint3d other)
    {
        ArgumentNullException.ThrowIfNull(other);
        ThrowIfDisposed();
        other.ThrowIfDisposed();

        NativeMethods.ThrowIfFailed(NativeMethods.occtnet_point_distance(
            handle.DangerousGetHandle(),
            other.handle.DangerousGetHandle(),
            out var distance));

        return distance;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        handle.Dispose();
        disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
