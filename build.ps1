$runnerPath = Join-Path $PSScriptRoot "runner"
if (!(Test-Path $runnerPath)) {
    New-Item -ItemType Directory -Path $runnerPath
}

# Install dependencies
if (!(where msbuild.exe)) {
    # install .net 6 sdk (x86)
}

if (!(where nuget.exe)) {
    Write-Host "Installing NuGet..."
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile (Join-Path $runnerPath "nuget.exe")
}

$env:Path = "$($runnerPath);$($env:Path)"

nuget.exe restore Starlight.sln
msbuild.exe Starlight.sln -p:Configuration=Release