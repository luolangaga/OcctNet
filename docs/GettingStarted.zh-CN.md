# OcctNet 新手使用文档

这份文档面向第一次使用本项目的人，目标是让你知道：

- 这个项目解决什么问题。
- 应该引用哪个 C# 项目。
- native DLL 应该放在哪里。
- 如何验证 wrapper 是否正常工作。

## 1. OcctNet 是什么

OCCT 是一个 C++ 几何建模内核。C# 不能直接调用复杂的 C++ 类，所以本项目分成两层：

```text
C# 业务代码
  -> OcctNet.Wrapper             C# 类库
  -> OcctNetNative.dll           C ABI 桥接层
  -> OCCT 官方 C++ DLL           例如 TKernel.dll、TKMath.dll
```

你平时写 C# 代码时，只需要使用 `OcctNet.Wrapper`。

`OcctNetNative.dll` 是本项目自己的桥接 DLL。它很薄，只负责把 C# 调用转发给 OCCT C++ API。

## 2. 环境要求

当前已验证环境：

- Windows x64
- .NET 10
- OCCT 7.9.2 官方预编译包
- Visual Studio 2026 C++ 工具链，仅在重新编译 native 桥接层时需要

如果只是使用当前仓库中已经编译好的 `OcctNetNative.dll`，通常不需要安装 C++ 工具链。

## 3. 运行时 DLL 在哪里

Windows x64 native DLL 已放在：

```text
OcctNet.Wrapper/runtimes/win-x64/native/
```

里面至少需要这些文件：

```text
OcctNetNative.dll
TKernel.dll
TKMath.dll
tbb12.dll
jemalloc.dll
msvcp140.dll
vcruntime140.dll
vcruntime140_1.dll
```

如果缺少其中某些 DLL，运行时可能会出现：

```text
System.DllNotFoundException
Unable to load DLL 'OcctNetNative' or one of its dependencies
```

这个错误不一定表示 `OcctNetNative.dll` 本身不存在，也可能是它依赖的 OCCT DLL 或 VC runtime DLL 不存在。

## 4. 在自己的 C# 项目中引用

如果你的项目和本仓库在同一个 solution 里，可以添加项目引用：

```powershell
dotnet add YourApp/YourApp.csproj reference OcctNet.Wrapper/OcctNet.Wrapper.csproj
```

然后在代码中：

```csharp
using OcctNet.Wrapper;
```

## 5. 验证 native 是否能加载

推荐先调用：

```csharp
if (OcctRuntime.TryGetNativeVersion(out var version, out var error))
{
    Console.WriteLine($"OCCT version: {version}");
}
else
{
    Console.WriteLine($"OCCT native 加载失败: {error}");
}
```

成功时应该输出类似：

```text
OCCT version: 7.9.2
```

如果你希望失败时直接抛异常，也可以使用：

```csharp
Console.WriteLine(OcctRuntime.NativeVersion);
```

## 6. 第一个几何示例

```csharp
using OcctNet.Wrapper;

using var origin = OcctPoint3d.Origin();
using var point = new OcctPoint3d(3, 4, 0);

var distance = origin.DistanceTo(point);

Console.WriteLine(distance); // 5
```

这里的 `OcctPoint3d` 对应 native 侧的 OCCT `gp_Pnt`。

因为它持有 native 内存，所以要用 `using` 或手动调用 `Dispose()`。

## 7. 为什么要 Dispose

`OcctPoint3d` 内部持有一个 native 指针。C# 的 GC 不知道 OCCT C++ 对象占用了什么资源，所以用完后应该释放：

```csharp
using var point = new OcctPoint3d(1, 2, 3);
```

或者：

```csharp
var point = new OcctPoint3d(1, 2, 3);
point.Dispose();
```

推荐使用 `using var`。

## 8. 常见问题

### DllNotFoundException

常见原因：

- `OcctNetNative.dll` 没有复制到输出目录。
- `TKernel.dll`、`TKMath.dll`、`tbb12.dll` 或 `jemalloc.dll` 缺失。
- 程序是 x86，但 native DLL 是 x64。

处理方式：

- 使用 x64 运行。
- 确认 `runtimes/win-x64/native/` 目录完整。
- 重新执行 `dotnet build`。

### 文件被 .NET Host 锁定

如果构建时报：

```text
The process cannot access the file because it is being used by another process
```

说明程序还在运行。关闭正在运行的 app 后重新构建。

### 我需要重新编译 OCCT 吗

不需要。当前 Windows 方案使用 OCCT 官方预编译包。

只有改了本项目的 C++ 桥接层，才需要重新编译 `OcctNetNative.dll`。
