# Metadata (don't change unless fork)
$repo = "Substrant/Starlight";
$packageName = "Starlight_win32.zip";

# Gets the latest release
function Get-ReleaseData {
	if ($Global:relData -eq $null) {
		$Global:relData = Invoke-WebRequest "https://api.github.com/repos/$($args[0])/releases/latest" | ConvertFrom-Json;
	}
	return $Global:relData;
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
		if ($res -eq "y" -or $res -eq "\n") {
			return $true;
		} elseif ($res -eq "n") {
			return $false;
		} else {
			Write-Host "Invalid response. Please try again.";
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

# Construct an installation path
$installPath = if ($args[0]) { $args[0] } else { "$($env:LOCALAPPDATA)\Starlight" };

# Get the latest release data
$getFailed = $false;
try {
	$data = Get-ReleaseData $repo;
	Write-Host "Latest release: $($data.tag_name). Downloading...";
} catch {
	$getFailed = $true;
} finally {
	if ($getFailed -or $data -eq $null) {
		Write-Host "Failed to get latest release data. Aborting." -ForegroundColor Red;
		exit;
	}
}

# Get the asset
$package = Get-ReleaseAsset $data $packageName;
if ($package -eq $null) {
	Write-Host "Failed to get asset. Aborting." -ForegroundColor Red;
	exit;
}

# Download the asset to a temporary location then unzip the package and delete the compressed file
$packagePath = Download-Asset $package;
Expand-Archive -Path $packagePath -DestinationPath $installPath -Force;
Remove-Item $packagePath -Force;
Write-Host "Starlight has been downloaded to $installPath";

# Create shortcuts
if (Send-Prompt "Create desktop shortcut?") {
	Create-Shortcut "$($env:USERPROFILE)\Desktop\Starlight.lnk" "$installPath\Starlight.Gui.exe";
}
if (Send-Prompt "Create start menu shortcut?") {
	Create-Shortcut "$($env:APPDATA)\Microsoft\Windows\Start Menu\Programs\Starlight.lnk" "$installPath\Starlight.Gui.exe";
}

Write-Host "Installation complete! Starlight can be found at $installPath." -ForegroundColor Green;