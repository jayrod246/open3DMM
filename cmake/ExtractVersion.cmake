include_guard(GLOBAL)

#[[
This is used to extract a version variable from nearly every executable in
existence. How these are used is up to the user.
]]
function(extract_version output)
  cmake_parse_arguments(ARG "" "OPTION;REGEX;COMMAND;DOC" "" ${ARGN})
  unset(version-output)
  unset(version-check)
  if (NOT ARG_OPTION)
    set(ARG_OPTION "--version")
  endif()
  if (NOT ARG_REGEX)
    set(ARG_REGEX "[^0-9]*([0-9]+)[.]([0-9]+)?[.]?([0-9]+)?[.]?([0-9]+)?.*")
  endif()
  if (ARG_COMMAND AND NOT DEFINED ${output})
    execute_process(
      COMMAND "${ARG_COMMAND}" "${ARG_OPTION}"
      OUTPUT_VARIABLE version-output
      OUTPUT_STRIP_TRAILING_WHITESPACE
      ENCODING UTF-8)
    if (version-output)
      string(REGEX MATCH "${ARG_REGEX}" version-check "${version-output}")
    endif()
    if (version-check)
      string(JOIN "." ${output}
        ${CMAKE_MATCH_1}
        ${CMAKE_MATCH_2}
        ${CMAKE_MATCH_3}
        ${CMAKE_MATCH_4})
      set(${output} "${${output}}" CACHE STRING "${ARG_DOC}" FORCE)
    endif()
  endif()
endfunction()
