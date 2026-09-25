# Firelink — состояние проекта и план работ

**Обновлено:** 2026-09-25
**Всего тестов:** 813, 0 failed
**Текущий блок:** — (Фаза 3 закрыта; Фаза 3.9 закрыта целиком: шаги 3.9.1–3.9.8)
**Следующий блок:** не определён (варианты: ручной прогон GUI, техдолг, Фаза 5)

**Спутние документы:**
- `DOC.md` (v4.5) — формальная документация: форматы, pipeline, CLI, обработка ошибок.
- `repo-dump.md` — свежий дамп репозитория.

---

## Что это

Firelink — инструмент для создания и установки воспроизводимых сборок
модов для Mod Organizer 2. C#/.NET 8.

Ключевая идея: манифест `modlist.json` — единственный источник правды.
Файлы восстанавливаются по хешам `xxHash64`. Firelink работает с
**результатом** установки, а не с процессом.

Один CLI (`Firelink.Cli`, exe → `Firelink.Cli.exe`) + один GUI
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

Прикладываю: FIRELINK.md, DOC.md, repo-dump.md (свежий).

Текущее состояние: 813 тестов, 0 failed. Закрыты: MVP (packer,
installer, verify), Фаза 1 (единый CLI Firelink.Cli),
Фаза 2 (общие API для GUI), Фаза 6 (Nexus Premium),
Фаза 3 (GUI, шаги 3.1–3.8), Фаза 3.9 (редизайн GUI +
Home-дашборд, шаги 3.9.1–3.9.8).

Фаза 3.9 — редизайн GUI и Home-дашборд:

3.9.1 — палитра и типографика в App.axaml.
3.9.2 — стили базовых контролов.
3.9.3 — NavigationView v2 (без шапки/глоу/гамбургера).
3.9.3.3 — Settings: ScreenType.Settings, SettingsVM, SettingsView.
3.9.5 — FilePickerView v2.
3.9.4 — Logs в отдельной вкладке; убрана автоочистка лога.
3.9.6 — убраны хардкод-цвета; Home → Done.
3.9.7.1 — DevMode (in-memory, тумблер в Settings);
  NavigationVM перестраивает Items; при DevMode=false
  только Home и Settings.
3.9.7.2 — InstalledPackScanner (IInstalledPackScanner,
  InstalledPackInfo, сканирование <exeDir>/Instances/).
3.9.7.3 — HomeVM дашборд, InstalledPackVM карточка,
  HomeView.axaml, навигация Home → Install через
  IInstallRequestHandler/IInstallTarget.
3.9.8 — Open MO2 / Install / Update на карточке;
  IProcessLauncher + ShellProcessLauncher;
  Update через IFilePickerService (выбор нового
  modlist.json); IsInstalled убран из InstalledPackInfo;
  inline WarningMessage на карточке.

Следующая задача: не определена. Варианты:
- Ручной прогон GUI на OmenRim 7 / OmenTest7 (аналог 3.8).
- Техдолг (убрать INavigationAware.SetNavigateHome и т.п.).
- Фаза 5 (Nexus Free, WebView2).

Стиль ответов:

1. Разбор задачи.
2. Полный код файлов с путями.
3. Инструкция по сборке/тестам.
4. Ожидаемый вывод dotnet test.

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
- **CommunityToolkit.Mvvm** 8.4.0.
- **Microsoft.Extensions.DependencyInjection**.
- **Microsoft.Extensions.Logging**.
- **AvaloniaUseCompiledBindingsByDefault=true** во всех GUI-проектах.

---

## Текущий статус

### Готово

- **Полный pipeline packer-а (13 шагов).**
- **Полный pipeline installer-а (11 шагов).**
- **Verify** (pipeline + tests + CLI).
- **CLI `Firelink.Cli.exe`** — 5 команд.
- **GUI `Firelink.exe`** — Avalonia, экраны Home/Install/Pack/
  Verify/Logs/Settings. Home — дашборд инстансов.
- **Nexus Premium** (Фаза 6).
- **Фаза 1** — единый CLI.
- **Фаза 2** — общие API.
- **Фаза 3** — GUI (3.1–3.8).
- **Фаза 3.9** — редизайн GUI + Home-дашборд (3.9.1–3.9.8).
- **813 тестов, 0 failed.**
- **Реальный прогон на `OmenRim 7`** через CLI: pack → install →
  verify, 4338 passed. Через GUI (3.8): 4522 passed.

### В работе

