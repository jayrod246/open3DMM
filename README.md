# Open3dmm

Welcome to the open source reimplementation of 3D Movie Maker project.

At its current state, the project requires you to have the original program properly installed in order to run.

# What is Open3dmmBootstrapper
This is the actual entrypoint of the program and is what you will use to launch the game. It has two tasks:
1. Ensures that the native 3dmm EXE gets loaded to the right address, allowing the native code to be called into.
2. Initializes the CLR runtime and calls our code inside Open3dmm.dll
