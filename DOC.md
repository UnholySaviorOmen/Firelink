# Firelink — Документация проекта

**Версия документа:** 3.2
**Обновлено:** 2026-09-16

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

---

## Обзор

**Firelink** — инструмент для создания и установки воспроизводимых сборок модов для Mod Organizer 2. Состоит из двух CLI-приложений:

- **`Firelink.Pack`** — для автора сборки. Создаёт манифест (`modlist.json`) на основе готового инстанса MO2.
- **`Firelink.Install`** — для пользователя. Воспроизводит сборку по манифесту.

**Ключевая идея:** манифест — единственный источник правды. Все файлы восстанавливаются по хешам (`xxHash64`). Firelink работает с **результатом** установки, а не с процессом. Как именно автор ставил моды — нас не интересует.

**Философия installer-а:** тупой исполнитель директив. Не проверяет игру, версии, совместимость. Просто воссоздаёт структуру, которую сделал автор.

**Целевая платформа:** Windows 10 1809+ / Windows 11.

**Целевая версия MO2:** 2.5.2.

**Целевые игры:** только игры, доступные на Nexus Mods. `meta.game` — это **Nexus game domain** (см. раздел [Nexus game domain](#nexus-game-domain)).

---

## Основные принципы

1. **Манифест — единственный источник правды.** Профиль MO2 генерируется из манифеста, а не копируется.
2. **Файлы восстанавливаются по хешам.** `xxHash64`, не по именам и путям.
3. **Firelink работает с результатом, а не с процессом.** Никаких FOMOD-парсеров, XML, `meta.ini` модов.
4. **Одна папка `downloads/` для всех архивов.** Моды, MO2, extras — всё в одном месте.
5. **Идентификация архивов — канонический id.** Для Nexus-модов: `nexus_{game_domain}_{modId}_{fileId}`. Имя файла — только информационное поле.
6. **Читаем `.meta`-файлы MO2.** Если рядом с архивом есть `.meta` в Nexus-формате — используем его. Иначе — ищем явное указание в `firelink-pack.json` (раздел `archiveSources`).
7. **Глобальный реестр архивов.** SQLite в `%USERPROFILE%\.firelink\archives.db`. Переиспользование между сборками.
8. **Ничего не удаляем.** Ни архивы, ни моды, кроме случаев, описанных в reconcile.
9. **Директивы выполняются последовательно.** `lastWins` при конфликтах.
10. **`[NoDelete]` в имени папки** (`MO2/mods/[NoDelete]SkyUI`) защищает пользовательские моды.
11. **BSA/BA2 — единые файлы.** Не разбираем содержимое.
12. **Никаких исполняемых скриптов.** Только декларативные директивы.
13. **Installer идемпотентен.** Можно запускать повторно.
14. **Installer не работает с игрой.** Не ищет, не копирует, не проверяет. `Stock Game/` — просто папка для extras.
15. **Шаги pipeline изолированы.** Не вызывают друг друга. Pipeline — единственный оркестратор.
16. **Nexus — один источник, несколько стратегий доступа.** Не дублируем в манифесте.
17. **Параллелизм на уровне pipeline.** Шаги, работающие с файловой системой, используют `ParallelOptions` из DI. `IStep` не меняется.
18. **Кеш хешей — обязателен.** In-memory на время прогона. Persist в SQLite — опционально, для ускорения повторных запусков.
19. **Манифест самодостаточен.** Не полагается на Nexus API при чтении. Все необходимые метаданные (включая `meta.ini` модов) сохранены в самом манифесте.
20. **Unmatched-файлы не сериализуются в манифест.** Всё, что не восстановимо из архивов, выгружается в `__Firelink_Output` (рабочий каталог автора). Автор решает судьбу этих файлов и делает патчи.
21. **`.mohidden` — часть пути, а не отдельная сущность.** Matcher работает с путями как со строками, специальных правил нет.

---

## Глоссарий

- **Инстанс MO2** — папка с `ModOrganizer.exe`, `mods/`, `profiles/`, `downloads/`. В portable-режиме.
- **`modlist.txt`** — файл профиля MO2 со списком модов и флагом «включён».
- **`plugins.txt`** — файл профиля MO2 со списком плагинов и флагом «включён».
- **`loadorder.txt`** — файл профиля MO2 с порядком загрузки плагинов.
- **`.meta`** — файл MO2 рядом с архивом в `downloads/`, содержит метаданные источника (для Nexus — `modID`, `fileID`).
- **`meta.ini`** — файл MO2 внутри папки мода в `mods/`, содержит метаданные мода (для Nexus — `modID`, `fileID`, `version`, `notes`).
- **Nexus game domain** — строковый идентификатор игры на Nexus Mods (например, `skyrimspecialedition`, `fallout4`).
- **Канонический id архива** — строка, однозначно идентифицирующая архив: `nexus_{game_domain}_{modId}_{fileId}` для Nexus-модов, `local_{slug}` для архивов без `.meta`.
- **Манифест** — `modlist.json`, единственный источник правды для installer-а.
- **Packer** — `Firelink.Pack`.
- **Installer** — `Firelink.Install`.
- **Директива** — атомарное действие installer-а (взять файл из архива, создать папку, удалить файл, взять из inline-файла).
- **Reconcile** — процесс приведения инстанса к состоянию, описанному в манифесте, при обновлении.
- **Unmatched-файл** — файл мода, который не удалось восстановить ни по `(hash, path)`, ни по `hash`. Выгружается в `__Firelink_Output`.
- **`__Firelink_Output`** — рабочий каталог автора в корне инстанса. Содержит unmatched-файлы с сохранением структуры. Очищается перед каждым прогоном `pack`.

---

## Архитектура

### Проекты solution

```
Firelink.slnx
src/
  Firelink.Core                  — ядро: модели, хеширование, абстракции
  Firelink.Platform.MO2          — чтение/запись modlist, plugins, loadorder,
                                   .meta (архивов), meta.ini (модов)
  Firelink.Platform.Nexus        — Nexus API + стратегии доступа
  Firelink.Platform.GitHub       — GitHub Releases
  Firelink.Pack                  — CLI для создания сборок
  Firelink.Install               — CLI для установки сборок
tests/
  Firelink.Core.Tests
  Firelink.Platform.MO2.Tests
  Firelink.Pack.Tests
```

### Принципы архитектуры

**Pipeline — единственный оркестратор.** Только pipeline знает порядок шагов. Шаги не знают друг о друге.

**Шаги изолированы.** Каждый шаг — это `IStep<TInput, TOutput>`. Получает вход, возвращает выход. Не вызывает другие шаги.

**Зависимости через DI.** Никаких `ServiceLocator`, никаких `static` классов.

**Ошибки на уровне pipeline.** Шаг либо успешен, либо бросает исключение. Pipeline решает, что делать.

**Параллелизм — через DI.** `ParallelOptions` регистрируется как синглтон (по умолчанию `MaxDegreeOfParallelism = Environment.ProcessorCount`). Шаги, которым нужен параллелизм, инжектят его через конструктор.

### Интерфейс шага

```csharp
public interface IStep<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct);
}
```

### Границы ответственности

**`Firelink.Core`:**

- Модели манифеста и директив.
- Модели `firelink-pack.json` (PackConfig).
- Модели packer-а: `InstanceSnapshot`, `ModScanResult`, `ScannedFile`,
  `MatchResult`, `UnmatchedFile`, `ModMeta`.
- Сериализация JSON.
- Хеширование (`xxHash64`).
- Абстракции (`IArchiveSource`, `IStep`, `IArchiveRegistry`).
- Валидаторы (имя сборки, semver, пути).
- Slug-генератор для fallback-идентификаторов.

**`Firelink.Platform.MO2`:**

- Чтение/запись `modlist.txt`, `plugins.txt`, `loadorder.txt`.
- Чтение `.meta`-файлов архивов (Nexus-формат, только `gameName`/`modID`/`fileID`).
- Чтение `meta.ini`-файлов модов (Nexus-формат, полная секция `[General]`).
- Валидация структуры инстанса.
- Генерация файлов профиля из манифеста.

**`Firelink.Platform.Nexus`:**

- `NexusSource : IArchiveSource` — единый источник.
- Стратегии доступа: `NexusApiStrategy` (работает), `NexusFreeStrategy` (заглушка).
- Работа с API, rate-limiting, retry.
- Хранение и чтение API-ключа (SQLite + DPAPI).
- Кеш game domain → game ID (если понадобится).

**`Firelink.Platform.GitHub`:**

- `GitHubSource : IArchiveSource`.

**`Firelink.Pack`:**

- CLI: `pack`, `index`, `verify`, `doctor`.
- `PackPipeline` + шаги.
- Шаги: `ReadConfigStep`, `ReadInstanceStep`, `IndexArchivesStep`, `ScanModsStep`, `ScanExtensionsStep`, `ScanExtrasStep`, `MatchStep`, `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.

**`Firelink.Install`:**

- CLI: `install`, `verify`, `repair`, `cache`, `config`, `doctor`.
- `InstallPipeline` + шаги.
- Шаги: `ReadManifestStep`, `ResolveTargetStep`, `ValidateTargetStep`, `BootstrapInstanceStep`, `BootstrapMo2Step`, `SyncArchivesStep`, `ExecuteExtensionsStep`, `ExecuteExtrasStep`, `SyncModsStep`, `RegenerateProfileStep`, `ConfigureMo2Step`.

---

## Структура папок

### Рабочая папка автора

```
C:\Mods\Dev\
  firelink-pack.json              ← конфиг packer-а
  NordicUI Overhaul\              ← инстанс MO2
    MO2\
      ModOrganizer.exe
      portable.txt
      plugins\
      tools\
      downloads\                  ← архивы (моды + MO2 + extras)
        SkyUI_5_1-3863-5-1.7z
        SkyUI_5_1-3863-5-1.7z.meta   ← .meta рядом
        Mod.Organizer-2.5.2.7z
      mods\
        SkyUI\
          meta.ini                ← файл мода (не архивный)
          interface/iconmenu.swf
      profiles\NordicUI\          ← modlist.txt, plugins.txt, loadorder.txt
    Stock Game\                   ← extras (SKSE, ENB)
    __Firelink_Output\            ← unmatched-файлы (после pack)
      mods\
        SkyUI\
          SKSE/Plugins/foo.ini
```

### Рабочая папка пользователя (после установки)

```
C:\Mods\NordicUI\
  modlist.json                    ← манифест
  firelink.log                    ← общий лог
  firelink-2026-09-13.log         ← ротация по дням
  NordicUI Overhaul\              ← инстанс MO2 (имя из meta.name)
    MO2\
      ModOrganizer.exe
      portable.txt
      ModOrganizer.ini            ← генерируется installer-ом
      plugins\
      tools\
      downloads\
      mods\
        [NoDelete]SkyUI\          ← пользовательский мод, не трогаем
        SkyUI\                    ← мод из сборки
          meta.ini                ← генерируется installer-ом из mods[].meta
      profiles\
    Stock Game\                   ← extras (игра копируется пользователем)
```

### Глобальные данные

```
%USERPROFILE%\.firelink\
  archives.db                     ← реестр архивов + конфиг (API-ключи)
```

---

## Форматы данных

### `firelink-pack.json`

Конфиг packer-а. Создаётся автором вручную рядом с инстансом MO2.

```json
{
  "meta": {
    "name": "Nordic UI Overhaul",
    "version": "1.2.0",
    "author": "Username",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },
  "mo2": {
    "version": "2.5.2",
    "archive": "Mod.Organizer-2.5.2.7z",
    "source": {
      "type": "github",
      "repo": "ModOrganizer2/modorganizer",
      "tag": "v2.5.2",
      "asset": "Mod.Organizer-2.5.2.7z"
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
          "url": "https://cdn.example.com/SomeModWithoutMeta.7z"
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
          "type": "github",
          "repo": "author/repo",
          "tag": "v1.0",
          "asset": "AnotherMod.7z"
        }
      ]
    }
  ]
}
```

**Поля:**

| Поле | Описание |
|---|---|
| `meta.name` | Имя сборки. Используется как имя папки инстанса. |
| `meta.version` | Версия сборки (semver). |
| `meta.author` | Автор. |
| `meta.game` | **Nexus game domain** (см. раздел [Nexus game domain](#nexus-game-domain)). |
| `meta.gameVersion` | Целевая версия игры (информационное). |
| `mo2.version` | Версия MO2. |
| `mo2.archive` | Имя архива MO2 в `downloads/`. |
| `mo2.source` | Источник для скачивания MO2 (тип `ArchiveSourceRef`). |
| `mo2.extensions` | Пути к файлам/папкам extensions **относительно `MO2/`**. |
| `stockGame.extras` | Пути к файлам/папкам extras **относительно `Stock Game/`**. |
| `archiveSources[]` | Явное указание источников для архивов без `.meta`. |
| `archiveSources[].archive` | Имя файла в `downloads/` (например, `SkyUI.7z`). |
| `archiveSources[].sources[]` | Список источников для этого архива. |

**Валидация:**

- `meta.name` не должен содержать `< > : " / \ | ? *` и управляющие символы. Не должен быть зарезервированным именем Windows (`CON`, `PRN`, `AUX`, `NUL`, `COM1`–`COM9`, `LPT1`–`LPT9`). Не должен начинаться или заканчиваться пробелом или точкой.
- `meta.version` должен быть валидным semver 2.0.0.
- `meta.game` должен быть непустой строкой. Желательно — из известных game domain (предупреждение при неизвестном, но не ошибка).
- `mo2.archive` должен существовать в `downloads/`.
- Пути в `mo2.extensions` и `stockGame.extras` должны быть относительными, без `..`, без абсолютных путей, без ведущего `/` или `\`.
- `archiveSources[].archive` не должен повторяться.
- Каждый `archiveSources[].sources[]` — валидный `ArchiveSourceRef`.

---

### `modlist.json`

Манифест сборки. Генерируется packer-ом, читается installer-ом.

```json
{
  "schemaVersion": "1.0.0",
  "manifestVersion": "1.2.0",
  "createdAt": "2026-09-13T10:00:00Z",
  "createdBy": "firelink-pack/0.1.0",

  "meta": {
    "name": "Nordic UI Overhaul",
    "version": "1.2.0",
    "author": "Username",
    "game": "skyrimspecialedition",
    "gameVersion": "1.6.1170"
  },

  "execution": {
    "directives": "sequential",
    "onConflict": "lastWins"
  },

  "mo2": {
    "version": "2.5.2",
    "archive": {
      "id": "github_modorganizer2_modorganizer_v2.5.2_Mod.Organizer-2.5.2.7z",
      "name": "Mod.Organizer-2.5.2.7z",
      "size": 123456789,
      "hash": "xxh64:abc123...",
      "sources": [
        {
          "type": "github",
          "repo": "ModOrganizer2/modorganizer",
          "tag": "v2.5.2",
          "asset": "Mod.Organizer-2.5.2.7z"
        }
      ]
    },
    "extensions": []
  },

  "stockGame": {
    "extras": []
  },

  "archives": [
    {
      "id": "nexus_skyrimspecialedition_3863_1000172397",
      "name": "SkyUI_5_1-3863-5-1.7z",
      "size": 12345678,
      "hash": "xxh64:abc123...",
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
        "gameId": "skyrimspecialedition",
        "modId": 3863,
        "fileId": 1000172397,
        "version": "5.1",
        "category": 0,
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
          "hash": "xxh64:mno345...",
          "size": 4570
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
  ],

  "inlineFiles": []
}
```

**Секции:**

| Секция | Описание |
|---|---|
| `schemaVersion` | Версия формата манифеста. |
| `manifestVersion` | Версия сборки. |
| `meta` | Метаданные сборки. |
| `execution` | Правила выполнения директив. |
| `mo2` | Архив MO2 + extensions. |
| `stockGame` | extras. |
| `archives` | Единый пул архивов, каждый с каноническим `id`. |
| `mods` | Моды с директивами, порядком, флагами, **`meta`** (structured `meta.ini`). |
| `plugins` | Плагины с флагами. |
| `loadorder` | Порядок загрузки. |
| `inlineFiles` | Файлы, которых нет в архивах (в MVP — пустой список). |

**Секция `mods[].meta`:**

Обязательна **только** если у мода есть `meta.ini` в исходном инстансе. Опциональна.

Поля (все — из `ModMeta`, см. раздел [Формат meta.ini мода](#формат-meta-ini-мода)):

| Поле | Описание |
|---|---|
| `gameName` | `gameName` из `meta.ini`. Справочно. |
| `gameId` | `gameID` из `meta.ini`. На практике — Nexus game domain. |
| `modId` | `modID` из `meta.ini` (Nexus). |
| `fileId` | `fileID` из `meta.ini` (Nexus). |
| `version` | Версия мода. |
| `category` | Категория мода в MO2. |
| `repository` | Репозиторий-источник (обычно `Nexus`). |
| `url` | URL страницы мода. |
| `comments` | Комментарий MO2. |
| `notes` | Заметки автора сборки. |

**Почему все поля, а не подмножество:** манифест самодостаточен и не полагается на Nexus API при чтении. У пользователя может не быть API-ключа, у автора может быть приватный мод. Все нужные метаданные — в манифесте.

**Ограничения:**

- `inlineFiles[]` — лимит 2 МБ на файл, 20 МБ суммарно (для будущего использования; в MVP пустой).
- `archives[].id` — уникален в пределах манифеста.
- `mods[].name` — уникально.
- `plugins[].name` — уникально.

---

### Директивы

**`FromArchive`** — взять файл из архива:

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

`source` — путь внутри архива. `destination` — путь внутри папки мода.
**Могут отличаться** (FOMOD, `.mohidden`, разные корни).

**`InlineFile`** — файл, которого нет в архивах:

```json
{
  "type": "InlineFile",
  "inlineFileId": "custom-patch-1",
  "destination": "interface/custom.swf"
}
```

**`CreateDirectory`** — создать пустую папку:

```json
{
  "type": "CreateDirectory",
  "destination": "meshes/empty/"
}
```

**`Delete`** — удалить файл из папки мода:

```json
{
  "type": "Delete",
  "destination": "interface/unwanted.swf"
}
```

---

### Источники архивов

**Nexus** — единый источник, стратегии доступа определяются на стороне installer-а:

```json
{
  "type": "nexus",
  "modId": 3863,
  "fileId": 1000172397,
  "game": "skyrimspecialedition"
}
```

**GitHub:**

```json
{
  "type": "github",
  "repo": "ModOrganizer2/modorganizer",
  "tag": "v2.5.2",
  "asset": "Mod.Organizer-2.5.2.7z"
}
```

**Mirror:**

```json
{
  "type": "mirror",
  "url": "https://cdn.example.com/SkyUI.7z",
  "hash": "xxh64:..."
}
```

Поле `hash` в mirror-источнике — опционально, для верификации скачанного файла. Если указано — installer сверяет; если нет — сверяет с `archives[].hash`.

---

### Формат `.meta`

MO2 создаёт `.meta`-файл рядом с каждым скачанным архивом в `downloads/`. Формат — INI-подобный. Для Nexus-модов содержит:

```ini
[General]
gameName=Skyrim
modID=3863
fileID=1000172397

[installedFiles]
...
```

Firelink читает **только секцию `[General]`** и **только поля `gameName`, `modID`, `fileID`**. Остальное игнорирует. Реализация — `MetaReader.TryRead` в `Firelink.Platform.MO2.Readers`.

**Если `.meta`:**

- **Есть и валиден** → архив получает канонический id `nexus_{game_domain}_{modId}_{fileId}`, источник — `NexusSourceRef`.
- **Есть, но не Nexus-формат** (например, `directURL`, `manualURL`, `IPS4`) → **fallback в `archiveSources`**.
- **Нет** → packer ищет запись в `archiveSources`. Если нашёл — использует указанные источники, id = `local_{slug}`. Если не нашёл — архив **игнорируется**, и если он используется модами — **unresolved**.

**Поле `gameName` игнорируется.** Game domain берётся из `meta.game` родительского конфига. Обоснование: инстанс MO2 — это инстанс одной игры, все архивы в его `downloads/` принадлежат одной игре.

---

### Формат `meta.ini` мода

MO2 создаёт `meta.ini` внутри каждой папки мода в `mods/`. Формат — INI-подобный. Для Nexus-модов содержит:

```ini
[General]
gameName=Skyrim Special Edition
gameID=skyrimspecialedition
modID=32349
fileID=795423
version=1.7.0
category=0
repository=Nexus
url=https://www.nexusmods.com/skyrimspecialedition/mods/32349
comments=
notes=

[installedFiles]
1\SKSE\Plugins\ActorLimitFix.dll=...
...
```

Firelink читает **только секцию `[General]`** — полностью. Реализация — `MetaIniReader.TryRead` в `Firelink.Platform.MO2.Readers`.

**Что читается:**

| Поле | Тип | Описание |
|---|---|---|
| `gameName` | `string?` | Справочно. |
| `gameID` | `string?` | На практике — Nexus game domain. |
| `modID` | `int?` | Мод на Nexus. |
| `fileID` | `int?` | Файл на Nexus. |
| `version` | `string?` | Версия мода. |
| `category` | `int?` | Категория в MO2. |
| `repository` | `string?` | Обычно `Nexus`. |
| `url` | `string?` | URL страницы мода. |
| `comments` | `string?` | Комментарий MO2. |
| `notes` | `string?` | Заметки автора сборки. |

**Что игнорируется:**

- Секция `[installedFiles]` — содержит абсолютные пути автора, бесполезна при воспроизведении.
- Поле `newestVersion` — зависит от времени проверки апдейтов.

**Ключи case-sensitive** (`modID` ≠ `modid`). **Имя секции case-insensitive** (`[General]` = `[general]` = `[GENERAL]`).

**Если `meta.ini` нет:** мод в `ModMetas` не попадает. В манифесте `mods[].meta` отсутствует. Installer не генерирует `meta.ini` для этого мода.

**Если `meta.ini` есть, но секция `[General]` пустая:** `ModMeta.IsEmpty == true`, но запись всё равно попадает в `ModMetas` (это сигнал «meta.ini был»).

### `meta.ini` vs `.meta` — разные сущности

|  | `.meta` | `meta.ini` |
|---|---|---|
| Расположение | `downloads/foo.7z.meta` | `mods/<ModName>/meta.ini` |
| Что описывает | Архив (файл) | Мод (папку) |
| Читается | `MetaReader` | `MetaIniReader` |
| Полей | 3 (`gameName`, `modID`, `fileID`) | 10 (см. выше) |
| Используется | `IndexArchivesStep` | `MatchStep` |
| Куда идёт | Канонический id архива | `mods[].meta` манифеста |

---

### `__Firelink_Output`

**Назначение.** Рабочий каталог автора в корне инстанса. Сюда `MatchStep` выгружает все unmatched-файлы — те, что не удалось восстановить ни по `(hash, path)`, ни по `hash`. Автор смотрит на них и решает: это его правки (делать патч-архив) или runtime-мусор (выкинуть).

**Расположение:** `<InstancePath>/__Firelink_Output/`.

**Структура:**

```
<InstancePath>/__Firelink_Output/
  mods/
    <ModName>/
      <relative/path>
```

Структура **в точности совпадает** с путями внутри `mods/`, чтобы автор мог легко сопоставить и упаковать патч.

**Что туда попадает:**

- все unmatched-файлы модов, независимо от размера (лимит 2 МБ убран);
- **не** попадает `meta.ini` (идёт в `ModMetas`).

**Что туда не попадает:**

- matched-файлы (они в манифесте как `FromArchive`);
- `meta.ini`.

**Жизненный цикл:**

1. `MatchStep` в начале `ExecuteAsync` **очищает** `<InstancePath>/__Firelink_Output/mods` (рекурсивно), не трогая саму `__Firelink_Output`.
2. Создаёт пустую `mods/`.
3. По ходу обхода модов копирует unmatched-файлы (`File.Copy overwrite: true`).
4. В конце логирует счётчик и диагностику.

**Почему так:**

- **Не раздувает манифест.** `inlineFiles` в манифесте пуст для MVP. Только matched-директивы.
- **Хирургия.** Автор видит ровно то, что надо превратить в патч. Файл с изменённым содержимым → изменённый хеш → выгружен. Явно.
- **Обновление инстанса.** При обновлении мода старый патч может перестать матчиться по хешу — автор видит это в `__Firelink_Output` и обновляет патч.
- **Простота.** Никаких эвристик «это runtime-лог, это правка, это meta».

**Что делает автор:**

1. Смотрит на `__Firelink_Output/mods/`.
2. Выкидывает очевидный мусор (логи, кеши).
3. Оставляет свои правки.
4. Упаковывает оставшееся в патч-архив (с сохранением структуры).
5. Кладёт патч-архив в `downloads/`, добавляет запись в `archiveSources`.
6. Перезапускает `pack`. Файлы находятся в патч-архиве → становятся `FromArchive` → `__Firelink_Output` для них пуст.

---

## Идентификация архивов

Каждый архив в манифесте имеет **уникальный `id`**. Формат зависит от источника:

**Nexus (есть `.meta`):**

```
nexus_{game_domain}_{modId}_{fileId}
```

Пример: `nexus_skyrimspecialedition_3863_1000172397`.

**Fallback (нет `.meta`, указан в `archiveSources`):**

```
local_{slug}
```

где `slug` — slug от имени файла без расширения. Правила slug:

- привести к нижнему регистру;
- все символы, кроме `a-z0-9`, заменить на `-`;
- схлопнуть повторяющиеся `-`;
- обрезать `-` в начале и конце.

Пример: `SkyUI_5_1-3863-5-1.7z` → `skyui-5-1-3863-5-1`.

**Коллизии:** если два архива имеют одинаковый id — **ошибка**, packer останавливается. На практике коллизии возможны только для fallback-id; для Nexus-id они исключены природой ключа.

---

## Nexus game domain

**Nexus game domain** — строковый идентификатор игры в URL Nexus Mods. Например:

- `skyrimspecialedition` (The Elder Scrolls V: Skyrim Special Edition)
- `skyrim` (The Elder Scrolls V: Skyrim, легендарное издание)
- `fallout4` (Fallout 4)
- `falloutnewvegas` (Fallout: New Vegas)
- `fallout3` (Fallout 3)
- `oblivion` (The Elder Scrolls IV: Oblivion)
- `morrowind` (The Elder Scrolls III: Morrowind)
- `starfield` (Starfield)
- `baldursgate3` (Baldur's Gate 3)
- и другие.

**Где используется:**

- В `firelink-pack.json` → `meta.game`.
- В `modlist.json` → `meta.game` (копируется из packer-конфига).
- В `NexusSourceRef` → поле `game`.
- В `NexusApiStrategy` при обращении к API.

**Валидация:** packer принимает любую непустую строку. Если строка не входит в **известный список** (`KnownGames`) — предупреждение, но не ошибка. Список расширяемый.

**Источник истины для списка:** Nexus API, эндпоинт `/v1/games.json`. Кешируется в `archives.db` (таблица `Config`). При отсутствии кеша — fallback на захардкоженный минимум.

**Манифест самодостаточен:** `mods[].meta.gameId` и `mods[].meta.gameName` сохраняются в манифесте. Installer не обращается к Nexus API при генерации `meta.ini`.

---

## Формат файлов MO2

### `modlist.txt`

```
# This file was automatically generated by Mod Organizer.
+NAT Effect 11
+Effect 11
-# 📂 Мои моды_separator
-Milkdrinker
+Classic
```

**Правила:**

- Кодировка: UTF-8 **with BOM**.
- Переводы строк: CRLF.
- **Первая строка** — комментарий с BOM в начале.
- Формат строки: `[+|-]ИмяМода`.
- **`+`** — мод включён, **`-`** — выключен.
- **Сепараторы** — это отключённые «моды» с именами вида `# ..._separator`. Firelink их **игнорирует**.
- **Порядок строк — обратный UI MO2.** Сверху — низкий приоритет, снизу — высокий. При генерации `modlist.txt` из манифеста список надо **разворачивать**.
- **Имя мода** может содержать любые символы, кроме `\r` и `\n`. Пробелы, эмодзи, кириллица, `+`, `-`, `[`, `]` — допустимы.
- Парсер устойчив к BOM внутри строки, хвостовым пробелам, пустым строкам.

### `plugins.txt`

```
# This file is used by Skyrim to keep track of your downloaded content.
# Please do not modify this file.
*unofficial skyrim special edition patch.esp
*unofficial skyrim creation club content patch.esl
wraithguardvaultfixer.esp
```

**Правила:**

- Кодировка: UTF-8 **with BOM**.
- Переводы строк: CRLF.
- Первые строки — комментарии.
- Формат строки: `[*]ИмяПлагина`.
- **`*`** — плагин включён, без `*` — выключен.
- **Регистр сохраняется** (`Requiem.esp` ≠ `requiem.esp`).
- Порядок строк **не является** порядком загрузки.
- Парсер устойчив к BOM внутри строки, хвостовым пробелам, пустым строкам.

### `loadorder.txt`

```
# This file was automatically generated by Mod Organizer.
Skyrim.esm
Update.esm
Dawnguard.esm
```

**Правила:**

- Кодировка: UTF-8 **with BOM**.
- Переводы строк: CRLF.
- Первая строка — комментарий.
- Просто список имён плагинов.
- **Порядок строк = порядок загрузки.** Сверху — раньше.
- Парсер устойчив к BOM внутри строки, хвостовым пробелам, пустым строкам.

---

## Пайплайн: создание сборки

### Этап 1.1. Подготовка окружения

**Что делает автор:**

1. Ставит портативный MO2 2.5.2.
2. Устанавливает моды через MO2 любым способом.
3. Кладёт архивы модов в `MO2/downloads/`. MO2 создаёт `.meta`-файлы рядом.
4. Устанавливает плагины MO2 в `MO2/plugins/`, `MO2/tools/`.
5. Устанавливает extras в `Stock Game/`.
6. Настраивает профиль: порядок модов, плагинов, load order.
7. Настраивает моды через MCM (по желанию) — эти правки сохраняются в папке мода.

**Результат:** рабочий инстанс MO2.

### Этап 1.2. Конфигурация packer-а

**Что делает автор:**

Создаёт `firelink-pack.json` рядом с инстансом. Указывает метаданные, версию MO2, пути к extensions и extras. Для архивов без `.meta` — заполняет `archiveSources`.

**Результат:** конфиг.

### Этап 1.3. Запуск packer-а

**Команда:**

```
firelink pack C:\Mods\Dev\firelink-pack.json
```

**Pipeline:**

```
1. ReadConfigStep
   └─ читает firelink-pack.json, валидирует meta, пути, semver

2. ReadInstanceStep
   ├─ читает profiles/<Name>/modlist.txt
   ├─ читает profiles/<Name>/plugins.txt
   ├─ читает profiles/<Name>/loadorder.txt
   ├─ вычисляет путь к __Firelink_Output
   └─ индексирует downloads/:
       ├─ для каждого файла-архива ищет <name>.meta
       ├─ если .meta есть и валиден → ArchiveEntry(id=nexus_..., sources=[Nexus])
       ├─ если .meta есть, но не Nexus → fallback в archiveSources
       ├─ если .meta нет → ищет в archiveSources
       │                    → если нашёл → ArchiveEntry(id=local_..., sources=[...])
       │                    → если нет → ArchiveEntry(sources=[]) + флаг "unresolved"
       └─ валидация: нет дубликатов id

3. ScanModsStep
   ├─ параллельно (Parallel.ForEach с лимитом)
   └─ для каждого мода из modlist.txt:
       ├─ если папки нет → ОШИБКА, остановка
       ├─ если папка содержит [NoDelete] → пропустить
       └─ хешировать все файлы (с кешем по (path, length, mtime))
           (включая meta.ini — фильтрация на этом шаге не делается)

4. ScanExtensionsStep
   ├─ для каждого пути из mo2.extensions:
   │   ├─ если файл → хешировать
   │   └─ если папка → хешировать все файлы
   └─ если файл не найден → ПРЕДУПРЕЖДЕНИЕ, игнорировать

5. ScanExtrasStep
   ├─ для каждого пути из stockGame.extras:
   │   ├─ если файл → хешировать
   │   └─ если папка → хешировать все файлы
   └─ если файл не найден → ПРЕДУПРЕЖДЕНИЕ, игнорировать

6. MatchStep
   ├─ очистить __Firelink_Output/mods (если есть)
   ├─ создать __Firelink_Output/mods
   ├─ распаковка архивов (по одному через TempWorkspace)
   ├─ построить индексы:
   │   ├─ (hash, path) → IndexEntry
   │   ├─ hash → List<IndexEntry>  (для fallback по хешу)
   │   └─ path → { hash → archiveId }  (для диагностики)
   ├─ для каждого файла мода:
   │   ├─ если RelativePath == "meta.ini" (в корне мода):
   │   │   ├─ MetaIniReader.TryRead → ModMeta
   │   │   └─ modMetas[modName] = ModMeta (если не null)
   │   ├─ иначе TryMatch:
   │   │   ├─ 1. точное (hash, path) → FromArchive (source == destination)
   │   │   ├─ 2. по hash (fallback) → FromArchive (source ≠ destination)
   │   │   └─ 3. не найдено → unmatched:
   │   │       ├─ UnmatchedFile в список
   │   │       └─ File.Copy в __Firelink_Output/mods/<Mod>/<path>
   │   └─ (для extensions/extras — аналогично, но в текущей версии
   │       ScanExtensionsStep/ScanExtrasStep не реализованы)
   └─ логировать диагностику unmatched (причины, топ-20, по модам)

7. BuildManifestStep     ← НЕ РЕАЛИЗОВАН
   ├─ meta, execution
   ├─ mo2 (архив MO2 + extensions с директивами)
   ├─ stockGame (extras с директивами)
   ├─ archives (единый пул)
   ├─ mods (с mods[].meta из ModMetas)
   ├─ plugins, loadorder
   └─ inlineFiles (пустой в MVP)

8. ValidateManifestStep  ← НЕ РЕАЛИЗОВАН
   ├─ все archive-ссылки существуют в archives
   ├─ все inlineFile-ссылки существуют в inlineFiles
   ├─ mods/plugins/loadorder согласованы
   └─ inlineFiles в пределах лимитов

9. WriteManifestStep     ← НЕ РЕАЛИЗОВАН
   └─ пишет modlist.json
```

**Результат:** `modlist.json` + рабочий каталог `__Firelink_Output/mods/`.

### Этап 1.4. Публикация

**Что публикуется:**

- `modlist.json` — манифест.
- Зеркала для архивов (опционально).
- Патч-архивы (если автор сделал их из `__Firelink_Output`).

**Результат:** сборка готова к распространению.

---

## Пайплайн: установка сборки

### Этап 2.1. Получение манифеста

**Что делает пользователь:**

- Скачивает `modlist.json`.
- Кладёт в `C:\Mods\NordicUI\`.

### Этап 2.2. Запуск installer-а

**Команда:**

```
firelink install C:\Mods\NordicUI\modlist.json
```

**Pipeline:**

```
1. ReadManifestStep
   └─ читает modlist.json, валидирует schemaVersion

2. ResolveTargetStep
   └─ определяет рабочую папку (папка modlist.json или --target)

3. ValidateTargetStep
   ├─ не корень диска
   ├─ не Program Files / ProgramData / Windows / Downloads
   └─ есть права на запись

4. BootstrapInstanceStep
   ├─ вычисляет instance path = working / normalize(meta.name)
   ├─ создаёт instance/
   ├─ создаёт instance/MO2/
   └─ создаёт instance/Stock Game/

5. BootstrapMo2Step
   ├─ если ModOrganizer.exe есть и версия совпадает → пропустить
   ├─ если есть, но версия не та → удалить MO2/ и поставить нужную
   ├─ скачать архив MO2 (sources по порядку)
   ├─ проверить хеш (вшит в код)
   ├─ распаковать
   └─ создать portable.txt

6. SyncArchivesStep
   ├─ для каждого archive в манифесте:
   │   ├─ если в instance/MO2/downloads/ есть файл с нужным хешем → пропустить
   │   ├─ иначе если в global registry есть запись с нужным хешем:
   │   │   ├─ проверить, что файл существует
   │   │   ├─ если да → скопировать в instance/MO2/downloads/
   │   │   └─ если нет → удалить запись, перейти к скачиванию
   │   └─ иначе → скачать по sources (по порядку):
   │       ├─ nexus: NexusSource → NexusApiStrategy → NexusFreeStrategy (заглушка)
   │       ├─ mirror: MirrorSource
   │       └─ github: GitHubSource
   │       ├─ 3 попытки на источник
   │       ├─ положить в instance/MO2/downloads/
   │       └─ записать в global registry
   └─ ничего не удаляем

7. ExecuteExtensionsStep
   ├─ для каждого extension в манифесте:
   │   └─ выполнить директивы (FromArchive → MO2/plugins/, tools/)
   └─ верифицировать по хешам

8. ExecuteExtrasStep
   ├─ для каждого extra в манифесте:
   │   └─ выполнить директивы (FromArchive → Stock Game/)
   └─ верифицировать по хешам

9. SyncModsStep (reconcile)
   ├─ для каждого mod в манифесте:
   │   ├─ если mods/<Name>/ содержит [NoDelete] → пропустить
   │   ├─ если mods/<Name>/ существует:
   │   │   ├─ проверить хеши файлов по директивам
   │   │   ├─ если все совпадают → пропустить
   │   │   └─ иначе → удалить папку и разложить заново
   │   ├─ если не существует → создать и разложить
   │   └─ если mods[].meta есть → сгенерировать meta.ini (см. п.10)
   └─ для каждой папки в mods/:
       ├─ если содержит [NoDelete] → пропустить
       └─ если не упомянута в манифесте → удалить

10. GenerateMetaIniStep (внутри SyncModsStep или отдельным шагом)
    ├─ для каждого mod с mods[].meta:
    │   ├─ сгенерировать [General] с полями из mods[].meta
    │   ├─ gameName/gameID — из mods[].meta (не из meta.game пакета!)
    │   ├─ секцию [installedFiles] НЕ писать
    │   ├─ поле newestVersion НЕ писать
    │   └─ положить в mods/<Name>/meta.ini
    └─ MO2 сам перестроит [installedFiles] при первом запуске

11. RegenerateProfileStep
    ├─ modlist.txt из манифеста (с разворотом порядка)
    ├─ plugins.txt из манифеста
    └─ loadorder.txt из манифеста

12. ConfigureMo2Step
    └─ сгенерировать ModOrganizer.ini с путём к Stock Game
```

**Результат:** готовый портативный инстанс MO2.

### Этап 2.3. Запуск игры

**Что делает пользователь:**

1. Копирует игру в `instance/Stock Game/` (вручную).
2. Открывает `instance/MO2/ModOrganizer.exe`.
3. Выбирает профиль.
4. Запускает игру через SKSE.

**Решения:**

- Firelink **не работает с игрой**.
- Firelink **не управляет** порядком загрузки.

---

## Пайплайн: обновление сборки

### Этап 3.1. Автор выпускает новую версию

**Что меняется:**

- Версия в `meta.version`.
- `modlist.json` пересобирается.
- Возможно, добавляются/удаляются моды, плагины, extras.
- Автор пересматривает `__Firelink_Output` — часть файлов могла стать
  unmatched снова (после обновления модов), часть патчей могла устареть.

**Решения:**

- Имя инстанса **не содержит версию** — обновление идёт «поверх».

### Этап 3.2. Пользователь обновляет

**Команда:**

```
firelink install C:\Mods\NordicUI\modlist.json
```

**Pipeline:**

- Installer видит, что инстанс уже есть.
- Верифицирует по новому манифесту.
- Докачивает новые архивы (локальная `downloads/` → глобальный реестр → скачать).
- Дописывает новые файлы.
- Пересоздаёт `meta.ini` для модов, у которых `mods[].meta` изменился.
- Удаляет моды, которых больше нет в манифесте.
- Перегенерирует профиль.

**Результат:** инстанс обновлён без перекачки всего.

---

## Работа с Nexus Mods

### Философия

Nexus — **один источник** (`type: "nexus"`). Способы доступа — **стратегии внутри `NexusSource`**. Не дублируем в манифесте.

**Манифест самодостаточен при чтении.** Installer не обращается к Nexus API, чтобы узнать версию или URL мода — всё это в `mods[].meta`. Nexus API нужен **только** для скачивания архивов.

### Стратегии доступа

```
NexusSource
  ├─ NexusApiStrategy      ← работает (Premium через API)
  └─ NexusFreeStrategy     ← заглушка (NotSupportedException)
```

**Порядок стратегий** — порядок регистрации в DI.

### Как работает

```csharp
public sealed class NexusSource : IArchiveSource
{
    private readonly INexusAccessStrategy[] _strategies;
    private readonly ILogger<NexusSource> _logger;

    public string Type => "nexus";

    public async Task<Stream> DownloadAsync(ArchiveSourceRef sourceRef, CancellationToken ct)
    {
        foreach (var strategy in _strategies)
        {
            try
            {
                return await strategy.DownloadAsync(sourceRef, ct);
            }
            catch (NotSupportedException)
            {
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Strategy {Strategy} failed", strategy.GetType().Name);
                continue;
            }
        }

        throw new InvalidOperationException(
            $"All Nexus access strategies failed for {sourceRef}");
    }
}
```

### `NexusApiStrategy`

- Читает API-ключ из конфига.
- Если ключа нет → бросает `NotSupportedException`.
- Если ключ есть → делает запрос к API.
- Если API вернул 401/403 → бросает исключение (логируется, стратегия пропускается).
- Если успех → возвращает `Stream`.

### `NexusFreeStrategy` (заглушка)

```csharp
public sealed class NexusFreeStrategy : INexusAccessStrategy
{
    public Task<Stream> DownloadAsync(ArchiveSourceRef sourceRef, CancellationToken ct)
    {
        throw new NotSupportedException(
            "Free Nexus downloads are not yet supported. " +
            "Please configure Nexus Premium or add a mirror source.");
    }
}
```

### Хранение API-ключа

**Расположение:** `%USERPROFILE%\.firelink\archives.db`, таблица `Config`.

**Шифрование:** Windows DPAPI (`DataProtectionScope.CurrentUser`).

**Схема:**

```sql
CREATE TABLE Config (
  key TEXT PRIMARY KEY,
  value BLOB NOT NULL,
  updated_at TEXT NOT NULL
);
```

**Команды:**

```
firelink config set nexus.apiKey <key>
firelink config get nexus.apiKey          # маскированный вывод
firelink config clear nexus.apiKey
firelink config list
```

### Приоритет источников в pipeline

```
для каждого archive:
  1. Локальная downloads/
  2. Глобальный реестр
  3. Sources по порядку:
     - nexus → NexusSource → стратегии по порядку
     - mirror → MirrorSource
     - github → GitHubSource
  4. Если ни один не сработал → ОШИБКА
```

### Что не делаем

- **Не используем `nxm://`.** Слишком много переменных.
- **Не используем WebView2.** Не CLI-опыт.
- **Не проверяем наличие Premium.** API сам скажет.

---

## Глобальный реестр архивов

### Назначение

Переиспользование архивов между сборками без повторной загрузки. Плюс — кеш **распакованных файлов** (хеши файлов внутри архивов) для ускорения `MatchStep`.

### Расположение

```
%USERPROFILE%\.firelink\archives.db
```

### Схема

```sql
CREATE TABLE CachedArchive (
  hash TEXT PRIMARY KEY,      -- xxHash64 архива
  path TEXT NOT NULL,         -- полный путь к файлу
  size INTEGER NOT NULL,
  last_seen TEXT NOT NULL     -- ISO 8601
);

CREATE TABLE CachedArchiveFile (
  archive_hash TEXT NOT NULL, -- ссылка на CachedArchive.hash
  file_hash TEXT NOT NULL,    -- xxHash64 файла внутри архива
  file_path TEXT NOT NULL,    -- путь внутри архива (относительный)
  size INTEGER NOT NULL,
  PRIMARY KEY (archive_hash, file_hash, file_path),
  FOREIGN KEY (archive_hash) REFERENCES CachedArchive(hash)
);

CREATE TABLE Config (
  key TEXT PRIMARY KEY,
  value BLOB NOT NULL,
  updated_at TEXT NOT NULL
);
```

**Индексы:**

```sql
CREATE INDEX idx_archive_file_hash ON CachedArchiveFile(file_hash);
```

Индекс по `file_hash` — критичен для `MatchStep`: поиск архива по хешу файла.

### Как работает

**При поиске архива:**

1. Проверить локальную `instance/MO2/downloads/`.
2. Если нет — проверить реестр по хешу.
3. Если файл найден — скопировать в локальную `downloads/`.
4. Если файла нет — удалить запись, скачать заново.

**При сканировании `downloads/`:**

- Просканировать папку, посчитать хеши.
- Добавить/обновить записи в `CachedArchive`.

**При распаковке архива:**

- Для каждого файла внутри посчитать хеш.
- Записать в `CachedArchiveFile`.

**При повреждении реестра:**

- Пересоздать из сканирования папок `downloads/` всех известных инстансов.

---

## CLI команды

### Firelink.Pack

| Команда | Описание |
|---|---|
| `firelink pack <config>` | Основной сценарий: создать манифест |
| `firelink index <instance>` | Пересобрать хеш-индекс |
| `firelink verify <manifest>` | Проверить манифест на согласованность |
| `firelink doctor` | Диагностика окружения (заглушка) |

### Firelink.Install

| Команда | Описание |
|---|---|
| `firelink install <manifest> [--target <dir>]` | Полная установка |
| `firelink verify <target>` | Проверить целостность установки |
| `firelink repair <manifest>` | Докачать/дописать битое |
| `firelink cache list` | Показать все архивы в реестре |
| `firelink cache prune` | Удалить битые записи |
| `firelink cache rebuild` | Пересоздать реестр |
| `firelink config set <key> <value>` | Установить значение конфига |
| `firelink config get <key>` | Прочитать значение (маскированное) |
| `firelink config clear <key>` | Удалить значение |
| `firelink config list` | Список ключей |
| `firelink doctor` | Проверка окружения (заглушка) |

---

## Обработка ошибок

### Матрица поведения packer-а

| Ситуация | Поведение |
|---|---|
| Мод в `modlist.txt` + папка есть | Включаем в манифест |
| Мод в `modlist.txt`, папки нет | **Ошибка**, packer останавливается |
| Мод не в `modlist.txt`, папка есть | **Игнорируем** |
| Папка мода содержит `[NoDelete]` | **Пропускаем** |
| Файл extension/extras не найден | **Предупреждение**, игнорируем |
| Файл мода **матчится по (hash, path)** | `FromArchive`, source == destination |
| Файл мода **матчится по hash** (путь ≠) | `FromArchive`, source ≠ destination |
| Файл мода **не матчится** | `UnmatchedFile` + выгрузка в `__Firelink_Output` |
| `meta.ini` в корне мода | `ModMeta` в `ModMetas`, не в unmatched |
| `meta.ini` в подпапке | Обычный файл (матчится или unmatched) |
| Архив с `.meta` (Nexus) | Используем `.meta` |
| Архив без `.meta`, есть в `archiveSources` | Используем `archiveSources` |
| Архив без `.meta` и без `archiveSources`, но используется модами | **unresolved** |
| Архив без `.meta` и без `archiveSources`, не используется | **Игнорируем** |
| Два архива с одинаковым каноническим id | **Ошибка** |
| `.meta` не Nexus-формата | Fallback в `archiveSources` |
| `meta.name` содержит запрещённые символы | **Ошибка** |
| `meta.version` не semver | **Ошибка** |
| Путь в `extensions`/`extras` абсолютный или с `..` | **Ошибка** |
| Суммарный размер `inlineFiles` > 20 МБ | **Ошибка** (если inlineFiles используются) |

### Матрица поведения installer-а

| Ситуация | Поведение |
|---|---|
| Инстанс не существует | Создать |
| Инстанс существует, но не наш | **Ошибка** |
| MO2 версия не совпадает | Удалить MO2/ и поставить нужную |
| Архив есть локально с нужным хешем | Пропустить |
| Архив есть в реестре | Скопировать |
| Архива нет | Скачать по sources (3 попытки на источник) |
| `NexusFreeStrategy` вызвана | `NotSupportedException` → пропустить стратегию |
| Все источники провалились | **Ошибка** |
| Мод содержит `[NoDelete]` | Пропустить |
| Мод изменился | Перезаписать |
| Мод исчез из манифеста | Удалить |
| Архив исчез из манифеста | **Не удалять** |
| `mods[].meta` есть | Сгенерировать `meta.ini` (без `[installedFiles]`) |
| `mods[].meta` нет | Не генерировать `meta.ini` |

---

## Технологический стек

| Компонент | Технология |
|---|---|
| Платформа | .NET 8 (`net8.0`) |
| CLI | Spectre.Console.Cli |
| JSON | System.Text.Json (source generators) |
| Хеширование | System.IO.Hashing (xxHash64) |
| База данных | Microsoft.Data.Sqlite |
| Шифрование | System.Security.Cryptography.ProtectedData (DPAPI) |
| Распаковка | 7z.exe + 7z.dll (GNU LGPL) |
| DI | Microsoft.Extensions.DependencyInjection |
| Логирование | Microsoft.Extensions.Logging |
| Retry | Polly |
| HTTP | Microsoft.Extensions.Http |
| GitHub API | Octokit |
| Тесты | xUnit + FluentAssertions |
| Целевая ОС | Windows 10 1809+ / Windows 11 |

**Примечание про таргеты:** все проекты таргетят `net8.0`. Windows-специфичный код (DPAPI) изолирован в `Firelink.Platform.Nexus` и включается условной компиляцией, если понадобится.

**Примечание про SharpCompress:** удалён из зависимостей. У него баг с `OpenEntryStream` на части 7z-архивов. Заменён на `7z.exe` (см. `SevenZipExtractor`).

---

## Дорожная карта

### MVP (v0.1.0)

- [x] Скелет solution
- [x] Модели Core (`ModlistManifest`, `Directive`, `ArchiveSourceRef`)
- [x] Сериализация JSON с полиморфизмом
- [x] Хеширование xxHash64
- [x] Чтение/запись `modlist.txt`, `plugins.txt`, `loadorder.txt`
- [x] Чтение `.meta` (Nexus-формат, архивный)
- [x] Модели `PackConfig` + валидаторы
- [x] Packer: `ReadConfigStep` + `ReadInstanceStep`
- [x] Packer: `IndexArchivesStep` + `ScanModsStep` (параллельно)
- [x] Packer: `MatchStep` с ленивой распаковкой
- [x] Packer: диагностика inline/unmatched файлов (блоки 10.6, 10.7)
- [x] Packer: `__Firelink_Output` — выгрузка unmatched-файлов
- [x] Packer: `MetaIniReader` + `ModMeta` (structured `meta.ini`)
- [x] Packer: `.mohidden` и расхождения имени при совпадении хеша
- [ ] Packer: `BuildManifestStep` + `ValidateManifestStep` + `WriteManifestStep`
- [ ] Installer: `BootstrapMo2Step` + `SyncArchivesStep`
- [ ] Installer: `SyncModsStep` + `RegenerateProfileStep`
- [ ] Installer: `GenerateMetaIniStep`
- [ ] Глобальный реестр архивов (SQLite)
- [ ] CLI: `pack`, `install`, `config`
- [ ] Примеры `firelink-pack.json` (minimal, full, invalid-*)

### v0.2.0

- [ ] Nexus API (Premium)
- [ ] GitHub downloader
- [ ] Команда `verify`
- [ ] Команда `repair`
- [ ] Команда `cache` (list, prune, rebuild)
- [ ] Генерация `ModOrganizer.ini`
- [ ] Параллельные загрузки
- [ ] Прогресс-бары
- [ ] Persist кеша распакованных файлов в SQLite
- [ ] Механизм патчей для `__Firelink_Output` (генерация патч-архивов)

### v0.3.0

- [ ] `NexusFreeStrategy` (реальная реализация)
- [ ] Команда `doctor`
- [ ] Ротация логов
- [ ] Поддержка нескольких игр (одновременно)

### v1.0.0

- [ ] GUI
- [ ] Hardlink-режим для кеша
- [ ] Документация для авторов сборок

---

## Статус реализации

**Обновлено:** 2026-09-16
**Версия документа:** 3.2

### Готово

- Скелет solution + 9 проектов + Central Package Management.
- `Firelink.Core`:
  - модели манифеста, JSON-сериализация, `XxHash64Value`, `IStep`;
  - `Pack*`-модели;
  - `InstanceSnapshot` (с `FirelinkOutputPath`), `ModScanResult`,
    `ScannedFile`, `MatchResult` (новый: `ModDirectives` + `Unmatched`
    + `ModMetas`), `UnmatchedFile`, `ModMeta`;
  - `InlineFileContent` / `OrphanFile` — типы есть, но не используются;
  - `Slug`, `ArchiveId`, валидаторы;
  - `ArchiveExtensions`, `FileHashCache`, `IArchiveExtractor`,
    `SevenZipExtractor`, `TempWorkspace`.
- `Firelink.Platform.MO2`:
  - чтение/запись `modlist.txt`, `plugins.txt`, `loadorder.txt`;
  - `MetaReader` (архивный `.meta`);
  - `MetaIniReader` (модовый `meta.ini`, полная секция `[General]`).
- `Firelink.Pack`:
  - `ReadConfigStep`, `ReadInstanceStep`, `IndexArchivesStep`,
    `ScanModsStep`, **`MatchStep`** (переписан в 10.7);
  - `PackPipeline`;
  - CLI (`pack`, `hash`, `doctor`).
- `Firelink.Install`: CLI-заглушки (`install`, `verify`, `doctor`).
- **288 тестов, все проходят.**

### В работе

Ничего. Блок 10.7 закрыт. Следующий — блок 11.

### Следующие блоки

1. Блок 11: `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.
2. Блок 12: installer.

### Ключевые решения, принятые в процессе

См. `HANDOFF.md`, раздел «Ключевые архитектурные решения».

### Что изменилось в 10.6–10.7

- **10.6:** диагностика inline-файлов через второй индекс `path → { hash → archiveId }`.
  Обнаружено: 46 inline = 33 `meta.ini` + 11 runtime-логов/авторских `.ini`.
- **10.7:** три ключевых изменения:
  1. **`meta.ini` — structured.** `MetaIniReader` читает полную секцию
     `[General]` в `ModMeta`. Идёт в `ModMetas`, попадает в `mods[].meta`
     манифеста. Installer генерирует `meta.ini` из этого.
  2. **Unmatched-файлы — в `__Firelink_Output`.** Не base64, не лимит
     2 МБ. Просто копия с сохранением структуры в корне инстанса.
     Автор вычищает мусор, оставляет правки, делает патч-архив.
  3. **Матчинг по `(hash, path)` → fallback по `hash`.** Детерминированный
     выбор по `(archiveId, relativePath)`. `source` и `destination` могут
     отличаться (`.mohidden`, FOMOD, разные корни).

  Результат на `OmenRim 7`: 142 matched (exact), 943 matched (by hash),
  6 unmatched, 40 `meta.ini`. Из 6 unmatched: 2 runtime-лога (hash-differs),
  4 авторских `.ini` (path-not-found).
