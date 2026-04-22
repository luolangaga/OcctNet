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

#ifdef __cplusplus
}
#endif
