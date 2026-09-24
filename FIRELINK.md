# Firelink — состояние проекта и план работ

**Обновлено:** 2026-09-24
**Всего тестов:** 759, 0 failed
**Текущий блок:** — (Фаза 3, шаги 3.1–3.7 закрыты)
**Следующий блок:** Фаза 3 — шаг 3.8 (ручной прогон GUI на TestInstance5)

**Спутние документы:**
- `DOC.md` (v4.4) — формальная документация: форматы, pipeline, CLI, обработка ошибок.
- `repo-dump.md` — свежий дамп репозитория.

---

## Что это

Firelink — инструмент для создания и установки воспроизводимых сборок
модов для Mod Organizer 2. C#/.NET 8.

Ключевая идея: манифест `modlist.json` — единственный источник правды.
Файлы восстанавливаются по хешам `xxHash64`. Firelink работает с
**результатом** установки, а не с процессом.

Один CLI (`Firelink.Cli`, exe → `Firelink.Cli.exe`) + в будущем один GUI
(Avalonia, exe → `Firelink.exe`). Логика — в библиотеках, интерфейсы
(CLI, GUI) — отдельные слои.

---

## Как начать работу в новом чате

**Скопируйте в первое сообщение:**

1. **FIRELINK.md** (этот файл) — полностью.
2. **DOC.md** — полностью.
3. **repo-dump.md** — свежий.

**Первое сообщение — шаблон:**

Продолжаем проект Firelink. Стиль — пошаговые блоки кода с тестами.

Прикладываю: FIRELINK.md, DOC.md (v4.5), repo-dump.md (свежий).

Текущее состояние: 759 тестов, 0 failed. Закрыты: MVP (packer,
installer, verify), Фаза 1 (единый CLI Firelink.Cli),
Фаза 2 (общие API для GUI), Фаза 6 (Nexus Premium), а также
Фаза 3 — шаги 3.1–3.7:

3.1 — проекты GUI + DI + базовые VM (LoadingLock,
FilePickerVM, ProgressViewModel, ObservableLoggerProvider).

3.2 — главное окно с навигацией (MainWindowVM, NavigationVM,
ScreenType, ViewLocator, плейсхолдеры).

3.3 — живые логи в UI (LogVM, LogView, автоскролл).

3.4.1 — инфраструктура: IScreenFactory, INavigationAware,
IFilePickerService, AvaloniaFilePickerService, ScreenFactory.

3.4.2 — экран Install: InstallVM, InstallView, IInstallRunner,
проект Firelink.Gui.Controls (общие контролы LogView,
FilePickerView).

3.5 — экран Pack: PackVM, PackView, IPackRunner/PackRunner,
проект Firelink.Gui.Pack, AddGuiPack(), удаление
PackPlaceholderVM/PackPlaceholderView.

3.5.1 — потокобезопасный лог-канал: IUiDispatcher +
ObservableLogSink с маршалингом мутаций на UI-поток,
AvaloniaUiDispatcher в Firelink.Gui. Устранена гонка
ObservableCollection vs worker-потоки pipeline.

3.6 — экран Verify: VerifyVM, VerifyView, IVerifyRunner/VerifyRunner,
проект Firelink.Gui.Verify, AddGuiVerify(),
VerifyRowVM (строка таблицы проверок), удаление
VerifyPlaceholderVM/VerifyPlaceholderView.

3.7 — дистрибутив: Directory.Build.props (VersionPrefix 0.1.0,
IncludeSourceRevisionInInformationalVersion=false), иконка GUI
(ApplicationIcon Assets\app.ico), версия CLI из assembly
(Firelink.Cli/Program.cs GetApplicationVersion()),
tools/build-release.bat (publish GUI+CLI в одну папку,
win-x64, framework-dependent, Compress-Archive → zip).
Тестов не добавляли — 759 passed сохраняется.

Следующая задача: Фаза 3 — шаг 3.8 (ручной прогон GUI на TestInstance5).

Стиль ответов:

Разбор задачи.

Полный код файлов с путями.

Инструкция по сборке/тестам.

Ожидаемый вывод dotnet test.

FIRELINK.md — только по запросу.

Не пиши код, пока я не подтвержу готовность.

---

## Стиль работы

- **Файлы давать целиком**, не патчами.
- **Запускать `dotnet test` сразу** после каждого блока.
- **Присылать полный вывод** тестов при падении (текст).
- **FIRELINK.md** — обновлять по запросу.
- **Не менять архитектурные решения без обсуждения.**
- **Не отвечать на китайском.**
- **Разбивать крупные блоки на 12.x.y** (или `<Фаза>.<шаг>.<подшаг>`).
- **Не писать код, пока не подтверждена готовность.**

---

## Стек

- **.NET 8**, C# 12.
- **xUnit + FluentAssertions**.
- **Spectre.Console.Cli** 0.48.0 (с `PropagateExceptions`).
- **System.Text.Json**.
- **System.IO.Hashing (xxHash64)**.
- **Microsoft.Data.Sqlite** (v0.2.0).
- **Microsoft.Extensions.*** — DI, Logging, Http.
- **Polly** (в `Firelink.Core`).
- **7z.exe + 7z.dll**.
- **SharpCompress удалён.**
- **Octokit удалён.**

### GUI (Фаза 3)
- **Avalonia** 11.2.1 (`Avalonia`, `Avalonia.Desktop`,
  `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`,
  `Avalonia.Diagnostics` Debug-only).
- **CommunityToolkit.Mvvm** 8.4.0 (source generators входят в основной пакет).
- **Microsoft.Extensions.DependencyInjection**.
- **Microsoft.Extensions.Logging**.
- **AvaloniaUseCompiledBindingsByDefault=true** во всех GUI-проектах.

---

## Текущий статус

### Готово

- **Полный pipeline packer-а (13 шагов).**
- **Полный pipeline installer-а (11 шагов).**
- **Verify** (pipeline + tests + CLI), включая `meta.ini`, extensions,
  extras.
- **Ctrl+C** в CLI, `Cancelled.`, exit 130.
- **Error-msg для пробелов без кавычек.**
- **593 теста, 0 failed.**
- **Реальный прогон на `OmenRim 7`** (pack → install → verify,
  4338 passed, 0 failed).
- **Фаза 1 целиком — единый CLI `Firelink.Cli.exe`:**
  - 1.1 — создан `Firelink.Cli`, pack-сторона перенесена.
  - 1.2 — install-сторона перенесена, все 5 команд работают.
  - 1.3 — `Firelink.Pack` стал class library.
  - 1.4 — `Firelink.Install` стал class library.
  - 1.5 — `AddFirelinkPack` / `AddFirelinkInstall`.
  - 1.6 — ручной прогон на OmenRim 7: pack → install (TestInstance3)
    → verify, 4338 passed, 0 failed. Идентично `TestInstance2`.
  - 1.7 — Ctrl+C на pack/install/verify (`Cancelled.`, exit 130),
    пробелы без кавычек (`CLI error` + hint, exit 2). Регрессий нет.
  - **Фаза 2 — общие API для будущего GUI (шаги 2.1–2.4):**
  - 2.1 — `StepProgress` + `IProgress<StepProgress>?` в обоих pipeline-ах.
  - 2.2 — `PackInputFactory` / `InstallInputFactory`.
    `PackPipeline.Input` введён (симметрично `InstallPipeline.Input`).
  - 2.3 — `PackSummary` / `InstallSummary` + Builder-ы.
  - 2.4 — реальный прогон на OmenRim 7 (TestInstance4) + verify.
  - **Фаза 6 — Nexus Premium (12.8):**
  - 12.8.1 — `NexusApiKeyProvider` (`INexusApiKeyProvider`,
    `NullNexusApiKeyProvider`, чтение `%USERPROFILE%\.firelink\nexus.key`).
  - 12.8.2 — `NexusClient` (`IsPremiumAsync` с кешем,
    `GetDownloadLinksAsync`, модели ответов).
  - 12.8.3 — `NexusDownloader : IArchiveDownloader` (`SourceType =>
    "nexus"`, перебор CDN-нод).
  - 12.8.4 — DI в `AddFirelinkInstall` (`TryAddEnumerable` для
    `IArchiveDownloader`, именованные `HttpClient`-ы).
  - 12.8.4a — рефакторинг:
    - downloader-ы принимают `IHttpClientFactory` (не `HttpClient`);
    - `TempFileStream` вместо `MemoryStream` (архивы > 2 ГБ);
    - кеш `IsPremiumAsync` в `NexusClient`.
  - 12.8.5 — `DOC.md` v4.3, обновление `FIRELINK.md`.
  - Ручной прогон на `TestInstance5` — 4 nexus-архива скачаны, USSEP
    (~250 МБ) без таймаутов.
  - **Фаза 3 — GUI (Avalonia), частично закрыта:**
  - 3.1 — проекты GUI + DI + базовые VM.
    Итог: 688 passed.
  - 3.2 — главное окно с навигацией.
    Итог: 700 passed.
  - 3.3 — живые логи в UI.
    Итог: 710 passed.
  - 3.4.1 — инфраструктура навигации и file picker.
    Итог: 716 passed.
  - 3.4.2 — экран Install (полный: диалоги, прогресс, сводка, лог).
    Итог: 731 passed.
  - 3.5 — экран Pack (симметрично Install) + удаление
    Pack-плейсхолдеров.
    Итог: 739 passed (после удаления тестов на плейсхолдеры).
  - 3.5.1 — потокобезопасный лог-канал (IUiDispatcher).
    Итог: 744 passed.
  - 3.6 — экран Verify (таблица проверок + Show all checks) +
    удаление Verify-плейсхолдеров.
    Итог: 759 passed.
  - 3.7 — дистрибутив: Directory.Build.props (VersionPrefix),
    иконка GUI, версия CLI из assembly, tools/build-release.bat.
    Итог: 759 passed (тестов не добавляли — шаг чисто
    инфраструктурный).

