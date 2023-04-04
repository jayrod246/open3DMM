include(FindPackageHandleStandardArgs)
# This is technically bad form because we aren't "finding" anything, however if
# there is ever any hope to eventually port 3DMM to "not Windows", there needs
# to be a shim of some kind for slotting in other possible releases of BRender.
#
# This file can be expanded *properly* at a later date.

# TODO: Keep in mind the NO_<...>_PATH stuff should be removed if 3DMMForever
# starts to support external versions of BRender

foreach (name IN ITEMS BRFMMXR BRFWMXR BRZBMXR)
  foreach (cfg IN ITEMS DEBUG RELEASE RELWITHDEBINFO MINSIZEREL)
    set(variable ${CMAKE_FIND_PACKAGE_NAME}_${name}_${cfg}_LIBRARY)
    set(suffix "s")
    if (${cfg} STREQUAL "DEBUG")
      set(suffix "d")
    endif()
    find_library(${variable}
      NAMES ${name}
      PATHS "${PROJECT_SOURCE_DIR}/elib/win${suffix}"
      NO_DEFAULT_PATH
      NO_PACKAGE_ROOT_PATH
      NO_CMAKE_PATH
      NO_CMAKE_ENVIRONMENT_PATH
      NO_SYSTEM_ENVIRONMENT_PATH
      NO_CMAKE_SYSTEM_PATH
      NO_CMAKE_FIND_ROOT_PATH)
    list(APPEND ${CMAKE_FIND_PACKAGE_NAME}_LIBRARIES ${variable})
  endforeach()
endforeach()

find_package_handle_standard_args(${CMAKE_FIND_PACKAGE_NAME}
  REQUIRED_VARS ${${CMAKE_FIND_PACKAGE_NAME}_LIBRARIES})

if (${CMAKE_FIND_PACKAGE_NAME}_FOUND AND NOT TARGET BRender::Libraries)
  add_library(BRender::Libraries INTERFACE IMPORTED)

  foreach (library IN ITEMS BRFMMXR BRFWMXR BRZBMXR)
    add_library(BRender::${library} STATIC IMPORTED)
    set_target_properties(BRender::${library}
      PROPERTIES
        IMPORTED_LOCATION_RELEASE ${${CMAKE_FIND_PACKAGE_NAME}_${library}_RELEASE_LIBRARY}
        IMPORTED_LOCATION_RELWITHDEBINFO ${${CMAKE_FIND_PACKAGE_NAME}_${library}_RELWITHDEBINFO_LIBRARY}
        IMPORTED_LOCATION_MINSIZEREL ${${CMAKE_FIND_PACKAGE_NAME}_${library}_MINSIZEREL_LIBRARY}
        IMPORTED_LOCATION_DEBUG ${${CMAKE_FIND_PACKAGE_NAME}_${library}_DEBUG_LIBRARY})
      target_link_libraries(BRender::Libraries INTERFACE BRender::${library})
    mark_as_advanced(
      ${CMAKE_FIND_PACKAGE_NAME}_${library}_RELEASE_LIBRARY
      ${CMAKE_FIND_PACKAGE_NAME}_${library}_DEBUG_LIBRARY)
  endforeach()
  # TODO(bruxisma): This needs to be enforce ONLY when targetin Visual Studio
  # 2015 or later
  target_link_libraries(BRender::BRFWMXR
    INTERFACE
      $<$<LINK_LANG_AND_ID:CXX,MSVC>:legacy_stdio_definitions>)

  # Precompiled BRender libraries do not support SafeSEH
  target_link_options(BRender::BRFWMXR INTERFACE
      $<$<LINK_LANG_AND_ID:CXX,MSVC>:/SAFESEH:NO>
  )
endif()
