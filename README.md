# OcctNet

OcctNet 是一个用于连接 C# 和 Open CASCADE Technology（OCCT）C++ 实现的跨平台 Wrapper 项目。

当前仓库已经包含 Windows x64 可用的 native 运行时文件，可以直接从 C# 调用 OCCT 的基础几何能力。项目采用 `P/Invoke + C ABI native shim`，而不是 C++/CLI，因此后续可以同时支持 Windows 和 macOS。

## 当前状态

- 已接入 OCCT 官方预编译发行版，不需要从源码编译 OCCT。
- 已编译 Windows x64 桥接库：`OcctNetNative.dll`。
- 已实现 C# 基础 API：
  - 获取 OCCT native 版本。
  - 创建三维点。
  - 读取和修改点坐标。
  - 计算两个点之间的距离。
  - 创建 OCCT `TopoDS_Shape` 形状句柄。
  - 使用 `BRepPrimAPI_MakeBox` 创建长方体。
  - 使用 `BRepPrimAPI_MakeCylinder` 创建圆柱。
  - 使用 `BRepPrimAPI_MakeSphere` 创建球。
  - 使用 `BRepBuilderAPI_Transform` 平移 shape。
  - 使用 `BRepBndLib` 查询 shape 包围盒。
  - 使用 `BRepMesh_IncrementalMesh` 把 shape 转成三角网格。
- 已通过加载测试：

```text
OCCT version: 7.9.2
Distance test: 5
```

## 项目结构

```text
OcctNet/
  OcctNet/                         Avalonia 示例应用
  OcctNet.Wrapper/                 C# wrapper 类库
    Native/                        P/Invoke 和 native library 加载逻辑
    runtimes/win-x64/native/       Windows x64 native DLL
  native/                          C++ 桥接层源码和 CMake 配置
  docs/                            中文文档
  external/                        本地下载/解压的 OCCT 官方预编译包
```

核心项目说明：

- `OcctNet.Wrapper`：给 C# 业务代码使用的类库。
- `native`：薄 C++ 桥接层，负责调用 OCCT C++ API，并导出稳定的 C 函数。
- `OcctNet`：Avalonia 示例程序，用于验证 wrapper 能否被桌面应用加载。

## 快速开始

如果你只是想在 C# 中使用当前已经封装好的功能，先看：

- [新手使用文档](docs/GettingStarted.zh-CN.md)
- [示例文档](docs/Examples.zh-CN.md)

最小示例：

```csharp
using OcctNet.Wrapper;

Console.WriteLine(OcctRuntime.NativeVersion);

using var a = OcctPoint3d.Origin();
using var b = new OcctPoint3d(3, 4, 0);

Console.WriteLine(a.DistanceTo(b)); // 5
```

## Windows Native 运行时

当前 Windows x64 最小运行时文件位于：

```text
OcctNet.Wrapper/runtimes/win-x64/native/
```

包含：

- `OcctNetNative.dll`
- `TKernel.dll`
- `TKMath.dll`
- `TKG2d.dll`
- `TKG3d.dll`
- `TKBRep.dll`
- `TKTopAlgo.dll`
- `TKPrim.dll`
- `TKMesh.dll`
- `TKGeomBase.dll`
- `TKGeomAlgo.dll`
- `TKShHealing.dll`
- `tbb12.dll`
- `jemalloc.dll`
- `msvcp140.dll`
- `vcruntime140.dll`
- `vcruntime140_1.dll`

这些 DLL 会在引用 `OcctNet.Wrapper` 的 .NET 项目构建时复制到输出目录的 `runtimes/win-x64/native/`。

## 重新编译桥接层

通常不需要重新编译。只有修改了 `native/src/OcctNetNative.cpp` 或 `native/include/OcctNetNative.h` 后，才需要重新生成 `OcctNetNative.dll`。

Windows 示例：

```powershell
cmake -S native -B native/build/win-x64-vs2026 -G "Visual Studio 18 2026" -A x64 -DOpenCASCADE_DIR=external/occt-vc14-64-combined/occt-vc14-64/cmake
cmake --build native/build/win-x64-vs2026 --config Release
```

然后复制：

```text
native/build/win-x64-vs2026/Release/OcctNetNative.dll
```

到：

```text
OcctNet.Wrapper/runtimes/win-x64/native/
```

macOS 方向：

```bash
brew install opencascade
cmake -S native -B native/build/osx-arm64 -DCMAKE_BUILD_TYPE=Release
cmake --build native/build/osx-arm64
```

macOS 需要生成并放置 `libOcctNetNative.dylib`，以及对应的 OCCT 动态库依赖。当前仓库已验证 Windows x64；macOS 支持路径已预留，但还需要在 macOS 机器上实际构建和验证。

## 文档

- [新手使用文档](docs/GettingStarted.zh-CN.md)
- [示例文档](docs/Examples.zh-CN.md)
- [Native 桥接层说明](docs/NativeBridge.zh-CN.md)
- [API 迁移进度](docs/API-Migration.zh-CN.md)
- [Avalonia OCCT 渲染控件设计](docs/AvaloniaRendering.zh-CN.md)

## 许可证和依赖

本项目依赖 Open CASCADE Technology（OCCT）。发布或商用前，请确认 OCCT 及其 third-party 依赖的许可证要求。
