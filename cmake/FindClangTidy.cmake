include(FindPackageHandleStandardArgs)
include(ExtractVersion)

find_program(ClangTidy_EXECUTABLE NAMES clang-tidy)
extract_version(ClangTidy_VERSION COMMAND "${ClangTidy_EXECUTABLE}")

find_package_handle_standard_args(ClangTidy
  REQUIRED_VARS ClangTidy_EXECUTABLE
  VERSION_VAR ClangTidy_VERSION)

if (ClangTidy_FOUND AND NOT TARGET Clang::Tidy)
  add_executable(Clang::Tidy IMPORTED)
  set_target_properties(Clang::Tidy
    PROPERTIES
      IMPORTED_LOCATION "${ClangTidy_EXECUTABLE}"
      VERSION "${ClangTidy_VERSION}")
  mark_as_advanced(ClangTidy_EXECUTABLE ClangTidy_VERSION)
endif()
