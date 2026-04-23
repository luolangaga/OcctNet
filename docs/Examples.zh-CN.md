# OcctNet 示例文档

本文档展示当前已经封装好的 C# API 用法。

命名空间：

```csharp
using OcctNet.Wrapper;
```

## 示例 1：获取 OCCT native 版本

适合程序启动时做自检。

```csharp
using OcctNet.Wrapper;

if (OcctRuntime.TryGetNativeVersion(out var version, out var error))
{
    Console.WriteLine($"OCCT native 已加载，版本: {version}");
}
else
{
    Console.WriteLine($"OCCT native 加载失败: {error}");
}
```

预期输出：

```text
OCCT native 已加载，版本: 7.9.2
```

如果你希望加载失败时直接抛异常：

```csharp
Console.WriteLine(OcctRuntime.NativeVersion);
```

## 示例 2：创建三维点

```csharp
using OcctNet.Wrapper;

using var point = new OcctPoint3d(1, 2, 3);

Console.WriteLine(point.X);
Console.WriteLine(point.Y);
Console.WriteLine(point.Z);
```

输出：

```text
1
2
3
```

## 示例 3：读取坐标

```csharp
using OcctNet.Wrapper;

using var point = new OcctPoint3d(10, 20, 30);

var coordinates = point.Coordinates;

Console.WriteLine($"X={coordinates.X}, Y={coordinates.Y}, Z={coordinates.Z}");
```

输出：

```text
X=10, Y=20, Z=30
```

## 示例 4：修改坐标

```csharp
using OcctNet.Wrapper;

using var point = new OcctPoint3d(1, 2, 3);

point.SetCoordinates(4, 5, 6);

Console.WriteLine(point.Coordinates);
```

也可以单独修改某一个轴：

```csharp
point.X = 100;
point.Y = 200;
point.Z = 300;
```

## 示例 5：创建原点

```csharp
using OcctNet.Wrapper;

using var origin = OcctPoint3d.Origin();

Console.WriteLine(origin.Coordinates);
```

原点坐标是：

```text
X=0, Y=0, Z=0
```

## 示例 6：计算两个点的距离

```csharp
using OcctNet.Wrapper;

using var a = new OcctPoint3d(0, 0, 0);
using var b = new OcctPoint3d(3, 4, 0);

var distance = a.DistanceTo(b);

Console.WriteLine(distance);
```

输出：

```text
5
```

## 示例 7：在方法中封装计算

```csharp
using OcctNet.Wrapper;

static double Distance(double ax, double ay, double az, double bx, double by, double bz)
{
    using var a = new OcctPoint3d(ax, ay, az);
    using var b = new OcctPoint3d(bx, by, bz);

    return a.DistanceTo(b);
}

Console.WriteLine(Distance(0, 0, 0, 3, 4, 0));
```

输出：

```text
5
```

## 示例 8：处理 native 异常

当前点操作通常不会失败，但后续封装复杂建模 API 时，OCCT C++ 异常会转换成 `OcctException`。

```csharp
using OcctNet.Wrapper;

try
{
    Console.WriteLine(OcctRuntime.NativeVersion);
}
catch (OcctException ex)
{
    Console.WriteLine($"OCCT 调用失败，状态码: {ex.StatusCode}");
    Console.WriteLine(ex.Message);
}
catch (DllNotFoundException ex)
{
    Console.WriteLine("native DLL 或其依赖没有找到。");
    Console.WriteLine(ex.Message);
}
```

## 示例 9：控制 native DLL 搜索位置

默认会从程序输出目录和 `runtimes/<rid>/native/` 查找 native DLL。

如果你想指定 native 目录，可以设置环境变量：

```powershell
$env:OCCTNET_NATIVE_DIR="C:\path\to\native"
dotnet run
```

如果你想指定完整 DLL 路径：

```powershell
$env:OCCTNET_NATIVE_LIBRARY="C:\path\to\OcctNetNative.dll"
dotnet run
```

一般项目内使用不需要设置这些变量。

## 示例 10：控制台完整示例

创建控制台项目：

```powershell
dotnet new console -n OcctNetConsoleDemo
dotnet add OcctNetConsoleDemo/OcctNetConsoleDemo.csproj reference OcctNet.Wrapper/OcctNet.Wrapper.csproj
```

`Program.cs`：

```csharp
using OcctNet.Wrapper;

if (!OcctRuntime.TryGetNativeVersion(out var version, out var error))
{
    Console.WriteLine($"OCCT 加载失败: {error}");
    return;
}

Console.WriteLine($"OCCT version: {version}");

using var origin = OcctPoint3d.Origin();
using var point = new OcctPoint3d(3, 4, 0);

Console.WriteLine($"point = {point.Coordinates}");
Console.WriteLine($"distance = {origin.DistanceTo(point)}");
```

