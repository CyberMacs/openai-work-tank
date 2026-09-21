param([switch]$Installer)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root
dotnet build .\OpenAIWorkTank.csproj -c Release
dotnet run --project .\Tests\OpenAIWorkTank.Tests.csproj -c Release
$output = Join-Path $root ('publish-' + [guid]::NewGuid().ToString('N'))
dotnet publish .\OpenAIWorkTank.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $output
Copy-Item (Join-Path $output 'OpenAIWorkTank.exe') .\Final\OpenAIWorkTank.exe -Force
if ($Installer) { & .\build-installer.ps1 }
Pop-Location
