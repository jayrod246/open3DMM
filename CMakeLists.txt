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
  $<$<PLATFORM_ID:Windows>:IN_80386>
  $<$<CONFIG:Debug>:DEBUG>
)

if (NOT CMAKE_SIZEOF_VOID_P EQUAL 4)
  message(FATAL_ERROR "Cannot compile for 64-bit yet")
endif()

# NOTE: kcdc-386 and kcd2-386 prevent cross compiling at this time.
add_executable(kcdc-386 EXCLUDE_FROM_ALL)
target_sources(kcdc-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcdc_386.c")
target_include_directories(kcdc-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_executable(kcd2-386 EXCLUDE_FROM_ALL)
target_sources(kcd2-386 PRIVATE "${PROJECT_SOURCE_DIR}/kauai/src/kcd2_386.c")
target_include_directories(kcd2-386 PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)

add_executable(chelp WIN32 EXCLUDE_FROM_ALL)
target_sources(chelp PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHTOP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHELP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHELPEXP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHELP.RC"
)
target_include_directories(chelp PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)
target_link_libraries(chelp PRIVATE $<TARGET_NAME_IF_EXISTS:kauai>)

# Kauai test applications
add_executable(ft WIN32 EXCLUDE_FROM_ALL)
target_sources(ft PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/SRC/TEST.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/FT.RC"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/FT.CPP"
)
target_link_libraries(ft PRIVATE kauai)

add_executable(ut EXCLUDE_FROM_ALL)
target_sources(ut PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UT.CPP"
)
target_link_libraries(ut PRIVATE kauai)

add_executable(chomp EXCLUDE_FROM_ALL)
target_sources(chomp PRIVATE "${PROJECT_SOURCE_DIR}/kauai/tools/chomp.cpp")
target_include_directories(chomp PRIVATE $<TARGET_PROPERTY:kauai,INCLUDE_DIRECTORIES>)
target_link_libraries(chomp PRIVATE $<TARGET_NAME_IF_EXISTS:kauai>)

# Chunk Editor
add_executable(ched WIN32 EXCLUDE_FROM_ALL)
target_sources(ched PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHED.RC"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHDOC.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHED.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHGRP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHHEX.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHMBMP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHPIC.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHTXT.CPP"
)
target_link_libraries(ched PRIVATE kauai)

# mkmbmp
add_executable(mkmbmp EXCLUDE_FROM_ALL)
target_sources(mkmbmp PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/MKMBMP.CPP"
)
target_link_libraries(mkmbmp PRIVATE kauai)

# kpack
add_executable(kpack EXCLUDE_FROM_ALL)
target_sources(kpack PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/KPACK.CPP"
)
target_link_libraries(kpack PRIVATE kauai)

# chmerge
add_executable(chmerge EXCLUDE_FROM_ALL)
target_sources(chmerge PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHMERGE.CPP"
)
target_link_libraries(chmerge PRIVATE kauai)

# chelpdmp
add_executable(chelpdmp EXCLUDE_FROM_ALL)
target_sources(chelpdmp PRIVATE
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHELPDMP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/TOOLS/CHELPEXP.CPP"
)
target_link_libraries(chelpdmp PRIVATE kauai)

# These are only here to ensure configure is rerun when dependencies are generated.
# This is 
file(GLOB building-chunk-sources CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/SRC/BUILDING/*.cht")
file(GLOB studio-chunk-sources CONFIGURE_DEPENDS "${PROJECT_SOURCE_DIR}/SRC/STUDIO/*.cht")

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
target_include_directories(
  kauai
    PUBLIC
      "${PROJECT_SOURCE_DIR}/kauai/src"
      "$<BUILD_INTERFACE:${PROJECT_BINARY_DIR}/generated/kauai/src>")

target_sources(kauai
  PRIVATE
    "${PROJECT_BINARY_DIR}/generated/kauai/src/kcdc_386.h"
    "${PROJECT_BINARY_DIR}/generated/kauai/src/kcd2_386.h"

    "${PROJECT_SOURCE_DIR}/kauai/SRC/APPB.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/BASE.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CHCM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CHSE.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CHUNK.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CLIP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CLOK.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CMD.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CODEC.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CODKAUAI.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CRF.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CTL.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/CURSOR.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/DLG.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/DOCB.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/FILE.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/FRAME.RC"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/GFX.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/GOB.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/GROUPS.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/GROUPS2.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/KIDHELP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/KIDSPACE.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/KIDWORLD.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/LEX.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MBMP.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MBMPGUI.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MIDI.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MIDIDEV.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MIDIDEV2.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/MSSIO.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/PIC.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/REGION.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/RTXT.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/RTXT2.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SCRCOM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SCRCOMG.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SCREXE.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SCREXEG.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SNDAM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SNDM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/SPELL.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/STREAM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/TEXT.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/TEXTDOC.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTIL.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILCOPY.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILERRO.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILGLOB.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILINT.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILMEM.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILRND.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/UTILSTR.CPP"
    "${PROJECT_SOURCE_DIR}/kauai/SRC/VIDEO.CPP"

    # Windows implementations
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/APPBWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/DLGWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/FILEWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/FNIWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/GFXWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/MEMWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/MENUWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/PICWIN.CPP>
    $<$<PLATFORM_ID:Windows>:${PROJECT_SOURCE_DIR}/kauai/SRC/GOBWIN.CPP>

    # Stubs for Visual C++ 2.1 CRT functions
    "${PROJECT_SOURCE_DIR}/kauai/SRC/stub.cpp"   
)

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
target_sources(brender
    PRIVATE
    "${PROJECT_SOURCE_DIR}/BREN/BWLD.CPP"
    "${PROJECT_SOURCE_DIR}/BREN/stderr.c"
    "${PROJECT_SOURCE_DIR}/BREN/stdfile.c"
    "${PROJECT_SOURCE_DIR}/BREN/stdmem.c"
    "${PROJECT_SOURCE_DIR}/BREN/TMAP.CPP"
    "${PROJECT_SOURCE_DIR}/BREN/ZBMP.CPP"
)
target_include_directories(brender PUBLIC "${PROJECT_SOURCE_DIR}/BREN/INC")
target_link_libraries(brender
  PUBLIC
    kauai
    BRender::Libraries)

add_library(engine)
target_sources(engine
    PRIVATE
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/ACTOR.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/ACTREDIT.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/ACTRSAVE.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/ACTRSND.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/BKGD.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/BODY.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/MODL.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/MOVIE.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/MSND.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/MTRL.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/SCENE.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/SREC.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TAGL.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TAGMAN.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TBOX.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TDF.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TDT.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/ENGINE/TMPL.CPP"
)
target_include_directories(engine PUBLIC
  $<BUILD_INTERFACE:${PROJECT_SOURCE_DIR}/INC>)
target_link_libraries(engine PUBLIC kauai brender)

# On non-windows WIN32 is a no-op
add_executable(studio WIN32)
target_sources(studio
    PRIVATE
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/APE.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/BROWSER.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/ESL.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/MMINSTAL.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/POPUP.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/PORTF.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/SCNSORT.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/SPLOT.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/STDIOBRW.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/STDIOSCB.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/STUDIO.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/TATR.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/TGOB.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/UTEST.CPP"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/UTEST.RC"
    "${PROJECT_SOURCE_DIR}/SRC/STUDIO/UTEST.RC2"
)
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

set_property(
    TARGET studio
    PROPERTY 3DMM_SAMPLES
    ${studio-samples}
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/BONGO.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/BOOOOOO.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/CITYTOUR.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/GRAVEYRD.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/HAUNTED.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/HOSPITAL.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/JUNGLE.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/MESSAGE.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/SPACE.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/SPROG.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/TERROR.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/THEBOOK.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/THELODGE.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/THETHIEF.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/VENUS31.3MM"
    "${PROJECT_SOURCE_DIR}/cd3/SAMPLES/WHERE.3MM"
)

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
  DESTINATION "Microsoft Kids/Users/Melanie")
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
