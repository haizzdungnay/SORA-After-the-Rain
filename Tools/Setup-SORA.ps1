# SORA: After the Rain - Windows setup helper
# Run from the unzipped project root in PowerShell:
#   powershell -ExecutionPolicy Bypass -File .\Tools\Setup-SORA.ps1
#   powershell -ExecutionPolicy Bypass -File .\Tools\Setup-SORA.ps1 -InstallEditor
param(
    [switch]$InstallEditor,
    [switch]$OpenProject,
    [string]$UnityExe = ""
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = '6000.3.7f1'
$hub = 'C:\Program Files\Unity Hub\Unity Hub.exe'
Write-Host "SORA Project: $root"
Write-Host "Target Unity Editor: $version (Unity 6.3 LTS)"

if (-not (Test-Path $hub)) {
    Write-Warning 'Unity Hub was not found in the default folder.'
    Write-Host 'Official download: https://unity.com/download'
    Write-Host 'Install Unity Hub, log in with YOUR Unity ID and activate YOUR license in Hub.'
    if ($InstallEditor) { throw 'Install Unity Hub before using -InstallEditor.' }
} else {
    Write-Host "Unity Hub found: $hub" -ForegroundColor Green
    if ($InstallEditor) {
        Write-Host 'Installing Unity Editor via the official Hub CLI (experimental)...'
        & $hub -- --headless install --version $version
        if ($LASTEXITCODE -ne 0) { throw "Hub editor installation failed with exit code $LASTEXITCODE. Install this version in the Hub GUI instead." }
    }
}
if (-not $UnityExe) {
    $candidate = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
    if (Test-Path $candidate) { $UnityExe = $candidate }
}
if ($UnityExe -and (Test-Path $UnityExe)) {
    Write-Host "Editor found: $UnityExe" -ForegroundColor Green
    if ($OpenProject) {
        Write-Host 'Opening SORA in Unity Editor...'
        Start-Process -FilePath $UnityExe -ArgumentList @('-projectPath', ('"' + $root + '"'))
    }
} else {
    Write-Warning "Unity Editor $version not found at its default path."
    Write-Host 'Open Unity Hub > Installs, install version 6000.3.7f1 and Windows Build Support.'
    Write-Host 'In Unity Hub > Projects > Add project from disk, select this unzipped project root.'
}
Write-Host 'Next: Open the project, wait for package import, then SORA > 1. Generate Demo Scene > Play.'
Write-Host 'Before gameplay, set Active Input Handling to Both or Input Manager (Old) if needed.'