运行：

```powershell
dotnet run --project OcctNetConsoleDemo/OcctNetConsoleDemo.csproj
```

预期输出：

```text
OCCT version: 7.9.2
point = OcctPointCoordinates { X = 3, Y = 4, Z = 0 }
distance = 5
```

## 示例 11：创建 OCCT 长方体

`OcctBox` 会在 native 层调用 OCCT 的 `BRepPrimAPI_MakeBox`，得到一个 `TopoDS_Shape`。

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);

Console.WriteLine(box.IsNull);
```

输出：

```text
False
```

## 示例 12：读取 Shape 包围盒

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);
var bounds = box.BoundingBox;

Console.WriteLine($"X: {bounds.MinX} -> {bounds.MaxX}");
Console.WriteLine($"Y: {bounds.MinY} -> {bounds.MaxY}");
Console.WriteLine($"Z: {bounds.MinZ} -> {bounds.MaxZ}");
Console.WriteLine($"Size: {bounds.SizeX}, {bounds.SizeY}, {bounds.SizeZ}");
```

预期尺寸：

```text
Size: 10, 20, 30
```

## 示例 13：创建圆柱和球

```csharp
using OcctNet.Wrapper;

using var cylinder = new OcctCylinder(radius: 2, height: 8);
using var sphere = new OcctSphere(radius: 5);

Console.WriteLine(cylinder.BoundingBox.SizeZ);
Console.WriteLine(sphere.BoundingBox.SizeX);
```

预期输出：

```text
8
10
```

## 示例 14：平移 Shape

`Translate` 不会修改原 shape，而是返回一个新的 `OcctShape`。

```csharp
using OcctNet.Wrapper;

using var sphere = new OcctSphere(5);
using var moved = sphere.Translate(new OcctVector3d(10, 0, 0));

Console.WriteLine(moved.BoundingBox.MinX);
```

预期输出大约是：

```text
5
```

## 示例 15：把 Shape 转成三角网格

这一步是后续 Avalonia 自己实现 3D 渲染的关键。OCCT 负责几何和三角化，Avalonia 负责显示。

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);
var mesh = box.Triangulate(linearDeflection: 0.5);

Console.WriteLine($"Vertices: {mesh.Vertices.Count}");
Console.WriteLine($"Triangles: {mesh.TriangleCount}");

foreach (var vertex in mesh.Vertices.Take(3))
{
    Console.WriteLine($"{vertex.X}, {vertex.Y}, {vertex.Z}");
}
```

`TriangleIndices` 每三个整数表示一个三角形：

```csharp
for (var i = 0; i < mesh.TriangleIndices.Count; i += 3)
{
    var a = mesh.TriangleIndices[i];
    var b = mesh.TriangleIndices[i + 1];
    var c = mesh.TriangleIndices[i + 2];

    Console.WriteLine($"{a}, {b}, {c}");
}
```

## 示例 16：Edge、Wire、Face 和拉伸

```csharp
using OcctNet.Wrapper;

using var e1 = new OcctEdge(new(0, 0, 0), new(10, 0, 0));
using var e2 = new OcctEdge(new(10, 0, 0), new(10, 10, 0));
using var e3 = new OcctEdge(new(10, 10, 0), new(0, 10, 0));
using var e4 = new OcctEdge(new(0, 10, 0), new(0, 0, 0));

using var wire = new OcctWire(e1, e2, e3, e4);
using var face = new OcctFace(wire);
using var solid = face.Extrude(new OcctVector3d(0, 0, 5));

Console.WriteLine(solid.BoundingBox.SizeZ);
```

## 示例 17：布尔运算

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 10, 10);
using var sphere = new OcctSphere(6);

using var fused = box.Fuse(sphere);
using var cut = box.Cut(sphere);
using var common = box.Common(sphere);

Console.WriteLine(fused.IsNull);
Console.WriteLine(cut.IsNull);
Console.WriteLine(common.IsNull);
```

## 示例 18：STL、STEP、IGES 导入导出

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);

box.ExportStl("box.stl");
box.ExportStep("box.step");
box.ExportIges("box.igs");

using var stlShape = OcctShape.ImportStl("box.stl");
using var stepShape = OcctShape.ImportStep("box.step");
using var igesShape = OcctShape.ImportIges("box.igs");

Console.WriteLine(stlShape.IsNull);
Console.WriteLine(stepShape.IsNull);
Console.WriteLine(igesShape.IsNull);
```
