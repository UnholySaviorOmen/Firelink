# Firelink — текущий статус

**Дата:** 2026-09-25
**Версия:** 0.1.0
**Тестов:** 766 passed, 0 failed

## Что готово

- **MVP** — packer, installer, verify. Полные пайплайны.
- **CLI** — единый `Firelink.Cli.exe` (pack/install/verify/hash/doctor).
- **GUI** — Avalonia 11.2.1, `Firelink.exe`. Экраны Home / Install /
  Pack / Verify / Logs / Settings. Плейсхолдеров нет.
- **Nexus Premium** (Фаза 6) — `NexusDownloader` через Premium API.
- **Фаза 1** — единый CLI, `Fireglink.Pack`/`Firelink.Install` как
  class libraries.
- **Фаза 2** — общие API (`StepProgress`, `IProgress`, фабрики,
  summary).
- **Фаза 3** — GUI, шаги 3.1–3.8. Ручной прогон на OmenRim 7:
  pack/install/verify, 4522 passed.
- **Фаза 3.9** — редизайн GUI (шаги 3.9.1–3.9.6). Тёмная тема,
  палитра `#1e1e1e` + `#d3b181`, иконки Lucide, Logs и Settings
  в отдельных вкладках.

## Что в работе

- **Фаза 3.9, шаг 3.9.7** — HomeView v2. Обсуждается:
  - **В1** — Home без карточек, приветствие с текстом «как начать».
  - **В2** — убрать Home вообще.
  - **В3** — Home с виджетами («last operation», «Nexus status»).

## Что не начато

- **Фаза 5** — Nexus Free (WebView2). Расширение для Free-аккаунтов.

## Тесты по проектам

- `Firelink.Core.Tests`
- `Firelink.Platform.MO2.Tests`
- `Firelink.Platform.Nexus.Tests`
- `Firelink.Pack.Tests`
- `Firelink.Install.Tests`
- `Firelink.Integration.Tests`
- `Firelink.Gui.Shared.Tests`
- `Firelink.Gui.Install.Tests`
- `Firelink.Gui.Pack.Tests`
- `Firelink.Gui.Verify.Tests`

**Всего: 766 passed, 0 failed.**

## Стиль работы

- Файлы давать целиком, не патчами.
- `dotnet test` после каждого блока. Полный вывод — при падении.
- Разбивать крупные блоки на подшаги (3.9.7.1, 3.9.7.2, ...).
- Не менять архитектурные решения без обсуждения.
- Не писать код, пока не подтверждена готовность.

## Ключевые документы

- **FIRELINK.md** — история, архитектурные решения, план работ.
- **DOC.md** — формальная документация (форматы, pipeline, CLI).
- **repo-dump.md** — свежий дамп кода.
