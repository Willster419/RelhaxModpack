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
  "login": "[xvm_dollar][lbracket][quote]login.xc[quote][colon][quote]login[quote][xvm_rbracket]",

  // Parameters for hangar.
  // Параметры ангара.
  "hangar": "[xvm_dollar][lbracket][quote]hangar.xc[quote][colon][quote]hangar[quote][xvm_rbracket]",

  // Parameters for userinfo window.
  // Параметры окна достижений.
  "userInfo": "[xvm_dollar][lbracket][quote]userInfo.xc[quote][colon][quote]userInfo[quote][xvm_rbracket]",

  // General parameters for the battle interface.
  // Общие параметры боевого интерфейса.
  "battle": "[xvm_dollar][lbracket][quote]battle.xc[quote][colon][quote]battle[quote][xvm_rbracket]",

  // Frag counter panel.
  // Панель счёта в бою.
  "fragCorrelation": "[xvm_dollar][lbracket][quote]battle.xc[quote][colon][quote]fragCorrelation[quote][xvm_rbracket]",

  // Ingame crits panel by "expert" skill.
  // Внутриигровая панель критов от навыка "эксперт".
  "expertPanel": "[xvm_dollar][lbracket][quote]battle.xc[quote][colon][quote]expertPanel[quote][xvm_rbracket]",

  // Battle interface text fields
  // Текстовые поля боевого интерфейса
  "battleLabels": "[xvm_dollar][lbracket][quote]battleLabels.xc[quote][colon][quote]labels[quote][xvm_rbracket]",

  // Log of the received hits.
  // Лог полученных попаданий.
  "damageLog": "[xvm_dollar][lbracket][quote]damageLog.xc[quote][colon][quote]damageLog[quote][xvm_rbracket]",

  // Special XVM hotkeys.
  // Специальные горячие клавиши XVM.
  "hotkeys": "[xvm_dollar][lbracket][quote]hotkeys.xc[quote][colon][quote]hotkeys[quote][xvm_rbracket]",

  // Parameters for squad window.
  // Параметры окна взвода.
  "squad": "[xvm_dollar][lbracket][quote]squad.xc[quote][colon][quote]squad[quote][xvm_rbracket]",

  // Parameters of the Battle Loading screen.
  // Параметры экрана загрузки боя.
  "battleLoading": "[xvm_dollar][lbracket][quote]battleLoading.xc[quote][colon][quote]battleLoading[quote][xvm_rbracket]",

  // Parameters for the alternative view of the Battle Loading screen.
  // Параметры альтернативного представления экрана загрузки боя.
  "battleLoadingTips": "[xvm_dollar][lbracket][quote]battleLoadingTips.xc[quote][colon][quote]battleLoadingTips[quote][xvm_rbracket]",

  // Parameters of the Battle Statistics form.
  // Параметры окна статистики по клавише Tab.
  "statisticForm": "[xvm_dollar][lbracket][quote]statisticForm.xc[quote][colon][quote]statisticForm[quote][xvm_rbracket]",

  // Parameters of the Players Panels ("ears").
  // Параметры панелей игроков ("ушей").
  "playersPanel": "[xvm_dollar][lbracket][quote]playersPanel.xc[quote][colon][quote]playersPanel[quote][xvm_rbracket]",

  // Parameters of the After Battle Screen.
  // Параметры окна послебоевой статистики.
  "battleResults": "[xvm_dollar][lbracket][quote]battleResults.xc[quote][colon][quote]battleResults[quote][xvm_rbracket]",

  // Hit log (my hits calculator).
  // Лог попаданий (счетчик своих попаданий).
  "hitLog": "[xvm_dollar][lbracket][quote]hitLog.xc[quote][colon][quote]hitLog[quote][xvm_rbracket]",

  // Capture bar.
  // Полоса захвата.
  "captureBar": "[xvm_dollar][lbracket][quote]captureBar.xc[quote][colon][quote]captureBar[quote][xvm_rbracket]",

  // Minimap.
  // Миникарта.
  "minimap": "[xvm_dollar][lbracket][quote]minimap.xc[quote][colon][quote]minimap[quote][xvm_rbracket]",

  // Minimap (alternative mode).
  // Миникарта (альтернативный режим).
  "minimapAlt": "[xvm_dollar][lbracket][quote]minimapAlt.xc[quote][colon][quote]minimap[quote][xvm_rbracket]",

  // Over-target markers.
  // Маркеры над танками.
  "markers": "[xvm_dollar][lbracket][quote]markers.xc[quote][colon][quote]markers[quote][xvm_rbracket]",

  // Color settings.
  // Настройки цветов.
  "colors": "[xvm_dollar][lbracket][quote]colors.xc[quote][colon][quote]colors[quote][xvm_rbracket]",

  // Options for dynamic transparency.
  // Настройки динамической прозрачности.
  "alpha": "[xvm_dollar][lbracket][quote]alpha.xc[quote][colon][quote]alpha[quote][xvm_rbracket]",

  // Text substitutions.
  // Текстовые подстановки.
  "texts": "[xvm_dollar][lbracket][quote]texts.xc[quote][colon][quote]texts[quote][xvm_rbracket]",

  // Icon sets.
  // Наборы иконок.
  "iconset": "[xvm_dollar][lbracket][quote]iconset.xc[quote][colon][quote]iconset[quote][xvm_rbracket]",

  // Vehicle names mapping.
  // Замена названий танков.
  "vehicleNames": "[xvm_dollar][lbracket][quote]vehicleNames.xc[quote][colon][quote]vehicleNames[quote][xvm_rbracket]",

  // Export data.
  // Выгрузка данных.
  "export": "[xvm_dollar][lbracket][quote]export.xc[quote][colon][quote]export[quote][xvm_rbracket]",

  // Parameters for tooltips.
  // Параметры всплывающих подсказок.
  "tooltips": "[xvm_dollar][lbracket][quote]tooltips.xc[quote][colon][quote]tooltips[quote][xvm_rbracket]",

  // Extra sounds settings.
  // Настройки дополнительных звуков.
  "sounds": "[xvm_dollar][lbracket][quote]sounds.xc[quote][colon][quote]sounds[quote][xvm_rbracket]",

  // XMQP services settings.
  // Настройки сервисов XMQP.
  "xmqp": "[xvm_dollar][lbracket][quote]xmqp.xc[quote][colon][quote]xmqp[quote][xvm_rbracket]",

  // Various settings for advanced users.
  // Различные настройки для продвинутых пользоватей.
  "tweaks": "[xvm_dollar][lbracket][quote]tweaks.xc[quote][colon][quote]tweaks[quote][xvm_rbracket]"
}
