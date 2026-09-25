# Firelink -- repo dump

**Generated:** 25.09.2026 10:14:02,41
**Root:** D:\Code\repos\Firelink

---

## .editorconfig

````text
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space
indent_size = 4

[*.{cs,csx}]
dotnet_sort_system_directives_first = true
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_prefer_braces = true:suggestion
csharp_style_namespace_declarations = file_scoped:warning
dotnet_diagnostic.CA1822.severity = none

[*.{csproj,props,targets}]
indent_size = 2

[*.json]
indent_size = 2

[*.{yml,yaml}]
indent_size = 2
````

## .gitignore

````text
# --- Visual Studio / Rider / VS Code ---
.vs/
.idea/
.vscode/
*.user
*.suo

# --- Build output ---
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
build/
build_artifacts/

# --- NuGet ---
*.nupkg
*.snupkg
**/packages/*
!**/packages/build/
.nuget/

# --- Test / coverage ---
[Tt]est[Rr]esult*/
*.trx
coverage/
*.coverage
*.coveragexml

# --- OS junk ---
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
*.lnk
.DS_Store

# --- Firelink-specific ---
# Логи, которые Firelink пишет
firelink.log
firelink-*.log

# Локальные инстансы MO2, которые не надо коммитить
# (раскомментируйте, если инстанс лежит рядом с репо)
# Firelink/
# OmenRim 7/
````

## samples/firelink-pack.back.json

````json
{
  "meta": {
    "name": "OmenRim 7",
    "version": "0.1.0",
    "author": "YourName",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "."
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://github.com/ModOrganizer2/modorganizer/releases/download/v2.5.2/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:E574E05EB6C470AD"
    },
	  "extensions": [
		"plugins/curationclub"
	]
  },
  "stockGame": {
	"extras": [
    "skse64_loader.exe",
    "skse64_1_7_104.dll"
	]
  },
  "archiveSources": [
    {
      "archive": "Effect 11-415-1.0.0-2026.08.24-[mod.pub].zip",
      "sources": [
        {
          "type": "mirror",
          "url": "https://mod.pub/skyrim-se/415/files/Effect-11-415-1.0.0-2026.08.24-[mod.pub].zip",
          "hash": "xxh64:B48AA9BEA422799E"
        }
      ]
    },
    {
      "archive": "NAT.ENB - ENB PRESET v3.1.1C-27141-3-1-1C-1685129135.zip",
      "sources": [
        {
          "type": "mirror",
          "url": "https://mod.pub/skyrim-se/415/files/NAT.ENB-ENB-PRESET-v3-1-1C-27141-3-1-1C-1685129135.zip",
          "hash": "xxh64:763D3DB4CD3ED579"
        }
      ]
    }
  ]
}
````

## samples/firelink-pack.full.json

````json
{
  "meta": {
    "name": "Nordic UI Overhaul",
    "version": "1.2.0",
    "author": "Username",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "NordicUI Overhaul"
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "NordicUI",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://github.com/ModOrganizer2/modorganizer/releases/download/v2.5.2/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:0000000000000001"
    },
    "extensions": [
      "plugins/fomod_plus_installer.dll",
      "plugins/fomod_plus_scanner.dll",
      "tools/BethINI/"
    ]
  },
  "stockGame": {
    "extras": [
      "skse64_loader.exe",
      "skse64_1_6_1170.dll",
      "d3d11.dll",
      "enbseries/"
    ]
  },
  "archiveSources": [
    {
      "archive": "SomeModWithoutMeta.7z",
      "sources": [
        {
          "type": "mirror",
          "url": "https://cdn.example.com/SomeModWithoutMeta.7z",
          "hash": "xxh64:0000000000000abc"
        },
        {
          "type": "nexus",
          "modId": 12345,
          "fileId": 67890,
          "game": "skyrimspecialedition"
        }
      ]
    },
    {
      "archive": "AnotherMod.7z",
      "sources": [
        {
          "type": "mirror",
          "url": "https://github.com/author/repo/releases/download/v1.0/AnotherMod.7z",
          "hash": "xxh64:0000000000000def"
        }
      ]
    }
  ]
}
````

## samples/firelink-pack.invalid-name.json

````json
{
  "meta": {
    "name": "CON",
    "version": "1.0.0",
    "author": "tester",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "Bad Name Pack"
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://example.com/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:0000000000000001"
    },
    "extensions": []
  },
  "stockGame": {
    "extras": []
  },
  "archiveSources": []
}
````

## samples/firelink-pack.invalid-path.json

