#include "OcctNetNative.h"

#include <Standard_Failure.hxx>
#include <Standard_Version.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepBndLib.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRep_Tool.hxx>
#include <Bnd_Box.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <Poly_Triangulation.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_StepModelType.hxx>
#include <STEPControl_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <StlAPI_Writer.hxx>
#include <TopExp_Explorer.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS_Shape.hxx>
#include <TopoDS.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <gp_Ax1.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>

#include <algorithm>
#include <cstring>
#include <exception>
#include <stdexcept>
#include <string>
#include <vector>

namespace
{
thread_local std::string lastError;

struct MeshData
{
    std::vector<double> vertices;
    std::vector<int> triangleIndices;
};

void copy_string(const std::string& value, char* buffer, int bufferLength)
{
    if (buffer == nullptr || bufferLength <= 0)
    {
        return;
    }

    const auto writableLength = static_cast<std::size_t>(bufferLength - 1);
    const auto bytesToCopy = std::min(value.size(), writableLength);
    std::memcpy(buffer, value.data(), bytesToCopy);
    buffer[bytesToCopy] = '\0';
}

int invalid_argument(const char* message)
{
    lastError = message;
    return OCCTNET_INVALID_ARGUMENT;
}

const TopoDS_Shape* require_shape(void* shape, const char* message)
{
    if (shape == nullptr)
    {
        throw std::invalid_argument(message);
    }

    return static_cast<const TopoDS_Shape*>(shape);
}

const char* require_path(const char* filePath)
{
    if (filePath == nullptr || filePath[0] == '\0')
    {
        throw std::invalid_argument("file path is null or empty.");
    }

    return filePath;
}

void ensure_done(IFSelect_ReturnStatus status, const char* operation)
{
    if (status != IFSelect_RetDone)
    {
        throw std::runtime_error(std::string(operation) + " failed.");
    }
}

template <typename TAction>
int invoke(TAction action)
{
    try
    {
        lastError.clear();
        action();
        return OCCTNET_OK;
    }
    catch (const Standard_Failure& failure)
    {
        lastError = failure.GetMessageString();
        return OCCTNET_OCCT_EXCEPTION;
    }
    catch (const std::exception& ex)
    {
        lastError = ex.what();
        return OCCTNET_UNKNOWN_EXCEPTION;
    }
    catch (...)
    {
        lastError = "Unknown native exception.";
        return OCCTNET_UNKNOWN_EXCEPTION;
    }
}
}

