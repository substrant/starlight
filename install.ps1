if ($InstallPath -eq $null) {
	$InstallPath = "$($env:USERPROFILE)\.starlight";
}

if ($Uninstall -or $Update) {
	$anyRemoved = $false;

	if (Test-Path $InstallPath -PathType Container) {
		Remove-Item $InstallPath -Recurse -Force;
		$anyRemoved = $true;
	}

	$desktopShortcut = "$($env:USERPROFILE)\Desktop\Starlight.lnk";
	if (Test-Path $desktopShortcut -PathType Leaf) {
		Remove-Item $desktopShortcut -Force;
		$anyRemoved = $true;
	}

	$menuShortcut = "$($env:APPDATA)\Microsoft\Windows\Start Menu\Programs\Starlight.lnk";
	if (Test-Path $menuShortcut -PathType Leaf) {
		Remove-Item $menuShortcut -Force;
		$anyRemoved = $true;
	}

	if ($Uninstall) {
		if ($anyRemoved) {
			Write-Output "Starlight has been uninstalled.";
		} else {
			Write-Output "Starlight installation doesn't exist.";
		}
		exit;
	}
}

# Metadata (don't change unless fork)
$repo = "Substrant/Starlight";
$packageName = "Starlight_win32.zip";

# Look for metadata of last installation and decode json to object
$metadataFile = "$InstallPath\InstallationMetadata.json";
$firstInstall = !(Test-Path $metadataFile -PathType Leaf);
$metadata = if (!$firstInstall) { Get-Content $metadataFile | ConvertFrom-Json; } else { @{}; }

# Gets the latest release
function Get-ReleaseData {
	return Invoke-WebRequest "https://api.github.com/repos/$($args[0])/releases/latest" | ConvertFrom-Json;
}

# Gets an asset from a release by name
function Get-ReleaseAsset {
	$assetName = $args[1];
	$asset = $args[0].assets | Where-Object { $_.name -eq $assetName };
	return $asset;
}

# Downloads a release asset to temp
function Download-Asset {
	$assetPath = "$($env:TEMP)\$($args[0].name)";
	$assetUrl = $args[0].browser_download_url;
	Invoke-WebRequest $assetUrl -OutFile $assetPath;
	return $assetPath;
}

# Sends a yes/no prompt to the user
function Send-Prompt {
	for (;;) {
		$res = Read-Host "$($args[0]) [y/n] ";
		if ($res -eq "y" -or $res -eq "") {
			return $true;
		} elseif ($res -eq "n") {
			return $false;
		} else {
			Write-Output "Invalid response. Please try again.";
		}
	}
}

# Creats a shortcut to an executable file in the given path
function Create-Shortcut {
	$shortcut = New-Object -ComObject WScript.Shell;
	$shortcutLink = $shortcut.CreateShortcut($args[0]);
	$shortcutLink.TargetPath = $args[1];
	$shortcutLink.Save();
}

# Get the latest release data
$getFailed = $false;
try {
	$data = Get-ReleaseData $repo;
	Write-Output "Latest release: $($data.tag_name)";
} catch {
	$getFailed = $true;
} finally {
	if ($getFailed -or $data -eq $null) {
		Write-Output "Failed to get latest release data. Aborting.";
		exit;
	}
}

# Check if that version is already installed and set version to install afterwards
if ($metadata.version -eq $data.tag_name) {
	if (!(Send-Prompt "Starlight is already up to date. Do you want to reinstall Starlight?")) {
		Write-Output "Aborting.";
		exit;
	}
}
$metadata.version = $data.tag_name;

# Get the asset
$package = Get-ReleaseAsset $data $packageName;
if ($package -eq $null) {
	Write-Output "Failed to get asset. Aborting.";
	exit;
}

# Download the asset to a temporary location then unzip the package and delete the compressed file
$packagePath = Download-Asset $package;
Expand-Archive -Path $packagePath -DestinationPath $InstallPath -Force;
Remove-Item $packagePath -Force;
Write-Output "Starlight has been downloaded to $InstallPath";

# Create shortcuts
if ($firstInstall -and !$Update) {
	if (Send-Prompt "Create desktop shortcut?") {
		Create-Shortcut "$($env:USERPROFILE)\Desktop\Starlight.lnk" "$InstallPath\Starlight.Gui.exe";
	}
	if (Send-Prompt "Create start menu shortcut?") {
		Create-Shortcut "$($env:APPDATA)\Microsoft\Windows\Start Menu\Programs\Starlight.lnk" "$InstallPath\Starlight.Gui.exe";
	}
}

# Save metadata
$metadata | ConvertTo-Json | Out-File $metadataFile -Force;

Write-Output "Installation complete! Starlight can be found at $InstallPath.";