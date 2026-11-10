# Run on a trusted Windows machine with Unity 6000.3.7f1 and an activated license.
param([string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path $UnityExe)) { throw "Unity Editor not found: $UnityExe" }
$results = Join-Path $root 'TestResults'
$logs = Join-Path $root 'Logs'
New-Item -ItemType Directory -Force -Path $results, $logs | Out-Null
$xmlPath = Join-Path $results 'editmode.xml'
$logPath = Join-Path $logs 'editmode.log'
if (Test-Path $xmlPath) { Remove-Item $xmlPath }
# Do not pass -quit: the Test Runner exits after tests complete.
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $root + '"'), '-runTests', '-testPlatform', 'EditMode', '-testResults', ('"' + $xmlPath + '"'), '-logFile', ('"' + $logPath + '"'))
$p = Start-Process -FilePath $UnityExe -ArgumentList $arguments -PassThru -NoNewWindow
$p.WaitForExit()
if ($p.ExitCode -ne 0) { throw "Unity tests exited with code $($p.ExitCode); inspect $logPath" }
if (-not (Test-Path $xmlPath)) { throw 'Test Runner did not produce an XML report.' }
[xml]$report = Get-Content -Raw $xmlPath
$run = $report.'test-run'
if ($null -eq $run -or [int]$run.total -lt 5 -or [int]$run.failed -gt 0 -or $run.result -ne 'Passed') {
    throw 'EditMode report is missing, incomplete, failed or inconclusive.'
}
Write-Host "PASS: $($run.passed) EditMode tests; $xmlPath"
