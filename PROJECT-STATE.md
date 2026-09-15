# Firelink — состояние проекта

**Обновлено:** 2026-09-15
**Всего тестов:** 263, 0 failed
**Текущий блок:** диагностика inline-файлов
**Последний закрытый блок:** 10.5 (первый успешный прогон pack с 7z.exe)

---

## Что это

Firelink — инструмент для создания и установки воспроизводимых сборок модов
для Mod Organizer 2. Два CLI-приложения: `Firelink.Pack` (автор) и
`Firelink.Install` (пользователь). Манифест `modlist.json` — единственный
источник правды. Файлы восстанавливаются по хешам `xxHash64`.

Подробности — в `Firelink — Документация проекта.txt` (v3.1).

---

## Структура репозитория
C:\Code\Firelink
Firelink.sln
Directory.Build.props
Directory.Build.targets ← глобальное копирование Assets/7z
Directory.Packages.props ← SharpCompress УДАЛЁН
Firelink — Документация проекта.txt
PROJECT-STATE.md
HANDOFF.md
samples/
firelink-pack.minimal.json
firelink-pack.full.json
firelink-pack.invalid-name.json
firelink-pack.invalid-version.json
firelink-pack.invalid-path.json
src/
Firelink.Core/
Hashing/
XxHash64Value.cs
XxHash64ValueJsonConverter.cs
Models/Manifest/
ArchiveEntry.cs
ModlistManifest.cs
ManifestJson.cs
Directives/ (Directive + 4 наследника)
Sources/ (ArchiveSourceRef + 3 наследника)
Models/Mo2/
ModlistEntry.cs
ModlistFile.cs
PluginEntry.cs
PluginsFile.cs
LoadorderFile.cs
Models/Pack/
PackConfig.cs
PackMeta.cs
PackInstance.cs
PackMo2.cs
PackStockGame.cs
PackArchiveSource.cs
PackConfigJson.cs
ArchiveIndex.cs
UnresolvedArchive.cs
InstanceSnapshot.cs
ModScanResult.cs
ScannedFile.cs
MatchResult.cs
InlineFileContent.cs
OrphanFile.cs
Identity/
Slug.cs
ArchiveId.cs
Validation/
ValidationResult.cs
NameValidator.cs
SemverValidator.cs
RelativePathValidator.cs
InstancePathValidator.cs
PackConfigValidator.cs
Archives/
ArchiveExtensions.cs
FileHashCache.cs
Extraction/
IArchiveExtractor.cs
SevenZipExtractor.cs
TempWorkspace.cs
Abstractions/
IStep.cs
Assets/7z/
7z.exe
7z.dll
License.txt
Firelink.Platform.MO2/
Models/
MetaFile.cs
Readers/
ModlistReader.cs
PluginsReader.cs
LoadorderReader.cs
MetaReader.cs
Writers/
ModlistWriter.cs
PluginsWriter.cs
LoadorderWriter.cs
Firelink.Platform.Nexus/ (пусто)
Firelink.Platform.GitHub/ (пусто)
Firelink.Pack/
Commands/
PackCommand.cs
HashCommand.cs ← включает HashSettings
DoctorCommand.cs
Settings/
PackSettings.cs
DoctorSettings.cs
Infrastructure/
TypeRegistrar.cs
Steps/
ReadConfigStep.cs
ReadInstanceStep.cs
IndexArchivesStep.cs
ScanModsStep.cs
MatchStep.cs
PackPipeline.cs
Program.cs
Firelink.Install/
Commands/ (InstallCommand, VerifyCommand, DoctorCommand — заглушки)
Settings/ (InstallSettings, VerifySettings, DoctorSettings)
Infrastructure/ (TypeRegistrar)
Program.cs
tests/
Firelink.Core.Tests/
ManifestRoundtripTests.cs
SlugTests.cs
ArchiveIdTests.cs
PackConfigJsonTests.cs
NameValidatorTests.cs
SemverValidatorTests.cs
RelativePathValidatorTests.cs
InstancePathValidatorTests.cs
PackConfigValidatorTests.cs
PackConfigSamplesTests.cs
XxHash64ValueParseTests.cs
SevenZipExtractorTests.cs
TempWorkspaceTests.cs
Firelink.Platform.MO2.Tests/
ModlistReaderTests.cs
PluginsReaderTests.cs
LoadorderReaderTests.cs
RoundtripTests.cs
MetaReaderTests.cs
Firelink.Pack.Tests/
IndexArchivesStepTests.cs
ReadConfigStepTests.cs
ReadInstanceStepTests.cs
ScanModsStepTests.cs
MatchStepTests.cs

