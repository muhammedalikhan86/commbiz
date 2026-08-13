# .github/scripts/graphify-update.ps1
#
# Runs the Graphify knowledge-graph update (graphify_update_run.py), scoped to src/,
# merging AST-extracted nodes/edges into graphify-out/graph.json and regenerating
# graphify-out/GRAPH_REPORT.md. Run from the repository root - the Python script's
# paths (src/, graphify-out/) are relative to the caller's cwd, not this script's
# location.

$ErrorActionPreference = "Stop"

function Find-Python {
    $cmd = Get-Command python -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $cmd = Get-Command python3 -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

$python = Find-Python
if (-not $python) {
    Write-Error "python not found on PATH - install Python 3, then re-run this script."
    exit 1
}

$scriptPath = Join-Path $PSScriptRoot "graphify_update_run.py"
if (-not (Test-Path $scriptPath)) {
    Write-Error "graphify_update_run.py not found next to this script at $scriptPath"
    exit 1
}

& $python $scriptPath
exit $LASTEXITCODE
