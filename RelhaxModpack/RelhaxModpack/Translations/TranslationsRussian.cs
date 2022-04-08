using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    public static partial class Translations
    {
        /// <summary>
        /// Loads all Russian translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsRussian()
        {
            #region General expressions
            Russian.Add("yes", "Да");
            Russian.Add("no", "Нет");
            Russian.Add("cancel", "Отмена");
            Russian.Add("delete", "Удалить");
            Russian.Add("warning", "ВНИМАНИЕ");
            Russian.Add("critical", "КРИТИЧЕСКАЯ ОШИБКА");
            Russian.Add("information", "Информация");
            Russian.Add("select", "Выбрать");
            Russian.Add("abort", "Отменить");
            Russian.Add("error", "Ошибка");
            Russian.Add("retry", "Повторить");
            Russian.Add("ignore", "Игнорировать");
            Russian.Add("lastUpdated", "Последнее обновление: ");
            Russian.Add("stepsComplete", "заданий выполнено");
            Russian.Add("allFiles", "Все файлы");
            Russian.Add("GoogleTranslateLanguageKey", "ru");
            Russian.Add("at", "в");
            Russian.Add("seconds", "сек.");
            Russian.Add("minutes", "мин.");
            Russian.Add("hours", "час.");
            Russian.Add("days", "дн.");
            Russian.Add("next", "Далее");
            Russian.Add("ContinueButton", "Продолжить");
            Russian.Add("previous", "Назад");
            Russian.Add("close", "Закрыть");
            Russian.Add("none", "Не выбрана");
            #endregion

            #region Application messages
            Russian.Add("appFailedCreateLogfile", "Приложению не удалось открыть файл журнала. Проверьте права доступа или переместите приложение в папку, в которой разрешена запись.");
            Russian.Add("failedToParse", "Не удалось обработать файл");
            Russian.Add("failedToGetDotNetFrameworkVersion", "Не удалось определить установленную версию .NET Framework. Вероятно, это вызвано недостаточными привилегиями или блокировкой работы антивирусным ПО.");
            Russian.Add("invalidDotNetFrameworkVersion", "На вашем компьютере установлена версия .NET Framework ниже 4.8. Для работы Relhax Modpack требуется версия 4.8 или выше. Хотите перейти на страницу загрузки актуальной версии?");
            #endregion

            #region Tray Icon
            Russian.Add("MenuItemRestore", "Восстановить");
            Russian.Add("MenuItemCheckUpdates", "Проверить наличие обновлений");
            Russian.Add("MenuItemAppClose", Russian["close"]);
            Russian.Add("newDBApplied", "Применена новая версия базы данных");
            #endregion

            #region Main Window
            Russian.Add("InstallModpackButton", "Начать выбор модов");
            Russian.Add("selectWOTExecutable", "Выберите исполняемый файл игры (WorldOfTanks.exe)");
            Russian.Add("InstallModpackButtonDescription", "Выберите моды, которые вы хотите установить в клиент World of Tanks");
            Russian.Add("UninstallModpackButton", "Удалить модпак Relhax");
            Russian.Add("UninstallModpackButtonDescription", "Удаление *всех* установленных модификаций в клиент WoT");
            Russian.Add("ViewNewsButton", "Информация об обновлениях");
            Russian.Add("ViewNewsButtonDescription", "Прочитать информацию об обновлениях приложения, базы данных и узнать другие новости.");
            Russian.Add("ForceManuelGameDetectionText", "Принудительно указать папку с игрой");
            Russian.Add("ForceManuelGameDetectionCBDescription", "Принудительный выбор папки с World of Tanks.\nИспользуйте только в случае проблем с автоматическим определением расположения игры.");
            Russian.Add("LanguageSelectionTextblock", "Выбрать язык");
            Russian.Add("LanguageSelectionTextblockDescription", "Выберите желаемый язык.\nЕсли вы заметите неполноту перевода или ошибки, не стесняйтесь сообщать нам о них.");
            Russian.Add("Forms_ENG_NAButtonDescription", "Перейти на страницу модпака на форуме World of Tanks NA (страница на английском)");
            Russian.Add("Forms_ENG_EUButtonDescription", "Перейти на страницу модпака на форуме World of Tanks EU (страница на английском)");
            Russian.Add("Forms_GER_EUButtonDescription", "Перейти на страницу модпака на форуме World of Tanks EU (страница на немецком)");
            Russian.Add("SaveUserDataText", "Сохранить пользовательские данные");
            Russian.Add("SaveUserDataCBDescription", "Установщик сохранит пользовательские данные (например, сессионную статистику из предыдущих боев)");
            Russian.Add("CleanInstallText", "Чистая установка (рекомендуется)");
            Russian.Add("CleanInstallCBDescription", "Перед установкой новых модификаций будут удалены все ранее установленные."); // No need to tell twice that it's a recommended option (following Windows localization style) - DrWeb7_1
            Russian.Add("BackupModsText", "Сделать резервную копию папки модификаций");
            Russian.Add("BackupModsSizeLabelUsed", "Бэкапов: {0} Размер: {1}");
            Russian.Add("backupModsSizeCalculating", "Вычисление размера...");
            Russian.Add("BackupModsCBDescription", "Созданные резервные копии будут находиться в папке 'RelHaxModBackup' в виде ZIP-архива с датой создания в названии.");
            Russian.Add("BackupModsSizeLabelUsedDescription", Russian["BackupModsCBDescription"]);
            Russian.Add("SaveLastInstallText", "Запомнить выбранные моды");
            Russian.Add("SaveLastInstallCBDescription", "Установщик автоматически выберет моды, указанные вами в прошлый раз.");
            Russian.Add("MinimizeToSystemTrayText", "Свернуть в трей");
            Russian.Add("MinimizeToSystemTrayDescription", "Приложение продолжит работу в фоновом режиме, когда вы его закроете.");
            Russian.Add("VerboseLoggingText", "Детальное журналирование");
            Russian.Add("VerboseLoggingCBDescription", "Объём данных в файле журнала будет увеличен: полезно при отправке отчётов об ошибке.");
            Russian.Add("AllowStatsGatherText", "Разрешить сбор статистики об используемых модификациях");
            Russian.Add("AllowStatsGatherCBDescription", "Установщик будет собирать на сервере анонимную статистику использованных модификаций.\nЭто позволит нам определить приоритет их поддержки.");
            Russian.Add("DisableTriggersText", "Отключить триггеры");
            Russian.Add("DisableTriggersCBDescription", "Включённые триггеры позволят ускорить установку, выполняя во время распаковки некоторые задачи (такие как создание контурных иконок)\nпосле подготовки всех необходимых ресурсов. При обнаружении пользовательских модов триггеры отключаются автоматически.");
            Russian.Add("appDataFolderNotExistHeader", "Не удалось найти папку кэша World of Tanks");
            Russian.Add("CancelDownloadInstallButton", Russian["cancel"]);
            Russian.Add("appDataFolderNotExist", "Установщик не обнаружил папку кэша игры. Продолжить установку без очистки кэша?");
            Russian.Add("viewAppUpdates", "Посмотреть последние обновления приложения");
            Russian.Add("viewDBUpdates", "Посмотреть последние обновления базы данных");
            Russian.Add("EnableColorChangeDefaultV2Text", "Заменить цвета");
            Russian.Add("EnableColorChangeDefaultV2CBDescription", "Включить замену цветов при выборе модификации или конфигурации.");
            Russian.Add("EnableColorChangeLegacyText", "Заменить цвета");
            Russian.Add("EnableColorChangeLegacyCBDescription", "Включить замену цветов при выборе модификации или конфигурации.");
            Russian.Add("ShowOptionsCollapsedLegacyText", "Показывать опции свёрнутыми");
            Russian.Add("ShowOptionsCollapsedLegacyCBDescription", "Списки модификаций для выбора (кроме категорий) будут свёрнуты.");
            Russian.Add("ClearLogFilesText", "Очистить файлы журнала");
            Russian.Add("ClearLogFilesCBDescription", "Очистка логов World of Tanks (python.log), XVM (xvm.log) и PMOD (pmod.log).");
            Russian.Add("CreateShortcutsText", "Создать ярлыки на рабочем столе");
            Russian.Add("CreateShortcutsCBDescription", "Будут созданы ярлыки на рабочем столе для модификаций, являющимися EXE-файлами (как WWIIHA)");
            Russian.Add("DeleteOldPackagesText", "Удалить старые пакеты модификаций");
            Russian.Add("DeleteOldPackagesCBDescription", "Удалять ненужные установщику ZIP-архивы из папки \"RelhaxDownloads\" с целью освобождения места на диске.");
            Russian.Add("MinimalistModeText", "Минималистичный режим");
            Russian.Add("MinimalistModeCBDescription", "В этом режиме установщик модпака исключает некоторые необязательные пакеты, такие как кнопка модпака или файлы темы Relhax.");
            Russian.Add("AutoInstallText", "Включить автоустановку");
            Russian.Add("AutoInstallCBDescription", "Установщик автоматически проверит наличие обновлений модификаций в указанное время и применит их, основываясь на выбранной конфигурации.");
            Russian.Add("OneClickInstallText", "Установка «в один клик»");
            Russian.Add("OneClickInstallCBDescription", "Установщик автоматически запустит установку модификаций сразу после выбора конфигурации.");
            Russian.Add("ForceEnabledCB", "Принудительно выбрать все пакеты [!]");
            Russian.Add("AutoOneclickShowWarningOnSelectionsFailText", "Показывать предупреждение, если шаблон предустановки загружен с ошибками.");
            Russian.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "При использовании функции установки в один клик или автоматической установки будет показываться предупреждение. Вы сможете прервать установку при появлении ошибок.");
            Russian.Add("ForceEnabledText", "Принудительно выбрать все пакеты [!]");
            Russian.Add("ForceEnabledCBDescription", "Отмечает все доступные к установке пакеты. Может привести к серьёзным проблемам со стабильностью.");
            Russian.Add("ForceVisibleText", "Принудительно показать все пакеты [!]");
            Russian.Add("ForceVisibleCBDescription", "Показывает все скрытые пакеты. Может привести к серьёзным проблемам со стабильностью.");
            Russian.Add("LoadAutoSyncSelectionFileText", "Загрузить файл конфигурации");
            Russian.Add("LoadAutoSyncSelectionFileDescription", "Файл конфигурации будет использован для автоматической установки или установки «в один клик»");
            Russian.Add("AutoSyncCheckFrequencyTextBox", "Частота: каждые");
            Russian.Add("DeveloperSettingsHeader", "Настройки для разработчиков [!]");
            Russian.Add("DeveloperSettingsHeaderDescription", "Указанные ниже опции могут привести к нестабильному поведению игры и вызывать проблемы!\nПожалуйста, не используйте их, если не знаете, что делаете!");
            Russian.Add("ApplyCustomScalingText", "Мастшабирование приложения");
            Russian.Add("ApplyCustomScalingTextDescription", "Применить масштабирование дисплея к окнам установщика.");
            Russian.Add("EnableCustomFontCheckboxText", "Использовать другой шрифт");
            Russian.Add("EnableCustomFontCheckboxTextDescription", "Вы можете выбрать любой установленный в системе шрифт,\nкоторый будет использован почти во всех окнах программы.");
            Russian.Add("LauchEditorText", "Запустить редактор БД");
            Russian.Add("LauchEditorDescription", "Запуск редактора базы данных непосредственно из Relhax, а не командной строки.");
            Russian.Add("LauchPatchDesignerText", "Запустить конструктор патчей");
            Russian.Add("LauchPatchDesignerDescription", "Запуск конструктора патчей непосредственно из Relhax, а не командной строки.");
            Russian.Add("LauchAutomationRunnerText", "Запустить средства автоматизации");
            Russian.Add("LauchAutomationRunnerDescription", "Запуск средств автоматизации непосредственно из Relhax, а не командной строки.");
            Russian.Add("InstallWhileDownloadingText", "Распаковка во время скачивания");
            Russian.Add("InstallWhileDownloadingCBDescription", "Установщик будет распаковывать ZIP-архив сразу после скачивания,\nа не ждать окончания загрузки всех файлов перед распаковкой.");
            Russian.Add("MulticoreExtractionCoresCountLabel", "Обнаружено ядер: {0}");
            Russian.Add("MulticoreExtractionCoresCountLabelDescription", "Количество логических процессоров (потоков), обнаруженных в вашем ПК."); // In this context, the system detects cores on PC. - DrWeb7_1
            Russian.Add("SaveDisabledModsInSelectionText", "Запоминать отключённые модификации при сохранении конфигурации");
            Russian.Add("SaveDisabledModsInSelectionDescription", "Когда мод будет включён в БД, он снова будет выбран из вашей конфигурации");
            Russian.Add("AdvancedInstallationProgressText", "Показывать больше данных в окне прогресса установки");
            Russian.Add("AdvancedInstallationProgressDescription", "Окно прогресса установки будет содержать больше данных, полезно при использовании многопоточной распаковки.");
            Russian.Add("ThemeSelectText", "Выберите тему:");
            Russian.Add("ThemeDefaultText", "Стандартная");
            Russian.Add("ThemeDefaultDescription", "Стандартная тема");
            Russian.Add("ThemeDarkText", "Тёмная");
            Russian.Add("ThemeDarkDescription", "Тёмная тема");
            Russian.Add("ThemeCustomText", "Сторонняя");
            Russian.Add("ThemeCustomDescription", "Сторонняя тема");
            Russian.Add("DumpColorSettingsButtonText", "Сохранить текущую цветовую схему"); // Again, Windows UI flashbacks: we specify the color *scheme* in the settings. - DrWeb7_1
            Russian.Add("DumpColorSettingsSaveSuccess", "Цветовая схема сохранена");
            Russian.Add("OpenColorPickerButtonText", "Открыть палитру");
            Russian.Add("OpenColorPickerButtonDescription", "В палитре можно выбрать цвета для создания собственной темы.");
            Russian.Add("DumpColorSettingsButtonDescription", "Будет создан XML-файл, в котором содержатся все параметры цветов для тех участков, где возможна замена цвета.");
            Russian.Add("MulticoreExtractionText", "Многопроцессорный режим распаковки");
            Russian.Add("MulticoreExtractionCBDescription", "Установщик будет использовать метод параллельной распаковки. Будет извлекаться несколько\nZIP-архивов одновременно, уменьшая время установки. ТОЛЬКО ДЛЯ SSD ДИСКОВ!");
            Russian.Add("UninstallDefaultText", "Стандартный");
            Russian.Add("UninstallQuickText", "Быстрый");
            Russian.Add("ExportModeText", "Режим экспорта");
            Russian.Add("ExportModeCBDescription", "Режим экспорта позволит выбрать папку для экспорта установленных модификаций в игру. Только для продвинутых пользователей.\n" +
                "Учтите, что XML-файлы игры не будут распакованы или пропатчены, атласы тоже не будут созданы. Инструкции находятся в папке экспорта.");
            Russian.Add("ViewCreditsButtonText", "Авторы модпака"); // I couldn't find anything more suitable, so "Modpack Authors" will be left here. *shrug* - DrWeb7_1
            Russian.Add("ViewCreditsButtonDescription", "Познакомьтесь с замечательными людьми и проектами, поддерживающими модпак!");
            Russian.Add("ExportWindowDescription", "Выберите версию WoT, для которой нужно произвести экспорт");
            Russian.Add("HelperText", "Вас приветствует Relhax Modpack!\n\nЯ старался сделать его максимально простым для пользователя, но вопросы всё же могут возникнуть.\n\nНаведите курсор мыши на любую настройку, и вы увидите пояснение к ней.\n\nБлагодарим вас за выбор в пользу Relhax, надеюсь, вам понравится! - Willster419");
            Russian.Add("helperTextShort", "Вас приветствует Relhax Modpack!");
            Russian.Add("NotifyIfSameDatabaseText", "Сообщать об актуальности БД (только для стабильной версии)");
            Russian.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Уведомлять в случае совпадения версий баз данных. Это означает отсутствие обновлений к каким-либо модификациям.");
            Russian.Add("NotifyIfSameDatabaseCBDescription", "Если ваша версия БД является актуальной, то вы увидите уведомление. Это значит, что модификации не получали обновлений.");
            Russian.Add("ShowInstallCompleteWindowText", "Показывать расширенное окно окончания установки");
            Russian.Add("ShowInstallCompleteWindowCBDescription", "Показывать окно по окончании установки с частыми действиями после установки модпака (запуск игры, открыть сайт XVM, и т. п.)");
            Russian.Add("applicationVersion", "Версия ПО:");
            Russian.Add("databaseVersion", "Версия БД:");
            Russian.Add("ClearCacheText", "Очистить кэш World of Tanks");
            Russian.Add("ClearCacheCBDescription", "Очистить папку кэша World of Tanks. Операция аналогична соответствующей опции, присутствовавшей в OMC.");
            Russian.Add("UninstallDefaultDescription", "Обычная деинсталляция удалит все моды, включая ярлыки и кэш в AppData.");
            Russian.Add("UninstallQuickDescription", "Быстрая деинсталляция удалит только моды, оставив ярлыки, созданные модпаком, и кэш в AppData.");
            Russian.Add("DiagnosticUtilitiesButton", "Диагностика");
            Russian.Add("DiagnosticUtilitiesButtonDescription", "Сообщить о баге, попытаться починить клиент игры, и т. д.");
            Russian.Add("UninstallModeGroupBox", "Режим деинсталляции:");
            Russian.Add("UninstallModeGroupBoxDescription", "Выбрать метод удаления");
            Russian.Add("FacebookButtonDescription", "Перейти на нашу страницу в Facebook");
            Russian.Add("DiscordButtonDescription", "Перейти на наш сервер Discord");
            Russian.Add("TwitterButtonDescription", "Перейти на нашу страницу в Twitter");
            Russian.Add("SendEmailButtonDescription", "Отправить нам письмо на e-mail (Не для техподдержки)");
            Russian.Add("HomepageButtonDescription", "Посетить наш веб-сайт");
            Russian.Add("DonateButtonDescription", "Поддержать копеечкой для дальнейшей разработки");
            Russian.Add("FindBugAddModButtonDescription", "Нашли баг? Хотите добавить мод? Пишите сюда!");
            Russian.Add("SelectionViewGB", "Вид списка");
            Russian.Add("SelectionDefaultText", "Стандартный");
            Russian.Add("SelectionLayoutDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");
            Russian.Add("SelectionDefaultDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");
            Russian.Add("SelectionLegacyDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");
            Russian.Add("LanguageSelectionGBDescription", "Выберите желаемый язык");
            Russian.Add("EnableBordersDefaultV2Text", "Включить границы");
            Russian.Add("EnableBordersLegacyText", "Включить границы");
            Russian.Add("EnableBordersDefaultV2CBDescription", "Включить показ чёрных рамок вокруг наименования каждой модификации и конфигурации.");
            Russian.Add("EnableBordersLegacyCBDescription", "Включить показ чёрных рамок вокруг наименования каждой модификации и конфигурации.");
            Russian.Add("UseBetaDatabaseText", "Использовать бета-версию БД");
            Russian.Add("UseBetaDatabaseCBDescription", "Использовать последнюю доступную бета-версию БД. Стабильность модификаций не гарантирована.");
            Russian.Add("UseBetaApplicationText", "Использовать бета-версию программы");
            Russian.Add("UseBetaApplicationCBDescription", "Использовать последнюю доступную бета-версию программы. Корректность перевода и стабильность приложения не гарантированы.");
            Russian.Add("SettingsTabIntroHeader", "Добро пожаловать!");
            Russian.Add("SettingsTabSelectionViewHeader", "Выбор вида списка");
            Russian.Add("SettingsTabInstallationSettingsHeader", "Параметры установки");
            Russian.Add("SettingsTabApplicationSettingsHeader", "Параметры приложения");
            Russian.Add("SettingsTabAdvancedSettingsHeader", "Расширенные настройки");
            Russian.Add("MainWindowSelectSelectionFileToLoad", "Выберите конфигурацию для загрузки");
            Russian.Add("verifyUninstallHeader", "Подтверждение");
            Russian.Add("verifyUninstallVersionAndLocation", "Подтвердите необходимость удалить моды для WoT в этой папке: \n\n{0}\n\nИспользуем метод «{1}»?");
            Russian.Add("failedVerifyFolderStructure", "Не удалось создать необходимую структуру папок. Проверьте права доступа к файлам или переместите приложение в папку, где разрешена запись.");
            Russian.Add("failedToExtractUpdateArchive", "Не удалось распаковать файлы обновлений. Проверьте права доступа к файлам или переместите приложение в папку, где разрешена запись.");
            Russian.Add("downloadingUpdate", "Загружается обновление приложения:"); // Colon is required here (at least in Russian) - DrWeb7_1
            Russian.Add("autoOneclickSelectionFileNotExist", "Указанный путь к файлу конфигурации не существует.");
            Russian.Add("AutoOneclickSelectionErrorsContinueBody", "Возникли проблемы при загрузке файла конфигурации (вероятно, были удалены или переименованы пакеты, и т. д.).\nПродолжить работу в любом случае?");
            Russian.Add("AutoOneclickSelectionErrorsContinueHeader", "Проблемы в импорте файла конфигурации");
            Russian.Add("noAutoInstallWithBeta", "При использовании бета-версии БД установка в автоматическом режиме невозможна.");
            Russian.Add("autoInstallWithBetaDBConfirmBody", "Будет включена автоматическая установка модификаций из бета-версии БД. Она обновляется чаще основной, и вполне допустима установка неоднократная установка одних и тех те модификаций.\nВы точно хотите включить автоматическую установку?");
            Russian.Add("autoInstallWithBetaDBConfirmHeader", Russian["verifyUninstallHeader"]);
            Russian.Add("ColorDumpSaveFileDialog", "Выберите путь для сохранения файла с настройками цветовой схемы");
            //"branch" is this context is git respoitory branches
            Russian.Add("loadingBranches", "Загружаются ветви репозитория");
            //"branch" is this context is git respoitory branches
            Russian.Add("failedToParseUISettingsFile", "Не удалось применить тему. Подробности в файле журнала. Включите «Детальное журналирование» для получения более детальной информации.");
            Russian.Add("UISettingsFileApplied", "Тема применена");
            Russian.Add("failedToFindWoTExe", "Не удалось получить расположение клиента World of Tanks. Пожалуйста, отправьте отчёт об ошибке разработчику.");
            Russian.Add("failedToFindWoTVersionXml", "Не удалось получить информацию о версии клиента World of Tanks. Проверьте наличие файла version.xml в папке с игрой.");
            #endregion

            #region ModSelectionList
            Russian.Add("ModSelectionList", "Файл предустановки");
            Russian.Add("ContinueButtonLabel", "Установить");
            Russian.Add("CancelButtonLabel", Russian["cancel"]);
            Russian.Add("HelpLabel", "Клик правой кнопкой мыши по компоненту покажет превью.");
            Russian.Add("LoadSelectionButtonLabel", "Загрузить конфигурацию");
            Russian.Add("SaveSelectionButtonLabel", "Сохранить конфигурацию");
            Russian.Add("SelectSelectionFileToSave", "Сохранить конфигурацию");
            Russian.Add("ClearSelectionsButtonLabel", "Снять все галочки");
            Russian.Add("SearchThisTabOnlyCB", "Искать только в этой вкладке");
            Russian.Add("searchComboBoxInitMessage", "Найти пакет...");
            Russian.Add("SearchTBDescription", "Вы так же можете искать по нескольким частям названия, разделяя их * (звёздочкой).\nК примеру, config*willster419 покажет в качестве результата поиска Willster419\'s Config");
            Russian.Add("InstallingAsWoTVersion", "Установка в клиент WoT версии {0}");
            Russian.Add("UsingDatabaseVersion", "Используется БД: {0} ({1})");
            Russian.Add("userMods", "Пользовательские моды");
            Russian.Add("FirstTimeUserModsWarning", "Данная вкладка предназначена для выбора модов, расположенных в папке \"RelhaxUserMods\". Они должны быть в виде ZIP-архивов и использовать корневую папку World of Tanks.");
            Russian.Add("downloadingDatabase", "Загружается база данных");
            Russian.Add("readingDatabase", "Читается база данных");
            Russian.Add("loadingUI", "Загрузка интерфейса");
            Russian.Add("verifyingDownloadCache", "Проверяется целостность файла ");
            Russian.Add("InstallProgressTextBoxDescription", "Прогресс текущей установки будет показан здесь");
            Russian.Add("testModeDatabaseNotFound", "КРИТИЧЕСКАЯ ОШИБКА: Тестовая БД не найдена по адресу:\n{0}");
            Russian.Add("duplicateMods", "КРИТИЧЕСКАЯ ОШИБКА: Обнаружен дубликат пакета с таким же ID");
            Russian.Add("databaseReadFailed", "КРИТИЧЕСКАЯ ОШИБКА: Ошибка чтения базы данных!\nПодробности в файле журнала.");
            Russian.Add("configSaveSuccess", "Конфигурация успешно сохранена");
            Russian.Add("selectConfigFile", "Найти файл конфигурации для загрузки");
            Russian.Add("configLoadFailed", "Файл конфигурации не может быть загружен, работа будет продолжена в обычном режиме");
            Russian.Add("modNotFound", "Пакет (ID = \"{0}\") не найден в базе данных. Вероятно, он был переименован или удалён.");
            Russian.Add("modDeactivated", "Данные пакеты на данный момент отключены в модпаке и не могут быть выбраны для установки");
            Russian.Add("modsNotFoundTechnical", "Не удалось найти данные пакеты. Вероятно, они были удалены.");
            Russian.Add("modsBrokenStructure", "Данные пакеты были отключены в связи с изменениями в их структуре. Вам нужно проверить их самостоятельно, если хотите произвести установку.");
            Russian.Add("packagesUpdatedShouldInstall",  "Следующие пакеты были обновлены с момента последней загрузки данной конфигурации. Файл конфигурации был обновлён и изменён (его резервная копия так же была сделана). Если она является основной, и вы хотите её сохранить, рекомендуем вам установить/обновить моды после этого уведомления.");
            Russian.Add("selectionFileIssuesTitle", "Внимание"); // I don't think it's possible to translate that directly, so leaving it as "Warning" - DrWeb7_1
            Russian.Add("selectionFileIssuesHeader", "Пожалуйста, прочтите следующие сообщения, связанные с вашим файлом конфигурации");
            Russian.Add("selectionFormatOldV2", "Этот файл конфигурации сохранён в устаревшем формате (v2) и будет обновлён до v3. Резервная копия старого файла также будет сделана.");
            Russian.Add("oldSavedConfigFile", "Сохранённый файл конфигурации использует устаревший формат и может некорректно работать в будущем. Хотите преобразовать его в новый? Резервная копия старого файла также будет сделана.");
            Russian.Add("prefrencesSet", "Настройки применены");
            Russian.Add("selectionsCleared", "Список выбранных модов очищен");
            Russian.Add("failedLoadSelection", "Сбой загрузки конфигурации");
            Russian.Add("unknownselectionFileFormat", "Неизвестная версия файла конфигурации");
            Russian.Add("ExpandAllButton", "Раскрыть текущую вкладку");
            Russian.Add("CollapseAllButton", "Свернуть текущую вкладку");
            Russian.Add("InstallingTo", "Установка в {0}");
            Russian.Add("selectWhereToSave", "Выберите путь для сохранения файла конфигурации");
            Russian.Add("updated", "обновлён");
            Russian.Add("disabled", "отключен");
            Russian.Add("invisible", "невидим");
            Russian.Add("SelectionFileIssuesDisplay", "Ошибка применения файла конфигурации");
            Russian.Add("selectionFileIssues", Russian["SelectionFileIssuesDisplay"]);
            Russian.Add("VersionInfo", "Обновление приложения");
            Russian.Add("VersionInfoYesText", Russian["yes"]);
            Russian.Add("VersionInfoNoText", Russian["no"]);
            Russian.Add("NewVersionAvailable", "Доступна новая версия");
            Russian.Add("HavingProblemsTextBlock", "Если возникли проблемы в процессе обновления, пожалуйста,");
            Russian.Add("ManualUpdateLink", "нажмите сюда.");
            Russian.Add("loadingApplicationUpdateNotes", "Загружается список изменений в обновлении...");
            Russian.Add("failedToLoadUpdateNotes", "Не удалось получить список изменений в обновлении приложения");
            Russian.Add("ViewUpdateNotesOnGoogleTranslate", "Посмотреть через переводчик Google");
            Russian.Add("VersionInfoAskText", "Хотите обновить прямо сейчас?");
            Russian.Add("SelectDownloadMirrorTextBlock", "Выберите зеркало для загрузки");
            Russian.Add("SelectDownloadMirrorTextBlockDescription", "Это зеркало будет использовано только для скачивания пакетов.");
            Russian.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Даллас, США");
            Russian.Add("downloadMirrorDe", "clanverwaltung.de, Франкфурт, Германия");
            #endregion

            #region Installer Messages
            Russian.Add("Downloading", "Скачивание");
            Russian.Add("patching", "Применение патчей");
            Russian.Add("done", "Готово");
            Russian.Add("cleanUp", "Очистка ресурсов");
            Russian.Add("idle", "Ожидание");
            Russian.Add("status", "Состояние:");
            Russian.Add("canceled", "Отменено");
            Russian.Add("appSingleInstance", "Проверяется наличие только одного процесса");
            Russian.Add("checkForUpdates", "Проверяется наличие обновлений");
            Russian.Add("verDirStructure", "Проверяется структура каталогов");
            Russian.Add("loadingSettings", "Загружаются настройки");
            Russian.Add("loadingTranslations", "Загружаются переводы на другие языки");
            Russian.Add("loading", "Загрузка");
            Russian.Add("of", "из");
            Russian.Add("failedToDownload1", "Не удалось загрузить пакет");
            Russian.Add("failedToDownload2", "Хотите попробовать ещё раз, прервать или продолжить установку?");
            Russian.Add("failedToDownloadHeader", "Не удалось загрузить");
            Russian.Add("failedManager_version", "Данная бета-версия не актуальна и должна быть обновлена через стабильный канал. Новых бета-версий в данный момент нет.");
            Russian.Add("fontsPromptInstallHeader", "Права администратора для установки шрифтов?");
            Russian.Add("fontsPromptInstallText", "У вас есть права администратора, необходимые для установки шрифтов?");
            Russian.Add("fontsPromptError_1", "Не удалось установить шрифты. Некоторые моды могут работать некорректно. Шрифты расположены в ");
            Russian.Add("fontsPromptError_2", "\\_fonts. Попробуйте установить их самостоятельно или перезапустите программу от имени администратора.");
            Russian.Add("cantDownloadNewVersion", "Невозможно загрузить новую версию, приложение будет закрыто.");
            Russian.Add("failedCreateUpdateBat", "Невозможно создать процесс программы обновления.\n\nУдалите вручную этот файл:\n{0}\n\nПереименуйте файл:\n{1}\nна:\n{2}\n\nПерейти к папке прямо сейчас?");
            Russian.Add("cantStartNewApp", "Не удалось запустить приложение, но оно расположено в \n");
            Russian.Add("autoDetectFailed", "Не удалось автоматически обнаружить игру. Используйте опцию «Принудительно указать папку с игрой».");
            Russian.Add("anotherInstanceRunning", "Запущен ещё один процесс Relhax Manager");
            Russian.Add("closeInstanceRunningForUpdate", "Пожалуйста, закройте ВСЕ запущенные процессы Relhax Manager перед тем, как мы сможем продолжить работу и обновление.");
            Russian.Add("skipUpdateWarning", "Вы пропустили обновление. Совместимость базы данных не гарантирована.");
            Russian.Add("patchDayMessage", "В настоящее время модпак недоступен в связи с выходом патча, как и обновлений модификаций. Приносим извинения за неудобства.\nЕсли вы работаете с базой данных модпака, то добавьте аргумент командной строки.");
            Russian.Add("configNotExist", "{0} НЕ существует, запуск в обычном режиме");
            Russian.Add("autoAndFirst", "Первый запуск не может быть произведён в режиме автоматической установкой, запуск в обычном режиме.");
            Russian.Add("confirmUninstallHeader", "Подтверждение");
            Russian.Add("confirmUninstallMessage", "Подтвердите необходимость удалить модификации World of Tanks в этой папке: \n\n{0}\n\nИспользуем метод «{1}»?");
            Russian.Add("uninstallingText", "Удаление...");
            Russian.Add("uninstallingFile", "Удаляется файл");
            Russian.Add("uninstallFinished", "Удаление модов завершено.");
            Russian.Add("uninstallFail", "Не удалось завершить деинсталляцию. Вы можете попробовать другой метод или отправить отчёт об ошибке.");
            Russian.Add("extractionErrorMessage", "Возникла ошибка при удалении папки res_mods или mods. Возможно, запущен World of Tanks или неверно настроены разрешения к папкам и файлам.");
            Russian.Add("extractionErrorHeader", Russian["error"]);
            Russian.Add("deleteErrorHeader", "Закройте папки");
            Russian.Add("deleteErrorMessage", "Закройте окна проводника, в которых открыты mods или res_mods (или глубже), и нажмите OK, чтобы продолжить.");
            Russian.Add("noUninstallLogMessage", "Файл со списком установленных файлов (installedRelhaxFiles.log) не существует. Хотите удалить все установленные моды в таком случае?");
            Russian.Add("noUninstallLogHeader", "Удалить все моды");
            Russian.Add("moveOutOfTanksLocation", "Модпак не может быть запущен из папки с игрой. Пожалуйста, переместите его в другую папку и попробуйте ещё раз.");
            Russian.Add("moveAppOutOfDownloads", "Приложение было запущено из папки «Загрузки». Мы не рекомендуем использовать эту папку, поскольку приложение создаёт несколько папок и файлов, " +
                "поиск которых может быть затруднительным в папке с загрузками. Вы должны переместить приложение и файлы/папки Relhax в другое расположение.");
            Russian.Add("DatabaseVersionsSameBody", "База данных не менялась с момента последней установки. В то же время не было обновлений к выбранным вами модификациям. Продолжить в любом случае?");
            Russian.Add("DatabaseVersionsSameHeader", "Одинаковые версии БД");
            Russian.Add("databaseNotFound", "По запрошенному адресу база данных не найдена");
            Russian.Add("detectedClientVersion", "Обнаруженная версия клиента");
            Russian.Add("supportedClientVersions", "Поддерживаемые версии клиента");
            Russian.Add("supportNotGuarnteed", "Эта версия клиента официально не поддерживается. Модификации могут не работать.\n"); // Nice idea, @Nullmaruzero. - DrWeb7_1
            Russian.Add("couldTryBeta", "Если недавно был выпущен патч, то команда разработчиков занята актуализацией модпака. Вы можете попробовать бета-версию БД.");
            Russian.Add("missingMSVCPLibrariesHeader", "Не удалось загрузить необходимые библиотеки");
            Russian.Add("missingMSVCPLibraries", "Не удалось загрузить библиотеку обработчика контурных иконок. Возможно, это признак того, что у вас отсутствует одна из DLL-библиотек Microsoft.");
            Russian.Add("openLinkToMSVCP", "Хотите открыть браузер, чтобы скачать установочный пакет?");
            Russian.Add("noChangeUntilRestart", "Для применения настроек потребуется перезапуск программы");
            Russian.Add("installBackupMods", "Создаётся бэкап модификации");
            Russian.Add("installBackupData", "Создаётся бэкап пользовательских данных");
            Russian.Add("installClearCache", "Удаляется кэш World of Tanks");
            Russian.Add("installClearLogs", "Удаляются файлы журнала");
            Russian.Add("installCleanMods", "Очищаются папки модификаций");
            Russian.Add("installExtractingMods", "Распаковывается пакет");
            Russian.Add("installZipFileEntry", "Файл");
            Russian.Add("installExtractingCompletedThreads", "Завершено потоков установки");
            Russian.Add("installExtractingOfGroup", "из установочной группы");
            Russian.Add("extractingUserMod", "Распаковывается пользовательский пакет");
            Russian.Add("installRestoreUserdata", "Восстанавливаются пользовательские данные");
            Russian.Add("installXmlUnpack", "Распаковка XML-файла");
            Russian.Add("installPatchFiles", "Применяется патч к файлу");
            Russian.Add("installShortcuts", "Создаются ярлыки");
            Russian.Add("installContourIconAtlas", "Создаётся файл-атлас");
            Russian.Add("installFonts", "Устанавливаются шрифты");
            Russian.Add("installCleanup", "Очистка...");
            Russian.Add("AtlasExtraction", "Распаковывается файл-атлас");
            Russian.Add("copyingFile", "Копирование файла");
            Russian.Add("deletingFile", "Удаление файла");
            Russian.Add("scanningModsFolders", "Сканируются папки модов...");
            Russian.Add("file", "Файл");
            Russian.Add("size", "Размер");
            Russian.Add("checkDatabase", "Идёт проверка БД на наличие неактуальных или ненужных файлов");
            Russian.Add("parseDownloadFolderFailed", "Не удалось обработать папку \"{0}\"");
            Russian.Add("installationFinished", "Установка завершена");
            Russian.Add("deletingFiles", "Удаляются файлы");
            Russian.Add("uninstalling", "Удаление...");
            Russian.Add("zipReadingErrorHeader", "Незаконченная загрузка");
            Russian.Add("zipReadingErrorMessage1", "ZIP-архив");
            Russian.Add("zipReadingErrorMessage3", "не может быть прочитан.");
            Russian.Add("patchingSystemDeneidAccessMessage", "Применение патча невозможно: нет доступа к папке с патчами. Попробуйте повторить операцию от имени администратора. Если вы снова видите это окно, то исправьте ошибки в правах доступа к файлам и папкам.");
            Russian.Add("patchingSystemDeneidAccessHeader", "В доступе отказано");
            Russian.Add("folderDeleteFailed", "Не удалось удалить папку");
            Russian.Add("fileDeleteFailed", "Не удалось удалить файл");
            Russian.Add("DeleteBackupFolder", "Бэкапы");
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            Russian.Add("installFailed", "Установка завершилась с ошибкой на следующем(их) этапе(ах)");
            #endregion

            #region Install finished window
            Russian.Add("InstallFinished", "Установка завершена");
            Russian.Add("InstallationCompleteText", "Установка завершена. Хотите...");
            Russian.Add("InstallationCompleteStartWoT", "Запустить игру? (WorldofTanks.exe)");
            Russian.Add("InstallationCompleteStartGameCenter", "Запустить Wargaming.net Game Center?"); // I'd rather support WGC than Steam. - DrWeb7_1
            Russian.Add("InstallationCompleteOpenXVM", "Открыть браузер на сайте XVM для активации статистики?");
            Russian.Add("InstallationCompleteCloseThisWindow", "Закрыть окно?");
            Russian.Add("InstallationCompleteCloseApp", "Закрыть приложение?");
            Russian.Add("xvmUrlLocalisation", "ru");
            Russian.Add("CouldNotStartProcess", "Не удалось запустить процесс");
            #endregion

            #region Diagnostics
            Russian.Add("Diagnostics", "Диагностика");
            Russian.Add("DiagnosticsMainTextBox", "Вы можете попробовать способы ниже для диагностики или выявления проблем с клиентом игры.");
            Russian.Add("LaunchWoTLauncher", "Запустить лаунчер World of Tanks в режиме проверки целостности"); // DEPRECATED!!!1 - DrWeb7_1
            Russian.Add("CollectLogInfo", "Собрать файлы журнала в ZIP-архив для отчёта об ошибке");
            Russian.Add("CollectLogInfoButtonDescription", "Собрать все необходимые файлы журнала в одном ZIP-архиве.\nЭто упростит процесс создания отчёта об ошибке.");
            Russian.Add("DownloadWGPatchFilesText", "Скачать обновления для любой игры из WGC через HTTP");
            Russian.Add("DownloadWGPatchFilesButtonDescription", "Данный мастер позволит загрузить обновления для любой игры от Wargaming (WoT, WoWS, WoWP) через HTTP, чтобы вы смогли установить их позднее.\nПолезно при возникновении проблем с режимом P2P (торрентом), используемым Wargaming Game Center по умолчанию.");
            Russian.Add("SelectedInstallation", "Текущая папка с игрой:");
            Russian.Add("SelectedInstallationNone", "("+Russian["none"].ToLower()+")");
            Russian.Add("collectionLogInfo", "Идёт сбор log-файлов...");
            Russian.Add("startingLauncherRepairMode", "Запускаю WoTLauncher в режиме проверки целостности..."); // DEPRECATED!!!1 - DrWeb7_1
            Russian.Add("failedStartLauncherRepairMode", "Не удалось запустить WoTLauncher в режиме проверки целостности"); // See the comment to "startingLauncherRepairMode". - DrWeb7_1
            Russian.Add("failedCollectFile", "Не найден файл ");
            Russian.Add("failedCreateZipfile", "Не удалось создать ZIP-архив");
            Russian.Add("launcherRepairModeStarted", "Проверка целостности успешно начата"); // See the comment to "startingLauncherRepairMode". - DrWeb7_1
            Russian.Add("ClearDownloadCache", "Очистить кэш загрузок");
            Russian.Add("ClearDownloadCacheDatabase", "Удалить кэш базы данных");
            Russian.Add("ClearDownloadCacheDescription", "Удаление всех файлов в папке \"RelhaxDownloads\"");
            Russian.Add("ClearDownloadCacheDatabaseDescription", "Удаление XML-файла базы данных. Это приведёт к тому, что все скачанныые ZIP-архивы будут проверены на целостность.\nВсе повреждённые файлы будут загружены заново в случае, если вы ещё раз выберете моды, соотвествующие этим архивам.");
            Russian.Add("clearingDownloadCache", "Очищается кэш загрузок");
            Russian.Add("failedToClearDownloadCache", "Не удалось очистить кэш загрузок");
            Russian.Add("cleaningDownloadCacheComplete", "Кэш загрузок успешно очищен");
            Russian.Add("clearingDownloadCacheDatabase", "Удаляется кэшированный XML-файл БД");
            Russian.Add("failedToClearDownloadCacheDatabase", "Не удалось удалить кэшированный XML-файл базы данных");
            Russian.Add("cleaningDownloadCacheDatabaseComplete", "Кэшированный XML-файл базы данных успешно удалён");
            Russian.Add("ChangeInstall", "Изменить выбранную папку с World of Tanks");
            Russian.Add("ChangeInstallDescription", "Это изменит набор файлов отчёта об ошибках, добавляемых в ZIP-архив.");
            Russian.Add("zipSavedTo", "ZIP-архив успешно сохранён в: ");
            Russian.Add("selectFilesToInclude", "Выберите файлы для включения в отчёт об ошибке");
            Russian.Add("TestLoadImageLibraries", "Протестировать библиотеки обработки изображений-атласов");
            Russian.Add("TestLoadImageLibrariesButtonDescription", "Тест библиотек обработчика текстурных атласов");
            Russian.Add("loadingAtlasImageLibraries", "Загружаются библиотеки обработчика текстурных атласов");
            Russian.Add("loadingAtlasImageLibrariesSuccess", "Библиотеки успешно загружены.");
            Russian.Add("loadingAtlasImageLibrariesFail", "Не удалось загрузить библиотеки");
            Russian.Add("CleanupModFilesText", "Удалить некорректно установленные модификации");
            Russian.Add("CleanupModFilesButtonDescription", "Будут удалены все модификации, установленные в папках win32 и win64 во избежание конфликтов в процессе загрузки.");
            Russian.Add("cleanupModFilesCompleted", "Удаление модов завершено.");
            Russian.Add("CleanGameCacheText", "Очистить кэш игры");
            Russian.Add("cleanGameCacheProgress", "Удаляются файлы кэша игры");
            Russian.Add("cleanGameCacheSuccess", "Кэш игры успешно очищен");
            Russian.Add("cleanGameCacheFail", "Не удалось удалить файлы кэша игры");
            Russian.Add("TrimRelhaxLogfileText", "Обрезать лог-файл Relhax до последних трёх запусков");
            Russian.Add("trimRelhaxLogProgress", "Обрезка файла журнала Relhax...");
            Russian.Add("trimRelhaxLogSuccess", "Файл журнала Relhax успешно обрезан");
            Russian.Add("trimRelhaxLogFail", "Не удалось обрезать файл журнала Relhax");
            #endregion

            #region Wot Client install selection
            Russian.Add("WoTClientSelection", "Выберите клиент World of Tanks");
            Russian.Add("ClientSelectionsTextHeader", "Следующие клиенты World of Tanks были найдены автоматически");
            Russian.Add("ClientSelectionsCancelButton", Russian["cancel"]);
            Russian.Add("ClientSelectionsManualFind", "Выбрать вручную");
            Russian.Add("ClientSelectionsContinueButton", Russian["select"]);
            Russian.Add("AddPicturesZip", "Добавить файлы в ZIP-архив");
            Russian.Add("DiagnosticsAddSelectionsPicturesLabel", "Добавить какие-либо дополнительные файлы (файл предустановки, изображения, и т. д.)");
            Russian.Add("DiagnosticsAddFilesButton", "Добавить файлы");
            Russian.Add("DiagnosticsRemoveSelectedButton", "Удалить файл");
            Russian.Add("DiagnosticsContinueButton", Russian["ContinueButton"]);
            Russian.Add("cantRemoveDefaultFile", "Невозможно удалить файл, добавляемый по умолчанию.");
            #endregion

            #region Preview Window
            Russian.Add("Preview", "Предпросмотр");
            Russian.Add("noDescription", "Описание отсутствует");
            Russian.Add("noUpdateInfo", "Отсутствует информация об обновлении");
            Russian.Add("noTimestamp", "Нет метки с датой");
            Russian.Add("PreviewNextPicButton", "след.");
            Russian.Add("PreviewPreviousPicButton", "пред.");
            Russian.Add("DevUrlHeader", "Сайт разработчика");
            Russian.Add("dropDownItemsInside", "Элементов внутри");
            Russian.Add("popular", "популярный");
            Russian.Add("previewEncounteredError", "Возникла проблема в работе окна предпросмотра. Не удалось отобразить превью.");
            Russian.Add("popularInDescription", "Это популярный пакет");
            Russian.Add("controversialInDescription", "Это подозрительный пакет");
            Russian.Add("encryptedInDescription", "Этот пакет зашифрован.\nПроверка на вирусы невозможна");
            Russian.Add("fromWgmodsInDescription", "Портал WGMods (wgmods.net) используется в качестве источника для этого пакета");
            #endregion

            #region Developer Selection Window
            Russian.Add("DeveloperSelectionsViewer", "Просмотр наборов");
            Russian.Add("DeveloperSelectionsTextHeader", "Набор для загрузки");
            Russian.Add("DeveloperSelectionsCancelButton", Russian["cancel"]);
            Russian.Add("DeveloperSelectionsLocalFile", "Локальный файл");
            Russian.Add("DeveloperSelectionsContinueButton", "Выбрать");
            Russian.Add("failedToParseSelections", "Сбой обработки набора");
            Russian.Add("lastModified", "Последнее изменение");
            #endregion

            #region Advanced Installer Window
            Russian.Add("AdvancedProgress", "Прогресс расширенной установки");
            Russian.Add("PreInstallTabHeader", "Подготовка"); // "Prepairing", otherwise it's too long. @SigmaTel71 (DrWeb7_1)
            Russian.Add("ExtractionTabHeader", "Распаковка");
            Russian.Add("PostInstallTabHeader", "Завершение"); // See comment to PreInstallTabHeader. @SigmaTel71 (DrWeb7_1)
            Russian.Add("AdvancedInstallBackupMods", "Бэкап имеющихся модов");
            Russian.Add("AdvancedInstallBackupData", "Бэкап данных модов");
            Russian.Add("AdvancedInstallClearCache", "Очистить кэш WoT");
            Russian.Add("AdvancedInstallClearLogs", "Очистить логи");
            Russian.Add("AdvancedInstallClearMods", "Удалить старые моды");
            Russian.Add("AdvancedInstallInstallMods", "Поток установки");
            Russian.Add("AdvancedInstallUserInstallMods", "Пользовательские моды");
            Russian.Add("AdvancedInstallRestoreData", "Восстановление данных");
            Russian.Add("AdvancedInstallXmlUnpack", "XML-распаковщик");
            Russian.Add("AdvancedInstallPatchFiles", "Патч файла");
            Russian.Add("AdvancedInstallCreateShortcuts", "Создать ярлыки");
            Russian.Add("AdvancedInstallCreateAtlas", "Создать атласы");
            Russian.Add("AdvancedInstallInstallFonts", "Установить шрифты");
            Russian.Add("AdvancedInstallTrimDownloadCache", "Очистить кэш загрузок");
            Russian.Add("AdvancedInstallCleanup", "Очистка");
            #endregion

            #region News Viewer
            Russian.Add("NewsViewer", "Информация об обновлениях"); // Keeping it consistent with ViewNewsButton - DrWeb7_1
            Russian.Add("application_Update_TabHeader", "Изменения в приложении");
            Russian.Add("database_Update_TabHeader", "Изменения в БД");
            Russian.Add("ViewNewsOnGoogleTranslate", "Посмотреть через переводчик Google");
            #endregion

            #region Loading Window
            Russian.Add("ProgressIndicator", "Загрузка");
            Russian.Add("LoadingHeader", "Загрузка, пожалуйста, подождите");
            #endregion

            #region First Load Acknowledgements
            Russian.Add("FirstLoadAcknowledgments", "Первый запуск");
            Russian.Add("AgreementLicense", "Я прочитал и согласен с ");
            Russian.Add("LicenseLink", "условиями лицензионного соглашения");
            Russian.Add("AgreementSupport1", "Я понимаю, что могу обратиться за помощью ");
            Russian.Add("AgreementSupportDiscord", "Discord");
            Russian.Add("AgreementHoster", "Я понимаю, что Relhax является площадкой хостинга модификаций и сервисом их установки и то, что Relhax не занимается разработкой каждой из них.");
            Russian.Add("AgreementAnonData", "Я понимаю, что Relhax V2 собирает анонимные сведения об использовании для улучшения приложения и знаю, что могу отключить сбор данных в разделе расширенных настроек");
            Russian.Add("V2UpgradeNoticeText", "Похоже, что вы производите апгрейд с V1 на V2 в первый раз.\nПосле нажатия кнопки \"Продолжить\" будет произведено необратимое обновление структуры файлов. Рекомендуется создание бэкапа папки с V1 перед продолжением");
            Russian.Add("upgradingStructure", "Обновление файлов и папок первой версии");
            #endregion

            #region Export Mode
            Russian.Add("ExportModeSelect", "Выберите клиент WoT для экспорта");
            Russian.Add("selectLocationToExport", "Выберите папку для экспорта устанавливаемых модов");
            Russian.Add("ExportSelectVersionHeader", "Выберите версию клиента игры, для которой будет произведён экспорт");
            Russian.Add("ExportContinueButton", Russian["ContinueButton"]);
            Russian.Add("ExportCancelButton", Russian["cancel"]);
            Russian.Add("ExportModeMajorVersion", "Онлайн-версия папки");
            Russian.Add("ExportModeMinorVersion", "Версия WoT");
            #endregion

            #region Asking to close WoT
            Russian.Add("AskCloseWoT", "Обнаружена запущенная игра!");
            Russian.Add("WoTRunningTitle", "World of Tanks запущен");
            Russian.Add("WoTRunningHeader", "Похоже, сейчас открыта папка с клиентом игры. Закройте её перед тем, как продолжить");
            Russian.Add("WoTRunningCancelInstallButton", "Отменить установку");
            Russian.Add("WoTRunningRetryButton", "Перепроверить");
            Russian.Add("WoTRunningForceCloseButton", "Принудительно закрыть");
            #endregion

            #region Scaling Confirmation
            Russian.Add("ScalingConfirmation", "Принять изменения");
            Russian.Add("ScalingConfirmationHeader", "Параметры масштабирования изменены. Хотите сохранить их?");
            Russian.Add("ScalingConfirmationRevertTime", "Отмена изменений через {0} сек.");
            Russian.Add("ScalingConfirmationKeep", "Сохранить");
            Russian.Add("ScalingConfirmationDiscard", "Отменить");
            #endregion

            #region Game Center download utility
            Russian.Add("GameCenterUpdateDownloader", "Мастер загрузки обновлений из Game Center");
            Russian.Add("GcDownloadStep1Header", "Выберите клиенты игры");
            Russian.Add("GcDownloadStep1TabDescription", "Выберите клиент игры, установленной через Game Center (WoT, WoWS, WoWP)");
            Russian.Add("GcDownloadStep1SelectClientButton", "Выбрать клиент");
            Russian.Add("GcDownloadStep1CurrentlySelectedClient", "Выбран клиент {0}");
            Russian.Add("GcDownloadStep1NextText", Russian["next"]);
            Russian.Add("GcDownloadStep1GameCenterCheckbox", "Проверить наличие обновлений для WGC вместо игры");
            Russian.Add("GcDownloadSelectWgClient", "Выбрать клиент (WGC)");
            Russian.Add("ClientTypeValue", "Нет данных");
            Russian.Add("LangValue", Russian["ClientTypeValue"]);
            Russian.Add("GcMissingFiles", "У выбранного вами клиента отсутствуют следующие XML-файлы");
            Russian.Add("GcDownloadStep2Header", "Закрыть Game Center");
            Russian.Add("GcDownloadStep2TabDescription", "Закройте Wargaming Game Center (приложение обнаружит, что он закрылся)");
            Russian.Add("GcDownloadStep2GcStatus", "На данный момент Game Center {0}");
            Russian.Add("GcDownloadStep2GcStatusOpened", "запущен");
            Russian.Add("GcDownloadStep2GcStatusClosed", "не запущен"); // it's not "closed", it's not a shopping center. Typical Russian. - DrWeb7_1
            Russian.Add("GcDownloadStep2PreviousText", Russian["previous"]);
            Russian.Add("GcDownloadStep2NextText", Russian["next"]);
            Russian.Add("GcDownloadStep3Header", "Получить сведения об обновлениях");
            Russian.Add("GcDownloadStep3TabDescription", "Получение сведений о списке файлов патчей, необходимых для загрузки");
            Russian.Add("GcDownloadStep3NoFilesUpToDate", "Вы используете актуальную версию игры.");
            Russian.Add("GcDownloadStep3PreviousText", Russian["previous"]);
            Russian.Add("GcDownloadStep3NextText", Russian["next"]);
            Russian.Add("GcDownloadStep4Header", "Скачать файлы обновлений");
            Russian.Add("GcDownloadStep4TabDescription", "Обновление клиента игры: получение обновлений");
            Russian.Add("GcDownloadStep4DownloadingCancelButton", Russian["cancel"]);
            Russian.Add("GcDownloadStep4DownloadingText", "Скачивание обновления {0} из {1}: {2}");
            Russian.Add("GcDownloadStep4DownloadComplete", "Получение обновлений завершено");
            Russian.Add("GcDownloadStep4PreviousText", Russian["previous"]);
            Russian.Add("GcDownloadStep4NextText", Russian["next"]);
            Russian.Add("GcDownloadStep5Header", "Готово!");
            Russian.Add("GcDownloadStep5TabDescription", "Процесс успешно завершён. Wargaming Game Center должен обнаружить файлы после запуска.");
            Russian.Add("GcDownloadStep5CloseText", Russian["close"]);
            Russian.Add("FirstLoadSelectLanguage", "Язык интерфейса");
            Russian.Add("SelectLanguageHeader", "Пожалуйста, выберите язык интерфейса");
            Russian.Add("SelectLanguagesContinueButton", Russian["ContinueButton"]);
            Russian.Add("Credits", "Авторы Relhax Modpack");
            Russian.Add("creditsProjectLeader", "Руководитель проекта");
            Russian.Add("creditsDatabaseManagers", "Операторы базы данных");
            Russian.Add("creditsTranslators", "Локализация");
            Russian.Add("creditsusingOpenSourceProjs", "В Relhax Modpack применяются следующие проекты с открытым исходным кодом");
            Russian.Add("creditsSpecialThanks", "Особая благодарность");
            Russian.Add("creditsGrumpelumpf", "руководитель проекта OMC Modpack, позволивший нам работать над Relhax, когда он отошёл от дел");
            Russian.Add("creditsRkk1945", "первый бета-тестер, кто работал со мной месяцами, чтобы привести проект в работоспособный вид");
            Russian.Add("creditsRgc", "за продвижение модпака, а так же за то, что стали первой группой бета-тестеров");
            Russian.Add("creditsBetaTestersName", "Нашей команде бета-тестеров");
            Russian.Add("creditsBetaTesters", "за тщательное тестирование и отправку отчётов об ошибках перед выходом в релиз");
            Russian.Add("creditsSilvers", "за помощь в работе с сообществом");
            Russian.Add("creditsXantier", "за помощь в настройке сервера в первые дни");
            Russian.Add("creditsSpritePacker", "за разработку алгоритма упаковки спрайтов и портирование на .NET");
            Russian.Add("creditsWargaming", "за создание легко автоматизируемой системы модификаций");
            Russian.Add("creditsUsersLikeU", "Таким пользователям, как вы");
            #endregion

            #region Conflicting Packages Dialog
            Russian.Add("ConflictingPackageDialog", "Обнаружен конфликт пакетов");
            Russian.Add("conflictingPackageMessageOptionA", "Вариант A");
            Russian.Add("conflictingPackageMessageOptionB", "Вариант B");
            Russian.Add("conflictingPackageMessagePartA", "Вы выбрали пакет \"{0}\", но он конфликтует со следующими пакетами:");
            Russian.Add("conflictingPackagePackageOfCategory", "- {0}, категория {1}");
            Russian.Add("conflictingPackageMessagePartB", Russian["conflictingPackageMessageOptionA"] + ": установить \"{0}\", и отказаться от установки всех конфликтующих пакетов");
            Russian.Add("conflictingPackageMessagePartC", Russian["conflictingPackageMessageOptionB"] + ": не устанавливать \"{0}\", сохранив конфликтующие пакеты");
            Russian.Add("conflictingPackageMessagePartD", "Закрытие окна равносильно выбору варианта B.");
            Russian.Add("conflictingPackageMessagePartE", "Выберите действие");
            #endregion

            #region End of Life announcement
            Russian.Add("EndOfLife", "Прекращение поддержки Relhax");
            Russian.Add("CloseWindowButton", Russian["close"]);
            Russian.Add("WoTForumAnnouncementsTextBlock", "Из анонса на форуме WoT:");
            Russian.Add("endOfLifeMessagePart1", "20.04.2022 сервера Relhax Modpack были выключены. Я хочу лично поблагодарить всю нашу команду и пользователей за успешную работу на протяжении более пяти лет!");
            Russian.Add("endOfLifeMessagePart2a", "01.01.2017 я поставил себе необычную задачу не только воссоздать модпак OMC с современным интерфейсом, но и создать самую быструю систему установки пакетов в сравнении с другими существующими модпаками.");
            Russian.Add("endOfLifeMessagePart2b", "На первых порах команда состояла из четырёх человек, трое из которых были члены OMC, желавшие внести свой вклад в проект. На протяжении четырёх лет я проектировал, собирал и переделывал приложение модпака с нуля, тратя на это десятки тысяч часов.");
            Russian.Add("endOfLifeMessagePart2c", "В какой-то момент команда выросла до восьми человек почти со всех регионов World of Tanks. Во время разработки я развивался как программист, узнавал о типичных практиках при разработке программ, в итоге сфокусировался на многопоточности приложения и обработке параллельных операций.");
            Russian.Add("endOfLifeMessagePart2d", "Я получал опыт в процессе работы над проектом, и стал взаимодействовать с впечатляющим сообществом мододелов. Это позволило внести свою лепту в Relic Gaming Community — в группу, к которой я присоединился в 2014 году.");
            Russian.Add("endOfLifeMessagePart3a", "В этом году, наконец, я закончил проектирование наболее оптимизированного и эффективного установщика, которого я мог создать для сообщества.");
            Russian.Add("endOfLifeMessagePart3b", "Видя, что поставленная задача выполнена, а мой интерес к игре (и проекту) сокращается, я решил закрыть проект.");
            Russian.Add("endOfLifeMessagePart3c", "Это было тяжёлое решение, но я не хотел поддерживать проект, к которому у меня больше нет интереса.");
            Russian.Add("endOfLifeMessagePart3d", "Я думаю, что это плохо бы отразилось на качестве приложения, и это было бы несправедливо по отношению к конечным пользователям. Я хотел закрыть проект, пока он всё ещё в жизнеспособном состоянии.");
            Russian.Add("endOfLifeMessagePart4", "И ещё раз, спасибо всем вам. За более пяти лет веселья, по которым я буду скучать.");
            #endregion
        }
    }
}