Реальные инстансы:

C:\Firelink\OmenRim 7\

82 мода в modlist.txt (40 включённых)

6 плагинов включённых, 86 в loadorder

68 архивов в downloads/: 36 .zip, 31 .7z, 1 .rar

67 .meta-файлов

135 файлов в downloads/ (~1.2 ГБ)

1131 файл в 40 включённых модах

Большой инстанс — 4370 модов в modlist.txt

downloads/ ≈ 200–300 ГБ

средний архив ≤ 200 МБ, гиганты 5–20 ГБ

text

---

## Компоненты

### Firelink.Core

**Hashing:** `XxHash64Value` (strict Parse — 16 hex, без пробелов),
`XxHash64ValueJsonConverter`.

**Models/Manifest:** `ArchiveEntry`, `ModlistManifest`, `ManifestJson`,
`Directive` + 4 наследника (`FromArchive`, `InlineFile`, `CreateDirectory`,
`Delete`), `ArchiveSourceRef` + 3 наследника (`Nexus`, `GitHub`, `Mirror`).

**Models/Mo2:** `ModlistEntry`, `ModlistFile`, `PluginEntry`, `PluginsFile`,
`LoadorderFile`.

**Models/Pack:** `PackConfig`, `PackMeta`, `PackInstance`, `PackMo2`,
`PackStockGame`, `PackArchiveSource`, `PackConfigJson`, `ArchiveIndex`,
`UnresolvedArchive`, `InstanceSnapshot`, `ModScanResult`, `ScannedFile`,
`MatchResult`, `InlineFileContent`, `OrphanFile`.

**Identity:** `Slug`, `ArchiveId`.

**Validation:** `ValidationResult`, `NameValidator`, `SemverValidator`,
`RelativePathValidator`, `InstancePathValidator`, `PackConfigValidator`.

**Archives:** `ArchiveExtensions`, `FileHashCache`,
`Extraction/IArchiveExtractor`, `Extraction/SevenZipExtractor`,
`Extraction/TempWorkspace`.

**Abstractions:** `IStep<TInput, TOutput>`.

**Assets:** `Assets/7z/7z.exe`, `7z.dll`, `License.txt`.

### Firelink.Platform.MO2

**Models:** `MetaFile`.

**Readers:** `ModlistReader`, `PluginsReader`, `LoadorderReader`, `MetaReader`.

**Writers:** `ModlistWriter`, `PluginsWriter`, `LoadorderWriter`.

### Firelink.Pack

**Steps:** `ReadConfigStep`, `ReadInstanceStep`, `IndexArchivesStep`,
`ScanModsStep`, `MatchStep`.

**PackPipeline** — оркестратор.

**CLI:** `pack`, `hash`, `doctor`.

### Firelink.Install

**CLI:** `install`, `verify`, `doctor` — заглушки.

---

## Пайплайн packer-а (текущий)
ReadConfigStep
↓ PackConfig
ReadInstanceStep
↓ InstanceSnapshot (82 mods, 6 plugins, 86 loadorder)
IndexArchivesStep
↓ ArchiveIndex (68 resolved, 0 unresolved)
ScanModsStep
↓ ModScanResult (40 mods, 1131 files)
MatchStep
↓ MatchResult (1085 matched, 46 inline, 0 orphans)
[STOP — pipeline не завершён]

text

**Что нужно добавить:**
- `BuildManifestStep` → `ModlistManifest`.
- `ValidateManifestStep` → проверка ссылок.
- `WriteManifestStep` → `modlist.json` на диск.

---

## Результаты последнего прогона `pack` на `OmenRim 7`
Config OK: OmenRim 7 v0.1.0 (skyrimspecialedition)
Instance snapshot: 82 mods, 6 plugins, 86 load order entries
Indexing 68 archives in ...downloads
Ignoring non-Nexus .meta for 'Effect 11-...zip' → archiveSources
Indexed: 68 resolved, 0 unresolved
ScanModsStep: scanning 40 enabled mods (out of 82)
ScanModsStep: 40 mods, 1131 files total
Extracting archive 10/68: Skylighting...
Extracting archive 20/68: JContainers SE...
Extracting archive 30/68: Better Jumping NG...
Extracting archive 40/68: SSE Display Tweaks...
Extracting archive 50/68: Curated Bosses for True HUD...
Extracting archive 60/68: dTry's Key Utils AE...
Extracting archive 68/68: ENB Extender and Helper...
Archive index built: 4803 unique hashes from 68 archives
Match complete: 1131 files, 1085 matched, 46 inline, 0 orphans

