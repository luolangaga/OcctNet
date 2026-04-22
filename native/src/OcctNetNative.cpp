#include "OcctNetNative.h"

#include <Standard_Failure.hxx>
#include <Standard_Version.hxx>
#include <gp_Pnt.hxx>

#include <algorithm>
#include <cstring>
#include <exception>
#include <string>

namespace
{
thread_local std::string lastError;

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
}