extern "C"
{
OCCTNET_API int occtnet_get_version(char* buffer, int bufferLength)
{
    return invoke([&] {
        copy_string(OCC_VERSION_COMPLETE, buffer, bufferLength);
    });
}

OCCTNET_API int occtnet_get_last_error(char* buffer, int bufferLength)
{
    copy_string(lastError, buffer, bufferLength);
    return OCCTNET_OK;
}

OCCTNET_API int occtnet_point_create(double x, double y, double z, void** point)
{
    if (point == nullptr)
    {
        return invalid_argument("point output pointer is null.");
    }

    return invoke([&] {
        *point = new gp_Pnt(x, y, z);
    });
}

OCCTNET_API int occtnet_point_destroy(void* point)
{
    return invoke([&] {
        delete static_cast<gp_Pnt*>(point);
    });
}

OCCTNET_API int occtnet_point_get_coordinates(void* point, double* x, double* y, double* z)
{
    if (point == nullptr || x == nullptr || y == nullptr || z == nullptr)
    {
        return invalid_argument("point or coordinate output pointer is null.");
    }

    return invoke([&] {
        const auto* nativePoint = static_cast<const gp_Pnt*>(point);
        *x = nativePoint->X();
        *y = nativePoint->Y();
        *z = nativePoint->Z();
    });
}

OCCTNET_API int occtnet_point_set_coordinates(void* point, double x, double y, double z)
{
    if (point == nullptr)
    {
        return invalid_argument("point pointer is null.");
    }

    return invoke([&] {
        auto* nativePoint = static_cast<gp_Pnt*>(point);
        nativePoint->SetCoord(x, y, z);
    });
}

OCCTNET_API int occtnet_point_distance(void* left, void* right, double* distance)
{
    if (left == nullptr || right == nullptr || distance == nullptr)
    {
        return invalid_argument("point or distance output pointer is null.");
    }

    return invoke([&] {
        const auto* leftPoint = static_cast<const gp_Pnt*>(left);
        const auto* rightPoint = static_cast<const gp_Pnt*>(right);
        *distance = leftPoint->Distance(*rightPoint);
    });
}

OCCTNET_API int occtnet_shape_destroy(void* shape)
{
    return invoke([&] {
        delete static_cast<TopoDS_Shape*>(shape);
    });
}

OCCTNET_API int occtnet_shape_is_null(void* shape, int* isNull)
{
    if (shape == nullptr || isNull == nullptr)
    {
        return invalid_argument("shape or isNull output pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        *isNull = nativeShape->IsNull() ? 1 : 0;
    });
}

OCCTNET_API int occtnet_shape_make_box(double dx, double dy, double dz, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    if (dx <= 0 || dy <= 0 || dz <= 0)
    {
        return invalid_argument("box dimensions must be greater than zero.");
    }

    return invoke([&] {
        BRepPrimAPI_MakeBox maker(dx, dy, dz);
        *shape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_make_cylinder(double radius, double height, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    if (radius <= 0 || height <= 0)
    {
        return invalid_argument("cylinder radius and height must be greater than zero.");
    }

    return invoke([&] {
        BRepPrimAPI_MakeCylinder maker(radius, height);
        *shape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_make_sphere(double radius, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    if (radius <= 0)
    {
        return invalid_argument("sphere radius must be greater than zero.");
    }

    return invoke([&] {
        BRepPrimAPI_MakeSphere maker(radius);
        *shape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_make_edge(
    double startX,
    double startY,
    double startZ,
    double endX,
    double endY,
    double endZ,
    void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    return invoke([&] {
        BRepBuilderAPI_MakeEdge maker(gp_Pnt(startX, startY, startZ), gp_Pnt(endX, endY, endZ));
        if (!maker.IsDone())
        {
            throw std::runtime_error("edge creation failed.");
        }

        *shape = new TopoDS_Shape(maker.Edge());
    });
}

OCCTNET_API int occtnet_shape_make_wire(void** edges, int edgeCount, void** shape)
{
    if (edges == nullptr || shape == nullptr)
    {
        return invalid_argument("edges or shape output pointer is null.");
    }

    if (edgeCount <= 0)
    {
        return invalid_argument("wire requires at least one edge.");
    }

    return invoke([&] {
        BRepBuilderAPI_MakeWire maker;
        for (int index = 0; index < edgeCount; ++index)
        {
            const auto* edgeShape = require_shape(edges[index], "edge pointer is null.");
            maker.Add(TopoDS::Edge(*edgeShape));
        }

        if (!maker.IsDone())
        {
            throw std::runtime_error("wire creation failed.");
        }

        *shape = new TopoDS_Shape(maker.Wire());
    });
}

OCCTNET_API int occtnet_shape_make_face(void* wire, void** shape)
{
    if (wire == nullptr || shape == nullptr)
    {
        return invalid_argument("wire or shape output pointer is null.");
    }

    return invoke([&] {
        const auto* wireShape = static_cast<const TopoDS_Shape*>(wire);
        BRepBuilderAPI_MakeFace maker(TopoDS::Wire(*wireShape), true);
        if (!maker.IsDone())
        {
            throw std::runtime_error("face creation failed.");
        }

        *shape = new TopoDS_Shape(maker.Face());
    });
}

OCCTNET_API int occtnet_shape_get_bounding_box(
    void* shape,
    double* minX,
    double* minY,
    double* minZ,
    double* maxX,
    double* maxY,
    double* maxZ)
{
    if (shape == nullptr || minX == nullptr || minY == nullptr || minZ == nullptr || maxX == nullptr || maxY == nullptr || maxZ == nullptr)
    {
        return invalid_argument("shape or bounding box output pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        Bnd_Box box;
        BRepBndLib::Add(*nativeShape, box);
        box.Get(*minX, *minY, *minZ, *maxX, *maxY, *maxZ);
    });
}

OCCTNET_API int occtnet_shape_translate(void* shape, double x, double y, double z, void** translatedShape)
{
    if (shape == nullptr || translatedShape == nullptr)
    {
        return invalid_argument("shape or translatedShape output pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        gp_Trsf transform;
        transform.SetTranslation(gp_Vec(x, y, z));
        BRepBuilderAPI_Transform transformer(*nativeShape, transform, true);
        *translatedShape = new TopoDS_Shape(transformer.Shape());
    });
}

OCCTNET_API int occtnet_shape_extrude(void* shape, double x, double y, double z, void** extrudedShape)
{
    if (shape == nullptr || extrudedShape == nullptr)
    {
        return invalid_argument("shape or extrudedShape output pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        BRepPrimAPI_MakePrism maker(*nativeShape, gp_Vec(x, y, z), true, true);
        *extrudedShape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_revolve(
    void* shape,
    double originX,
    double originY,
    double originZ,
    double directionX,
    double directionY,
    double directionZ,
    double angleRadians,
    void** revolvedShape)
{
    if (shape == nullptr || revolvedShape == nullptr)
    {
        return invalid_argument("shape or revolvedShape output pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        const gp_Ax1 axis(gp_Pnt(originX, originY, originZ), gp_Dir(directionX, directionY, directionZ));
        BRepPrimAPI_MakeRevol maker(*nativeShape, axis, angleRadians, true);
        *revolvedShape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_fuse(void* left, void* right, void** resultShape)
{
    if (left == nullptr || right == nullptr || resultShape == nullptr)
    {
        return invalid_argument("shape input or result output pointer is null.");
    }

    return invoke([&] {
        BRepAlgoAPI_Fuse maker(*static_cast<const TopoDS_Shape*>(left), *static_cast<const TopoDS_Shape*>(right));
        maker.Build();
        if (!maker.IsDone())
        {
            throw std::runtime_error("boolean fuse failed.");
        }

        *resultShape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_cut(void* left, void* right, void** resultShape)
{
    if (left == nullptr || right == nullptr || resultShape == nullptr)
    {
        return invalid_argument("shape input or result output pointer is null.");
    }

    return invoke([&] {
        BRepAlgoAPI_Cut maker(*static_cast<const TopoDS_Shape*>(left), *static_cast<const TopoDS_Shape*>(right));
        maker.Build();
        if (!maker.IsDone())
        {
            throw std::runtime_error("boolean cut failed.");
        }

        *resultShape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_common(void* left, void* right, void** resultShape)
{
    if (left == nullptr || right == nullptr || resultShape == nullptr)
    {
        return invalid_argument("shape input or result output pointer is null.");
    }

    return invoke([&] {
        BRepAlgoAPI_Common maker(*static_cast<const TopoDS_Shape*>(left), *static_cast<const TopoDS_Shape*>(right));
        maker.Build();
        if (!maker.IsDone())
        {
            throw std::runtime_error("boolean common failed.");
        }

        *resultShape = new TopoDS_Shape(maker.Shape());
    });
}

OCCTNET_API int occtnet_shape_export_stl(void* shape, const char* filePath, int ascii)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape pointer is null.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        BRepMesh_IncrementalMesh mesher(*nativeShape, 0.1, false, 0.5, true);
        StlAPI_Writer writer;
        writer.ASCIIMode() = ascii != 0;
        if (!writer.Write(*nativeShape, require_path(filePath)))
        {
            throw std::runtime_error("STL export failed.");
        }
    });
}

OCCTNET_API int occtnet_shape_import_stl(const char* filePath, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    return invoke([&] {
        TopoDS_Shape importedShape;
        StlAPI_Reader reader;
        if (!reader.Read(importedShape, require_path(filePath)))
        {
            throw std::runtime_error("STL import failed.");
        }

        *shape = new TopoDS_Shape(importedShape);
    });
}

OCCTNET_API int occtnet_shape_export_step(void* shape, const char* filePath)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape pointer is null.");
    }

    return invoke([&] {
        STEPControl_Writer writer;
        ensure_done(writer.Transfer(*static_cast<const TopoDS_Shape*>(shape), STEPControl_AsIs), "STEP transfer");
        ensure_done(writer.Write(require_path(filePath)), "STEP export");
    });
}

OCCTNET_API int occtnet_shape_import_step(const char* filePath, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    return invoke([&] {
        STEPControl_Reader reader;
        ensure_done(reader.ReadFile(require_path(filePath)), "STEP read");
        if (reader.TransferRoots() <= 0)
        {
            throw std::runtime_error("STEP transfer produced no shapes.");
        }

        *shape = new TopoDS_Shape(reader.OneShape());
    });
}

OCCTNET_API int occtnet_shape_export_iges(void* shape, const char* filePath)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape pointer is null.");
    }

    return invoke([&] {
        IGESControl_Writer writer("MM", 1);
        if (!writer.AddShape(*static_cast<const TopoDS_Shape*>(shape)))
        {
            throw std::runtime_error("IGES transfer failed.");
        }

        writer.ComputeModel();
        if (!writer.Write(require_path(filePath)))
        {
            throw std::runtime_error("IGES export failed.");
        }
    });
}

OCCTNET_API int occtnet_shape_import_iges(const char* filePath, void** shape)
{
    if (shape == nullptr)
    {
        return invalid_argument("shape output pointer is null.");
    }

    return invoke([&] {
        IGESControl_Reader reader;
        ensure_done(reader.ReadFile(require_path(filePath)), "IGES read");
        if (reader.TransferRoots() <= 0)
        {
            throw std::runtime_error("IGES transfer produced no shapes.");
        }

        *shape = new TopoDS_Shape(reader.OneShape());
    });
}

OCCTNET_API int occtnet_mesh_create(void* shape, double linearDeflection, double angularDeflection, void** mesh)
{
    if (shape == nullptr || mesh == nullptr)
    {
        return invalid_argument("shape or mesh output pointer is null.");
    }

    if (linearDeflection <= 0 || angularDeflection <= 0)
    {
        return invalid_argument("mesh deflection values must be greater than zero.");
    }

    return invoke([&] {
        const auto* nativeShape = static_cast<const TopoDS_Shape*>(shape);
        BRepMesh_IncrementalMesh mesher(*nativeShape, linearDeflection, false, angularDeflection, true);
        auto* meshData = new MeshData();

        for (TopExp_Explorer explorer(*nativeShape, TopAbs_FACE); explorer.More(); explorer.Next())
        {
            TopLoc_Location location;
            const auto face = TopoDS::Face(explorer.Current());
            const Handle(Poly_Triangulation)& triangulation = BRep_Tool::Triangulation(face, location);

            if (triangulation.IsNull())
            {
                continue;
            }

            const auto transform = location.Transformation();
            const auto vertexOffset = static_cast<int>(meshData->vertices.size() / 3);

            for (int nodeIndex = 1; nodeIndex <= triangulation->NbNodes(); ++nodeIndex)
            {
                gp_Pnt point = triangulation->Node(nodeIndex);
                point.Transform(transform);
                meshData->vertices.push_back(point.X());
                meshData->vertices.push_back(point.Y());
                meshData->vertices.push_back(point.Z());
            }

            for (int triangleIndex = 1; triangleIndex <= triangulation->NbTriangles(); ++triangleIndex)
            {
                int n1 = 0;
                int n2 = 0;
                int n3 = 0;
                triangulation->Triangle(triangleIndex).Get(n1, n2, n3);
                meshData->triangleIndices.push_back(vertexOffset + n1 - 1);
                meshData->triangleIndices.push_back(vertexOffset + n2 - 1);
                meshData->triangleIndices.push_back(vertexOffset + n3 - 1);
            }
        }

        *mesh = meshData;
    });
}

OCCTNET_API int occtnet_mesh_destroy(void* mesh)
{
    return invoke([&] {
        delete static_cast<MeshData*>(mesh);
    });
}

OCCTNET_API int occtnet_mesh_get_counts(void* mesh, int* vertexCount, int* triangleIndexCount)
{
    if (mesh == nullptr || vertexCount == nullptr || triangleIndexCount == nullptr)
    {
        return invalid_argument("mesh or count output pointer is null.");
    }

    return invoke([&] {
        const auto* meshData = static_cast<const MeshData*>(mesh);
        *vertexCount = static_cast<int>(meshData->vertices.size() / 3);
        *triangleIndexCount = static_cast<int>(meshData->triangleIndices.size());
    });
}

OCCTNET_API int occtnet_mesh_copy(void* mesh, double* vertices, int vertexDoubleCount, int* triangleIndices, int triangleIndexCount)
{
    if (mesh == nullptr || vertices == nullptr || triangleIndices == nullptr)
    {
        return invalid_argument("mesh or output buffer pointer is null.");
    }

    return invoke([&] {
        const auto* meshData = static_cast<const MeshData*>(mesh);
        if (vertexDoubleCount < static_cast<int>(meshData->vertices.size()) ||
            triangleIndexCount < static_cast<int>(meshData->triangleIndices.size()))
        {
            throw std::runtime_error("mesh output buffers are too small.");
        }

        std::copy(meshData->vertices.begin(), meshData->vertices.end(), vertices);
        std::copy(meshData->triangleIndices.begin(), meshData->triangleIndices.end(), triangleIndices);
    });
}
}
