# Building Starlight

Say you downloaded the source, or modified Starlight. How do you build it? This guide will show you how to build Starlight.

## Prerequisites

You will need a couple of things to build Starlight:
- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- [Git](https://git-scm.com/downloads) (optional)
- .NET Framework 4.8 build tools (check the box for it in Visual Studio Installer)

## Building

To build Starlight, you will need to clone the repository. You can do this by either downloading the source code as a ZIP file, or by cloning the repository with Git. If you're using Git, you can clone the repository with the following command:

```batch
git clone https://github.com/Substrant/Starlight.git
```

Once you have the source code, you can open the solution in Visual Studio. You can do this by double clicking `src/Starlight.sln`, or opening it in Visual Studio. Once you have the solution open, set your build targets to your preference, and build the solution by right clicking the solution in the Solution Explorer, and clicking Build. The binaries will be compiled into `src/Starlight/bin/`.