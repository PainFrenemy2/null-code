<div align="center">

<img src="docs/banner.svg" alt="Null Monitor" width="100%"/>

# Null Monitor

**Красивый мониторинг железа для Windows 11 — температура, нагрузка, память, диски и сеть в одном окне.**

[![Build & Release](https://github.com/PainFrenemy2/null-code/actions/workflows/build.yml/badge.svg)](https://github.com/PainFrenemy2/null-code/actions/workflows/build.yml)
[![Latest release](https://img.shields.io/github/v/release/PainFrenemy2/null-code?label=%D1%80%D0%B5%D0%BB%D0%B8%D0%B7&color=7C3AED)](https://github.com/PainFrenemy2/null-code/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/PainFrenemy2/null-code/total?label=%D1%81%D0%BA%D0%B0%D1%87%D0%B8%D0%B2%D0%B0%D0%BD%D0%B8%D0%B9&color=22D3EE)](https://github.com/PainFrenemy2/null-code/releases)
![Windows 11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows11&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/license-MIT-22C55E)](LICENSE)

### [⬇ Скачать установщик](https://github.com/PainFrenemy2/null-code/releases/latest/download/NullMonitor-Setup.exe) · [все версии](https://github.com/PainFrenemy2/null-code/releases)

</div>

---

## 📌 Прямо на панели задач

После запуска цифры появляются **на панели задач рядом с часами и значками трея** — никаких окон открывать не нужно:

```
CPU  23%  54°C    ↓ 1.2 МБ/с    RAM 41%
GPU   8%  45°C    ↑  36 КБ/с
```

| Действие | Что происходит |
|---|---|
| 🖱 **Правый клик** | меню настроек: что показывать, вид, частота обновления, положение, **автозапуск с Windows**, выход |
| 🖱🖱 **Двойной клик** | открывает большую панель со всеми датчиками и графиками |
| ✋ **Зажать и тянуть** | сдвинуть виджет влево/вправо по панели задач |
| 👀 **Навести мышь** | подсказка с названиями железа и подробностями |

**Настройки в меню (ПКМ):**
- **Показывать** — нагрузка и температура CPU/GPU, оперативка, скорость загрузки и отдачи (включай/выключай что нужно)
- **Вид** — одна или две строки, размер шрифта, цвет текста (авто по теме Windows / белый / чёрный), цветная температура
- **Обновление** — каждые 0,5 / 1 / 2 / 5 секунд
- **Положение** — возле трея или слева на панели, сброс сдвига
- **Скрывать в полноэкранных играх**
- **Запускать вместе с Windows** — через планировщик задач, с правами администратора и без окна UAC при входе

Настройки сохраняются в `%AppData%\NullMonitor\settings.json`.

## ✨ Большая панель (двойной клик)

| | |
|---|---|
| 🧠 **Процессор** | нагрузка (кольцо), температура, средняя частота, мощность, загрузка каждого ядра/потока, график за 60 с |
| 🎮 **Видеокарта** | нагрузка, температура и hot spot, частота, мощность, видеопамять, обороты вентилятора, график |
| 🧩 **Оперативка** | занято / свободно / всего, процент, график |
| 🌐 **Сеть** | скорость загрузки и отдачи по всем адаптерам с живым графиком |
| 💾 **Накопители** | температура, заполненность, скорость чтения/записи для каждого диска |
| 🖥 **Система** | материнская плата, вентиляторы (об/мин), время работы, число процессов |

Плюс — тёмная тема в стиле Windows 11 со скруглёнными углами, кнопка «поверх всех окон» 📌 и цвет температуры, который меняется от 🟢 к 🟡 и 🔴.

## 🚀 Установка

### Вариант 1 — одной командой (без SmartScreen) ⭐

Открой **PowerShell** (Win+X → «Терминал») и вставь:

```powershell
irm https://github.com/PainFrenemy2/null-code/releases/latest/download/NullMonitor-Setup.exe -OutFile $env:TEMP\NullMonitor-Setup.exe; & $env:TEMP\NullMonitor-Setup.exe
```

### Вариант 2 — скачать установщик

[**⬇ NullMonitor-Setup.exe**](https://github.com/PainFrenemy2/null-code/releases/latest/download/NullMonitor-Setup.exe) → запустить → «Далее».

Если вылезет синее окно SmartScreen — **«Подробнее» → «Выполнить в любом случае»**.

### Вариант 3 — портативная версия

[**⬇ NullMonitor-Portable.exe**](https://github.com/PainFrenemy2/null-code/releases/latest/download/NullMonitor-Portable.exe) — просто запусти, ничего не ставится.

После запуска смотри на панель задач возле часов 👉 там появятся цифры.

> [!TIP]
> Если температура CPU показывает «—» или «~», установи бесплатный драйвер **[PawnIO](https://pawnio.eu)** и перезапусти программу. Он нужен библиотеке датчиков для чтения регистров процессора (на смену устаревшему WinRing0, который блокирует Защитник Windows).

## 🔄 Автообновление

Установленная версия раз в несколько часов проверяет [Releases](https://github.com/PainFrenemy2/null-code/releases). Когда выходит новая версия, она **сама скачивается, тихо ставится и перезапускается** — делать ничего не нужно. Выключить: ПКМ → «О программе» → «Обновляться автоматически». Проверить вручную: ПКМ → «О программе» → «Проверить обновления».

Обновления скачивает сама программа, поэтому SmartScreen на них не ругается.

## ❓ Частые вопросы

**Как удалить?** «Параметры» → «Приложения» → «Null Monitor» → «Удалить». Или ПКМ по цифрам → «О программе» → «Удалить Null Monitor…». Удаляется всё: программа, автозапуск и настройки.

**Почему появляется SmartScreen?** Windows показывает его для всех программ без платной цифровой подписи (сертификат стоит от ~$100 в год). Программа безопасна — весь код открыт в этом репозитории и собирается GitHub Actions прямо из исходников. Установка командой из «Варианта 1» и автообновления SmartScreen не вызывают.

**Портативная версия: если удалить `.exe`, автозапуск сломается?** Нет. При включении «Запускать вместе с Windows» портативная версия копирует себя в `%LocalAppData%\NullMonitor` и стартует оттуда.

**Температура CPU с «~» или «—»?** Без драйвера [PawnIO](https://pawnio.eu) Windows не даёт прочитать датчики процессора. Тогда программа показывает примерную температуру с датчика платы (со значком «~») или «—». Поставь PawnIO и перезапусти программу — будет точная.

## ⚙️ Автосборка

Каждый пуш в репозиторий запускает [GitHub Actions](.github/workflows/build.yml), который:

1. собирает проект на `windows-latest` через `dotnet publish` в один `.exe` (self-contained, win-x64);
2. присваивает версию `1.0.<номер сборки>`;
3. собирает установщик через [Inno Setup](https://jrsoftware.org/isinfo.php) (`installer/NullMonitor.iss`);
4. сразу публикует **релиз** с тегом `v1.0.N` — установленные программы подхватывают его автообновлением.

Ничего руками делать не нужно — запушил код, через пару минут новая версия в Releases.

## 🛠 Собрать самому

```powershell
git clone https://github.com/PainFrenemy2/null-code.git
cd null-code
dotnet publish src/NullMonitor/NullMonitor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
.\publish\NullMonitor.exe
```

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

## 📁 Структура

```
src/NullMonitor/
├─ Controls/            # отрисовка: кольца, графики, полоски ядер
├─ Services/            # опрос датчиков через LibreHardwareMonitor
├─ Assets/app.ico       # иконка
├─ App.xaml             # тема, стили, меню
├─ TaskbarWidget.xaml   # цифры на панели задач + меню настроек
└─ MainWindow.xaml      # большая панель
installer/              # скрипт установщика Inno Setup
.github/workflows/      # автосборка и релизы
```

## 🙏 Спасибо

Датчики читаются через [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) (MPL-2.0).

<div align="center">
<sub>Сделано с 💜 · <b>painfrenemy</b></sub>
</div>
