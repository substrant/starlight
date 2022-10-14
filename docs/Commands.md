# Commands

This is not a guide, rather documentation. This is a list of all commands and their arguments for Starlight's command line interface.

## `help`

Description: Displays a list of commands, or information about a specific command.\
Usage: `help [command]`

## `debug`

Description: Saves a log file after the command finishes running. Must be the first verb provided.\
Usage: `Starlight.Cli.exe debug <verb> [options]`\
Example: `Starlight.Cli.exe debug launch -t <token> -p 606849621`

This is provided for debugging purposes. It will save a log file to the `Logs` directory after the command finishes running. This is useful for debugging issues with the CLI.

## `hook`

Description: Hooks the `roblox-player` scheme.\
Usage: `Starlight.Cli.exe hook`

## `unhook`

Description: Unhooks the `roblox-player` scheme.\
Usage: `Starlight.Cli.exe unhook`

## `launch`

Description: Launches a Roblox game.\
Usage: `Starlight.Cli.exe launch -t <token> -p <placeId> [-j <jobId>] [launchOptions]`\
Example: `Starlight.Cli.exe launch -t <token> -p 606849621 -j 883685fd-6bbb-409a-a2ae-031b46b93ac6`

Guide: [Launching via CLI](GettingStarted.md#launching-via-cli)

### Options

- `-t, --token <token>`: The authentication token to use. This can be obtained from the `ROBLOSECURITY` cookie.
- `-p, --placeid` - The place ID of the game to launch.
- `-j, --jobid` - The job ID (server instance ID) to launch.

## `launchraw`

Description: Launches a Roblox game using a `roblox-player` scheme payload.\
Usage: `Starlight.Cli.exe launchraw <payload>`\
Example: `Starlight.Cli.exe launchraw roblox-player:1+launchmode:play+gameinfo:YOUR_AUTHENTICATION_TICKET+launchtime:LAUNCH_TIME+placelauncherurl:https%3A%2F%2Fassetgame.roblox.com%2Fgame%2FPlaceLauncher.ashx%3Frequest%3DRequestGame%26browserTrackerId%TRACKER_ID%26placeId%3D606849621%26isPlayTogetherGame%3Dfalse+browsertrackerid:TRACKER_ID+robloxLocale:en_us+gameLocale:en_us+channel:+LaunchExp:InApp`

This command is not recommended to be used. It is only provided for debugging purposes.

## `install`

Description: Installs Roblox.\
Usage: `Starlight.Cli.exe install [-h <hash>]`\
Example: `Starlight.Cli.exe install -h da93e2c4e15845b1`

The latest Roblox hash can be found at [https://setup.rbxcdn.com/version.txt](https://setup.rbxcdn.com/version.txt), and the history of releases can be found at [https://setup.rbxcdn.com/DeployHistory.txt](https://setup.rbxcdn.com/DeployHistory.txt).

### Options

- `-h, --hash` - The hash of the Roblox version to install. Defaults to the latest version.

## `uninstall`

Description: Uninstalls Roblox.\
Usage: `Starlight.Cli.exe uninstall -h <hash>`\

### Options

- `-h, --hash` - The hash of the Roblox version to uninstall. This option is required.

## `unlock`

Description: Unlocks Roblox's singleton mutex. This allows for multiple clients to run simultaneously.\
Usage: `Starlight.Cli.exe unlock -e`

### Options

- `-e, --relock` - Locks the mutex when all Roblox clients close.