### В работе

- Фаза 3 — GUI. Шаг 3.8 (ручной прогон GUI на TestInstance5) —
  впереди.

### Не начато

- Фаза 3 — GUI (Avalonia).
- Фаза 5 — Nexus Free (WebView2).

### Вычеркнуто

- **Фаза 4 — вариации дистрибутивов.** Решение 2026-09-21: не делаем.
  Один CLI, один GUI, один набор exe в дистрибутиве.
- **E1 (прогон на большом инстансе 4370 модов)** — отменён, не критично.

---

## Структура репозитория (после шагов 1.1–1.4)

```
    src/
      Firelink.Core/
      Firelink.Platform.MO2/
      Firelink.Platform.Nexus/
      Firelink.Pack/
      Firelink.Install/
      Firelink.Cli/              ← exe → Firelink.Cli.exe
      Firelink.Gui.Shared/       ← MVVM-инфра, БЕЗ Avalonia
      Firelink.Gui.Controls/     ← общие Avalonia-контролы
      Firelink.Gui.Install/      ← модуль installer
      Firelink.Gui.Pack/         ← модуль packer
      Firelink.Gui.Verify/       ← модуль verify
      Firelink.Gui/              ← exe → Firelink.exe
    tests/
      Firelink.Core.Tests/
      Firelink.Platform.MO2.Tests/
      Firelink.Platform.Nexus.Tests/
      Firelink.Pack.Tests/
      Firelink.Install.Tests/
      Firelink.Integration.Tests/
      Firelink.Gui.Shared.Tests/
      Firelink.Gui.Install.Tests/
      Firelink.Gui.Pack.Tests/
      Firelink.Gui.Verify.Tests/
```

После шага 1.4 — **только один exe** в solution: `Firelink.Cli.exe`.
`Firelink.Pack.exe` и `Firelink.Install.exe` больше нет. Команды:
`firelink pack`, `firelink install`, `firelink verify`, `firelink hash`,
`firelink doctor` (usage-строка; имя exe — `Firelink.Cli.exe`).

---

## Пайплайн packer-а (полный)

```
ReadConfigStep → ReadInstanceStep → IndexArchivesStep → ScanModsStep →
ScanExtensionsStep → ScanExtrasStep →
[build ArchiveMatcher via BuildAsync] →
MatchStep → MatchExtensionsStep → MatchExtrasStep →
[write unmatched extensions/extras] →
BuildManifestStep → ValidateManifestStep → WriteManifestStep
```

## Пайплайн installer-а

```
ReadManifestStep → ResolveTargetStep → ValidateTargetStep →
BootstrapInstanceStep → BootstrapMo2Step → SyncArchivesStep →
ExecuteExtensionsStep → ExecuteExtrasStep → SyncModsStep →
GenerateMetaIniStep → RegenerateProfileStep
```

**CLI:** `firelink install <manifest> [--target <dir>]`.
**CLI:** `firelink verify <target> [--verbose]`.

## GUI-навигация (Фаза 3)

MainWindow (Grid: sidebar + content)
  ├── NavigationView (DataContext = NavigationVM)
  │     └── ListBox с NavigationItem[Home|Install|Pack|Verify]
  └── ContentControl (Content = MainWindowVM.ActivePane)
        └── ViewLocator → View

MainWindowVM:
  IScreenFactory.Create(ScreenType) → object (VM)
  NavigateTo(screen):
    pane = screens.Create(screen)
    if (pane is INavigationAware) pane.SetNavigateHome(...)
    ActivePane = pane
    Navigation.SelectScreen(screen)

ScreenFactory (в Firelink.Gui):
  Home    → HomeVM
  Install → InstallVM          (Firelink.Gui.Install)
  Pack    → PackVM             (Firelink.Gui.Pack)
  Verify  → VerifyVM           (Firelink.Gui.Verify)

ViewLocator:
  param.ViewModels.XxxVM → ищет Control с FullName *".Views.XxxView"
  через перебор всех загруженных сборок.

Плейсхолдеров больше нет — все 4 экрана реальные.

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

**Перепроверка через `Firelink.Cli.exe install ... --target ...` (21.09.2026,
после шага 1.2):**

- Archives already present: 69.
- Mods skipped: 82. meta.ini written: 82.
- Extensions/extras: 0 written, 1/2 skipped.
- Done.

**Важно:** `install` без `--target` создаёт инстанс в
`<exeDir>/Instances/<meta.name>/`, а не рядом с манифестом. Это
архитектурное решение (см. решение №52). Для переустановки поверх
существующего инстанса — всегда указывать `--target`.

---

## Ключевые архитектурные решения (не переделывать)

Ниже — накопленный список. Сгруппирован по темам, но **пункты
сохранены все**. Нумерация — историческая, чтобы не сбиться при
ссылках.

### Общие принципы (1–19)

1. **Манифест — единственный источник правды.** Профиль MO2
   генерируется из манифеста, а не копируется.
2. **Файлы восстанавливаются по хешам** (`xxHash64`), не по именам
   и путям.
3. **Firelink работает с результатом, а не с процессом.** Никаких
   FOMOD-парсеров, XML, `meta.ini` (читается только для метаданных).
4. **Одна папка `downloads/` для всех архивов.** Моды, MO2, extras —
   всё в одном месте.
5. **Идентификация архивов — канонический id.** Не по имени файла:
   `nexus_{game}_{modId}_{fileId}` / `local_{slug}`.
6. **Глобальный реестр архивов** — SQLite в `%USERPROFILE%\.firelink\archives.db`
   (v0.2.0).
7. **Ничего не удаляем** из `downloads/`.
8. **Директивы выполняются последовательно.** `lastWins`.
9. **`[NoDelete]` в имени папки** защищает пользовательские моды.
10. **BSA/BA2 — единые файлы.** Не разбираем содержимое.
11. **Никаких исполняемых скриптов.** Только декларативные директивы.
12. **Installer идемпотентен.**
13. **Installer не работает с игрой.** `Stock Game/` — просто папка.
14. **Шаги pipeline изолированы.** Pipeline — единственный
    оркестратор.
15. **Nexus — один источник, несколько стратегий доступа.**
16. **Параллелизм на уровне pipeline** (`Parallel.ForEach`).
17. **Кеш хешей обязателен** (`FileHashCache`, in-memory; persist —
    v0.2.0).
18. **Манифест самодостаточен.** Installer не ходит на Nexus за
    метаданными.
19. **Unmatched → `__Firelink_Output`.** Не `InlineFile`, не base64.

