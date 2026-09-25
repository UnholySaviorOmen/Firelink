# Firelink — Документация проекта

**Версия документа:** 4.8
**Обновлено:** 2026-09-25

## Оглавление

1. [Обзор](#обзор)
2. [Основные принципы](#основные-принципы)
3. [Глоссарий](#глоссарий)
4. [Архитектура](#архитектура)
5. [Структура папок](#структура-папок)
6. [Форматы данных](#форматы-данных)
    - 6.1. [firelink-pack.json](#firelink-packjson)
    - 6.2. [modlist.json](#modlistjson)
    - 6.3. [Директивы](#директивы)
    - 6.4. [Источники архивов](#источники-архивов)
    - 6.5. [Формат .meta](#формат-meta)
    - 6.6. [Формат meta.ini мода](#формат-meta-ini-мода)
    - 6.7. [__Firelink_Output](#__firelink_output)
7. [Идентификация архивов](#идентификация-архивов)
8. [Nexus game domain](#nexus-game-domain)
9. [Формат modlist.txt / plugins.txt / loadorder.txt](#формат-файлов-mo2)
10. [Пайплайн: создание сборки](#пайплайн-создание-сборки)
11. [Пайплайн: установка сборки](#пайплайн-установка-сборки)
12. [Пайплайн: обновление сборки](#пайплайн-обновление-сборки)
13. [Работа с Nexus Mods](#работа-с-nexus-mods)
14. [Глобальный реестр архивов](#глобальный-реестр-архивов)
15. [CLI команды](#cli-команды)
16. [Обработка ошибок](#обработка-ошибок)
17. [Технологический стек](#технологический-стек)
18. [Дорожная карта](#дорожная-карта)
19. [Статус реализации](#статус-реализации)

---

## Обзор

**Firelink** — инструмент для создания и установки воспроизводимых
сборок модов для Mod Organizer 2. Состоит из:

- **`Firelink.Cli`** (exe → `Firelink.Cli.exe`) — CLI с командами
  `pack`, `install`, `verify`, `hash`, `doctor`.
- **`Firelink.Gui`** (exe → `Firelink.exe`) — Avalonia GUI.

Логика — в библиотеках `Firelink.Pack` и `Firelink.Install`.

**Ключевая идея:** манифест — единственный источник правды. Все файлы
восстанавливаются по хешам (`xxHash64`). Firelink работает с **результатом**
установки, а не с процессом. Как именно автор ставил моды — нас не интересует.

**Философия packer-а:** снапшот инстанса. Автор готовит инстанс MO2
любым способом. Packer индексирует, что получилось. Всё, что не
восстановимо из архивов, честно складывается в `__Firelink_Output` —
автор сам решает, делать ли из этого патч.

**Философия installer-а:** тупой исполнитель директив. Не проверяет
игру, версии, совместимость. Просто воссоздаёт структуру, которую
сделал автор.

**Целевая платформа:** Windows 10 1809+ / Windows 11.

**Целевая версия MO2:** 2.5.2.

---

## Основные принципы

1. **Манифест — единственный источник правды.** Профиль MO2 генерируется
   из манифеста, а не копируется.
2. **Файлы восстанавливаются по хешам.** `xxHash64`, не по именам и путям.
3. **Firelink работает с результатом, а не с процессом.** Никаких
   FOMOD-парсеров, XML, `meta.ini` (читается только для метаданных).
4. **Одна папка `downloads/` для всех архивов.** Моды, MO2, extras —
   всё в одном месте.
5. **Идентификация архивов — канонический id.** Не по имени файла:
   `nexus_{game}_{modId}_{fileId}` для Nexus-модов, `local_{slug}` —
   для архивов без `.meta`.
6. **Глобальный реестр архивов.** SQLite в `%USERPROFILE%\.firelink\archives.db`
   (v0.2.0). Переиспользование между сборками.
7. **Ничего не удаляем.** Ни архивы, ни моды, кроме случаев, описанных
   в reconcile.
8. **Директивы выполняются последовательно.** `lastWins` при конфликтах.
9. **`[NoDelete]` в имени папки** (`MO2/mods/[NoDelete]SkyUI`) защищает
   пользовательские моды.
10. **BSA/BA2 — единые файлы.** Не разбираем содержимое (принципиальное
    отличие от Wabbajack; см. §Идентификация архивов).
11. **Никаких исполняемых скриптов.** Только декларативные директивы.
12. **Installer идемпотентен.** Можно запускать повторно.
13. **Installer не работает с игрой.** Не ищет, не копирует, не проверяет.
    `Stock Game/` — просто папка для extras.
14. **Шаги pipeline изолированы.** Не вызывают друг друга. Pipeline —
    единственный оркестратор.
15. **Nexus — один источник, несколько стратегий доступа.** Не дублируем
    в манифесте.
16. **Параллелизм на уровне pipeline.** `Parallel.ForEach` в шагах.
17. **Кеш хешей обязателен.** `FileHashCache` (in-memory; persist — v0.2.0).
18. **Манифест самодостаточен.** Installer не ходит на Nexus за метаданными.
19. **Unmatched → `__Firelink_Output`.** Не `InlineFile`, не base64.
    Автор сам решает.
20. **`.mohidden` — часть пути.** Файл `meshes.mohidden/foo.nif` — это
    файл с относительным путём `meshes.mohidden/foo.nif`, не «mod с
    суффиксом».
21. **Инстансы в `<exeDir>/Instances/`.** Имя = `meta.name` (нормализованное).
22. **Сепараторы (`#...`) — часть сборки.** Packer сохраняет, installer
    пропускает, `RegenerateProfileStep` пишет.
23. **Один extract архивов на pack pipeline.** `ArchiveMatcher.Build`
    вызывается один раз (12.13.6).
24. **Отмена — не ошибка.** `Ctrl+C` нормализуется в `Cancelled.` + exit 130.
    `AggregateException`, все inner которого — отмены, трактуется как
    отмена (`CancellationHelper.IsCancellation`).
25. **Пути с пробелами — в кавычках.** CLI даёт hint, если `argv` разбит
    пробелами и парсинг не удался.
26. **DevMode по умолчанию выключен.** В обычном режиме sidebar
    показывает только Home и Settings. Install / Pack / Verify / Logs
    скрыты — включаются тумблером в Settings.

---

## Глоссарий

| Термин | Определение |
|---|---|
| **Манифест** | `modlist.json` — единственный источник правды. |
| **Инстанс** | Рабочая папка MO2 с подпапками `MO2/`, `Stock Game/`, `__Firelink_Output/` (у автора) или `modlist.json` (у пользователя). |
| **Канонический id** | Идентификатор архива: `nexus_{game}_{modId}_{fileId}` или `local_{slug}`. |
| **Директива** | Декларативное действие: `FromArchive`, `CreateDirectory`, `Delete`. |
| **`FromArchive`** | Директива: взять файл из архива по hash, положить по destination. |
| **Сепаратор** | Строка `#...` в `modlist.txt` — визуальный разделитель. Часть сборки. |
| **`[NoDelete]`** | Маркер в имени мода: installer не трогает такие папки. |
| **Unmatched** | Файл мода/extension/extra, не найденный в архивах. Кладётся в `__Firelink_Output`. |
| **Extensions** | Файлы в корне `MO2/`, кроме модов: `plugins/*.dll`, `tools/*`. Задаются в `mo2.extensions[]`. |
| **Extras** | Файлы в корне `Stock Game/`: `skse64_loader.exe`, `enbseries/`. Задаются в `stockGame.extras[]`. |
| **`.mohidden`** | Часть пути, а не отдельный «скрытый» файл. `meshes.mohidden/foo.nif` — обычный файл. |
| **`__Firelink_Output`** | Каталог автора. Складывается всё, что не восстановимо из архивов. |
| **BSA/BA2** | Единые файлы. Firelink не разбирает содержимое. |
| **Mirror** | Источник архива: прямая URL-ссылка + hash. |
| **Nexus** | Источник архива: `modId` + `fileId` + `game`. API — блок 9. |
| **`ArchiveMatcher`** | Распаковывает все архивы один раз, строит hash-индексы, матчит файлы. |
| **`manifest.archives[]`** | Все mod-архивы. MO2-архив — отдельно в `manifest.mo2.archive`. |
| **`ArchiveDownloadHelper`** | Общий helper скачивания с retry, `.part`, hash-check. |
| **DevMode** | Тумблер в Settings. Включает видимость Install/Pack/Verify/Logs в sidebar. In-memory, default false. |
| **InstalledPackInfo** | Модель инстанса, найденного в `<exeDir>/Instances/`. Name / Version / Game / GameVersion / CreatedAt / InstancePath / ManifestPath. |
| **Home-дашборд** | Экран Home: карточки инстансов с кнопками Open MO2 / Install / Update. |

---

## Архитектура

### Проекты solution

Firelink.slnx
src/
  Firelink.Core               — ядро: модели, JSON, хеширование, абстракции.
  Firelink.Platform.MO2       — чтение/запись modlist, plugins, loadorder, meta.ini.
  Firelink.Platform.Nexus     — NexusClient, NexusDownloader,
                                 NexusApiKeyProvider.
  Firelink.Pack               — class library: pipeline packer.
  Firelink.Install            — class library: pipeline installer + verify.
  Firelink.Cli                — CLI (exe → Firelink.Cli.exe): единая точка входа.
  Firelink.Gui.Shared         — MVVM-инфра для GUI. БЕЗ Avalonia.
                                 Включает InstalledPackScanner, IInstalledPackScanner,
                                 IProcessLauncher, IInstallRequestHandler,
                                 IInstallTarget.
  Firelink.Gui.Controls       — общие Avalonia-контролы (LogView,
                                 FilePickerView, конвертеры).
  Firelink.Gui.Install        — модуль installer (InstallVM, InstallView,
                                 IInstallRunner/InstallRunner).
  Firelink.Gui.Pack           — модуль packer (PackVM, PackView,
                                 IPackRunner/PackRunner).
  Firelink.Gui.Verify         — модуль verify (VerifyVM, VerifyView,
                                 IVerifyRunner/VerifyRunner, VerifyRowVM).
  Firelink.Gui                — GUI (exe → Firelink.exe): App.axaml,
                                 MainWindow.axaml, ViewLocator, ScreenFactory,
                                 AvaloniaFilePickerService, AvaloniaUiDispatcher,
                                 ShellProcessLauncher.
tests/
  Firelink.Core.Tests
  Firelink.Platform.MO2.Tests
  Firelink.Platform.Nexus.Tests
  Firelink.Pack.Tests
  Firelink.Install.Tests
  Firelink.Integration.Tests
  Firelink.Gui.Shared.Tests
  Firelink.Gui.Install.Tests
  Firelink.Gui.Pack.Tests
  Firelink.Gui.Verify.Tests


### Принципы архитектуры

**Pipeline — единственный оркестратор.** Только pipeline знает порядок
шагов. Шаги не знают друг о друге.

**Шаги изолированы.** Каждый шаг — это `IStep<TInput, TOutput>`.
Получает вход, возвращает выход. Не вызывает другие шаги.

**Зависимости через DI.** Никаких `ServiceLocator`, никаких `static` классов.

**Ошибки на уровне pipeline.** Шаг либо успешен, либо бросает исключение.
Pipeline решает, что делать.

### Интерфейсы

```csharp
public interface IStep<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct);
}

public interface IArchiveDownloader
{
    string SourceType { get; }
    Task<Stream> DownloadAsync(ArchiveSourceRef source, CancellationToken ct);
}

public interface IArchiveExtractor
{
    bool CanExtract(string archivePath);
    Task<IReadOnlyList<string>> ExtractAsync(
        string archivePath, string destinationDirectory, CancellationToken ct);
}
```

## Границы ответственности

Firelink.Core: модели, JSON, хеширование, валидаторы, Slug,
ArchiveId, абстракции, SevenZipExtractor, TempWorkspace,
FileHashCache, ArchiveDownloadHelper, CancellationHelper,
StepProgress.

Firelink.Platform.MO2: чтение/запись MO2-файлов, MetaReader,
MetaIniReader, MetaIniWriter, ModlistWriter, PluginsWriter,
LoadorderWriter.

Firelink.Platform.Nexus: HTTP-клиент к Nexus API (NexusClient),
NexusDownloader (IArchiveDownloader для SourceType "nexus"),
INexusApiKeyProvider + NexusApiKeyProvider (чтение
%USERPROFILE%\.firelink\nexus.key), модели ответов (NexusValidateResponse,
NexusDownloadLink).

Firelink.Pack: class library. PackPipeline + 13 шагов;
Firelink.Pack.Matching (ArchiveMatcher, ArchiveIndexes,
Mo2ArchiveBuilder) — построение индексов архивов и матчинг файлов
по хешу. Один экземпляр ArchiveMatcher на весь pipeline. Используется
шагами MatchStep, MatchExtensionsStep, MatchExtrasStep.
PackInputFactory — сборка PackPipeline.Input.
PackSummary + PackSummaryBuilder — плоская сводка результата.
DI-extension: `AddFirelinkPack`.

Firelink.Install: class library. InstallPipeline + 11 шагов;
MirrorDownloader, DownloaderRegistry; VerifyPipeline.
InstallInputFactory — сборка InstallPipeline.Input.
InstallSummary + InstallSummaryBuilder — плоская сводка результата.
DI-extension: `AddFirelinkInstall`.

Firelink.Cli: exe. Единая точка входа: `Program.cs`,
`Commands/` (PackCommand, InstallCommand, VerifyCommand,
HashCommand, DoctorCommand), `Settings/`,
`Infrastructure/TypeRegistrar`. Использует `AddFirelinkPack` и
`AddFirelinkInstall`.

Firelink.Gui.Shared: MVVM-инфра. `ViewModel`, `ProgressViewModel`,
`HomeVM`, `InstalledPackVM`, `LogVM`, `LogsVM`, `SettingsVM`,
`MainWindowVM`, `NavigationVM`. Сервисы: `IFilePickerService`,
`IUiDispatcher`, `IInstalledPackScanner` / `InstalledPackScanner`,
`IProcessLauncher`. Навигация: `IScreenFactory`, `INavigationAware`,
`IInstallRequestHandler`, `IInstallTarget`. Модели:
`InstalledPackInfo`. Состояния: `InstallState`, `PackState`,
`VerifyState`. Логи: `ObservableLogSink`, `ObservableLoggerProvider`.

Firelink.Gui.Controls: `LogView`, `FilePickerView`,
`LogLevelToBrushConverter`.

Firelink.Gui.Install / Firelink.Gui.Pack / Firelink.Gui.Verify:
Avalonia-модули. `XxxVM`, `XxxView`, `IXxxRunner`.

Firelink.Gui: exe. `App.axaml`, `MainWindow.axaml`, `ViewLocator`,
`ScreenFactory`, `AvaloniaFilePickerService`, `AvaloniaUiDispatcher`,
`ShellProcessLauncher`, `NavigationView`, `HomeView`, `SettingsView`,
`LogsView`, конвертеры.

## Общие API для клиентов (CLI + GUI)

Библиотеки `Firelink.Pack` и `Firelink.Install` предоставляют
унифицированный публичный API. CLI использует его сейчас, GUI
будет использовать в Фазе 3. Логика не дублируется.

## GUI (Фаза 3)

### Стек

- Avalonia 11.2.1.
- CommunityToolkit.Mvvm 8.4.0 (source generators входят в основной пакет).
- Microsoft.Extensions.DependencyInjection.
- Microsoft.Extensions.Logging.

### Слои

- `Firelink.Gui.Shared` — без Avalonia. VM, интерфейсы навигации
  (`IScreenFactory`, `INavigationAware`, `IInstallRequestHandler`,
  `IInstallTarget`), интерфейсы сервисов (`IFilePickerService`,
  `IUiDispatcher`, `IProcessLauncher`, `IInstalledPackScanner`), логи
  (`ObservableLogSink`, `ObservableLoggerProvider`), state-enum'ы.
- `Firelink.Gui.Controls` — Avalonia class library. Общие контролы:
  `LogView`, `FilePickerView`, `LogLevelToBrushConverter`.
- `Firelink.Gui.Install`, `Firelink.Gui.Pack`, `Firelink.Gui.Verify` —
  Avalonia class library. Модули: `XxxVM`, `XxxView`, `IXxxRunner`.
- `Firelink.Gui` — exe. `App.axaml`, `MainWindow.axaml`, `ViewLocator`,
  `ScreenFactory`, `AvaloniaFilePickerService`, `AvaloniaUiDispatcher`,
  `ShellProcessLauncher`.

### Навигация

MainWindow (Grid: sidebar + content)
├── NavigationView (DataContext = NavigationVM)
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
  Install → InstallVM   (Firelink.Gui.Install)
  Pack    → PackVM      (Firelink.Gui.Pack)
  Verify  → VerifyVM    (Firelink.Gui.Verify)
  Logs    → LogsVM
  Settings→ SettingsVM

### DevMode (Settings)

`SettingsVM.IsDevMode` — `[ObservableProperty]`, default `false`. In-memory.

Влияет на `NavigationVM.Items`:

- DevMode=false → `[Home, Settings]`
- DevMode=true  → `[Home, Install, Pack, Verify, Logs, Settings]`

`NavigationVM` подписан на `SettingsVM.PropertyChanged`. При смене
`IsDevMode` вызывает `RebuildItems()`. Если текущий `SelectedItem`
скрывается — переключает на Home и уведомляет `_navigate(Home)`.

`MainWindowVM` резолвит `SettingsVM` из DI и передаёт в
`NavigationVM(Action<ScreenType>, SettingsVM)`.

### Home-дашборд

`HomeVM` — дашборд инстансов. `ObservableCollection<InstalledPackVM> Items`.
`Refresh()` вызывается `MainWindowVM.NavigateTo(Home)` и в конструкторе
`MainWindowVM`. Пустое состояние — пустой экран (заголовок `Home`
остаётся).

`InstalledPackScanner` (`IInstalledPackScanner`) сканирует
`<exeDir>/Instances/*/modlist.json` через `ManifestJson.Load`.
Битые/отсутствующие манифесты — skip + log. Корень передаётся
в конструктор, DI регистрирует с `AppContext.BaseDirectory + "Instances"`.

`InstalledPackInfo` — 7 полей: `Name`, `Version`, `Game`,
`GameVersion`, `CreatedAt`, `InstancePath`, `ManifestPath`. Без
`IsInstalled` — проверка наличия `ModOrganizer.exe` происходит
на клике Open MO2.

`InstalledPackVM` — карточка с тремя командами:

- **Open MO2** — всегда активна. `File.Exists(<InstancePath>/MO2/ModOrganizer.exe)`.
  Если нет — `WarningMessage = "ModOrganizer.exe not found. Reinstall the pack to restore MO2."`.
  Если есть — `_launcher.OpenFile(mo2ExePath)`.
- **Install** — `InstallRequested(<InstancePath>/modlist.json, <InstancePath>)`.
  Переустановка из текущего манифеста.
- **Update** — диалог выбора нового `modlist.json` через
  `IFilePickerService.PickFileAsync`. Если юзер отменил — no-op.
  Если выбрал — `InstallRequested(<выбранный>, <InstancePath>)`.
  Installer сам копирует новый манифест в `<InstancePath>/modlist.json`
  (`ResolveTargetStep`).

`WarningMessage` — `[ObservableProperty]`, `string?`. Стирается
только при `Refresh()` (VM пересоздаётся). Отображается inline
на карточке снизу, во всю ширину, `ErrorBrush`.

### ViewLocator

`param` → VM. `ViewLocator`:
1. Ищет `Type` с `FullName`, полученным заменой
   `.ViewModels.` → `.Views.` в `FullName` VM.
2. Если не нашёл — перебирает `asm.GetTypes()` всех загруженных
   сборок и ищет `Type`, чей `FullName` заканчивается на
   `.Views.<ShortName>` и содержит `.Views.` в середине.
3. Возвращает `TextBlock "View not found: ..."` при неудаче.

### Ключевые интерфейсы

```csharp
public interface IInstallRunner
{
    Task<InstallSummary> RunAsync(
        string manifestPath, string? target,
        IProgress<StepProgress> progress, CancellationToken ct);
}

public interface IPackRunner
{
    Task<PackSummary> RunAsync(
        string configPath,
        IProgress<StepProgress> progress, CancellationToken ct);
}

public interface IVerifyRunner
{
    Task<VerifyReport> RunAsync(
        string targetPath, CancellationToken ct);
}

public interface IUiDispatcher
{
    void Post(Action action);
}

public interface IProcessLauncher
{
    void OpenFile(string path);
}

public interface IInstalledPackScanner
{
    IReadOnlyList<InstalledPackInfo> Scan();
}

public interface IInstallRequestHandler
{
    event Action<string, string>? InstallRequested;
}

public interface IInstallTarget
{
    void PrepareForInstall(string manifestPath, string targetPath);
}
```

### DI
AddGuiShared() регистрирует:

- ObservableLogSink, ILoggerProvider (через ObservableLoggerProvider).
- LogVM, HomeVM, LogsVM, SettingsVM.
- IInstalledPackScanner → InstalledPackScanner (фабрика с
  AppContext.BaseDirectory + "Instances").

AddGuiInstall() регистрирует:

- IInstallRunner → InstallRunner.
- InstallVM.

AddGuiPack() регистрирует:

- IPackRunner → PackRunner.
- PackVM.

AddGuiVerify() регистрирует:

- IVerifyRunner → VerifyRunner.
- VerifyVM.

App.BuildServices() (в Firelink.Gui) дополнительно регистрирует:

- IUiDispatcher → AvaloniaUiDispatcher.
- IFilePickerService → AvaloniaFilePickerService.
- IProcessLauncher → ShellProcessLauncher.
- IScreenFactory → ScreenFactory.
- MainWindowVM.

### Фаза 3.9 — редизайн

**Цель:** тёмная тема с тёплым песочным акцентом (#d3b181),
чистый монохромный UI, иконки Lucide.

**Палитра** (в `App.axaml` → `Application.Resources`):

| Токен | Hex | Назначение |
|---|---|---|
| `SurfaceBase` | `#1e1e1e` | Фон окна, sidebar |
| `SurfaceRaised` | `#242424` | Карточки, поля ввода |
| `SurfaceOverlay` | `#2a2a2a` | Активный пункт nav, кнопки секондари |
| `SurfaceHover` | `#2f2f2f` | Hover на интерактиве |
| `BorderSubtle` | `#333333` | Тонкие рамки, разделители |
| `BorderStrong` | `#3f3f3f` | Рамки полей, кнопок секондари |
| `TextPrimary` | `#e8e8e8` | Основной текст |
| `TextSecondary` | `#a0a0a0` | Подписи, meta |
| `TextMuted` | `#6a6a6a` | Совсем приглушённый |
| `Accent` | `#d3b181` | Тёплый песочный акцент |
| `AccentHover` | `#e0c090` | Наведение |
| `AccentPressed` | `#b89868` | Нажатие |
| `AccentMuted` | `#4a3e2a` | Приглушённый (selection) |
| `Success` | `#7fc98a` | «All checks passed», success |
| `Error` | `#d97777` | Ошибки |
| `Warning` | `#d3b181` | Синоним Accent |

**Тема:** только тёмная (`RequestedThemeVariant="Dark"`).

**Шрифт:** Inter (из `Avalonia.Fonts.Inter`).

**Классы `TextBlock`:** `.h1`, `.h2`, `.subtitle`, `.caption`,
`.muted`.

**Кнопки:** базовый `Button` — секондари; `Button.accent` —
акцентная.

**Иконки:** Lucide-стиль, SVG-пути в `ScreenIconConverter`.

**Экраны:**

- **Home** — дашборд инстансов (`InstalledPackScanner` +
  `InstalledPackVM`). Карточка: имя, версия, игра + версия игры,
  кнопки `Open MO2` / `Install` / `Update`, inline `WarningMessage`.
- **Install / Pack / Verify** — стилизованы под палитру. Кнопка
  `Home` → `Done`.
- **Logs** (`ScreenType.Logs`, `LogsVM`, `LogsView`) — отдельный экран.
- **Settings** (`ScreenType.Settings`, `SettingsVM`, `SettingsView`) —
  About + `CheckBox "Developer mode"` + копирайт
  `Firelink v0.1.0 · AGPL-3.0-or-later · Copyright (C) 2026 omen`.

**Автоочистка лога убрана.** `Log.Clear()` в начале
`InstallAsync`/`PackAsync`/`VerifyAsync` удалён.

**`VerifyVM` и `VerifyRowVM`** возвращают цвета из палитры.

### Версия приложения

Единый источник правды — `Directory.Build.props` в корне репо
(`<VersionPrefix>0.1.0</VersionPrefix>`). MSBuild генерирует
`AssemblyVersion` (0.1.0.0), `FileVersion` (0.1.0.0),
`InformationalVersion` (0.1.0). `<IncludeSourceRevisionInInformationalVersion>false</...>`
отключает добавление git-хэша к `InformationalVersion`.

`Firelink.Cli/Program.cs` читает `InformationalVersion` через
`AssemblyInformationalVersionAttribute` entry assembly и передаёт
в `SetApplicationVersion(...)`. Хардкода версии в коде нет.

### Иконка

GUI: `<ApplicationIcon>Assets\app.ico</ApplicationIcon>` в
`Firelink.Gui.csproj`, файл `src/Firelink.Gui/Assets/app.ico`.
Иконка вшивается в PE-заголовок `Firelink.exe`. CLI-иконки нет
(сознательно).

### Сборка релиза

Скрипт `tools/build-release.bat`:

1. Читает `<VersionPrefix>` из `Directory.Build.props`.
2. `dotnet publish` GUI и CLI в одну папку
   `build_artifacts/Firelink-<version>-win-x64/`
   (`-c Release -r win-x64 --self-contained false`).
3. `Compress-Archive` → `build_artifacts/Firelink-<version>-win-x64.zip`
   (файлы в корне архива, без вложенной папки).

Раскладка дистрибутива — «как есть» (~90 файлов: оба exe,
managed-сборки, нативные DLL Avalonia/Skia, `Assets/7z/`).
Схема с `lib/` для «чистого корня» сознательно не делается.
</｜｜DSML｜｜ parameter>
</｜｜DSML｜｜ invoke>
</｜｜DSML｜｜ calls>

---

**Раздел «Технологический стек» (дописать строки):**
Avalonia 11.2.1 (GUI)
CommunityToolkit.Mvvm 8.4.0 (GUI)

text

---

**Раздел «Дорожная карта» (дописать блок):**
Фаза 3 (GUI)

☑ 3.1 — проекты GUI + DI + базовые VM.
☑ 3.2 — главное окно с навигацией.
☑ 3.3 — живые логи в UI.
☑ 3.4.1 — инфраструктура (IScreenFactory, INavigationAware,
IFilePickerService).
☑ 3.4.2 — экран Install (+ Firelink.Gui.Controls).
☑ 3.5 — экран Pack (+ Firelink.Gui.Pack).
☑ 3.5.1 — потокобезопасный лог-канал (IUiDispatcher).
☑ 3.6 — экран Verify (+ Firelink.Gui.Verify).
☑ 3.7 — дистрибутив (Directory.Build.props, иконка GUI,
  версия CLI из assembly, build-release.bat).
☑ 3.8 — ручной прогон GUI на OmenRim 7 / OmenTest7
  (pack/install/verify, 4522 passed).

### Фаза 3.9 — редизайн GUI

☑ 3.9.1 — палитра и типографика.
☑ 3.9.2 — стили базовых контролов.
☑ 3.9.3 — NavigationView v2 (без шапки/глоу/гамбургера;
  иконки Lucide).
☑ 3.9.3.3 — Settings: ScreenType.Settings, SettingsVM,
  SettingsView.
☑ 3.9.4 — Logs в отдельной вкладке, перекраска LogView,
  удаление автоочистки.
☑ 3.9.5 — FilePickerView v2.
☑ 3.9.6 — Pack/Install/Verify: хардкод-цвета убраны,
  Home → Done.
☑ 3.9.7.1 — DevMode (in-memory) + тумблер в Settings +
  NavigationVM.
☑ 3.9.7.2 — InstalledPackScanner + InstalledPackInfo.
☑ 3.9.7.3 — HomeVM дашборд + InstalledPackVM карточка +
  HomeView.
☑ 3.9.8 — Open MO2 / Install / Update + IProcessLauncher +
  WarningMessage.

### Фабрики Input

```csharp
// Packer:
var input = PackInputFactory.Create(
    configPath,
    parallelOptions: opts);   // opts = null → Environment.ProcessorCount

// Installer:
var input = InstallInputFactory.Create(
    manifestPath,
    target: target,           // null → auto-resolve в <exeDir>/Instances/
    parallelOptions: opts);
```

Фабрики — единственная точка, где:

Path.GetFullPath нормализует user-facing пути;

ParallelOptions получает дефолт при null.

Прогресс
csharp
var progress = new Progress<StepProgress>(p =>
    logger.LogInformation("Step {Index}/{Total}: {Name}",
        p.StepIndex, p.TotalSteps, p.StepName));

await pipeline.ExecuteAsync(input, ct, progress);
StepProgress — record в Firelink.Core.Progress:
(int StepIndex, int TotalSteps, string StepName). StepIndex — 1-based.
Report вызывается перед шагом.

Packer: 14 имён (включая WriteUnmatchedExtensionsExtras).

Installer: 11 имён.

StepName — стабильный контракт для GUI: имена не менять без причины.

Сводки
csharp
// Packer:
var summary = PackSummaryBuilder.Build(result);
Console.WriteLine($"{summary.Name} v{summary.Version}: " +
    $"{summary.DirectivesTotal} directives, " +
    $"{summary.UnmatchedFiles} unmatched");

// Installer:
var summary = InstallSummaryBuilder.Build(output);
Console.WriteLine($"{summary.ModsCreated} created, " +
    $"{summary.ModsSkipped} skipped");
Summary — плоский sealed record только из примитивов.
Никаких ссылок на PackResult / InstallPipeline.Output /
Manifest. GUI может получить Summary и не тащить за собой
весь pipeline-контекст.

Полные списки (Created, Recreated, Skipped, Deleted) — не в
Summary. Кому нужно — берёт из Pipeline.Output напрямую.

Порядок использования
Собрать Input через фабрику.

Вызвать pipeline.ExecuteAsync(input, ct, progress), где
progress опционален.

Построить Summary из результата.

Отобразить Summary (CLI — таблица, GUI — ViewModel).

Никаких Path.GetFullPath в клиентах. Никакой логики подсчёта
директив в клиентах. Всё — в библиотеке.

## Структура папок

### Рабочая папка автора

C:\Mods\Dev\
  firelink-pack.json              ← конфиг packer-а
  NordicUI Overhaul\              ← инстанс MO2
    MO2\
      ModOrganizer.exe
      portable.txt
      plugins\                    ← extensions (fomod_plus_installer.dll)
      tools\                      ← extensions (BethINI)
      downloads\                  ← архивы (моды + MO2 + extras)
      mods\
        SkyUI\
          meta.ini
        [NoDelete]UserMod\        ← пользовательский мод
      profiles\NordicUI\          ← modlist.txt, plugins.txt, loadorder.txt
    Stock Game\                   ← extras (SKSE, ENB)
    __Firelink_Output\
      modlist.json                ← манифест (WriteManifestStep)
      MO2\
        mods\<ModName>\<path>     ← unmatched модов
        <path>                    ← unmatched extensions
      Stock Game\
        <path>                    ← unmatched extras

### Рабочая папка пользователя (после установки)

D:\Games\Firelink\
  Firelink.Cli.exe
  Firelink.Core.dll
  Firelink.Pack.dll
  Firelink.Install.dll
  Firelink.Platform.MO2.dll
  ...
  Assets\7z\ (7z.exe, 7z.dll)
  Instances\
    Nordic UI Overhaul\           ← имя из meta.name
      modlist.json                ← копия манифеста (ResolveTargetStep)
      MO2\
        ModOrganizer.exe
        portable.txt
        plugins\
        tools\
        downloads\
          Mod.Organizer-2.5.2.7z
          SkyUI.7z
        mods\
          SkyUI\
            meta.ini
          [NoDelete]UserMod\      ← пользовательский мод, installer не трогает
        profiles\Default\
          modlist.txt / plugins.txt / loadorder.txt
      Stock Game\
        skse64_loader.exe
        enbseries\

### Глобальные данные

%USERPROFILE%\.firelink\
  archives.db                     ← реестр + конфиг (v0.2.0)
  nexus.key                       ← API-ключ (блок 9, plaintext; v0.2.0 — DPAPI)

## Форматы данных

### firelink-pack.json

Конфиг автора сборки. Лежит рядом с инстансом MO2.

```json
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
    }
  ]
}
```

Поле	Описание
meta.name	Имя сборки. Используется как имя папки инстанса. Проходит NameValidator.
meta.version	Версия сборки. Semver 2.0.0. Проходит SemverValidator.
meta.author	Автор.
meta.game	Nexus game domain (skyrimspecialedition, fallout4, ...).
meta.gameVersion	Целевая версия игры (информационное).
instance.path	Относительный путь к корню инстанса от папки с конфигом. Проходит InstancePathValidator.
mo2.version	Версия MO2.
mo2.profile	Имя профиля MO2 в profiles/. Проходит NameValidator. Обязательно.
mo2.archive	Имя архива MO2 в downloads/ (только имя, не путь).
mo2.source	Источник MO2-архива. Обязательно MirrorSourceRef (с hash).
mo2.extensions	Относительные пути от MO2/ (файлы или папки).
stockGame.extras	Относительные пути от Stock Game/.
archiveSources	Источники для архивов без .meta. Уникальные имена.

**Валидация**: PackConfigValidator. Проверки:

meta.name — NameValidator (запрещённые символы, reserved names).

meta.version — SemverValidator.

meta.author, meta.game — непустые.

instance.path — InstancePathValidator (относительный, без ..).

mo2.profile — NameValidator.

mo2.archive — не путь.

mo2.source — MirrorSourceRef.

mo2.extensions[], stockGame.extras[] — RelativePathValidator.

archiveSources[].archive — не путь, уникально.

archiveSources[].sources — непустой.

### modlist.json

Манифест. Единственный источник правды для installer-а.

```json
{
  "schemaVersion": "1.0.0",
  "manifestVersion": "0.1.0",
  "createdAt": "2026-09-20T21:41:13.000Z",
  "createdBy": "firelink-pack/0.1.0",

  "meta": {
    "name": "OmenRim 7",
    "version": "0.1.0",
    "author": "YourName",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },

  "execution": {
    "directives": "sequential",
    "onConflict": "lastWins"
  },

  "mo2": {
    "version": "2.5.2",
    "profile": "Default",
    "archive": {
      "id": "local_mod-organizer-2-5-2",
      "name": "Mod.Organizer-2.5.2.7z",
      "size": 0,
      "hash": "xxh64:e574e05eb6c470ad",
      "sources": [
        {
          "type": "mirror",
          "url": "https://github.com/ModOrganizer2/modorganizer/releases/download/v2.5.2/Mod.Organizer-2.5.2.7z",
          "hash": "xxh64:e574e05eb6c470ad"
        }
      ]
    },
    "extensions": [
      {
        "name": "plugins/curationclub",
        "directives": [
          {
            "type": "FromArchive",
            "archive": "nexus_skyrimspecialedition_60552_123456",
            "source": "curationclub.dll",
            "destination": "plugins/curationclub/curationclub.dll",
            "hash": "xxh64:...",
            "size": 12345
          }
        ]
      }
    ]
  },

  "stockGame": {
    "extras": [
      {
        "name": "skse64_loader.exe",
        "directives": [
          {
            "type": "FromArchive",
            "archive": "nexus_skyrimspecialedition_30379_456789",
            "source": "skse64_loader.exe",
            "destination": "skse64_loader.exe",
            "hash": "xxh64:...",
            "size": 67890
          }
        ]
      }
    ]
  },

  "archives": [
    {
      "id": "nexus_skyrimspecialedition_3863_1000172397",
      "name": "SkyUI_5_1-3863-5-1.7z",
      "size": 12345678,
      "hash": "xxh64:...",
      "sources": [
        {
          "type": "nexus",
          "modId": 3863,
          "fileId": 1000172397,
          "game": "skyrimspecialedition"
        }
      ]
    }
  ],

  "mods": [
    {
      "name": "SkyUI",
      "enabled": true,
      "order": 5,
      "meta": {
        "gameName": "Skyrim Special Edition",
        "gameID": "skyrimspecialedition",
        "modID": 3863,
        "fileID": 1000172397,
        "version": "5.1",
        "repository": "Nexus",
        "url": "https://www.nexusmods.com/skyrimspecialedition/mods/3863",
        "comments": "",
        "notes": ""
      },
      "directives": [
        {
          "type": "FromArchive",
          "archive": "nexus_skyrimspecialedition_3863_1000172397",
          "source": "interface/iconmenu.swf",
          "destination": "interface/iconmenu.swf",
          "hash": "xxh64:...",
          "size": 4567
        }
      ]
    }
  ],

  "plugins": [
    { "name": "SkyUI.esp", "enabled": true, "order": 1 }
  ],

  "loadorder": [
    "Skyrim.esm",
    "Update.esm",
    "SkyUI.esp"
  ]
}
```

Секция	Описание
schemaVersion	Версия формата. ManifestSchema.IsSupported.
manifestVersion	Версия сборки (= meta.version).
createdAt	yyyy-MM-ddTHH:mm:ss.fffZ.
createdBy	firelink-pack/0.1.0.
meta	Метаданные сборки.
execution	directives: "sequential", onConflict: "lastWins".
mo2	Версия MO2, профиль, MO2-архив, extensions.
stockGame	extras.
archives	Все mod-архивы. MO2-архив сюда НЕ попадает.
mods	Моды с директивами и ModMeta.
plugins	Плагины с флагами.
loadorder	Порядок загрузки.
ModMeta (поле mods[].meta):

Все поля опциональны.

ModMeta.Empty — валидное состояние (meta.ini есть, [General] пустой).

Поля: GameName, GameId, ModId, FileId, Version, Repository,
Url, Comments, Notes. Без Category.

### Директивы

Полиморфизм: type — дискриминатор.

FromArchive — взять файл из архива по hash:

```json
{
  "type": "FromArchive",
  "archive": "nexus_skyrimspecialedition_3863_1000172397",
  "source": "interface/iconmenu.swf",
  "destination": "interface/iconmenu.swf",
  "hash": "xxh64:...",
  "size": 4567
}
```

archive — id из manifest.archives[] или manifest.mo2.archive.id.

source — путь внутри архива.

destination — путь относительно корня мода / MO2 / Stock Game.

hash, size — проверка после распаковки.

CreateDirectory — создать папку:

```json
{
  "type": "CreateDirectory",
  "destination": "meshes/empty/"
}
```

Delete — удалить файл (не используется packer-ом, но модель есть):

json
{
  "type": "Delete",
  "destination": "interface/unwanted.swf"
}
InlineFile — модель удалена (решение №40). Packer не создаёт
inline-файлов; всё, что не сматчилось, идёт в __Firelink_Output.

### Источники архивов

mirror — прямая URL-ссылка + hash:

```json
{
  "type": "mirror",
  "url": "https://cdn.example.com/skyui.7z",
  "hash": "xxh64:..."
}
```

url — HTTPS-ссылка.

hash — обязателен.

nexus — Nexus API:

```json
{
  "type": "nexus",
  "modId": 3863,
  "fileId": 1000172397,
  "game": "skyrimspecialedition"
}
```

modId, fileId — положительные.

game — Nexus game domain.

GitHub source — удалён (cleanup, 12.10.1). Всё через mirror.

### Формат .meta

Файл рядом с архивом в downloads/: SkyUI.7z.meta.

```
[General]
gameName=Skyrim
modID=3863
fileID=1000172397
```

Правила:

Ключи modID / fileID — case-sensitive (заглавные ID).
Так пишет Wabbajack и MO2.

gameName — опционально (используется только в логе).

Секция [General] — case-insensitive.

.meta без modID/fileID (или с lowercase modid) →
MetaReader.TryRead вернёт null → fallback на archiveSources[].

.meta не считается архивом (ArchiveExtensions.IsArchive возвращает
false для .meta).

### Формат meta.ini мода

Файл mods/<Name>/meta.ini. Создаётся MO2.

```
[General]
gameName=Skyrim Special Edition
gameID=skyrimspecialedition
modID=32349
fileID=795423
version=1.7.0
repository=Nexus
url=https://www.nexusmods.com/skyrimspecialedition/mods/32349
comments=
notes=
```

Чтение (MetaIniReader):

Секция [General]. Ключи case-insensitive (modID, modid,
ModId — эквивалентны).

[installedFiles] игнорируется.

category, newestVersion, nexusFileStatus, timestamps — игнорируются.

Если [General] пуста → ModMeta.Empty.

Запись (MetaIniWriter):

[General], camelCase (modID, fileID, gameID).

UTF-8 без BOM, CRLF.

Не пишет [installedFiles], category, newestVersion.

Null-поля не пишутся. Пустые строки (comments="") — пишутся как key=.

Порядок полей: gameName, gameID, modID, fileID, version,
repository, url, comments, notes.

### __Firelink_Output

Каталог автора сборки. Создаётся packer-ом в корне инстанса. Не
пересекается с MO2/ и не сканируется ScanModsStep.

Назначение: сохранить всё, что не удалось восстановить из архивов,
чтобы автор увидел свои ручные правки и решил, делать ли из них патч.

Структура:

```
__Firelink_Output/
  modlist.json                    ← манифест (WriteManifestStep)
  MO2/
    mods/
      <ModName>/<path>            ← unmatched модов (MatchStep)
    <path>                        ← unmatched extensions (плоско)
  Stock Game/
    <path>                        ← unmatched extras (плоско)
```

Три категории unmatched:

Моды (MatchStep) — MO2/mods/<ModName>/<path>. Сохраняется имя
мода, потому что для модов это осмысленный идентификатор. meta.ini
в корне мода сюда не попадает — он уходит в mods[].meta манифеста.

Extensions (MatchExtensionsStep) — MO2/<path>. Плоско,
полный путь от корня MO2/. Пример: mo2.extensions = ["tools/BethINI"],
внутри папки файл readme.txt →
__Firelink_Output/MO2/tools/BethINI/readme.txt.

Extras (MatchExtrasStep) — Stock Game/<path>. Плоско,
полный путь от корня Stock Game/. Пример: stockGame.extras = ["enbseries"],
внутри папки файл enbseries.ini →
__Firelink_Output/Stock Game/enbseries/enbseries.ini.

Почему плоско для extensions/extras: пути в __Firelink_Output
совпадают с реальными путями в инстансе. Автор может сравнить «что
лежит в инстансе» и «что не восстановилось», не переключаясь между
двумя структурами папок.

Поле EntryName (в UnmatchedEntry) используется для логов и
диагностики, но на путь файла в __Firelink_Output не влияет.
Путь строится только из RelativePath + корень (MO2/ или Stock Game/).

#### Чистка:

MatchStep перед своим прогоном удаляет __Firelink_Output/MO2/mods/
целиком.

PackPipeline перед write unmatched extensions удаляет содержимое
__Firelink_Output/MO2/, кроме mods/.

PackPipeline перед write unmatched extras удаляет содержимое
__Firelink_Output/Stock Game/ целиком.

modlist.json перезаписывается WriteManifestStep с предупреждением,
если файл уже был.

#### Создание папок:

__Firelink_Output/MO2/mods/ — создаётся всегда
(MatchStep.PrepareFirelinkOutput).

__Firelink_Output/MO2/ (без mods/) — создаётся только если
unmatched extensions непусты.

__Firelink_Output/Stock Game/ — создаётся только если unmatched
extras непусты.

Т.е. если всё сматчилось — будут только modlist.json и MO2/mods/
(возможно, с файлами unmatched модов).

## Идентификация архивов

Канонический id:

nexus_{game_domain}_{modId}_{fileId} — для Nexus-архивов (есть .meta
с modID/fileID).
Пример: nexus_skyrimspecialedition_3863_1000172397.

local_{slug} — для архивов без .meta.
Slug от имени файла без расширения.
Пример: SkyUI.7z → local_skyui, FomodTools.7z → local_fomodtools.

Slug (Slug.From):

ASCII-only, lowercase.

Разделитель — дефис.

Обрезка до 80 символов.

.7z отрезается до slug-ификации (см. решение №116):
Mod.Organizer-2.5.2.7z → mod-organizer-2-5-2.

Дубликаты: IndexArchivesStep бросает InvalidOperationException
при дубликате id (не имени файла).

archives[] vs mo2.archive: MO2-архив никогда не попадает в
manifest.archives[], даже если он есть в downloads/. Он живёт в
manifest.mo2.archive.

BSA/BA2. .bsa/.ba2 считаются архивами (ArchiveExtensions.IsArchive),
но на практике они всегда упакованы внутри .7z/.zip, а не
лежат в downloads/ отдельными файлами. Это означает:

В downloads/ .bsa/.ba2 обычно не встречаются как отдельные
файлы.

Если .bsa лежит внутри .7z, ArchiveMatcher распаковывает
.7z и находит .bsa как обычный файл. Матчинг файлов модов идёт
по хешу целого .bsa.

Если автор распаковал .bsa (ассеты отдельно) — ассеты уйдут
в __Firelink_Output, автор сам решит, делать ли патч.

Firelink не разбирает .bsa/.ba2 на содержимое — это
принципиальное отличие от Wabbajack.

## Nexus game domain

meta.game — Nexus game domain, не человекочитаемое имя игры.

Примеры: skyrimspecialedition, fallout4, starfield, skyrim.

Не путать с gameName (Skyrim Special Edition) — это поле в
meta.ini, не в firelink-pack.json.

Используется в:

ArchiveId.FromNexus(game, modId, fileId) — game domain попадает
в id.

NexusSourceRef.Game — при скачивании через Nexus API (блок 9).

manifest.meta.game — для installer-а (сам installer значение
не использует, но хранит для диагностики).

При смене игры — meta.game меняется, id архивов становятся
другими. Это сознательно: skyrimspecialedition_3863_1000172397 и
fallout4_3863_1000172397 — разные архивы.

## Формат файлов MO2

modlist.txt (profiles/<Name>/modlist.txt):

Строка: +Name (enabled) или -Name (disabled).

Комментарии #... игнорируются (но сепараторы -# — данные, не
комментарии: ModlistReader включает их в Entries с Enabled = false).

UTF-8 с BOM, CRLF.

Порядок строк сохраняется как есть.

Заголовок # This file was automatically generated by Mod Organizer.
пишется ModlistWriter.

plugins.txt (profiles/<Name>/plugins.txt):

Строка: *Name.esp (enabled) или Name.esp (disabled).

Комментарии #... игнорируются.

UTF-8 с BOM, CRLF.

Заголовок пишется PluginsWriter.

loadorder.txt (profiles/<Name>/loadorder.txt):

Строка: имя плагина.

Порядок = порядок загрузки.

UTF-8 с BOM, CRLF.

Заголовок пишется LoadorderWriter.

Сепараторы — #... в modlist.txt. Packer сохраняет в манифесте;
installer пропускает; RegenerateProfileStep пишет в modlist.txt.

## Пайплайн: создание сборки

### Этап 1.1. Подготовка окружения
Автор:

Ставит портативный MO2 2.5.2.

Устанавливает моды через MO2 любым способом (FOMOD, вручную, через
MO2 installer).

Кладёт архивы модов в MO2/downloads/.

Устанавливает плагины MO2 в MO2/plugins/, MO2/tools/.

Устанавливает extras в Stock Game/.

Настраивает профиль: порядок модов, плагинов, load order.

Результат: рабочий инстанс MO2.

### Этап 1.2. Конфиг и инстанс

Автор кладёт `firelink-pack.json` рядом с инстансом MO2. `instance.path` в конфиге — относительный путь к корню инстанса.

### Этап 1.3. Запуск packer-а
firelink-pack pack firelink-pack.json

text

**Pipeline (12 шагов):**

1. ReadConfigStep — читает и валидирует `firelink-pack.json`.
2. ReadInstanceStep — читает `MO2/`, `profiles/<profile>/modlist.txt`, `plugins.txt`, `loadorder.txt`. Возвращает `InstanceSnapshot`.
3. IndexArchivesStep — индексирует `MO2/downloads/`, для каждого архива определяет источник через `.meta` или `archiveSources[]`. Возвращает `ArchiveIndex`.
4. ScanModsStep — сканирует все моды из `modlist.txt` (кроме `[NoDelete]`). Возвращает `ModScanResult` (`ModName → files`).
5. ScanExtensionsStep — сканирует `config.Mo2.Extensions[]`. Каждый entry — относительный путь от корня `MO2/` (файл или папка). Возвращает `EntryScanResult` (`entryName → files`).
6. ScanExtrasStep — симметричен `ScanExtensionsStep`, но читает `config.StockGame.Extras[]` и работает от корня `Stock Game/`. Возвращает `EntryScanResult`.
7. MatchStep — сопоставляет файлы модов с архивами. Использует **общий** `ArchiveMatcher`. Unmatched модов выгружает в `__Firelink_Output/MO2/mods/<ModName>/<path>`. `meta.ini` мода уходит в `ModMetas`. Возвращает `MatchResult`.8. MatchExtensionsStep — сопоставляет файлы extensions с архивами. Принимает **общий** `ArchiveMatcher` через `Input`. Возвращает `MatchEntriesResult` (`Directives`, `Unmatched`). Unmatched **не пишет** — только возвращает.
9. MatchExtrasStep — симметричен `MatchExtensionsStep`, но для extras.
10. Write unmatched extensions/extras (private-хелпер `PackPipeline`) — пишет unmatched в `__Firelink_Output/MO2/<path>` и `__Firelink_Output/Stock Game/<path>` (плоско). Перед записью чистит соответствующие корни (`MO2/` — кроме `mods/`; `Stock Game/` — целиком).
11. BuildManifestStep — собирает `ModlistManifest`. Заполняет `Mo2.Extensions[]` из `ExtensionsMatch.Directives` и `StockGame.Extras[]` из `ExtrasMatch.Directives`. Порядок entry — как в config.
12. ValidateManifestStep — валидирует manifest: уникальность id/имён, ссылочная целостность директив, лимиты inline. WriteManifestStep — пишет `modlist.json` в `__Firelink_Output/`.

**`ArchiveMatcher`:**

- Один экземпляр создаётся в `PackPipeline.ExecuteAsync` перед `MatchExtensionsStep`, `BuildAsync(ct)` вызывается один раз (12.13.6, 12.11.7.1).
- Передаётся в `MatchStep`, `MatchExtensionsStep` и `MatchExtrasStep` через `Input.Matcher` (`internal`).
- `ArchiveMatcher` детерминирован: при дубликатах `(hash, path)` или `hash` между архивами побеждает минимальный `archiveId` (Ordinal).
- Отмена: `BuildAsync` **пробрасывает** `OperationCanceledException` как есть, не «глотает» отмену. Прочие ошибки (битый архив, ошибка 7z) — skip с логированием.

### Этап 1.4. Итог

- `__Firelink_Output/modlist.json` — манифест.
- `__Firelink_Output/MO2/mods/<ModName>/<path>` — unmatched модов.
- `__Firelink_Output/MO2/<path>` — unmatched extensions.
- `__Firelink_Output/Stock Game/<path>` — unmatched extras.

### Этап 1.5. Публикация
Автор публикует `modlist.json`. Зеркала для архивов (mirror) —
опционально.

## Пайплайн: установка сборки

### Этап 2.1. Получение манифеста

Пользователь кладёт `modlist.json` куда угодно.

### Этап 2.2. Запуск installer-а

```
firelink-install install C:\Downloads\modlist.json
```

или с явным target:

```
firelink-install install C:\Downloads\modlist.json --target D:\Games\MyPack
```

**Расположение инстанса:** `<exeDir>/Instances/<normalize(meta.name)>/`.

**Pipeline:**

1. ReadManifestStep — читает modlist.json, валидирует schemaVersion.
2. ResolveTargetStep — вычисляет instancePath, копирует манифест в `<instancePath>/modlist.json`.
3. ValidateTargetStep — 4 проверки (корень диска, системные папки, папка exe, права записи).
4. BootstrapInstanceStep — создаёт MO2/, MO2/downloads/, MO2/mods/, MO2/profiles/, MO2/plugins/, MO2/tools/, Stock Game/. Контрактные проверки.
5. BootstrapMo2Step — самодостаточный: сам скачивает MO2-архив в MO2/downloads/ (если нет по хешу), сам распаковывает в MO2/ с заменой.
6. SyncArchivesStep — сканирует downloads/ → hash → path, для каждого mod-архива скачивает или находит локально. Только manifest.Archives, без MO2.
7. SyncModsStep — reconcile mods/ под манифест.
8. GenerateMetaIniStep — reconcile meta.ini.
9. RegenerateProfileStep — генерирует modlist.txt/plugins.txt/loadorder.txt.
10. ExecuteExtensionsStep — [12.9].
11. ExecuteExtrasStep — [12.9].
12. ConfigureMo2Step — [техдолг, не делаем].

**Разделение ответственности:**
- **MO2-логика** полностью в `BootstrapMo2Step` (шаг 5).
- **Логика mods/** полностью в `SyncArchivesStep` + `SyncModsStep` (6–7).
- Никакой связи между ними.

**MO2-логика** — в `BootstrapMo2Step` (шаг 5).
**Логика mods/** — в `SyncArchivesStep` + `SyncModsStep` (6, 9).
**Логика extensions** — в `ExecuteExtensionsStep` (7).
**Логика extras** — в `ExecuteExtrasStep` (8).
Никакой связи между ними.

### Этап 2.3. Запуск игры

Пользователь:
Копирует игру в instance/Stock Game/ (вручную).
Открывает instance/MO2/ModOrganizer.exe.
Выбирает профиль.
Запускает игру через SKSE.

Решения:
Firelink не работает с игрой. Не ищет, не копирует, не проверяет.
Firelink не управляет порядком загрузки — он приходит из манифеста.
Stock Game/ — просто папка для extras.

## Пайплайн: обновление сборки

**Реализовано через GUI (3.9.8).**

### Как работает

1. Пользователь кликает `Update` на карточке существующего инстанса
   в Home-дашборде.
2. Открывается диалог выбора файла (`IFilePickerService.PickFileAsync`,
   фильтр `.json`). Пользователь указывает путь к новому
   `modlist.json` (например, `Загрузки/modlist.json`).
3. Если пользователь отменил — no-op.
4. Если выбрал — `MainWindowVM` переключается на `Install` и вызывает
   `InstallVM.PrepareForInstall(<выбранный>, <InstancePath>)`.
5. Пользователь видит экран Install с уже заполненными `ModlistPicker`
   и `TargetPicker`, нажимает `Install`.
6. `InstallPipeline`:
   - `ResolveTargetStep.CopyManifestIfNeeded` копирует выбранный
     манифест в `<InstancePath>/modlist.json` (перезапись).
   - Дальше — обычный пайплайн: SyncArchives, SyncMods,
     GenerateMetaIni, RegenerateProfile.
7. По итогу `<InstancePath>/modlist.json` — обновлённый, инстанс
   синхронизирован с ним.

### Отличие от CLI

CLI-команды `firelink update` нет. Обновление делается через
`firelink install <new-manifest> --target <existing-instance>` —
тот же pipeline, тот же результат. GUI-кнопка `Update` — это
удобная обёртка над этим сценарием.

### Про идемпотентность

Если пользователь выбрал тот же самый манифест, что уже лежит в
инстансе, — install идемпотентен: ничего не меняется, всё `Skipped`.

## Замысел (v0.2.0+)

**Этап 3.1.** Автор выпускает новую версию:
Меняет meta.version.
Пересобирает modlist.json.
Возможно, добавляет/удаляет моды, плагины, extras.
Имя инстанса не содержит версию — обновление идёт «поверх».

**Этап 3.2.** Пользователь:

```
firelink-install install C:\Mods\NordicUI\modlist.json
```

Installer видит, что инстанс есть. Верифицирует по новому манифесту.
Докачивает новые архивы. Дописывает новые файлы. Удаляет моды, которых
больше нет в манифесте. Перегенерирует профиль. Результат — инстанс
обновлён без перекачки всего.

## Работа с Nexus Mods

**Реализовано (Фаза 6, 12.8).**

### Философия

Nexus — один источник (`type: "nexus"`). Способы доступа — стратегии
внутри `NexusDownloader`. Не дублируем в манифесте.

### Что делает

`NexusDownloader : IArchiveDownloader` (`SourceType => "nexus"`):

1. Проверяет, что аккаунт Premium, через `GET /v1/users/validate.json`.
   Результат кешируется на время жизни `NexusClient` — один запрос
   на весь pipeline.
2. Запрашивает список CDN-ссылок через
   `GET /v1/games/{game}/mods/{modId}/files/{fileId}/download_link.json`.
3. Перебирает ссылки по очереди: первая успешная выигрывает.
4. Возвращает `Stream` с содержимым архива.

Скачивание идёт в `TempFileStream` (временный файл на диске), а не в
`MemoryStream` — у Nexus есть моды на 3+ ГБ, `MemoryStream` такие не
держит.

Retry, `.part`-файлы, hash-check — на стороне `ArchiveDownloadHelper`,
как для `MirrorDownloader`.

### Аутентификация

API-ключ читается из `%USERPROFILE%\.firelink\nexus.key` (plaintext,
одна строка). Отправляется как HTTP-заголовок `apikey`.

Дополнительные заголовки на каждый запрос:
- `Application-Name: Firelink`
- `Application-Version: 0.1.0`
- `User-Agent: Firelink/0.1.0`

### HttpClients

Через `IHttpClientFactory`, именованные клиенты:
- `"nexus-api"` — 2 минуты (API-запросы).
- `"nexus"` — 10 минут (CDN-ноды).
- `"mirror"` — 10 минут (mirror).

### Обработка ошибок

| HTTP | Значение |
|---|---|
| 401 | `InvalidOperationException` — ключ невалиден или отозван |
| 403 | `InvalidOperationException` — нужен Premium |
| 404 | `InvalidOperationException` — мод/файл не найден |
| 429 | `InvalidOperationException` — rate limit |
| Прочее | `HttpRequestException` |

### Что не делаем

- OAuth-логин (только API-key).
- Кеширование download-ссылок (они временные).
- `nxm://`, WebView2 (это Фаза 5, Nexus Free).
- Автоматический retry внутри `NexusDownloader` — retry на
  `ArchiveDownloadHelper`.

### v0.2.0+

- DPAPI-шифрование файла ключа.
- SQLite-хранилище.
- OAuth-логин (альтернатива API-key).

Глобальный реестр архивов
Не реализовано. v0.2.0.

Назначение
Переиспользование архивов между сборками без повторной загрузки.

Расположение
text
%USERPROFILE%\.firelink\archives.db
Схема (черновик)
sql
CREATE TABLE CachedArchive (
  hash TEXT PRIMARY KEY,      -- xxHash64 архива
  path TEXT NOT NULL,         -- полный путь к файлу
  size INTEGER NOT NULL,
  last_seen TEXT NOT NULL     -- ISO 8601
);

CREATE TABLE Config (
  key TEXT PRIMARY KEY,
  value BLOB NOT NULL,
  updated_at TEXT NOT NULL
);
Как работает
При поиске архива:

Проверить локальную instance/MO2/downloads/.

Если нет — проверить реестр.

Если файл найден — скопировать в локальную downloads/.

Если файла нет — удалить запись, скачать заново.

При сканировании downloads/:

Просканировать папку, посчитать хеши.

Добавить/обновить записи в реестре.

При скачивании:

Записать в реестр: (hash, path, size, now).

При повреждении реестра:

Пересоздать из сканирования папок downloads/ всех известных
инстансов.

## CLI команды

Имя exe — `Firelink.Cli.exe`. Usage-строка — `firelink`
(сознательное расхождение display name и file name).

| Команда | Описание |
|---|---|
| `firelink pack <config>` | Создать манифест (`<config>` — путь к `firelink-pack.json`) |
| `firelink install <manifest> [--target <dir>]` | Установка |
| `firelink verify <target> [--verbose]` | Проверка целостности |
| `firelink hash <file>` | xxHash64 (`xxh64:hex`) |
| `firelink doctor` | Диагностика окружения (stub) |

**Return codes:**

- `pack`: 0 — OK, 2 — ошибка (включая CLI-parse), 130 — отменено (Ctrl+C).
- `install`: 0 — OK, 2 — ошибка, 130 — отменено (Ctrl+C).
- `verify`: 0 — OK, 1 — есть падения, 2 — ошибка, 130 — отменено (Ctrl+C).

**Не делаем / отложено:**

| Команда | Статус |
|---|---|
| `firelink index` | Не делаем (нет SQLite-индекса) |
| `firelink repair` | Не делаем (install идемпотентен) |
| `firelink cache list/prune/rebuild` | v0.2.0+ (глобальный реестр) |
| `firelink config set/get/clear/list` | v0.2.0+ (API-ключ через конфиг) |

## Обработка ошибок

### Матрица packer-а

Ситуация	Поведение
firelink-pack.json не найден	FileNotFoundException
Невалидный JSON	InvalidOperationException («Failed to parse»)
meta.name не проходит NameValidator	InvalidOperationException
meta.version не semver	InvalidOperationException
instance.path не проходит InstancePathValidator	InvalidOperationException
mo2.source не MirrorSourceRef	InvalidOperationException
instance.path не существует	DirectoryNotFoundException
MO2/ не существует	DirectoryNotFoundException
downloads/ не существует	DirectoryNotFoundException
mods/ не существует	DirectoryNotFoundException
Профиль не найден	DirectoryNotFoundException
Мод enabled, папки нет	DirectoryNotFoundException
Мод disabled, папки нет	Warning, пропустить
Сепаратор без папки	LogDebug, пропустить
Дубликат archiveId	InvalidOperationException
.meta без modID/fileID	Warning, fallback на archiveSources
Архив без .meta и без archiveSources	UnresolvedArchive, warning
mo2.archive не найден в downloads/	Size = 0, hash из mo2.source.hash
Hash MO2-архива mismatch	InvalidOperationException
Entry в mo2.extensions[] не найден	FileNotFoundException
Entry в stockGame.extras[] не найден	FileNotFoundException
Файл мода не найден в архивах	Unmatched → __Firelink_Output
Файл extension/extra не найден в архивах	Unmatched → __Firelink_Output
FromArchiveDirective.Archive не существует	InvalidOperationException
Ctrl+C во время pack	Cancelled., exit 130
Пути с пробелами без кавычек	CLI error + hint, exit 2

### Матрица installer-а

Ситуация	Поведение
schemaVersion не поддерживается	Ошибка
meta.name не проходит NameValidator	Ошибка
--target — корень / системная папка / папка exe	Ошибка
Нет прав записи	Ошибка
Инстанс не существует	Создать
Архив есть локально с нужным хешем	Пропустить
Архив с другим именем, но тем же хешем	Использовать
Архив отсутствует	Скачать по sources
nexus (до блока 9)	Warning, Skipped, fallback
nexus (после блока 9)	NexusDownloader — API, качает
Hash не совпадает	Удалить .part, следующий источник
Все источники провалились	Ошибка
Мод [NoDelete]	Пропустить
Мод-сепаратор (#...)	Пропустить
mods[].meta != null	MetaIniWriter.WriteFile
mods[].meta == null, файл есть	Удалить
mods[].meta == null, файла нет	Ничего не делать
MO2: локальный архив с нужным хешем	Использовать
MO2: локальный архив с другим хешем	Перекачать
MO2: архива нет	Скачать
MO2: hash mismatch после скачивания	.part удалён, следующий источник
MO2: все источники провалились	Ошибка
MO2: распаковка	Всегда, с заменой (идемпотентно)
Ctrl+C во время install / verify	Cancelled., exit 130
Пути с пробелами без кавычек	CLI error + hint, exit 2
nexus, ключ отсутствует	Ошибка: InvalidOperationException
nexus, аккаунт не Premium	Ошибка: InvalidOperationException
nexus, все CDN-ноды упали	Ошибка: InvalidOperationException (внутри NexusDownloader)

### Матрица GUI

| Ситуация | Поведение |
|---|---|
| Install: пользователь отменил | State = Configuration, ErrorMessage = "Cancelled." |
| Install: pipeline бросил | State = Failure, ErrorMessage = ex.Message |
| Install: pipeline успех | State = Success, Summary заполнен |
| FilePicker: пользователь отменил | Path не меняется |
| FilePicker: путь невалиден | IsValid = false, Error = "File not found" / "Folder not found" |
| Home: клик `Open MO2`, `ModOrganizer.exe` не найден | `WarningMessage = "ModOrganizer.exe not found. Reinstall the pack to restore MO2."`, launcher не вызывается |
| Home: клик `Open MO2`, `Process.Start` бросил | `WarningMessage` с текстом исключения |
| Home: клик `Open MO2`, успех | `WarningMessage` сбрасывается в `null` |
| Home: клик `Update`, юзер отменил диалог | no-op, `WarningMessage` сбрасывается |
| Home: клик `Install` / `Update` | `MainWindowVM.OnInstallRequested(manifest, target)` → `NavigateTo(Install)` → `InstallVM.PrepareForInstall(...)` |
| Home: пустой список инстансов | Пустой экран, заголовок `Home` остаётся |
| Settings: переключение DevMode | `NavigationVM.RebuildItems()`; если текущий экран скрыт — переход на Home |

Технологический стек
Компонент	Технология
Платформа	.NET 8 (net8.0), C# 12
CLI	Spectre.Console.Cli
JSON	System.Text.Json
Хеширование	System.IO.Hashing (xxHash64)
Распаковка	7z.exe + 7z.dll (Assets/7z/)
База данных	Microsoft.Data.Sqlite (v0.2.0)
Шифрование	System.Security.Cryptography.ProtectedData (DPAPI, v0.2.0)
DI	Microsoft.Extensions.DependencyInjection
Логирование	Microsoft.Extensions.Logging (+ .Console)
Retry	Polly 8 (в Firelink.Core — блок 5)
HTTP	Microsoft.Extensions.Http
Тесты	xUnit + FluentAssertions
Целевая ОС	Windows 10 1809+ / Windows 11
Удалено: SharpCompress (заменён на 7z), Octokit (GitHub cleanup).

Дорожная карта

MVP (v0.1.0)

Packer:

☑ Скелет solution.
☑ Core: модели, JSON, XxHash64Value, IStep.
☑ MO2-файлы: чтение/запись.
☑ MetaReader, MetaIniReader, MetaIniWriter.
☑ Packer: 13 шагов, полный pipeline.
☑ UtcDateTimeOffsetJsonConverter.
☑ Mo2Section.Profile.
☑ ScanExtensionsStep / ScanExtrasStep.
☑ MatchExtensionsStep / MatchExtrasStep.
☑ Унификация ArchiveMatcher (12.13.6).
☑ ArchiveMatcher.Build → BuildAsync (12.11.7.1).

Installer:

☑ 12.1 — ReadManifestStep, ResolveTargetStep, ValidateTargetStep.
☑ 12.2 — BootstrapInstanceStep.
☑ 12.3 — SyncArchivesStep (локально + mirror).
☑ 12.4 — SyncModsStep.
☑ 12.5 — GenerateMetaIniStep + MetaIniWriter.
☑ 12.6 — RegenerateProfileStep.
☑ 12.7 — BootstrapMo2Step (самодостаточный).
☑ 12.9 — ExecuteExtensionsStep + ExecuteExtrasStep.
☑ 12.10 — InstallPipeline.
☑ 12.11.1–12.11.3 — Verify (Pipeline + Tests + CLI).
☑ 12.12 — интеграционный тест pack → install.
☑ 12.8 — NexusDownloader (Фаза 6).

**Хвосты MVP (перед 12.8):**

- [x] 12.13.7 + 12.13.8 — таблицы CLI.
- [x] 12.11.5 — `meta.ini` в verify.
- [x] 12.11.6 — extensions/extras в verify.
- [x] 13.1 — общий helper скачивания.
- [x] 12.11.7 — Ctrl+C в CLI.
- [x] 12.13.10 — error-msg для пробелов без кавычек.

v0.2.0
□ Глобальный реестр archives.db.
□ Persist кеша хешей (SQLite).
□ Прогресс-бар Spectre.
□ File-logging (ротация).
□ Команда firelink-install cache list/prune/rebuild.
□ Команда firelink-install config set/get/clear/list.
□ Механизм патчей для inline-файлов.

v0.3.0
□ NexusFreeStrategy (реальная реализация).
□ Команда doctor — расширенная диагностика.
□ Поддержка нескольких игр (проверка meta.game).
□ Пайплайн обновления сборки.
v1.0.0
□ GUI.
□ Hardlink-режим для кеша.
□ Документация для авторов сборок.

## Статус реализации

**Обновлено:** 2026-09-25
**Версия документа:** 4.8

### Готово

- Полный pipeline packer-а (13 шагов).
- Installer: 12.1–12.7, 12.9.1, 12.10, 12.11.1–12.11.7, 12.12,
  12.13.1–12.13.10, 13.1.
- **Фаза 1 — единый CLI `Firelink.Cli.exe`.**
- **Фаза 2 — общие API для GUI.**
- **Фаза 6 — Nexus Premium (12.8).**
- **Фаза 3 — шаги 3.1–3.8:**
  - 3.1 — `Firelink.Gui.Shared`, `Firelink.Gui.Install`,
    `Firelink.Gui.Pack`, `Firelink.Gui.Verify`, `Firelink.Gui`.
  - 3.2 — `MainWindow`, `MainWindowVM`, `NavigationVM`,
    `ViewLocator`, плейсхолдеры (удалены в 3.6).
  - 3.3 — `LogVM`, `LogView`, `ObservableLoggerProvider` в UI.
  - 3.4.1 — `IScreenFactory`, `INavigationAware`,
    `IFilePickerService`, `ScreenFactory`,
    `AvaloniaFilePickerService`.
  - 3.4.2 — `InstallVM`, `InstallView`, `IInstallRunner`,
    `InstallRunner`, проект `Firelink.Gui.Controls`.
  - 3.5 — `PackVM`, `PackView`, `IPackRunner`, `PackRunner`,
    проект `Firelink.Gui.Pack`.
  - 3.5.1 — `IUiDispatcher`, `AvaloniaUiDispatcher`,
    потокобезопасный `ObservableLogSink`.
  - 3.6 — `VerifyVM`, `VerifyView`, `VerifyRowVM`,
    `IVerifyRunner`, `VerifyRunner`, проект `Firelink.Gui.Verify`.
  - 3.7 — `Directory.Build.props` (VersionPrefix 0.1.0,
    IncludeSourceRevisionInInformationalVersion=false),
    иконка GUI (`Assets\app.ico` + `<ApplicationIcon>`),
    версия CLI из assembly (`GetApplicationVersion()`),
    `tools/build-release.bat` (publish GUI+CLI в одну папку,
    zip).
  - 3.8 — ручной прогон GUI на `OmenRim 7` / `OmenTest7`:
    pack (71 mods, 7853 files, 4389 directives), install
    (71 created, 58 downloaded, 71 meta.ini), verify
    (**4522 passed, 0 failed**).
- **Фаза 3.9 — редизайн GUI + Home-дашборд (шаги 3.9.1–3.9.8):**
  - 3.9.1 — палитра и типографика (App.axaml).
  - 3.9.2 — стили базовых контролов (Button, CheckBox,
    ListBox, ScrollBar, ProgressBar, TextBlock-классы).
  - 3.9.3 — NavigationView v2 (без шапки/глоу/гамбургера;
    иконки Lucide).
  - 3.9.3.3 — Settings (ScreenType.Settings, SettingsVM,
    SettingsView).
  - 3.9.4 — Logs в отдельной вкладке, перекраска LogView,
    удаление автоочистки.
  - 3.9.5 — FilePickerView v2 (палитра + TextBox-стили).
  - 3.9.6 — Pack/Install/Verify: хардкод-цвета убраны,
    Home → Done.
  - 3.9.7.1 — DevMode (in-memory) + тумблер в Settings +
    NavigationVM + SettingsVM.IsDevMode.
  - 3.9.7.2 — InstalledPackScanner + InstalledPackInfo +
    IInstalledPackScanner; сканирование
    `<exeDir>/Instances/`.
  - 3.9.7.3 — HomeVM дашборд + InstalledPackVM карточка +
    HomeView.axaml; IInstallRequestHandler + IInstallTarget.
  - 3.9.8 — три кнопки на карточке (Open MO2 / Install /
    Update); IProcessLauncher + ShellProcessLauncher;
    WarningMessage inline.
- **813 тестов, все проходят.**

### В работе

- — (Фаза 3.9 закрыта целиком: 3.9.1–3.9.8)

### Ключевые решения

- `MatchStep` использует **общий** `ArchiveMatcher` через `Input.Matcher`.
  Один extract на весь pipeline packer-а (12.13.6).
- `ArchiveMatcher.BuildAsync` — async, отмена пробрасывается, не
  глотается (12.11.7.1).
- Unmatched extensions/extras → плоско в `__Firelink_Output/MO2/<path>`
  и `__Firelink_Output/Stock Game/<path>`.
- `entry` с пустым списком директив не попадает в
  `manifest.Mo2.Extensions[]` и `manifest.StockGame.Extras[]`.
- Папки `__Firelink_Output/MO2/` (без `mods/`) и
  `__Firelink_Output/Stock Game/` создаются только при непустых unmatched.
- `.bsa`/`.ba2` — единые файлы, не контейнеры.
- `InlineFile` / `OrphanFile` — модель удалена.
- `firelink index` / `firelink repair` — не делаем.
- `firelink cache` / `firelink config` — v0.2.0+.
- Verify сравнивает `meta.ini` **семантически**.
- Verify проверяет `mo2.extensions[]`/`stockGame.extras[]`.
- `ArchiveDownloadHelper` — общий helper для `SyncArchivesStep` и
  `BootstrapMo2Step` (13.1).
- `CancellationHelper.IsCancellation` — единая точка распознавания
  «отмены» (включая `AggregateException`).
- `config.PropagateExceptions()` в CLI — для того, чтобы
  обрабатывать `CommandParseException` самим.
- **Единый CLI:** `Firelink.Cli` (exe → `Firelink.Cli.exe`) +
  `Firelink.Pack` и `Firelink.Install` как class libraries.
- **DI:** `AddFirelinkPack` / `AddFirelinkInstall` в
  `Firelink.Pack/PackServices.cs` и
  `Firelink.Install/InstallServices.cs`.
- **`Firelink.exe`** зарезервировано под GUI (Фаза 3).
- **`StepProgress`** — общий тип в `Firelink.Core.Progress`.
  `Report` перед шагом. Packer: 14, installer: 11 имён.
- **Фабрики** — единственная точка нормализации путей и
  `ParallelOptions`. Static-классы, не DI.
- **Summary** — плоский DTO только из примитивов. Builder-ы
  static. Полные списки — не в Summary.
- **CLI рисует таблицы из Summary**, не из `Output`. Внешний
  вид не изменился.
- **GUI-логи маршалятся на UI-поток через `IUiDispatcher`**
  (решение №183). Устраняет гонку `ObservableCollection` с
  worker-потоками pipeline.
- **`VerifyRunner` оборачивает синхронный `VerifyPipeline.Execute`
  в `Task.Run`** (решение №185). UI не блокируется.
- **`VerifyVM` показывает только `Failures` по умолчанию;
  чекбокс «Show all checks» заменяет CLI-флаг `--verbose`**
  (решение №187).
- **Плейсхолдеров GUI больше нет** — все 4 экрана реальные.
- **Версия — в `Directory.Build.props`**
  (`<VersionPrefix>0.1.0</VersionPrefix>`), единый источник
  правды. CLI читает `InformationalVersion` из entry assembly.
- **Иконка GUI** — `Assets\app.ico` + `<ApplicationIcon>`,
  вшивается в PE-заголовок `Firelink.exe`. Иконки CLI нет.
- **`tools/build-release.bat`** — publish GUI+CLI в одну папку
  (`-c Release -r win-x64 --self-contained false`),
  `Compress-Archive` → zip. Раскладка дистрибутива — «как есть»
  (~90 файлов), `lib/`-схема сознательно не делается.
- **3.8 — ручной прогон GUI подтверждает эквивалентность CLI:**
  pack + install + verify на `OmenRim 7` → `OmenTest7`,
  **4522 passed, 0 failed**.
- **Палитра Firelink в `Application.Resources` (App.axaml).**
  Тёмный фон `#1e1e1e` + тёплый песочный акцент `#d3b181`.
- **Только тёмная тема** (`RequestedThemeVariant="Dark"`).
- **`ScreenIconConverter` — `public sealed`.**
- **`BoxShadow` в Avalonia — 5 токенов.**
- **У `Grid` нет `RowSpacing`/`ColumnSpacing`.**
- **Кнопка `Home` → `Done` в Pack/Install/Verify.**
- **Автоочистка лога убрана.**
- **Logs — отдельный экран.**
- **Settings — отдельный экран.**
- **Sidebar без шапки.**
- **DevMode — in-memory, default false.** `SettingsVM.IsDevMode`,
  `NavigationVM` перестраивает `Items`, при выключении скрытый
  экран → переход на Home.
- **`InstalledPackScanner`** сканирует `<exeDir>/Instances/*/modlist.json`.
  Битые манифесты — skip + log.
- **`InstalledPackInfo` — без `IsInstalled`.** Наличие
  `ModOrganizer.exe` проверяется на клике Open MO2, не хранится.
- **`InstalledPackVM` — три команды: Open MO2 / Install / Update.**
  Open MO2 всегда активна, inline `WarningMessage` при отсутствии
  файла.
- **`Update` — диалог выбора нового `modlist.json`.** Установка
  с `target = InstancePath`. Installer сам копирует манифест
  (`ResolveTargetStep`).
- **`IProcessLauncher` в Shared, `ShellProcessLauncher` в Gui.**
- **`IInstallRequestHandler`** — `event Action<string, string>?`
  `(manifestPath, targetPath)`.
- **`IInstallTarget.PrepareForInstall(manifestPath, targetPath)`** —
  оба аргумента обязательны.
- **`Install` всегда шлёт `target = InstancePath`.**
