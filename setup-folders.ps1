$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "        KeyForge Folder Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$Root = Get-Location

$Folders = @(
    # --------------------------------------------------------
    # Application / Features
    # --------------------------------------------------------

    "src\KeyForge\Features\Exercises\Models",
    "src\KeyForge\Features\Exercises\Services",
    "src\KeyForge\Features\Exercises\Components",

    "src\KeyForge\Features\Lessons\Models",
    "src\KeyForge\Features\Lessons\Services",
    "src\KeyForge\Features\Lessons\Components",

    "src\KeyForge\Features\Practice\Models",
    "src\KeyForge\Features\Practice\Services",
    "src\KeyForge\Features\Practice\Components",

    "src\KeyForge\Features\Progress\Models",
    "src\KeyForge\Features\Progress\Services",

    "src\KeyForge\Features\Midi\Models",
    "src\KeyForge\Features\Midi\Services",

    # --------------------------------------------------------
    # Infrastructure
    # --------------------------------------------------------

    "src\KeyForge\Infrastructure\Yaml\Parsing",
    "src\KeyForge\Infrastructure\Yaml\Validation",
    "src\KeyForge\Infrastructure\Midi",
    "src\KeyForge\Infrastructure\Persistence",

    # --------------------------------------------------------
    # Content
    # --------------------------------------------------------

    "src\KeyForge\Content\Lessons",
    "src\KeyForge\Content\Exercises",

    # --------------------------------------------------------
    # Tests
    # --------------------------------------------------------

    "tests\KeyForge.Tests\Features\Exercises",
    "tests\KeyForge.Tests\Features\Lessons",
    "tests\KeyForge.Tests\Features\Practice",
    "tests\KeyForge.Tests\Features\Progress",
    "tests\KeyForge.Tests\Features\Midi",

    "tests\KeyForge.Tests\Infrastructure\Yaml"
)

foreach ($RelativePath in $Folders) {

    $FullPath = Join-Path $Root $RelativePath

    if (-not (Test-Path $FullPath)) {

        New-Item `
            -ItemType Directory `
            -Path $FullPath `
            -Force | Out-Null

        Write-Host "[CREATED] $RelativePath" -ForegroundColor Green
    }
    else {

        Write-Host "[EXISTS ] $RelativePath" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Folder structure created successfully." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""