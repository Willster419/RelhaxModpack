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
        /// Loads all English translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsEnglish()
        {
            //Syntax is as follows:
            //languageName.Add("componentName","TranslatedString");

            #region General expressions
            // Confirm "yes" button used in multiple places.
            // REFERRERS: VersionInfoYesText;
            English.Add("yes", "yes");

            // Decline button used in multiple places.
            // REFERRERS: VersionInfoNoText;
            English.Add("no", "no");

            // Cancel button (phrase) used in multiple places.
            // REFERRERS: CancelDownloadInstallButton; CancelButtonLabel; DeveloperSelectionsCancelButton; ExportCancelButton; GcDownloadStep4DownloadingCancelButton;
            English.Add("cancel", "Cancel");
            English.Add("delete", "Delete");
            English.Add("warning", "WARNING");
            English.Add("critical", "CRITICAL");
            English.Add("select", "Select");
            English.Add("abort", "Abort");

            // REFERRERS: extractionErrorHeader; GcDownloadStep1ValueError;
            English.Add("error", "Error");
            English.Add("retry", "Retry");
            English.Add("ignore", "Ignore");
            English.Add("lastUpdated", "Last Updated: ");
            English.Add("stepsComplete", "tasks completed");

            //Component: allFiles
            //
            English.Add("allFiles", "All files");

            //Component: GoogleTranslateLanguageKey
            //
            English.Add("GoogleTranslateLanguageKey", "en");

            //Component: at
            //example: downloading file x "at" 10MB/s
            English.Add("at", "at");

            //Component: seconds
            // Used for download progress. Remaining time. Example: X minutes, Y seconds.
            English.Add("seconds", "seconds");

            //Component: minutes
            // Used for download progress (remaining time) AND AutoSyncFrequencyComboBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("minutes", "minutes");

            //Component: hours @ AutoSyncFrequencyComboBox
            // Used for AutoSyncCheckFrequencyTextBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("hours", "hours");

            //Component: days @ AutoSyncFrequencyComboBox
            // Used for AutoSyncFrequencyComboBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("days", "days");

            //Component: next
            // Used as a button in multiple places.
            // REFERRERS: PreviewNextPicButton; GcDownloadStep1NextText; GcDownloadStep2NextText; GcDownloadStep3NextText; GcDownloadStep4NextText;
            English.Add("next", "Next");

            //Component: ContinueButton
            // Continue button used in multiple places (at least this particular translation of it). Sometimes used as NEXT STEP, sometimes as NEXT ELEMENT.
            // REFERRERS: DiagnosticsContinueButton; ExportContinueButton; SelectLanguagesContinueButton;
            English.Add("ContinueButton", "Continue");

            //Component: previous
            // Previous button used in multiple places (at least this particular translation of it). Sometimes used as GO BACK, sometimes as PREVIOUS ELEMENT.
            // REFERRERS: PreviewPreviousPicButton; GcDownloadStep1PreviousText; GcDownloadStep2PreviousText; GcDownloadStep3PreviousText; GcDownloadStep4PreviousText;
            English.Add("previous", "Previous");

            //Component: close
            // Close button used in multiple places.
            // REFFERERS: GcDownloadStep5CloseText; CloseWindowButton
            English.Add("close", "Close");

            //Component: none
            // REFERRERS: SelectedInstallationNone;
            English.Add("none", "None");
            #endregion

            #region Application messages
            //Component: appFailedCreateLogfile
            //When the application first starts, it tries to open a logfile
            English.Add("appFailedCreateLogfile", "The application failed to open a logfile. Check your file permissions or move the application to a folder with write access.");

            //Component: failedToParse
            //
            English.Add("failedToParse", "Failed to parse the file");

            //Component: failedToGetDotNetFrameworkVersion
            //
            English.Add("failedToGetDotNetFrameworkVersion", "Failed to get the installed .NET Framework version. This could indicate a permissions problem or your antivirus software could be blocking it.");

            //Component: invalidDotNetFrameworkVersion
            //
            English.Add("invalidDotNetFrameworkVersion", "The installed version of the .NET Framework is less then 4.8. Relhax Modpack requires version 4.8 or above to operate. Would you like to open a link" +
                "to get the latest version of the .NET Framework?");
            #endregion

            #region Tray Icon
            //Component: MenuItemRestore
            //The menu item for restoring the application
            English.Add("MenuItemRestore", "Restore");

            //Component: MenuItemCheckUpdates
            //The menu item for restoring the application
            English.Add("MenuItemCheckUpdates", "Check for Updates");

            //Component: MenuItemAppClose
            //The menu item for restoring the application
            English.Add("MenuItemAppClose", English["close"]);

            //Component: newDBApplied
            //MessageBox for when a new database version is applied
            English.Add("newDBApplied", "New database version applied");
            #endregion

            #region Main Window
            //Component: InstallModpackButton
            //The button for installing the modpack
            English.Add("InstallModpackButton", "Start mod selection");

            //Component: manualWoTFind
            // File picker window asking the user to select WorldOfTanks.exe manually.
            English.Add("selectWOTExecutable", "Select your WOT executable (WorldOfTanks.exe)");

            //Component: InstallModpackButtonDescription
            //
            English.Add("InstallModpackButtonDescription", "Select the mods you want to install to your WoT client");

            //Component: UninstallModpackButton
            //
            English.Add("UninstallModpackButton", "Uninstall Relhax Modpack");

            //Component: UninstallModpackButtonDescription
            //
            English.Add("UninstallModpackButtonDescription", "Remove *all* mods installed to your WoT client");

            //Component: ViewNewsButton
            //
            English.Add("ViewNewsButton", "View update news");

            //Component: ViewNewsButtonDescription
            //
            English.Add("ViewNewsButtonDescription", "View application, database, and other update news");

            //Component: ForceManuelGameDetectionText
            //
            English.Add("ForceManuelGameDetectionText", "Force manual game detection");

            //Component: ForceManuelGameDetectionCBDescription
            //
            English.Add("ForceManuelGameDetectionCBDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");

            //Component: LanguageSelectionTextblock
            // A label for a ComboButton below with available languages.
            English.Add("LanguageSelectionTextblock", "Language selection");

            //Component: LanguageSelectionTextblockDescription
            // A tooltip for LanguageSelectionTextblock button label.
            English.Add("LanguageSelectionTextblockDescription", "Select your prefered language.\nIf you encounter missing translations or mistakes, feel free to inform us about them.");

            //Component: Forms_ENG_NAButtonDescription
            English.Add("Forms_ENG_NAButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the NA server");

            //Component: FormsENG_EUButtonDescription
            English.Add("Forms_ENG_EUButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the EU server");

            //Component: FormsENG_GERButtonDescription
            English.Add("Forms_GER_EUButtonDescription", "Go to the German-speaking 'World of Tanks' forum page for the EU server");

            //Component: SaveUserDataText
            //
            English.Add("SaveUserDataText", "Save user data");

            //Component:SaveUserDataCBDescription
            //
            English.Add("SaveUserDataCBDescription", "If this is selected, the installer will save user created data (like session stats from previous battles)");


            //Component: CleanInstallText
            //
            English.Add("CleanInstallText", "Clean installation (recommended)");

            //Component: CleanInstallCBDescription
            //
            English.Add("CleanInstallCBDescription", "This recommended option will uninstall your previous installation before installing the new one.");

            //Component: BackupModsText
            //
            English.Add("BackupModsText", "Backup current mods folder");

            //Component: BackupModsSizeLabelUsed
            //
            English.Add("BackupModsSizeLabelUsed", "Backups: {0}  Size: {1}");

            //Component: backupModsSizeCalculating
            //
            English.Add("backupModsSizeCalculating", "Calculating backups size...");

            //Component: BackupModsCBDescription
            //
            English.Add("BackupModsCBDescription", "When enabled, the installer will backup your current mods installation folders to the specified location");

            //Component: BackupModsSizeLabelUsedDescription
            //
            English.Add("BackupModsSizeLabelUsedDescription", English["BackupModsCBDescription"]);

            //Component: SaveLastInstallText
            //
            English.Add("SaveLastInstallText", "Save selection of last install");

            //Component: SaveLastInstallCBDescription
            //
            English.Add("SaveLastInstallCBDescription", "When enabled, the installer will automatically apply your last used selection");

            //Component: MinimizeToSystemTrayText
            //
            English.Add("MinimizeToSystemTrayText", "Minimize to system tray");

            //Component: MinimizeToSystemTrayDescription
            //
            English.Add("MinimizeToSystemTrayDescription", "When checked, the application will continue to run in the system tray when you press close");

            //Component: VerboseLoggingText
            //
            English.Add("VerboseLoggingText", "Verbose Logging");

            //Component: VerboseLoggingCBDescription
            //
            English.Add("VerboseLoggingCBDescription", "Enable more logging messages in the log file. Useful for reporting bugs");

            //Component: AllowStatsGatherText
            //
            English.Add("AllowStatsGatherText", "Allow statistics gathering of mod usage");

            //Component: AllowStatsGatherCBDescription
            //
            English.Add("AllowStatsGatherCBDescription", "Allow the installer to upload anonymous statistic data to the server about mod selections. This allows us to prioritize our support");

            //Component: DisableTriggersText
            //
            English.Add("DisableTriggersText", "Disable Triggers");

            //Component: DisableTriggersCBDescription
            //
            English.Add("DisableTriggersCBDescription", "Allowing triggers can speed up the installation by running some tasks (like creating contour icons) during extraction " +
                "after all required resources for that task are ready. This is turned off automatically if User Mods are detected");

            //Component: CancelDownloadInstallButton
            //
            English.Add("CancelDownloadInstallButton", English["cancel"]);


            //Component: appDataFolderNotExist
            //
            English.Add("appDataFolderNotExist", "The installer could not detect the WoT cache folder. Continue the installation without clearing WoT cache?");

            //Component: viewAppUpdates
            //
            English.Add("viewAppUpdates", "View latest application updates");

            //Component: viewDBUpdates
            //
            English.Add("viewDBUpdates", "View latest database updates");

            //Component: EnableColorChangeDefaultV2Text
            //
            English.Add("EnableColorChangeDefaultV2Text", "Enable color change");

            //Component: EnableColorChangeDefaultV2CBDescription
            //
            English.Add("EnableColorChangeDefaultV2CBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");


            //Component: EnableColorChangeLegacyText
            //
            English.Add("EnableColorChangeLegacyText", "Enable color change");

            //Component: EnableColorChangeLegacyCBDescription
            //
            English.Add("EnableColorChangeLegacyCBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");

            //Component: ShowOptionsCollapsedLegacyText
            //
            English.Add("ShowOptionsCollapsedLegacyText", "Show options collapsed on start");

            //Component: ShowOptionsCollapsedLegacyCBDescription
            //
            English.Add("ShowOptionsCollapsedLegacyCBDescription", "When checked, all options in the selection list (except at the category level) will be collapsed.");


            //Component: ClearLogFilesText
            //
            English.Add("ClearLogFilesText", "Clear log files");

            //Component: ClearLogFilesCBDescription
            //
            English.Add("ClearLogFilesCBDescription", "Clear the WoT log files, (python.log), as well as xvm log files (xvm.log) and pmod log files (pmod.log)");


            //Component: CreateShortcutsText
            //
            English.Add("CreateShortcutsText", "Create desktop shortcuts");

            //Component: CreateShortcutsCBDescription
            //
            English.Add("CreateShortcutsCBDescription", "When selected, it will create shortcut icons on your desktop for mods that are exe files (like WWIIHA configuration)");

            //Component: DeleteOldPackagesText
            //
            English.Add("DeleteOldPackagesText", "Delete old package files");

            //Component: DeleteOldPackagesCBDescription
            //
            English.Add("DeleteOldPackagesCBDescription", "Delete any zip files that are no longer used by the installer in the \"RelhaxDownloads\" folder to free up disk space");

            //Component: MinimalistModeText
            //
            English.Add("MinimalistModeText", "Minimalist Mode");

            //Component: MinimalistModeCBDescription
            //
            English.Add("MinimalistModeCBDescription", "When checked, the modpack will exclude certain packages from installation that are not required, like the modpack button or Relhax theme files.");

            //Component: AutoInstallText
            //
            English.Add("AutoInstallText", "Enable auto install");

            //Component: AutoInstallCBDescription
            //
            English.Add("AutoInstallCBDescription", "When a selection file and time is set below, the installer will automatically check for updates to your mods and apply them");

            //Component: OneClickInstallText
            //
            English.Add("OneClickInstallText", "Enable one-click install");

            //Component: AutoOneclickShowWarningOnSelectionsFailText
            //
            English.Add("AutoOneclickShowWarningOnSelectionsFailText", "Show warning if selection document has errors when loaded");

            //Component: ForceEnabledText
            //
            English.Add("ForceEnabledText", "Force all packages enabled [!]");

            //Component: ForceEnabledCBDescription
            //
            English.Add("ForceEnabledCBDescription", "Causes all packages to be enabled. Can lead to severe stability issues of your installation");

            //Component: ForceVisibleText
            //
            English.Add("ForceVisibleText", "Force all packages visible [!]");

            //Component: ForceVisibleCBDescription
            //
            English.Add("ForceVisibleCBDescription", "Causes all packages to be visible. Can lead to severe stability issues of your installation");

            //Component: LoadAutoSyncSelectionFileText
            // A button opening a OpenFileDialong system window for selecting the mod selection file. Used for One-Click and Auto-Install features.
            English.Add("LoadAutoSyncSelectionFileText", "Load selection file");

            //Component: LoadAutoSyncSelectionFileText
            // A tooltip for LoadAutoSyncSelectionFileText button.
            English.Add("LoadAutoSyncSelectionFileDescription", "Load your mods selection file to use the One-Click and Auto-Install features.");

            //Component: AutoSyncCheckFrequencyTextBox
            // A label followed by a small text field asking for update check interval (time unit selected via a drop-list at the end of the line).
            // Frequency: every [ ... ] {Minutes / Hours / Days}
            English.Add("AutoSyncCheckFrequencyTextBox", "Frequency: every");

            //Component: DeveloperSettingsHeader
            // A header-label for multiple checkboxes with developer options below.
            English.Add("DeveloperSettingsHeader", "Developer Settings [!]");

            //Component: DeveloperSettingsHeaderDescription
            // A tooltip for DeveloperSettingsHeader.
            English.Add("DeveloperSettingsHeaderDescription", "The options below may cause problems and stability issues!\nPlease, don't use them unless you know what you're doing!");

            //Component: ApplyCustomScalingText
            //
            English.Add("ApplyCustomScalingText", "Application Scaling");

            //Component: ApplyCustomScalingTextDescription
            // A tooltip for a ApplyCustomScalingText label. Describes a slider below used for changing application scaling.
            English.Add("ApplyCustomScalingTextDescription", "Apply display scaling to the installer windows");

            //Component: EnableCustomFontCheckboxText
            //
            English.Add("EnableCustomFontCheckboxText", "Enable custom font");

            //Component: EnableCustomFontCheckboxTextDescription
            //
            English.Add("EnableCustomFontCheckboxTextDescription", "Enable using a custom font installed on your system inside most application windows");

            //Component: LauchEditorText
            //button for launching the editor from the main application window
            English.Add("LauchEditorText", "Launch Database Editor");

            //Component: LauchEditorDescription
            //button for launching the editor from the main application window
            English.Add("LauchEditorDescription", "Launch the Database Editor from here, instead of from command line");

            //Component: LauchPatchDesignerText
            //button for launching the editor from the main application window
            English.Add("LauchPatchDesignerText", "Launch Patch Designer");

            //Component: LauchPatchDesignerDescription
            //button for launching the editor from the main application window
            English.Add("LauchPatchDesignerDescription", "Launch the Patch Designer from here, instead of from command line");

            //Component: LauchAutomationRunnerText
            //button for launching the automation runner from the main application window
            English.Add("LauchAutomationRunnerText", "Launch Automation Runner");

            //Component: LauchAutomationRunnerDescription
            //button for launching the editor from the main application window
            English.Add("LauchAutomationRunnerDescription", "Launch the Automation Runner from here, instead of from command line");

            //Component: InstallWhileDownloadingText
            //
            English.Add("InstallWhileDownloadingText", "Extract while downloading");

            //Component: InstallWhileDownloadingCBDescription
            //
            English.Add("InstallWhileDownloadingCBDescription", "When enabled, the installer will extract a zip file as soon as it is downloaded," +
                " rather than waiting for every zip file to be downloaded before extraction.");

            //Component: MulticoreExtractionCoresCountLabel
            //
            English.Add("MulticoreExtractionCoresCountLabel", "Detected Cores: {0}");

            //Component: MulticoreExtractionCoresCountLabelDescription
            //
            English.Add("MulticoreExtractionCoresCountLabelDescription", "Number of logical CPU cores (threads) detected on your system");

            //Component: SaveDisabledModsInSelectionText
            //
            English.Add("SaveDisabledModsInSelectionText", "Keep disabled mods when saving selection");

            //Component: SaveDisabledModsInSelectionDescription
            //
            English.Add("SaveDisabledModsInSelectionDescription", "When a mod is re-enabled, it will be selected from your selection file");

            //Component: AdvancedInstallationProgressText
            //
            English.Add("AdvancedInstallationProgressText", "Show advanced installation progress window");

            //Component: AdvancedInstallationProgressDescription
            //
            English.Add("AdvancedInstallationProgressDescription", "Shows an advanced installation window during extraction, useful when you have multicore extraction enabled");

            //Component: ThemeSelectText
            // A label with 3 radio buttons underneach for selecting app's color theme.
            English.Add("ThemeSelectText", "Select theme:");

            //Component: ThemeDefaultText
            //
            English.Add("ThemeDefaultText", "Default");

            //Component: ThemeDefaultDescription
            //
            English.Add("ThemeDefaultDescription", "Default Theme");

            //Component: ThemeDarkText
            //
            English.Add("ThemeDarkText", "Dark");

            //Component: ThemeDarkDescription
            //
            English.Add("ThemeDarkDescription", "Dark Theme");

            //Component: MulticoreExtractionText
            //
            English.Add("MulticoreExtractionText", "Multicore extraction mode");

            //Component: MulticoreExtractionCBDescription
            //
            English.Add("MulticoreExtractionCBDescription", "The installer will use a parallel extraction method. It will extract multiple zip files at the same time," +
                " reducing install time. For SSD drives ONLY.");

            //Component: UninstallDefaultText
            //
            English.Add("UninstallDefaultText", "Default"); // Check //verify the uninstall @ 'UninstallModpackButton_Click' before changing!

            //Component: UninstallQuickText
            //
            English.Add("UninstallQuickText", "Quick"); // Check //verify the uninstall @ 'UninstallModpackButton_Click' before changing!

            //Component: ExportModeText
            //
            English.Add("ExportModeText", "Export Mode");

            //Component: ExportModeCBDescription
            //Explaining the export mode
            English.Add("ExportModeCBDescription", "Export mode will allow you to select a folder and WoT version you wish to export your mods installation to. For advanced users only." +
                "\nNote it will NOT: Unpack game xml files, patch files (provided from the game), or create the atlas files. Instructions can be found in the export directory.");

            //Component: ViewCreditsButtonText
            //
            English.Add("ViewCreditsButtonText", "View Credits");

            //Component: ViewCreditsButtonDescription
            //
            English.Add("ViewCreditsButtonDescription", "See all the awesome people and projects that support the modpack!");

            //Component: ExportWindowDescription
            //
            English.Add("ExportWindowDescription", "Select the version of WoT you wish to export for");

            //Component: HelperText
            // A large area in the middle of the default window and view for the modpack. Introduces the user to the modpack. You may format it using linebreaks -> \n
            English.Add("HelperText", "Welcome to the Relhax Modpack!" +
                "\nI have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over a setting to have it explained." +
                "\nThanks for using Relhax, I hope you enjoy it! - Willster419");

            //Component: helperTextShort
            //
            English.Add("helperTextShort", "Welcome to the Relhax Modpack!");

            //Component: NotifyIfSameDatabaseText
            //
            English.Add("NotifyIfSameDatabaseText", "Inform if no new database available (stable database only)");

            //Component: NotifyIfSameDatabaseCBDescription
            //
            English.Add("NotifyIfSameDatabaseCBDescription", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods." +
                " This only works with the stable database.");

            //Component: ShowInstallCompleteWindowText
            //
            English.Add("ShowInstallCompleteWindowText", "Show advanced install complete window");

            //Component: ShowInstallCompleteWindowCBDescription
            //
            English.Add("ShowInstallCompleteWindowCBDescription", "Show a window upon installation completion with popular operations to" +
                " perform after modpack installation, such as launching the game, going to the xvm website, etc.");

            //Component: applicationVersion
            //
            English.Add("applicationVersion", "Application Version");

            //Component: databaseVersion
            //
            English.Add("databaseVersion", "Latest Database");

            //Component: ClearCacheText
            //
            English.Add("ClearCacheText", "Clear WoT cache data");

            //Component: ClearCachCBDescription
            //
            English.Add("ClearCacheCBDescription", "Clear the WoT cache app data directory. Performs the same task as the similar option that was in OMC.");

            //Component: UninstallDefaultDescription
            // A radio button tooltip explaining the difference between this option and the option next (to the right) of it.
            English.Add("UninstallDefaultDescription", "Default Uninstall will remove all files in the game's mod directories, including shortcuts and appdata cache files.");

            //Component: UninstallQuickDescription
            // A radio button tooltip explaining the difference between this option and the option next (to the left) of it.
            English.Add("UninstallQuickDescription", "Quick Uninstall will only remove files in the game's mod directories. It does not remove modpack" +
                " created shortcuts or appdata cache files.");

            //Component: DiagnosticUtilitiesButton
            //
            English.Add("DiagnosticUtilitiesButton", "Diagnostic utilities");

            //Component: DiagnosticUtilitiesButtonDescription
            //
            English.Add("DiagnosticUtilitiesButtonDescription", "Report a bug, attempt a WG client repair, etc.");

            //Component: UninstallModeGroupBox
            //
            English.Add("UninstallModeGroupBox", "Uninstall Mode:");

            //Component: UninstallModeGroupBoxDescription
            English.Add("UninstallModeGroupBoxDescription", "Select the uninstall mode to use");

            //Component: FacebookButtonDescription
            English.Add("FacebookButtonDescription", "Go to our Facebook page");

            //Component: DiscordButtonDescription
            English.Add("DiscordButtonDescription", "Go to our Discord server");

            //Component: TwitterButtonDescription
            English.Add("TwitterButtonDescription", "Go to our Twitter page");

            //Component: SendEmailButtonDescription
            English.Add("SendEmailButtonDescription", "Send us an Email (No modpack support)");

            //Component: HomepageButtonDescription
            English.Add("HomepageButtonDescription", "Visit our Website");

            //Component: DonateButtonDescription
            English.Add("DonateButtonDescription", "Donation for further development");

            //Component: FindBugAddModButtonDescription
            English.Add("FindBugAddModButtonDescription", "Find a bug? Want a mod added? Report here please!");

            //Component: Mod Selection view Group Box
            //
            English.Add("SelectionViewGB", "Selection View");

            //Component: SelectionDefaultText
            //Mod selection view Default
            English.Add("SelectionDefaultText", "Default");

            //Component: SelectionLegacyText
            //Mod selection view legacy (OMC) [Now default]
            AddTranslationToAll("SelectionLegacyText", "OMC Legacy");

            //Component: Mod selection Description
            //
            English.Add("SelectionLayoutDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");

            //Component: Mod selection Description
            //
            English.Add("SelectionDefaultDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");

            //Component: Mod selection Description
            //
            English.Add("SelectionLegacyDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");

            //Component: LanguageSelectionGBDescription
            //
            English.Add("LanguageSelectionGBDescription", "Select your preferred language.");

            //Component: EnableBordersDefaultV2Text
            //
            English.Add("EnableBordersDefaultV2Text", "Enable borders");

            //Component: EnableBordersLegacyText
            //
            English.Add("EnableBordersLegacyText", "Enable borders");

            //Component: EnableBordersDefaultV2CBDescription
            //
            English.Add("EnableBordersDefaultV2CBDescription", "Enable the black borders around each mod and config sublevel.");


            //Component: EnableBordersLegacyCBDescription
            //
            English.Add("EnableBordersLegacyCBDescription", "Enable the black borders around each mod and config sublevel.");


            //Component: UseBetaDatabaseText
            English.Add("UseBetaDatabaseText", "Use beta database");

            //Component: UseBetaDatabaseCBDescription
            English.Add("UseBetaDatabaseCBDescription", "Use the latest beta database. Mod stability is not guaranteed");


            //Component: UseBetaApplicationText
            English.Add("UseBetaApplicationText", "Use beta application");

            //Component: UseBetaApplicationCBDescription
            English.Add("UseBetaApplicationCBDescription", "Use the latest beta application. Translations and application stability are not guaranteed");


            //Component: SettingsTabIntroHeader
            //
            English.Add("SettingsTabIntroHeader", "Welcome!");

            //Component: SettingsTabSelectionViewHeader
            //
            English.Add("SettingsTabSelectionViewHeader", "Selection View");

            //Component: SettingsTabInstallationSettingsHeader
            //
            English.Add("SettingsTabInstallationSettingsHeader", "Installation Settings");

            //Component: SettingsTabApplicationSettingsHeader
            //
            English.Add("SettingsTabApplicationSettingsHeader", "Application Settings");

            //Component: SettingsTabAdvancedSettingsHeader
            //
            English.Add("SettingsTabAdvancedSettingsHeader", "Advanced");

            //Component: MainWindowSelectSelectionFileToLoad
            //
            English.Add("MainWindowSelectSelectionFileToLoad", "Select selection file to load");

            //Component: verifyUninstallHeader
            //
            English.Add("verifyUninstallHeader", "Confirmation");

            //Component: verifyUninstallVersionAndLocation
            //
            English.Add("verifyUninstallVersionAndLocation", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");

            //Component: failedVerifyFolderStructure
            //When the application first starts, it tries to open a logfile
            English.Add("failedVerifyFolderStructure", "The application failed to create the required folder structure. Check your file permissions or move the application to a folder with write access.");

            //Component: failedToExtractUpdateArchive
            //Upon update download, if it can't extract the exe
            English.Add("failedToExtractUpdateArchive", "The application failed to extract the update files. Check your file permissions and antivirus application.");

            //Component: downloadingUpdate
            //
            English.Add("downloadingUpdate", "Downloading application update");

            //Component: AutoOneclickSelectionErrorsContinueBody
            //When loading the selection file for "one-click" or "auto-install" modes, if there are issues with loading the selection,
            //and the user selected to be informed of issues
            //(removed components, disabled components, etc.)
            English.Add("AutoOneclickSelectionErrorsContinueBody", "There were problems loading the selection file (most likely disabled/removed packages, etc.)" +
                "\nWould you like to continue anyway?");

            //Component: AutoOneclickSelectionErrorsContinueHeader
            //see above
            English.Add("AutoOneclickSelectionErrorsContinueHeader", "Problems loading selection file");

            //Component: noAutoInstallWithBeta
            //
            English.Add("noAutoInstallWithBeta", "Auto install mode cannot be used with the beta database");

            //Component: autoInstallWithBetaDBConfirmBody
            //
            English.Add("autoInstallWithBetaDBConfirmBody", "Auto install will be enabled with beta database. The beta database is updated frequenty and could result in multiple" +
                " installations in one day. Are you sure you would like to do this?");

            //Component: autoInstallWithBetaDBConfirmHeader
            //
            English.Add("autoInstallWithBetaDBConfirmHeader", English["verifyUninstallHeader"]);

            //Component: loadingBranches
            //"branch" is this context is git respoitory branches
            English.Add("loadingBranches", "Loading branches");

            //Component: failedToParseUISettingsFile
            //"branch" is this context is git respoitory branches
            English.Add("failedToParseUISettingsFile", "Failed to apply the theme. Check the log for details. Enable \"Verbose Logging\" for additional information.");

            //Component: UISettingsFileApplied
            //the message when the UISettings.xml file is parsed and the custom theme is loaded
            English.Add("UISettingsFileApplied", "Theme applied");

            //Component: failedToFindWoTExe
            //the message when the app cannot locate the WoT.exe
            English.Add("failedToFindWoTExe", "Failed to get the WoT client installation location. Please send a bug report to the developer.");

            //Component: failedToFindWoTVersionXml
            //the message when the UISettings.xml file is parsed and the custom theme is loaded
            English.Add("failedToFindWoTVersionXml", "Failed to get WoT client installation version information. Check if the file 'version.xml' exists in the 'World_of_Tanks' directory.");
            #endregion

            #region ModSelectionList
            //Component: ModSelectionList
            //
            English.Add("ModSelectionList", "Selection List");

            //Component: ContinueButtonLabel
            //
            English.Add("ContinueButtonLabel", "Install");

            //Component: CancelButtonLabel
            //
            English.Add("CancelButtonLabel", English["cancel"]);

            //Component: HelpLabel
            //
            English.Add("HelpLabel", "Right-click a selection component to see a preview window");

            //Component: LoadSelectionButtonLabel
            //
            English.Add("LoadSelectionButtonLabel", "Load selection");

            //Component: SaveSelectionButtonLabel
            //
            English.Add("SaveSelectionButtonLabel", "Save selection");

            //Component: SelectSelectionFileToSave
            //File save dialog box when the user presses 'save selection' in Mod Selection List
            English.Add("SelectSelectionFileToSave", "Save selection file");

            //Component: ClearSelectionsButtonLabel
            //
            English.Add("ClearSelectionsButtonLabel", "Clear selections");

            //Component: SearchThisTabOnlyCB
            // A checkbox under the search field SearchCB in mods selection window. Toggles the search engine to search only in user's currently active tab.
            English.Add("SearchThisTabOnlyCB", "Search in this tab only");

            //Component: searchComboBoxInitMessage
            // The first text value inside the SearchCB combo box when the window is launched
            English.Add("searchComboBoxInitMessage", "Search for a package...");

            //Component: SearchTBDescription
            // A tooltip for SearchTB. Describes the use of a wild character in the search query.
            English.Add("SearchTBDescription", "You can also search for multiple name parts, separated by a * (asterisk).\nFor example: config*willster419 will display" +
                " as search result: Willster419\'s Config");

            //Component: InstallingAsWoTVersion
            //
            English.Add("InstallingAsWoTVersion", "Installing as WoT version: {0}");

            //Component: UsingDatabaseVersion
            // A label in the package selection list (previously called mod selection list) to indicate the distribution version of the database currently in use
            English.Add("UsingDatabaseVersion", "Using database: {0} ({1})");

            //Component: userMods
            //
            English.Add("userMods", "User Mods");

            //Component: FirstTimeUserModsWarning
            //
            English.Add("FirstTimeUserModsWarning", "This tab is for selecting zip files you place in the \"RelhaxUserMods\" folder." + 
                " They must be zip files, and should use a root directory folder of the \"World_of_Tanks\" directory");

            //Component: downloadingDatabase
            //
            English.Add("downloadingDatabase", "Downloading database");

            //Component: readingDatabase
            //
            English.Add("readingDatabase", "Reading database");

            //Component: loadingUI
            //
            English.Add("loadingUI", "Loading UI");

            //Component: verifyingDownloadCache
            //
            English.Add("verifyingDownloadCache", "Verifying file integrity of ");

            //Component: InstallProgressTextBoxDescription
            //
            English.Add("InstallProgressTextBoxDescription", "Progress of an installation will be shown here");

            //Component: testModeDatabaseNotFound
            //
            English.Add("testModeDatabaseNotFound", "CRITICAL: TestMode Database not found at:\n{0}");


            //Component:
            //
            English.Add("duplicateMods", "CRITICAL: Duplicate package ID detected");


            //Component:
            //
            English.Add("databaseReadFailed", "CRITICAL: Failed to read database\n\nSee logfile for detailed info");


            //Component:
            //
            English.Add("configSaveSuccess", "Selection Saved Successfully");

            //Component:
            //
            English.Add("selectConfigFile", "Find a selection file to load");

            //Component:
            //
            English.Add("configLoadFailed", "The selection file could not be loaded, loading in standard mode");

            //Component:
            //
            English.Add("modNotFound", "The package (ID = \"{0}\") was not found in the database. It could have been renamed or removed.");

            //Component:
            //
            English.Add("modDeactivated", "The following packages are currently deactivated in the modpack and could not to be selected to install");

            //Component:
            //
            English.Add("modsNotFoundTechnical", "The following packages could not be found and were most likely removed");


            //Component:
            //
            English.Add("modsBrokenStructure", "The following packages were disabled due to modifications in the package structure. You need to re-check them if you want to install them");

            //Component: packagesUpdatedShouldInstall
            //
            English.Add("packagesUpdatedShouldInstall", "The following packages were updated since you last loaded this selection file. Your selection file has been updated with the changes (a one-time backup was also made). " +
                "If this is your current installation, and you wish to keep it, it is recommended to install/update after seeing this message.");

            //Component: selectionFileIssuesTitle
            //
            English.Add("selectionFileIssuesTitle", "Selection loading messages");

            //Component: selectionFormatOldV2
            //the message box to show up when your selection file is old (v2) and will be upgraded
            English.Add("selectionFormatOldV2", "This selection file format is legacy (V2) and will be upgraded to V3. A V2 backup will be created.");

            //Component:
            //
            English.Add("oldSavedConfigFile", "The saved preferences file your are using is in an outdated format and will be inaccurate in the future. Convert it to the new format? (A backup of the old format will be made)");

            //Component:
            //
            English.Add("prefrencesSet", "Preferences Set");

            //Component:
            //
            English.Add("selectionsCleared", "Selections Cleared");

            //Component: failedLoadSelection
            //
            English.Add("failedLoadSelection", "Failed to load selection");

            //Component: unknownselectionFileFormat
            //
            English.Add("unknownselectionFileFormat", "Unknown selection file version");

            //Component: ExpandAllButton
            //
            English.Add("ExpandAllButton", "Expand Current Tab");

            //Component: CollapseAllButton
            //
            English.Add("CollapseAllButton", "Collapse Current Tab");

            //Component: InstallingTo
            //
            English.Add("InstallingTo", "Installing to: {0}");

            //Component: saveConfig
            //
            English.Add("selectWhereToSave", "Select where to save selection file");

            //Component: updated
            //shows (updated) next to a component
            English.Add("updated", "updated");

            //Component: disabled
            //shows (disabled) next to a component
            English.Add("disabled", "disabled");

            //Component: invisible
            //shows (invisible) next to a component
            English.Add("invisible", "invisible");

            //Component: SelectionFileIssuesDisplay
            //window title for when issues applying a user's selection
            English.Add("SelectionFileIssuesDisplay", "Errors applying selection file");

            //Component: selectionFileIssues
            //alias of SelectionFileIssuesDisplay
            English.Add("selectionFileIssues", English["SelectionFileIssuesDisplay"]);

            //Component: selectionFileIssuesHeader
            //
            English.Add("selectionFileIssuesHeader", "Please read the following messages related to your selection file");

            //Component: VersionInfoYesText
            // A button in VersionInfo window confirming the installation of the new available version of the application.
            English.Add("VersionInfoYesText", English["yes"]);

            //Component: VersionInfoNoText
            // A button in VersionInfo window aborting the installation of the new available version of the application.
            English.Add("VersionInfoNoText", English["no"]);

            //--- Fix my description.
            //  Without it, the update windows displays the bare string name.
            English.Add("NewVersionAvailable", "New version available");

            //Component: HavingProblemsTextBlock
            // Text on the bottom of the Updater window, followed by ManualUpdateLink with hard-coded empty space (not the key) in between.
            English.Add("HavingProblemsTextBlock", "If you are having problems updating, please");

            //Component: ManualUpdateLink
            // Link following HavingProblemsTextBlock text. Preceeded by hard-coded empty space (not the key).
            English.Add("ManualUpdateLink", "Click Here");

            //Component: loadingApplicationUpdateNotes
            //
            English.Add("loadingApplicationUpdateNotes", "Loading application update notes...");

            //Component: failedToLoadUpdateNotes
            //
            English.Add("failedToLoadUpdateNotes", "Failed to load application update notes");

            //Component: ViewUpdateNotesOnGoogleTranslate
            //
            English.Add("ViewUpdateNotesOnGoogleTranslate", "View this on Google Translate");

            //Component: VersionInfoAskText
            // Text in the window with Yes and No buttons asking the user if he/she wants to update the application
            English.Add("VersionInfoAskText", "Do you wish to update now?");

            //Component: SelectDownloadMirrorTextBlock
            //
            English.Add("SelectDownloadMirrorTextBlock", "Select a download mirror");

            //Component: SelectDownloadMirrorTextBlockDescription
            //
            English.Add("SelectDownloadMirrorTextBlockDescription", "This mirror will be used only for package downloads");

            //Component: downloadMirrorUsaDefault
            //
            English.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Dallas, USA");

            //Component: downloadMirrorDe
            //
            English.Add("downloadMirrorDe", "clanverwaltung.de, Frankfurt, Germany");
            #endregion

            #region Installer Messages
            //Component:
            //
            English.Add("Downloading", "Downloading");

            //Component:
            //
            English.Add("patching", "Patching");

            //Component:
            //
            English.Add("done", "Done");

            //Component:
            //
            English.Add("cleanUp", "Clean up resources");

            //Component:
            //
            English.Add("idle", "Idle");

            //Component:
            //
            English.Add("status", "Status:");

            //Component:
            //
            English.Add("canceled", "Canceled");

            //Component:
            //
            English.Add("appSingleInstance", "Checking for single instance");

            //Component:
            //
            English.Add("checkForUpdates", "Checking for updates");

            //Component:
            //
            English.Add("verDirStructure", "Verifying directory structure");

            //Component:
            //
            English.Add("loadingSettings", "Loading settings");

            //Component:
            //
            English.Add("loadingTranslations", "Loading translations");

            //Component:
            //
            English.Add("loading", "Loading");

            //Component:
            //
            English.Add("of", "of");

            //Component: failedToDownload1
            //
            English.Add("failedToDownload1", "Failed to download the package");

            //Component: failedToDownload2
            //
            English.Add("failedToDownload2", "Would you like to retry the download, abort the installation, or continue?");

            //Component:
            //
            English.Add("failedToDownloadHeader", "Failed to download");

            //Component: update check against online app version
            //
            English.Add("failedManager_version", "Current Beta application is outdated and must be updated against stable channel. No new Beta version online now.");

            //Component:
            //
            English.Add("fontsPromptInstallHeader", "Admin to install fonts?");

            //Component:
            //
            English.Add("fontsPromptInstallText", "Do you have admin rights to install fonts?");

            //Component:
            //
            English.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");

            //Component:
            //
            English.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");

            //Component:
            //
            English.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");

            //Component:
            //
            English.Add("failedCreateUpdateBat", "Unable to create update process.\n\nPlease manually delete the file:\n{0}\n\nrename file:\n{1}\nto:\n{2}\n\nDirectly jump to the folder?");

            //Component:
            //
            English.Add("cantStartNewApp", "Unable to start application, but it is located in \n");

            //Component:
            //
            English.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");

            //Component: MainWindow_Load
            //
            English.Add("anotherInstanceRunning", "Another Instance of the Relhax Manager is already running");

            //Component: closeInstanceRunningForUpdate
            //
            English.Add("closeInstanceRunningForUpdate", "Please close ALL running instances of the Relhax Manager before we can go on and update.");

            //Component:
            //
            English.Add("skipUpdateWarning", "You are skipping updating. Database Compatibility is not guaranteed");

            //Component:
            //
            English.Add("patchDayMessage", "The modpack is currently down for patch day testing and mods updating. Sorry for the inconvenience." +
                " If you are a database manager, please add the command argument");

            //Component:
            //
            English.Add("autoAndFirst", "First time loading cannot be an auto install mode, loading in regular mode");

            //Component:
            //
            English.Add("confirmUninstallHeader", "Confirmation");

            //Component:
            //
            English.Add("confirmUninstallMessage", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");

            //Component:
            //
            English.Add("uninstallingText", "Uninstalling...");

            //Component:
            //progress message
            English.Add("uninstallingFile", "Uninstalling file");

            //Component: uninstallfinished messagebox
            //
            English.Add("uninstallFinished", "Uninstallation of mods finished.");

            //Component: uninstallFail
            //
            English.Add("uninstallFail", "The uninstallation failed. You could try another uninstallation mode or submit a bug report.");

            //Component:
            //
            English.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file" +
                " and folder security permissions are incorrect");

            //Component: extractionErrorHeader
            //
            English.Add("extractionErrorHeader", English["error"]);

            //Component:
            //
            English.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");

            //Component:
            //
            English.Add("noUninstallLogMessage", "The log file containing the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");

            //Component:
            //
            English.Add("noUninstallLogHeader", "Remove all mods");

            //Component: moveOutOfTanksLocation
            //
            English.Add("moveOutOfTanksLocation", "The modpack can not be run from the World_of_Tanks directory. Please move the application and try again.");

            //Component: moveAppOutOfDownloads
            // Many users download the application right from the website and run it in the downloads folder. This is not reccomended
            English.Add("moveAppOutOfDownloads", "The application detected that it is launched from the 'Downloads' folder. This is not recommended because the application creates several folders and files " +
                "that may be difficult to find in a large 'Downloads' folder. You should move the application and all 'Relhax' files and folders into a new folder.");

            //Component: Current database is same as last installed database (body)
            //
            English.Add("DatabaseVersionsSameBody", "The database has not changed since your last installation. Therefore there are no updates to your current mods selection." +
                " Continue anyway?");

            //Component: Current database is same as last installed database (header)
            //
            English.Add("DatabaseVersionsSameHeader", "Database version is the same");

            //Component:
            //
            English.Add("databaseNotFound", "Database not found at supplied URL");

            //Component: Detected client version
            //
            English.Add("detectedClientVersion", "Detected Client version");

            //Component: Supported client versions
            //
            English.Add("supportedClientVersions", "Supported Clients");

            //Component: Supported clients notice
            //
            English.Add("supportNotGuarnteed", "This client version is not offically supported. Mods may not work.");

            //Component: couldTryBeta
            //
            English.Add("couldTryBeta", "If a game patch was recently released, the team is working on supporting it. You could try using the beta database.");

            //Component: missingMSVCPLibrariesHeader
            //
            English.Add("missingMSVCPLibrariesHeader", "Failed to load required libraries");

            //Component: missingMSVCPLibraries
            //Microsoft Visual C++ 2013 libraries (msvcp120.dll, etc.) are required to load and process atlas images
            English.Add("missingMSVCPLibraries", "The contour icon image processing libraries failed to load. This could indicate you are missing a required Microsoft dll package.");

            //Component: openLinkToMSVCP
            //Microsoft Visual C++ 2013 libraries (msvcp120.dll, etc.) are required to load and process atlas images
            English.Add("openLinkToMSVCP", "Would you like to open your browser to the package download page?");

            //Component: notifying the user the change won't take effect until application restart
            //
            English.Add("noChangeUntilRestart", "This option won't take effect until application restart");

            //Component: installBackupMods
            //
            English.Add("installBackupMods", "Backing up mod file");

            //Component: installBackupData
            //
            English.Add("installBackupData", "Backing up user data");

            //Component: installClearCache
            //
            English.Add("installClearCache", "Deleting WoT cache");

            //Component: installClearLogs
            //
            English.Add("installClearLogs", "Deleting log files");

            //Component: installCleanMods
            //
            English.Add("installCleanMods", "Cleaning mods folders");

            //Component: installExtractingMods
            //
            English.Add("installExtractingMods", "Extracting package");

            //Component: installZipFileEntry
            //
            English.Add("installZipFileEntry", "File entry");

            //Component: installExtractingCompletedThreads
            //
            English.Add("installExtractingCompletedThreads", "Completed threads");

            //Component: installExtractingOfGroup
            //
            English.Add("installExtractingOfGroup", "of install group");

            //Component: extractingUserMod
            //
            English.Add("extractingUserMod", "Extracting user package");

            //Component: installRestoreUserdata
            //
            English.Add("installRestoreUserdata", "Restoring user data");

            //Component: installXmlUnpack
            //
            English.Add("installXmlUnpack", "Unpacking XML file");

            //Component: installPatchFiles
            //
            English.Add("installPatchFiles", "Patching file");

            //Component: installShortcuts
            //
            English.Add("installShortcuts", "Creating shortcuts");

            //Component: installContourIconAtlas
            English.Add("installContourIconAtlas", "Creating Atlas file");

            //Component: installFonts
            //
            English.Add("installFonts", "Installing fonts");

            //Component: installCleanup
            //
            English.Add("installCleanup", "Cleaning Up");

            //Component: ExtractAtlases
            English.Add("AtlasExtraction", "Extracting Atlas file");

            //Component: copyingFile
            //
            English.Add("copyingFile", "Copying file");

            //Component: deletingFile
            //
            English.Add("deletingFile", "Deleting file");

            //Component scanningModsFolders
            //
            English.Add("scanningModsFolders", "Scanning mods folders ...");

            //Component: file
            //
            English.Add("file", "File");

            //Component: size
            //
            English.Add("size", "Size");

            //Component: CheckDatabase
            //
            English.Add("checkDatabase", "Checking the database for outdated or no longer needed files");

            //Component:
            //function checkForOldZipFiles()
            English.Add("parseDownloadFolderFailed", "Parsing the \"{0}\" folder failed.");

            //Component: installation finished
            //
            English.Add("installationFinished", "Installation is finished");

            //Component: deletingFiles
            //
            English.Add("deletingFiles", "Deleting files");

            //Component:
            //
            English.Add("uninstalling", "Uninstalling");

            //Component:
            //
            English.Add("zipReadingErrorHeader", "Incomplete Download");

            //Component:
            //
            English.Add("zipReadingErrorMessage1", "The zip file");

            //Component:
            //
            English.Add("zipReadingErrorMessage3", "could not be read.");

            //Component:
            //
            English.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your" +
                " file and folder security permissions");

            //Component:
            //
            English.Add("patchingSystemDeneidAccessHeader", "Access Denied");

            //Component: Failed To Delete folder
            //
            English.Add("folderDeleteFailed", "Failed to delete the folder");

            //Component: Failed To Delete file
            //
            English.Add("fileDeleteFailed", "Failed to delete the file");

            //Component: DeleteBackupFolder
            //
            English.Add("DeleteBackupFolder", "Backups");

            //Component: installFailed
            //Error message to show at the end of an unsuccessful installation
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            English.Add("installFailed", "The installation failed at the following steps");
            #endregion

            #region Install finished window
            //Component: InstallFinished
            //
            English.Add("InstallFinished", "Install Finished");

            //Component: InstallationCompleteText
            //
            English.Add("InstallationCompleteText", "The Installation is complete. Would you like to...");

            //Component: InstallationCompleteStartWoT
            //
            English.Add("InstallationCompleteStartWoT", "Start the game? (WorldofTanks.exe)");

            //Component: InstallationCompleteStartGameCenter
            //
            English.Add("InstallationCompleteStartGameCenter", "Start WG Game Center?");

            //Component: InstallationCompleteOpenXVM
            //
            English.Add("InstallationCompleteOpenXVM", "Open the xvm login website?");

            //Component: InstallationCompleteCloseThisWindow
            //
            English.Add("InstallationCompleteCloseThisWindow", "Close this window?");

            //Component: InstallationCompleteCloseApp
            //
            English.Add("InstallationCompleteCloseApp", "Close the application?");

            //Component: xvmUrlLocalisation
            //localisation to which page you will jump
            English.Add("xvmUrlLocalisation", "en");

            //Component: CouldNotStartProcess
            //
            English.Add("CouldNotStartProcess", "Could not start process");
            #endregion

            #region Diagnostics
            //Component: Diagnostics
            //
            English.Add("Diagnostics", "Diagnostics");

            //Component: MainTextBox
            //
            English.Add("DiagnosticsMainTextBox", "You can use the options below to try to diagnose or solve the issues you are having.");

            //Component: LaunchWoTLauncher
            //
            English.Add("LaunchWoTLauncher", "Start the World of Tanks launcher in integrity validation mode");

            //Component: CollectLogInfo
            //
            English.Add("CollectLogInfo", "Collect log files into a zip file to report a problem");

            //Component: CollectLogInfoButtonDescription
            //
            English.Add("CollectLogInfoButtonDescription", "Collects all the necessary log files into a one ZIP files.\nThis makes it easier for you to report a problem.");

            //Component: DownloadWGPatchFilesText
            // A button for a new window with multiple tabs, guiding the user through the process of manual WG patch files download over HTTP for later installation by the WGC.
            English.Add("DownloadWGPatchFilesText", "Download WG Patch files for any WG client via HTTP");

            //Component: DownloadWGPatchFilesButtonDescription
            // A tooltip for the DownloadWGPatchFiles button in diagnostics window. Used to manualy download WG patch files over HTTP for later installation by the WGC.
            English.Add("DownloadWGPatchFilesButtonDescription", "Guides you through & downloads patch files for Wargaming games (WoT, WoWs, WoWp) over HTTP so you can install them later.\n" +
                "Particularily useful for people who cannot use Wargaming Game Center's default P2P protocol.");

            //Component: SelectedInstallation
            //
            English.Add("SelectedInstallation", "Currently Selected Installation:");

            //Component: SelectedInstallationNone
            // A text label with the (not selected in this case) path to the selected (active) installation.
            English.Add("SelectedInstallationNone", "(" + English["none"].ToLower() + ")");

            //Component: collectionLogInfo
            //
            English.Add("collectionLogInfo", "Collecting log files...");

            //Component: startingLauncherRepairMode
            //
            English.Add("startingLauncherRepairMode", "Starting WoTLauncher in integrity validation mode...");

            //Component: failedCreateZipfile
            //
            English.Add("failedCreateZipfile", "Failed to create the zip file ");

            //Component: launcherRepairModeStarted
            //
            English.Add("launcherRepairModeStarted", "Repair mode successfully started");

            //Component: ClearDownloadCache
            //
            English.Add("ClearDownloadCache", "Clear download cache");

            //Component: ClearDownloadCacheDatabase
            //
            English.Add("ClearDownloadCacheDatabase", "Delete download cache database file");

            //Component: ClearDownloadCacheDescription
            //
            English.Add("ClearDownloadCacheDescription", "Delete all files in the \"RelhaxDownloads\" folder");

            //Component: ClearDownloadCacheDatabaseDescription
            //
            English.Add("ClearDownloadCacheDatabaseDescription", "Delete the xml database file. This will cause all zip files to be re-checked for integrity.\nAll invalid files will be re-downloaded if selected in your next installation.");

            //Component: clearingDownloadCache
            //
            English.Add("clearingDownloadCache", "Clearing download cache");

            //Component: failedToClearDownloadCache
            //
            English.Add("failedToClearDownloadCache", "Failed to clear download cache");

            //Component: cleaningDownloadCacheComplete
            //
            English.Add("cleaningDownloadCacheComplete", "Download cache cleared");

            //Component: clearingDownloadCacheDatabase
            //
            English.Add("clearingDownloadCacheDatabase", "Deleting xml database cache file");

            //Component: failedToClearDownloadCacheDatabase
            //
            English.Add("failedToClearDownloadCacheDatabase", "Failed to delete xml database cache file");

            //Component: cleaningDownloadCacheDatabaseComplete
            //
            English.Add("cleaningDownloadCacheDatabaseComplete", "Xml database cache file deleted");

            //Component: ChangeInstall
            //
            English.Add("ChangeInstall", "Change the currently selected WoT installation");

            //Component: ChangeInstallDescription
            //
            English.Add("ChangeInstallDescription", "This will change which log files will get added to the diagnostics zip file");

            //Component: zipSavedTo
            //
            English.Add("zipSavedTo", "Zip file saved to: ");

            //Component: selectFilesToInclude
            //
            English.Add("selectFilesToInclude", "Select files to include in the bug report");

            //Component: TestLoadImageLibraries
            // A button in Diagnostics window for test-loading Atlas processing libraries.
            English.Add("TestLoadImageLibraries", "Test loading the atlas image processing libraries");

            //Component: TestLoadImageLibrariesButtonDescription
            // A tooltip for TestLoadImageLibraries button. Does hmmmm... @Willster? :>
            English.Add("TestLoadImageLibrariesButtonDescription", "Tests the atlas image processing libraries"); // I will need your help here, @Willster. @Nullmaruzero

            //Component: loadingAtlasImageLibraries
            //
            English.Add("loadingAtlasImageLibraries", "Loading atlas image processing libraries");

            //Component: loadingAtlasImageLibrariesSuccess
            //
            English.Add("loadingAtlasImageLibrariesSuccess", "Successfully loaded atlas image processing libraries");

            //Component: loadingAtlasImageLibrariesFail
            //
            English.Add("loadingAtlasImageLibrariesFail", "Failed to load atlas image processing libraries");

            //Component: CleanupModFilesText
            // One of the buttons in the diagnostics window.
            English.Add("CleanupModFilesText", "Cleanup mod files placed in incorrect locations");

            //Component: CleanupModFilesButtonDescription
            // The hint/tooltip for CleanupModFilesText
            English.Add("CleanupModFilesButtonDescription", "Deletes any mods located in folders like win32 and win64 that could be causing load conflicts");

            //Component: cleanupModFilesCompleted
            // Text displaying (upon mods being cleaned up) in the dedicated status area below all the buttons in the diagnostics window (it's large enough for longer messages).
            English.Add("cleanupModFilesCompleted", "Cleanup mod files completed");

            //Component: CleanGameCacheText
            // Text block for button to allow user to clear cache data from applicationData folders
            English.Add("CleanGameCacheText", "Clear game cache files");

            //Component: cleanGameCacheProgress
            //
            English.Add("cleanGameCacheProgress", "Clearing game cache files");

            //Component: cleanGameCacheSuccess
            //
            English.Add("cleanGameCacheSuccess", "Sucessfully cleared game cache files");

            //Component: cleanGameCacheFail
            //
            English.Add("cleanGameCacheFail", "Failed to clear game cache files");

            //Component: TrimRelhaxLogfileText
            // Text block for allowing the user to trim relhax.log logfile to the last 3 launches (assuming header/footer entries exist)
            English.Add("TrimRelhaxLogfileText", "Trim the Relhax log file to the last 3 launches");

            //Component: trimRelhaxLogProgress
            //
            English.Add("trimRelhaxLogProgress", "Trimming the Relhax log file");

            //Component: trimRelhaxLogSuccess
            //
            English.Add("trimRelhaxLogSuccess", "Sucessfully trimmed the Relhax log file");

            //Component: trimRelhaxLogFail
            //
            English.Add("trimRelhaxLogFail", "Failed to trim the Relhax log file");
            #endregion

            #region Wot Client install selection
            //Component: WoTClientSelection
            // Title for the wot client selection window. Allows the user to select from a list of detected installations.
            English.Add("WoTClientSelection", "WoT Client Selection");

            //Component: ClientSelectionsTextHeader
            //
            English.Add("ClientSelectionsTextHeader", "The following client installations were automatically detected");

            //Component: ClientSelectionsCancelButton
            //
            English.Add("ClientSelectionsCancelButton", English["cancel"]);

            //Component: ClientSelectionsManualFind
            // The text for the button that allows a user to manually locate a WoT installation. This was the previous feature before replaced with this window
            English.Add("ClientSelectionsManualFind", "Manual Selection");

            //Component: ClientSelectionsContinueButton
            //
            English.Add("ClientSelectionsContinueButton", English["select"]);

            //Component: DiagnosticsAddSelectionsPicturesLabel
            //
            English.Add("DiagnosticsAddSelectionsPicturesLabel", "Add any additional files here (your selection file, picture, etc.)");

            //Component: DiagnosticsAddFilesButton
            //
            English.Add("DiagnosticsAddFilesButton", "Add Files");

            //Component: DiagnosticsRemoveSelectedButton
            //
            English.Add("DiagnosticsRemoveSelectedButton", "Remove Selected");

            //Component: DiagnosticsContinueButton
            //
            English.Add("DiagnosticsContinueButton", English["ContinueButton"]);
            #endregion

            #region Preview Window
            //Component: Preview
            //
            English.Add("Preview", "Preview");

            //Component: noDescription
            //
            English.Add("noDescription", "No description provided");

            //Component: noUpdateInfo
            //
            English.Add("noUpdateInfo", "No update info provided");

            //Component: noTimestamp
            //
            English.Add("noTimestamp", "No timestamp provided");

            //Component: PreviewNextPicButton
            // A button in mod preview window used to display the previous preview image.
            English.Add("PreviewNextPicButton", English["next"]);

            //Component: PreviewPreviousPicButton
            // A button in mod preview window used to display the next preview image.
            English.Add("PreviewPreviousPicButton", English["previous"]);

            //Component: DevUrlHeader
            //
            English.Add("DevUrlHeader", "Developer links");

            //Component: dropDownItemsInside
            //
            English.Add("dropDownItemsInside", "Items Inside");

            //Component: popular
            //
            English.Add("popular", "popular");

            //Component: previewEncounteredError
            //
            English.Add("previewEncounteredError", "The preview window encountered an error. Failed to display preview.");

            //Component: popularInDescription
            //
            English.Add("popularInDescription", "This is a popular package");

            //Component: controversialInDescription
            //
            English.Add("controversialInDescription", "This is a controversial package");

            //Component: encryptedInDescription
            //
            English.Add("encryptedInDescription", "This is an encrypted package that can't be checked for viruses");

            //Component: fromWgmodsInDescription
            //
            English.Add("fromWgmodsInDescription", "The source of this package is the WGmods portal (wgmods.net)");
            #endregion

            #region Developer Selection Window
            //Component: DeveloperSelectionsViewer
            //
            English.Add("DeveloperSelectionsViewer", "Selections Viewer");

            //Component: DeveloperSelectionsTextHeader
            //
            English.Add("DeveloperSelectionsTextHeader", "Selection to load");

            //Component: DeveloperSelectionsCancelButton
            //
            English.Add("DeveloperSelectionsCancelButton", English["cancel"]);

            //Component: DeveloperSelectionsLocalFile
            //The text in the first radioButton in the selection viewer, for the user to select their own personal config file to load
            English.Add("DeveloperSelectionsLocalFile", "Local file");

            //Component: DeveloperSelectionsContinueButton
            //
            English.Add("DeveloperSelectionsContinueButton", "Select");

            //Component: failedToParseSelections
            //
            English.Add("failedToParseSelections", "Failed to parse selections");

            //Component: lastModified
            //
            English.Add("lastModified", "Last modified");
            #endregion

            #region Advanced Installer Window
            //Component: AdvancedProgress
            //
            English.Add("AdvancedProgress", "Advanced Installer Progress");

            //Component: PreInstallTabHeader
            //
            English.Add("PreInstallTabHeader", "Pre-install tasks");

            //Component: PostInstallTabHeader

            //Component: ExtractionTabHeader
            //
            English.Add("ExtractionTabHeader", "Extraction");

            //
            English.Add("PostInstallTabHeader", "Post-install tasks");

            //Component: AdvancedInstallBackupData
            //
            English.Add("AdvancedInstallBackupData", "Backup Mod Data");

            //Component: AdvancedInstallClearCache
            //
            English.Add("AdvancedInstallClearCache", "Clear WoT Cache");

            //Component: AdvancedInstallClearLogs
            //
            English.Add("AdvancedInstallClearLogs", "Clear Logfiles");

            //Component: AdvancedInstallClearMods
            //
            English.Add("AdvancedInstallClearMods", "Uninstall previous installation");

            //Component: AdvancedInstallInstallMods
            //
            English.Add("AdvancedInstallInstallMods", "Install Thread");

            //Component: AdvancedInstallUserInstallMods
            //
            English.Add("AdvancedInstallUserInstallMods", "User packages");

            //Component: AdvancedInstallRestoreData
            //
            English.Add("AdvancedInstallRestoreData", "Restore Data");

            //Component: AdvancedInstallXmlUnpack
            //
            English.Add("AdvancedInstallXmlUnpack", "Xml Unpacker");

            //Component: AdvancedInstallPatchFiles
            //
            English.Add("AdvancedInstallPatchFiles", "Patch files");

            //Component: AdvancedInstallCreateShortcuts
            //
            English.Add("AdvancedInstallCreateShortcuts", "Create Shortcuts");

            //Component: AdvancedInstallCreateAtlas
            //
            English.Add("AdvancedInstallCreateAtlas", "Create atlases");

            //Component: AdvancedInstallInstallFonts
            //
            English.Add("AdvancedInstallInstallFonts", "Install fonts");

            //Component: AdvancedInstallTrimDownloadCache
            //
            English.Add("AdvancedInstallTrimDownloadCache", "Trim download cache");

            //Component: AdvancedInstallCleanup
            //
            English.Add("AdvancedInstallCleanup", "Cleanup");

            #endregion

            #region News Viewer
            //Component: NewsViewer
            //
            English.Add("NewsViewer", "News Viewer");

            //Component: application_Update_TabHeader
            //
            English.Add("application_Update_TabHeader", "Application");

            //Component: database_Update_TabHeader
            //
            English.Add("database_Update_TabHeader", "Database");

            //Component: ViewNewsOnGoogleTranslate
            //
            English.Add("ViewNewsOnGoogleTranslate", "View this on Google Translate");
            #endregion

            #region Loading Window
            //Component: ProgressIndicator
            //
            English.Add("ProgressIndicator", "Loading");

            //Component: LoadingHeader
            //
            English.Add("LoadingHeader", "Loading, please wait");
            #endregion

            #region First Load Acknowledgements
            //Component: FirstLoadAcknowledgments
            // ACKS window title.
            English.Add("FirstLoadAcknowledgments", "First Load Acknowledgements");

            //---- Fix my description, I was missing and @Nullmaruzero found me!
            English.Add("AgreementLicense", "I have read and agree to the ");

            //Component: LicenseLink
            // A link inlining at the end with the AgreementLicense first checkbox text from above.
            English.Add("LicenseLink", "License Agreement");

            //Component: AgreementSupport1
            // The second acknowledgement checkbox, inlines with link to forums, the conjunction phrase and discord server link.
            // EXAMPLE: [x] I understand that I can receive support on the dedicated 
            English.Add("AgreementSupport1", "I understand that I can receive support on the dedicated ");

            //Component: AgreementSupportDiscord
            // Third and the last inlined elements for AgreementSupport1, a link to Relhax Discord server.
            English.Add("AgreementSupportDiscord", "Discord");

            //Component: AgreementHoster
            // The third acknowledgement checkbox, just plain text. Make sure you communicate this one well.
            // EXAMPLE: [x] I understand Relhax is a mod hosting and installation service and Relhax does not maintain every mod found in this Modpack
            English.Add("AgreementHoster", "I understand Relhax is a mod hosting and installation service and Relhax does not maintain every mod found in this Modpack");

            //Component: AgreementAnonData
            // The fourth acknowledgement checkbox. Make sure you communicate this one well.
            // EXAMPLE: I understand that Relhax V2 collects anonymous usage data to improve the application, and can be disabled in the advanced settings tab
            English.Add("AgreementAnonData", "I understand that Relhax V2 collects anonymous usage data to improve the application, and can be disabled in the advanced settings tab");

            //Component: V2UpgradeNoticeText
            // A text block appearing (in red) under the last acknowledgement, centered in the window - only if the app detects that a conversion is required.
            English.Add("V2UpgradeNoticeText", "It looks like you are running an upgrade from V1 to V2 for the first time.\n" +
                "Pressing continue will result in an upgrade to the file structure that cannot be reverted. It is recommended to make a backup of your V1 folder before continuing");
            #endregion

            #region Export Mode
            //Component: ExportModeSelect
            //
            English.Add("ExportModeSelect", "Select WoT client for export");

            //Component: selectLocationToExport
            //
            English.Add("selectLocationToExport", "Select the folder to export the mod installation into");

            //Component: ExportSelectVersionHeader
            //
            English.Add("ExportSelectVersionHeader", "Please select the version of the WoT client you want to export for");

            //Component: ExportContinueButton
            //
            English.Add("ExportContinueButton", English["ContinueButton"]);

            //Component: ExportCancelButton
            //
            English.Add("ExportCancelButton", English["cancel"]);

            //Component: ExportModeMinorVersion
            //
            English.Add("ExportModeMinorVersion", "WoT version");
            #endregion

            #region Asking to close WoT
            //Component: AskCloseWoT
            //
            English.Add("AskCloseWoT", "WoT is Running");

            //Component: WoTRunningTitle
            //
            English.Add("WoTRunningTitle", "WoT is Running");

            //Component: WoTRunningHeader
            //
            English.Add("WoTRunningHeader", "It looks like your WoT install is currently open. Please close it before we can proceed");

            //Component: WoTRunningCancelInstallButton
            //
            English.Add("WoTRunningCancelInstallButton", "Cancel Installation");

            //Component: WoTRunningRetryButton
            //
            English.Add("WoTRunningRetryButton", "Re-detect");

            //Component: WoTRunningForceCloseButton
            //
            English.Add("WoTRunningForceCloseButton", "Force close the game");
            #endregion

            #region Scaling Confirmation
            //Component: ScalingConfirmation
            //
            English.Add("ScalingConfirmation", "Scaling Confirmation");

            //Component: ScalingConfirmationHeader
            //
            English.Add("ScalingConfirmationHeader", "The scaling value has changed. Would you like to keep it?");

            //Component: ScalingConfirmationRevertTime
            //
            English.Add("ScalingConfirmationRevertTime", "Reverting in {0} Second(s)");

            //Component: ScalingConfirmationKeep
            //
            English.Add("ScalingConfirmationKeep", "Keep");

            //Component: ScalingConfirmationDiscard
            //
            English.Add("ScalingConfirmationDiscard", "Discard");
            #endregion

            #region Game Center download utility
            //Component: GameCenterUpdateDownloader
            //Application window title
            English.Add("GameCenterUpdateDownloader", "Game Center Update Downloader");

            //Component: GcDownloadStep1Header
            //
            English.Add("GcDownloadStep1Header", "Select Game Client");

            //Component: GcDownloadStep1TabDescription
            //
            English.Add("GcDownloadStep1TabDescription", "Select the Wargaming client to collect data for (WoT, WoWs, WoWp)");

            //Component: GcDownloadStep1SelectClientButton
            //
            English.Add("GcDownloadStep1SelectClientButton", "Select client");

            //Component: GcDownloadStep1CurrentlySelectedClient
            //
            English.Add("GcDownloadStep1CurrentlySelectedClient", "Currently selected client: {0}");

            //Component: GcDownloadStep1NextText
            //
            English.Add("GcDownloadStep1NextText", English["next"]);

            //Component: GcDownloadStep1GameCenterCheckbox
            //
            English.Add("GcDownloadStep1GameCenterCheckbox", "Check for game center updates instead");

            //Component: GcDownloadSelectWgClient
            //
            English.Add("GcDownloadSelectWgClient", "Select WG Client");

            //Component: ClientTypeValue
            //Initial value for the Component -> "None" (No current entry)
            English.Add("ClientTypeValue", "None");

            //Component: LangValue
            //
            English.Add("LangValue", English["ClientTypeValue"]); //Is this needed?

            //Component: GcMissingFiles
            //
            English.Add("GcMissingFiles", "Your Client is missing the following xml definition files");

            //Component: GcDownloadStep2Header
            //
            English.Add("GcDownloadStep2Header", "Close Game Center");

            //Component: GcDownloadStep2TabDescription
            //
            English.Add("GcDownloadStep2TabDescription", "Close the game center (application will detect closure)");

            //Component: GcDownloadStep2GcStatus
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatus", "Game Center is {0}");

            //Component: GcDownloadStep2GcStatusOpened
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatusOpened", "opened");

            //Component: GcDownloadStep2GcStatusClosed
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatusClosed", "closed");

            //Component: GcDownloadStep2PreviousText
            //
            English.Add("GcDownloadStep2PreviousText", English["previous"]);

            //Component: GcDownloadStep2NextText
            //
            English.Add("GcDownloadStep2NextText", English["next"]);

            //Component: GcDownloadStep3Header
            //
            English.Add("GcDownloadStep3Header", "Get Update Information");

            //Component: GcDownloadStep3TabDescription
            //
            English.Add("GcDownloadStep3TabDescription", "Getting the list of patch files to download");

            //Component: GcDownloadStep3PreviousText
            //
            English.Add("GcDownloadStep3PreviousText", English["previous"]);

            //Component: GcDownloadStep3NextText
            //
            English.Add("GcDownloadStep3NextText", English["next"]);

            //Component: GcDownloadStep4Header
            //
            English.Add("GcDownloadStep4Header", "Download Update Files");

            //Component: GcDownloadStep4TabDescription
            //
            English.Add("GcDownloadStep4TabDescription", "Downloading the patch files...");

            //Component: GcDownloadStep3NoFilesUpToDate
            //
            English.Add("GcDownloadStep3NoFilesUpToDate", "No patch files to download (up to date)");

            //Component: GcDownloadStep4DownloadingCancelButton
            //
            English.Add("GcDownloadStep4DownloadingCancelButton", English["cancel"]);

            //Component: GcDownloadStep4DownloadingText
            //Downloading patch 1 of 2: wg_filename.wgpkg
            English.Add("GcDownloadStep4DownloadingText", "Downloading patch {0} of {1}: {2}");

            //Component: GcDownloadStep4Previous
            //
            English.Add("GcDownloadStep4PreviousText", English["previous"]);

            //Component: GcDownloadStep4NextText
            //
            English.Add("GcDownloadStep4NextText", English["next"]);

            //Component: GcDownloadStep4DownloadComplete
            //
            English.Add("GcDownloadStep4DownloadComplete", "Package downloads complete");

            //Component: GcDownloadStep5Header
            //
            English.Add("GcDownloadStep5Header", "Complete!");

            //Component: GcDownloadStep5TabDescription
            //
            English.Add("GcDownloadStep5TabDescription", "The process is complete! The game center should detect the files when opened.");

            //Component: GcDownloadStep5CloseText
            //
            English.Add("GcDownloadStep5CloseText", English["close"]);

            //--- Fix my description, @Nullmaruzero
            //      Without this string in English, the window title does not update back to English after selecting any other language.
            English.Add("FirstLoadSelectLanguage", "Language Selection");

            //Component: SelectLanguageHeader
            // A header (label) in the window with a list of radio buttons with available languages below.
            English.Add("SelectLanguageHeader", "Please select your language");

            //Component: SelectLanguagesContinueButton
            // Just a "Continue" button on the bottom. Proceeds to first-run acknowlegements window.
            English.Add("SelectLanguagesContinueButton", English["ContinueButton"]);
            #endregion

            #region Credits window
            //Component: Credits
            //Application window title
            English.Add("Credits", "Relhax Modpack Credits");

            //Component: creditsProjectLeader
            //
            English.Add("creditsProjectLeader", "Project Leader");

            //Component: creditsDatabaseManagers
            //
            English.Add("creditsDatabaseManagers", "Database Managers");

            //Component: creditsTranslators
            //
            English.Add("creditsTranslators", "Translators");

            //Component: creditsusingOpenSourceProjs
            //
            English.Add("creditsusingOpenSourceProjs", "Relhax Modpack uses the following Open Source projects");

            //Component: creditsSpecialThanks
            //
            English.Add("creditsSpecialThanks", "Special thanks");

            //Component: creditsGrumpelumpf
            //
            English.Add("creditsGrumpelumpf", "Project leader of OMC, allowed us to pick up Relhax from where he left off");

            //Component: creditsRkk1945
            //
            English.Add("creditsRkk1945", "The first beta tester who worked with me for months to get the project running");

            //Component: creditsRgc
            // RGC = Relic Gaming Community
            English.Add("creditsRgc", "Sponsoring the modpack and being my first beta tester group");

            //Component: creditsBetaTestersName
            //
            English.Add("creditsBetaTestersName", "Our beta testing team");

            //Component: creditsBetaTesters
            //
            English.Add("creditsBetaTesters", "Continuing to test and report issues in the application before it goes live");

            //Component: creditsSilvers
            //
            English.Add("creditsSilvers", "Helping with the community outreach and social networking");

            //Component: creditsXantier
            //
            English.Add("creditsXantier", "Initial IT support and setting up our server");

            //Component: creditsSpritePacker
            //
            English.Add("creditsSpritePacker", "Developing the sprite sheet packer algorithm and porting to .NET");

            //Component: creditsWargaming
            //
            English.Add("creditsWargaming", "Making an easy to automate modding system");

            //Component: creditsUsersLikeU
            // Exlamation mark at the end is hard-coded. @Nullmaruzero
            English.Add("creditsUsersLikeU", "Users like you");
            #endregion

            #region Conflicting Packages Dialog
            //Component: ConflictingPackageDialog
            //Application window title
            English.Add("ConflictingPackageDialog", "Conflicting package dialog");

            //Component: conflictingPackageMessageOptionA
            //
            English.Add("conflictingPackageMessageOptionA", "Option A");

            //Component: conflictingPackageMessageOptionB
            //
            English.Add("conflictingPackageMessageOptionB", "Option B");

            //Component: conflictingPackageMessagePartA
            //
            English.Add("conflictingPackageMessagePartA", "You have selected package \"{0}\", but it conflicts with the following package(s):");

            //Component: conflictingPackagePackageOfCategory
            //
            English.Add("conflictingPackagePackageOfCategory", "- {0}, of category {1}");

            //Component: conflictingPackageMessagePartB
            //
            English.Add("conflictingPackageMessagePartB", English["conflictingPackageMessageOptionA"] + ": Select \"{0}\", which will un-check all conflicting package(s)");

            //Component: conflictingPackageMessagePartC
            //
            English.Add("conflictingPackageMessagePartC", English["conflictingPackageMessageOptionB"] + ": Don't select \"{0}\", which will keep the conflicting package(s)");

            //Component: conflictingPackageMessagePartD
            //
            English.Add("conflictingPackageMessagePartD", "Closing the window will select option b");

            //Component: conflictingPackageMessagePartE
            //
            English.Add("conflictingPackageMessagePartE", "Please select an option");
            #endregion

            #region End of Life announcement
            //Component: EndOfLife
            //Application window title
            English.Add("EndOfLife", "Relhax End of Life");

            //Component: CloseWindowButton
            //Close button
            English.Add("CloseWindowButton", English["close"]);

            //Component: WoTForumAnnouncementsTextBlock
            //Text block at the bottom describing the forum post links
            English.Add("WoTForumAnnouncementsTextBlock", "WoT forum announcement posts:");

            //Component: endOfLifeMessagePart1
            // First sentence, main point of the dialog box.
            English.Add("endOfLifeMessagePart1", "On April 20th, 2022, the Relhax Modpack was shut down. I want to give a personal thank you to all our contributers and users for a successful 5+ years!");

            //Component: endOfLifeMessagePart2a
            // The rest of the "endOfLifeMessage" messages are my closing statements and thoughts on the modpack.
            English.Add("endOfLifeMessagePart2a", "On January 1st, 2017, I set a challenge to myself to not only re-create the OMC modpack in a modern UI, but to make the fastest package installation system of any modpack that existed.");

            //Component: endOfLifeMessagePart2b
            //
            English.Add("endOfLifeMessagePart2b", "I started as a team of 4, taking 3 members of OMC who wanted to contribute to the project. Over the course of 4 years, I designed, built, and re-built the modpack application from scratch, spending tens of thousands of hours.");

            //Component: endOfLifeMessagePart2c
            //
            English.Add("endOfLifeMessagePart2c", "At one point, the team grew to over 8 people from most WoT server regions. During the process, I grew as a programmer, learned about common software practices, and specialized in application multi-threading and handling concurrent operations.");

            //Component: endOfLifeMessagePart2d
            //
            English.Add("endOfLifeMessagePart2d", "I gained experience through the project, and got to interact with a great modding community. It allowed me to contribute back to the Relic Gaming Community, a group I joined in 2014.");

            //Component: endOfLifeMessagePart3a
            //
            English.Add("endOfLifeMessagePart3a", "As of this year, I finally finished my design work on the most optimized and efficient installer I could make for the community.");

            //Component: endOfLifeMessagePart3b
            //
            English.Add("endOfLifeMessagePart3b", "Seeing the project meet my original goal, and my interest in the game (and the project) dwindling, I decided to close the project.");

            //Component: endOfLifeMessagePart3c
            //
            English.Add("endOfLifeMessagePart3c", "It was a hard decision to make, but I did not want to continue to support a project I no longer had the interest to maintain.");

            //Component: endOfLifeMessagePart3d
            //
            English.Add("endOfLifeMessagePart3d", "I think it would have reflected poorly on the quality of product, and it would not be fair to end users. I wanted to close the project while it was still in a healthy state.");

            //Component: endOfLifeMessagePart4
            //
            English.Add("endOfLifeMessagePart4", "Again, thank you to everyone. It was a fun 5+ years, and I will miss it.");
            #endregion
        }
    }
}
