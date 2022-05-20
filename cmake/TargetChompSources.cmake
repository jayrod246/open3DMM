include_guard(GLOBAL)

# any values after ${target} are treated as sources
function(target_chomp_sources target)
  # We cannot use $<CXX_COMPILER_ID:...> because we are not generating binary
  # targets 😢
  set(is-msvc $<STREQUAL:${CMAKE_CXX_COMPILER_ID},MSVC>)
  set(include-directories $<TARGET_PROPERTY:${target},INCLUDE_DIRECTORIES>)
  set(compile-definitions $<TARGET_PROPERTY:${target},COMPILE_DEFINITIONS>)
  string(CONCAT include-directories $<
    $<BOOL:${include-directories}>:
    -I$<JOIN:$<REMOVE_DUPLICATES:${include-directories}>,$<SEMICOLON>-I>
  >)
  string(CONCAT compile-definitions $<
    $<BOOL:${compile-definitions}>:
    -D$<JOIN:$<REMOVE_DUPLICATES:${compile-definitions}>,$<SEMICOLON>-D>
  >)
  foreach(source IN LISTS ARGN)
    cmake_path(GET source FILENAME filename)
    cmake_path(REPLACE_EXTENSION filename LAST_ONLY ".chk" OUTPUT_VARIABLE output)
    # Because of how files are laid out, we need to do the following line twice
    # but operate on the resulting path
    cmake_path(GET source PARENT_PATH parent)
    target_include_directories(${target} PRIVATE ${parent})
    cmake_path(GET parent PARENT_PATH parent)
    target_include_directories(${target} PRIVATE ${parent})
    set(output "${CMAKE_CURRENT_BINARY_DIR}/chomp/${target}/${output}")
    set(processed "${CMAKE_CURRENT_BINARY_DIR}/chomp/${target}/${filename}.i")
    add_custom_command(
      OUTPUT "${processed}"
      COMMAND "${CMAKE_CXX_COMPILER}"
        "$<${is-msvc}:/nologo>"
        "$<${is-msvc}:/E>"
        "$<${is-msvc}:/TP>"
        "${include-directories}"
        "${compile-definitions}"
        "${source}" > "${processed}"
      IMPLICIT_DEPENDS CXX "${source}"
      MAIN_DEPENDENCY "${source}"
      COMMENT "Preprocessing ${source}"
      COMMAND_EXPAND_LISTS
      USES_TERMINAL
      VERBATIM)
    add_custom_command(
      OUTPUT "${output}"
      COMMAND chomp /c  "${processed}" "${output}"
      COMMENT "Chompin' ${processed}"
      MAIN_DEPENDENCY "${processed}"
      WORKING_DIRECTORY "${parent}"
      COMMAND_EXPAND_LISTS
      VERBATIM)
    set_property(TARGET "${target}" APPEND PROPERTY CHOMPED_CHUNKS "${output}")
  endforeach()
  add_custom_target(${target}-chomp-chunks
    DEPENDS $<TARGET_PROPERTY:${target},CHOMPED_CHUNKS>)
  add_dependencies(${target} ${target}-chomp-chunks)
endfunction()
