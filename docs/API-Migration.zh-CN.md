# OCCT API 迁移进度

OCCT 是一个大型 C++ 建模内核，API 数量非常多，不能靠手写一次性完整迁移。OcctNet 的迁移策略是：

1. 先迁移稳定的 C ABI native shim。
2. 再在 C# 层提供安全、可释放、符合 .NET 风格的对象。
3. 每迁移一个模块，都补示例和运行时依赖说明。
4. 优先迁移建模工作流需要的最小闭环：几何、拓扑、建模算法、文件交换、渲染。

## 当前迁移状态

| OCCT 模块 | C++ 代表类型/API | C# API | 状态 | 说明 |
| --- | --- | --- | --- | --- |
| Foundation | `Standard_Version` | `OcctRuntime.NativeVersion` | 已完成 | 获取 native OCCT 版本 |
| Geometry gp | `gp_Pnt` | `OcctPoint3d` | 已完成 | 创建点、读写坐标、距离计算 |
| Topology | `TopoDS_Shape` | `OcctShape` | 已开始 | 托管 shape 句柄、释放、判空 |
| Modeling Prim | `BRepPrimAPI_MakeBox` | `OcctBox` | 已完成第一版 | 创建长方体 shape |
| Modeling Prim | `BRepPrimAPI_MakeCylinder` | `OcctCylinder` | 已完成第一版 | 创建圆柱 shape |
| Modeling Prim | `BRepPrimAPI_MakeSphere` | `OcctSphere` | 已完成第一版 | 创建球 shape |
| Transform | `BRepBuilderAPI_Transform`、`gp_Trsf` | `OcctShape.Translate` | 已完成第一版 | 平移 shape，返回新 shape |
| Bounding | `BRepBndLib::Add`、`Bnd_Box` | `OcctBoundingBox` | 已完成第一版 | 查询 shape 包围盒 |
| Mesh | `BRepMesh_IncrementalMesh`、`Poly_Triangulation` | `OcctMesh` | 已完成第一版 | shape 三角化，导出顶点和索引 |
| Curves | `Geom_Curve`、`GC_MakeSegment` | 未开始 | 待迁移 | 计划支持线段、圆、样条 |
| Surfaces | `Geom_Surface` | 未开始 | 待迁移 | 计划支持平面、圆柱、曲面 |
| BRep Builder | `BRepBuilderAPI_*` | 未开始 | 待迁移 | 计划支持 wire、face、solid |
| Boolean | `BRepAlgoAPI_*` | 未开始 | 待迁移 | 计划支持并、交、差 |
| Fillet/Chamfer | `BRepFilletAPI_*` | 未开始 | 待迁移 | 计划支持倒角/圆角 |
| STEP | `STEPControl_Reader/Writer` | 未开始 | 待迁移 | 计划导入/导出 STEP |
| IGES | `IGESControl_Reader/Writer` | 未开始 | 待迁移 | 计划导入/导出 IGES |
| STL | `StlAPI_Reader/Writer` | 未开始 | 待迁移 | 计划导入/导出 STL |
| Visualization | `V3d_View`、`AIS_InteractiveContext` | 设计中 | 设计中 | 见 Avalonia 渲染控件文档 |

## 当前已可用的 C# API

### Runtime

```csharp
Console.WriteLine(OcctRuntime.NativeVersion);
```

### 点

```csharp
using var a = OcctPoint3d.Origin();
using var b = new OcctPoint3d(3, 4, 0);

Console.WriteLine(a.DistanceTo(b)); // 5
```

### 长方体和包围盒

```csharp
using var box = new OcctBox(10, 20, 30);

Console.WriteLine(box.IsNull); // False

var bounds = box.BoundingBox;
Console.WriteLine($"{bounds.SizeX}, {bounds.SizeY}, {bounds.SizeZ}");
```

### 圆柱、球和平移

```csharp
using var cylinder = new OcctCylinder(2, 8);
using var sphere = new OcctSphere(5);
using var movedSphere = sphere.Translate(new OcctVector3d(10, 0, 0));

Console.WriteLine(cylinder.BoundingBox.SizeZ); // 8
Console.WriteLine(movedSphere.BoundingBox.MinX); // 5
```

### Shape 三角化

```csharp
using var box = new OcctBox(10, 20, 30);
var mesh = box.Triangulate(0.5);

Console.WriteLine(mesh.Vertices.Count);
Console.WriteLine(mesh.TriangleCount);
```

## Native 运行时依赖

当前 Windows x64 建模基础包需要：

```text
OcctNetNative.dll
TKernel.dll
TKMath.dll
TKG2d.dll
TKG3d.dll
TKBRep.dll
TKTopAlgo.dll
TKPrim.dll
TKMesh.dll
TKGeomBase.dll
TKGeomAlgo.dll
TKShHealing.dll
tbb12.dll
jemalloc.dll
msvcp140.dll
vcruntime140.dll
vcruntime140_1.dll
```

后续每接入一个 OCCT 模块，都要同步补充依赖 DLL。

## 迁移优先级

下一批建议迁移：

1. `gp_Vec`、`gp_Dir`、`gp_Ax1`、`gp_Ax2`。
2. `BRepBuilderAPI_MakeEdge`、`BRepBuilderAPI_MakeWire`、`BRepBuilderAPI_MakeFace`。
3. `BRepPrimAPI_MakeCylinder`、`BRepPrimAPI_MakeSphere`。
4. `BRepAlgoAPI_Cut/Fuse/Common`。
5. STEP/STL 导入导出。
6. OCCT Visualization 原生视口。
