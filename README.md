# Starlight
C# implementation of the Roblox launcher. This launcher has the full
capabilities of the official launcher, and is designed to be a drop-in
replacement. It's fast, logless, and has many features that the official
launcher lacks such as an FPS unlocker.

Adding on top of speed, Starlight has zero unnecessary features. It's
designed to be a simple, lightweight launcher that does what it needs to do and
nothing more. It's designed to be anonymous, and includes features to help
protect your privacy such as tracker spoofing.

## Features
- ✔️ 2x+ faster than the official launcher
- ✔️ Spoofs tracker ID (people call it "HWID spoofing" but it's not)
- ✔️ Built-in FPS unlocker/limiter
- ✔️ Multiple Roblox processes
- ✔️ Management and launching of multiple versions of Roblox
- ✔️ Headless mode (no GUI, good in combination with the FPS limiter)
- ✔️ Can be used as a library
- ✔️ Scheme hooking and handling (launching from the browser)
- ❌ Disables rendering for headless mode (planned)
- ❌ Support for Roblox Studio (not planned because useless and 64 bit)

## Safety
Starlight should work with any Roblox exploit that doesn't hook the launch
scheme. If you're using a Roblox exploit that hooks the launch scheme, you
will have to turn off the hooking feature in that specific exploit.

Exploits like Synapse X are known to hook the launch scheme, and may not work
simultaneously with Starlight. You will explicitly have to disable the custom
launcher in your exploit's settings.

This program does not inject any DLLs, however it does modify the Roblox
task scheduler from an external process. This is done to set the FPS cap. This
shouldn't be a problem, but I thought I'd throw that out. It should be
undetectable by Roblox's anti-cheat.

## Building
The only prerequisite for building Starlight is .NET Framework 4.8 build tools
and Visual Studio. You can download both from the Visual Studio Installer.

To build Starlight, simply open the solution in Visual Studio and build it. The
binaries will be compiled into `Starlight/bin`.

## Issues
If you have any issues with Starlight, please open an issue on the GitHub
repository. I will try to respond as soon as possible.

## Contributing
If you want to contribute, please make sure to follow the code style of the
project. There's not much to it, but it's important to keep it consistent. I
don't have any more rules, so as long as that's followed, you're good to go.

## License
Starlight is licensed under the GNU General Public License. See [LICENSE](LICENSE) for more
information.

I'd prefer that you don't steal my code. If you want to use it for your own
projects, please give me credit. At the least, to follow legal requirements,
you have to include the license file in your project and follow its terms.
