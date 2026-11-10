# Builds Windows x64 using a licensed and installed local Unity Editor.
# Example:
#   powershell -ExecutionPolicy Bypass -File .\Tools\Build-SORA.ps1
#   powershell -ExecutionPolicy Bypass -File .\Tools\Build-SORA.ps1 -UnityExe 'D:\Unity\6000.3.7f1\Editor\Unity.exe' -SmokeOnly
param(
    [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe',
    [switch]$SmokeOnly
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path $UnityExe)) { throw "Unity Editor not found: $UnityExe" }
$method = if ($SmokeOnly) { 'Sora.AfterTheRain.Demo.Editor.SoraDevAutomation.CompileSmokeCheck' } else { 'Sora.AfterTheRain.Demo.Editor.SoraDevAutomation.BuildWindows' }
$logs = Join-Path $root 'Logs'
New-Item -ItemType Directory -Force -Path $logs | Out-Null
$log = Join-Path $logs $(if ($SmokeOnly) { 'smoke-check.log' } else { 'windows-build.log' })
Write-Host "Running $method"
Write-Host "Unity log: $log"
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $root + '"'), '-logFile', ('"' + $log + '"'), '-executeMethod', $method)
$p = Start-Process -FilePath $UnityExe -ArgumentList $arguments -NoNewWindow -PassThru
$p.WaitForExit()
if ($p.ExitCode -ne 0) {
    if (Test-Path $log) { Get-Content $log -Tail 50 }
    throw "Unity exited with code $($p.ExitCode). See $log"
}
if (-not $SmokeOnly) {
    $exe = Join-Path $root 'Builds\Windows\SORA_After_the_Rain.exe'
    if (-not (Test-Path $exe)) { throw "Unity returned code 0, but Windows EXE is missing: $exe" }
    Write-Host "SUCCESS: $exe" -ForegroundColor Green
} else {
    $scene = Join-Path $root 'Assets\SORA\Scenes\SORA_Prototype.unity'
    if (-not (Test-Path $scene)) { throw "Smoke check did not generate expected scene: $scene" }
    Write-Host "SUCCESS: Unity import and scene generator completed: $scene" -ForegroundColor Green
}
