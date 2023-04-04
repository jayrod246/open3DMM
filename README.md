# 3DMMForever

_Making 3D Movie Maker accessible to all and preserving it for generations to come._

![3D Movie Maker](img/3dmovie.jpg?raw=true)

## Goals

Our mission is to create a version of the original 3D Movie Maker software with these goals in mind:

- Includes the original feature set, 640x480 UI, kidspace, and graphics.
- Builds with modern open source tools.
- Runs on multiple additional platforms including MacOS and Linux.
- Ensure movie files produced in 3DMMForever can play in the original 3DMM.
  - Reduced quality is OK.
- Ensure movie files produced with the original 3DMM can playback in true form.
- Integrates [v3dmm](https://twitter.com/Foone/status/1307750230679412736).
- Enhancements remain light and preserve backwards compatibility.
  - Heavier enhnacements will be reserved for [3DMMPlus](#3dmmplus).

## About

Released by Microsoft in 1995, 3D Movie Maker (3DMM) is a creativity program originally designed for kids that allows users to create 3D animated movies through a simple user interface using a wide assortment of included scenes, 3D models, sounds and music. Users can place, animate and otherwise manipulate 3D models using simple mouse movements and drags. They can also record and import their own sound files. Finished movies can be saved and shared with others. The program also includes a "kidspace" in the form a movie theater that can be navigated around where a user can find movie making tutorials, and inspiration.

In May 2022, Microsoft released the original source code of 3DMM under the [MIT license](LICENSE) as open source. Which is how 3DMMForever became possible!

## Build instructions

CMake 3.23 and Visual Studio 2022 are required.

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

3DMMForever and 3DMMPlus will not be possible without an enthusiastic open source developer community backing it.

We're working on a set of contribution guidelines that we will be using going forward.

## 3DMMPlus

3DMMPlus is a future fork of 3DMMForever that will be created when we've finished the porting work to MacOS and Linux. The sky will be the limit with 3DMMPlus! Our mission will be to create a heavily enhanced version of 3DMM that:

- Has a flexible full color UI that looks great at modern resolutions.
- Has a modern full color 3D renderer with lightning, shading, moveable camera, and more.
- Produces a new enhanced file type.
- Can import 3MM files and play them back as they were originally created.
- Maintains a strong consideration for and familiarity with the original 3DMM’s UI decisions and approach.

## Legal stuff

The following sections have been carried over from the original 3D Movie Maker [GitHub repository](https://github.com/microsoft/Microsoft-3D-Movie-Maker) released by Microsoft in May 2022.

### Code cleanup

This code was restored from the Microsoft corporate archives and cleared for release.

- Developer names and aliases were removed, with the exception of current employees who worked on the
  original release who consented to keeping their names in place
- The archive consisted of several CDs, some of which were for alternate builds or products, and
  have been excluded
- The code does not build with today's engineering tools, and is released as-is.

### Trademarks

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