### Packer (20–51)

20. **`meta.game` = Nexus game domain.**
21. **`instance.path`** — относительный.
22. **`mo2.profile`** — обязательный.
23. **`mo2.source` — обязательно `MirrorSourceRef`.**
24. **`mo2.archive.size/hash` — из `mo2.source.hash`** (size с диска,
    если файл есть).
25. **MO2-архив НЕ попадает в `manifest.Archives[]`.**
26. **`mo2.extensions`** — от `MO2/`. **`stockGame.extras`** — от
    `Stock Game/`.
27. **`.meta`** — Nexus-формат. `MetaReader.TryRead`.
28. **Канонический id:** `nexus_...` / `local_{slug}`.
29. **`archiveSources`** вместо `mirrors`.
30. **Slug** — ASCII-only.
31. **Semver** — регулярка semver.org.
32. **`Pack*`-модели** — `record`.
33. **`ValidationResult`** — накапливает ошибки.
34. **Trailing slash** разрешён.
35. **Зарезервированные имена Windows** запрещены.
36. **Модели MO2** — в `Core.Models.Mo2`.
37. **CLI** — Spectre.Console.Cli + `TypeRegistrar` + `PropagateExceptions`.
38. **CLI требует явной команды.**
39. **Структура инстанса:** `MO2/`, `Stock Game/`, `__Firelink_Output/`.
40. **`ModlistReader`** читает все строки.
41. **`[NoDelete]`** — через `Contains`.
42. **`.meta` не считается архивом.**
43. **Невалидный `.meta`** → fallback.
44. **`MirrorSourceRef.Hash` обязателен.**
45. **`XxHash64Value`** — `xxh64:hex`, 16 символов.
46. **`MatchStep`** делегирует матчинг `ArchiveMatcher`-у.
47. **`meta.ini` в корне мода** → `ModMetas`.
48. **Unmatched модов** → `__Firelink_Output/MO2/mods/<ModName>/<path>`.
49. **`7z.exe` + `7z.dll`** через `Directory.Build.targets`.
50. **`SevenZipExtractor`** — таймаут 10 минут.
51. **`__Firelink_Output`** — каталог автора.

### Pack-модели и валидация (52–73)

52. **`MatchResult`** — `ModDirectives` + `Unmatched` + `ModMetas`.
53. **`MetaIniReader`** — `[General]`. Ключи case-insensitive.
54. **`MetaIniWriter`** — `[General]`, camelCase, без
    `[installedFiles]`, без BOM, CRLF. Не пишет `category`.
55. **`ModMeta` — без `Category`.**
56. **`.mohidden` — часть пути.**
57. **`mods[].meta` со всеми полями** (кроме `Category`).
58. **`createdAt`** — `yyyy-MM-ddTHH:mm:ss.fffZ`.
59. **`InlineFileContent` / `OrphanFile` / `InlineFile` удалены.**
60. **`PackCommand`:** `Manifest archives`, `Manifest extensions`,
    `Manifest extras`, `mo2.archive`, `Unmatched written to` при `> 0`.
61. **Сепараторы** сохраняются в манифесте.
62. **`ScanModsStep`:** сепаратор без папки → `LogDebug`.
63. **Мод без восстановимых файлов остаётся в манифесте с пустыми
    директивами.**
64. **`ArchiveMatcher`** — в `Firelink.Pack.Matching`. Детерминизм
    при дубликатах: минимальный `archiveId` (Ordinal).
    `InternalsVisibleTo("Firelink.Pack.Tests")`. Принимает
    `ILogger<ArchiveMatcher>`. Метод — `BuildAsync(ct)`.
65. **`ScanExtensionsStep`/`ScanExtrasStep`** — тип результата
    `EntryScanResult`. `RelativePath` файла — ОТ КОРНЯ `MO2/` или
    `Stock Game/`. `Parallel.ForEach` + восстановление порядка
    ключей. Отсутствие entry → `FileNotFoundException`.
66. **`MatchExtensionsStep`/`MatchExtrasStep`** — тип результата
    `MatchEntriesResult`. Принимают `ArchiveMatcher` через
    `Input.Matcher` (`internal`, не `required`). Unmatched **не
    пишут** на диск — это делает `PackPipeline`.
67. **`Mo2ArchiveBuilder`** — статический класс в
    `Firelink.Pack.Matching`. Один источник правды для построения
    `ArchiveEntry` MO2-архива.
68. **Unmatched extensions** → `__Firelink_Output/MO2/<relativePath>`
    (плоско). **Unmatched extras** → `__Firelink_Output/Stock Game/<relativePath>`.
    `EntryName` на путь не влияет — только `RelativePath`.
69. **`PackPipeline`** перед write unmatched чистит
    `__Firelink_Output/MO2/` (кроме `mods/`) и
    `__Firelink_Output/Stock Game/`. Папки `MO2/` (без `mods/`) и
    `Stock Game/` создаются **только** при `entries.Count > 0`.
70. **`PackPipeline`** создаёт `ArchiveMatcher` один раз, `BuildAsync`,
    передаёт в `MatchStep`, `MatchExtensions`, `MatchExtras`.
71. **`BuildManifestStep.BuildExtensions`/`BuildExtras`** пропускают
    entry с пустым списком директив.
72. **`PackPipeline`** добавляет MO2-архив в `ArchiveIndex` перед
    созданием `ArchiveMatcher`: `archiveIndexWithMo2 =
    AddMo2ArchiveToResolved(...)`.
73. **`Slug.FromFileName`** отрезает **последнее** расширение до
    slug-ификации.

### Installer (74–113)

74. **Инстансы:** `<exeDir>/Instances/<normalize(meta.name)>/`.
75. **Копирование манифеста** — `ResolveTargetStep`.
76. **`--target <dir>`** — escape-hatch.
77. **`meta.name` перепроверяется.**
78. **`ValidateTargetStep`** — 4 проверки.
79. **`[NoDelete]`** — уважаем.
80. **Существующая папка** — рефлорация.
81. **`verify` — да, `repair` — нет.**
82. **File-logging — v0.3.0.**
83. **Прогресс-бары — v0.2.0.**
84. **`BootstrapInstanceStep`** — создаёт все папки.
85. **`IArchiveDownloader`** — абстракция.
86. **`DownloaderRegistry`** — map sourceType → downloader.
87. **`SyncArchivesStep`** — только `manifest.Archives`, не трогает
    MO2.
88. **Hash — источник правды.**
89. **Ничего не удаляем** из `downloads/`.
90. **GitHub — удалён.** Всё через `mirror`.
91. **Параллельная загрузка** — `ParallelOptions`.
92. **3 попытки + Polly backoff (2 сек).**
93. **Скачивание в `.part`**, `File.Move` после проверки.
94. **Проверка хеша после скачивания обязательна.**
95. **`nexus` до Фазы 6 — warning + `Skipped`.**
96. **Глобальный реестр — v0.2.0.**
97. **`IHttpClientFactory` через DI.**
98. **`SyncModsStep`** — reconcile `mods/`.
99. **Только `FromArchive` директивы.**
100. **`TempWorkspace` на мод.**
101. **Файлы, которых нет в директивах, — удаляются** (recreate).
102. **Откат при ошибке — никакого.**
103. **`SyncModsStep.Input.ArchivesById`.**
104. **`SyncModsStep.Output`:** `Created`/`Recreated`/`Skipped`/`Deleted`.
105. **Сепараторы в `SyncModsStep`:** pass 1 — `Skipped`; pass 2 —
     не удаляются.
106. **`GenerateMetaIniStep`:** reconcile `meta.ini`.
107. **`RegenerateProfileStep`:** сортировка по `Order` ascending,
     **без `Reverse()`**.
108. **`BootstrapMo2Step` — самодостаточный.** Распаковка всегда,
     с заменой.
109. **`BootstrapMo2Step.Output = Input`.**
110. **Порядок pipeline:** BootstrapInstance → BootstrapMo2 →
     SyncArchives → ExecuteExtensions → ExecuteExtras → SyncMods →
     GenerateMetaIni → RegenerateProfile.
111. **`ConfigureMo2Step`** — не делаем.
112. **`InstallPipeline`** — склейка шагов.
113. **`InstallPipeline.BuildArchivesById`** — включая MO2-архив.

