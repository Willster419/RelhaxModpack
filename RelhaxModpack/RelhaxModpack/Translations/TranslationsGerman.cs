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
        /// Loads all German translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsGerman()
        {
            #region General expressions
            German.Add("yes", "ja");
            German.Add("no", "nein");
            German.Add("cancel", "Abbrechen");
            German.Add("delete", "Löschen");
            German.Add("warning", "WARNUNG");
            German.Add("critical", "KRITISCH");
            German.Add("information", "Information");
            German.Add("select", "Auswählen");
            German.Add("abort", "Abbrechen");
            German.Add("error", "Fehler");
            German.Add("retry", "Wiederholen");
            German.Add("ignore", "Ignorieren");
            German.Add("lastUpdated", "Letzte Aktualisierung: ");
            German.Add("stepsComplete", "erledigte Aufgaben");
            German.Add("allFiles", "Alle Dateien");
            German.Add("GoogleTranslateLanguageKey", "de");
            German.Add("at", "bei");
            German.Add("seconds", "sekunden");
            German.Add("minutes", "minuten");
            German.Add("hours", "Stunden");
            German.Add("days", "Tage");
            German.Add("next", "Weiter");
            German.Add("ContinueButton", "Fortsetzen");
            German.Add("previous", "Zurück");
            German.Add("close", "Schließen");
            German.Add("none", "Nichts");
            #endregion

            #region Application messages
            German.Add("appFailedCreateLogfile", "Das Programm konnte eine Log-Datei nicht öffnen. Überprüfe die Berechtigungen oder verschiebe das Programm in einen Ordner mit Schreibrechten.");
            German.Add("failedToParse", "Die Datei konnte nicht verarbeitet werden");
            German.Add("failedToGetDotNetFrameworkVersion", "Fehler beim Abrufen der installierten .NET Framework-Version. Dies könnte auf ein Berechtigungsproblem hinweisen oder von Antivirensoftware blockiert werden.");
            German.Add("invalidDotNetFrameworkVersion", "Die installierte Version von .NET Framework ist kleiner als 4.8. Für den Betrieb von Relhax Modpack ist Version 4.8 oder höher erforderlich." +
                "Möchten Sie einen Link öffnen um die neueste Version von .NET Framework zu erhalten?");
            #endregion

            #region Tray Icon
            German.Add("MenuItemRestore", "Wiederherstellen");
            German.Add("MenuItemCheckUpdates", "Nach Updates suchen");
            German.Add("MenuItemAppClose", German["close"]);
            German.Add("newDBApplied", "Neue Datenbankversion angewendet");
            #endregion

            #region Main Window
            German.Add("InstallModpackButton", "Wähle Mods");
            German.Add("selectWOTExecutable", "Wähle deine WoT Programmdatei (WorldOfTanks.exe)");
            German.Add("InstallModpackButtonDescription", "Wähle die Mods aus, die du auf deinem WoT Client installieren möchtest");
            German.Add("UninstallModpackButton", "Deinstalliere das Relhax Modpack");
            German.Add("UninstallModpackButtonDescription", "*Alle* Mods entfernen, die auf deinem WoT Client installiert sind");
            German.Add("ViewNewsButton", "Aktualisierungen");
            German.Add("ViewNewsButtonDescription", "Anzeigen von Anwendungs-, Datenbank- und anderen Aktualisierungsnachrichten");
            German.Add("ForceManuelGameDetectionText", "Erzwinge manuelle Spielerkennung");
            German.Add("ForceManuelGameDetectionCBDescription", "Diese Option ist für die manuelle selektion des World of Tanks Spiel-" +
                    "speicherortes. Nutze dies wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            German.Add("LanguageSelectionTextblock", "Sprachauswahl");
            German.Add("LanguageSelectionTextblockDescription", "Wähle deine bevorzugte Sprache.\nFalls dir fehlende Übersetzungen auffallen, kannst du uns gerne darüber informieren.");
            German.Add("Forms_ENG_NAButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den NA Server");
            German.Add("Forms_ENG_EUButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den EU Server");
            German.Add("Forms_GER_EUButtonDescription", "Gehe zur deutschsprachigen 'World of Tanks' Forum Seite für den EU Server");
            German.Add("SaveUserDataText", "Mod-Daten speichern");
            German.Add("SaveUserDataCBDescription", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten" +
                " (wie Sitzungsstatistiken aus früheren Gefechten)");
            German.Add("CleanInstallText", "Saubere Installation (empfohlen)");
            German.Add("CleanInstallCBDescription", "Diese empfohlene Option deinstalliert deine vorherige Installation, bevor du die neue installierst.");
            German.Add("BackupModsText", "Sicherung des aktuellen Mod-Verzeichnis");
            German.Add("BackupModsSizeLabelUsed", "Sicherungen: {0}  Größe: {1}");
            German.Add("backupModsSizeCalculating", "Berechne Größe des Backups...");
            German.Add("BackupModsCBDescription", "Wähle diese Option, um ein Backup deiner aktuellen Mod-Installation zu erstellen. "+
                     "Diese werden im Ordner 'RelHaxModBackup' als ZIP-Datei mit einem Zeitstempel gespeichert.");
            German.Add("BackupModsSizeLabelUsedDescription", "Wenn aktiviert, sichert der Installationsprogramm Ihre aktuellen Mods-Installationsordner an den angegebenen Ort");
            German.Add("SaveLastInstallText", "Speicherung der letzten Installation");
            German.Add("SaveLastInstallCBDescription", "Wenn diese Option aktiviert ist, wendet das Installationsprogramm automatisch deine zuletzt verwendete Auswahl an");
            German.Add("MinimizeToSystemTrayText", "Ins Benachrichtigungsfeld minimieren");
            German.Add("MinimizeToSystemTrayDescription", "Wenn diese Option aktiviert ist, wird die Anwendung weiterhin im Benachtrichtigungsfeld ausgeführt, wenn du auf `Schließen` klickst.");
            German.Add("VerboseLoggingText", "Ausführliche Protokollierung");
            German.Add("VerboseLoggingCBDescription", "Weitere Protokollmeldungen in der Protokolldatei aktivieren. Nützlich für das Melden von Fehlern.");
            German.Add("AllowStatsGatherText", "Sende Satistik zur Mod-Nutzung");
            German.Add("AllowStatsGatherCBDescription", "Erlaube dem Installer, anonyme Statistikdaten über die Mod-Auswahl auf den Server hochzuladen. Dies ermöglicht es uns, unseren Support zu priorisieren");
            German.Add("DisableTriggersText", "Trigger deaktivieren");
            German.Add("DisableTriggersCBDescription", "Das Zulassen von Triggern kann die Installation beschleunigen, indem einige Aufgaben ausgeführt werden (z. B. das Erstellen von Kontursymbolen), während extrahiert wird," +
                "sobald alle für diese Aufgabe erforderlichen Ressourcen verfügbar sind. Dies wird automatisch deaktiviert, wenn Benutzermodifikationen erkannt werden");
            German.Add("appDataFolderNotExistHeader", "Konnte den Cache-Ordner WoT nicht erkennen");
            German.Add("CancelDownloadInstallButton", German["cancel"]);
            German.Add("appDataFolderNotExist", "Der Installer konnte den WoT-Cache-Ordner nicht erkennen. Installation fortsetzen ohne den WoT-Cache zu löschen?");
            German.Add("viewAppUpdates", "Programmaktualisierungen anzeigen");
            German.Add("viewDBUpdates", "Datenbankaktualisierungen anzeigen");
            German.Add("EnableColorChangeDefaultV2Text", "Farbwechsel");
            German.Add("EnableColorChangeDefaultV2CBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            German.Add("EnableColorChangeLegacyText", "Farbwechsel");
            German.Add("EnableColorChangeLegacyCBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            German.Add("ShowOptionsCollapsedLegacyText", "Optionen einklappen");
            German.Add("ShowOptionsCollapsedLegacyCBDescription", "Bei Auswahl wird die Optionen Liste bis auf die Hauptkategorien eingeklappt");
            German.Add("ClearLogFilesText", "Protokolldatei löschen");
            German.Add("ClearLogFilesCBDescription", "Löschen der WoT Protokolldatei, sowie XVM und PMOD Protokolldatei");
            German.Add("CreateShortcutsText", "Erstelle Desktop Verknüpfungen");
            German.Add("CreateShortcutsCBDescription", "Wenn diese Option aktiviert ist, werden bei der Installation die Verknüpfungen für \"World of Tanks\"," +
                " \"World of Tanks launcher\" und, wenn bei der Installation aktiviert, auch andere Verknüpfungen zu Konfigurationsprogrammen erstellt (z.B. WWIIHA Konfiguration)");
            German.Add("DeleteOldPackagesText", "Lösche alte Archiv-Dateien");
            German.Add("DeleteOldPackagesCBDescription", "Lösche alle ZIP-Dateien im Ordner \"RelhaxDownloads\", welche vom Installationsprogramm nicht mehr verwendet werden, um Speicherplatz freizugeben.");
            German.Add("MinimalistModeText", "Minimal Modus");
            German.Add("MinimalistModeCBDescription", "Wenn ausgewählt wird das Modpack einige nicht benötigte Pakete bei der Installation ausschließen, wie zum Beispiel den Modpack Button oder Relhax Themendateien");
            German.Add("AutoInstallText", "Automatische Installation");
            German.Add("AutoInstallCBDescription", "Wenn unten eine Auswahldatei und eine Zeit eingestellt sind, sucht das Installationsprogramm automatisch nach Updates für deine Mods und wendet diese an.");
            German.Add("OneClickInstallText", "Ein-Klick-Installation");
            German.Add("ForceEnabledCB", "Erzwinge, dass alle Pakete aktiviert sind [!]");
            German.Add("AutoOneclickShowWarningOnSelectionsFailText", "Warnung bei Fehler mit der Auswahldatei");
            German.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Zeige eine Warnung, wenn bei Benutzung der One-Click oder Auto-Install Funktion" +
                "ein Fehler mit der Auswahldatei auftritt");
            German.Add("ForceEnabledText", "Aktiviere alle Pakete  [!]");
            German.Add("ForceEnabledCBDescription", "Bewirkt, dass alle deaktivierten Pakete aktiviert werden. Kann zu schwerwiegenden Stabilitätsproblemen deiner Installation führen");
            German.Add("ForceVisibleText", "Alle Pakete sichtbar [!]");
            German.Add("ForceVisibleCBDescription", "Bewirkt, dass alle ausgeblendeten Pakete sichtbar sind. Kann zu schwerwiegenden Stabilitätsproblemen deiner Installation führen");
            German.Add("LoadAutoSyncSelectionFileText", "Auswahldatei laden");
            German.Add("LoadAutoSyncSelectionFileDescription", "Lade (d)eine Auswahldatei und nutze diese für die Ein-Klick und Auto-Install Funktionen. ");
            German.Add("AutoSyncCheckFrequencyTextBox", "Häufigkeit: alle ");
            German.Add("DeveloperSettingsHeader", "Entwickleroptionen [!]");
            German.Add("DeveloperSettingsHeaderDescription", "Die folgenden Optionen könnten Fehler und Instabilitäten verursachen!\nBitte wähle diese nur aus, wenn du weißt was du tust!");
            German.Add("ApplyCustomScalingText", "Anwendungsskalierung");
            German.Add("ApplyCustomScalingTextDescription", "Wende die Anzeigeskalierung auf das Installationsfenster an");
            German.Add("EnableCustomFontCheckboxText", "Aktiviere eigene Schriftarten");
            German.Add("EnableCustomFontCheckboxTextDescription", "Aktiviere eine deiner installierten Schriftarten für die Anzeige des Textes im Programm");
            German.Add("LauchEditorText", "Starte Datenbank Editor");
            German.Add("LauchEditorDescription", "Starte den Datenbank Editor von hier, anstatt über die Befehlszeile");
            German.Add("LauchPatchDesignerText", "Starte Patch Designer");
            German.Add("LauchPatchDesignerDescription", "Starte den Patch Designer von hier, anstatt über die Befehlszeile");
            German.Add("LauchAutomationRunnerText", "Starte den Automation Runner");
            German.Add("LauchAutomationRunnerDescription", "Starte den Automation Runner von hier, anstatt über die Befehlszeile");
            German.Add("InstallWhileDownloadingText", "Entpacke während des Downloads");
            German.Add("InstallWhileDownloadingCBDescription", "Wenn aktiviert, der Installer wird die Zip-Dateien sofort nach dem Download entpacken" +
                " und nicht erst auf das Herunterladen aller Dateien warten bevor mit dem Entpacken begonnen wird.");
            German.Add("MulticoreExtractionCoresCountLabel", "Erkannte Kerne: {0}");
            German.Add("MulticoreExtractionCoresCountLabelDescription", "Anzahl der auf deinem System erkannten logischen CPU-Kerne (Threads)");
            German.Add("SaveDisabledModsInSelectionText", "Behalte deaktivierte Mods beim Speichern der Auswahl");
            German.Add("SaveDisabledModsInSelectionDescription", "Wenn ein Mod wieder aktiviert wird, wird er aus deiner Auswahldatei ausgewählt");
            German.Add("AdvancedInstallationProgressText", "Erweitertes Installationsfenster");
            German.Add("AdvancedInstallationProgressDescription", "Zeigt während der Extraktion ein erweitertes Installationsfenster an, das nützlich ist, wenn die Multicore-Extraktion aktiviert ist");
            German.Add("ThemeSelectText", "Wähle Thema");
            German.Add("ThemeDefaultText", "Standard");
            German.Add("ThemeDefaultDescription", "Standard Thema");
            German.Add("ThemeDarkText", "Dunkel");
            German.Add("ThemeDarkDescription", "Dunkles Thema");
            German.Add("ThemeCustomText", "Benutzerdefiniert");
            German.Add("ThemeCustomDescription", "Benutzerdefiniertes Thema");
            German.Add("DumpColorSettingsButtonText", "Speichere derzeitige Farbeinstellungen");
            German.Add("OpenColorPickerButtonText", "Öffne Farbauswahl");
            German.Add("OpenColorPickerButtonDescription", "Öffnet Farbwahl, und erlaubt dir eigene Themas zu erstellen");
            German.Add("DumpColorSettingsButtonDescription", "Schreibt ein XML-Dokument aller Komponenten, auf die eine benutzerdefinierte Farbe angewendet werden kann, um ein benutzerdefiniertes Thema anzupassen");
            German.Add("MulticoreExtractionText", "Mehrkern Extraktion");
            German.Add("MulticoreExtractionCBDescription", "Wird der Installer den parallelen Entpack-Modus verwenden. Er wird mehrere Zip-Dateien gleichzeitig entpacken" +
                " und dadurch die Installationszeit reduziert. Nur für SSD Festplatten.");
            German.Add("UninstallDefaultText", "Standard");
            German.Add("UninstallQuickText", "Schnell");
            German.Add("ExportModeText", "Export-Modus");
            German.Add("ExportModeCBDescription", "Der Export-Modus ermöglicht es dir, einen Ordner und WoT-Version zu wählen, in die du deine Mods-Installation exportieren möchtest." +
                " Nur für fortgeschrittene Benutzer. Bitte beachten: es werden KEINE Spiel-XML-Dateien entpackt und nicht modifiziert oder Atlas Dateien erstellt (jeweils aus dem Spiel" +
                " bereitgestellt). Anweisungen dazu findest du im Export-Verzeichnis.");
            German.Add("ViewCreditsButtonText", "Danksagung");
            German.Add("ViewCreditsButtonDescription", "Übersicht aller großartigen Leute und Projekte, die das Modpack unterstützen");
            German.Add("ExportWindowDescription", "Wähle die Version von WoT, für die du exportieren möchtest");
            German.Add("HelperText", "Willkommen beim Relhax Modpack!" +
                " \n\nIch habe versucht, das Modpack so einfach wie möglich zu gestalten. " +
                " \nZum Beispiel kannst du mit einem Klick der rechten Maustaste auf einen Eintrag eine Beschreibung oder Vorschau anzeigen." +
                " \nNatürlich können dennoch Fragen entstehen." +
                " In so einem Fall nutze bitte eine unserer unten angezeigten Kontaktmöglichkeiten." +
                " \n\nVielen Dank für die Nutzung des Relhax Modpacks\n - Willster419");
            German.Add("helperTextShort", "Willkommen zum Relhax Modpack!");
            German.Add("NotifyIfSameDatabaseText", "Benachrichtigung wenn es keine Aktuallisierungen für die Datenbank gibt");
            German.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Erhalte eine Benachrichtigung wenn es keine Aktualisierung der Datenbank gegeben hat und diese den selben Stand wie beim letzten Start aufweist.");
            German.Add("NotifyIfSameDatabaseCBDescription", "Benachrichtigt dich, wenn deine zuletzt installierte Datenbankversion dieselbe ist. Wenn ja, bedeutet dies, dass keine Modifikationen aktualisiert wurden.");
            German.Add("ShowInstallCompleteWindowText", "Zeige erweitertes Fenster bei abgeschlossener Installation");
            German.Add("ShowInstallCompleteWindowCBDescription", "Zeigte am Ende der Installation ein Auswahlfenster mit nützlichen Befehlen an," +
                " wie: starte das Spiel, gehe zur XVM Webseite, usw ...");
            German.Add("applicationVersion", "Programmversion");
            German.Add("databaseVersion", "Datenbank");
            German.Add("ClearCacheText", "Cache-Daten für WoT löschen");
            German.Add("ClearCacheCBDescription", "Lösche das WoT-Cache-App-Datenverzeichnis. Führt die gleiche Aufgabe wie die ähnliche Option aus, die in OMC war.");
            German.Add("UninstallDefaultDescription", "Die Standard Deinstallation wird alle Dateien in den Mod-Verzeichnissen des Spieles löschen, inklusive der" +
                " Verknüpfungen und Dateien im 'AppData' Speicher.");
            German.Add("UninstallQuickDescription", "Die schnelle Deinstallation wird nur Dateien in den Mod-Verzeichnissen der Spieles löschen." +
                " Es werden keine vom ModPack erstellten Verknüpfungen oder Dateien im 'AppData' Speicher gelöscht.");
            German.Add("DiagnosticUtilitiesButton", "Diagnosedienst");
            German.Add("DiagnosticUtilitiesButtonDescription", "Fehler melden, versuche eine Client Reparatur, etc.");
            German.Add("UninstallModeGroupBox", "Deinstallationsmodus:");
            German.Add("UninstallModeGroupBoxDescription", "Wähle den Deinstallationsmodus");
            German.Add("FacebookButtonDescription", "Unsere Facebook Seite aufrufen");
            German.Add("DiscordButtonDescription", "Zum Discord Server");
            German.Add("TwitterButtonDescription", "Unsere Twitter Seite aufrufen");
            German.Add("SendEmailButtonDescription", "Sende uns eine eMail (kein Modpack Support)");
            German.Add("HomepageButtonDescription", "Zu unserer Homepage");
            German.Add("DonateButtonDescription", "Spende für die Weiterentwicklung");
            German.Add("FindBugAddModButtonDescription", "Fehler gefunden? Willst Du einen Mod hinzufügen? Bitte hier melden!");
            German.Add("SelectionViewGB", "Darstellungsart");
            German.Add("SelectionDefaultText", "Standard");
            German.Add("SelectionLayoutDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            German.Add("SelectionDefaultDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            German.Add("SelectionLegacyDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            German.Add("LanguageSelectionGBDescription", "wähle Deine bevorzugte Sprache");
            German.Add("EnableBordersDefaultV2Text", "Einrahmen");
            German.Add("EnableBordersLegacyText", "Einrahmen");
            German.Add("EnableBordersDefaultV2CBDescription", "Jede Auswahl schwarz einrahmen");
            German.Add("EnableBordersLegacyCBDescription", "Jede Auswahl schwarz einrahmen");
            German.Add("UseBetaDatabaseText", "Beta-Datenbank");
            German.Add("UseBetaDatabaseCBDescription", "Verwende die letzte Beta-Version der ModPack-Datenbank. Die Stabilität der Mods kann nicht garantiert werden, jedoch werden hier auch Fehlerbehebungen als erstes getestet und implementiert.");
            German.Add("UseBetaApplicationText", "Beta-Version des Installers");
            German.Add("UseBetaApplicationCBDescription", "Verwende die letzte Beta-Version des ModPack Managers. Fehlerfreie Übersetzungen und Programmstabilität können nicht garantiert werden.");
            German.Add("SettingsTabIntroHeader", "Willkommen");
            German.Add("SettingsTabSelectionViewHeader", "Auswahlansicht");
            German.Add("SettingsTabInstallationSettingsHeader", "Installationseinstellungen");
            German.Add("SettingsTabApplicationSettingsHeader", "Programmeinstellungen");
            German.Add("SettingsTabAdvancedSettingsHeader", "Erweitert");
            German.Add("MainWindowSelectSelectionFileToLoad", "Wähle die zu ladende Auswahldatei");
            German.Add("verifyUninstallHeader", "Bestätigung");
            German.Add("verifyUninstallVersionAndLocation", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            German.Add("failedVerifyFolderStructure", "Das Programm konnte die erforderliche Ordnerstruktur nicht erstellen. Bitte überprüfe die Berechtigungen oder verschiebe das Programm an einen Ort mit Schreibrechten.");
            German.Add("failedToExtractUpdateArchive", "Das Programm konnte die Updatedateien nicht entpacken. Bitte überprüfe die Berechtigungen und/oder deine Anti-Virus-Software.");
            German.Add("downloadingUpdate", "Lade Programmupdate");
            //(removed components, disabled components, etc.)
            German.Add("AutoOneclickSelectionErrorsContinueBody", "Es gibt Probleme mit deiner Auswahldatei. Ein oder mehrere Einträge sind derzeit nicht verfügbar." +
                "\nWillst du fortfahren?");
            German.Add("AutoOneclickSelectionErrorsContinueHeader", "Probleme beim Laden der Auswahldatei");
            German.Add("noAutoInstallWithBeta", "Die automatische Installation kann nicht gemeinsam mit der Beta-Datenbank genutzt werden.");
            German.Add("autoInstallWithBetaDBConfirmBody", "Die automatische Installation wird jetzt die Beta Datenbank nutzen. Diese wird häufig aktualisiert und es" +
                " kann zu mehreren Installationen am Tag kommen. Willst du das wirklich tun?");
            German.Add("autoInstallWithBetaDBConfirmHeader", German["verifyUninstallHeader"]);
            German.Add("ColorDumpSaveFileDialog", "Wähle, wo die Farbdatei gespeichert werden soll");
            //"branch" is this context is git respoitory branches
            German.Add("loadingBranches", "Lade Branch");
            //"branch" is this context is git respoitory branches
            German.Add("failedToParseUISettingsFile", "Fehler beim Anwenden. Überprüfe log für Details. Aktiviere \"Ausführliche Protokollierung\" für erweiterte informationen.");
            German.Add("UISettingsFileApplied", "Thema angewendet");
            German.Add("failedToFindWoTExe", "Konnte den Installationsort des WoT Clienten nicht finden. Bitte sende einen Fehlerbericht an den Entwickler.");
            German.Add("failedToFindWoTVersionXml", "Fehler beim Abrufen der Versionsinformationen für die WoT-Client-Installation. Überprüfe, ob die Datei 'version.xml' im Verzeichnis 'World_of_Tanks' vorhanden ist");
            #endregion

            #region ModSelectionList
            German.Add("ModSelectionList", "Auswahldatei");
            German.Add("ContinueButtonLabel", "Installieren");
            German.Add("CancelButtonLabel", German["cancel"]);
            German.Add("HelpLabel", "Klicke mit der rechten Maustaste auf eine Auswahlkomponente, um ein Vorschaufenster anzuzeigen");
            German.Add("LoadSelectionButtonLabel", "Auswahl laden");
            German.Add("SaveSelectionButtonLabel", "Auswahl speichern");
            German.Add("SelectSelectionFileToSave", "Speichere Asuwahldatei");
            German.Add("ClearSelectionsButtonLabel", "Auswahl löschen");
            German.Add("SearchThisTabOnlyCB", "Suche nur in diesem Tab");
            German.Add("searchComboBoxInitMessage", "Suche nach einem Paket...");
            German.Add("SearchTBDescription", "Du kannst auch nach mehreren Namensteilen suchen, getrennt durch ein * (Sternchen).\nZum Beispiel: config*willster419" +
                "  wird als Suchergebnis anzeigen: Willster419\'s Config");
            German.Add("InstallingAsWoTVersion", "Installation als WoT Version: {0}");
            German.Add("UsingDatabaseVersion", "Nutze Datenbank: {0} ({1})");
            German.Add("userMods", "Benutzermodifikationen");
            German.Add("FirstTimeUserModsWarning", "Auf dieser Registerkarte kannst du ZIP-Dateien auswählen, welche du zuvor im Ordner \"RelhaxUserMods\" hinterlegst. Es müssen Zip-Dateien sein und sollten als Stammverzeichnisordner einen Ordner des Verzeichnisses \"World_of_Tanks\" beinhalten (zB: mods, oder res_mods)");
            German.Add("downloadingDatabase", "Datenbank herunterladen");
            German.Add("readingDatabase", "Lese Datenbank");
            German.Add("loadingUI", "Lade Benutzerinterface");
            German.Add("verifyingDownloadCache", "Überprüfen der Dateiintegrität von ");
            German.Add("InstallProgressTextBoxDescription", "Der Fortschritt einer Installation wird hier angezeigt");
            German.Add("testModeDatabaseNotFound", "KRITISCH: Die Datanbank für den Testmodus wurde nicht gefunden:\n{0}");
            German.Add("duplicateMods", "KRITISCH: Doppelte Paket-ID erkannt");
            German.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden! \n\n In der Protokolldatei stehen weitere Informationen zu diesem Fehler");
            German.Add("configSaveSuccess", "Auswahl erfolgreich gespeichert");
            German.Add("selectConfigFile", "Eine zu ladende Auswahldatei finden");
            German.Add("configLoadFailed", "Die Auswahldatei konnte nicht geladen werden und wurde im Standardmodus geladen");
            German.Add("modNotFound", "Das Paket (ID = \"{0}\") wurde nicht in der Datenbank gefunden. Es könnte umbenannt oder entfernt worden sein");
            German.Add("modDeactivated", "Die folgenden Pakete sind deaktiviert und können nciht zur Installation gewählt werden");
            German.Add("modsNotFoundTechnical", "Die folgenden Pakete konnten nicht gefunden werden und wurden wahrscheinlich entfernt");
            German.Add("modsBrokenStructure", "Die folgenden Pakete wurden aufgrund Modifizierungen an der Datenbankstruktur deaktiviert und müssen neu ausgewählt werden.");
            German.Add("packagesUpdatedShouldInstall", "Die folgenden Pakete bekamen ein Update seitdem du das letzte Mal diese Auswahldatei geladen hast. Die Datei wurde mit den Änderungen neu gespeichert (und ein einmaliges Backup wurde erstellt). " +
                "Falls dies deine aktuelle Installation ist und du diese behalten möchtest, ist es empfehlenswert eine (Neu-)Installation auszuführen wenn du diese Nachricht liest.");
            German.Add("selectionFileIssuesTitle", "Auswahl Ladenachrichten");
            German.Add("selectionFormatOldV2", "Dieses Auswahldateiformat ist ein Vermächtnis der Version 2 und wird auf Version 3 aktualisiert. Eine V2-Sicherung wird erstellt.");
            German.Add("oldSavedConfigFile", "Die Konfigurationsdatei die benutzt wurde, wird in Zukunft immer ungenauer werden. Soll auf das neue Standardformat umgestellt werden? (Eine Sicherung des alten Formats erfolgt)");
            German.Add("prefrencesSet", "Bevorzugte Einstellungen");
            German.Add("selectionsCleared", "Auswahl gelöscht");
            German.Add("failedLoadSelection", "Konnte Auswahl nicht laden");
            German.Add("unknownselectionFileFormat", "Unbekannte Version der Auswahldatei");
            German.Add("ExpandAllButton", "Erweitere aktuelle Registerkarte");
            German.Add("CollapseAllButton", "Reduziere aktuelle Registerkarte");
            German.Add("InstallingTo", "Installiere nach: {0}");
            German.Add("selectWhereToSave", "Wähle aus, wo die Auswahldatei gespeichert werden soll");
            German.Add("updated", "aktualisiert");
            German.Add("disabled", "deaktiviert");
            German.Add("invisible", "unsichtbar");
            German.Add("SelectionFileIssuesDisplay", "Fehler beim Anwenden der Auswahldatei");
            German.Add("selectionFileIssues", German["SelectionFileIssuesDisplay"]);
            German.Add("selectionFileIssuesHeader", "Bitte lesen Sie die folgenden Meldungen zu Ihrer Auswahldatei");
            German.Add("VersionInfo", "Porgrammaktualisierung");
            German.Add("VersionInfoYesText", German["yes"]);
            German.Add("VersionInfoNoText", German["no"]);
            German.Add("NewVersionAvailable", "Neue version verfügbar");
            German.Add("HavingProblemsTextBlock", "Wenn du Probleme mit der Aktualisierung hast, bitte");
            German.Add("ManualUpdateLink", "Klick Hier");
            German.Add("loadingApplicationUpdateNotes", "Anwendungsaktualisierungsnotizen werden geladen...");
            German.Add("failedToLoadUpdateNotes", "Fehler beim Laden der Anwendungsaktualisierungsnotizen");
            German.Add("ViewUpdateNotesOnGoogleTranslate", "Sieh dir das auf Google Translate an");
            German.Add("VersionInfoAskText", "Willst du jetzt updaten?");
            German.Add("SelectDownloadMirrorTextBlock", "Wähle einen Download-Mirror");
            German.Add("SelectDownloadMirrorTextBlockDescription", "Dieser Mirror wird nur für Paketdownloads verwendet");
            German.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Dallas, USA");
            German.Add("downloadMirrorDe", "clanverwaltung.de, Frankfurt, Germany");
            #endregion

            #region Installer Messages
            German.Add("Downloading", "Wird heruntergeladen");
            German.Add("patching", "Patching");
            German.Add("done", "Fertig");
            German.Add("cleanUp", "Bereinige Ressourcen");
            German.Add("idle", "Leerlauf");
            German.Add("status", "Status:");
            German.Add("canceled", "Abgebrochen");
            German.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            German.Add("checkForUpdates", "Auf Updates prüfen");
            German.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            German.Add("loadingSettings", "Einstellungen laden");
            German.Add("loadingTranslations", "Laden der Übersetzungen");
            German.Add("loading", "Laden");
            German.Add("of", "von");
            German.Add("failedToDownload1", "Konnte Paket nicht herunterladen");
            German.Add("failedToDownload2", "Willst du den Download wiederholen, die Installation abbrechen oder weitermachen?");
            German.Add("failedToDownloadHeader", "Fehler beim Herunterladen");
            German.Add("failedManager_version", "Die aktuelle Beta-Anwendung ist veraltet und muss für einen stabilen Kanal aktualisiert werden. Derzeit ist keine neue Beta-Version online");
            German.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
            German.Add("fontsPromptInstallText", "Hast Du Admin-Rechte um Schriftarten zu installieren?");
            German.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
            German.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe den Relhax Manager erneut als Administrator aus.");
            German.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
            German.Add("failedCreateUpdateBat", "Updateprozess kann leider nicht erstellt werden.\n\nLösche bitte diese Datei von Hand:\n{0}\n\nbenennte diese" +
                " Datei:\n{1}\nin diese um:\n{2}\n\nDirekt zum Ordner springen?");
            German.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
            German.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutze die 'erzwinge manuelle Spielerkennung' Option");
            German.Add("anotherInstanceRunning", "Eine weitere Instanz des Relhax Manager  läuft bereits");
            German.Add("closeInstanceRunningForUpdate", "Bitte schließe ALLE laufenden Instanzen des Relhax Managers bevor wir fortfahren und aktualisieren können.");
            German.Add("skipUpdateWarning", "Du überspringst die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
            German.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten." +
                " Wenn Du ein Datenbankmanager bist, füge bitte das Befehlsargument hinzu");
            German.Add("configNotExist", "{0} existiert nicht, lädt im regulären Modus");
            German.Add("autoAndFirst", "Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulären Modus");
            German.Add("confirmUninstallHeader", "Bestätigung");
            German.Add("confirmUninstallMessage", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            German.Add("uninstallingText", "Deinstalliere...");
            German.Add("uninstallingFile", "Deinstalliere datei");
            German.Add("uninstallFinished", "Deinstallation der mods beendet.");
            German.Add("uninstallFail", "Das Deinstallieren war nicht erfolgreich. Bitte wiederhole den Vorgang oder sende einen Fehlerbericht.");
            German.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder deine Datei-" +
                " und Ordnersicherheitsberechtigungen sind falsch.");
            German.Add("extractionErrorHeader", German["error"]);
            German.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            German.Add("deleteErrorMessage", "Bitte schließe alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicke auf OK um fortzufahren.");
            German.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            German.Add("noUninstallLogHeader", "Entferne alle Mods");
            German.Add("moveOutOfTanksLocation", "Das Modpack kann nicht aus dem World_of_Tanks Verzeichnis laufen. Bitte verschiebe die Anwendung in ein" +
                " anderes Verzeichnis und versuche es erneut.");
            German.Add("moveAppOutOfDownloads", "Die Anwendung hat festgestellt, dass sie über den Ordner \"Downloads\" gestartet wurde. Dies wird nicht empfohlen, da die Anwendung mehrere Ordner und Dateien erstellt." +
                "Sie sollten die Anwendung und alle 'Relhax'-Dateien und -Ordner in einen eigenen neuen Ordner verschieben.");
            German.Add("DatabaseVersionsSameBody", "Die Datenbank  wurde seit deiner letzten Installation nicht verändert. Daher gibt es keine Aktuallisierungen zu deinen aktuellen" +
                " Modifikationen. Trotzdem fortfahren?");
            German.Add("DatabaseVersionsSameHeader", "Datenbank Version ist identisch");
            German.Add("databaseNotFound", "Datenbank nicht an der angegebenen URL gefunden");
            German.Add("detectedClientVersion", "Erkannte Client Version");
            German.Add("supportedClientVersions", "Unterstützte Clients");
            German.Add("supportNotGuarnteed", "Diese Client Version wird (noch) nicht offiziell unterstützt. Die Mods könnten nicht funktionieren oder sogar Dein World of Tanks beschädigen.");
            German.Add("couldTryBeta", "Falls gerade ein Spiel-Patch veröffentlicht wurde, arbeitet das Team an einer Unterstützung der aktuellen Version. Du kannst versuchen die Beta-Datenbank zu nutzen.");
            German.Add("missingMSVCPLibrariesHeader", "Fehler beim Laden erforderlicher Bibliotheksdateien");
            German.Add("missingMSVCPLibraries", "Die Bibliotheken für die Konturbildverarbeitung konnten nicht geladen werden. Das könnte auf ein fehlendes Microsoft DLL-Paket hinweisen.");
            German.Add("openLinkToMSVCP", "Die Seite für den Paketdownload im Browser anzeigen?");
            German.Add("noChangeUntilRestart", "Diese Option hat keine Auswirkungen bis das Programm neu gestartet wurde");
            German.Add("installBackupMods", "Mod Dateien sichern");
            German.Add("installBackupData", "Benutzerdaten sichern");
            German.Add("installClearCache", "WoT Zwischenspeicher löschen");
            German.Add("installClearLogs", "Protokolldateien löschen");
            German.Add("installCleanMods", "Bereinige Mods-Ordner");
            German.Add("installExtractingMods", "Entpacke Paket");
            German.Add("installZipFileEntry", "Dateneingang");
            German.Add("installExtractingCompletedThreads", "Vollständige Enpackungsvorgänge");
            German.Add("installExtractingOfGroup", "der Installations Gruppe");
            German.Add("extractingUserMod", "Entpacke Benutzermod");
            German.Add("installRestoreUserdata", "Benutzerdaten wiederherstellen");
            German.Add("installXmlUnpack", "Entpacke XML Datei");
            German.Add("installPatchFiles", "Datei wird geändert");
            German.Add("installShortcuts", "Erstelle Verknüpfungen");
            German.Add("installContourIconAtlas", "Erstelle Atlas Datei");
            German.Add("installFonts", "Installieren von Schriftarten");
            German.Add("installCleanup", "Räume auf");
            German.Add("AtlasExtraction", "Entpacke Atlas Datei");
            German.Add("copyingFile", "Kopieren von Dateien");
            German.Add("deletingFile", "Lösche Datei");
            German.Add("scanningModsFolders", "Durchsuche Mod Verzeichnisse ...");
            German.Add("file", "Datei");
            German.Add("size", "Größe");
            German.Add("checkDatabase", "Durchsuche das Dateiarchive nach veralteten oder nicht mehr benötigten Dateien");
            German.Add("parseDownloadFolderFailed", "Durchsehen des \"{0}\" Verzeichnisses ist fehlgeschlagen.");
            German.Add("installationFinished", "Die Installation ist abgeschlossen");
            German.Add("deletingFiles", "Lösche Dateien");
            German.Add("uninstalling", "Deinstallieren");
            German.Add("zipReadingErrorHeader", "Unvollständiger Download");
            German.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            German.Add("zipReadingErrorMessage3", "konnte nicht gelesen werden.");
            German.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn du diese Meldung wieder siehst," +
                " mußt du deine Datei- und Ordnersicherheitsberechtigungen reparieren.");
            German.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            German.Add("folderDeleteFailed", "Löschen des Verzeichnis fehlgeschlagen");
            German.Add("fileDeleteFailed", "Löschen der Datei fehlgeschlagen");
            German.Add("DeleteBackupFolder", "Sicherungen");
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            German.Add("installFailed", "Die Installation misslang bei folgendem Schritt:");
            #endregion

            #region Install finished window
            German.Add("InstallFinished", "Installation abgeschlossen");
            German.Add("InstallationCompleteText", "Installation ist beendet. ");
            German.Add("InstallationCompleteStartWoT", "Spiel starten (WorldofTanks.exe)");
            German.Add("InstallationCompleteStartGameCenter", "WG Game Center");
            German.Add("InstallationCompleteOpenXVM", "Öffne die XVM Login Webseite");
            German.Add("InstallationCompleteCloseThisWindow", "Schließe dieses Fenster");
            German.Add("InstallationCompleteCloseApp", "Anwendung schließen");
            German.Add("xvmUrlLocalisation", "de");
            German.Add("CouldNotStartProcess","Konnte Prozess nicht starten");
            #endregion

            #region Diagnostics
            German.Add("Diagnostics", "Diagnose");
            German.Add("DiagnosticsMainTextBox", "Du kannst mit den folgenden Optionen Probleme mit dem Spiel diagnostizieren und ggf. beheben.");
            German.Add("LaunchWoTLauncher", "Starte den \"World of Tanks Launcher\" im Integritätsvalidierungsmodus");
            German.Add("CollectLogInfo", "Sammle und packe Protokolldateien in einer ZIP-Datei um ein Problem zu melden");
            German.Add("CollectLogInfoButtonDescription", "Sammelt alle notwendigen Log-Dateien in ein .zip-Datei. \nDas macht es dir einfacher ein Problem zu melden.");
            German.Add("DownloadWGPatchFilesText", "Lade WG Patchdateien für jeden WG Client über HTTP runter");
            German.Add("DownloadWGPatchFilesButtonDescription", "Führt dich durch die Auswahl und lädt Patch-Dateien für Wargaming-Spiele (WoT, WoWs, WoWp) über HTTP herunter, damit du sie später installieren kannst. \n" +
                 "Besonders nützlich für Leute, die das standardmäßige P2P-Protokoll von Wargaming Game Center nicht verwenden können.");
            German.Add("SelectedInstallation", "Aktuell gewählte Installation:");
            German.Add("SelectedInstallationNone", "("+German["none"].ToLower()+")");
            German.Add("collectionLogInfo", "Sammeln der Protokolldateien...");
            German.Add("startingLauncherRepairMode", "Starte den WoT Launcher im Integritätsvalidierungsmodus...");
            German.Add("failedCreateZipfile", "Fehler beim Erstellen der Zip-Datei ");
            German.Add("launcherRepairModeStarted", "Reparaturmodus wurde erfolgreich gestartet");
            German.Add("ClearDownloadCache", "Lösche Downlaod Cache");
            German.Add("ClearDownloadCacheDatabase", "Lösche Datenbank-Cache");
            German.Add("ClearDownloadCacheDescription", "Lösche alle Daten aus dem \"RelhaxDownloads\" Ordner");
            German.Add("ClearDownloadCacheDatabaseDescription", "Lösche die XML-Datenbankdatei. Dadurch werden alle ZIP-Dateien erneut auf Integrität überprüft. \nAlle ungültigen Dateien werden erneut heruntergeladen, falls diese bei deiner nächsten Installation ausgewählt werden.");
            German.Add("clearingDownloadCache", "Bereinige Download Cache");
            German.Add("failedToClearDownloadCache", "Fehler beim Bereinigen des Download Cache");
            German.Add("cleaningDownloadCacheComplete", "Download Cache gelöscht");
            German.Add("clearingDownloadCacheDatabase", "Lösche XML Datenbank Cache Datei");
            German.Add("failedToClearDownloadCacheDatabase", "Fehler beim löschen der XML Datenbank Cache Datei");
            German.Add("cleaningDownloadCacheDatabaseComplete", "XML Datenbank Cache Datei gelöscht");
            German.Add("ChangeInstall", "Ändere die aktuell ausgewählte WoT-Installation");
            German.Add("ChangeInstallDescription", "Dadurch wird geändert, welche Protokolldateien zur ZIP-Diagnosedatei hinzugefügt werden");
            German.Add("zipSavedTo", "Zip-Datei gespeichert in: ");
            German.Add("selectFilesToInclude", "Wähle Dateien um diese dem Fehlerbericht hinzuzufügen");
            German.Add("TestLoadImageLibraries", "Teste das Laden der Bibliotheken zur Atlasverarbeitung");
            German.Add("TestLoadImageLibrariesButtonDescription", "Testet die Atlas-Bildverarbeitungsbibliotheken");
            German.Add("loadingAtlasImageLibraries", "Lade Bibliotheken zur Atlasverarbeitung");
            German.Add("loadingAtlasImageLibrariesSuccess", "Erolgreiches Laden der Bibliotheken zur Atlasverarbeitung");
            German.Add("loadingAtlasImageLibrariesFail", "Fehler beim Laden der Bibliotheken zur Atlasverarbeitung");
            German.Add("CleanupModFilesText", "Bereinige falsch platzierte Moddaten");
            German.Add("CleanupModFilesButtonDescription", "Löscht alle Moddaten aus den win32 und win64 Ordnern um Konflikte zu vermeiden");
            German.Add("cleanupModFilesCompleted", "Bereinigen der Moddaten abgeschlossen");
            German.Add("CleanGameCacheText", "Lösche Cache des Spiels");
            German.Add("cleanGameCacheProgress", "Lösche Spielcache");
            German.Add("cleanGameCacheSuccess", "Spielcache erfolgreich gelöscht");
            German.Add("cleanGameCacheFail", "Konnte Cache-Dateien nicht löschen");
            German.Add("TrimRelhaxLogfileText", "Kürze die Relhax-Protokolldatei auf die letzten drei Starts");
            German.Add("trimRelhaxLogProgress", "Kürze die Relhax-Protokolldatei");
            German.Add("trimRelhaxLogSuccess", "Die Relhax-Protokolldatei wurde erfolgreich gekürzt");
            German.Add("trimRelhaxLogFail", "Fehler beim Kürzen der Relhax-Protokolldatei");
            #endregion

            #region Wot Client install selection
            German.Add("WoTClientSelection", "Auswahl des WoT-Clients");
            German.Add("ClientSelectionsTextHeader", "Die folgenden Client-Installationen wurden automatisch erkannt");
            German.Add("ClientSelectionsCancelButton", German["cancel"]);
            German.Add("ClientSelectionsManualFind", "Manuelle Auswahl");
            German.Add("ClientSelectionsContinueButton", German["select"]);
            German.Add("AddPicturesZip", "Füge Dateien zum ZIP-Archiv hinzu");
            German.Add("DiagnosticsAddSelectionsPicturesLabel", "Füge zusätzliche Dateien hinzu (deine Auswahldatei, Bilder, etc.)");
            German.Add("DiagnosticsAddFilesButton", "Dateien hinzufügen");
            German.Add("DiagnosticsRemoveSelectedButton", "Entferne Ausgewähltes");
            German.Add("DiagnosticsContinueButton", German["ContinueButton"]);
            German.Add("cantRemoveDefaultFile", "Kann keine Standard Dateien entfernen");
            #endregion

            #region Preview Window
            German.Add("Preview", "Vorschau");
            German.Add("noDescription", "Keine Beschreibung verfügbar");
            German.Add("noUpdateInfo", "Keine Aktualisierungsinformationen verfügbar");
            German.Add("noTimestamp", "Kein Zeitstempel verfügbar");
            German.Add("PreviewNextPicButton", German["next"]);
            German.Add("PreviewPreviousPicButton", German["previous"]);
            German.Add("DevUrlHeader", "Entwickler-Link");
            German.Add("dropDownItemsInside", "Gegenstände im Inneren");
            German.Add("popular", "beliebt");
            German.Add("previewEncounteredError", "Das Vorschaufenster stellte einen Fehler fest und kann die Vorschau nicht laden");
            German.Add("popularInDescription", "Dies ist ein popüläres Paket");
            German.Add("controversialInDescription", "Dies ist ein umstrittenes Paket");
            German.Add("encryptedInDescription", "Dies ist ein  verschlüsseltes Paket, welches nicht auf Viren überprüft werden kann");
            German.Add("fromWgmodsInDescription", "Die Quelle dieses Pakets ist das WGmods Portal (wgmods.net)");
            #endregion

            #region Developer Selection Window
            German.Add("DeveloperSelectionsViewer", "Auswahl-Betrachter");
            German.Add("DeveloperSelectionsTextHeader", "Auswahl zum Laden");
            German.Add("DeveloperSelectionsCancelButton", German["cancel"]);
            German.Add("DeveloperSelectionsLocalFile", "Lokale Datei");
            German.Add("DeveloperSelectionsContinueButton", "Auswahl bestätigen");
            German.Add("failedToParseSelections", "Auswahl konnte nicht analysiert werden");
            German.Add("lastModified", "Zuletzt geändert");
            #endregion

            #region Advanced Installer Window
            German.Add("AdvancedProgress", "Erweiterte Instaltionsfortschrittsnanziege");
            German.Add("PreInstallTabHeader", "Vorbereitung");
            German.Add("ExtractionTabHeader", "Entpackungsvorgang");
            German.Add("PostInstallTabHeader", "Abschlußarbeiten");
            German.Add("AdvancedInstallBackupData", "Sicherungskopie der Mod-Daten");
            German.Add("AdvancedInstallClearCache", "Lösche den WoT Cache");
            German.Add("AdvancedInstallClearLogs", "Lösche Protokolldateien");
            German.Add("AdvancedInstallClearMods", "Deinstalliere letzte Installation");
            German.Add("AdvancedInstallInstallMods", "Installiere Thread");
            German.Add("AdvancedInstallUserInstallMods", "Benutzerinstallation");
            German.Add("AdvancedInstallRestoreData", "Daten widerherstellen");
            German.Add("AdvancedInstallXmlUnpack", "XML Entpacker");
            German.Add("AdvancedInstallPatchFiles", "Patch Dateien");
            German.Add("AdvancedInstallCreateShortcuts", "Erstelle Verknüpfungen");
            German.Add("AdvancedInstallCreateAtlas", "Erstelle Atlase");
            German.Add("AdvancedInstallInstallFonts", "Installiere Schriftarten");
            German.Add("AdvancedInstallTrimDownloadCache", "Download Cache kürzen");
            German.Add("AdvancedInstallCleanup", "Aufräumen");
            #endregion

            #region News Viewer
            German.Add("NewsViewer", "Nachrichten Viewer");
            German.Add("application_Update_TabHeader", "App");
            German.Add("database_Update_TabHeader", "Datenbank");
            German.Add("ViewNewsOnGoogleTranslate", "Sieh das auf Google Translate an");
            #endregion

            #region Loading Window
            German.Add("ProgressIndicator", "Lade");
            German.Add("LoadingHeader", "Lade, bitte warten");
            #endregion

            #region First Load Acknowledgements
            German.Add("FirstLoadAcknowledgments", "Einverständniserklärung");
            German.Add("AgreementLicense", "Von mir gelesen und bestätigt: ");
            German.Add("LicenseLink", "Lizenzvereinbarung");
            German.Add("AgreementSupport1", "Ich erhalte Support nur über das dafür bereitgestellte ");
            German.Add("AgreementSupportDiscord", "Discord");
            German.Add("AgreementHoster", "Ich verstehe, dass Relhax ein Mod-Hosting- und Installationsservice ist und Relhax nicht alle Mods verwaltet, die in diesem Modpack enthalten sind");
            German.Add("AgreementAnonData", "Ich verstehe, dass Relhax V2 anonyme Nutzungsdaten sammelt, um die Anwendung zu verbessern, und auf der Registerkarte für erweiterte Einstellungen deaktiviert werden kann.");
            German.Add("V2UpgradeNoticeText", "Es sieht so aus, als würdest du zum ersten Mal ein Upgrade von V1 auf V2 ausführen.\n" +
                "Wenn du auf Fortsetzen klickst, wird ein Upgrade der Dateistruktur durchgeführt, das nicht wiederhergestellt werden kann. Es wird empfohlen, eine Sicherungskopie deines V1-Ordners zu erstellen, bevor du fortfährst");
            German.Add("upgradingStructure", "Upgrad der V1 Datei- und Ordnerstruktur");
            #endregion

            #region Export Mode
            German.Add("ExportModeSelect", "Wähle WoT Client für den Export");
            German.Add("selectLocationToExport", "Wähle den Ordner für den Export der Mod-Installation");
            German.Add("ExportSelectVersionHeader", "Bitte wähle die WoT Klientversion, für die du den Export durchführen willst");
            German.Add("ExportContinueButton", German["ContinueButton"]);
            German.Add("ExportCancelButton", German["cancel"]);
            German.Add("ExportModeMajorVersion", "Version des Online Ordners");
            German.Add("ExportModeMinorVersion", "WoT Version");
            #endregion

            #region Asking to close WoT
            German.Add("AskCloseWoT", "WoT läuft bereits");
            German.Add("WoTRunningTitle", "WoT wird gerade ausgeführt.");
            German.Add("WoTRunningHeader", "Es sieht so aus als wäre WoT geöffnet. Bitte schließe das Programm um fortzufahren");
            German.Add("WoTRunningCancelInstallButton", "Abbruch der Installation");
            German.Add("WoTRunningRetryButton", "Neuerkennung");
            German.Add("WoTRunningForceCloseButton", "Erzwinge das Schließen des Spiels");
            #endregion

            #region Scaling Confirmation
            German.Add("ScalingConfirmation", "Bestätige Skalierung");
            German.Add("ScalingConfirmationHeader", "Der Wert für die Skalierung hat sich geändert. Willst du dies beibehalten?");
            German.Add("ScalingConfirmationRevertTime", "Rückgängig machen in {0} Sekunde(n)");
            German.Add("ScalingConfirmationKeep", "Behalten");
            German.Add("ScalingConfirmationDiscard", "Verwerfen");
            #endregion

            #region Game Center download utility
            German.Add("GameCenterUpdateDownloader", "Game Center Update Downloader");
            German.Add("GcDownloadStep1Header", "Wähle Spielclient");
            German.Add("GcDownloadStep1TabDescription", "Wähle den Wargaming CLient um Daten für (WoT, WoWS, WoWP) zu sammeln");
            German.Add("GcDownloadStep1SelectClientButton", "Wähle Client");
            German.Add("GcDownloadStep1CurrentlySelectedClient", "Derzeit gewählter Client: {0}");
            German.Add("GcDownloadStep1NextText", German["next"]);
            German.Add("GcDownloadStep1GameCenterCheckbox", "Suche stattdessen nach Game Center Updates");
            German.Add("GcDownloadSelectWgClient", "Wähle WG Client");
            German.Add("ClientTypeValue", "Nichts");
            German.Add("LangValue", German["ClientTypeValue"]);
            German.Add("GcMissingFiles", "Deinem Clienten fehlen die folgenden XML Definitionsdateien");
            German.Add("GcDownloadStep2Header", "Schließe Game Center");
            German.Add("GcDownloadStep2TabDescription", "Schließe das Spiel (das Programm erkennt das Schließen)");
            German.Add("GcDownloadStep2GcStatus", "Game Center ist {0}");
            German.Add("GcDownloadStep2GcStatusOpened", "geöffnet");
            German.Add("GcDownloadStep2GcStatusClosed", "geschlossen");
            German.Add("GcDownloadStep2PreviousText", German["previous"]);
            German.Add("GcDownloadStep2NextText", German["next"]);
            German.Add("GcDownloadStep3Header", "Hole Update Information");
            German.Add("GcDownloadStep3TabDescription", "Hole die Liste der Patchdateien für den Download");
            German.Add("GcDownloadStep3NoFilesUpToDate", "Keine Patchdateien zum Download (aktuell)");
            German.Add("GcDownloadStep3PreviousText", German["previous"]);
            German.Add("GcDownloadStep3NextText", German["next"]);
            German.Add("GcDownloadStep4Header", "Lade Dateien für das Update runter");
            German.Add("GcDownloadStep4TabDescription", "Lade Patchdateien runter...");
            German.Add("GcDownloadStep4DownloadingCancelButton", German["cancel"]);
            German.Add("GcDownloadStep4DownloadingText", "Lade Patch {0} von {1} runter: {2}");
            German.Add("GcDownloadStep4DownloadComplete", "Paketdownload abgeschlossen");
            German.Add("GcDownloadStep4PreviousText", German["previous"]);
            German.Add("GcDownloadStep4NextText", German["next"]);
            German.Add("GcDownloadStep5Header", "Abgeschlossen");
            German.Add("GcDownloadStep5TabDescription", "Der Vorgang is fertig. Das Game Center sollte die Dateien beim Öffnen erkennen.");
            German.Add("GcDownloadStep5CloseText", German["close"]);
            German.Add("FirstLoadSelectLanguage", "Sprachauswahl");
            German.Add("SelectLanguageHeader", "Bitte wähle deine Sprache");
            German.Add("SelectLanguagesContinueButton", German["ContinueButton"]);
            German.Add("Credits", "Relhax Modpack Danksagung");
            German.Add("creditsProjectLeader", "Projektleiter");
            German.Add("creditsDatabaseManagers", "Datenbank Manager");
            German.Add("creditsTranslators", "Übersetzer");
            German.Add("creditsusingOpenSourceProjs", "Relhax Modpack nutzt folgende Open-Source Projekte");
            German.Add("creditsSpecialThanks", "Besonderer Dank");
            German.Add("creditsGrumpelumpf", "Projektleiter des OMC Modpack, hat Relhax erlaubt dort weiter zu machen wo er aufgehört hat");
            German.Add("creditsRkk1945", "Der erste Betatester, der mir über Monate half das Projekt zu auf die Beine zu stellen");
            German.Add("creditsRgc", "Sponsor des Modpacks und meine erste Betatestgruppe");
            German.Add("creditsBetaTestersName", "Unser Betatest Team");
            German.Add("creditsBetaTesters", "Kontinuierlich Probleme in der Anwendung zu testen und zu melden, bevor sie live geschaltet wird");
            German.Add("creditsSilvers", "Hilfe bei der Öffentlichkeitsarbeit und sozialen Netzwerken");
            German.Add("creditsXantier", "Erster IT-Support und Einrichtung unseres Servers");
            German.Add("creditsSpritePacker", "Entwicklung des Sprite Sheet Packer-Algorithmus und Portierung nach .NET");
            German.Add("creditsWargaming", "Ein einfach zu automatisierendes Modding-System");
            German.Add("creditsUsersLikeU", "Benutzer wie du");
            #endregion

            #region Conflicting Packages Dialog
            German.Add("ConflictingPackageDialog", "Widersprüchliche  Beschreibung");
            German.Add("conflictingPackageMessageOptionA", "Option A");
            German.Add("conflictingPackageMessageOptionB", "Option B");
            German.Add("conflictingPackageMessagePartA", "Du hast \"{0}\" ausgewählt, aber es widerspricht der folgenden Auswahl:");
            German.Add("conflictingPackagePackageOfCategory", "- {0}, der Kategorie {1}");
            German.Add("conflictingPackageMessagePartB", German["conflictingPackageMessageOptionA"] + ": Wähle \"{0}\" aus, das alle Widersprüche in der Auswahl abwählt.");
            German.Add("conflictingPackageMessagePartC", German["conflictingPackageMessageOptionB"] + ": Wähle \"{0}\" nicht aus, dann bleiben alle Widersprüche in der Auswahl erhalten.");
            German.Add("conflictingPackageMessagePartD", "Schließe das Fenster, Option B wird ausgewählt");
            German.Add("conflictingPackageMessagePartE", "Bitte wähle eine Option aus");
            #endregion

            #region End of Life announcement
            German.Add("EndOfLife", "Relhax am Lebensende End of Life");
            German.Add("CloseWindowButton", German["close"]);
            German.Add("WoTForumAnnouncementsTextBlock", "WoT forum Bekanntmachung posts:");
            German.Add("endOfLifeMessagePart1", "Am 22.April 2022 wird das Relhax Modpack beendet. Ich will ein ganz besonderes Dankeschön ausrichten an alle Mitarbeitende und Benutzer für mehr als 5 erfolgreiche Jahre!");
            German.Add("endOfLifeMessagePart2a", "Am 1. Januar 2017, stellte ich mir selbst eine (die) Herausforderung nicht nur das alte (old) OMC Modpack mit einer modernen UI Wiederauferstehen zu lassen, sondern das schnellste Installationssystem aller existierenden Modpacks zu programmieren.");
            German.Add("endOfLifeMessagePart2b", "Ich begann mit einem Team aus 4 Leuten, davon waren 3 Mitglieder von OMC weiter beim Projekt \"Relhax\" dabei. In den folgenden 4 Jahre, entwickelte ich es , baute es zusammen, verwarf es , began es neu und baute es erneut zusammen, verbrachte dabei 10.000 Stunden am PC.");
            German.Add("endOfLifeMessagePart2c", "Zu einem Zeitpunkt wuchs das Team auf über 8 Personen aus allen Server Regionen von WoT. Während diese gesamten Prozess wuchs ich als Programmierer, lernte viel über Software im Allgemeinen und ganz speziell zu Anwendungen (\"Apps\") mit multi-threading und handling concurrent operations.");
            German.Add("endOfLifeMessagePart2d", "Ich sammelte Erfahrung durch das Projekt und konnte mit einer großartigen Modding-Community interagieren. Es ermöglichte mir meinen Beitrag zur Relic Gaming Community, der ich 2014 beitrat.");
            German.Add("endOfLifeMessagePart3a", "(Der Rest dieser Nachricht wurde mit Google Translate übersetzt. Entschuldigung für die Unannehmlichkeiten.) In diesem Jahr habe ich endlich meine Designarbeit am optimiertesten und effizientesten Installer abgeschlossen, den ich für die Community machen konnte.");
            German.Add("endOfLifeMessagePart3b", "Als ich sah, dass das Projekt mein ursprüngliches Ziel erreichte und mein Interesse am Spiel (und dem Projekt) nachließ, beschloss ich, das Projekt zu schließen.");
            German.Add("endOfLifeMessagePart3c", "Es war eine schwere Entscheidung, aber ich wollte ein Projekt nicht weiter unterstützen, an dessen Fortführung ich kein Interesse mehr hatte.");
            German.Add("endOfLifeMessagePart3d", "Ich denke, es hätte die Qualität des Produkts schlecht widergespiegelt und es wäre nicht fair gegenüber den Endverbrauchern. Ich wollte das Projekt schließen, während es noch in einem gesunden Zustand war.");
            German.Add("endOfLifeMessagePart4", "Nochmals vielen Dank an alle. Es waren lustige 5+ Jahre, und ich werde es vermissen.");
            #endregion
        }
    }
}
