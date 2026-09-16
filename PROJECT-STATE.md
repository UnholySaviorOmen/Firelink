# Firelink — состояние проекта

**Обновлено:** 2026-09-16
**Всего тестов:** 288, 0 failed
**Текущий блок:** ждём старта блока 11
**Последний закрытый блок:** 10.7 (диагностика inline, `__Firelink_Output`,
structured meta.ini)

---

## Что это

Firelink — инструмент для создания и установки воспроизводимых сборок модов
для Mod Organizer 2. Два CLI-приложения: `Firelink.Pack` (автор) и
`Firelink.Install` (пользователь). Манифест `modlist.json` — единственный
источник правды. Файлы восстанавливаются по хешам `xxHash64`.

Подробности — в `DOC.md` (v3.2).

---

## Структура репозитория (актуально после 10.7)
C:\Code\Firelink
Firelink.slnx
Directory.Build.props
Directory.Build.targets ← глобальное копирование Assets/7z
Directory.Packages.props ← SharpCompress УДАЛЁН
DOC.md ← v3.2
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
ModlistEntry.cs / ModlistFile.cs
PluginEntry.cs / PluginsFile.cs
LoadorderFile.cs
Models/Pack/
PackConfig.cs / PackMeta.cs / PackInstance.cs
PackMo2.cs / PackStockGame.cs
PackArchiveSource.cs / PackConfigJson.cs
ArchiveIndex.cs / UnresolvedArchive.cs
InstanceSnapshot.cs ← + FirelinkOutputPath (10.7)
ModScanResult.cs / ScannedFile.cs
MatchResult.cs ← переписан (10.7)
UnmatchedFile.cs ← НОВЫЙ (10.7)
ModMeta.cs ← НОВЫЙ (10.7)
InlineFileContent.cs ← не используется (10.7)
OrphanFile.cs ← не используется (10.7)
Identity/
Slug.cs / ArchiveId.cs
Validation/
ValidationResult.cs
NameValidator.cs / SemverValidator.cs
RelativePathValidator.cs / InstancePathValidator.cs
PackConfigValidator.cs
Archives/
ArchiveExtensions.cs / FileHashCache.cs
Extraction/IArchiveExtractor.cs
Extraction/SevenZipExtractor.cs
Extraction/TempWorkspace.cs
Abstractions/IStep.cs
Assets/7z/7z.exe, 7z.dll, License.txt
Firelink.Platform.MO2/
Models/MetaFile.cs
Readers/
ModlistReader.cs / PluginsReader.cs
LoadorderReader.cs / MetaReader.cs
MetaIniReader.cs ← НОВЫЙ (10.7)
Writers/
ModlistWriter.cs / PluginsWriter.cs / LoadorderWriter.cs
Firelink.Platform.Nexus/ (пусто)
Firelink.Platform.GitHub/ (пусто)
Firelink.Pack/
Commands/
PackCommand.cs ← обновлён (10.7)
HashCommand.cs
DoctorCommand.cs
Settings/
PackSettings.cs / DoctorSettings.cs
Infrastructure/TypeRegistrar.cs
Steps/
ReadConfigStep.cs
ReadInstanceStep.cs ← + FirelinkOutputPath (10.7)
IndexArchivesStep.cs
ScanModsStep.cs
MatchStep.cs ← переписан (10.7)
PackPipeline.cs ← обновлён (10.7)
Program.cs
Firelink.Install/
Commands/ (InstallCommand, VerifyCommand, DoctorCommand — заглушки)
Settings/ (InstallSettings, VerifySettings, DoctorSettings)
Infrastructure/TypeRegistrar.cs
Program.cs
tests/
Firelink.Core.Tests/
ManifestRoundtripTests.cs
SlugTests.cs / ArchiveIdTests.cs
PackConfigJsonTests.cs
NameValidatorTests.cs / SemverValidatorTests.cs
RelativePathValidatorTests.cs / InstancePathValidatorTests.cs
PackConfigValidatorTests.cs / PackConfigSamplesTests.cs
XxHash64ValueParseTests.cs
SevenZipExtractorTests.cs / TempWorkspaceTests.cs
Firelink.Platform.MO2.Tests/
ModlistReaderTests.cs / PluginsReaderTests.cs
LoadorderReaderTests.cs / RoundtripTests.cs
MetaReaderTests.cs
MetaIniReaderTests.cs ← НОВЫЙ (10.7)
Firelink.Pack.Tests/
IndexArchivesStepTests.cs
ReadConfigStepTests.cs
ReadInstanceStepTests.cs
ScanModsStepTests.cs ← +FirelinkOutputPath в MakeSnapshot (10.7)
MatchStepTests.cs ← переписан (10.7)

text

Реальные инстансы:

