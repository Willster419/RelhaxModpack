/**
 * Main configuration file (hereinafter - the configuration).
 *
 * Attention! Use \n as newline character instead of <br> tag
 *
 * --------------------------------------------------------------------------
 * Esli vy vidite nizhe bessmyslennyj nabor simvolov - smenite kodirovku
 * na UTF8+BOM
 *
 * Главный конфигурационный файл (далее - конфиг).
 *
 * Внимание! Для переноса на новую строку используйте \n вместо тэга <br>
 *
 * Внимание! Кодировка файлов должна оставаться UTF8+BOM. В противном случае
 * вместо кириллицы в игре будут пустые глифы.
 * Для редактирования используйте Notepad++. https://kr.cm/f/t/1382/c/25815/
 * В случае блокнота Windows: Сохранить как -> Кодировка: UTF.
 */
{
  // Version of the config. Do not remove or change it unnecessarily.
  // Версия конфига. Не удаляйте и не изменяйте её без необходимости.
  "configVersion": "6.1.0",

  // Automatically reload config. Requires client restart.
  // Автоматически перезагружать конфиг. Требует перезапуска клиента.
  "autoReloadConfig": false,

  // Language used in mod
  // "auto" - automatically detect language from game client,
  // or specify file name located in res_mods/mods/shared_resources/xvm/l10n/ (ex: "en").
  // Используемый язык в моде
  // "auto" - автоматически определять язык клиента игры,
  // или укажите имя файла в папке res_mods/mods/shared_resources/xvm/l10n/ (например, "en").
  "language": "auto",

  // Game Region:
  // "auto" - automatically detect game region from game client,
  // or specify one of: "RU", "EU", "NA", "ASIA", "KR", "CN"
  // Регион (игровой кластер):
  // "auto" - автоматически определять регион из клиента игры,
  // или укажите один из: "RU", "EU", "NA", "ASIA", "KR", "CN"
  "region": "auto",

  // Common config options. All settings information in the mod not being used.
  // Общие параметры конфига. Все параметры информационные, в моде не используются.
  "definition": {
    // Config author.
    // Автор конфига.
    "author": "XVM team",

    // Config description.
    // Описание конфига.
    "description": "Default settings for XVM",

    // Address to config updates.
    // Адрес, где выкладываются обновления конфига.
    "url": "https://modxvm.com/",

    // Config last modified.
    // Дата последней модификации конфига.
    "date": "16.05.2019",

    // Supported version of the game.
    // Поддерживаемая версия игры.
    "gameVersion": "1.5.0.1",

    // The minimum required version of the XVM mod.
    // Минимально необходимая версия мода XVM.
    "modMinVersion": "7.9.2"
  },

  // Parameters for login screen.
  // Параметры экрана логина.
  "login": ${"login.xc":"login"},

  // Parameters for hangar.
  // Параметры ангара.
  "hangar": ${"hangar.xc":"hangar"},

  // Parameters for userinfo window.
  // Параметры окна достижений.
  "userInfo": ${"userInfo.xc":"userInfo"},

  // General parameters for the battle interface.
  // Общие параметры боевого интерфейса.
  "battle": ${"battle.xc":"battle"},

  // Frag counter panel.
  // Панель счёта в бою.
  "fragCorrelation": ${"battle.xc":"fragCorrelation"},

  // Ingame crits panel by "expert" skill.
  // Внутриигровая панель критов от навыка "эксперт".
  "expertPanel": ${"battle.xc":"expertPanel"},

  // Battle interface text fields
  // Текстовые поля боевого интерфейса
  "battleLabels": ${"battleLabels.xc":"labels"},

  // Log of the received hits.
  // Лог полученных попаданий.
  "damageLog": ${"damageLog.xc":"damageLog"},

  // Special XVM hotkeys.
  // Специальные горячие клавиши XVM.
  "hotkeys": ${"hotkeys.xc":"hotkeys"},

  // Parameters for squad window.
  // Параметры окна взвода.
  "squad": ${"squad.xc":"squad"},

  // Parameters of the Battle Loading screen.
  // Параметры экрана загрузки боя.
  "battleLoading": ${"battleLoading.xc":"battleLoading"},

  // Parameters for the alternative view of the Battle Loading screen.
  // Параметры альтернативного представления экрана загрузки боя.
  "battleLoadingTips": ${"battleLoadingTips.xc":"battleLoadingTips"},

  // Parameters of the Battle Statistics form.
  // Параметры окна статистики по клавише Tab.
  "statisticForm": ${"statisticForm.xc":"statisticForm"},

  // Parameters of the Players Panels ("ears").
  // Параметры панелей игроков ("ушей").
  "playersPanel": ${"playersPanel.xc":"playersPanel"},

  // Parameters of the After Battle Screen.
  // Параметры окна послебоевой статистики.
  "battleResults": ${"battleResults.xc":"battleResults"},

  // Hit log (my hits calculator).
  // Лог попаданий (счетчик своих попаданий).
  "hitLog": ${"hitLog.xc":"hitLog"},

  // Capture bar.
  // Полоса захвата.
  "captureBar": ${"captureBar.xc":"captureBar"},

  // Minimap.
  // Миникарта.
  "minimap": ${"minimap.xc":"minimap"},

  // Minimap (alternative mode).
  // Миникарта (альтернативный режим).
  "minimapAlt": ${"minimapAlt.xc":"minimap"},

  // Over-target markers.
  // Маркеры над танками.
  "markers": ${"markers.xc":"markers"},

  // Color settings.
  // Настройки цветов.
  "colors": ${"colors.xc":"colors"},

  // Options for dynamic transparency.
  // Настройки динамической прозрачности.
  "alpha": ${"alpha.xc":"alpha"},

  // Text substitutions.
  // Текстовые подстановки.
  "texts": ${"texts.xc":"texts"},

  // Icon sets.
  // Наборы иконок.
  "iconset": ${"iconset.xc":"iconset"},

  // Vehicle names mapping.
  // Замена названий танков.
  "vehicleNames": ${"vehicleNames.xc":"vehicleNames"},

  // Export data.
  // Выгрузка данных.
  "export": ${"export.xc":"export"},

  // Parameters for tooltips.
  // Параметры всплывающих подсказок.
  "tooltips": ${"tooltips.xc":"tooltips"},

  // Extra sounds settings.
  // Настройки дополнительных звуков.
  "sounds": ${"sounds.xc":"sounds"},

  // XMQP services settings.
  // Настройки сервисов XMQP.
  "xmqp": ${"xmqp.xc":"xmqp"},

  // Various settings for advanced users.
  // Различные настройки для продвинутых пользоватей.
  "tweaks": ${"tweaks.xc":"tweaks"}
}
