# Firelink — состояние проекта и план работ

**Обновлено:** 2026-09-22
**Всего тестов:** 621, 0 failed
**Текущий блок:** Legacy cleanup (HANDOFF/PROJECT-STATE удалены)
**Следующий блок:** Фаза 6 — Nexus Premium (12.8)

**Спутние документы:**
- `DOC.md` (v4.2) — формальная документация: форматы, pipeline, CLI, обработка ошибок.
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

> Продолжаем проект Firelink. Стиль — пошаговые блоки кода с тестами.
>
> Прикладываю: FIRELINK.md, DOC.md (v4.2), repo-dump.md (свежий).
>
> Текущее состояние: 621 тест, 0 failed. Закрыты: MVP (packer,
> installer, verify), Фаза 1 (единый CLI `Firelink.Cli`),
> Фаза 2 (общие API для GUI).
>
> Следующая задача: [из раздела «План работ», актуальный блок].
>
> Стиль ответов:
> - Разбор задачи.
> - Полный код файлов с путями.
> - Инструкция по сборке/тестам.
> - Ожидаемый вывод dotnet test.
> - FIRELINK.md — только по запросу.
>
> Не пиши код, пока я не подтвержу готовность.

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

### В работе

- Ничего. Фаза 2 закрыта.

### Не начато

- Фаза 6 — Nexus Premium (12.8).
- Фаза 3 — GUI (Avalonia).
- Фаза 5 — Nexus Free (WebView2).

### Вычеркнуто

- **Фаза 4 — вариации дистрибутивов.** Решение 2026-09-21: не делаем.
  Один CLI, один GUI, один набор exe в дистрибутиве.
- **E1 (прогон на большом инстансе 4370 модов)** — отменён, не критично.

---

## Структура репозитория (после шагов 1.1–1.4)

```
C:\Code\Firelink
  Firelink.slnx
  Directory.Build.props
  Directory.Build.targets
  Directory.Packages.props
  DOC.md                       ← v4.2
  FIRELINK.md                  ← этот файл
  repo-dump.md
  samples/
    firelink-pack.minimal.json
    firelink-pack.full.json
    firelink-pack.invalid-name.json
    firelink-pack.invalid-path.json
    firelink-pack.invalid-version.json
  src/
    Firelink.Core/             ← ядро: модели, JSON, хеширование,
                                  валидаторы, абстракции,
                                  SevenZipExtractor, TempWorkspace,
                                  FileHashCache, ArchiveDownloadHelper,
                                  CancellationHelper,
                                  StepProgress
    Firelink.Platform.MO2/     ← чтение/запись MO2-файлов
    Firelink.Platform.Nexus/   ← пусто (Фаза 6)
    Firelink.Pack/             ← class library: PackPipeline + Steps + Matching
                                  + PackInputFactory + PackSummary
                                  + PackSummaryBuilder
    Firelink.Install/          ← class library: InstallPipeline + Steps +
                                  Downloaders + Verify
                                  + InstallInputFactory + InstallSummary
                                  + InstallSummaryBuilder
    Firelink.Cli/              ← exe (→ Firelink.Cli.exe):
                                  Program.cs, Commands/, Settings/,
                                  Infrastructure/TypeRegistrar.cs
                                  Зависимости: Firelink.Pack,
                                  Firelink.Install, Firelink.Core,
                                  Firelink.Platform.*
  tests/
    Firelink.Core.Tests/
    Firelink.Platform.MO2.Tests/
    Firelink.Pack.Tests/
    Firelink.Install.Tests/
    Firelink.Integration.Tests/
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

### Фаза 6 — Nexus Premium (12.8)

**Цель:** убрать warning `No downloader for source type 'nexus'` и
дать возможность скачивать архивы с Nexus через Premium-API.

**Статус:** не начата. Идёт **после Фазы 2** и **до Фазы 3** (GUI).

**Разбиение:**

- **12.8.1** — `NexusClient` + `NexusApiKeyProvider` + модели
  ответов.
  - HTTP к `https://api.nexusmods.com/v1/`.
  - API-ключ из `%USERPROFILE%\.firelink\nexus.key`.