- Ничего. Ожидается выбор следующего блока.

### Не начато

- **Фаза 5** — Nexus Free (WebView2).

### Вычеркнуто

- **Фаза 4 — вариации дистрибутивов.** Решение 2026-09-21: не делаем.
- **E1 (прогон на большом инстансе 4370 модов)** — отменён.

---

## Структура репозитория

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

### Новые сервисы `Firelink.Gui.Shared` (3.9.7–3.9.8)

- `IInstalledPackScanner` / `InstalledPackScanner` — скан
  `<exeDir>/Instances/` (Models/InstalledPackInfo.cs).
- `IProcessLauncher` — абстракция запуска процесса
  (реализация `ShellProcessLauncher` — в `Firelink.Gui.Services`).
- `IInstallRequestHandler` (Navigation/) — Home → MainWindowVM
  запрос перехода в Install с `(manifestPath, targetPath)`.
- `IInstallTarget` (Navigation/) — MainWindowVM → InstallVM,
  `PrepareForInstall(manifestPath, targetPath)`.

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

## GUI-навигация

MainWindow (Grid: sidebar + content)
  ├── NavigationView (DataContext = NavigationVM)
  │     └── ListBox с NavigationItem
  │          DevMode=false: Home, Settings
  │          DevMode=true:  Home, Install, Pack, Verify, Logs, Settings
  └── ContentControl (Content = MainWindowVM.ActivePane)
        └── ViewLocator → View

MainWindowVM:
  IScreenFactory.Create(ScreenType) → object (VM)
  NavigateTo(screen):
    pane = screens.Create(screen)
    if (pane is INavigationAware) pane.SetNavigateHome(...)
    ActivePane = pane
    Navigation.SelectScreen(screen)
    if (pane is HomeVM) { подписка на InstallRequested; Refresh(); }

  OnInstallRequested(manifestPath, targetPath):
    NavigateTo(Install)
    if (ActivePane is IInstallTarget t)
        t.PrepareForInstall(manifestPath, targetPath)

ScreenFactory (в Firelink.Gui):
  Home    → HomeVM
  Install → InstallVM          (Firelink.Gui.Install)
  Pack    → PackVM             (Firelink.Gui.Pack)
  Verify  → VerifyVM           (Firelink.Gui.Verify)
  Logs    → LogsVM
  Settings→ SettingsVM

ViewLocator: param.ViewModels.XxxVM → ищет Control с FullName
*".Views.XxxView" через перебор всех загруженных сборок.

Плейсхолдеров нет — все 6 экранов реальные.

---

## Прогоны на реальных инстансах