````json
{
  "meta": {
    "name": "Bad Path Pack",
    "version": "1.0.0",
    "author": "tester",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "Bad Path Pack"
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://example.com/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:0000000000000001"
    },
    "extensions": [
      "plugins/../../../etc/passwd"
    ]
  },
  "stockGame": {
    "extras": []
  },
  "archiveSources": []
}
````

## samples/firelink-pack.invalid-version.json

````json
{
  "meta": {
    "name": "Bad Version Pack",
    "version": "v1.0",
    "author": "tester",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "Bad Version Pack"
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://example.com/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:0000000000000001"
    },
    "extensions": []
  },
  "stockGame": {
    "extras": []
  },
  "archiveSources": []
}
````

## samples/firelink-pack.json

````json
{
  "meta": {
    "name": "OmenRim 7",
    "version": "0.1.0",
    "author": "YourName",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "."
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://github.com/ModOrganizer2/modorganizer/releases/download/v2.5.2/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:E574E05EB6C470AD"
    },
	  "extensions": [
		"plugins/curationclub"
	]
  },
  "stockGame": {
	"extras": [
    "skse64_loader.exe",
    "skse64_1_7_104.dll"
	]
  },
  "archiveSources": []
}
````

## samples/firelink-pack.minimal.json

````json
{
  "meta": {
    "name": "Minimal Pack",
    "version": "1.0.0",
    "author": "tester",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "instance": {
    "path": "Minimal Pack"
  },
  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "mirror",
      "url": "https://github.com/ModOrganizer2/modorganizer/releases/download/v2.5.2/Mod.Organizer-2.5.2.7z",
      "hash": "xxh64:0000000000000001"
    },
    "extensions": []
  },
  "stockGame": {
    "extras": []
  },
  "archiveSources": []
}
````

## src/Firelink.Cli/Firelink.Cli.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>Firelink.Cli</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
    <ProjectReference Include="..\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj" />
    <ProjectReference Include="..\Firelink.Platform.Nexus\Firelink.Platform.Nexus.csproj" />
    <ProjectReference Include="..\Firelink.Pack\Firelink.Pack.csproj" />
    <ProjectReference Include="..\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Spectre.Console.Cli" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
    <PackageReference Include="Microsoft.Extensions.Http" />
  </ItemGroup>
</Project>
````

## src/Firelink.Core/Firelink.Core.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="System.Text.Json" />
    <PackageReference Include="System.IO.Hashing" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Data.Sqlite" />
    <PackageReference Include="Polly" />
  </ItemGroup>
</Project>
````

## src/Firelink.Core/Assets/7z/License.txt

````text
  7-Zip
  ~~~~~
  License for use and distribution
  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  7-Zip Copyright (C) 1999-2026 Igor Pavlov.

  The licenses for files are:

    - 7z.dll:
         - The "GNU LGPL" as main license for most of the code
         - The "GNU LGPL" with "unRAR license restriction" for some code
         - The "BSD 3-clause License" for some code
         - The "BSD 2-clause License" for some code
    - All other files: the "GNU LGPL".

  Redistributions in binary form must reproduce related license information from this file.

  Note:
    You can use 7-Zip on any computer, including a computer in a commercial
    organization. You don't need to register or pay for 7-Zip.


GNU LGPL information
--------------------

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You can receive a copy of the GNU Lesser General Public License from
    http://www.gnu.org/




BSD 3-clause License in 7-Zip code
----------------------------------

  The "BSD 3-clause License" is used for the following code in 7z.dll
    1) LZFSE data decompression.
       That code was derived from the code in the "LZFSE compression library" developed by Apple Inc,
       that also uses the "BSD 3-clause License".
    2) ZSTD data decompression.
       that code was developed using original zstd decoder code as reference code.
       The original zstd decoder code was developed by Facebook Inc,
       that also uses the "BSD 3-clause License".

  Copyright (c) 2015-2016, Apple Inc. All rights reserved.
  Copyright (c) Facebook, Inc. All rights reserved.
  Copyright (c) 2023-2026 Igor Pavlov.

Text of the "BSD 3-clause License"
----------------------------------

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may
   be used to endorse or promote products derived from this software without
   specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

---




BSD 2-clause License in 7-Zip code
----------------------------------

  The "BSD 2-clause License" is used for the XXH64 code in 7-Zip.

  XXH64 code in 7-Zip was derived from the original XXH64 code developed by Yann Collet.

  Copyright (c) 2012-2021 Yann Collet.
  Copyright (c) 2023-2026 Igor Pavlov.

Text of the "BSD 2-clause License"
----------------------------------

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

---




unRAR license restriction
-------------------------

The decompression engine for RAR archives was developed using source
code of unRAR program.
All copyrights to original unRAR code are owned by Alexander Roshal.