- **12.8.2** — `NexusDownloader : IArchiveDownloader`
  (`SourceType => "nexus"`).
  - `DownloadAsync`: получить ссылку через `NexusClient`, скачать
    `HttpClient`-ом.
  - Hash — на стороне `SyncArchivesStep` (через
    `ArchiveDownloadHelper`).
- **12.8.3** — DI в `Firelink.Cli/Program.cs` (через
  `AddFirelinkInstall`) + тесты с fake-`HttpMessageHandler`.
- **12.8.4** — обновить `DOC.md` и `FIRELINK.md`.

**Что НЕ делаем:**

- Nexus Premium API (отдельная подписка) — это отдельная задача.
- Кеширование ссылок.
- `nxm://`, WebView2.

**Проверка:**

- Тесты: ~610 passed.
- Ручной: скачать архив с Nexus через Premium-аккаунт.

**Время:** ~1 неделя. **Риск:** низкий.

---

### Фаза 3 — GUI (Avalonia)

**Цель:** графический интерфейс поверх pipeline.

**Статус:** не начата.

**Архитектура (модульная):**

```
src/
  Firelink.Gui.Shared/       ← MVVM-инфра, стили, DI-extension
  Firelink.Gui.Install/      ← модуль installer (class library, Avalonia)
  Firelink.Gui.Pack/         ← модуль packer (class library, Avalonia)
  Firelink.Gui/              ← exe-оркестратор: Install + Pack
                                 → <AssemblyName>Firelink</AssemblyName>
                                 → Firelink.exe
```

**Контракт модуля:**

```csharp
public interface IGuiModule
{
    string Title { get; }
    string Icon { get; }
    int Order { get; }
    object CreateViewModel();
}
```

Главное окно — sidebar с модулями, content area.

**Что в MVP GUI:**

- Экран Install: выбор `modlist.json`, кнопка «Установить»,
  прогресс-бар (шаг X из Y), лог-панель (живой), кнопка «Отмена»,
  экран результата.
- Экран Pack: выбор `firelink-pack.json`, кнопка «Создать манифест»,
  тот же прогресс/лог, результат.
- Экран Verify: выбор инстанса, кнопка «Проверить», результат
  таблицей.
- `ObservableLoggerProvider` — логи в `ObservableCollection<LogEntry>`.
- `CancellationTokenSource` ← кнопка «Отмена».

**Что НЕ в MVP:**

- Настройки.
- Интерактивные диалоги (конфликты, выбор источника).
- Темы/иконки.
- WebView2.

**Проверка:**

- Ручная: запустить GUI, установить сборку на `TestInstance3`, verify.
- Сравнить с CLI-результатом — идентично.

**Стек:** Avalonia 11.x, CommunityToolkit.Mvvm,
Microsoft.Extensions.DependencyInjection.

**Время:** ~3 недели. **Риск:** средний.

**Важное:** `Firelink.exe` (GUI) — зарезервированное имя. В
дистрибутиве оба: `Firelink.Cli.exe` + `Firelink.exe`.

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
3. **Фаза 6** (12.8) — Nexus Premium. **← следующая**.
4. **Фаза 3** — GUI. Основной UI.
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

## Что НЕ делать

- Не делать «единый exe через `Process.Start` дочерних процессов».
- Не выносить presentation в pipeline. Pipeline — оркестрация,
  presentation — в клиентах.
- Не делать GUI до Фазы 2 (общие API).
- Не делать Free-стратегию (Фаза 5) до Premium (Фаза 6).
- Не трогать `Firelink.Core`, `Firelink.Platform.*` — они не меняются.

---

## Грабли и подводные камни

Накопленные замечания. Актуальны при правках.

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
- Nexus API — **Фаза 6 (12.8)**.
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
