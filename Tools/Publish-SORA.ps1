# Run once AFTER creating an EMPTY public repo named SORA-After-the-Rain
# Requires Git for Windows and authenticated GitHub access (browser / Git Credential Manager).
param([string]$RemoteUrl = "https://github.com/haizzdungnay/SORA-After-the-Rain.git")
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Install Git for Windows before running this script.' }
if (-not (Test-Path .git)) {
    git init -b main
    if ($LASTEXITCODE -ne 0) { throw 'git init failed.' }
}
if (-not (git remote)) {
    git remote add origin $RemoteUrl
    if ($LASTEXITCODE -ne 0) { throw 'Could not add git remote.' }
}
$remote = git remote get-url origin
if ($remote -ne $RemoteUrl) { throw "Remote origin is $remote (expected $RemoteUrl). Refusing to change it." }
# This helper is for the first import into an EMPTY repository only.
$existingRefs = git ls-remote --heads origin
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect remote branches; refusing to publish.' }
if ($existingRefs) { throw 'Repository already has branches. Use a feature branch and Pull Request; this bootstrap helper will not update main.' }
$branch = git branch --show-current
if ($branch -ne 'main') { throw 'Bootstrap expects a new local main branch.' }
# Add only files permitted by .gitignore.
git add .
if ($LASTEXITCODE -ne 0) { throw 'git add failed.' }
$staged = git diff --cached --name-only
if (-not $staged) { Write-Host 'No new changes to commit'; exit 0 }
git commit -m 'chore: bootstrap SORA Unity prototype and GDD'
if ($LASTEXITCODE -ne 0) { throw 'Commit failed. Configure git user.name/user.email and retry.' }
git push -u origin main
if ($LASTEXITCODE -ne 0) { throw 'Push failed. Confirm the empty repo exists and your account is authenticated.' }
Write-Host 'SORA source pushed to GitHub successfully.'
