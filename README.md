# Microsoft 3D Movie Maker

Released in 1995, this is the original source code to the Microsoft 3D Movie Maker project, now released
under the [MIT license](LICENSE) as open source.

![3D Movie Maker](https://github.com/microsoft/Microsoft-3D-Movie-Maker/blob/main/IMG/3dmovie.jpg?raw=true)

## Building instructions

This project is unlikely to build successfully under modern hardware/software, but you can get started with compilation and get partial completed binaries. Here's what will get you going. Thanks to Mac Sample for their work on getting this far!

- Make sure this repo is checked out to a folder with a short name, ideally right on the root of a drive (i.e. C:\3d).
- You will need Visual C++ 2.1's dev tools (located under MSVC21\BIN on its installer disk) on your path. Modern compilers dislike some of the pre C++98 conventions.
- Set these environment variables:
  - set MSVCNT_ROOT=C:\msvc21
  - set PATH=%MSVCNT_ROOT%\bin;%PATH%;
  - set INCLUDE=%MSVCNT_ROOT%\include
  - set LIB=%MSVCNT_ROOT%\lib
- From the root of this repo, run `setvars.bat` you can change the values in this script to change what your build will target.
  - Environment variables used by the build:
    - SOC_ROOT: Path to root of repo (where this README.md is)
    - KAUAI_ROOT: Path to Kauai: %SOC_ROOT%\kauai
    - ARCH: Operating system to build for
      - WIN: Windows
      - MAC: Macintosh 68k
    - INCLUDE: Set to the MSVC include directories, plus:
      - %SOC_ROOT%\INC
      - %SOC_ROOT%\BREN\INC
      - %SOC_ROOT%\SRC
      - %KAUAI_ROOT%\SRC
    - UNICODE: If set to non-empty, build Unicode instead of ANSI
    - TYPE: Sets build type
      - DAY: Debug, incremental (daily?)
      - HOME: Debug, not incremental
      - SHIP: Release
      - DBSHIP: Release, with linker debug output
    - BLD_TYPE_DIR: Name of directory under OBJ to place binaries. **This will be set automatically by the Kauai makefiles.**
      - WINS for ANSI release builds
      - WINUS for Unicode release builds
      - WIND for ANSI debug builds
      - WINUD for Unicode debug builds
    - CHIP: Set to use optimized assembly language implementations of some functions in Kauai
      - IN_80386: Intel 80386
      - MC_68020: Motorola 68020 (for Macintosh)
      - If not set, just use the C implementation
- Locate and place font files (see [FONTS.md](FONTS.md))
- Build Kauai
  - `cd %SOC_ROOT%\kauai`
  - `nmake all`
- Build 3D Movie Maker
  - `cd %SOC_ROOT%`
  - `nmake all`
- Create a distribution directory that will be usable as-is
  - `nmake dist`
- Now you can run 3DMOVIE.EXE in the "dist" subdirectory and it should run without errors.
- Optionally, generate a release binary in dist, using the current version number in version.def (requires a "7z" binary in the path)
  - `nmake zip`

### CMake Building Instruction

With this current CMake change, CMake 3.23 and Visual Studio 2022 are required.

To setup an environment quickly, one can install
[VCVars](https://github.com/bruxisma/VCVars) for powershell and use

```console
$ pushvc (invoke-vcvars -TargetArch x86 -HostArch AMD64)
```

to enable the environment. (To remove the environment simply call `popvc` afterwards

As of right now it's only safe to target x86, which means using a cross
compiler environment in conjunction with CMake. Using Ninja is an option via
configure presets:

```console
$ cmake --preset x86:msvc:debug
$ cmake --build build
```

This should generate the `3dmovie.exe` file with little to no issue as long as
your environment is setup correctly.

The CMake project *does not* currently setup a correct install, nor does it
show files inside of visual studio correctly (This will be added later)

### Known Issues

- Compilation of `SITOBREN.EXE` is disabled
  - This requires the SoftImage SDK "DKIT" to compile


## Contributing

The source files in this repo are for historical reference and will be kept static, and this repository will
be archived. Feel free to fork this repo and experiment.

## Code cleanup

This code was restored from the Microsoft corporate archives and cleared for release.

- Developer names and aliases were removed, with the exception of current employees who worked on the
  original release who consented to keeping their names in place
- The archive consisted of several CDs, some of which were for alternate builds or products, and
  have been excluded
- The code does not build with today's engineering tools, and is released as-is.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.

This repo includes a build from 1995 of BRender from Argonaut software. Approval to open source BRender as MIT was given in an email from Jez San, former CEO of Argonaut. Other versions of BRender exist at https://github.com/foone/BRender-v1.3.2 and https://github.com/foone/BRender-1997 Thanks to Jez and the whole BRender team for their hard work on this amazing engine. A full historical list of BRender contributors is available at https://github.com/foone/BRender-v1.3.2/blob/main/README.md 

This repo does NOT include the SoftImage SDK "./DKIT" from 1992.

Jez also offered this interesting BRender anecdote in an email:

```
When Sam Littlewood designed BRender, he didn’t write the code. And then document it.  
The way most things were built at the time.
First, he wrote the manual.  The full documentation
That served as the spec.  Then the coding started.
```


