# Handoff — как продолжить проект Firelink в новом чате

**Обновлено:** 2026-09-15
**Последний закрытый блок:** 10.5 (первый успешный прогон `pack` с 7z.exe)
**Текущая задача:** диагностика inline-файлов
**Всего тестов:** 263, 0 failed

---

## Что это

Firelink — C#/.NET 8 проект для создания и установки воспроизводимых сборок модов
для Mod Organizer 2. Два CLI-приложения: `Firelink.Pack` (автор) и
`Firelink.Install` (пользователь). Манифест `modlist.json` — единственный
источник правды. Файлы восстанавливаются по хешам `xxHash64`.

Разработка в чате с ИИ-ассистентом. Переписка может упереться в лимит длины.
Этот файл описывает, как продолжить в новом чате.

---

## Как начать работу в новом чате

**Скопируйте в первое сообщение** (как отдельные блоки кода или приложения):

1. **HANDOFF.md** (этот файл) — полностью.
2. **PROJECT-STATE.md** — полностью.
3. **Firelink — Документация проекта.txt** (v3.1) — полностью, если помещается.
   Если не помещается — только секции 1–7 (Обзор, Принципы, Глоссарий,
   Архитектура, Структура, Форматы, Идентификация).
4. **Последний блок кода**, если он ещё не применён (см. раздел
   «Что в работе» ниже).

**Первое сообщение — шаблон:**
Продолжаем проект Firelink. Стиль — пошаговые блоки кода с тестами.

Прикладываю:

HANDOFF.md

PROJECT-STATE.md

Firelink — Документация проекта.txt (v3.1)

Текущее состояние: 263 теста, 0 failed. Блок 10.5 закрыт.
Текущая задача: диагностика inline-файлов (46 из 1131 при прогоне на OmenRim 7).

Стиль ответов:

Разбор задачи — что делаем и почему.

Полный код файлов с путями (замена целиком, не патч).

Инструкция по сборке/тестам.

Ожидаемый вывод dotnet test.

HANDOFF.md и PROJECT-STATE.md в конце сообщения.

Не пиши код, пока я не подтвержу готовность.

text

---

## Стиль работы

- **Файлы давать целиком**, не патчами, если правок больше двух в одном файле.
- **Запускать `dotnet test` сразу** после каждого блока.
- **Присылать полный вывод** тестов при падении (текст, не скриншот).
- **HANDOFF.md и PROJECT-STATE.md** — в конце каждого сообщения ассистента
  (ассистент сам их включает, если попросить).
- **Не менять архитектурные решения без обсуждения.**
- **Не отвечать на китайском** (инцидент был, повторять не надо).

---

## Стек

- **.NET 8**, C# 12.
- **xUnit + FluentAssertions** — тесты.
- **Spectre.Console.Cli** — CLI.
- **System.Text.Json** — сериализация.
- **System.IO.Hashing (xxHash64)** — хеши.
- **Microsoft.Data.Sqlite** — БД (пока не используется).
- **Microsoft.Extensions.*** — DI, Logging.
- **7z.exe + 7z.dll** — распаковка архивов (встроены в проект).
- **SharpCompress удалён** — у него баг с `OpenEntryStream` на части 7z.

---

## Ключевые архитектурные решения (не переделывать)

1. **`meta.game` = Nexus game domain** (например, `skyrimspecialedition`).
2. **`instance.path`** — обязательный, относительный путь к корню инстанса.
   Разрешён `"."`.
3. **`mo2.profile`** — обязательный, имя профиля MO2.
4. **`mo2.extensions`** — относительно `MO2/`.
   **`stockGame.extras`** — относительно `Stock Game/`.
5. **`.meta`** — Nexus-формат (`gameName`, `modID`, `fileID`, case-sensitive).
   Читается через `MetaReader.TryRead`.
6. **Канонический id архива:**
   - `nexus_{game}_{modId}_{fileId}` — для Nexus.
   - `local_{slug}` — для архивов без `.meta` (указаны в `archiveSources`).
   - `github_{owner}_{repo}_{tag}_{asset}` — для GitHub-релизов.
7. **`archiveSources`** вместо `mirrors`.
8. **Slug** — ASCII-only, без транслитерации. Кириллица выбрасывается.
9. **Semver** — регулярка (официальный паттерн semver.org).
10. **`Pack*`-модели** — `record`.
11. **`ValidationResult`** — накапливает все ошибки, не падает на первой.
12. **Trailing slash** разрешён в `RelativePathValidator`
    (`tools/BethINI/` — валидно). Двойной `//` — ошибка.
13. **Зарезервированные имена Windows** запрещены (`CON`, `PRN`, ..., `LPT9`).
14. **Модели MO2** (`ModlistEntry`, `PluginsFile` и т.д.) — в `Core.Models.Mo2`.
    `MetaFile` — в `Platform.MO2.Models`.
15. **CLI** — Spectre.Console.Cli + адаптер `TypeRegistrar` для DI.
16. **CLI требует явной команды:** `firelink-pack pack <config>`.
17. **Структура инстанса:**
{instance.path}/
MO2/
ModOrganizer.exe
downloads/
mods/
profiles/
plugins/
tools/
Stock Game/

