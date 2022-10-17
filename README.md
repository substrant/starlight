# Starlight

![Downloads](https://img.shields.io/github/downloads/Substrant/Starlight/total)
![Open Issues](https://img.shields.io/github/issues-raw/Substrant/Starlight)
![Open Issues](https://img.shields.io/github/issues-closed-raw/Substrant/Starlight)

C# implementation of the Roblox launcher. This launcher has the full
capabilities of the official launcher. It's agile, trackerless, and has many
more features that the official launcher lacks. This may be the 2nd rewrite of
Starlight, but it's the first one that's actually good, and finally out of
early development hell.

Sources:
* [Roadmap](Roadmap.md)
* [Documentation](docs/Home.md)

## Safety
Starlight should work with any Roblox exploit that doesn't hook the launch
scheme. If you're using a Roblox exploit that hooks the launch scheme, you
will have to turn off the hooking feature in that specific exploit.

Exploits like Synapse X are known to hook the launch scheme, and may not work
simultaneously with Starlight. You may have to explicitly disable the custom
launcher in your exploit's settings.

This program does not inject any DLLs, however it does modify the Roblox
task scheduler from an external process. This is done to set the FPS cap. This
shouldn't be a problem, but I thought I'd throw that out. It should be
undetectable by Roblox's anti-cheat.

## Issues and Support
**Starlight does not have a Discord server.**

If you have any issues with Starlight, please open an issue on the GitHub
repository. I will try to respond as soon as possible. Please include as much
information as possible, including screenshots and logs. If you're not being
very descriptive, I may not be able to help you.

## Contributing
If you want to contribute, please make sure to follow the code style of the
project. There's not much to it, but it's important to keep it consistent. I
don't have any more rules, so as long as that's followed, you're good to go.

## License
Starlight is licensed under the BSD 3-Clause license. See [LICENSE](LICENSE) for
more information. I probably won't enforce it unless you're being a jerk by
saying you made 100% of it or something along the lines of that.