### Verify (114–130)

114. **Verify — read-only.**
115. **Изоляция (вариант A).**
116. **`VerifyPipeline.Execute` — синхронный.**
117. **Проверки — приватные методы внутри `VerifyPipeline`.**
118. **`VerifyContext`** — единый контекст.
119. **`VerifyReport`** — `Checks`, `IsOk`, `PassedCount`, `FailedCount`.
120. **`VerifyCheckResult`** — `Name`, `Passed`, `Message`.
121. **Регенерация modlist.txt / plugins.txt / loadorder.txt в память**
     через `Serialize`-методы writer-ов.
122. **Сравнение `meta.ini`** — семантическое.
123. **`schemaVersion`** — через `ManifestSchema.IsSupported`.
124. **Пустые директивы у disabled-мода** — OK.
125. **Return code CLI:** 0 — OK, 1 — есть падения, 2 — ошибка, 130 — Ctrl+C.
126. **`ManifestJson.Load` / `Save` — sync-версии для verify.**
127. **`VerifyCommand`** — summary + провалы; `--verbose` — все проверки.
      `Markup.Escape`.
128. **`VerifySettings`** — `<target>` + `--verbose`/`-v`.
129. **`CheckMod` делает `yield break`** при отсутствии папки мода.
130. **Verify проверяет extensions/extras.** Общий helper
      `CheckDirectiveFile(displayPrefix, rootPath, directive)`.

### Cancellation / CLI (131–136)

131. **`CancellationHelper.IsCancellation`** в `Firelink.Core` —
      распознаёт отмену, включая `AggregateException` со всеми
      cancellation-inner.
132. **`ArchiveMatcher.BuildAsync`** — async; `catch (OperationCanceledException)
      { throw; }` **перед** `catch (Exception)`.
133. **`PackCommand`/`InstallCommand`/`VerifyCommand`** — `catch (Exception ex)
      when (CancellationHelper.IsCancellation(ex))` → `Cancelled.` + exit 130.
      `Console.CancelKeyPress` подписывается на время выполнения команды,
      `e.Cancel = true` + `cts.Cancel()`, отписка в `finally`.
134. **`Program.cs` CLI** — `catch (Exception ex)
      when (CancellationHelper.IsCancellation(ex))` → `Cancelled.` + exit 130
      (fallback на случай отмены до подписки).
135. **`config.PropagateExceptions()`** включено в CLI. Без него
      Spectre перехватывает `CommandParseException` и
      `OperationCanceledException` до нашего `try/catch`.
136. **`catch (CommandParseException ex)`** в `Program.cs` — печатает
      `CLI error: <msg>` + `Hint: paths with spaces must be quoted: ...`
      (hint только если в `argv` нет строк с пробелами).

### 12.13.x / 13.1 (137–143)

137. **`MatchStep` использует общий `ArchiveMatcher` через
     `Input.Matcher`.**
138. **`.bsa`/`.ba2` — единые файлы, не контейнеры.**
139. **CLI-таблицы `PackCommand`/`InstallCommand` включают
     extensions/extras.**
140. **`ArchiveDownloadHelper`** в `Firelink.Core.Archives` —
     общий helper скачивания (`.part`, retry через Polly, hash-check).
141. **`ExecuteExtensionsStep`** — раскладывает `manifest.Mo2.Extensions[]`
     в `<target>/MO2/`. Группирует директивы по archiveId, extract
     один раз на архив, `FileMatches`-skip для идемпотентности.
142. **`ExecuteExtrasStep`** — симметричен `ExecuteExtensionsStep`,
     корень `<target>/Stock Game/`.
143. **`ExecuteExtensionsStep`/`ExecuteExtrasStep` — Skipped**, если
     файлы уже на месте.

### Фаза 1 — Единый CLI (144–152)

Решения, принятые при рефакторинге CLI (шаги 1.1–1.7, 2026-09-21).

144. **Один CLI-проект:** `Firelink.Cli`, exe → `Firelink.Cli.exe`.
     `<AssemblyName>` не задаём — имя exe берётся из имени `.csproj`.
145. **`Firelink.Pack` и `Firelink.Install` — class libraries.**
     Не exe. Все `Program.cs`, `Commands/`, `Settings/`,
     `Infrastructure/` переехали в `Firelink.Cli`.
146. **Namespace `Firelink.Cli.*`.** Команды —
     `Firelink.Cli.Commands`, настройки — `Firelink.Cli.Settings`,
     `TypeRegistrar` — `Firelink.Cli.Infrastructure`.
147. **`SetApplicationName("firelink")`** — usage-строка `firelink pack`,
     `firelink install` и т.д. Имя exe — `Firelink.Cli.exe`. Это
     сознательное расхождение display name и file name (как `dotnet`
     vs `dotnet.exe`).
148. **`Firelink.exe`** зарезервировано под GUI (Фаза 3).
     `<AssemblyName>Firelink</AssemblyName>` будет у `Firelink.Gui.csproj`.
     В дистрибутиве будут оба exe: `Firelink.Cli.exe` (CLI) и
     `Firelink.exe` (GUI).
149. **`TryAddSingleton` вместо `AddSingleton`** для
     `FileHashCache`, `IArchiveExtractor` — эти регистрации
     встречаются и в pack-, и в install-части. `TryAdd` не даёт
     плодить дубли в DI-контейнере.
150. **DI-extension-методы: `AddFirelinkPack` / `AddFirelinkInstall`.**
     Регистрируют все сервисы своей библиотеки.
     Файлы: `Firelink.Pack/PackServices.cs`,
     `Firelink.Install/InstallServices.cs`.
     Namespace'ы — `Firelink.Pack` / `Firelink.Install` (то есть
     extension-метод виден из CLI без лишних using'ов).
151. **`AddFirelinkInstall` регистрирует `MirrorDownloader`** через
     `AddHttpClient<T>` (таймаут 10 минут). Требует
     `Microsoft.Extensions.Http` — явная `<PackageReference>` в
     `Firelink.Install.csproj`.
152. **`Firelink.Cli/Program.cs` регистрирует только то, что
     относится к CLI:** `AddLogging`, `IAnsiConsole`, `ParallelOptions`.
     Всё остальное — через `AddFirelinkPack()` и `AddFirelinkInstall()`.

### Фаза 2 — Общие API (153–160)

Решения, принятые при подготовке общих API для GUI (шаги 2.1–2.4,
2026-09-21).

153. **`StepProgress` — общий тип в `Firelink.Core.Progress`.** Record
     `(int StepIndex, int TotalSteps, string StepName)`. StepIndex
     — 1-based. StepName — стабильный контракт для GUI: имена не
     менять без причины.
154. **`IProgress<StepProgress>? progress = null` — опциональный
     последний параметр** в `PackPipeline.ExecuteAsync` и
     `InstallPipeline.ExecuteAsync`. `Report` вызывается **перед**
     шагом, не после. Packer: 14 имён шагов. Installer: 11.
155. **`PackPipeline.Input` введён.** Симметрично
     `InstallPipeline.Input`. `ExecuteAsync(Input, ct, progress?)`.
     Все тесты packer-а обновлены.
156. **`PackInputFactory` / `InstallInputFactory`** — статические
     классы. Единственная точка, где `Path.GetFullPath` и
     `ParallelOptions`. Принимают `ParallelOptions? = null`,
     дефолт — `Environment.ProcessorCount`.
157. **`PackSummary` / `InstallSummary`** — `sealed record` с
     `required` полями. Только примитивы. Никаких ссылок на
     `PackResult`/`InstallPipeline.Output`. Плоские DTO для
     отображения.
158. **`PackSummaryBuilder` / `InstallSummaryBuilder`** —
     статические классы. Единственное место, где решается
     «что показывать пользователю». Логика подсчёта
     `DirectivesTotal`/`DirectivesFromArchive` (была в
     `PackCommand`) ушла сюда.
159. **CLI-таблицы строятся из Summary, не из Output.**
     Внешний вид таблиц не изменился. Поменялся только источник
     данных: `summary.X` вместо `output.Xxx.Yyy.Count`.
