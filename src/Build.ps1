$ErrorActionPreference = 'Stop'
$assemblyInfo = Join-Path $PSScriptRoot 'AssemblyInfo.cs'
$sources = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' -File -Recurse |
    Where-Object { $_.Name -ne 'AssemblyInfo.cs' } |
    Sort-Object FullName |
    ForEach-Object { $_.FullName })
$projectRoot = Split-Path -Parent $PSScriptRoot
$output = Join-Path $projectRoot 'Portable'
# Prefer the 64-bit compiler; fall back to the standard Framework compiler when needed.
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework C# compiler was not found.' }
New-Item -ItemType Directory -Force -Path $output | Out-Null
$exe = Join-Path $output 'ClipCourier.exe'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /out:$exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Runtime.Serialization.dll $assemblyInfo $sources
if ($LASTEXITCODE -ne 0) { throw "Compilation failed with exit code $LASTEXITCODE" }
# Ship the license beside the portable executable so the release archive is self-contained.
Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination (Join-Path $output 'LICENSE') -Force
Write-Host "Built $exe"
