# ============================================================
#  Firelink — скелет solution. Без C#-кода.
#  Требует .NET SDK 8+ (.NET 10 SDK тоже подойдёт).
#  TargetFramework: net8.0.
# ============================================================

$ErrorActionPreference = 'Stop'
$root = Get-Location

Write-Host "=== Firelink setup ===" -ForegroundColor Cyan
Write-Host "Корень: $root" -ForegroundColor Gray

# ---------- 1. Solution и проекты ----------
Write-Host "`n[1/5] Solution + проекты..." -ForegroundColor Yellow

dotnet new sln -n Firelink | Out-Null

dotnet new classlib -n Firelink.Core            -o src/Firelink.Core            -f net8.0 | Out-Null
dotnet new classlib -n Firelink.Platform.MO2    -o src/Firelink.Platform.MO2    -f net8.0 | Out-Null
dotnet new classlib -n Firelink.Platform.Nexus  -o src/Firelink.Platform.Nexus  -f net8.0 | Out-Null
dotnet new classlib -n Firelink.Platform.GitHub -o src/Firelink.Platform.GitHub -f net8.0 | Out-Null
dotnet new console  -n Firelink.Pack            -o src/Firelink.Pack            -f net8.0 | Out-Null
dotnet new console  -n Firelink.Install         -o src/Firelink.Install         -f net8.0 | Out-Null

dotnet new xunit -n Firelink.Core.Tests         -o tests/Firelink.Core.Tests         | Out-Null
dotnet new xunit -n Firelink.Platform.MO2.Tests -o tests/Firelink.Platform.MO2.Tests | Out-Null
dotnet new xunit -n Firelink.Pack.Tests         -o tests/Firelink.Pack.Tests         | Out-Null

# Убираем шаблонный мусор
Get-ChildItem -Recurse -Include Class1.cs,UnitTest1.cs | Remove-Item -Force

# Добавляем проекты в solution
Get-ChildItem -Recurse -Filter *.csproj |
    ForEach-Object { dotnet sln add $_.FullName | Out-Null }

Write-Host "  OK: solution + 9 проектов" -ForegroundColor Green

# ---------- 2. Directory.Build.props / Directory.Packages.props ----------
Write-Host "`n[2/5] props..." -ForegroundColor Yellow

$buildProps = @'
<Project>
  <PropertyGroup>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>
</Project>
'@
Set-Content -Path Directory.Build.props -Value ($buildProps -replace "`r`n","`n") -Encoding UTF8 -NoNewline

$pkgProps = @'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Spectre.Console.Cli" Version="0.48.0" />
    <PackageVersion Include="System.Text.Json" Version="8.0.5" />
    <PackageVersion Include="System.IO.Hashing" Version="8.0.0" />
    <PackageVersion Include="Microsoft.Data.Sqlite" Version="8.0.10" />
    <PackageVersion Include="System.Security.Cryptography.ProtectedData" Version="8.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="8.0.1" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="8.0.1" />
    <PackageVersion Include="Microsoft.Extensions.Http" Version="8.0.1" />
    <PackageVersion Include="Polly" Version="8.4.2" />
    <PackageVersion Include="Octokit" Version="13.0.1" />
    <PackageVersion Include="SharpCompress" Version="0.38.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
'@
Set-Content -Path Directory.Packages.props -Value ($pkgProps -replace "`r`n","`n") -Encoding UTF8 -NoNewline

Write-Host "  OK: props" -ForegroundColor Green

# ---------- 3. Чистим Version= из тестовых csproj ----------
Write-Host "`n[3/5] Чистка Version= в тестовых csproj..." -ForegroundColor Yellow

$testProjects = @(
    'tests/Firelink.Core.Tests/Firelink.Core.Tests.csproj',
    'tests/Firelink.Platform.MO2.Tests/Firelink.Platform.MO2.Tests.csproj',
    'tests/Firelink.Pack.Tests/Firelink.Pack.Tests.csproj'
)

foreach ($p in $testProjects) {
    if (-not (Test-Path $p)) { continue }
    $xml = Get-Content $p -Raw

    # Убираем Version=... и PrivateAssets=... из всех PackageReference
    $xml = [regex]::Replace($xml, '<PackageReference\s+([^/>]*?)/>', {
        param($m)
        $attrs = $m.Groups[1].Value
        $attrs = [regex]::Replace($attrs, '\s+Version="[^"]*"', '')
        $attrs = [regex]::Replace($attrs, '\s+PrivateAssets="[^"]*"', '')
        $attrs = $attrs.TrimEnd()
        if ([string]::IsNullOrWhiteSpace($attrs)) { return '<PackageReference />' }
        return '<PackageReference ' + $attrs + ' />'
    })

    # То же для парных тегов <PackageReference ...>...</PackageReference>
    $xml = [regex]::Replace($xml, '<PackageReference\s+([^>]*?)>(.*?)</PackageReference>', {
        param($m)
        $attrs = $m.Groups[1].Value
        $inner = $m.Groups[2].Value
        $attrs = [regex]::Replace($attrs, '\s+Version="[^"]*"', '')
        $attrs = [regex]::Replace($attrs, '\s+PrivateAssets="[^"]*"', '')
        $attrs = $attrs.TrimEnd()
        return '<PackageReference ' + $attrs + '>' + $inner + '</PackageReference>'
    })

    Set-Content $p -Value $xml -Encoding UTF8 -NoNewline
    Write-Host "  OK: $p" -ForegroundColor Green
}