text
18. **Путь с пробелами** — в PowerShell в кавычках.
19. **Профиль MO2** — обычно `Default`.
20. **Два инстанса:**
- `C:\Firelink\OmenRim 7\` — тестовый (82 мода, 40 включённых).
- Большой — 4370 модов, `downloads/` ≈ 200–300 ГБ.
21. **`ModlistReader`** читает **все** строки. Фильтрация `Enabled == true` —
в `ScanModsStep`.
22. **`[NoDelete]`** — через `Contains` (регистронезависимо).
23. **`.meta` не считается архивом** — явная проверка в `ArchiveExtensions`.
24. **Невалидный `.meta`** не бросает исключение — fallback в `archiveSources`
→ `UnresolvedArchive`.
25. **`MirrorSourceRef.Hash` обязателен** (требование безопасности).
26. **`XxHash64Value`** — строгий формат: `xxh64:hex`, **16 символов**,
**без пробелов**. Пробелы падают с `FormatException`.
27. **CLI:** `pack`, `hash`, `doctor`.
28. **`MatchStep`:**
- Распаковка архивов по одному через `TempWorkspace`.
- Удаление temp после каждого.
- Индекс `hash → (archiveId, relativePath, size)`, first-wins при дубликатах.
- Файл найден → `FromArchiveDirective`.
- Не найден и ≤ 2 МБ → `InlineFileDirective`.
- Не найден и > 2 МБ → `OrphanFile`.
29. **Форматы в `OmenRim 7`:** 36 `.zip`, 31 `.7z`, 1 `.rar`.
Все через 7z.exe.
30. **`7z.exe` + `7z.dll`** в `src/Firelink.Core/Assets/7z/`.
Лицензия — GNU LGPL, redistribution разрешён.
**Копирование в output** — через `Directory.Build.targets` в корне
(не через `<Content>` в Core.csproj — транзитивно не работает).
31. **`HashCommand` и `HashSettings`** — в `Commands/HashCommand.cs`.
32. **`IArchiveExtractor`** — интерфейс без изменений. Реализация —
`SevenZipExtractor`.
33. **`SevenZipExtractor`** использует `Process.Start` с:
- `UseShellExecute = false`
- `CreateNoWindow = true`
- `RedirectStandardOutput/Error = true`
- Таймаут 10 минут
- `Kill(entireProcessTree: true)`
- Рабочий каталог = папка с `7z.exe` (иначе не найдёт `7z.dll`).
34. **`MatchStepTests.cs`** использует `SevenZipExtractor`.

---

## Что сделано (кратко)

См. PROJECT-STATE.md — там полный список.

**Пайплайн packer-а:**
ReadConfigStep → ReadInstanceStep → IndexArchivesStep → ScanModsStep → MatchStep

text
Все пять шагов работают. Пайплайн **не завершён** — нет `BuildManifestStep`.

**Результат последнего прогона `pack` на `OmenRim 7`:**
Config OK: OmenRim 7 v0.1.0
Instance snapshot: 82 mods, 6 plugins, 86 load order entries
Indexing 68 archives
Indexed: 68 resolved, 0 unresolved
ScanModsStep: 40 mods, 1131 files total
Extracting archive 68/68 (16 секунд, 7z.exe)
Archive index built: 4803 unique hashes
Match complete: 1131 files, 1085 matched, 46 inline, 0 orphans

text

**Все 263 теста проходят.**

---

## Что в работе

**Диагностика inline-файлов.**

46 файлов из 1131 не нашли источник в архивах. Причины могут быть:

1. **Пользовательские правки** — автор редактировал файл в `mods/` после установки.
2. **FOMOD-выборы** — но обычно файлы есть в архиве, просто по другому пути.
3. **Файлы, сгенерированные модами** — `config.json`, кеши, логи, патчи.
4. **Файлы без источника** — установлены вручную, архива нет в `downloads/`.
5. **Merged-моды** — объединённые или модифицированные сборки.

**План:** добавить в `MatchStep` **диагностику**:
- Второй индекс `path → (archiveId, hash)` — для ответа «есть ли файл с таким
  путём в архиве?».
- Логирование inline-файлов, сгруппированных по модам.
- Для первых 20 inline-файлов — причина: «path not found» или «hash differs».

**Решение о судьбе inline-файлов** (в manifest через base64 или механизм
патчей) — **после диагностики**.

**Пока код диагностики не написан.**

---

## Следующие блоки

1. **Диагностика inline-файлов** (текущая задача).
2. **Блок 11:** `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.
3. **Блок 12:** installer.

**Вопросы перед Блоком 11 (ждут подтверждения):**
1. `order` модов = индекс в `modlist.txt` (0, 1, 2...). Развернём при генерации
   профиля installer-ом.
2. Все плагины сохраняем, включая `disabled`.
3. `mo2.archive` = заглушка (`id` есть, `hash=0`, `size=0`, `sources=[]`).
   Installer скачает по `mo2.source`.
4. `extensions = []`, `extras = []` — пока пусто.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — после Блока 11.
- Nexus API — v0.2.0.
- Прогресс-бар Spectre — после Блока 11.
- `Firelink.Platform.Nexus` и `Firelink.Platform.GitHub` пусты.
- Диагностика inline-файлов — текущая задача.
- Механизм патчей для inline-файлов — v0.2.0.
- `extensions`/`extras` — `ScanExtensionsStep`/`ScanExtrasStep` не написаны.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже установлен, проекты таргетят `net8.0`).
- Visual Studio 2022.
- Тестовый инстанс `C:\Firelink\OmenRim 7\`.
- Большой инстанс — 4370 модов.