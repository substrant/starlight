**THIS IS A DRAFT BRANCH. DO NOT INSTALL STARLIGHT FROM HERE.**

# Starlight

![Downloads](https://img.shields.io/github/downloads/Substrant/Starlight/total)
[![CodeFactor](https://www.codefactor.io/repository/github/substrant/starlight/badge)](https://www.codefactor.io/repository/github/substrant/starlight)
[![Build status](https://ci.appveyor.com/api/projects/status/c8yjgsuwky78ai42?svg=true)](https://ci.appveyor.com/project/RealNickk/starlight)

An open-sourced Roblox multi-tool.

## Installation

**Starlight may be falsely flagged by your antivirus. If your antivirus is deleting Starlight, you'll have to make an exception rule.**

<details>
<summary>Creating an exception for Starlight (Windows Defender)</summary>
<br>
If Starlight is being flagged/deleted by Windows Defender, you can quite easily make an exception.

</details>

### Powershell

You can install Starlight with a single command line entry using Powershell.

1. Hit <kbd>Win</kbd> + <kbd>R</kbd>
1. Type in "powershell.exe" and press enter.
1. Paste the following entry into the command line:
    ```powershell
    Invoke-WebRequest "https://get-starlight.substrant.dev" | Invoke-Expression
    ```

### Portable Binaries

Portable installations of Starlight do not have the capability of automatically updating. Installation through Powershell is highly recommended.

Official releases of Starlight can be found on the [releases](https://github.com/Substrant/Starlight/releases) page.

Experimental builds of Starlight can be found on [AppVeyor](https://ci.appveyor.com/project/RealNickk/starlight/history), where you can download build artifacts.

## Support and Issues

**Starlight does not have a Discord server.**

If there is an issue with Starlight on your end, or you want to provide enhancement ideas for Starlight, please write an issue! When making an issue, please provide as much information as possible—reproduction steps, detailed examples. Don't be shy if it's your first issue—you're just talking to another person; there's no need to speak formally.

## Contribution

Contribution to Starlight allowed and actively encouraged. When making pull requests, ensure that the new code follows the code style and is tested. Failure to do so will result in a postpone of the merge until the issues are fixed.

## Disclaimers

Starlight is open-sourced software. The binaries that are distributed regarding Starlight may be flagged by an antivirus. Digital licenses for signing executable files are quite expensive, and we can't afford it. Antivirus programs are very unreliable and sometimes make zero sense. Starlight is not malware, and you can check that yourself by reverse engineering the binaries, which aren't obfuscated, but packed. You can also check the source code on GitHub and compile from the source. Releases of Starlight from Substrant are only available at GitHub, AppVeyor, or a Substrant-owned domain. Anywhere else Starlight is downloaded is not a safe source.

Licenses provided with other software that modify Roblox—Synapse X, Script-Ware, etc.—may be compatible with Starlight. However, their licenses may not allow third party tools to modify Roblox along with their software.

Roblox's terms of service states that reverse engineering software provided by Roblox and the attempt to bypass security measures put in place by Roblox violates their terms of service. When using Starlight, you are virtually breaking Roblox's rules.

Starlight supports user-made plugins to improve the software without having to contribute to the repository. Plugins not released under Substrant are not maintained by Substrant and may me malicious. It is your responsibility as a client to ensure that a plugin is safe—which does **NOT** mean to scan it with an antivirus. Ensure that the source of a plugin is safe with software like dotPeek or dnSpy, as plugins have the ability to execute arbitrary code.

## Legal

Starlight is provided as open-source software. To allow for flexible use, Starlight is licensed under the BSD 3-Clause license. To view the terms of the license, see [LICENSE](LICENSE).
