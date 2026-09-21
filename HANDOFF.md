# Handoff — как продолжить проект Firelink в новом чате

**Обновлено:** 2026-09-21
**Последний закрытый блок:** 12.13.10 — error-msg для пробелов без кавычек
**Следующий блок:** 12.8 — NexusDownloader
**Всего тестов:** 593, 0 failed

---

## Что это

Firelink — C#/.NET 8 проект для создания и установки воспроизводимых
сборок модов для Mod Organizer 2. Два CLI-приложения: `Firelink.Pack`
(автор) и `Firelink.Install` (пользователь). Манифест `modlist.json` —
единственный источник правды. Файлы восстанавливаются по хешам `xxHash64`.

---

## Как начать работу в новом чате

**Скопируйте в первое сообщение:**

1. **HANDOFF.md** (этот файл) — полностью.
2. **PROJECT-STATE.md** — полностью.
3. **DOC.md** (v3.9) — полностью.
4. **repo-dump.md** — свежий.

**Первое сообщение — шаблон:**

Продолжаем проект Firelink. Стиль — пошаговые блоки кода с тестами.

Прикладываю: HANDOFF.md, PROJECT-STATE.md, DOC.md (v3.9), repo-dump.md (свежий).

Текущее состояние: 593 теста, 0 failed. Закрыты блоки 10.7, 11,
12.1–12.7, 12.10.1–12.10.3, 12.6.1, Packer fix, Meta.ini fix, GitHub cleanup,
12.12, 12.11.1–12.11.7 (Verify + Ctrl+C), 12.13.1–12.13.10
(extensions/extras + таблицы CLI + error-msg), 13.1 (общий helper скачивания).

MVP работает end-to-end. Прогон на C:\Firelink\TestInstance2\ успешен.
Verify 4338 passed на OmenRim 7, 0 failed. Остался только 12.8 (Nexus).

Следующая задача: 12.8 — NexusDownloader.

Стиль ответов:
- Разбор задачи.
- Полный код файлов с путями.
- Инструкция по сборке/тестам.
- Ожидаемый вывод dotnet test.
- HANDOFF.md и PROJECT-STATE.md — только по запросу.

Не пиши код, пока я не подтвержу готовность.

---

## Стиль работы

- **Файлы давать целиком**, не патчами.
- **Запускать `dotnet test` сразу** после каждого блока.
- **Присылать полный вывод** тестов при падении (текст).
- **HANDOFF.md и PROJECT-STATE.md** — только по запросу.
- **Не менять архитектурные решения без обсуждения.**
- **Не отвечать на китайском.**
- **Разбивать крупные блоки на 12.x.y.**
- **Не писать код, пока не подтверждена готовность.**

### Замечания (накопленные)

- **Про копипаст:** были ошибки (`Pack.Steps` vs `Install.Steps`,
  пропущенные `Profile`, `params` vs named args, shadowing в тестах,
  `PluginsEntry` вместо `PluginEntry`).
  Если билд падает — вероятнее ошибка в коде ассистента.
- **Про BOM:** файлы в репозитории часто с BOM (`\uFEFF`).
  При перезаписи файлов не копировать BOM из вывода `dump`.
  `firelink-pack.json` у автора тоже бывает с BOM — `PackConfigJson`
  читает его корректно, но лучше без.
- **Про samples:** `samples/*.json` копируются в output
  тестов через `PreserveNewest`. Если правите sample — обновите и
  исходник, и (при необходимости) очистите `bin/obj`.
- **Про пути:** все проекты живут в `src/` и `tests/`.
  Новые проекты создавать **строго** в `tests/<Name>/`, иначе
  `..\..\src\...` в ProjectReference не разрешится.
- **Про пустой DisabledMod:** мод без восстановимых файлов
  остаётся в манифесте с пустыми директивами. Installer создаёт
  папку, файлов нет. Это норма.
