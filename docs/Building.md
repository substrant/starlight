# Building

Say you downloaded the source, or modified Starlight. How do you build it? This guide will show you how to build Starlight.

## Building With Visual Studio

### Prerequisites

You will need a couple of things to build Starlight:
- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- [Git](https://git-scm.com/downloads) (optional)
- .NET Framework 4.8 build tools (check the box for it in Visual Studio Installer)

### Building

To build Starlight, you will need to clone the repository. You can do this by either downloading the source code as a ZIP file, or by cloning the repository with Git. If you're using Git, you can clone the repository with the following command:

```bash
git clone https://github.com/Substrant/Starlight.git --recursive
```

You have to recursively clone the repository, because Starlight uses submodules. If you don't, you will get an error when you try to build the solution. If you're using the ZIP file, you will have to download the submodules manually.

Once you have the source code, you can open the solution in Visual Studio. You can do this by double clicking `Starlight.sln`, or selecting the file in Visual Studio. Once you have the solution open, set your build targets to your preference, and build the solution by right clicking the solution in the Solution Explorer, and clicking Build. It's recommended that you build in `Release` mode. The binaries will be compiled into the `bin` folder.

## Building With The Command Line (Windows)

### Prerequisites

You will need a couple of things to build Starlight:
- [Git](https://git-scm.com/downloads)
- [MSBuild](https://visualstudio.microsoft.com/downloads/) (may come with Visual Studio but needs to be in your PATH)
- [NuGet](https://www.nuget.org/downloads) (or `dotnet` CLI)
- [.NET Framework 4.8 developer pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)

### Building

Follow the instructions states in the previous section to clone the repository. Once you have the source code, you can build the solution by running the following command:

```bash
msbuild Starlight.sln /p:Configuration=Release
```

This will build the solution in `Release` mode. The binaries will be compiled into the `bin` folder.