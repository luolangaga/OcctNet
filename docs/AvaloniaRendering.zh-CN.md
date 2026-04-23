# Avalonia OCCT 渲染控件设计

目标：在 Avalonia 中提供一个可复用控件，让 C# 应用可以显示和交互 OCCT 模型。

## 当前状态

当前 `OcctNet` 示例应用已经有一个 Avalonia 自绘工作台：

```text
OcctNet/Controls/SketchViewport.cs
```

它用于演示交互和 wrapper 调用：

- 绘制等轴测网格。
- 画线、画矩形面。
- 推拉矩形面。
- 使用 `OcctPoint3d` 测距。
- 使用 `OcctBox` 创建 OCCT shape 并读取包围盒。

它还不是 OCCT 原生渲染视口。

## 为什么直接接 OCCT 渲染更复杂

OCCT 原生渲染通常涉及：

- `Aspect_DisplayConnection`
- `OpenGl_GraphicDriver`
- `V3d_Viewer`
- `V3d_View`
- `AIS_InteractiveContext`
- 平台窗口句柄
  - Windows: `HWND` + `WNT_Window`
  - macOS: Cocoa/Metal/OpenGL 相关 native handle

Avalonia 是跨平台 UI 框架，控件本身不直接等于 Win32 HWND。要优雅实现，需要把“窗口宿主”和“OCCT Viewer 生命周期”拆开。

## 推荐架构

```text
OcctNet.Wrapper
  OcctShape
  OcctBox
  后续：OcctViewer、OcctView、OcctInteractiveContext

OcctNet.Avalonia
  OcctViewControl
  WindowsOcctNativeHost
  MacOcctNativeHost

OcctNet 示例应用
  使用 OcctViewControl
```

## 控件职责

理想控件名：

```csharp
public sealed class OcctViewControl : NativeControlHost
```

它应该负责：

- 创建平台 native child window。
- 把 native window handle 传给 `OcctViewer`。
- resize 时调用 `V3d_View::MustBeResized()`。
- paint/request render 时调用 `V3d_View::Redraw()`。
- 鼠标事件转发给 `AIS_InteractiveContext`。
- dispose 时按顺序释放 view、viewer、driver、window。

## Native API 草案

后续 native shim 可以增加：

```cpp
int occtnet_viewer_create(void* nativeWindowHandle, int width, int height, void** viewer);
int occtnet_viewer_destroy(void* viewer);
int occtnet_viewer_resize(void* viewer, int width, int height);
int occtnet_viewer_redraw(void* viewer);
int occtnet_viewer_display_shape(void* viewer, void* shape);
int occtnet_viewer_fit_all(void* viewer);
```

C# 层对应：

```csharp
public sealed class OcctViewer : IDisposable
{
    public void Resize(int width, int height);
    public void Redraw();
    public void Display(OcctShape shape);
    public void FitAll();
}
```

Avalonia 控件对应：

```csharp
public sealed class OcctViewControl : NativeControlHost
{
    public OcctViewer Viewer { get; }
}
```

## Windows 实现路线

Windows 下优先实现，因为当前仓库已经验证 Windows x64 native runtime。

步骤：

1. 在 Avalonia 控件中创建 Win32 child HWND。
2. native shim 中使用 `WNT_Window` 包装 HWND。
3. 创建 `OpenGl_GraphicDriver`、`V3d_Viewer`、`V3d_View`。
4. 创建 `AIS_InteractiveContext`。
5. `OcctShape` 转成 `AIS_Shape` 并 display。
6. resize/redraw/mouse event 接通。

需要新增 OCCT DLL：

```text
TKV3d.dll
TKOpenGl.dll
TKService.dll
TKMesh.dll
TKHLR.dll
```

实际依赖还需要用 `dumpbin /dependents` 继续确认。

## macOS 实现路线

macOS 的窗口句柄和图形后端与 Windows 不同，应单独实现：

```text
MacOcctNativeHost
```

建议等 Windows 版本稳定后再迁移 macOS。

## 短期替代方案

在 OCCT 原生视口完成前，已经先迁移：

```text
BRepMesh_IncrementalMesh
```

现在可以通过 `OcctShape.Triangulate()` 把 `TopoDS_Shape` 三角化，再用 Avalonia/Skia 自绘网格。这不是 OCCT 原生渲染，但跨平台更简单，也适合做模型预览。