**`C:\Firelink\TestInstance\` (19.09.2026, до 12.13.x):**
- Install: 82 мода, 68 архивов, 82 meta.ini, профиль `Default`.
- Verify: 4337 passed.

**`C:\Firelink\TestInstance2\` (20.09.2026, после 12.13.6, OmenRim 7):**
- Pack: 82 мода, 4236 matched, 69 архивов, 49/49 extensions, 2/2 extras.
- Install: 82 мода, 1 extension written, 2 extras written, 82 meta.ini.
- Verify: **4338 passed, 0 failed**.

**`TestInstance5` (22.09.2026, Nexus Premium):**
- 4 nexus-архива скачаны через Premium API, включая USSEP (~250 МБ).
- Verify идентичен TestInstance2.

**`OmenTest7` через GUI (25.09.2026, 3.8):**
- Pack: 71 мод, 7853 файла, 4389 директив, 58 архивов.
- Install: 71 created, 58 downloaded, 71 meta.ini.
- Verify: **4522 passed, 0 failed**.

**Важно:** `install` без `--target` создаёт инстанс в
`<exeDir>/Instances/<meta.name>/`, а не рядом с манифестом.
Это архитектурное решение (см. решение №74).

---

## Ключевые архитектурные решения

Накопленный список. Сгруппирован по темам. Нумерация — историческая,
чтобы не сбиться при ссылках.

### Общие принципы (1–19)

1. **Манифест — единственный источник правды.**
2. **Файлы восстанавливаются по хешам** (`xxHash64`).
3. **Firelink работает с результатом, а не с процессом.**
4. **Одна папка `downloads/` для всех архивов.**
5. **Идентификация архивов — канонический id.**
6. **Глобальный реестр архивов** — SQLite (v0.2.0).
7. **Ничего не удаляем** из `downloads/`.
8. **Директивы выполняются последовательно.** `lastWins`.
9. **`[NoDelete]` в имени папки** защищает пользовательские моды.
10. **BSA/BA2 — единые файлы.**
11. **Никаких исполняемых скриптов.**
12. **Installer идемпотентен.**
13. **Installer не работает с игрой.** `Stock Game/` — просто папка.
14. **Шаги pipeline изолированы.**
15. **Nexus — один источник, несколько стратегий доступа.**
16. **Параллелизм на уровне pipeline.**
17. **Кеш хешей обязателен.**
18. **Манифест самодостаточен.**
19. **Unmatched → `__Firelink_Output`.**

### Packer (20–51)

20. **`meta.game` = Nexus game domain.**
21. **`instance.path`** — относительный.
22. **`mo2.profile`** — обязательный.
23. **`mo2.source` — обязательно `MirrorSourceRef`.**
24. **`mo2.archive.size/hash` — из `mo2.source.hash`.**
25. **MO2-архив НЕ попадает в `manifest.Archives[]`.**
26. **`mo2.extensions`** — от `MO2/`. **`stockGame.extras`** — от `Stock Game/`.
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
54. **`MetaIniWriter`** — `[General]`, camelCase, без `[installedFiles]`,
    без BOM, CRLF.
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
65. **`ScanExtensionsStep`/`ScanExtrasStep`** — тип `EntryScanResult`.
66. **`MatchExtensionsStep`/`MatchExtrasStep`** — тип
    `MatchEntriesResult`. Принимают `ArchiveMatcher` через
    `Input.Matcher` (`internal`, не `required`).
67. **`Mo2ArchiveBuilder`** — статический класс в
    `Firelink.Pack.Matching`.
68. **Unmatched extensions** → `__Firelink_Output/MO2/<relativePath>`
    (плоско). **Unmatched extras** → `__Firelink_Output/Stock Game/<relativePath>`.
69. **`PackPipeline`** перед write unmatched чистит
    `__Firelink_Output/MO2/` (кроме `mods/`) и
    `__Firelink_Output/Stock Game/`.
70. **`PackPipeline`** создаёт `ArchiveMatcher` один раз.
71. **`BuildManifestStep.BuildExtensions`/`BuildExtras`** пропускают
    entry с пустым списком директив.
72. **`PackPipeline`** добавляет MO2-архив в `ArchiveIndex` перед
    созданием `ArchiveMatcher`.
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
87. **`SyncArchivesStep`** — только `manifest.Archives`, не трогает MO2.
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
108. **`BootstrapMo2Step` — самодостаточный.**
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
121. **Регенерация modlist.txt / plugins.txt / loadorder.txt в память.**
122. **Сравнение `meta.ini`** — семантическое.
123. **`schemaVersion`** — через `ManifestSchema.IsSupported`.
124. **Пустые директивы у disabled-мода** — OK.
125. **Return code CLI:** 0/1/2/130.
126. **`ManifestJson.Load` / `Save` — sync-версии для verify.**
127. **`VerifyCommand`** — summary + провалы; `--verbose` — все проверки.
128. **`VerifySettings`** — `<target>` + `--verbose`/`-v`.
129. **`CheckMod` делает `yield break`** при отсутствии папки мода.
130. **Verify проверяет extensions/extras.**

### Cancellation / CLI (131–136)

131. **`CancellationHelper.IsCancellation`** в `Firelink.Core`.
132. **`ArchiveMatcher.BuildAsync`** — async.
133. **`PackCommand`/`InstallCommand`/`VerifyCommand`** — `catch (...)
      when (CancellationHelper.IsCancellation(ex))` → `Cancelled.` + 130.
134. **`Program.cs` CLI** — fallback-обработчик отмены.
135. **`config.PropagateExceptions()`.**
136. **`catch (CommandParseException ex)`** — `CLI error` + hint.

### 12.13.x / 13.1 (137–143)

137. **`MatchStep` использует общий `ArchiveMatcher`.**
138. **`.bsa`/`.ba2` — единые файлы, не контейнеры.**
139. **CLI-таблицы `PackCommand`/`InstallCommand` включают extensions/extras.**
140. **`ArchiveDownloadHelper`** в `Firelink.Core.Archives`.
141. **`ExecuteExtensionsStep`** — раскладывает `manifest.Mo2.Extensions[]`
      в `<target>/MO2/`.
142. **`ExecuteExtrasStep`** — симметричен, корень `<target>/Stock Game/`.
143. **`ExecuteExtensionsStep`/`ExecuteExtrasStep` — Skipped**, если
      файлы уже на месте.

### Фаза 1 — Единый CLI (144–152)

144. **Один CLI-проект:** `Firelink.Cli`.
145. **`Firelink.Pack` и `Firelink.Install` — class libraries.**
146. **Namespace `Firelink.Cli.*`.**
147. **`SetApplicationName("firelink")`.**
148. **`Firelink.exe`** зарезервировано под GUI.
149. **`TryAddSingleton` вместо `AddSingleton`** для общих сервисов.
150. **DI-extension-методы: `AddFirelinkPack` / `AddFirelinkInstall`.**
151. **`AddFirelinkInstall` регистрирует `MirrorDownloader`** через
      `AddHttpClient<T>`.
152. **`Firelink.Cli/Program.cs` регистрирует только CLI-специфику.**

### Фаза 2 — Общие API (153–160)

153. **`StepProgress` — общий тип в `Firelink.Core.Progress`.**
154. **`IProgress<StepProgress>? progress = null` — опциональный
      последний параметр.**
155. **`PackPipeline.Input` введён.**
156. **`PackInputFactory` / `InstallInputFactory`** — статические.
157. **`PackSummary` / `InstallSummary`** — `sealed record` с
      `required` полями.
158. **`PackSummaryBuilder` / `InstallSummaryBuilder`** — статические.
159. **CLI-таблицы строятся из Summary, не из Output.**
160. **`ParallelOptions` остаётся в DI.**

### Фаза 6 — Nexus Premium (161–167)

161. **`NexusDownloader` перебирает CDN-ноды внутри `DownloadAsync`.**
162. **`NexusClient` и `NexusDownloader` используют разные именованные
      `HttpClient`-ы.**
163. **`IArchiveDownloader` регистрируется через `TryAddEnumerable`
      с явным `ImplementationType`.**
164. **Nexus API-ключ — plaintext-файл `%USERPROFILE%\.firelink\nexus.key`.**
165. **Downloader-ы принимают `IHttpClientFactory`, а не `HttpClient`.**
166. **Скачивание идёт в `TempFileStream`, а не в `MemoryStream`.**
167. **`NexusClient.IsPremiumAsync` кешируется на время жизни клиента.**

### Фаза 3 — GUI (168–187)

168. Стек GUI: Avalonia 11.2.1 + CommunityToolkit.Mvvm 8.4.0.
169. `<AssemblyName>Firelink</AssemblyName>` у `Firelink.Gui.csproj`.
170. Отказ от `IGuiModule` в пользу плоского `MainWindowVM.ActivePane`.
171. Навигация — прямой `MainWindowVM.NavigateTo(ScreenType)`.
172. Lazy-резолв панелей через `IScreenFactory`.
173. `INavigationAware` вместо `event HomeRequested`.
174. `IFilePickerService` в Shared, `AvaloniaFilePickerService` в Gui.
175. `Firelink.Gui.Shared` — без Avalonia.
176. `Firelink.Gui.Controls` — отдельный проект для общих контролов.
177. `IInstallRunner` в `Firelink.Gui.Install.Services`.
178. `LogVM.Clear()` — публичный метод.
179. `ViewLocator` — перебор по всем загруженным сборкам.
180. `ObservableLoggerProvider` регистрируется как `ILoggerProvider`.
181. `MainWindowVM` регистрируется в `Firelink.Gui`.
182. **`IUiDispatcher` — абстракция UI-диспетчера.**
183. **`ObservableLogSink` маршалит мутации через `IUiDispatcher`.**
184. **`FakeUiDispatcher` и `DeferredUiDispatcher` — две тестовые реализации.**
185. **`VerifyPipeline.Execute` синхронный, `IVerifyRunner.RunAsync` асинхронный.**
186. **`VerifyVM` — три состояния в Success-ветке через производные свойства.**
187. **`VerifyVM.Rows` + чекбокс `ShowAllChecks` заменяют `--verbose`.**

### Фаза 3 — дистрибутив (188–191)

188. **`Directory.Build.props` в корне репо — единый источник версии.**
189. **Версия CLI читается из assembly.**
190. **Иконка GUI — `src/Firelink.Gui/Assets/app.ico`.**
191. **`tools/build-release.bat` — релизный скрипт.**

### Фаза 3 — редизайн GUI (192–207)

192. **Палитра Firelink в `Application.Resources` (App.axaml).**
193. **Только тёмная тема.**
194. **Шрифт Inter.**
195. **Классы `TextBlock`: `.h1`, `.h2`, `.subtitle`, `.caption`, `.muted`.**
196. **`Button` — секондари + `.accent`.**
197. **`BoxShadow` в Avalonia — 5 токенов.**
198. **У `Grid` нет `RowSpacing`/`ColumnSpacing`.**
199. **`ScreenIconConverter` — `public sealed`.**
200. **`FilePickerView` — не TextBox.**
201. **Кнопка `Home` в Pack/Install/Verify заменена на `Done`.**
202. **Автоочистка лога убрана.**
203. **Logs — отдельный экран.**
204. **Settings — отдельный экран.**
205. **Хардкод-цвета в `*View.axaml` отсутствуют.**
206. **Sidebar без шапки.**
207. **Навигация между экранами — через sidebar.**

### Фаза 3.9 — DevMode и Home-дашборд (208–219)

208. **`SettingsVM.IsDevMode` — in-memory, default false.**
     Меняется через `CheckBox` в SettingsView. NavigationVM подписан
     на `PropertyChanged` и перестраивает `Items`. Persist — v0.2.0.

209. **`NavigationVM` принимает `SettingsVM` вторым параметром.**
     `MainWindowVM` резолвит `SettingsVM` из DI и передаёт в
     `NavigationVM`. `Items` перестраивается через `RebuildItems()`.
     При выключении DevMode, если текущий экран скрывается —
     `SelectedItem = Home` + `_navigate(Home)`.

210. **`ScreenType.Logs` и `ScreenType.Settings` видны всегда.**
     `ScreenType.Home` — всегда. `Install`/`Pack`/`Verify`/`Logs` —
     только при DevMode=true. Итог: DevMode=false → `[Home, Settings]`,
     DevMode=true → `[Home, Install, Pack, Verify, Logs, Settings]`.

211. **`InstalledPackScanner` / `IInstalledPackScanner` — в
     `Firelink.Gui.Shared.Services`.**
     Сканирует `<exeDir>/Instances/*/modlist.json` через
     `ManifestJson.Load`. Битые/отсутствующие манифесты — skip + log.
     Корень передаётся в конструктор (тестируемость), DI-регистрация
     вычисляет `Path.Combine(AppContext.BaseDirectory, "Instances")`.
     Синхронный `Scan()` без `ct`.

212. **`InstalledPackInfo` — record с 7 полями.**
     `Name`, `Version`, `Game`, `GameVersion`, `CreatedAt`,
     `InstancePath`, `ManifestPath`. Без `IsInstalled` — состояние
     «MO2 установлен» проверяется на клике `Open MO2`, не хранится
     в модели (нет stale-состояния).

213. **`HomeVM` — дашборд.**
     `ObservableCollection<InstalledPackVM> Items`.
     `Refresh()` вызывается `MainWindowVM.NavigateTo(Home)` и в
     конструкторе `MainWindowVM`. Реализует `IInstallRequestHandler`.

214. **`InstalledPackVM` — карточка с тремя командами.**
     `OpenMo2` (всегда активна; проверяет `File.Exists(<InstancePath>/
     MO2/ModOrganizer.exe)`; если нет — `WarningMessage`),
     `Install` (`InstallRequested(<InstancePath>/modlist.json,
     <InstancePath>)`),
     `UpdateAsync` (диалог `IFilePickerService.PickFileAsync`;
     `InstallRequested(<выбранный>, <InstancePath>)`).
     `WarningMessage` — `[ObservableProperty]`, стирается только при
     `Refresh()` (пересоздании VM).

215. **Кнопка `Open MO2` — всегда активна.**
     Не используем `CanExecute`/`IsVisible` для `File.Exists`:
     Avalonia дёргает `CanExecute` один раз, дизейбл «залипает».
     Вместо этого: попытка запуска, inline `WarningMessage` при ошибке.
     Сообщение: `"ModOrganizer.exe not found. Reinstall the pack to
     restore MO2."`.

216. **`IProcessLauncher` — абстракция `Process.Start`.**
     В `Firelink.Gui.Shared.Services`. Реализация
     `ShellProcessLauncher` — в `Firelink.Gui.Services`
     (`Process.Start` + `UseShellExecute = true`). В DI —
     singleton. В тестах — `FakeProcessLauncher`.

217. **`IInstallRequestHandler` — Home → MainWindowVM.**
     `event Action<string, string>? InstallRequested` —
     `(manifestPath, targetPath)`. Оба обязательны. `MainWindowVM`
     подписывается при `NavigateTo(Home)`, отписывается до подписки
     (идемпотентность: HomeVM singleton).

218. **`IInstallTarget` — MainWindowVM → InstallVM.**
     `PrepareForInstall(string manifestPath, string targetPath)`.
     `MainWindowVM` после `NavigateTo(Install)` проверяет
     `ActivePane is IInstallTarget`. Контракт: сбрасывает `State`
     в `Configuration`, обнуляет `Summary`/`ErrorMessage`,
     устанавливает `ModlistPicker.Path` и `TargetPicker.Path`.

219. **`Install` всегда шлёт `target = InstancePath`.**
     Не `null`. Гарантирует, что переустановка идёт в тот же
     инстанс, независимо от расхождений `meta.name` и имени папки.
     Installer без target (`<exeDir>/Instances/<meta.name>/`) —
     только через CLI.

---

## План работ

### Фаза 1 — Единый CLI ✅ ЗАКРЫТА

Закрыта 2026-09-21. Итог: один exe `Firelink.Cli.exe`, все 5 команд,
593 теста.

### Фаза 2 — Общие API для будущего GUI ✅ ЗАКРЫТА

Закрыта 2026-09-21. Итог: `StepProgress`, фабрики, Summary.
621 тест.

### Фаза 6 — Nexus Premium ✅ ЗАКРЫТА

Закрыта 2026-09-22. Итог: `NexusDownloader` через Premium API.
674 теста.

### Фаза 3 — GUI (Avalonia) ✅ ЗАКРЫТА

- ✅ 3.1 — проекты GUI + DI + базовые VM. 688.
- ✅ 3.2 — главное окно с навигацией. 700.
- ✅ 3.3 — живые логи в UI. 710.
- ✅ 3.4.1 — инфраструктура навигации и file picker. 716.
- ✅ 3.4.2 — экран Install. 731.
- ✅ 3.5 — экран Pack. 739.
- ✅ 3.5.1 — потокобезопасный лог-канал. 744.
- ✅ 3.6 — экран Verify. 759.
- ✅ 3.7 — дистрибутив. 759.
- ✅ 3.8 — ручной прогон GUI. 759.

### Фаза 3.9 — редизайн GUI + Home-дашборд ✅ ЗАКРЫТА

- ✅ 3.9.1 — палитра и типографика.
- ✅ 3.9.2 — стили базовых контролов.
- ✅ 3.9.3 / 3.9.3.2 / 3.9.3.3 — NavigationView v2, Settings. 764.
- ✅ 3.9.4 — Logs в отдельной вкладке.
- ✅ 3.9.5 — FilePickerView v2.
- ✅ 3.9.6 — Pack/Install/Verify: без хардкод-цветов, Home → Done.
- ✅ 3.9.7.1 — DevMode (in-memory) + тумблер в Settings + NavigationVM.
- ✅ 3.9.7.2 — InstalledPackScanner + InstalledPackInfo.
- ✅ 3.9.7.3 — HomeVM дашборд + InstalledPackVM карточка + HomeView.
- ✅ 3.9.8 — Open MO2 / Install / Update + IProcessLauncher +
  WarningMessage. **813 тестов.**

### Следующий блок — не определён

Варианты:

- **Ручной прогон GUI на OmenRim 7 / OmenTest7** (аналог 3.8) —
  проверить Home-дашборд, DevMode, Update, Open MO2 на реальных
  данных.
- **Техдолг** — убрать `INavigationAware.SetNavigateHome` (заглушка,
  никто не использует, решение №201); возможно, вычистить
  `INavigationAware` целиком.
- **Фаза 5** — Nexus Free (WebView2). Большая работа.

---

## Ключевые принципы рефакторинга

- Никаких больших изменений за один шаг.
- 813 тестов — зелёные на каждом шаге.
- Ручной прогон на OmenRim 7 после каждой фазы.
- Никаких изменений в pipeline, шагах, моделях. Только композиция.
- `TryAddSingleton` в DI-extensions.
- Никаких `Process.Start` для внутренних вызовов.
- GUI — отдельные проекты, ссылаются на pipeline. Обратных ссылок нет.
- CLI — первоклассный клиент. GUI — дополнение.
- `Firelink.Gui.Shared` — **без Avalonia**.
- VM в GUI-модулях не зависят от pipeline напрямую — только через
  `IRunner` интерфейсы.

## Что НЕ делать

- Не делать «единый exe через `Process.Start` дочерних процессов».
- Не выносить presentation в pipeline.
- Не делать GUI до Фазы 2 (общие API).
- Не делать Free-стратегию (Фаза 5) до Premium (Фаза 6).
- Не трогать `Firelink.Core`, `Firelink.Platform.*`.
- Не использовать ReactiveUI/DynamicData/MessageBus.
- Не использовать `Avalonia` в `Firelink.Gui.Shared`.
- Не давать `Firelink.Gui.Install`/`Firelink.Gui.Pack` обратных
  ссылок на `Firelink.Gui`.

---

## Грабли и подводные камни

### Про GUI

**Про `<AssemblyName>Firelink</AssemblyName>` (решение 169).**
Имя сборки `Firelink.Gui` — `Firelink`. Root namespace — `Firelink.Gui`.
Ломает наивный `asm.GetName().Name + ".Views." + shortName` в
`ViewLocator`. Правильный алгоритм — перебор `asm.GetTypes()`.
Решение 179.

**Про `Grid.ColumnSpacing`/`RowSpacing`.**
В Avalonia 11.x у `Grid` нет этих свойств.

**Про `Color` — неоднозначность.**
`Color` есть и в `System.Drawing`, и в `Avalonia.Media`.
Alias `using AvaloniaColor = Avalonia.Media.Color;`.

**Про `[RelayCommand]` и публичность.**
`private void Clear()` генерирует `ClearCommand`, но не публичный
`Clear()`. Решение 178.

**Про `ObservableLoggerProvider` и `AddLogging`.**
`builder.AddProvider(sp => ...)` не существует. Через
`services.AddSingleton<ILoggerProvider>(...)`. Решение 180.

**Про `ObservableCollection<T>` в VM.**
Мутации из не-UI-потока → маршалинг через `IUiDispatcher`.
Решение 183.

**Про `Task.Run` в `VerifyRunner`.**
`VerifyPipeline.Execute` — синхронный. Решение 185.

**Про производные свойства в CommunityToolkit.**
`[ObservableProperty]` не уведомляет об изменениях computed properties.
Ручной `OnPropertyChanged(nameof(X))`.

**Про `internal` между проектами.**
`internal` виден **только внутри одной сборки**. Если файл
`src/Firelink.Gui.Shared/Services/Foo.cs` объявлен `internal` и
namespace `Firelink.Gui.Services`, то `Firelink.Gui` (exe) его
**не увидит**, даже если namespace совпадает. Симптом: `CS0122`
«недоступен из-за уровня защиты» на строке регистрации в DI.
Решение: положить файл физически в ту сборку, где он используется,
или сделать тип `public`. Пример 3.9.8: `ShellProcessLauncher`
должен лежать в `src/Firelink.Gui/Services/`, не в Shared.

**Про `Update` vs `Install` на карточке.**
`Install` — `<InstancePath>/modlist.json`, `target = <InstancePath>`.
`Update` — выбранный через диалог файл, `target = <InstancePath>`.
Оба ведут в `InstallVM.PrepareForInstall(manifestPath, targetPath)`.
Installer сам копирует manifest в target (`ResolveTargetStep`).
Отдельного копирования в HomeVM нет.

**Про пустой Home.**
Если в `<exeDir>/Instances/` нет валидных манифестов — список пуст,
Home пустой. Заголовок «Home» остаётся. Решение 3.9.7.3.

**Про `FakeScreenFactory` в `Firelink.Gui.Shared.Tests`.**
Не ссылается на `Firelink.Gui.Pack`/`Firelink.Gui.Verify` (Avalonia).
Фабрика умеет только Home. Настоящая маршрутизация тестируется в
своих проектах.

### Про копипаст

Были ошибки в коде ассистента. Если билд падает — вероятнее ошибка
в коде ассистента, не в проекте.

### Про BOM

Файлы в репозитории часто с BOM (`\uFEFF`). При перезаписи —
не копировать BOM из вывода dump.

### Про samples

`samples/*.json` копируются в output тестов через `PreserveNewest`.

### Про пути

Все проекты в `src/` и `tests/`. Новые проекты — строго в `tests/<Name>/`.

### Про пустой DisabledMod

Мод без восстановимых файлов остаётся в манифесте с пустыми
директивами. Installer создаёт папку, файлов нет. Норма.

### Про пустой extension/extra

Entry с пустым списком директив **не попадает** в манифест.

### Про xUnit1031

Не использовать `.GetAwaiter().GetResult()`. Async API → async-тест.

### Про Spectre markup

`[...]` — разметка. `Markup.Escape` для имён файлов.

### Про verify-счётчики

`CheckMod` делает `yield break` — одна fail-проверка вместо 10+.

### Про `Slug.FromFileName`

`.7z` отрезается до slug-ификации.

### Про `MatchStep.Input.Matcher`

`internal`, не `required`. В тестах — задавать обязательно.

### Про `Mod.Organizer-2.5.2.7z`

Отсутствие файла в downloads/ → `Size = 0`. Hash из `mo2.source.hash`.

### Про runtime-файлы

`.log`/`.ini` от SKSE-плагинов → `__Firelink_Output`. Не «чинить».

### Про `.bsa`/`.ba2`

Единые файлы, не контейнеры.

### Про `meta.ini` в verify

Семантическое сравнение. `null ≡ ""`.

### Про `ArchiveMatcher.BuildAsync`

Async, отмена пробрасывается как есть.

### Про `CancellationHelper`

`catch (Exception ex) when (CancellationHelper.IsCancellation(ex))`.

### Про `config.PropagateExceptions()`

Включено в CLI.

### Про `install` без `--target`

`<exeDir>/Instances/<meta.name>/`, а не рядом с манифестом.

### Про CS0104 при переезде CLI

Псевдонимы `using X = Firelink.Cli.X;` — историческое, после 1.3/1.4
не нужно.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Persist DevMode в `%USERPROFILE%\.firelink\settings.json` (v0.2.0).
- **Убрать `INavigationAware.SetNavigateHome`** — заглушка, никто
  не использует после 3.9.6 (решение №201). Возможно, вычистить
  `INavigationAware` целиком.
- Глобальный реестр `archives.db` — v0.2.0.
- Прогресс-бар Spectre — v0.2.0.
- File-logging — v0.3.0.
- `SyncModsStep` поддерживает только `FromArchive`.
- `ConfigureMo2Step` — не делаем.
- E1 (прогон на большом инстансе) — отменён.
- Механизм патчей для inline-файлов — v0.2.0+.

## Сознательно не делаем

- `ConfigureMo2Step`.
- Автопатчи / autoPack / inlinePatterns.
- `repair` в CLI.
- Обработка `.bsa`/`.ba2` как контейнеров.
- `firelink index`.
- Фаза 4 — вариации дистрибутивов.
- Nexus Premium API как отдельная подписка.
- Кеширование Nexus download-ссылок.
- `IDialogService` для `Open MO2` — inline-предупреждение проще
  (решение №215).

---

## Окружение

- Windows 10/11.
- .NET 8 SDK.
- Visual Studio 2022.
- `C:\Firelink\TestInstance\` — MVP прогон (до 12.13.x).
- `C:\Firelink\TestInstance2\` — после 12.13.6.
- `C:\Firelink\OmenRim 7\` — тестовый оригинал.
- `OmenTest7` — GUI-прогон 3.8.
- Большой инстанс — 4370 модов (не используется).

---

## История изменений документа

- **2026-09-21** — создан `FIRELINK.md`.
- **2026-09-21** — Фаза 1 закрыта. Решения 150–152.
- **2026-09-22** — legacy cleanup, DOC.md v4.2.
- **2026-09-22** — Фаза 6 закрыта. Решения 161–167. DOC.md v4.3.
- **2026-09-23** — Фаза 3, шаги 3.1–3.4.2. Решения 168–181. DOC.md v4.4.
  674 → 731.
- **2026-09-24** — Фаза 3, шаги 3.5–3.6. Решения 182–187. DOC.md v4.5.
  731 → 759.
- **2026-09-24** — Фаза 3, шаг 3.7. Решения 188–191.
- **2026-09-25** — Фаза 3, шаг 3.8 (ручной прогон GUI, 4522 passed).
- **2026-09-25** — Фаза 3.9, шаги 3.9.1–3.9.6. Решения 192–207.
  759 → 766.
- **2026-09-25** — Фаза 3.9, шаги 3.9.7.1–3.9.7.3 (DevMode,
  InstalledPackScanner, Home-дашборд). 766 → 808.
- **2026-09-25** — Фаза 3.9, шаг 3.9.8 (Open MO2 / Install / Update
  на карточке, IProcessLauncher, WarningMessage). Решения 208–219.
  808 → **813**. Фаза 3.9 закрыта целиком.

---
