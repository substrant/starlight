# Getting Started With Starlight

You're interested in using Starlight, but you don't know where to start? This guide will help you get started with Starlight.

## Installation

Starlight is avialable for Windows. You can download the latest version from the [releases page](https://github.com/Substrant/Starlight/releases). if you want to ensure that the binaries are from me, you can check if I've signed the installer with my [PGP key](https://keybase.io/realnickk). You can also build Starlight from source, which is specified in [Building.md](Building.md).

Once you download the binaries, you need to extract them to a folder. Make sure that you don't run it from the archive, as that's not safe. Extract Starlight to a directory that won't change, such as `C:\Starlight` or `C:\Users\nick\Starlight`.

## Running Starlight

Once you've extracted Starlight, you can run it. You can access Starlight's CLI by opening your command prompt, changing your directory to the Starlight folder, and running `Starlight.Cli.exe`. You can also run Starlight from the GUI by running `Starlight.Gui.exe`.

When you first launch Starlight, it'll make a file called `GlobalLaunchSettings.json` in its base directory. That is your configuration file for launching through Roblox's scheme. You can modify it to your liking. A tutorial will also pop up, giving you a quick explaination of how the launcher works, and how to use it. The tutorial won't show up again unless you delete the `GlobalLaunchSettings.json` file.

More information on the command line interface can be found in [Commands.md](Commands.md).

## Launching via Hooking

Starlight can hook into Roblox's `roblox-player` scheme, which means that it can hook Roblox's launching from the browser. You can hook into Roblox by running `Starlight.Cli.exe hook`. If you want to undo that, you can unhook by running `Starlight.Cli.exe unhook`.

An video overview of launching from the browser is shown below:\
[![YouTube Video](https://img.youtube.com/vi/AXArOcOWNbU/mqdefault.jpg)](https://www.youtube.com/watch?v=AXArOcOWNbU).

## Launching via CLI

You can launch Roblox by running `Starlight.Cli.exe launch`. You will need a couple of arguments to launch Roblox:
- `-p, --placeid <long>` - The place ID to launch
- `-t, --token <string>` - The auth token (`.ROBLOSECURITY`) to authenticate with.

You can get your authentication token by going to your browser, right clicking and opening inspect element, going to the `Application` tab, and then going to `Cookies`. You can then copy the value of `.ROBLOSECURITY`--your authentication token.

Your place ID can be found in the URL of the game. For example, the place ID of [https://www.roblox.com/games/606849621](https://www.roblox.com/games/606849621) is `606849621`.

There are other optional arguments that you can use:
- `-j, --jobid <string>` - The job ID (server instance ID) to launch
- `-s, --spoof` - Spoof your browser tracking ID.
- `-h, --hash <string>` - The hash of Roblox to launch.
- `-r, --res <string>` - The resolution to launch Roblox in. The format is `WIDTHxHEIGHT`, so for example: `800x600`. If provided, Roblox's window border will be removed.
- `--fps-cap <int>` - The FPS cap to launch Roblox with. The default is zero, which means that Starlight will use Roblox's default FPS cap. If you provide a value, Starlight will set the FPS cap to that value. There is no support for setting the FPS cap to unlimited, so you can set your FPS cap to 1000 if you want to have an unlimited FPS cap.
- `--headless` - If this option is provided, Roblox will be launched in headless mode. This means that Roblox will not have a window, and will not be visible. This is useful for running Roblox in the background.

## Debugging

If you have an issue with Starlight and you need to make an issue, you can get logs to help me debug the issue without me having to ask you to do a bunch of hacker stuff.

If you want to view the logs of Starlight with the CLI, you can put `debug` as your first verb, for example, `Starlight.Cli.exe debug launch [options]`. This will write Starlight's logs to the `Logs` folder.

If you want to view the logs of Starlight when hooking, you can modify `GlobalLaunchSettings.json`, and set `saveLog` to `true`. This will write Starlight's logs to the `Logs` folder. You can also enable `verbose` to view more detailed logs.