# ---------- 4. Ссылки в .csproj ----------
Write-Host "`n[4/5] Ссылки в .csproj..." -ForegroundColor Yellow

function Add-PackageRefs {
    param([string]$Csproj, [string[]]$Packages)
    [xml]$xml = Get-Content $Csproj
    $ns = $xml.Project.NamespaceURI
    $ig = $xml.CreateElement('ItemGroup', $ns)
    foreach ($pkg in $Packages) {
        $pr = $xml.CreateElement('PackageReference', $ns)
        $pr.SetAttribute('Include', $pkg)
        $ig.AppendChild($pr) | Out-Null
    }
    $xml.Project.AppendChild($ig) | Out-Null
    $xml.Save((Resolve-Path $Csproj))
}

function Add-ProjectRefs {
    param([string]$Csproj, [string[]]$Paths)
    [xml]$xml = Get-Content $Csproj
    $ns = $xml.Project.NamespaceURI
    $ig = $xml.CreateElement('ItemGroup', $ns)
    foreach ($p in $Paths) {
        $pr = $xml.CreateElement('ProjectReference', $ns)
        $pr.SetAttribute('Include', $p)
        $ig.AppendChild($pr) | Out-Null
    }
    $xml.Project.AppendChild($ig) | Out-Null
    $xml.Save((Resolve-Path $Csproj))
}

Add-PackageRefs 'src/Firelink.Core/Firelink.Core.csproj' @(
    'System.Text.Json','System.IO.Hashing',
    'Microsoft.Extensions.Logging.Abstractions',
    'Microsoft.Extensions.DependencyInjection.Abstractions',
    'Microsoft.Data.Sqlite'
)
Add-ProjectRefs 'src/Firelink.Platform.MO2/Firelink.Platform.MO2.csproj' @('..\Firelink.Core\Firelink.Core.csproj')
Add-PackageRefs  'src/Firelink.Platform.MO2/Firelink.Platform.MO2.csproj' @('Microsoft.Extensions.Logging.Abstractions')
Add-ProjectRefs 'src/Firelink.Platform.Nexus/Firelink.Platform.Nexus.csproj' @('..\Firelink.Core\Firelink.Core.csproj')
Add-PackageRefs  'src/Firelink.Platform.Nexus/Firelink.Platform.Nexus.csproj' @(
    'Microsoft.Extensions.Http','Microsoft.Extensions.Logging.Abstractions',
    'Polly','System.Security.Cryptography.ProtectedData'
)
Add-ProjectRefs 'src/Firelink.Platform.GitHub/Firelink.Platform.GitHub.csproj' @('..\Firelink.Core\Firelink.Core.csproj')
Add-PackageRefs  'src/Firelink.Platform.GitHub/Firelink.Platform.GitHub.csproj' @('Octokit','Microsoft.Extensions.Logging.Abstractions')

foreach ($cli in @('src/Firelink.Pack/Firelink.Pack.csproj','src/Firelink.Install/Firelink.Install.csproj')) {
    Add-ProjectRefs $cli @(
        '..\Firelink.Core\Firelink.Core.csproj',
        '..\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj',
        '..\Firelink.Platform.Nexus\Firelink.Platform.Nexus.csproj',
        '..\Firelink.Platform.GitHub\Firelink.Platform.GitHub.csproj'
    )
    Add-PackageRefs $cli @(
        'Spectre.Console.Cli','Microsoft.Extensions.DependencyInjection',
        'Microsoft.Extensions.Logging','Microsoft.Extensions.Logging.Console',
        'Microsoft.Extensions.Http'
    )
}

Add-PackageRefs 'tests/Firelink.Core.Tests/Firelink.Core.Tests.csproj' @('FluentAssertions')
Add-ProjectRefs 'tests/Firelink.Core.Tests/Firelink.Core.Tests.csproj' @('..\..\src\Firelink.Core\Firelink.Core.csproj')

Add-PackageRefs 'tests/Firelink.Platform.MO2.Tests/Firelink.Platform.MO2.Tests.csproj' @('FluentAssertions')
Add-ProjectRefs 'tests/Firelink.Platform.MO2.Tests/Firelink.Platform.MO2.Tests.csproj' @('..\..\src\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj')

Add-PackageRefs 'tests/Firelink.Pack.Tests/Firelink.Pack.Tests.csproj' @('FluentAssertions')
Add-ProjectRefs 'tests/Firelink.Pack.Tests/Firelink.Pack.Tests.csproj' @('..\..\src\Firelink.Pack\Firelink.Pack.csproj')

Write-Host "  OK: csproj" -ForegroundColor Green

# ---------- 5. Restore + Build ----------
Write-Host "`n[5/5] Restore + Build..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "restore failed" }
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { throw "build failed" }

Write-Host "`n=== СКЕЛЕТ ГОТОВ ===" -ForegroundColor Green
Write-Host "Открывайте Firelink.sln в IDE и создавайте файлы по списку." -ForegroundColor Cyan