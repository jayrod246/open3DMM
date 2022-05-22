cmake_minimum_required(VERSION 3.22)
project(3DMMForever LANGUAGES CXX C VERSION 0.2)

list(APPEND CMAKE_MODULE_PATH "${PROJECT_SOURCE_DIR}/cmake")

set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
set(CMAKE_CXX_STANDARD 14)

include(CMakeDependentOption)
include(TargetChompSources)

find_package(AudioMan REQUIRED)
find_package(BRender REQUIRED)

# Options
option(BUILD_PACKAGES "Build packages for 3DMM" ON)

cmake_dependent_option(3DMM_PACKAGE_WIX "Generate an MSI via WiX" ON "BUILD_PACKAGES AND DEFINED ENV{WIX}" OFF)
cmake_dependent_option(3DMM_PACKAGE_ZIP "Generate a portable ZIP" ON "BUILD_PACKAGES" OFF)

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

# Local Tooling

add_compile_definitions(
  $<$<PLATFORM_ID:Windows>:WIN>
  $<$<PLATFORM_ID:Windows>:IN_80386>)

# NOTE: kcdc-386 and kcd2-386 prevent cross compiling at this time.
add_executable(kcdc-386 EXCLUDE_FROM_ALL)
target_sources(kcdc-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcdc_386.c")
target_include_directories(kcdc-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_executable(kcd2-386 EXCLUDE_FROM_ALL)
target_sources(kcd2-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcd2_386.c")
target_include_directories(kcd2-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_executable(chelp EXCLUDE_FROM_ALL)
target_sources(chelp PRIVATE "${PROJECT_SOURCE_DIR}/kauai/tools/chelp.cpp")
target_include_directories(chelp PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)
target_link_libraries(chelp PRIVATE $<TARGET_NAME_IF_EXISTS:kauai>)

add_executable(chomp EXCLUDE_FROM_ALL)
target_sources(chomp PRIVATE "${PROJECT_SOURCE_DIR}/kauai/tools/chomp.cpp")
target_include_directories(chomp PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)
target_link_libraries(chomp PRIVATE $<TARGET_NAME_IF_EXISTS:kauai>)

# These are only here to ensure configure is rerun when dependencies are generated.
# This is 
file(GLOB building-chunk-sources CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/SRC/BUILDING/*.cht")
file(GLOB studio-chunk-sources CONFIGURE_DEPENDS "${PROJECT_SORUCE_DIR}/SRC/STUDIO/*.cht")

add_custom_command(
  OUTPUT "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
  COMMAND cmake -E make_directory "${PROJECT_BINARY_DIR}/generated/kauai/src"
  COMMAND kcdc-386 "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
  COMMENT "Generating kcdc_386.h"
  VERBATIM
)

add_custom_command(
  OUTPUT "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"
  COMMAND cmake -E make_directory "${PROJECT_BINARY_DIR}/generated/kauai/src"
  COMMAND kcd2-386 "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"
  COMMENT "Generating kcd2_386.h"
  VERBATIM
)

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
  STRICT)

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
file(GLOB studio-samples CONFIGURE_DEPENDS
  "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/*.3mm")
list(FILTER studio-sources EXCLUDE REGEX "UTESTSCB.CPP$")
target_sources(studio PRIVATE ${studio-sources})
target_include_directories(studio
  PUBLIC $<TARGET_PROPERTY:engine,INCLUDE_DIRECTORIES>
  "${PROJECT_SOURCE_DIR}/SRC")
target_chomp_sources(studio
  "${PROJECT_SOURCE_DIR}/SRC/STUDIO/UTEST.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/STUDIO/STUDIO.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/BUILDING/BLDGHD.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/BUILDING/BUILDING.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/HELPAUD/HELPAUD.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/HELP/HELP.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/SHARED/SHARED.CHT"
  "${PROJECT_SOURCE_DIR}/SRC/SHARED/SHARECD.CHT")

target_link_libraries(studio PRIVATE engine)
set_property(TARGET studio PROPERTY OUTPUT_NAME 3dmovie)
target_link_options(studio BEFORE PRIVATE $<$<LINK_LANG_AND_ID:CXX,MSVC>:/MANIFESTUAC:NO>)
target_link_options(studio
  BEFORE PRIVATE
    $<$<AND:$<LINK_LANG_AND_ID:CXX,MSVC>,$<CONFIG:DEBUG>>:/NODEFAULTLIB:libcmt.lib>)
set_property(TARGET studio PROPERTY 3DMM_SAMPLES ${studio-samples})

# Please pay attention to the ending / in each DIRECTORY declaration. It
# affects the destination!
install(TARGETS studio RUNTIME DESTINATION .)
# HACK(bruxisma): This is due to UTEST.CHT not generating 3DMOVIE.CHK
#                 Renaming it to 3DMOVIE.CHT would resolve this.
install(FILES $<FILTER:$<TARGET_PROPERTY:studio,CHOMPED_CHUNKS>,EXCLUDE,.+UTEST[.]chk$>
  DESTINATION "Microsoft Kids/3D Movie Maker")
install(FILES $<FILTER:$<TARGET_PROPERTY:studio,CHOMPED_CHUNKS>,INCLUDE,.+UTEST[.]chk$>
  RENAME 3dmovie.chk
  DESTINATION "Microsoft Kids/3D Movie Maker")
install(FILES $<TARGET_PROPERTY:studio,3DMM_SAMPLES>
  DESTINATION "Microsoft Kids/3D Movie Maker/Users/Melanie")
install(DIRECTORY "${PROJECT_SOURCE_DIR}/cd9/"
  DESTINATION "Microsoft Kids/3D Movie Maker")

# CPack must be included *last*, which is antithetical to how most CMake
# Modules work. We can set all variables for all packaging regardless of
# whether they're supported. This way we can just swap out what is added to
# `CPACK_GENERATOR`

# DO NOT CHANGE THIS. EVER. Generated manually via uuidgen. This needs to be
# the same so that user's can automatically upgrade via MSI
set(CPACK_WIX_UPGRADE_GUID 01c5fef4-82b5-4194-aaf3-bd0247004941)
# Change this as needed.
set(CPACK_WIX_PROPERTY_ARPURLINFOABOUT https://github.com/foone/3DMMForever)

if (BUILD_PACKAGES)
  add_custom_target(dist DEPENDS $<TARGET_NAME_IF_EXISTS:package>)
  foreach (generator IN ITEMS ZIP WIX)
    if (3DMM_PACKAGE_${generator})
      list(APPEND CPACK_GENERATOR ${generator})
    endif()
  endforeach()
  include(CPack)
endif()
