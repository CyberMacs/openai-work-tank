$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (Test-Path (Join-Path $root 'Final\OpenAIWorkTank.exe'))) { throw 'Előbb futtasd a build.ps1 parancsot.' }
$output = Join-Path $root ('installer-publish-' + [guid]::NewGuid().ToString('N'))
dotnet publish (Join-Path $root 'Installer\OpenAIWorkTank.Installer.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $output
$built = Join-Path $output 'OpenAIWorkTank-Setup.exe'
if (-not (Test-Path $built)) { throw 'A telepítő .exe nem készült el.' }
Copy-Item $built (Join-Path $root 'Final\OpenAIWorkTank-Setup.exe') -Force
Write-Host 'Kesz telepito:' (Join-Path $root 'Final\OpenAIWorkTank-Setup.exe')