- `C:\Firelink\OmenRim 7\`
  - 82 мода в modlist.txt (40 включённых)
  - 6 плагинов включённых, 86 в loadorder
  - 68 архивов в downloads/: 36 .zip, 31 .7z, 1 .rar
  - 67 .meta-файлов
  - 135 файлов в downloads/ (~1.2 ГБ)
  - 1131 файл в 40 включённых модах
  - `__Firelink_Output/mods/` — 6 файлов после 10.7
- Большой инстанс — 4370 модов в modlist.txt
  - downloads/ ≈ 200–300 ГБ
  - средний архив ≤ 200 МБ, гиганты 5–20 ГБ

---

## Компоненты

### Firelink.Core

**Hashing:** `XxHash64Value` (strict Parse — 16 hex, без пробелов),
`XxHash64ValueJsonConverter`.

**Models/Manifest:** `ArchiveEntry`, `ModlistManifest`, `ManifestJson`,
`Directive` + 4 наследника, `ArchiveSourceRef` + 3 наследника.

**Models/Mo2:** `ModlistEntry`, `ModlistFile`, `PluginEntry`, `PluginsFile`,
`LoadorderFile`.

**Models/Pack:** `PackConfig`, `PackMeta`, `PackInstance`, `PackMo2`,
`PackStockGame`, `PackArchiveSource`, `PackConfigJson`, `ArchiveIndex`,
`UnresolvedArchive`, `InstanceSnapshot` (с `FirelinkOutputPath`),
`ModScanResult`, `ScannedFile`, `MatchResult` (новый), `UnmatchedFile` (новый),
`ModMeta` (новый), `InlineFileContent` (не используется),
`OrphanFile` (не используется).

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

**Readers:** `ModlistReader`, `PluginsReader`, `LoadorderReader`,
`MetaReader` (архивный `downloads/*.meta`), **`MetaIniReader`** (новый,
`mods/<Name>/meta.ini`).

**Writers:** `ModlistWriter`, `PluginsWriter`, `LoadorderWriter`.

### Firelink.Pack

**Steps:** `ReadConfigStep`, `ReadInstanceStep`, `IndexArchivesStep`,
`ScanModsStep`, **`MatchStep`** (переписан).

**PackPipeline** — оркестратор.

**CLI:** `pack`, `hash`, `doctor`.

### Firelink.Install

**CLI:** `install`, `verify`, `doctor` — заглушки.

---

## Пайплайн packer-а (текущий)
ReadConfigStep
↓ PackConfig
ReadInstanceStep
↓ InstanceSnapshot (82 mods, 6 plugins, 86 loadorder, +FirelinkOutputPath)
IndexArchivesStep
↓ ArchiveIndex (68 resolved, 0 unresolved)
ScanModsStep
↓ ModScanResult (40 mods, 1131 files)
MatchStep
↓ MatchResult (1085 directives, 6 unmatched, 40 meta.ini)
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
Extracting archive 68/68 (16 секунд через 7z.exe)
Archive index built: 4803 unique hashes, 6524 unique paths across 68 archives
Match complete: 1131 files, 142 matched (exact), 943 matched (by hash), 6 unmatched, 40 meta.ini
Unmatched files written to __Firelink_Output: 6
Unmatched diagnostics: 6 files across 5 mods
by reason: path-not-found = 4, hash-differs = 2

text

Таблица:
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
│ Directives │ 1085 │
│ → FromArchive │ 1085 │
│ → Inline │ 0 │
│ Unmatched files │ 6 │
│ meta.ini │ 40 │
╰───────────────────────┴───────────────────────╯

text

**Ключевые числа:**
- **142 matched (exact)** — точное совпадение `(hash, path)`.
- **943 matched (by hash)** — совпадение хеша, путь отличается.
  Крупная цифра (83%), объясняется тем, что в архивах путь может
  отличаться от пути после установки MO2 (FOMOD, разная структура корня).
  Содержимое идентично — семантических потерь нет.
- **6 unmatched** — то, что не восстановимо из архивов.
- **40 meta.ini** — по числу сканированных модов.

**Что лежит в `__Firelink_Output/mods/` после прогона:**
Actor Limit Fix/SKSE/Plugins/ActorLimitFix.log
Bug Fixes SSE/SKSE/Plugins/BugFixesSSE.log
Enhanced Invisibility/SKSE/Plugins/po3_EnhancedInvisibility.ini
Enhanced Reanimation/SKSE/Plugins/po3_EnhancedReanimation.ini
SKSE Output/SKSE/Plugins/po3_SpellPerkItemDistributor.ini
SKSE Output/SKSE/Plugins/po3_Tweaks.ini

text

Из 6:
- 2 runtime-лога (`ActorLimitFix.log`, `BugFixesSSE.log`) — hash-differs,
  автор выкинет.
- 4 авторских `.ini` (po3_*) — path-not-found, автор оставит и сделает
  патч-архив.

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
| Уникальных путей в архивах | 6524 |
| Matched exact | 142 |
| Matched by hash | 943 |
| Unmatched | 6 |
| `meta.ini` | 40 |

---

## Следующие блоки

1. **Блок 11:** `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.
2. **Блок 12:** installer.

**Вопросы перед Блоком 11 (подтвердить перед кодом):**
1. `order` модов = индекс в `modlist.txt` (0, 1, 2...).
2. Все плагины сохраняем, включая `disabled`.
3. `mo2.archive` = заглушка (`id`, `hash=0`, `size=0`, `sources=[]`).
4. `extensions = []`, `extras = []` — пока пусто.
5. `mods[].meta` пишется **со всеми полями `ModMeta`** (включая
   `gameName`/`gameId`/`repository`/`url`) — манифест самодостаточный,
   без опоры на Nexus API.
6. `inlineFiles` в манифесте — **не заполняем** в MVP.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — после Блока 11.
- Nexus API — v0.2.0.
- Прогресс-бар Spectre — после Блока 11.
- `Firelink.Platform.Nexus` и `Firelink.Platform.GitHub` пусты.
- `PackCommand` показывает устаревшую колонку `Inline` (всегда 0).
- `InlineFileContent` / `OrphanFile` не используются — решить судьбу.
- `943 matched by hash` — понять природу расхождений путей (не срочно).
- Механизм патчей для inline-файлов — v0.2.0 (использует `__Firelink_Output`).
- `ScanExtensionsStep`/`ScanExtrasStep` не написаны.
- `mo2.archive` в манифесте — заглушка, нужно реализовать правильную загрузку.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже установлен, проекты таргетят `net8.0`).
- Visual Studio 2022.
- Тестовый инстанс `C:\Firelink\OmenRim 7\`.
- Большой инстанс — 4370 модов.
