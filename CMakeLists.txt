cmake_minimum_required(VERSION 3.23)
project(3DMMForever LANGUAGES CXX C)
list(APPEND CMAKE_MODULE_PATH "${PROJECT_SOURCE_DIR}/cmake")

set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
set(CMAKE_CXX_STANDARD 14)

find_package(AudioMan REQUIRED)
find_package(BRender REQUIRED)

# Optional Tools
find_package(ClangTidy)
find_package(CCache)

if (TARGET CCache::CCache AND NOT CMAKE_CXX_COMPILER_LAUNCHER)
  get_property(CMAKE_CXX_COMPILER_LAUNCHER TARGET CCache::CCache PROPERTY IMPORTED_LOCATION)
  get_property(CMAKE_C_COMPILER_LAUNCHER TARGET CCache::CCache PROPERTY IMPORTED_LOCATION)
endif()

# TODO: Enable this when the 
#if (TARGET Clang::Tidy AND NOT CMAKE_CXX_CLANG_TIDY)
#  get_property(CMAKE_CXX_CLANG_TIDY TARGET Clang::Tidy PROPERTY IMPORTED_LOCATION)
#endif()

add_executable(kcdc-386 EXCLUDE_FROM_ALL)
target_sources(kcdc-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcdc_386.c")
target_include_directories(kcdc-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_executable(kcd2-386 EXCLUDE_FROM_ALL)
target_sources(kcd2-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcd2_386.c")
target_include_directories(kcd2-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_custom_command(
  OUTPUT "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
  COMMAND cmake -E make_directory "${PROJECT_BINARY_DIR}/generated/kauai/src"
  COMMAND kcdc-386 "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
  COMMENT "Generating kcdc_386.h"
  VERBATIM)

add_custom_command(
  OUTPUT "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"
  COMMAND cmake -E make_directory "${PROJECT_BINARY_DIR}/generated/kauai/src"
  COMMAND kcd2-386 "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"
  COMMENT "Generating kcd2_386.h"
  VERBATIM)

add_compile_options($<$<COMPILE_LANG_AND_ID:CXX,MSVC>:/wd4430>)

add_library(kauai)
file(GLOB kauai-sources CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/kauai/src/*.cpp")
file(GLOB kauai-headers CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/kauai/src/*.h")
list(FILTER kauai-sources EXCLUDE REGEX "MAC[.](CPP|cpp)")
target_include_directories(
  kauai
    PUBLIC
      "${PROJECT_SOURCE_DIR}/kauai/src"
      "$<BUILD_INTERFACE:${PROJECT_BINARY_DIR}/generated/kauai/src>")
target_sources(kauai
  PRIVATE
    "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
    "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"
    ${kauai-sources})
target_compile_definitions(kauai PUBLIC
  _LPCVOID_DEFINED
  STRICT
  $<$<PLATFORM_ID:Windows>:IN_80386>
  $<$<PLATFORM_ID:Windows>:WIN>)
target_link_libraries(kauai
  PUBLIC
    3DMMForever::AudioMan
    $<$<PLATFORM_ID:Windows>:Msacm32>
    $<$<PLATFORM_ID:Windows>:Vfw32>
    $<$<PLATFORM_ID:Windows>:Winmm>
    $<$<PLATFORM_ID:Windows>:mpr>)

add_library(brender)
file(GLOB brender-sources CONFIGURE_DEPENDS
  "${PROJECT_SOURCE_DIR}/BREN/*.CPP"
  "${PROJECT_SOURCE_DIR}/BREN/*.C")
list(FILTER brender-sources EXCLUDE REGEX "(MATERIAL|BRENFUN).CPP$")
target_sources(brender PRIVATE ${brender-sources})
target_include_directories(brender PUBLIC "${PROJECT_SOURCE_DIR}/BREN/INC")
target_link_libraries(brender
  PUBLIC
    kauai
    BRender::Libraries)

add_library(engine)
file(GLOB engine-sources CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/SRC/ENGINE/*.CPP")
target_sources(engine PRIVATE ${engine-sources})
target_include_directories(engine PUBLIC
  $<BUILD_INTERFACE:${PROJECT_SOURCE_DIR}/INC>)
target_link_libraries(engine PUBLIC kauai brender)

# On non-windows WIN32 is a no-op
add_executable(studio WIN32)
file(GLOB studio-sources CONFIGURE_DEPENDS
  "${PROJECT_SOURCE_DIR}/src/studio/*.cpp"
  "${PROJECT_SOURCE_DIR}/src/studio/*.rc")
list(FILTER studio-sources EXCLUDE REGEX "UTESTSCB.CPP$")
target_sources(studio PRIVATE ${studio-sources})
target_include_directories(studio PUBLIC $<TARGET_PROPERTY:engine,INCLUDE_DIRECTORIES>)
target_link_libraries(studio PRIVATE engine)
set_property(TARGET studio PROPERTY OUTPUT_NAME 3dmovie)
#target_link_options(studio BEFORE PRIVATE $<$<LINK_LANG_AND_ID:CXX,MSVC>:/VERBOSE:LIB>)
# This resolves the linker warning where multiple runtime libraries are linked in.
target_link_options(studio
  BEFORE PRIVATE
    $<$<AND:$<LINK_LANG_AND_ID:CXX,MSVC>,$<CONFIG:DEBUG>>:/NODEFAULTLIB:libcmt.lib>)