The license for original unRAR code has the following restriction:

  The unRAR sources cannot be used to re-create the RAR compression algorithm,
  which is proprietary. Distribution of modified unRAR sources in separate form
  or as a part of other software is permitted, provided that it is clearly
  stated in the documentation and source comments that the code may
  not be used to develop a RAR (WinRAR) compatible archiver.

--

````

## src/Firelink.Gui/Firelink.Gui.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Firelink</AssemblyName>
    <RootNamespace>Firelink.Gui</RootNamespace>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
    <ApplicationIcon>Assets\app.ico</ApplicationIcon>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" />
    <PackageReference Include="Avalonia.Desktop" />
    <PackageReference Include="Avalonia.Themes.Fluent" />
    <PackageReference Include="Avalonia.Fonts.Inter" />
    <PackageReference Include="Avalonia.Diagnostics" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Gui.Install\Firelink.Gui.Install.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Pack\Firelink.Gui.Pack.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Verify\Firelink.Gui.Verify.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Controls\Firelink.Gui.Controls.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\Firelink.Install\Firelink.Install.csproj" />
    <ProjectReference Include="..\Firelink.Pack\Firelink.Pack.csproj" />
  </ItemGroup>
  <ItemGroup>
    <AvaloniaResource Include="Assets\app.ico" />
  </ItemGroup>    
</Project>
````

## src/Firelink.Gui.Controls/Firelink.Gui.Controls.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>  <ItemGroup>
    <ProjectReference Include="..\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
  </ItemGroup>
</Project>

````

## src/Firelink.Gui.Install/Firelink.Gui.Install.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Gui.Controls\Firelink.Gui.Controls.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>
</Project>
````

## src/Firelink.Gui.Pack/Firelink.Gui.Pack.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Gui.Controls\Firelink.Gui.Controls.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\Firelink.Pack\Firelink.Pack.csproj" />
  </ItemGroup>
</Project>
````

## src/Firelink.Gui.Shared/Firelink.Gui.Shared.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="Firelink.Gui.Shared.Tests" />
  </ItemGroup>
</Project>
````

## src/Firelink.Gui.Verify/Firelink.Gui.Verify.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\Firelink.Gui.Controls\Firelink.Gui.Controls.csproj" />
    <ProjectReference Include="..\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>
</Project>

````

## src/Firelink.Install/Firelink.Install.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
    <ProjectReference Include="..\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj" />
    <ProjectReference Include="..\Firelink.Platform.Nexus\Firelink.Platform.Nexus.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" />
  </ItemGroup>
</Project>
````

## src/Firelink.Pack/Firelink.Pack.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
    <ProjectReference Include="..\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj" />
    <ProjectReference Include="..\Firelink.Platform.Nexus\Firelink.Platform.Nexus.csproj" />
  </ItemGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="Firelink.Pack.Tests" />
  </ItemGroup>
</Project>
````

## src/Firelink.Platform.GitHub/Firelink.Platform.GitHub.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Octokit" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Http" />
  </ItemGroup>
</Project>
````

## src/Firelink.Platform.MO2/Firelink.Platform.MO2.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
````

## src/Firelink.Platform.Nexus/Firelink.Platform.Nexus.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Firelink.Core\Firelink.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Polly" />
    <PackageReference Include="System.Security.Cryptography.ProtectedData" />
  </ItemGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="Firelink.Platform.Nexus.Tests" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Core.Tests/Firelink.Core.Tests.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Core\Firelink.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Include="..\..\samples\**\*.json"
          Link="samples\%(RecursiveDir)%(Filename)%(Extension)"
          CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Gui.Install.Tests/Firelink.Gui.Install.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Gui.Install\Firelink.Gui.Install.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Gui.Pack.Tests/Firelink.Gui.Pack.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Gui.Pack\Firelink.Gui.Pack.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Pack\Firelink.Pack.csproj" />
  </ItemGroup>

</Project>
````

## tests/Firelink.Gui.Shared.Tests/Firelink.Gui.Shared.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Gui.Verify.Tests/Firelink.Gui.Verify.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Gui.Verify\Firelink.Gui.Verify.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Gui.Shared\Firelink.Gui.Shared.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>

</Project>
````

## tests/Firelink.Install.Tests/Firelink.Install.Tests.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Integration.Tests/Firelink.Integration.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Core\Firelink.Core.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Pack\Firelink.Pack.csproj" />
    <ProjectReference Include="..\..\src\Firelink.Install\Firelink.Install.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Pack.Tests/Firelink.Pack.Tests.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Pack\Firelink.Pack.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Platform.MO2.Tests/Firelink.Platform.MO2.Tests.csproj

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Platform.MO2\Firelink.Platform.MO2.csproj" />
  </ItemGroup>