- **Про пустой extension/extra:** entry с пустым списком директив
  **не попадает** в манифест (решение №114). Unmatched уже
  выгружены в `__Firelink_Output`; entry без директив бессмысленна.
- **Про xUnit1031:** не использовать `.GetAwaiter().GetResult()`
  в тестах. Если API async — тест `async Task`, `await`.
- **Про Spectre markup:** `[...]` — это разметка.
  Для имён файлов и мод-неймов (особенно `[NoDelete]`) — обязательно
  `Markup.Escape`.
- **Про verify-счётчики:** при отсутствии папки мода
  `CheckMod` делает `yield break` — одна fail-проверка вместо
  10+ «file missing». Сознательное решение.
- **Про `Slug.FromFileName`:** `.7z` отрезается **до**
  slug-ификации. `FomodTools.7z` → `fomodtools`,
  `Mod.Organizer-2.5.2.7z` → `mod-organizer-2-5-2`.
- **Про `MatchStep.Input.Matcher`:** `internal`, не `required`.
  При создании `MatchStep` из тестов (не через DI) — **обязательно**
  задавать. `MatchExtensionsStep`/`MatchExtrasStep` — аналогично.
- **Про `Mod.Organizer-2.5.2.7z`:** если файла нет
  в `OmenRim 7\MO2\downloads\`, pack пишет `Size = 0` для
  `manifest.Mo2.Archive`. **Не ошибка.** Hash берётся из `mo2.source.hash`.
- **Про runtime-файлы:** `.log`/`.ini` от SKSE-плагинов
  не восстанавливаются из архивов и уходят в `__Firelink_Output`.
  **Не пытаться «чинить»** — это правильное поведение.
- **Про `.bsa`/`.ba2`:** единые файлы, не контейнеры для Firelink.
  Отдельных `.bsa` в `downloads/` не бывает на практике.
- **Про `meta.ini` в verify:** сравнение **семантическое** (парсим
  через `MetaIniReader.Parse`, сравниваем `ModMeta` по полям).
  Нормализация: `null ≡ ""`. `mod.Meta == null` + файл есть → fail.
- **Про `ArchiveMatcher.BuildAsync`:** async, отмена пробрасывается
  как есть (`catch (OperationCanceledException) { throw; }` **перед**
  `catch (Exception)`). В тестах хелпер называется `MakeMatcherAsync`,
  `await matcher.BuildAsync(ct)`.
- **Про `CancellationHelper`:** в `Firelink.Core`. Используется
  в `PackCommand`, `InstallCommand`, `VerifyCommand` и в обоих
  `Program.cs` — `catch (Exception ex) when (CancellationHelper.IsCancellation(ex))`.
- **Про `config.PropagateExceptions()`:** включено в обоих CLI,
  чтобы `CommandParseException` долетел до нашего `catch`. Без этого
  Spectre ловит его сам и печатает свой формат без hint.

---

## Стек

- **.NET 8**, C# 12.
- **xUnit + FluentAssertions**.
- **Spectre.Console.Cli** 0.48.0 (с `PropagateExceptions`).
- **System.Text.Json**.
- **System.IO.Hashing (xxHash64)**.
- **Microsoft.Data.Sqlite** (v0.2.0).
- **Microsoft.Extensions.*** — DI, Logging, Http.
- **Polly** (в `Firelink.Core` — блок 5).
- **7z.exe + 7z.dll**.
- **SharpCompress удалён.**
- **Octokit удалён.**

---

## Ключевые архитектурные решения (не переделывать)

### Packer

1. **`meta.game` = Nexus game domain.**
2. **`instance.path`** — относительный.
3. **`mo2.profile`** — обязательный.
4. **`mo2.source` — обязательно `MirrorSourceRef`.**
5. **`mo2.archive.size/hash` — из `mo2.source.hash`** (size с диска,
   если файл есть).
6. **MO2-архив НЕ попадает в `manifest.Archives[]`.**
7. **`mo2.extensions`** — от `MO2/`. **`stockGame.extras`** — от `Stock Game/`.
8. **`.meta`** — Nexus-формат. `MetaReader.TryRead`.
9. **Канонический id:** `nexus_...` / `local_{slug}`.
10. **`archiveSources`** вместо `mirrors`.
11. **Slug** — ASCII-only.
12. **Semver** — регулярка semver.org.
13. **`Pack*`-модели** — `record`.
14. **`ValidationResult`** — накапливает ошибки.
15. **Trailing slash** разрешён.
16. **Зарезервированные имена Windows** запрещены.
17. **Модели MO2** — в `Core.Models.Mo2`.
18. **CLI** — Spectre.Console.Cli + `TypeRegistrar` + `PropagateExceptions`.
19. **CLI требует явной команды.**
20. **Структура инстанса:** MO2/, Stock Game/, `__Firelink_Output/`.
21. **`ModlistReader`** читает все строки.
22. **`[NoDelete]`** — через `Contains`.
23. **`.meta` не считается архивом.**
24. **Невалидный `.meta`** → fallback.
25. **`MirrorSourceRef.Hash` обязателен.**
26. **`XxHash64Value`** — `xxh64:hex`, 16 символов.
27. **`MatchStep`** делегирует матчинг `ArchiveMatcher`-у.
28. **`meta.ini` в корне мода** → `ModMetas`.
29. **Unmatched модов** → `__Firelink_Output/MO2/mods/<ModName>/<path>`.
30. **`7z.exe` + `7z.dll`** через `Directory.Build.targets`.
31. **`SevenZipExtractor`** — таймаут 10 минут.
32. **`__Firelink_Output`** — каталог автора.
33. **`MatchResult`** — `ModDirectives` + `Unmatched` + `ModMetas`.
34. **`MetaIniReader`** — `[General]`. Ключи case-insensitive.
35. **`MetaIniWriter`** — `[General]`, camelCase, без `[installedFiles]`,
    без BOM, CRLF. Не пишет `category`.
36. **`ModMeta` — без `Category`.**
37. **`.mohidden` — часть пути.**
38. **`mods[].meta` со всеми полями** (кроме `Category`).
39. **`createdAt`** — `yyyy-MM-ddTHH:mm:ss.fffZ`.
40. **`InlineFileContent` / `OrphanFile` / `InlineFile` удалены.**
41. **`PackCommand`:** `Manifest archives`, `Manifest extensions`,
    `Manifest extras`, `mo2.archive`, `Unmatched written to` при `> 0`.
42. **Сепараторы** сохраняются в манифесте.
43. **`ScanModsStep`:** сепаратор без папки → `LogDebug`.
44. **Мод без восстановимых файлов остаётся в манифесте с пустыми
    директивами.**
45. **`ArchiveMatcher`** — в `Firelink.Pack.Matching`. Детерминизм
    при дубликатах: минимальный `archiveId` (Ordinal).
    `InternalsVisibleTo("Firelink.Pack.Tests")`. Принимает
    `ILogger<ArchiveMatcher>`. Метод — `BuildAsync(ct)` (12.11.7.1).
46. **`ScanExtensionsStep`/`ScanExtrasStep`** — тип результата
    `EntryScanResult`. `RelativePath` файла — ОТ КОРНЯ MO2/ или
    Stock Game/. `Parallel.ForEach` + восстановление порядка ключей.
    Отсутствие entry → `FileNotFoundException`.
47. **`MatchExtensionsStep`/`MatchExtrasStep`** — тип результата
    `MatchEntriesResult`. Принимают `ArchiveMatcher` через
    `Input.Matcher` (`internal`, не `required`). Unmatched **не пишут**
    на диск — это делает `PackPipeline`.
48. **`Mo2ArchiveBuilder`** — статический класс в `Firelink.Pack.Matching`.
    Один источник правды для построения `ArchiveEntry` MO2-архива.
49. **Unmatched extensions** → `__Firelink_Output/MO2/<relativePath>`
    (плоско). **Unmatched extras** → `__Firelink_Output/Stock Game/<relativePath>`.
    `EntryName` на путь не влияет — только `RelativePath`.
50. **`PackPipeline`** перед write unmatched чистит
    `__Firelink_Output/MO2/` (кроме `mods/`) и
    `__Firelink_Output/Stock Game/`. Папки `MO2/` (без `mods/`) и
    `Stock Game/` создаются **только** при `entries.Count > 0`.
51. **`PackPipeline`** создаёт `ArchiveMatcher` один раз, `BuildAsync`,
    передаёт в `MatchStep`, `MatchExtensions`, `MatchExtras` (12.13.6).

### Installer

52. **Инстансы:** `<exeDir>/Instances/<normalize(meta.name)>/`.
53. **Копирование манифеста** — `ResolveTargetStep`.
54. **`--target <dir>`** — escape-hatch.
55. **`meta.name` перепроверяется.**
56. **`ValidateTargetStep`** — 4 проверки.
57. **`[NoDelete]`** — уважаем.
58. **Существующая папка** — рефлорация.
59. **`verify` — да, `repair` — нет.**
60. **File-logging — v0.3.0.**
61. **Прогресс-бары — v0.2.0.**
62. **`BootstrapInstanceStep`** — создаёт все папки.
63. **`IArchiveDownloader`** — абстракция.
64. **`DownloaderRegistry`** — map sourceType → downloader.
65. **`SyncArchivesStep`** — только `manifest.Archives`, не трогает MO2.
66. **Hash — источник правды.**
67. **Ничего не удаляем** из `downloads/`.
68. **GitHub — удалён.** Всё через `mirror`.
69. **Параллельная загрузка** — `ParallelOptions`.
70. **3 попытки + Polly backoff (2 сек).**
71. **Скачивание в `.part`**, `File.Move` после проверки.
72. **Проверка хеша после скачивания обязательна.**
73. **`nexus` до 12.8 — warning + `Skipped`.**
74. **Глобальный реестр — v0.2.0.**
75. **`IHttpClientFactory` через DI.**
76. **`SyncModsStep`** — reconcile `mods/`.
77. **Только `FromArchive` директивы.**
78. **`TempWorkspace` на мод.**
79. **Файлы, которых нет в директивах, — удаляются** (recreate).
80. **Откат при ошибке — никакого.**
81. **`SyncModsStep.Input.ArchivesById`.**
82. **`SyncModsStep.Output`:** `Created`/`Recreated`/`Skipped`/`Deleted`.
83. **Сепараторы в `SyncModsStep`:** pass 1 — `Skipped`; pass 2 —
    не удаляются.
84. **`GenerateMetaIniStep`:** reconcile `meta.ini`.
85. **`RegenerateProfileStep`:** сортировка по `Order` ascending,
    **без `Reverse()`**.
86. **`BootstrapMo2Step` — самодостаточный.** Распаковка всегда, с заменой.
87. **`BootstrapMo2Step.Output = Input`.**
88. **Порядок pipeline:** BootstrapInstance → BootstrapMo2 → SyncArchives →
    ExecuteExtensions → ExecuteExtras → SyncMods → GenerateMetaIni →
    RegenerateProfile.
89. **`ConfigureMo2Step`** — не делаем.
90. **`InstallPipeline`** — склейка шагов.
91. **`InstallPipeline.BuildArchivesById`** — включая MO2-архив.
92. **`InstallCommand`** — печатает таблицу, включая extensions/extras.
93. **DI:** шаги — синглтоны, `MirrorDownloader` — через `AddHttpClient<T>`,
    регистрируется как `IArchiveDownloader`.
94. **`ExecuteExtensionsStep`** — раскладывает `manifest.Mo2.Extensions[]`
    в `<target>/MO2/`. Группирует директивы по archiveId, extract один
    раз на архив, `FileMatches`-skip для идемпотентности.
95. **`ExecuteExtrasStep`** — симметричен `ExecuteExtensionsStep`, корень
    `<target>/Stock Game/`.
96. **`ExecuteExtensionsStep`/`ExecuteExtrasStep` — Skipped**, если
    файлы уже на месте.

### Verify

97. **Verify — read-only.**
98. **Изоляция (вариант A).**
99. **`VerifyPipeline.Execute` — синхронный.**
100. **Проверки — приватные методы внутри `VerifyPipeline`.**
101. **`VerifyContext`** — единый контекст.
102. **`VerifyReport`** — `Checks`, `IsOk`, `PassedCount`, `FailedCount`.
103. **`VerifyCheckResult`** — `Name`, `Passed`, `Message`.
104. **Регенерация modlist.txt / plugins.txt / loadorder.txt в память**
     через `Serialize`-методы writer-ов.
105. **Сравнение `meta.ini`** — семантическое.
106. **`schemaVersion`** — через `ManifestSchema.IsSupported`.
107. **Пустые директивы у disabled-мода** — OK.
108. **Return code CLI:** 0 — OK, 1 — есть падения, 2 — ошибка, 130 — Ctrl+C.
109. **`ManifestJson.Load` / `Save` — sync-версии для verify.**
110. **`VerifyCommand`** — summary + провалы; `--verbose` — все проверки.
      `Markup.Escape`.
111. **`VerifySettings`** — `<target>` + `--verbose`/`-v`.
112. **`CheckMod` делает `yield break`** при отсутствии папки мода.
113. **Verify проверяет extensions/extras.** Общий helper
      `CheckDirectiveFile(displayPrefix, rootPath, directive)`.

### Cancellation / CLI (12.11.7, 12.13.10)

114. **`CancellationHelper.IsCancellation`** в `Firelink.Core` —
      распознаёт отмену, включая `AggregateException` со всеми
      cancellation-inner.
115. **`ArchiveMatcher.BuildAsync`** — async; `catch (OperationCanceledException)
      { throw; }` **перед** `catch (Exception)`.
116. **`PackCommand`/`InstallCommand`/`VerifyCommand`** — `catch (Exception ex)
      when (CancellationHelper.IsCancellation(ex))` → `Cancelled.` + exit 130.
      `Console.CancelKeyPress` подписывается на время выполнения команды,
      `e.Cancel = true` + `cts.Cancel()`, отписка в `finally`.
117. **`Program.cs` обоих CLI** — `catch (Exception ex)
      when (CancellationHelper.IsCancellation(ex))` → `Cancelled.` + exit 130
      (fallback на случай отмены до подписки).
118. **`config.PropagateExceptions()`** включено в обоих CLI. Без него
      Spectre перехватывает `CommandParseException` и `OperationCanceledException`
      до нашего `try/catch`.
119. **`catch (CommandParseException ex)`** в `Program.cs` — печатает
      `CLI error: <msg>` + `Hint: paths with spaces must be quoted: ...`
      (hint только если в `argv` нет строк с пробелами — иначе проблема
      не в кавычках).

### 12.13.x

120. **`BuildManifestStep.BuildExtensions`/`BuildExtras`** пропускают
     entry с пустым списком директив.
121. **`PackPipeline`** добавляет MO2-архив в `ArchiveIndex` перед
     созданием `ArchiveMatcher`: `archiveIndexWithMo2 = AddMo2ArchiveToResolved(...)`.
122. **`Slug.FromFileName`** отрезает **последнее** расширение до
     slug-ификации.
123. **`MatchStep` использует общий `ArchiveMatcher` через
     `Input.Matcher`.**
124. **`.bsa`/`.ba2` — единые файлы, не контейнеры.**
125. **CLI-таблицы `PackCommand`/`InstallCommand` включают
     extensions/extras.**
126. **`ArchiveDownloadHelper`** в `Firelink.Core.Archives` —
     общий helper скачивания (`.part`, retry через Polly, hash-check).

---

## Что сделано (кратко)

**Pipeline packer-а:**

ReadConfigStep → ReadInstanceStep → IndexArchivesStep → ScanModsStep →
ScanExtensionsStep → ScanExtrasStep →
**[build ArchiveMatcher]** →
MatchStep → MatchExtensionsStep → MatchExtrasStep →
**[write unmatched extensions/extras]** →
BuildManifestStep → ValidateManifestStep → WriteManifestStep

**Pipeline installer-а:**

ReadManifestStep → ResolveTargetStep → ValidateTargetStep →
BootstrapInstanceStep → BootstrapMo2Step → SyncArchivesStep →
ExecuteExtensionsStep → ExecuteExtrasStep → SyncModsStep →
GenerateMetaIniStep → RegenerateProfileStep

**Оркестратор:** `InstallPipeline`.
**CLI:** `InstallCommand`, `VerifyCommand`.

**MVP работает:**
- Прогон на `C:\Firelink\TestInstance\` — успешно (до 12.13.x).
- **Реальный прогон на `OmenRim 7` после 12.13.6:**
  - Pack: 82 мода, 4236 matched-директив, 69 архивов,
    49/49 extensions, 2/2 extras, 6 unmatched (runtime).
  - Install: 82 мода, 82 meta.ini, 1 extension written, 2 extras written.
  - Verify: **4338 passed, 0 failed**.

**593 теста, 0 failed.**

---

## План работы — оставшиеся блоки

**Всё, кроме 12.8, закрыто.**

### Блок 9 — 12.8: `NexusDownloader` ← **СЛЕДУЮЩИЙ**

**Цель:** убрать warning `No downloader for source type 'nexus'`
и дать возможность скачивать архивы с Nexus напрямую.

**Разбиение:**

- **12.8.1** — `NexusClient` + `NexusApiKeyProvider` + модели ответов.
  HTTP к `https://api.nexusmods.com/v1/`. API-ключ из
  `%USERPROFILE%\.firelink\nexus.key`.
