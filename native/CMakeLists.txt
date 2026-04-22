cmake_minimum_required(VERSION 3.21)

project(OcctNetNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

find_package(OpenCASCADE REQUIRED COMPONENTS FoundationClasses ModelingData)

add_library(OcctNetNative SHARED
    src/OcctNetNative.cpp
)

target_compile_definitions(OcctNetNative PRIVATE OCCTNET_NATIVE_EXPORTS)

target_include_directories(OcctNetNative
    PUBLIC
        include
    PRIVATE
        ${OpenCASCADE_INCLUDE_DIR}
        ${OpenCASCADE_INCLUDE_DIRS}
)

target_link_libraries(OcctNetNative PRIVATE TKernel TKMath)

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
