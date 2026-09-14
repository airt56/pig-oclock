param([string]$OutputPath = '.\dist\PigLiftClock.exe')
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (!(Test-Path -LiteralPath $compiler)) { throw '需要 Windows .NET Framework 4.x 编译器' }
if ([System.IO.Path]::IsPathRooted($OutputPath)) { $outputFullPath = [System.IO.Path]::GetFullPath($OutputPath) } else { $outputFullPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot $OutputPath)) }
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($outputFullPath)) | Out-Null
& $compiler /nologo /target:winexe /optimize+ /codepage:65001 /win32icon:src\app.ico "/out:$outputFullPath" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Core.dll src\Core.cs src\Artwork.cs src\MainForm.cs src\Program.cs
if ($LASTEXITCODE -ne 0) { throw '编译失败' }
Write-Output "Build succeeded: $outputFullPath"