160. **`ParallelOptions` остаётся в DI.** Фабрики принимают его
     параметром. CLI передаёт `_parallelOptions` из DI. GUI
     сможет передавать свой.

### Фаза 6 — Nexus Premium (161–167)

Решения, принятые при реализации 12.8 (2026-09-22).

161. **`NexusDownloader` перебирает CDN-ноды внутри `DownloadAsync`.**
     Не расширяем `IArchiveDownloader` до перебора источников.
     Nexus API отдаёт массив `URI`; логика «попробовать следующую» —
     деталь реализации источника nexus, а не общая механика
     pipeline-а. `ArchiveDownloadHelper` по-прежнему делает retry
     на уровне «попытка скачать весь архив».

162. **`NexusClient` и `NexusDownloader` используют разные именованные
     `HttpClient`-ы.** Через `IHttpClientFactory`:
     `"nexus-api"` — 2 минуты, `"nexus"` — 10 минут, `"mirror"` —
     10 минут.

163. **`IArchiveDownloader` регистрируется через `TryAddEnumerable`
     с явным `ImplementationType`.**
     `TryAddSingleton<IArchiveDownloader>` регистрирует только одну
     реализацию — вторая молча теряется. `TryAddEnumerable` с фабрикой
     падает: у descriptor-а `ImplementationType == null`, и вторая
     регистрация считается дубликатом. Правильно:
     `ServiceDescriptor.Singleton<IArchiveDownloader, TConcrete>()`.

164. **Nexus API-ключ — plaintext-файл
     `%USERPROFILE%\.firelink\nexus.key`.** Одна строка, trim,
     BOM-agnostic. `INexusApiKeyProvider` — абстракция для тестов.
     DPAPI и SQLite — v0.2.0 (в техдолге).

165. **Downloader-ы принимают `IHttpClientFactory`, а не `HttpClient`.**
     `TryAddEnumerable(ServiceDescriptor.Singleton<IArchiveDownloader,
     TConcrete>())` создаёт downloader через активацию конструктора.
     Если конструктор принимает `HttpClient`, DI подставит
     **безымянный** `HttpClient.Default` (таймаут 100 секунд) — этого
     мало для гигабайтных архивов. `IHttpClientFactory.CreateClient(
     "nexus")` даёт клиент с 10-минутным таймаутом.

166. **Скачивание идёт в `TempFileStream`, а не в `MemoryStream`.**
     `MemoryStream` не держит файлы > ~2 ГБ (`int.MaxValue`). У Nexus
     есть моды на 3+ ГБ. `TempFileStream` создаёт файл в системном
     temp с `FileOptions.DeleteOnClose` — O(1) памяти, любые размеры.
     Цена — двойная запись: downloader → temp → `.part`.

167. **`NexusClient.IsPremiumAsync` кешируется на время жизни клиента.**
     Первый успешный запрос к `/users/validate.json` запоминается как
     `Task<bool>`; параллельные вызовы ждут ту же Task — один
     HTTP-запрос на весь pipeline. Faulted/canceled результат
     **не** кешируется: retry не должен получить «отравленный» результат.

### Фаза 3 — GUI (168–181)

168. Стек GUI: Avalonia 11.2.1 + CommunityToolkit.Mvvm 8.4.0,
     никаких ReactiveUI/DynamicData/MessageBus.
169. `<AssemblyName>Firelink</AssemblyName>` у `Firelink.Gui.csproj`.
     Имя exe — `Firelink.exe`, root namespace — `Firelink.Gui`.
     Сознательное расхождение.
170. Отказ от `IGuiModule` в пользу плоского `MainWindowVM.ActivePane`
     + `ScreenType` enum.
171. Навигация — прямой `MainWindowVM.NavigateTo(ScreenType)`,
     без MessageBus. `NavigationVM` принимает `Action<ScreenType>`.
172. Lazy-резолв панелей через `IScreenFactory`. VM — singleton
     в DI, состояние сохраняется между переходами.
173. `INavigationAware` вместо `event HomeRequested`.
174. `IFilePickerService` в Shared, `AvaloniaFilePickerService`
     в Gui.
175. `Firelink.Gui.Shared` — без Avalonia.
176. `Firelink.Gui.Controls` — отдельный проект для общих контролов.
177. `IInstallRunner` в `Firelink.Gui.Install.Services` — обёртка
     над `InstallPipeline`. Тестируемость через fake runner.
178. `LogVM.Clear()` — публичный метод (`[RelayCommand] public void Clear()`).
179. `ViewLocator` — перебор по всем загруженным сборкам,
     поиск по суффиксу `.Views.<ShortName>`. Обрабатывает
     `ReflectionTypeLoadException`.
180. `ObservableLoggerProvider` регистрируется как `ILoggerProvider`
     в `AddGuiShared` через `AddSingleton<ILoggerProvider>(sp => ...)`.
181. `MainWindowVM` регистрируется в `Firelink.Gui`, не в `AddGuiShared`.
182. **`IUiDispatcher` — абстракция UI-диспетчера в
     `Firelink.Gui.Shared.Logging`.** Один метод `Post(Action)`.
     Реализация — `AvaloniaUiDispatcher` в `Firelink.Gui`
     (`Dispatcher.UIThread.Post`). `Firelink.Gui.Shared` остаётся
     без Avalonia (решение №175).

183. **`ObservableLogSink` маршалит мутации `Entries` через
     `IUiDispatcher`.** Конструктор принимает `IUiDispatcher?`.
     Если `null` (тесты, не-GUI) — синхронный путь. Если задан —
     `Add`/`Clear` идут через `Post`. Устраняет гонку
     `ObservableCollection<T>` (worker-потоки pipeline vs
     UI-биндинг `ItemsControl`), которая роняла процесс вне
     `try/catch` VM.

184. **`FakeUiDispatcher` (синхронный) и `DeferredUiDispatcher`
     (очередь + Flush) — две тестовые реализации `IUiDispatcher`.**
     Первая нужна там, где `ObservableLogSink` резолвится через DI
     (MainWindowVMTests). Вторая доказывает, что маршалинг реальный:
     до `Flush` коллекция не меняется.

185. **`VerifyPipeline.Execute` синхронный, `IVerifyRunner.RunAsync`
     асинхронный.** `VerifyRunner` оборачивает вызов в `Task.Run`,
     чтобы не блокировать UI-поток. `CancellationToken`
     пробрасывается внутрь `Execute`. `IProgress<StepProgress>` у
     Verify нет — шагов не публикуется.

186. **`VerifyVM` — три состояния в Success-ветке через производные
     свойства.** `Report.IsOk=true` → «All checks passed», строк нет.
     `Report.IsOk=false` → «N check(s) failed» + таблица `Failures`.
     `VerifyState` (Configuration/Verifying/Success/Failure) остаётся
     как в `Firelink.Gui.Shared.State`.

187. **`VerifyVM.Rows` + чекбокс `ShowAllChecks` заменяют CLI-флаг
     `--verbose`.** По умолчанию показываются только `Failures`;
     при включении — все `Checks`. `Rows` перестраивается после
     каждого прогона и при переключении чекбокса. `VerifyRowVM` —
     плоская обёртка над `VerifyCheckResult` с `StatusGlyph`
     («✓»/«×») и `StatusColor` (hex).

### Фаза 3 — дистрибутив (188–191)

188. **`Directory.Build.props` в корне репо — единый источник
     правды для версии.** `<VersionPrefix>0.1.0</VersionPrefix>`,
     `<IncludeSourceRevisionInInformationalVersion>false</...>`
     (без git-хэша в `InformationalVersion`).
     `AssemblyInformationalVersion` = `"0.1.0"`.

189. **Версия CLI читается из assembly, не хардкодится.**
     `Firelink.Cli/Program.cs` →
     `GetApplicationVersion()` через
     `AssemblyInformationalVersionAttribute` entry assembly,
     fallback `"0.0.0"`. Обрезка `+...` на всякий случай
     (страховка, если `IncludeSourceRevision` когда-то
     вернут в `true`).

190. **Иконка GUI — `src/Firelink.Gui/Assets/app.ico` +
     `<ApplicationIcon>Assets\app.ico</ApplicationIcon>`.**
     Иконка вшивается в PE-заголовок exe, в output не
     копируется. Иконки CLI нет (сознательно — Q3).

