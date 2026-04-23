# Native 桥接层说明

本文档解释 `native/` 目录和 `OcctNet.Wrapper/Native/` 目录的作用。普通使用者可以先看 `GettingStarted.zh-CN.md`。

## 为什么需要桥接层

OCCT 是 C++ 库，里面有大量 C++ 类、模板、异常和引用计数对象。C# 的 P/Invoke 更适合调用稳定的 C ABI 函数，不适合直接调用 C++ 类方法。

因此本项目采用：

```text
C# P/Invoke
  -> extern "C" 函数
  -> OCCT C++ 对象
```

这样可以隔离 C++ ABI 差异，并为 Windows 和 macOS 使用同一套 C# API。

## native C++ 文件

```text
native/include/OcctNetNative.h
native/src/OcctNetNative.cpp
native/CMakeLists.txt
```

`OcctNetNative.h` 导出 C ABI 函数，例如：

```cpp
OCCTNET_API int occtnet_get_version(char* buffer, int bufferLength);
OCCTNET_API int occtnet_point_create(double x, double y, double z, void** point);
OCCTNET_API int occtnet_point_destroy(void* point);
```

这些函数不能直接暴露 C++ 类。参数尽量使用：

- `int`
- `double`
- `char*`
- `void*`
- 简单结构体

## C# P/Invoke 文件

```text
OcctNet.Wrapper/Native/NativeMethods.cs
OcctNet.Wrapper/Native/OcctPointHandle.cs
OcctNet.Wrapper/Native/OcctStatus.cs
```

`NativeMethods.cs` 负责：

- 安装 native DLL resolver。
- 查找 `OcctNetNative.dll`。
- 声明 `[DllImport]` 函数。
- 把 native 状态码转换成 C# 异常。

`OcctPointHandle.cs` 使用 `SafeHandle` 释放 native 指针，避免忘记调用 native destroy 函数。

## native DLL 搜索顺序

wrapper 会尝试从以下位置查找 native library：

1. `OCCTNET_NATIVE_LIBRARY` 指定的完整路径。
2. `OCCTNET_NATIVE_DIR` 指定目录下的 native library。
3. 程序输出目录的 `runtimes/<rid>/native/`。
4. 程序输出目录。
5. wrapper assembly 目录的 `runtimes/<rid>/native/`。
6. wrapper assembly 目录。
7. 系统默认搜索路径。

Windows x64 对应 RID：

```text
win-x64
```

macOS Apple Silicon 对应 RID：

```text
osx-arm64
```

## 添加新的 OCCT API

推荐流程：

1. 在 `native/include/OcctNetNative.h` 增加一个 C 函数声明。
2. 在 `native/src/OcctNetNative.cpp` 实现这个函数。
3. 用 `try/catch` 捕获 `Standard_Failure`，返回状态码。
4. 在 `NativeMethods.cs` 添加 `[DllImport]`。
5. 在 `OcctNet.Wrapper` 中写一个对用户友好的 C# 类或方法。
6. 重新编译 `OcctNetNative.dll`。
7. 把新的 DLL 复制到 `OcctNet.Wrapper/runtimes/win-x64/native/`。

## 错误处理约定

native 函数返回 `int` 状态码：

```cpp
enum OcctNetStatus
{
    OCCTNET_OK = 0,
    OCCTNET_INVALID_ARGUMENT = 1,
    OCCTNET_OCCT_EXCEPTION = 2,
    OCCTNET_UNKNOWN_EXCEPTION = 3
};
```

C# 侧用 `NativeMethods.ThrowIfFailed(status)` 转换异常。

OCCT 抛出的 `Standard_Failure` 会被 native 层捕获，并通过 `occtnet_get_last_error` 返回错误消息。

## 当前链接的 OCCT 模块

当前 native shim 链接：

```text
TKernel
TKMath
TKG2d
TKG3d
TKBRep
TKTopAlgo
TKPrim
TKMesh
```

这支持 `gp_Pnt`、`TopoDS_Shape`、box/cylinder/sphere、shape 平移、包围盒查询和三角化网格导出。

如果后续要支持 BRep、STEP、IGES、可视化，需要在 `native/CMakeLists.txt` 中追加对应模块，并同步复制对应 DLL。

常见模块方向：

- 基础几何：`TKMath`
- 拓扑和形体：`TKBRep`、`TKTopAlgo`
- 建模算法：`TKPrim`、`TKBool`、`TKFillet`
- STEP/IGES：`TKDESTEP`、`TKDEIGES`
- 可视化：`TKV3d`、`TKOpenGl`

增加模块后，运行时依赖 DLL 会变多。建议每次新增模块后，用 `dumpbin /dependents` 检查依赖。