</Project>
````

## tests/Firelink.Platform.Nexus.Tests/Firelink.Platform.Nexus.Tests.csproj

````xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Firelink.Platform.Nexus\Firelink.Platform.Nexus.csproj" />
  </ItemGroup>
</Project>
````

## tools/build-release.bat

````batch
@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

rem ============================================================
rem  Firelink release build
rem
rem  Usage:
rem    build-release.bat
rem
rem  Результат:
rem    build_artifacts/Firelink-<version>-win-x64.zip
rem
rem  Что внутри:
rem    Firelink.exe        — GUI
rem    Firelink.Cli.exe    — CLI
rem    *.dll               — общие зависимости
rem    Assets/7z/          — 7z.exe + 7z.dll + License.txt
rem
rem  Скрипт ожидает, что лежит в <repo>\tools\build-release.bat.
rem ============================================================

set "ROOT=%~dp0.."
pushd "%ROOT%" || (echo Failed to cd to "%ROOT%" & exit /b 1)

set "PROPS=Directory.Build.props"
set "GUI_PROJECT=src\Firelink.Gui\Firelink.Gui.csproj"
set "CLI_PROJECT=src\Firelink.Cli\Firelink.Cli.csproj"

echo === Firelink release build ===
echo Root: %CD%
echo.

rem --- 1. Читаем версию из Directory.Build.props ---
if not exist "%PROPS%" (
    echo ERROR: %PROPS% not found.
    popd
    exit /b 1
)

set "VERSION="
for /f "usebackq delims=" %%L in (`powershell -NoProfile -Command ^
    "(Select-Xml -Path '%PROPS%' -XPath '//VersionPrefix').Node.InnerText.Trim()"`) do (
    set "VERSION=%%L"
)

if "!VERSION!"=="" (
    echo ERROR: could not read ^<VersionPrefix^> from %PROPS%.
    popd
    exit /b 1
)

echo Version: !VERSION!
echo.

set "ARTIFACT_DIR=build_artifacts\Firelink-!VERSION!-win-x64"
set "ARTIFACT_ZIP=build_artifacts\Firelink-!VERSION!-win-x64.zip"

rem --- 2. Готовим пустую папку для publish ---
if exist "%ARTIFACT_DIR%" rmdir /s /q "%ARTIFACT_DIR%"
if exist "%ARTIFACT_ZIP%" del /q "%ARTIFACT_ZIP%"
mkdir "%ARTIFACT_DIR%" || (echo Failed to create "%ARTIFACT_DIR%" & popd & exit /b 1)

echo Publishing GUI: %GUI_PROJECT%
dotnet publish "%GUI_PROJECT%" ^
    -c Release ^
    -r win-x64 ^
    --self-contained false ^
    -o "%ARTIFACT_DIR%"
if errorlevel 1 (
    echo ERROR: dotnet publish failed for GUI.
    popd
    exit /b 1
)
echo.

echo Publishing CLI: %CLI_PROJECT%
dotnet publish "%CLI_PROJECT%" ^
    -c Release ^
    -r win-x64 ^
    --self-contained false ^
    -o "%ARTIFACT_DIR%"
if errorlevel 1 (
    echo ERROR: dotnet publish failed for CLI.
    popd
    exit /b 1
)
echo.

rem --- 3. Проверяем, что оба exe на месте ---
set "GUI_EXE=%ARTIFACT_DIR%\Firelink.exe"
set "CLI_EXE=%ARTIFACT_DIR%\Firelink.Cli.exe"

if not exist "%GUI_EXE%" (
    echo ERROR: %GUI_EXE% not found after publish.
    popd
    exit /b 1
)
if not exist "%CLI_EXE%" (
    echo ERROR: %CLI_EXE% not found after publish.
    popd
    exit /b 1
)

rem --- 4. Пакуем в zip ---
echo Packing: %ARTIFACT_ZIP%
powershell -NoProfile -Command ^
    "Compress-Archive -Path '%ARTIFACT_DIR%\*' -DestinationPath '%ARTIFACT_ZIP%' -Force"
if errorlevel 1 (
    echo ERROR: Compress-Archive failed.
    popd
    exit /b 1
)

rem --- 5. Итог ---
for %%F in ("%ARTIFACT_ZIP%") do set "ZIP_SIZE=%%~zF"
set /a ZIP_SIZE_MB=!ZIP_SIZE! / 1048576

echo.
echo Done.
echo Zip: %ARTIFACT_ZIP%
echo Size: !ZIP_SIZE! bytes (~!ZIP_SIZE_MB! MB)
echo.

popd
endlocal
exit /b 0
````