191. **`tools/build-release.bat` — релизный скрипт.**
     Читает `<VersionPrefix>` из `Directory.Build.props`,
     publish GUI+CLI в одну папку
     `build_artifacts/Firelink-<version>-win-x64/`
     (`-c Release -r win-x64 --self-contained false`),
     `Compress-Archive` → zip с файлами в корне.
     Раскладка дистрибутива — «как есть» (≈90 файлов,
     `lib/`-схема сознательно не делается).

---

## План работ

### Фаза 1 — Единый CLI (`Firelink.Cli`) ✅ ЗАКРЫТА

**Цель:** объединить `Firelink.Pack` и `Firelink.Install` в один exe.

**Статус:** закрыта 2026-09-21.

**Все шаги:**

- ✅ 1.1 — создан `Firelink.Cli`, pack-сторона перенесена.
- ✅ 1.2 — install-сторона перенесена, все 5 команд работают.
- ✅ 1.3 — `Firelink.Pack` стал class library.
- ✅ 1.4 — `Firelink.Install` стал class library. Один exe
  (`Firelink.Cli.exe`).
- ✅ 1.5 — `AddFirelinkPack` / `AddFirelinkInstall`. DI-регистрация
  в библиотеках, `Firelink.Cli/Program.cs` сокращён.
- ✅ 1.6 — ручной прогон на OmenRim 7: pack → install (TestInstance3)
  → verify, **4338 passed, 0 failed**. Идентично `TestInstance2`
  (modlist.txt, plugins.txt, loadorder.txt, ModOrganizer.exe).
- ✅ 1.7 — Ctrl+C на pack/install/verify (`Cancelled.`, exit 130),
  пробелы без кавычек (`CLI error` + hint, exit 2). Регрессий нет.

**Итог Фазы 1:**

- Один exe `Firelink.Cli.exe`, все 5 команд.
- `Firelink.Pack` и `Firelink.Install` — чистые библиотеки.
- Все 593 теста зелёные на каждом шаге.
- Ручной прогон на OmenRim 7 подтверждает отсутствие регрессий.

---

### Фаза 2 — Общие API для будущего GUI ✅ ЗАКРЫТА

**Цель:** подготовить код так, чтобы GUI мог переиспользовать
pipeline без дублирования логики.

**Статус:** закрыта 2026-09-21.

**Все шаги:**

- ✅ 2.1 — `StepProgress` + `IProgress<StepProgress>?` в
  `PackPipeline` и `InstallPipeline`.
- ✅ 2.2 — `PackInputFactory` / `InstallInputFactory`.
  `PackPipeline.Input` введён.
- ✅ 2.3 — `PackSummary` / `InstallSummary` + Builder-ы.
- ✅ 2.4 — ручной прогон на OmenRim 7 (pack → install →
  verify), **4338 passed, 0 failed**.

**Итог Фазы 2:**

- Публичный API библиотек готов к использованию из GUI:
  - `PackInputFactory.Create(...)` / `InstallInputFactory.Create(...)`.
  - `pipeline.ExecuteAsync(input, ct, progress)`.
  - `PackSummaryBuilder.Build(result)` / `InstallSummaryBuilder.Build(output)`.
- CLI — первый потребитель этих API.
- Все 621 тест зелёные на каждом шаге.

---

### Фаза 6 — Nexus Premium (12.8) ✅ ЗАКРЫТА

**Цель:** убрать warning `No downloader for source type 'nexus'` и
дать возможность скачивать архивы с Nexus через Premium-API.

**Статус:** закрыта 2026-09-22.

**Все шаги:**

- ✅ 12.8.1 — `NexusApiKeyProvider`.
- ✅ 12.8.2 — `NexusClient`.
- ✅ 12.8.3 — `NexusDownloader : IArchiveDownloader`.
- ✅ 12.8.4 — DI в `AddFirelinkInstall`.
- ✅ 12.8.4a — рефакторинг (`IHttpClientFactory`, `TempFileStream`,
  кеш `IsPremiumAsync`).
- ✅ 12.8.5 — обновление `DOC.md` (v4.3) и `FIRELINK.md`.

**Итог Фазы 6:**

- `NexusDownloader` скачивает архивы через Premium-API.
- Оба `IArchiveDownloader` (`mirror`, `nexus`) в `DownloaderRegistry`.
- Гигабайтные архивы работают: `TempFileStream` + 10-минутный
  таймаут на CDN.
- Ручной прогон на `TestInstance5`: 4 nexus-архива (включая USSEP
  ~250 МБ) скачаны без ошибок; verify идентичен `TestInstance2`.
- 674 теста зелёные.

---

### Фаза 3 — GUI (Avalonia) — ЧАСТИЧНО ЗАКРЫТА

**Цель:** графический интерфейс поверх pipeline.

**Статус:** шаги 3.1–3.4.2 закрыты (731 тест). Следующий — 3.5.

**Стек:** Avalonia 11.2.1, CommunityToolkit.Mvvm 8.4.0,
Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging.

**Структура проектов:**

src/
  Firelink.Gui.Shared/       ← MVVM-инфра, без Avalonia
  Firelink.Gui.Controls/     ← общие Avalonia-контролы
  Firelink.Gui.Install/      ← модуль installer
  Firelink.Gui.Pack/         ← модуль packer
  Firelink.Gui/              ← exe → Firelink.exe

**Все шаги:**

- ✅ **3.1** — проекты GUI + DI + базовые VM.
  - 14 тестов. Итог: 688 passed.
- ✅ **3.2** — главное окно с навигацией.
  - 12 тестов. Итог: 700 passed.
- ✅ **3.3** — живые логи в UI.
  - 10 тестов. Итог: 710 passed.
- ✅ **3.4.1** — инфраструктура навигации и file picker.
  - `IScreenFactory`, `INavigationAware`, `IFilePickerService`,
    `ScreenFactory`, `AvaloniaFilePickerService`.
  - 6 тестов. Итог: 716 passed.
- ✅ **3.4.2** — экран Install:
  - Новый проект `Firelink.Gui.Controls`.
  - `IInstallRunner` + `InstallRunner`.
  - `InstallVM` (наследует `ProgressViewModel`, реализует
    `INavigationAware`).
  - `InstallView.axaml` — четыре состояния.
  - 13 тестов. Итог: 731 passed.
- ✅ **3.5** — экран Pack:
  - `IPackRunner` + `PackRunner` в `Firelink.Gui.Pack.Services`.
  - `PackVM` в `Firelink.Gui.Pack.ViewModels`.
  - `PackView.axaml`.
  - `PackPlaceholderVM`/`PackPlaceholderView` удалены.
  - 13 тестов. Итог: 739 passed.
- ✅ **3.5.1** — потокобезопасный лог-канал:
  - `IUiDispatcher` в `Firelink.Gui.Shared.Logging`.
  - `AvaloniaUiDispatcher` в `Firelink.Gui.Services`.
  - `ObservableLogSink` маршалит мутации через `IUiDispatcher`.
  - `FakeUiDispatcher` (синхронный), `DeferredUiDispatcher`
    (очередь + Flush).
  - 5 тестов. Итог: 744 passed.
- ✅ **3.6** — экран Verify:
  - Новый проект `Firelink.Gui.Verify`.
  - `IVerifyRunner` + `VerifyRunner` (обёртка над `VerifyPipeline`
    через `Task.Run`).
  - `VerifyVM` в `Firelink.Gui.Verify.ViewModels` — три состояния
    Success через `IsOk`.
  - `VerifyRowVM` — строка таблицы.
  - `VerifyView.axaml` — таблица проверок + чекбокс
    «Show all checks».
  - `VerifyPlaceholderVM`/`VerifyPlaceholderView` удалены.
  - 18 тестов. Итог: 759 passed.

- ✅ **3.7** — дистрибутив:
  - `Directory.Build.props` (VersionPrefix 0.1.0,
    IncludeSourceRevisionInInformationalVersion=false).
  - Иконка GUI (`Assets\app.ico` + `<ApplicationIcon>`).
  - Версия CLI из assembly (`GetApplicationVersion()`).
  - `tools/build-release.bat` — publish GUI+CLI в одну папку,
    zip с файлами в корне.
  - `<AssemblyName>Firelink</AssemblyName>` — уже был
    (решение №169), проверено.
  - Раскладка дистрибутива оставлена «как есть» (~90 файлов,
    `lib/` не делаем — см. решение №191).
  - Тестов не добавляли — 759 passed.

