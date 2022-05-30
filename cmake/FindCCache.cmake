include(FindPackageHandleStandardArgs)
include(ExtractVersion)

find_program(CCache_EXECUTABLE NAMES ccache)
extract_version(CCache_VERSION COMMAND "${CCache_EXECUTABLE}" DOC "ccache version")

find_package_handle_standard_args(CCache
  REQUIRED_VARS CCache_EXECUTABLE
  VERSION_VAR CCache_VERSION)

if (CCache_FOUND AND NOT TARGET CCache::CCache)
  add_executable(CCache::CCache IMPORTED)
  set_target_properties(CCache::CCache
    PROPERTIES
      IMPORTED_LOCATION "${CCache_EXECUTABLE}"
      VERSION "${CCache_VERSION}")
  mark_as_advanced(CCache_EXECUTABLE CCache_VERSION)
endif()
