# Handoff — как продолжить проект Firelink в новом чате

**Обновлено:** 2026-09-16
**Последний закрытый блок:** 10.7 (диагностика inline, `__Firelink_Output`, structured meta.ini)
**Текущая задача:** ждём старта блока 11
**Всего тестов:** 288, 0 failed

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
3. **DOC.md** (v3.2) — полностью, если помещается.
   Если не помещается — только секции 1–7 (Обзор, Принципы, Глоссарий,
   Архитектура, Структура, Форматы, Идентификация).
4. **repo-dump.md** — если есть свежий; иначе — последний блок кода.

**Первое сообщение — шаблон:**
Продолжаем проект Firelink. Стиль — пошаговые блоки кода с тестами.

Прикладываю:

HANDOFF.md

PROJECT-STATE.md

DOC.md (v3.2)

Текущее состояние: 288 тестов, 0 failed. Блок 10.7 закрыт.
Следующая задача: блок 11 — BuildManifestStep, ValidateManifestStep, WriteManifestStep.

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
- **HANDOFF.md и PROJECT-STATE.md** — в конце каждого сообщения ассистента.
- **Не менять архитектурные решения без обсуждения.**
- **Не отвечать на китайском** (инцидент был, повторять не надо).
- **Разбивать крупные блоки на подшаги 10.x.y**, чтобы после каждого можно
  было прогнать `dotnet test`. Красный билд между подшагами — нормально,
  но лучше минимизировать.

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
__Firelink_Output/ ← новое (см. п.35)

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
28. **`MatchStep` (обновлён в 10.7):**
- Распаковка архивов по одному через `TempWorkspace`.
- Три индекса: `(hash, path) → IndexEntry`, `hash → List<IndexEntry>`,
  `path → { hash → archiveId }` (последний — для диагностики).
- Матчинг: сначала точное `(hash, path)`, потом fallback по `hash`
  (детерминированно по `(archiveId, relativePath)`).
- `source` (путь в архиве) и `destination` (путь в моде) **могут
  отличаться** — это норма для `.mohidden` и любых расхождений имени.
- `meta.ini` в корне мода — отдельная сущность, читается через
  `MetaIniReader`, идёт в `ModMetas`. Не в `Unmatched`, не в
  `__Firelink_Output`, не в директивы.
- Всё ненайденное → `UnmatchedFile` + выгрузка в
  `__Firelink_Output/mods/<ModName>/<path>`. **Без base64, без лимита 2 МБ.**
- `__Firelink_Output/mods` очищается перед каждым прогоном.
- Диагностика: `path-not-found` / `hash-differs`, топ-20, по модам.
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
35. **`__Firelink_Output`** — рабочий каталог автора в корне инстанса:
- структура `mods/<ModName>/<relative/path>`;
- только unmatched-файлы (не `meta.ini`, не найденное);
- очищается перед каждым прогоном;
- `ScanModsStep` его не видит (лежит вне `mods/`);
- автор вычищает мусор, оставляет нужное, упаковывает в патч-архив.
36. **`MatchResult` (после 10.7):**
- `ModDirectives` — как было;
- `Unmatched` — список `UnmatchedFile` (в манифест не идёт, только
  диагностика + наполнение `__Firelink_Output`);
- `ModMetas` — dictionary `modName → ModMeta` (идёт в манифест как
  `mods[].meta`, см. блок 11).
- `InlineFiles` / `Orphans` **удалены** из контракта. Типы
  `InlineFileContent` / `OrphanFile` пока лежат в `Firelink.Core.Models.Pack`
  без использования — могут пригодиться в блоке 11.
37. **`MetaIniReader`** (новый, `Firelink.Platform.MO2.Readers`) — читает
`[General]` из `mods/<Name>/meta.ini` полностью. Отдельный от
`MetaReader` (который читает `downloads/*.meta` для идентификации архива).
Секция `[installedFiles]` игнорируется. Ключи case-sensitive,
имя секции — case-insensitive.
38. **`ModMeta`** (`Firelink.Core.Models.Pack`) — structured `[General]`:
`GameName`, `GameId`, `ModId`, `FileId`, `Version`, `Category`,
`Repository`, `Url`, `Comments`, `Notes`. Все поля nullable.
`newestVersion` не читается.
39. **`.mohidden` — часть пути, а не отдельная сущность.** Matcher работает
с путями как со строками, никаких специальных правил для `.mohidden` нет.
Покрыто тестами (файл и папка).
40. **При работе с `meta.ini` в манифесте (блок 11):** `mods[].meta` пишется
**со всеми полями, включая `gameName`/`gameId`/`repository`/`url`** —
манифест должен быть самодостаточным, без опоры на Nexus API.

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
Indexed: 68 resolved, 0 unresolved
ScanModsStep: 40 mods, 1131 files total
Archive index built: 4803 unique hashes, 6524 unique paths
Match complete: 1131 files, 142 matched (exact), 943 matched (by hash), 6 unmatched, 40 meta.ini
Unmatched files written to __Firelink_Output: 6
by reason: path-not-found = 4, hash-differs = 2

text

**Все 288 тестов проходят.**

---

## Что в работе

Ничего. Блок 10.7 закрыт. Следующий — блок 11.

**Блок 11:** `BuildManifestStep`, `ValidateManifestStep`, `WriteManifestStep`.

**Вопросы перед Блоком 11 (ждут подтверждения):**
1. `order` модов = индекс в `modlist.txt` (0, 1, 2...). Развернём при генерации
   профиля installer-ом.
2. Все плагины сохраняем, включая `disabled`.
3. `mo2.archive` = заглушка (`id` есть, `hash=0`, `size=0`, `sources=[]`).
   Installer скачает по `mo2.source`.
4. `extensions = []`, `extras = []` — пока пусто.
5. `mods[].meta` пишется **со всеми полями `ModMeta`**, включая
   `gameName`/`gameId`/`repository`/`url` — манифест самодостаточный.
6. `inlineFiles` в манифесте — пока **не заполняем**. Автор использует
   `__Firelink_Output` и патчи.

---

## Технический долг

- Persist кеша хешей в SQLite (v0.2.0).
- Глобальный реестр `archives.db` — после Блока 11.
- Nexus API — v0.2.0.
- Прогресс-бар Spectre — после Блока 11.
- `Firelink.Platform.Nexus` и `Firelink.Platform.GitHub` пусты.
- `PackCommand` показывает устаревшую колонку `Inline` (всегда 0) —
  подчистить в блоке 11.
- `InlineFileContent` / `OrphanFile` — не используются. Решить судьбу
  в блоке 11 (inlineFiles-секция или удаление).
- `943 matched by hash` — большой процент. Стоит когда-нибудь понять,
  откуда расхождение путей. Сейчас работает корректно (хеш совпал →
  содержимое идентично).
- Механизм патчей для inline-файлов — v0.2.0 (использует `__Firelink_Output`).
- `extensions`/`extras` — `ScanExtensionsStep`/`ScanExtrasStep` не написаны.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже установлен, проекты таргетят `net8.0`).
- Visual Studio 2022.
- Тестовый инстанс `C:\Firelink\OmenRim 7\`.
- Большой инстанс — 4370 модов.
