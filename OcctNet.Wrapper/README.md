# OcctNet.Wrapper

`OcctNet.Wrapper` 是一个面向 .NET 的 Open CASCADE Technology（OCCT）封装库。它通过稳定的 native C ABI 桥接层调用 OCCT C++ API，让 C# 程序可以创建、变换、三角化、导入和导出 CAD 几何数据。

本包只包含 `OcctNet.Wrapper` 类库和 native runtime，不包含仓库中的 Avalonia 示例应用。

## 安装

```powershell
dotnet add package OcctNet.Wrapper
```

## 支持的运行时

NuGet 包会把 native 文件放在：

```text
runtimes/<rid>/native/
```

当前 CI 构建目标：

- `win-x64`
- `linux-x64`
- `osx-arm64`

默认情况下不需要手动配置 native 路径。如果你需要指定 native 目录，可以设置：

```powershell
$env:OCCTNET_NATIVE_DIR="C:\path\to\native"
```

也可以直接指定完整 native library 路径：

```powershell
$env:OCCTNET_NATIVE_LIBRARY="C:\path\to\OcctNetNative.dll"
```

## 快速开始

```csharp
using OcctNet.Wrapper;

Console.WriteLine(OcctRuntime.NativeVersion);

using var origin = OcctPoint3d.Origin();
using var point = new OcctPoint3d(3, 4, 0);

Console.WriteLine(origin.DistanceTo(point)); // 5
```

## 创建基础实体

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);
using var cylinder = new OcctCylinder(radius: 2, height: 8);
using var sphere = new OcctSphere(radius: 5);

Console.WriteLine(box.BoundingBox.SizeX);
Console.WriteLine(cylinder.BoundingBox.SizeZ);
Console.WriteLine(sphere.IsNull);
```

## 草图建模：Edge、Wire、Face、Extrude

可以先创建边，再组成线框，生成面，最后拉伸成实体：

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

## 平移和旋转成形

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 10, 10);
using var moved = box.Translate(new OcctVector3d(20, 0, 0));

using var edge = new OcctEdge(new(5, 0, 0), new(5, 0, 10));
using var revolved = edge.Revolve(OcctAxis1d.ZAxis);
```

## 布尔运算

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 10, 10);
using var sphere = new OcctSphere(6);

using var fused = box.Fuse(sphere);
using var cut = box.Cut(sphere);
using var common = box.Common(sphere);
```

对应关系：

- `Fuse`：并集
- `Cut`：差集
- `Common`：交集

## 三角化网格

可以把 shape 转成三角网格，用于自定义渲染或导出：

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);
var mesh = box.Triangulate(linearDeflection: 0.5);

Console.WriteLine(mesh.Vertices.Count);
Console.WriteLine(mesh.TriangleCount);
```

`TriangleIndices` 每三个索引表示一个三角形。

## STL、STEP、IGES 导入导出

```csharp
using OcctNet.Wrapper;

using var box = new OcctBox(10, 20, 30);

box.ExportStl("box.stl");
box.ExportStep("box.step");
box.ExportIges("box.igs");

using var stlShape = OcctShape.ImportStl("box.stl");
using var stepShape = OcctShape.ImportStep("box.step");
using var igesShape = OcctShape.ImportIges("box.igs");
```

建议：

- 渲染或简单网格交换优先用 STL。
- CAD 数据交换优先用 STEP。
- IGES 可用于兼容旧系统。

## 异常处理

OCCT native 调用失败时会转换成 `OcctException`。

```csharp
using OcctNet.Wrapper;

try
{
    using var box = new OcctBox(10, 20, 30);
    box.ExportStep("box.step");
}
catch (OcctException ex)
{
    Console.WriteLine(ex.StatusCode);
    Console.WriteLine(ex.Message);
}
catch (DllNotFoundException ex)
{
    Console.WriteLine("native runtime 或其依赖没有找到。");
    Console.WriteLine(ex.Message);
}
```

## 许可说明

`OcctNet.Wrapper` wrapper 代码采用 MIT License。

本包依赖 Open CASCADE Technology（OCCT）及其第三方依赖。MIT License 只适用于本项目 wrapper 代码，不会改变 OCCT 及其第三方依赖的许可证。发布、分发或商用前，请确认 OCCT 和相关依赖的许可证要求。

