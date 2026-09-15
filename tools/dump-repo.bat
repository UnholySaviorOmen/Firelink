@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

rem ============================================================
rem  Firelink repo dump — CMD version
rem  Usage:
rem    dump-repo.bat                    — repo-dump.md
rem    dump-repo.bat output.md          — свой путь
rem ============================================================

set "ROOT=%~dp0.."
pushd "%ROOT%"

set "OUT=%~1"
if "%OUT%"=="" set "OUT=repo-dump.md"
for %%I in ("%OUT%") do set "OUT=%%~fI"

set "OUT_DIR=%~dp1"
if "%OUT_DIR%"=="" set "OUT_DIR=%CD%\"

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%" >nul 2>&1

echo === Firelink repo dump ===
echo Root:   %CD%
echo Output: %OUT%
echo.

if exist "%OUT%" del "%OUT%"

rem --- Заголовок ---
echo # Firelink -- repo dump>> "%OUT%"
echo.>> "%OUT%"
echo **Generated:** %DATE% %TIME%>> "%OUT%"
echo **Root:** %CD%>> "%OUT%"
echo.>> "%OUT%"
echo --->> "%OUT%"
echo.>> "%OUT%"

rem --- Обход файлов ---
rem  dir /b /s /a-d — только файлы, полные пути
rem  Фильтрация папок — через findstr /v с регулярками
rem  Фильтрация по расширениям — .cs .csproj .sln .props .targets .json
rem  .txt .md .ps1 .yml .yaml .xml .config .editorconfig .gitignore

set "TEMP_LIST=%TEMP%\firelink-dump-list-%RANDOM%.txt"

dir /b /s /a-d "%CD%" ^
  | findstr /v /i "\\\.git\\ \\\.vs\\ \\\.idea\\ \\\.vscode\\ \\\bin\\ \\\obj\\ \\\packages\\ \\\node_modules\\ \\\TestResults\\ \\\coverage\\ \\\.nuget\\ \\\Debug\\ \\\Release\\" ^
  | findstr /i /e ".cs .csproj .sln .props .targets .json .txt .md .ps1 .psm1 .yml .yaml .xml .config .editorconfig .gitignore .gitattributes .sh .bat .cmd" ^
  > "%TEMP_LIST%"

set COUNT=0
for /f "usebackq delims=" %%F in ("%TEMP_LIST%") do set /a COUNT+=1

echo Files to include: %COUNT%
echo.

set /a INDEX=0

for /f "usebackq delims=" %%F in ("%TEMP_LIST%") do (
    set /a INDEX+=1
    set "FULL=%%F"
    set "REL=!FULL:%CD%\=!"
    set "REL=!REL:\=/!"
    set "REL=!REL:/=\!"

    echo [!INDEX!/%COUNT%] !REL!

    echo ## !REL!>> "%OUT%"
    echo.>> "%OUT%"

    rem Определить язык (упрощённо — без switch)
    set "EXT=%%~xF"
    set "LANG=text"
    if /i "!EXT!"==".cs"            set "LANG=csharp"
    if /i "!EXT!"==".csproj"        set "LANG=xml"
    if /i "!EXT!"==".props"         set "LANG=xml"
    if /i "!EXT!"==".targets"       set "LANG=xml"
    if /i "!EXT!"==".json"          set "LANG=json"
    if /i "!EXT!"==".md"            set "LANG=markdown"
    if /i "!EXT!"==".ps1"           set "LANG=powershell"
    if /i "!EXT!"==".psm1"          set "LANG=powershell"
    if /i "!EXT!"==".yml"           set "LANG=yaml"
    if /i "!EXT!"==".yaml"          set "LANG=yaml"
    if /i "!EXT!"==".xml"           set "LANG=xml"
    if /i "!EXT!"==".config"        set "LANG=xml"
    if /i "!EXT!"==".bat"           set "LANG=batch"
    if /i "!EXT!"==".cmd"           set "LANG=batch"
    if /i "!EXT!"==".sh"            set "LANG=bash"

    rem Открыть блок кода
    echo ````!LANG!>> "%OUT%"

    rem Содержимое файла
    type "%%F" >> "%OUT%"

    rem Гарантировать перевод строки перед закрывающей ```
    rem (если файл не заканчивался на newline)
    echo.>> "%OUT%"

    rem Закрыть блок кода
    echo ````>> "%OUT%"
    echo.>> "%OUT%"
)

del "%TEMP_LIST%" >nul 2>&1

echo.
echo Written: %OUT%
echo Done.

popd
endlocal