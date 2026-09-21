# Firelink — состояние проекта

**Обновлено:** 2026-09-21
**Всего тестов:** 593, 0 failed
**Текущий блок:** ничего (все хвосты закрыты)
**Последний закрытый блок:** 12.13.10 — error-msg для пробелов без кавычек
**Следующий блок:** 12.8 — NexusDownloader

---

## Что это

Firelink — инструмент для создания и установки воспроизводимых сборок
модов для Mod Organizer 2. Два CLI: `Firelink.Pack` (автор) и
`Firelink.Install` (пользователь). Манифест `modlist.json` — единственный
источник правды. Файлы восстанавливаются по хешам `xxHash64`.

Подробности — в `DOC.md` (v3.9). План — в `HANDOFF.md`.

---

## Статус MVP

**MVP работает end-to-end:**

- Прогон `firelink-install install` на `C:\Firelink\TestInstance\`
  (19.09.2026, до 12.13.x).
- 82 мода разложены, 82 meta.ini записаны, профиль сгенерирован.
- **12.11.1–12.11.3:** Verify (Pipeline + Tests + CLI).
- **12.11.5:** `meta.ini` в verify (семантическое сравнение).
- **12.11.6:** extensions/extras в verify.
- **12.11.7:** Ctrl+C в CLI (Normalized cancellation + `CancellationHelper`).
- **12.11.7.1:** `ArchiveMatcher.Build` → `BuildAsync`, отмена
  пробрасывается.
- **12.12:** integration pack → install.
- **12.13.1:** ArchiveMatcher + фикс Unmatched root + InternalsVisibleTo.
- **12.13.2:** ScanExtensionsStep + ScanExtrasStep + EntryScanResult.
- **12.13.3:** MatchExtensionsStep + MatchExtrasStep + MatchEntriesResult.
- **12.13.4:** BuildManifestStep + PackPipeline + DI + write unmatched.
- **12.13.5:** integration test pack → install с extensions/extras.
- **12.13.6:** унификация extract-а `MatchStep` через `ArchiveMatcher`.
- **12.13.7 + 12.13.8:** таблицы CLI включают extensions/extras.
- **12.13.10:** error-msg для пробелов без кавычек.
- **13.1:** общий helper скачивания `ArchiveDownloadHelper`.

**Реальный прогон на `OmenRim 7` после 12.13.6 (2026-09-20):**
- Pack: 82 мода, 4236 matched-директив, 69 архивов.
  - extensions (`plugins/curationclub`) — 49/49 matched.
  - extras (`skse64_loader.exe`, `skse64_1_7_104.dll`) — 2/2 matched.
  - 6 unmatched (runtime `.log`/`.ini` от SKSE) → `__Firelink_Output`.
- Install: 82 мода, 82 meta.ini, 1 extension written, 2 extras written.
- Verify: **4338 passed, 0 failed**.
- `ArchiveMatcher` — **один** extract на 70 архивов (12.13.6).

---

## Структура репозитория (после 12.13.10 + 13.1 + 12.11.7)
C:\Code\Firelink
Firelink.slnx
Directory.Build.props
Directory.Build.targets
Directory.Packages.props
DOC.md ← v3.9
PROJECT-STATE.md
HANDOFF.md
repo-dump.md
samples/
firelink-pack.minimal.json
firelink-pack.full.json
firelink-pack.invalid-name.json
firelink-pack.invalid-path.json
firelink-pack.invalid-version.json
src/
Firelink.Core/
Abstractions/ (IStep, IArchiveDownloader)
CancellationHelper.cs ← блок 6
Hashing/ (XxHash64Value, XxHash64ValueJsonConverter)
Models/Manifest/
ArchiveEntry, ModlistManifest, ManifestJson (Load/Save sync),
ManifestSchema, UtcDateTimeOffsetJsonConverter, ModMeta,
Directives/ (Directive, FromArchiveDirective,
CreateDirectoryDirective, DeleteDirective),
Sources/ (ArchiveSourceRef, NexusSourceRef, MirrorSourceRef)
Models/Mo2/ (ModlistEntry, ModlistFile, PluginEntry, PluginsFile,
LoadorderFile)
Models/Pack/ (PackConfig, PackMeta, PackInstance, PackMo2,
PackStockGame, PackArchiveSource, PackConfigJson,
InstanceSnapshot, MatchResult, ModScanResult,
UnmatchedFile, UnmatchedEntry, EntryScanResult,
ArchiveIndex, MatchEntriesResult)
Identity/ (Slug, ArchiveId)
Validation/ (ValidationResult, NameValidator, SemverValidator,
RelativePathValidator, InstancePathValidator,
PackConfigValidator)
Archives/ (ArchiveExtensions, FileHashCache,
ArchiveDownloadHelper, ← блок 5
Extraction/ (IArchiveExtractor, SevenZipExtractor,
TempWorkspace))
Assets/7z/ (7z.exe, 7z.dll, License.txt)
Firelink.Platform.MO2/
Models/MetaFile.cs
Readers/ (ModlistReader, PluginsReader, LoadorderReader,
MetaReader, MetaIniReader)
Writers/ (ModlistWriter, PluginsWriter, LoadorderWriter,
MetaIniWriter; + Serialize)
Firelink.Platform.Nexus/ (пусто)
Firelink.Pack/
Commands/ (PackCommand, HashCommand, DoctorCommand)
Settings/ (PackSettings, DoctorSettings)
Infrastructure/TypeRegistrar.cs
Matching/ (ArchiveMatcher, ArchiveIndexes, Mo2ArchiveBuilder)
Steps/ (ReadConfigStep, ReadInstanceStep, IndexArchivesStep,
ScanModsStep, ScanExtensionsStep, ScanExtrasStep,
MatchStep, MatchExtensionsStep, MatchExtrasStep,
BuildManifestStep, ValidateManifestStep, WriteManifestStep)
PackPipeline.cs
Program.cs (с PropagateExceptions + CommandParseException catch)
Firelink.Install/
Commands/ (InstallCommand, VerifyCommand, DoctorCommand)
Settings/ (InstallSettings, VerifySettings, DoctorSettings)
Infrastructure/TypeRegistrar.cs
Downloaders/ (MirrorDownloader, DownloaderRegistry)
Steps/ (ReadManifestStep, ResolveTargetStep, ValidateTargetStep,
BootstrapInstanceStep, BootstrapMo2Step, SyncArchivesStep,
ExecuteExtensionsStep, ExecuteExtrasStep, SyncModsStep,
GenerateMetaIniStep, RegenerateProfileStep)
Verify/ (VerifyContext, VerifyCheckResult, VerifyReport, VerifyPipeline)
InstallPipeline.cs
Program.cs (с PropagateExceptions + CommandParseException catch)
tests/
Firelink.Core.Tests/
Firelink.Platform.MO2.Tests/
Firelink.Pack.Tests/
ArchiveMatcherTests.cs
ScanExtensionsStepTests.cs
ScanExtrasStepTests.cs
MatchExtensionsStepTests.cs
MatchExtrasStepTests.cs
Firelink.Install.Tests/
Verify/ (VerifyPipelineTests)
Firelink.Integration.Tests/
PackInstallRoundtripTests.cs
PackInstallExtensionsExtrasTests.cs

text

Реальные инстансы:

- `C:\Firelink\TestInstance\` — MVP прогон (до 12.13.x), verify OK.
- `C:\Firelink\TestInstance2\` — после 12.13.6, 82 мода,
  extensions/extras, verify 0 failed.
- `C:\Firelink\OmenRim 7\` — тестовый оригинал.
- Большой инстанс — 4370 модов (прогон E1 отменён).

---

## Пайплайн packer-а (полный)
ReadConfigStep → ReadInstanceStep → IndexArchivesStep → ScanModsStep →
ScanExtensionsStep → ScanExtrasStep →
[build ArchiveMatcher via BuildAsync] →
MatchStep → MatchExtensionsStep → MatchExtrasStep →
[write unmatched extensions/extras] →
BuildManifestStep → ValidateManifestStep → WriteManifestStep

text

---

## Пайплайн installer-а
ReadManifestStep → ResolveTargetStep → ValidateTargetStep →
BootstrapInstanceStep → BootstrapMo2Step → SyncArchivesStep →
ExecuteExtensionsStep → ExecuteExtrasStep → SyncModsStep →
GenerateMetaIniStep → RegenerateProfileStep

text

**CLI:** `firelink-install install <manifest> [--target <dir>]`.
**CLI:** `firelink-install verify <target> [--verbose]`.

---

## Прогоны на реальных инстансах

**`C:\Firelink\TestInstance\` (19.09.2026, до 12.13.x):**

- Install: 82 мода, 68 архивов, 82 meta.ini, профиль `Default`.
- Verify: 4337 passed (здоровый), 4310/2 (сломанный), install
  восстанавливает.

**`C:\Firelink\TestInstance2\` (20.09.2026, после 12.13.6, `OmenRim 7`):**

- Pack: 82 мода, 4236 matched, 69 архивов, 49/49 extensions, 2/2 extras.
- Install: 82 мода, 1 extension written, 2 extras written, 82 meta.ini.
- Verify: **4338 passed, 0 failed**.

---

## CLI: сценарии и exit codes

| Сценарий | Вывод | Exit code |
|---|---|---|
| `pack` OK | таблица, `Done.` | 0 |
| `pack` с пробелами без кавычек | `CLI error` + hint | 2 |
| `pack` без `<config>` | `ERROR: Command 'pack' is missing required argument 'config'.` | 2 |
| `pack` + Ctrl+C | `Cancelled.` | 130 |
| `install` OK | таблица, `Done.` | 0 |
| `install` + Ctrl+C | `Cancelled.` | 130 |
| `verify` OK | `All checks passed.` | 0 |
| `verify` с падениями | summary + failures | 1 |

---

## Что в работе

Ничего. Все хвосты MVP закрыты.

---

## Следующий блок

**12.8 — NexusDownloader.** Подробное описание — в `HANDOFF.md`,
раздел «План работы».

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — v0.2.0.
- Nexus API — **12.8**.
- Прогресс-бар Spectre — v0.2.0.
- `Firelink.Platform.Nexus` пуст — до 12.8.
- `SyncModsStep` поддерживает только `FromArchive`.
- `ConfigureMo2Step` — не делаем.
- E1 (прогон на большом инстансе) — отменён по решению.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже).
- Visual Studio 2022.
- `C:\Firelink\TestInstance\` — MVP прогон (до 12.13.x).
- `C:\Firelink\TestInstance2\` — после 12.13.6, extensions/extras,
  verify 0 failed.
- `C:\Firelink\OmenRim 7\` — тестовый оригинал.
- Большой инстанс — 4370 модов (не используется).
