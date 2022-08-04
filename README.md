# Starlight
A rewrite of the Roblox launcher that is able to launch and bootstrap Roblox.\
This code is licensed under the GNU General Public License v3, so don't steal my code unless you use a compatible license.

## Features
- Roblox installation
- Headless mode (Clients without a window)
- Tracker ID spoofing
- Launching specific versions of Roblox

## Usage
For help do, `starlight --help`.

### Launching Games
To launch a game, you need to do `starlight launch [launchOptions] --payload <launchPayload>`.\
The payload is the `roblox-player:1+gameinfo:...` link you see inside the network tab in your browser when you launch Roblox. Soon I'll make a more user-friendly way of launching the game but that's what I have for you right now.

### Launch Options
- `--payload`: The information needed to launch the game.
- `--headless`: If enabled, the client will open, but the window will not show, which means no rendering is done on the client. Exploits will still work. This is mainly used for botting.
- `--no-spoof`: If enabled, the launcher will not spoof the joining tracker ID.
- `--git-hash`: The hash (`version-<hash>`) of Roblox to launch.
- `--strict`: If enabled, the launcher will not install Roblox and will instead exit with code 1 if there is no existing version of Roblox installed.

### Hooking the `roblox-player` Scheme
To hook the launch scheme, you can do `starlight hook [launchOptions]`. To unhook, just do `starlight unhook`. To unhook, a valid installation of Roblox must exist. Refer to the Launch Options section for information. If you are hooking, the `--payload` option is autofilled and should not be provided.

### Installing Roblox
To install Roblox, do `starlight install [installOptions]`. There is currently no support for uninstalling Roblox.

### Install Options
- `--git-hash` The hash of Roblox to install.

## Building
To build Starlight, first clone the repository:
```bash
git clone https://github.com/RealNickk/Starlight-Launcher.git
```
After cloning, open `Starlight-Launcher/Starlight.sln` in Visual Studio, make sure you're in the `Release` build mode, then right click the solution and click `Build Solution`. The binaries will be in `Starlight-Launcher/bin`.

Made by Nick stolen by Pepsi
