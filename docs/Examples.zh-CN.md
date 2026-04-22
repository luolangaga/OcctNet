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
