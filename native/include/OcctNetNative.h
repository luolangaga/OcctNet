#pragma once

#ifdef _WIN32
#    ifdef OCCTNET_NATIVE_EXPORTS
#        define OCCTNET_API __declspec(dllexport)
#    else
#        define OCCTNET_API __declspec(dllimport)
#    endif
#else
#    define OCCTNET_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

enum OcctNetStatus
{
    OCCTNET_OK = 0,
    OCCTNET_INVALID_ARGUMENT = 1,
    OCCTNET_OCCT_EXCEPTION = 2,
    OCCTNET_UNKNOWN_EXCEPTION = 3
};

OCCTNET_API int occtnet_get_version(char* buffer, int bufferLength);
OCCTNET_API int occtnet_get_last_error(char* buffer, int bufferLength);

OCCTNET_API int occtnet_point_create(double x, double y, double z, void** point);
OCCTNET_API int occtnet_point_destroy(void* point);
OCCTNET_API int occtnet_point_get_coordinates(void* point, double* x, double* y, double* z);
OCCTNET_API int occtnet_point_set_coordinates(void* point, double x, double y, double z);
OCCTNET_API int occtnet_point_distance(void* left, void* right, double* distance);

OCCTNET_API int occtnet_shape_destroy(void* shape);
OCCTNET_API int occtnet_shape_is_null(void* shape, int* isNull);
OCCTNET_API int occtnet_shape_make_box(double dx, double dy, double dz, void** shape);
OCCTNET_API int occtnet_shape_make_cylinder(double radius, double height, void** shape);
OCCTNET_API int occtnet_shape_make_sphere(double radius, void** shape);
OCCTNET_API int occtnet_shape_make_edge(
    double startX,
    double startY,
    double startZ,
    double endX,
    double endY,
    double endZ,
    void** shape);
OCCTNET_API int occtnet_shape_make_wire(void** edges, int edgeCount, void** shape);
OCCTNET_API int occtnet_shape_make_face(void* wire, void** shape);
OCCTNET_API int occtnet_shape_get_bounding_box(
    void* shape,
    double* minX,
    double* minY,
    double* minZ,
    double* maxX,
    double* maxY,
    double* maxZ);
OCCTNET_API int occtnet_shape_translate(void* shape, double x, double y, double z, void** translatedShape);
OCCTNET_API int occtnet_shape_extrude(void* shape, double x, double y, double z, void** extrudedShape);
OCCTNET_API int occtnet_shape_revolve(
    void* shape,
    double originX,
    double originY,
    double originZ,
    double directionX,
    double directionY,
    double directionZ,
    double angleRadians,
    void** revolvedShape);
OCCTNET_API int occtnet_shape_fuse(void* left, void* right, void** resultShape);
OCCTNET_API int occtnet_shape_cut(void* left, void* right, void** resultShape);
OCCTNET_API int occtnet_shape_common(void* left, void* right, void** resultShape);
OCCTNET_API int occtnet_shape_export_stl(void* shape, const char* filePath, int ascii);
OCCTNET_API int occtnet_shape_import_stl(const char* filePath, void** shape);
OCCTNET_API int occtnet_shape_export_step(void* shape, const char* filePath);
OCCTNET_API int occtnet_shape_import_step(const char* filePath, void** shape);
OCCTNET_API int occtnet_shape_export_iges(void* shape, const char* filePath);
OCCTNET_API int occtnet_shape_import_iges(const char* filePath, void** shape);

OCCTNET_API int occtnet_mesh_create(void* shape, double linearDeflection, double angularDeflection, void** mesh);
OCCTNET_API int occtnet_mesh_destroy(void* mesh);
OCCTNET_API int occtnet_mesh_get_counts(void* mesh, int* vertexCount, int* triangleIndexCount);
OCCTNET_API int occtnet_mesh_copy(void* mesh, double* vertices, int vertexDoubleCount, int* triangleIndices, int triangleIndexCount);

#ifdef __cplusplus
}
#endif