- ⬜ **3.8** — ручной прогон:
  - Сравнить GUI-результат с CLI на TestInstance5.
  - Проверить pack, install, verify через GUI.

---

### Фаза 5 — Nexus Free (WebView2)

**Цель:** скачивание модов с Nexus для Free-аккаунтов через
WebView2.

**Статус:** не начата. Технически — после Фазы 3.

**Контекст:**

- Nexus не отдаёт прямые ссылки для Free-аккаунтов через API.
- Premium — через `download_link` API (Фаза 6).
- Free — только через автоматизацию UI (как Wabbajack, Nolvus).

**Задачи:**

- Новый проект `Firelink.NexusHelper` — отдельное Avalonia-приложение
  (не библиотека!).
- WebView2.
- Открывает `nexusmods.com`, пользователь логинится.
- Кликает «Slow Download» для каждого мода из списка.
- Скачанные файлы кладёт в `downloads/`.
- CLI-команда `firelink nexus-helper` — запускает
  `Firelink.NexusHelper.exe` как отдельный процесс.
- В GUI — отдельный экран «Nexus Free Download».

**Время:** ~2–3 недели. **Риск:** средний (юридические нюансы,
хрупкость UI).

---

### Порядок фаз

**Строгая последовательность:**

1. **Фаза 1** — единый CLI. ✅ закрыта.
2. **Фаза 2** — общие API. ✅ закрыта.
3. **Фаза 6** (12.8) — Nexus Premium. ✅ закрыта.
4. **Фаза 3** — GUI. **← следующая**.
5. **Фаза 5** — Nexus Free. Расширение для Free.

**Почему 12.8 (Фаза 6) перед GUI (Фаза 3):**

- 12.8 быстрый и независимый. Даёт Premium-функционал.
- После 12.8 можно сразу работать с Nexus-архивами — GUI уже будет
  надстраивать UI над этой функциональностью.

**Почему Free (Фаза 5) — последняя:**

- Требует WebView2 — это уже GUI-стек.
- Premium-путь проще и даёт работающий продукт для многих.
- Free — расширение, не базис.

**Фаза 4 — вариации дистрибутивов — вычеркнута.**

---

## Ключевые принципы рефакторинга

- Никаких больших изменений за один шаг. Каждый шаг компилируется.
- 593 теста — зелёные на каждом шаге. Если упали — откат.
- Ручной прогон на OmenRim 7 после каждой фазы.
- Никаких изменений в pipeline, шагах, моделях. Только композиция.
- `TryAddSingleton` в DI-extensions — защита от дублирования.
- Никаких `Process.Start` для внутренних вызовов. Только прямые
  вызовы pipeline.
- GUI — отдельные проекты, ссылаются на pipeline. Обратных ссылок
  нет.
- CLI — первоклассный клиент. GUI — дополнение, не замена.
- `Firelink.Gui.Shared` — **без Avalonia**. Всё Avalonia-зависимое —
  в `Firelink.Gui.Controls`, `Firelink.Gui.Install`,
  `Firelink.Gui.Pack`, `Firelink.Gui`.
- `Firelink.Gui.Install`/`Firelink.Gui.Pack` не ссылаются на
  `Firelink.Gui` (обратной ссылки нет).
- VM в GUI-модулях не зависят от pipeline напрямую — только через
  `IRunner` интерфейсы.
- GUI — отдельные проекты, ссылаются на pipeline. Обратных ссылок нет.

## Что НЕ делать

- Не делать «единый exe через `Process.Start` дочерних процессов».
- Не выносить presentation в pipeline. Pipeline — оркестрация,
  presentation — в клиентах.
- Не делать GUI до Фазы 2 (общие API).
- Не делать Free-стратегию (Фаза 5) до Premium (Фаза 6).
- Не трогать `Firelink.Core`, `Firelink.Platform.*` — они не меняются.
- Не использовать ReactiveUI/DynamicData/MessageBus.
- Не использовать `Avalonia` в `Firelink.Gui.Shared`.
- Не давать `Firelink.Gui.Install`/`Firelink.Gui.Pack`
  обратных ссылок на `Firelink.Gui`.

---

## Грабли и подводные камни

Накопленные замечания. Актуальны при правках.

### Про GUI

**Про `<AssemblyName>Firelink</AssemblyName>` (решение 169).**
Имя сборки `Firelink.Gui` — `Firelink`. Root namespace — `Firelink.Gui`.
Это ломает наивный `asm.GetName().Name + ".Views." + shortName` в
`ViewLocator`. Правильный алгоритм — перебор `asm.GetTypes()` и
поиск по суффиксу `.Views.<ShortName>`. Решение 179.

**Про `Grid.ColumnSpacing`/`RowSpacing`.**
В Avalonia 11.x у `Grid` **нет** этих свойств (WPF-наследие).
Используем `Margin` на детях или `StackPanel.Spacing`.

**Про `Color` — неоднозначность.**
`Color` есть и в `System.Drawing`, и в `Avalonia.Media`.
В файлах с обоими using — CS0104. Решение: alias
`using AvaloniaColor = Avalonia.Media.Color;`.

**Про `[RelayCommand]` и публичность.**
`[RelayCommand] private void Clear()` — генерирует `ClearCommand`,
но не даёт публичного метода `Clear()`. Если нужен программный
вызов — `[RelayCommand] public void Clear()`. Решение 178.

**Про `ObservableLoggerProvider` и `AddLogging`.**
`builder.AddProvider(sp => ...)` **не существует**. Провайдер
регистрируется через `services.AddSingleton<ILoggerProvider>(...)`,
`AddLogging` сам подтянет. Решение 180.

**Про `ObservableCollection<T>` в VM.** Любая мутация `ObservableCollection`
из не-UI-потока — потенциальная гонка с UI-биндингом. Если коллекция
наполняется из worker-потока (как `ObservableLogSink.Entries` от
`Parallel.ForEach` в pipeline) — нужен маршалинг через `IUiDispatcher`.
Решение №183. `VerifyVM.Rows` наполняется на UI-потоке — гонки нет.

**Про `Task.Run` в `VerifyRunner`.** `VerifyPipeline.Execute` —
синхронный (решение №116). Без `Task.Run` UI-поток блокируется на
время верификации инстанса (десятки секунд на OmenRim 7). Решение №185.

**Про производные свойства в CommunityToolkit.** `[ObservableProperty]
private VerifyReport? _report;` **не** уведомляет об изменениях
`IsOk`, `HasFailures`, `PassedCount`, `FailedCount`, `ResultTitle`,
`ResultColor`, `TargetPath`, если они объявлены как `=> Report?.X`.
Стандартное решение — ручной `OnPropertyChanged(nameof(X))` после
присвоения `Report`. В `VerifyVM` это делает `NotifyResultChanged()`.

**Про `Placeholders`-папку.** После 3.6 удалены все плейсхолдеры
(`PackPlaceholderVM`, `VerifyPlaceholderVM`). Папка
`Firelink.Gui.Shared/ViewModels/Placeholders/` и одноимённые View в
`Firelink.Gui/Views/` больше не существуют. Если добавляешь новый
экран — делай сразу настоящий VM, не плейсхолдер.

**Про `FakeScreenFactory` в `Firelink.Gui.Shared.Tests`.** Этот
проект **не** ссылается на `Firelink.Gui.Pack`/`Firelink.Gui.Verify`
(они Avalonia-зависимые). Фабрика умеет только Home; для
Pack/Install/Verify возвращает Home. Реальная маршрутизация
тестируется в `Firelink.Gui.Install.Tests`/`Pack.Tests`/`Verify.Tests`.

### Про копипаст

Были ошибки (`Pack.Steps` vs `Install.Steps`, пропущенные `Profile`,
`params` vs named args, shadowing в тестах, `PluginsEntry` вместо
`PluginEntry`). Если билд падает — вероятнее ошибка в коде ассистента.

### Про BOM

