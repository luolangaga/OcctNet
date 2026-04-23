cmake_minimum_required(VERSION 3.21)

project(OcctNetNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

find_package(OpenCASCADE REQUIRED COMPONENTS FoundationClasses ModelingData ModelingAlgorithms)

add_library(OcctNetNative SHARED
    src/OcctNetNative.cpp
)

target_compile_definitions(OcctNetNative PRIVATE OCCTNET_NATIVE_EXPORTS)

if(APPLE)
    set_target_properties(OcctNetNative PROPERTIES
        INSTALL_RPATH "@loader_path"
        BUILD_WITH_INSTALL_RPATH ON
    )
elseif(UNIX)
    set_target_properties(OcctNetNative PROPERTIES
        INSTALL_RPATH "$ORIGIN"
        BUILD_WITH_INSTALL_RPATH ON
    )
endif()

target_include_directories(OcctNetNative
    PUBLIC
        include
    PRIVATE
        ${OpenCASCADE_INCLUDE_DIR}
        ${OpenCASCADE_INCLUDE_DIRS}
)

set(OCCTNET_OCCT_LIBRARIES
    TKernel
    TKMath
    TKG2d
    TKG3d
    TKBRep
    TKTopAlgo
    TKPrim
    TKMesh
    TKBO
    TKBool
    TKGeomBase
    TKGeomAlgo
    TKShHealing
    TKXSBase
    TKDE
    TKDESTL
    TKDESTEP
    TKDEIGES
)

foreach(OCCT_LIBRARY_NAME IN LISTS OCCTNET_OCCT_LIBRARIES)
    find_library(
        OCCT_${OCCT_LIBRARY_NAME}_LIBRARY
        NAMES ${OCCT_LIBRARY_NAME}
        HINTS
            ${OpenCASCADE_LIBRARY_DIR}
            ${OpenCASCADE_LIBRARY_DIRS}
    )

    if(NOT OCCT_${OCCT_LIBRARY_NAME}_LIBRARY)
        message(FATAL_ERROR "Could not find OpenCASCADE library: ${OCCT_LIBRARY_NAME}")
    endif()

    list(APPEND OCCTNET_OCCT_LIBRARY_PATHS ${OCCT_${OCCT_LIBRARY_NAME}_LIBRARY})
endforeach()

target_link_libraries(OcctNetNative PRIVATE ${OCCTNET_OCCT_LIBRARY_PATHS})

if(MSVC)
    target_compile_options(OcctNetNative PRIVATE /W4 /permissive-)
else()
    target_compile_options(OcctNetNative PRIVATE -Wall -Wextra -Wpedantic -fvisibility=hidden)
endif()

install(TARGETS OcctNetNative
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)
