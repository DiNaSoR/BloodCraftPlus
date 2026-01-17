param(
  [switch]$Check
)

$ErrorActionPreference = 'Stop'

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$templatePath = Join-Path $root 'src/index.template.html'
$outputPath = Join-Path $root 'index.html'

function Expand-Includes {
  param(
    [Parameter(Mandatory = $true)][string]$Text,
    [Parameter(Mandatory = $true)][string]$BaseDir
  )

  $pattern = '<!--\s*@include\s+(?<path>[^ ]+)\s*-->'
  $expanded = $Text

  while ($true) {
    $match = [regex]::Match($expanded, $pattern)
    if (-not $match.Success) {
      break
    }

    $relativePath = $match.Groups['path'].Value.Trim()
    $includePath = Join-Path $BaseDir $relativePath

    if (-not (Test-Path -LiteralPath $includePath)) {
      throw \"Include not found: $includePath\"
    }

    $includeText = [System.IO.File]::ReadAllText($includePath)
    $includeExpanded = Expand-Includes -Text $includeText -BaseDir (Split-Path -Parent $includePath)

    $expanded =
      $expanded.Substring(0, $match.Index) +
      $includeExpanded +
      $expanded.Substring($match.Index + $match.Length)
  }

  return $expanded
}

if (-not (Test-Path -LiteralPath $templatePath)) {
  throw \"Template not found: $templatePath\"
}

$template = [System.IO.File]::ReadAllText($templatePath)
$built = Expand-Includes -Text $template -BaseDir (Split-Path -Parent $templatePath)

if ($Check) {
  if (-not (Test-Path -LiteralPath $outputPath)) {
    throw \"Output not found: $outputPath\"
  }

  $existing = [System.IO.File]::ReadAllText($outputPath)
  if ($existing -ne $built) {
    throw \"Out of date: run build.ps1 to regenerate $outputPath\"
  }

  Write-Host 'OK'
  exit 0
}

[System.IO.File]::WriteAllText($outputPath, $built, $utf8NoBom)
Write-Host \"Wrote $outputPath\"