Файлы в репозитории часто с BOM (`\uFEFF`). При перезаписи файлов
не копировать BOM из вывода dump. `firelink-pack.json` у автора тоже
бывает с BOM — `PackConfigJson` читает его корректно, но лучше без.

### Про samples

`samples/*.json` копируются в output тестов через `PreserveNewest`.
Если правите sample — обновите и исходник, и (при необходимости)
очистите `bin/obj`.

### Про пути

Все проекты живут в `src/` и `tests/`. Новые проекты создавать
**строго** в `tests/<Name>/`, иначе `..\..\src\...` в
`ProjectReference` не разрешится.

### Про пустой DisabledMod

Мод без восстановимых файлов остаётся в манифесте с пустыми
директивами. Installer создаёт папку, файлов нет. Это норма.

### Про пустой extension/extra

Entry с пустым списком директив **не попадает** в манифест.
Unmatched уже выгружены в `__Firelink_Output`; entry без директив
бессмысленна.

### Про xUnit1031

Не использовать `.GetAwaiter().GetResult()` в тестах. Если API
async — тест `async Task`, `await`.

### Про Spectre markup

`[...]` — это разметка. Для имён файлов и мод-неймов (особенно
`[NoDelete]`) — обязательно `Markup.Escape`.

### Про verify-счётчики

При отсутствии папки мода `CheckMod` делает `yield break` — одна
fail-проверка вместо 10+ «file missing». Сознательное решение.

### Про `Slug.FromFileName`

`.7z` отрезается **до** slug-ификации. `FomodTools.7z` →
`fomodtools`, `Mod.Organizer-2.5.2.7z` → `mod-organizer-2-5-2`.

### Про `MatchStep.Input.Matcher`

`internal`, не `required`. При создании `MatchStep` из тестов (не
через DI) — **обязательно** задавать.
`MatchExtensionsStep`/`MatchExtrasStep` — аналогично.

### Про `Mod.Organizer-2.5.2.7z`

Если файла нет в `OmenRim 7\MO2\downloads\`, pack пишет `Size = 0`
для `manifest.Mo2.Archive`. **Не ошибка.** Hash берётся из
`mo2.source.hash`.

### Про runtime-файлы

`.log`/`.ini` от SKSE-плагинов не восстанавливаются из архивов и
уходят в `__Firelink_Output`. **Не пытаться «чинить»** — это
правильное поведение.

### Про `.bsa`/`.ba2`

Единые файлы, не контейнеры для Firelink. Отдельных `.bsa` в
`downloads/` не бывает на практике.

### Про `meta.ini` в verify

Сравнение **семантическое** (парсим через `MetaIniReader.Parse`,
сравниваем `ModMeta` по полям). Нормализация: `null ≡ ""`.
`mod.Meta == null` + файл есть → fail.

### Про `ArchiveMatcher.BuildAsync`

Async, отмена пробрасывается как есть
(`catch (OperationCanceledException) { throw; }` **перед**
`catch (Exception)`). В тестах хелпер называется
`MakeMatcherAsync`, `await matcher.BuildAsync(ct)`.

### Про `CancellationHelper`

В `Firelink.Core`. Используется в `PackCommand`, `InstallCommand`,
`VerifyCommand` и в `Program.cs` — `catch (Exception ex) when
(CancellationHelper.IsCancellation(ex))`.

### Про `config.PropagateExceptions()`

Включено в CLI. Без него Spectre ловит `CommandParseException` сам
и печатает свой формат без hint.

### Про `install` без `--target` (Фаза 1)

`firelink install <manifest>` без `--target` создаёт инстанс в
`<exeDir>/Instances/<meta.name>/`, а **не рядом с манифестом**.
Это архитектурное решение. Для переустановки поверх существующего
инстанса — всегда указывать `--target`.

### Про CS0104 при переезде CLI (Фаза 1)

При переносе команд в `Firelink.Cli` возникли коллизии имён
(`PackSettings`, `PackCommand` и т.д.) между `Firelink.Cli.*` и
`Firelink.Pack.*`/`Firelink.Install.*`. Решались псевдонимами
(`using X = Firelink.Cli.X;`). После шагов 1.3/1.4 псевдонимы убраны —
коллизий больше нет, потому что `Firelink.Pack.Commands` и
`Firelink.Install.Commands` больше не существуют.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — v0.2.0.
- Прогресс-бар Spectre — v0.2.0.
- File-logging — v0.3.0.
- `SyncModsStep` поддерживает только `FromArchive`.
- `ConfigureMo2Step` — не делаем.
- E1 (прогон на большом инстансе) — отменён по решению.
- Механизм патчей для inline-файлов — v0.2.0+.

## Сознательно не делаем

- `ConfigureMo2Step` — автоконфигурация MO2.
- Автопатчи / autoPack / inlinePatterns — отменены.
- `repair` в CLI — install идемпотентен.
- Обработка `.bsa`/`.ba2` как контейнеров — они единые файлы.
- `firelink index`.
- Фаза 4 — вариации дистрибутивов (2026-09-21).
- Nexus Premium API как отдельная подписка (кроме Фазы 6).
- Кеширование Nexus download-ссылок.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже).
- Visual Studio 2022.
- `C:\Firelink\TestInstance\` — MVP прогон (до 12.13.x), verify OK.
- `C:\Firelink\TestInstance2\` — после 12.13.6, extensions/extras,
  verify 0 failed.
- `C:\Firelink\OmenRim 7\` — тестовый оригинал.
- Большой инстанс — 4370 модов (не используется).

---

## История изменений документа

- **2026-09-21** — создан `FIRELINK.md`: объединены `HANDOFF.md`,
  `PROJECT-STATE.md`, `ROADMAP.md`. Фаза 4 вычеркнута. Добавлен
  раздел «Фаза 1 — Единый CLI» (решения 144–150). Обновлён план
  работ с учётом закрытых шагов 1.1–1.4.
- **2026-09-21** — Фаза 1 закрыта. Добавлены решения 150–152
  (`AddFirelinkPack`/`AddFirelinkInstall`, `AddHttpClient<MirrorDownloader>`,
  регистрация только CLI-специфики в `Program.cs`).
  Раздел «План работ → Фаза 1» помечен как закрытый.
- **2026-09-22** — legacy cleanup: удалены `HANDOFF.md` и
  `PROJECT-STATE.md` (объединены в `FIRELINK.md` 2026-09-21).
  Убраны записи из `Firelink.slnx`. Обновлена версия `DOC.md` до
  v4.2 в шапке.
- **2026-09-22** — Фаза 6 (Nexus Premium, 12.8) закрыта.
- Добавлены решения 161–167.
- Обновлён план работ, `DOC.md` v4.3, `FIRELINK.md`.
- Ручной прогон на `TestInstance5`: 4 nexus-архива скачаны через
Premium API, включая USSEP (~250 МБ). CDN-запросы идут через
именованный `HttpClient.nexus` (10 мин), API — через
`HttpClient.nexus-api` (2 мин).
- Тесты: 621 → 674.
- **2026-09-23** — Фаза 3, шаги 3.1–3.4.2 закрыты.
- Добавлены решения 168–181.
- Обновлён план работ, `DOC.md` v4.4.
- Тесты: 674 → 731.
- Новый проект `Firelink.Gui.Controls`.
- `ViewLocator` переписан на перебор сборок.
- **2026-09-24** — Фаза 3, шаги 3.5, 3.5.1, 3.6 закрыты.
- Добавлены решения 182–187.
- Обновлён план работ, `DOC.md` v4.5.
- Тесты: 731 → 759.
- Новые проекты `Firelink.Gui.Pack`, `Firelink.Gui.Verify`.
- Добавлен `IUiDispatcher` — фикс гонки `ObservableCollection`.
- Удалены все плейсхолдеры GUI.
- **2026-09-24** — Фаза 3, шаг 3.7 закрыт.
- Добавлены решения 188–191.
- Новый файл `Directory.Build.props` (версия 0.1.0).
- Новый файл `tools/build-release.bat`.
- Иконка GUI (`src/Firelink.Gui/Assets/app.ico`).
- Версия CLI читается из assembly.
- Тестов не добавляли: 759 passed.
- Раскладка дистрибутива — «как есть» (без `lib/`).