- **12.8.2** — `NexusDownloader : IArchiveDownloader` (`SourceType => "nexus"`).
  `DownloadAsync`: получить ссылку через `NexusClient`, скачать
  `HttpClient`-ом. Hash — на стороне `SyncArchivesStep` (через
  `ArchiveDownloadHelper`).
- **12.8.3** — DI в `Firelink.Install/Program.cs` + тесты с
  fake-`HttpMessageHandler`.
- **12.8.4** — обновить DOC/HANDOFF/PROJECT-STATE.

**Что НЕ делаем:**
- Nexus Premium API (отдельная подписка).
- Кеширование ссылок.

**Ожидаемый `dotnet test`:** ~610 passed.

---

## Отложено на v0.2.0+

- Глобальный реестр `archives.db` (SQLite).
- Persist кеша хешей.
- Прогресс-бар Spectre.
- File-logging.
- Механизм патчей для inline-файлов.
- Nexus Premium API.

## Сознательно не делаем

- `ConfigureMo2Step` — автоконфигурация MO2.
- Автопатчи / autoPack / inlinePatterns — отменены.
- `repair` в CLI — install идемпотентен.
- Обработка `.bsa`/`.ba2` как контейнеров — они единые файлы.
- `firelink index`.

---

## Окружение

- Windows 10/11.
- .NET 8 SDK (SDK 10 тоже).
- Visual Studio 2022.
- `C:\Firelink\TestInstance\` — MVP прогон (до 12.13.x).
- `C:\Firelink\TestInstance2\` — после 12.13.6, extensions/extras,
  verify 0 failed.
- `C:\Firelink\OmenRim 7\` — тестовый оригинал.
- Большой инстанс — 4370 модов (прогон E1 отменён, не критично).