╭───────────────────────┬───────────────────────╮
│ Field │ Value │
├───────────────────────┼───────────────────────┤
│ Name │ OmenRim 7 │
│ Version │ 0.1.0 │
│ Game │ skyrimspecialedition │
│ Instance │ C:\Firelink\OmenRim 7 │
│ Mods (all) │ 82 │
│ Plugins │ 6 │
│ Loadorder │ 86 │
│ Archives (resolved) │ 68 │
│ Archives (unresolved) │ 0 │
│ Mods (scanned) │ 40 │
│ Files scanned │ 1131 │
│ Directives │ 1131 │
│ → FromArchive │ 1085 │
│ → Inline │ 46 │
│ Inline files │ 46 │
│ Orphans │ 0 │
╰───────────────────────┴───────────────────────╯

text

**Распаковка 68 архивов: 16 секунд через 7z.exe.**
**0 failed extracts. 0 orphans.** Переход с SharpCompress на 7z.exe — полный успех.

---

## Ключевые метрики инстанса `OmenRim 7`

| Метрика | Значение |
|---|---|
| Строк в `modlist.txt` | 82 |
| Включённых модов (`+`) | 40 |
| Отключённых (`-`) | 42 |
| `[NoDelete]` модов | 0 |
| Архивов в `downloads/` | 68 |
| `.meta` файлов | 67 |
| Архивов без валидного `.meta` | 2 (Effect 11, NAT.ENB) |
| Файлов в `downloads/` | 135 |
| Размер `downloads/` | ~1.2 ГБ |
| Файлов в 40 включённых модах | 1131 |
| Уникальных хешей в архивах | 4803 |

---

## Текущая задача — диагностика inline-файлов

**Проблема:** 46 файлов из 1131 не нашли источник в архивах.

**Причины могут быть:**
1. Пользовательские правки (изменённое содержимое).
2. Файлы, сгенерированные модами (config.json, кеши, логи).
3. Файлы без источника (архива нет в downloads/).
4. Merged-моды.
5. FOMOD-выборы с модификацией.

**План диагностики (не написан):**
1. Второй индекс в `MatchStep`: `path → (archiveId, hash)`.
2. Логирование inline-файлов, сгруппированных по модам.
3. Для первых 20 — причина: «path not found» или «hash differs».

**Решение о судьбе inline (после диагностики):**
- Вариант A — оставить как base64 в манифесте (просто).
- Вариант B — механизм патчей (заменители).
- Вариант C — `--strict` режим (автор чистит инстанс).

**Для большого инстанса** inline критичен:
46/1131 = 4% → на 4370 модах ~40 000 файлов × ~13 КБ base64 = ~520 МБ манифест.

---

## Следующие блоки

1. **Диагностика inline-файлов** (текущая задача).
2. **Блок 11:** `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.
3. **Блок 12:** installer.

**Вопросы перед Блоком 11 (подтвердить перед кодом):**
1. `order` модов = индекс в `modlist.txt` (0, 1, 2...).
2. Все плагины сохраняем, включая `disabled`.
3. `mo2.archive` = заглушка (`id`, `hash=0`, `size=0`, `sources=[]`).
4. `extensions = []`, `extras = []` — пока пусто.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — после Блока 11.
- Nexus API — v0.2.0.
- Прогресс-бар Spectre — после Блока 11.
- `Firelink.Platform.Nexus` и `Firelink.Platform.GitHub` пусты.
- Диагностика inline-файлов — текущая задача.
- Механизм патчей для inline — v0.2.0.
- `ScanExtensionsStep`/`ScanExtrasStep` не написаны.
- `mo2.archive` в манифесте — заглушка, нужно реализовать правильную загрузку.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже установлен, проекты таргетят `net8.0`).
- Visual Studio 2022.
- Тестовый инстанс `C:\Firelink\OmenRim 7\`.
- Большой инстанс — 4370 модов.