using System;
using System.Collections;

namespace RelhaxModpack
{
    //A static class to exist throughout the entire application life, will always have translations
    public static class Translations
    {
        //Enumerator to determine which translated string to return
        public enum Languages { English = 0, German = 1, Polish = 2, French = 3 };
        public static Languages language = Languages.English;//set it to this default
        public static Hashtable english = new Hashtable();
        public static Hashtable german = new Hashtable();
        public static Hashtable polish = new Hashtable();
        public static Hashtable french = new Hashtable();
        //load hashes on application startup

        public static string getTranslatedString(string componetName)
        {
            try
            {
                string s = "";
                switch (language)
                {
                    case (Languages.English):
                        if (english.Contains(componetName))
                        {
                            return (string)english[componetName];
                        }
                        break;
                    case (Languages.German):
                        if (german.Contains(componetName))
                        {
                            s = (string)german[componetName];
                            if (s.ToUpper().Equals("TODO"))
                            {
                                Utils.AppendToLog(string.Format("WARNING: german translation for \"{0}\" is missing.", componetName));
                                s = (string)english[componetName];
                            }
                            return s;
                        }
                        break;
                    case (Languages.Polish):
                        if (polish.Contains(componetName))
                        {
                            s = (string)polish[componetName];
                            if (s.ToUpper().Equals("TODO"))
                            {
                                Utils.AppendToLog(string.Format("WARNING: polish translation for \"{0}\" is missing.", componetName));
                                s = (string)english[componetName];
                            }
                            return s;
                        }
                        break;
                    case (Languages.French):
                        if (french.Contains(componetName))
                        {
                            s = (string)french[componetName];
                            if (s.ToUpper().Equals("TODO"))
                            {
                                Utils.AppendToLog(string.Format("WARNING: french translation for \"{0}\" is missing.", componetName));
                                s = (string)english[componetName];
                            }
                            return s;
                        }
                        break;
                }
                Utils.AppendToLog(string.Format("ERROR: no value in language hash for key: {0}  Language: {1}", componetName, language));
                return componetName;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("getTranslatedString", string.Format("key: \"{0}\"  Language: english", componetName), ex);
                return componetName;
            }

        }
        //method to load each translated string based on which language is selected
        public static void loadHashes()
        {
            //Syntax is as follows:
            //languageName.Add("componetName","TranslatedString");
            //General expressions
            english.Add("yes", "yes");
            german.Add("yes", "ja");
            polish.Add("yes", "Tak");
            french.Add("yes", "Oui");

            english.Add("no", "no");
            german.Add("no", "nein");
            polish.Add("no", "Nie");
            french.Add("no", "Non");

            english.Add("cancel", "Cancel");
            german.Add("cancel", "Abbrechen");
            polish.Add("cancel", "Anuluj");
            french.Add("cancel", "Anuler");

            english.Add("warning", "WARNING");
            german.Add("warning", "WARNUNG");
            polish.Add("warning", "OSTRZEŻENIE");
            french.Add("warning", "ATTENTION");

            english.Add("critical", "CRITICAL");
            german.Add("critical", "KRITISCH");
            polish.Add("critical", "BŁĄD KRYTYCZNY");
            french.Add("critical", "CRITIQUAL");
      

            english.Add("information", "Information");
            german.Add("information", "Information");
            polish.Add("information", "Informacja");
            french.Add("information", "information");

            english.Add("select", "Select");
            german.Add("select", "Auswählen");
            polish.Add("select", "Wybierz");
            french.Add("select", "Sélectionner");

            english.Add("abort", "Abort");
            german.Add("abort", "Abbrechen");
            polish.Add("abort", "Anulować");
            french.Add("abort", "Annuler");

            english.Add("retry", "Retry");
            german.Add("retry", "Wiederholen");
            polish.Add("retry", "Spróbować ponownie");
            french.Add("retry", "Reaissayer");

            english.Add("ignore", "Ignore");
            german.Add("ignore", "Ignorieren");
            polish.Add("ignore", "Ignorować");
            french.Add("ignore", "Ignorer");

            //Section: MainWindow

            //Componet: installRelhaxMod
            //The button for installing the modpack
            english.Add("installRelhaxMod", "Start Mod Selection");
            german.Add("installRelhaxMod", "Auswahl der Mods");
            polish.Add("installRelhaxMod", "Przejdź Do Wyboru Modyfikacji");
            french.Add("installRelhaxMod", "Sélection des mods");

            //Componet: uninstallRelhaxMod
            //
            english.Add("uninstallRelhaxMod", "Uninstall Relhax Modpack");
            german.Add("uninstallRelhaxMod", "Relhax Modpack deinstallieren");
            polish.Add("uninstallRelhaxMod", "Odinstaluj Paczkę Relhax");
            french.Add("uninstallRelhaxMod", "Désinstaller Relhax Modpack");

            //Componet: toolTip
            //MainForm
            english.Add("mainFormToolTip", "Right Click for extended description");
            german.Add("mainFormToolTip", "Rechtsklick für eine erweiterte Beschreibung");
            polish.Add("mainFormToolTip", "Rozwiń opis PPM");
            french.Add("mainFormToolTip", "TODO");
            

            //Componet: forceManuel
            //
            english.Add("forceManuel", "Force manual game detection");
            german.Add("forceManuel", "Erzwinge manuelle Spielerkennung");
            polish.Add("forceManuel", "Wymuś ręczną weryfikację ścieżki gry");
            french.Add("forceManuel", "Forcer détection manuel");

            //Componet: forceManuel
            //
            english.Add("languageSelectionGB", "Language Selection");
            german.Add("languageSelectionGB", "Sprachauswahl");
            polish.Add("languageSelectionGB", "Wybór języka");
            french.Add("languageSelectionGB", "Choix de langue");

            //Componet: formPageLink
            //
            english.Add("formPageLink", "View Modpack Form Page (NA)");
            german.Add("formPageLink", "Zeige Modpack Forumbeiträge (NA)");
            polish.Add("formPageLink", "Forum Paczki (NA)");
            french.Add("formPageLink", "Afficher la page du formulaire (NA)");

            //Componet: saveUserDataCB
            //
            english.Add("saveUserDataCB", "Save user data");
            german.Add("saveUserDataCB", "Mod Daten speichern");
            polish.Add("saveUserDataCB", "Zapisz ustawienia użytkownika");
            french.Add("saveUserDataCB", "Sauvegarder les données utilisateur");

            //Componet: cleanInstallCB
            //
            english.Add("cleanInstallCB", "Clean Installation (Recommended)");
            german.Add("cleanInstallCB", "Saubere Installation (empfohlen)");
            polish.Add("cleanInstallCB", "Czysta instalacja (Zalecane)");
            french.Add("cleanInstallCB", "Installation propre (Recommandé)");

            //Componet: cancerFontCB
            //
            english.Add("cancerFontCB", "Comic Sans Font");
            german.Add("cancerFontCB", "Comic Sans Schriftart");
            polish.Add("cancerFontCB", "Czcionka Comic Sans");
            french.Add("cancerFontCB", "Police Comic Sans");

            //Componet: backupModsCheckBox
            //
            english.Add("backupModsCheckBox", "Backup current mods folder");
            german.Add("backupModsCheckBox", "Sicherung des aktuellen Modsordner");
            polish.Add("backupModsCheckBox", "Zrób kopię zapasową obecnego pliku z modyfikacjami");
            french.Add("backupModsCheckBox", "Sauvegarder le dossier de mods");

            //Componet: settingsGroupBox
            //
            english.Add("settingsGroupBox", "Settings (Right-click for descriptions)");
            german.Add("settingsGroupBox", "Einstellungen (Rechts-Klick für eine Beschreibung)");
            polish.Add("settingsGroupBox", "Ustawienia (Opis pod PPM)");
            french.Add("settingsGroupBox", "Paramètres (clic droit pour les descriptions)");

            //Componet: darkUICB
            //
            english.Add("darkUICB", "Dark UI");
            german.Add("darkUICB", "Dunkle Benutzeroberfläche");
            polish.Add("darkUICB", "Ciemny Interfejs");
            french.Add("darkUICB", "Interface sombre");

            //Componet: cleanUninstallCB
            //
            english.Add("cleanUninstallCB", "Clean uninstallation");
            german.Add("cleanUninstallCB", "Saubere Deinstallation");
            polish.Add("cleanUninstallCB", "Czysta deinstalacja");
            french.Add("cleanUninstallCB", "Désinstallation propre");

            //Componet: saveLastInstallCB
            //
            english.Add("saveLastInstallCB", "Save last install\'s config");
            german.Add("saveLastInstallCB", "Speicherung der letzten Installation");
            polish.Add("saveLastInstallCB", "Zapisz ostatnią konfigurację instalacji");
            french.Add("saveLastInstallCB", "Sauvegarder la denière configuration");

            //Componet: largerFontButton
            //
            english.Add("largerFontButton", "Larger Font");
            german.Add("largerFontButton", "Grössere Schriftart");
            polish.Add("largerFontButton", "Większa czcionka");
            french.Add("largeFontButton", "Grande police");

            //Componet: loadingImageGroupBox
            //
            english.Add("loadingImageGroupBox", "Loading Image");
            german.Add("loadingImageGroupBox", "Ladebild");
            polish.Add("loadingImageGroupBox", "Ładowanie obrazka");
            french.Add("loadingImageGroupBox", "Image de chargement");

            //Componet: standardImageRB
            //
            english.Add("standardImageRB", "Standard");
            german.Add("standardImageRB", "Standard");
            polish.Add("standardImageRB", "Podstawowe");
            french.Add("standardImageRB", "Standard");

            //Componet: findBugAddModLabel
            //
            english.Add("findBugAddModLabel", "Find a bug? Want a mod added?");
            german.Add("findBugAddModLabel", "Fehler gefunden? Willst Du einen Mod hinzufügen?");
            polish.Add("findBugAddModLabel", "Znalazłeś błąd? Chcesz dodać mod?");
            french.Add("findBugAddModLabel", "Trouvé un bug? Recommandation de mod?");

            //Componet: cancelDownloadButton
            //
            english.Add("cancelDownloadButton", "Cancel Download");
            german.Add("cancelDownloadButton", "Download abbrechen");
            polish.Add("cancelDownloadButton", "Anuluj pobieranie");
            french.Add("cancelDownloadButton", "Anuler le téléchargement");

            //Componet: appDataFolderNotExistHeader
            //
            english.Add("appDataFolderNotExistHeader", "Could not detect WoT cache folder");
            german.Add("appDataFolderNotExistHeader", "Konnte den Cache-Ordner WoT nicht erkennen");
            polish.Add("appDataFolderNotExistHeader", "Nie wykryto foldera WoT cache");
            french.Add("appDataFolderNotExistHeader", "Impossible de détecter le dossier de cache WoT");


            //Componet: appDataFolderNotExist
            //
            english.Add("appDataFolderNotExist", "The installer could not detect the WoT cache folder. Continue the installation witout clearing WoT cache?");
            german.Add("appDataFolderNotExist", "Der Installer konnte den WoT-Cache-Ordner nicht erkennen. Installation fortsetzen ohne den WoT-Cache zu löschen?");
            polish.Add("appDataFolderNotExist", "Instalato nie wykrył foldera cache. Czy kontynuować bez czyszczenia folderu cache?");
            french.Add("appDataFolderNotExist", "L'installateur n'as pas pus détecter le dossier de cache WoT. Continuer l'installation sans nettoyer le cache?");

            //Componet: DiscordServerLink
            //
            english.Add("DiscordServerLink", "Discord Server");
            german.Add("DiscordServerLink", "Discord Server");
            polish.Add("DiscordServerLink", "Serwer Discorda");
            french.Add("DiscordServerLink", "Serveur Discord");

            //Componet: viewAppUpdates
            //
            english.Add("viewAppUpdates", "View latest application updates");
            german.Add("viewAppUpdates", "Programmaktualisierungen anzeigen");
            polish.Add("viewAppUpdates", "Pokaż ostatnie zmiany w aplikacji");
            french.Add("viewAppUpdates", "Afficher les dernières mises à jour de l'applications");

            //Componet: viewDBUpdates
            //
            english.Add("viewDBUpdates", "View latest database updates");
            german.Add("viewDBUpdates", "Datenbankaktualisierungen anzeigen");
            polish.Add("viewDBUpdates", "Pokaż ostatnie zmiany w bazie danych");
            french.Add("viewDBUpdates", "Afficher les dernières mises à jour de la base de données");

            //Componet: disableColorsCB
            //
            english.Add("disableColorsCB", "Disable color change");
            german.Add("disableColorsCB", "Farbwechsel deaktivieren");
            polish.Add("disableColorsCB", "Wyłącz zmianę kolorów");
            french.Add("disableColorsCB", "Désactiver le changement de couleur");

            //Componet: clearLogFilesCB
            //
            english.Add("clearLogFilesCB", "Clear log files");
            german.Add("clearLogFilesCB", "Protokolldatei löschen");
            polish.Add("clearLogFilesCB", "Wyczyść logi");
            french.Add("clearLogFilesCB", "Effacer les fichiers logs");

            //Componet: createShortcutsCB
            //
            english.Add("createShortcutsCB", "Create Desktop Shortcuts");
            german.Add("createShortcutsCB", "Erstelle Desktop Verknüpfungen");
            polish.Add("createShortcutsCB", "Stwórz skróty na pulpicie");
            french.Add("createShortcutsCB", "Créer des raccourcis sur le bureau");

            //Section: FirstLoadHelper

            //Componet: helperText
            //
            english.Add("helperText", "Welcome to the RelHax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over (or right click) a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            german.Add("helperText", "Willkommen im RelHax Modpack! Ich habe versucht, das Modpack so einfach wie möglich zu gestalten, aber Fragen können dennoch entstehen. Rechtsklick auf eine Einstellung erklärt diese dann. Du siehst diese Dialogbox nicht mehr, ausser du löscht die xml Datei.");
            polish.Add("helperText", "Witamy w paczce RelHax! Próbowałem stworzyć jak najprostszą w użytku paczkę modyfikacji, ale wciąż możesz mieć pytania. Kliknik PPM na opcji, by wyświetlić opis. Nie zobaczysz tej wiadomości ponownie, dopóki nie usuniesz pliku ustawień xml.");
            french.Add("helperText", "Bienvenue au ModPack Relhax! J'ai aissayé de faire le modpack le plus simple possible, mais des questions peuvent survenir. Survolez (ou cliquez droit) un paramètre pour voire une explication. Vous n'allez plus voire cette boite, sauf si vous supprimez le fichier de configuration xml ");

            //Component: donateLabel
            english.Add("donateLabel", "Donation for further development");
            german.Add("donateLabel", "Spende für die Weiterentwicklung");
            polish.Add("donateLabel", "Dotacja na dalszy rozwój");
            french.Add("donateLabel", "Donation pour aider au développement");

            //Component: notifyIfSameDatabaseCB
            english.Add("notifyIfSameDatabaseCB", "Inform if no new database available");
            german.Add("notifyIfSameDatabaseCB", "Hinweis wenn keine Aktuallisierungen erfolgt sind");
            polish.Add("notifyIfSameDatabaseCB", "Poinformuj, jeśli nie będzie dostępna nowa baza danych");
            french.Add("notifyIfSameDatabaseCB", "Informer si aucune nouvelle base de données est disponible");

            //Component: ShowInstallCompleteWindow
            english.Add("ShowInstallCompleteWindowCB", "Show install complete window");
            german.Add("ShowInstallCompleteWindowCB", "Zeigt \"Installation fertig\" Fenster");
            polish.Add("ShowInstallCompleteWindowCB", "Pokaż okno akcji po instalacji");
            french.Add("ShowInstallCompleteWindowCB", "Afficher la fenêtre \"installation terminé\"");

            //Section: ModSelectionList

            //Componet: continueButton
            //
            english.Add("continueButton", "Install");
            german.Add("continueButton", "Installieren");
            polish.Add("continueButton", "Zainstaluj");
            french.Add("continueButton", "Installer");

            //Componet: cancelButton
            //
            english.Add("cancelButton", "Cancel");
            german.Add("cancelButton", "Abbrechen");
            polish.Add("cancelButton", "Anuluj");
            french.Add("cancelButton", "Anuler");

            //Componet: helpLabel
            //
            english.Add("helpLabel", "Right-click a mod name to preview it");
            german.Add("helpLabel", "Klick mit rechten Maustaste auf einen Mod-Namen, um eine Vorschau zu sehen");
            polish.Add("helpLabel", "PPM by wyświetlić opis");
            french.Add("helpLabel", "Cliquez droit sur un nom de mod pour un apercu");

            //Componet: loadConfigButton
            //
            english.Add("loadConfigButton", "Load selection");
            german.Add("loadConfigButton", "Auswahl laden");
            polish.Add("loadConfigButton", "Wczytaj konfigurację z pliku");
            french.Add("loadConfigButton", "Charger une configuration");

            //Componet: saveConfigButton
            //
            english.Add("saveConfigButton", "Save selection");
            german.Add("saveConfigButton", "Auswahl speichern");
            polish.Add("saveConfigButton", "Zapisz konfigurację w pliku");
            french.Add("saveConfigButton", "Sauvegarder une configuration");
      

            //Componet: label2
            //
            english.Add("label2", "\"*\" tab indicates single selection tab");
            german.Add("label2", "Ein Tab mit einem\"*\" kann nur eins der Mods ausgewählt werden.");
            polish.Add("label2", "\"*\" wskazuje pojedynczą kartę wyboru");
            french.Add("label2", "Onglet \"*\" indique un onglet a sélection unique");

            //Componet: clearSelectionsButton
            //
            english.Add("clearSelectionsButton", "Clear selections");
            german.Add("clearSelectionsButton", "Auswahl löschen");
            polish.Add("clearSelectionsButton", "Wyczyść wybór");
            french.Add("clearSelectionsButton", "Réinitialiser la sélection");

            //Componet: readingDatabase
            //
            english.Add("readingDatabase", "Reading Database");
            german.Add("readingDatabase", "Lese Datenbank");
            polish.Add("readingDatabase", "Wczytywanie baz danych");
            french.Add("readingDatabase", "Chargement de la base de données");

            //Componet: buildingUI
            //
            english.Add("buildingUI", "Building UI");
            german.Add("buildingUI", "Erstelle UI");
            polish.Add("buildingUI", "Budowanie interfejsu");
            french.Add("buildingUI", "Construction de l'interface");

            //Section: Preview
            //Componet: descriptionBox
            //
            english.Add("noDescription", "No Description Provided");
            german.Add("noDescription", "keine Beschreibung verfügbar");
            polish.Add("noDescription", "nie podano opisu");
            french.Add("noDescription", "non Description fournie");

            //Componet: updateBox
            //
            english.Add("noUpdateInfo", "No Update Info Provided");
            german.Add("noUpdateInfo", "keine Aktualisierungsinformationen verfügbar");
            polish.Add("noUpdateInfo", "brak informacji o aktualizacji");
            french.Add("noUpdateInfo", "Aucune information mise à jour fournie");
            

            //Componet: nextPicButton
            //
            english.Add("nextPicButton", "next");
            german.Add("nextPicButton", "weiter");
            polish.Add("nextPicButton", "Dalej");
            french.Add("nextPicButton", "Suivant");

            //Componet: previousPicButton
            //
            english.Add("previousPicButton", "previous");
            german.Add("previousPicButton", "zurück");
            polish.Add("previousPicButton", "Wstecz");
            french.Add("previousPicButton", "Précedent");

            //Componet: devLinkLabel
            //
            english.Add("devLinkLabel", "Developer Website");
            german.Add("devLinkLabel", "Entwickler Webseite");
            polish.Add("devLinkLabel", "Strona Dewelopera");
            french.Add("devLinkLabel", "Site web du développeur");

            //Section: VersionInfo
            //Componet: updateAcceptButton
            //
            english.Add("updateAcceptButton", "yes");
            german.Add("updateAcceptButton", "ja");
            polish.Add("updateAcceptButton", "Tak");
            french.Add("updateAcceptButton", "Oui");

            //Componet: updateDeclineButton
            //
            english.Add("updateDeclineButton", "no");
            german.Add("updateDeclineButton", "nein");
            polish.Add("updateDeclineButton", "Nie");
            french.Add("updateDeclineButton", "Non");

            //Componet: newVersionAvailableLabel
            //
            english.Add("newVersionAvailableLabel", "New Version Available");
            german.Add("newVersionAvailableLabel", "Neue Version verfügbar");
            polish.Add("newVersionAvailableLabel", "Dostępna Nowa Wersja");
            french.Add("newVersionAvailableLabel", "Nouvelle version disponible");

            //Componet: updateQuestionLabel
            //
            english.Add("updateQuestionLabel", "Update?");
            german.Add("updateQuestionLabel", "Aktualisieren?");
            polish.Add("updateQuestionLabel", "Zaktualizować?");
            french.Add("updateQuestionLabel", "Mettre à jour?");

            //Componet: problemsUpdatingLabel
            //
            english.Add("problemsUpdatingLabel", "If you are having problems updating, please");
            german.Add("problemsUpdatingLabel", "Wenn Sie Probleme mit der Aktualisierung haben, bitte");
            polish.Add("problemsUpdatingLabel", "Jeśli masz problemy z aktualizają proszę");
            french.Add("problemsUpdatingLabel", "Si vous avez des problèmes avec la mise à jour, s'il vous plaît");
      

            //Componet: 
            //
            english.Add("clickHereUpdateLabel", "click here");
            german.Add("clickHereUpdateLabel", "klick hier");
            polish.Add("clickHereUpdateLabel", "kliknij tutaj");
            french.Add("clickHereUpdateLabel", "Cliquez ici");

            //Section: PleaseWait
            //Componet: label1
            //
            english.Add("label1", "Loading...please wait...");
            german.Add("label1", "Lädt...bitte warten...");
            polish.Add("label1", "Ładowanie... proszę czekać...");
            french.Add("label1", "Chargement... Patientez, s'il vous plaît...");
      
            //Section: Messages of MainWindow

            //Componet: 
            //
            english.Add("Downloading", "Downloading");
            german.Add("Downloading", "Wird heruntergeladen");
            polish.Add("Downloading", "Pobieranie");
            french.Add("Downloading", "Téléchargement");

            //Componet: 
            //
            english.Add("patching", "Patching");
            german.Add("patching", "Patching");
            polish.Add("patching", "Patchowanie");
            french.Add("patching", "Patching");

            //Componet: 
            //
            english.Add("done", "Done");
            german.Add("done", "Fertig");
            polish.Add("done", "Zrobione");
            french.Add("done", "Terminé");

            //Componet: 
            //
            english.Add("cleanUp", "Clean up resources");
            german.Add("cleanUp", "Bereinige Ressourcen");
            polish.Add("cleanUp", "Oczyszczanie zasobów");
            french.Add("cleanUp", "Nettoyer les ressources");
            
            //Componet: 
            //
            english.Add("idle", "Idle");
            german.Add("idle", "Leerlauf");
            polish.Add("idle", "Bezczynny");
            french.Add("idle", "En attente");

            //Componet: 
            //
            english.Add("status", "Status:");
            german.Add("status", "Status:");
            polish.Add("status", "Stan:");
            french.Add("status", "Statut");

            //Componet: 
            //
            english.Add("canceled", "Canceled");
            german.Add("canceled", "Abgebrochen");
            polish.Add("canceled", "Anulowano");
            french.Add("canceled", "Anulé");

            //Componet: 
            //
            english.Add("appSingleInstance", "Checking for single instance");
            german.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            polish.Add("appSingleInstance", "Sprawdzanie ");
            french.Add("appSingleInstance", "Vérification d'instance unique");

            //Componet: 
            //
            english.Add("checkForUpdates", "Checking for updates");
            german.Add("checkForUpdates", "Auf Updates prüfen");
            polish.Add("checkForUpdates", "Sprawdzanie aktualizacji");
            french.Add("checkForUpdates", "Vérification de mise à jours");

            //Componet: 
            //
            english.Add("verDirStructure", "Verifying directory structure");
            german.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            polish.Add("verDirStructure", "Sprawdzanie struktury dostępu");
            french.Add("verDirStructure", "Vérification de la structure du directoire");

            //Componet: 
            //
            english.Add("loadingSettings", "Loading Settings");
            german.Add("loadingSettings", "Einstellungen laden");
            polish.Add("loadingSettings", "Ładowanie ustawnień");
            french.Add("loadingSettings", "Chargement des paramètres");

            //Componet: 
            //
            english.Add("loadingTranslations", "Loading Translations");
            german.Add("loadingTranslations", "Laden der Übersetzungen");
            polish.Add("loadingTranslations", "ładowanie tłumaczenia");
            french.Add("loadingTranslations", "Chargement des traductions");

            //Componet: 
            //
            english.Add("loading", "Loading");
            german.Add("loading", "Laden");
            polish.Add("loading", "Ładowanie");
            french.Add("loading", "Chargement de");

            //Componet: 
            //
            english.Add("uninstalling", "Uninstalling");
            german.Add("uninstalling", "Deinstallieren");
            polish.Add("uninstalling", "Deinstalacja w toku");
            french.Add("uninstalling", "Désinstallation");

            //Componet: 
            //
            english.Add("installingFonts", "Installing Fonts");
            german.Add("installingFonts", "Installieren von Schriftarten");
            polish.Add("installingFonts", "Instalowanie czcionek");
            french.Add("installingFonts", "Installation des polices");

            //Componet: 
            //
            english.Add("loadingExtractionText", "Loading Extraction Text");
            german.Add("loadingExtractionText", "Extraktionstext laden");
            polish.Add("loadingExtractionText", "Ładowanie tekstu");
            french.Add("loadingExtractionText", "Chargement du texte d'extraction");

            //Componet: 
            //
            english.Add("extractingRelhaxMods", "Extracting RelHax Mods");
            german.Add("extractingRelhaxMods", "Extrahieren von RelHax Mods");
            polish.Add("extractingRelhaxMods", "Wypakowywanie modyfikacji RelHax");
            french.Add("extractingRelhaxMods", "Extraction des mods Relhax");

            //Componet: 
            //
            english.Add("extractingUserMods", "Extracting User Mods");
            german.Add("extractingUserMods", "Extrahieren von benutzerdefinierten Mods");
            polish.Add("extractingUserMods", "Wypakowywanie modyfikacji użytkownika");
            french.Add("extractingUserMods", "Extraction des mods d'utilisateur");


            //Componet: 
            //
            english.Add("startingSmartUninstall", "Starting smart uninstall");
            german.Add("startingSmartUninstall", "Starten der intelligenten Deinstallation");
            polish.Add("startingSmartUninstall", "Rozpoczynanie inteligentnej deinstalacji");
            french.Add("startingSmartUninstall", "Lancement de la désinstallation intéligente");

            //Componet: 
            //
            english.Add("copyingFile", "Copying file");
            german.Add("copyingFile", "Kopieren von Dateien");
            polish.Add("copyingFile", "Kopiowanie plików");
            french.Add("copyingFile", "Copie des fichiers");

            //Componet: 
            //
            english.Add("deletingFile", "Deleting file");
            german.Add("deletingFile", "Lösche Datei");
            polish.Add("deletingFile", "Usuwanie plików");
            french.Add("deletingFile", "Supression du fichier");

            //Componet: 
            //
            english.Add("of", "of");
            german.Add("of", "von");
            polish.Add("of", "z");
            french.Add("of", "de");

            //Componet: 
            //
            english.Add("forceManuelDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            german.Add("forceManuelDescription", "Diese Option ist für die manuelle selektion des World of Tanks Spiel-" +
                    "speicherortes. Nutze dies wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            polish.Add("forceManuelDescription", "Ta opcja wymusza ręczne znalezienie lokacji gry World of Tanks." +
                    "Zaznacz, jeśli masz problem z automatycznym znalezieniem ścieżki dostępu do gry.");
            french.Add("forceManuelDescription", "Cette option consiste à forcer une détection manuel" +
                    "de World of Tanks. Sélectionnez cette option si vous rencontrez des problèmes pour localiser automatiquement le jeu.");    

            //Componet: 
            //
            english.Add("cleanInstallDescription", "This recommended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            german.Add("cleanInstallDescription", "Diese empfohlene Option leert den Ordner res_mods vor der Installation" +
                    "Deiner neuen Mod-Auswahl. Bis du genau weißt, was du tust, empfohlen wir, dass du das weiter behältst, um Probleme zu vermeiden.");
            polish.Add("cleanInstallDescription", "To rekomendowane ustawienie usunie zawartość folderu res_mods przed instalacją" +
                     "nowej konfiguracji modów. Jeśli nie wiesz co robisz zalecamy włączyć tą opcję, aby uniknąć problemów.");
            french.Add("cleanInstallDescription", "Cette option recommandé vas nettoyer le dossier res_mods avant d'installer" +
                    "votre nouvelle sélection de mods. À moins que vous ne sachiez ce que vous faites, il est recommandé de laisser ceci activé pour éviter des problèmes.");     

            //Componet: 
            //
            english.Add("backupModsDescription", "Select this to make a backup of your current res_mods folder." +
                    "They are stored in the 'RelHaxModBackup' folder, saved in a folder inside by a time stamp.");
            german.Add("backupModsDescription", "Wähle diese Option, um eine Sicherungskopie Deines aktuellen res_mods-Ordners zu erstellen." +
                    "Sie werden im Ordner 'RelHaxModBackup' gespeichert und in einem Ordner nach innen durch einen Zeitstempel gespeichert.");
            polish.Add("backupModsDescription", "Zaznacz, aby zrobić kopię zapasową folderu res_mods." +
                     "Pliki będą przechowane w folderze RelHaxModBackup, zapisane w folderze oznaczonym datą.");
            french.Add("backupModsDescription", "Choisissez ceci pour faire une sauvegarde du dossier res_mods actuel");     

            //Componet: 
            //
            english.Add("comicSansDescription", "Enable Comic Sans font");
            german.Add("comicSansDescription", "Schriftart Comic Sans aktivieren");
            polish.Add("comicSansDescription", "Włącz czcionkę Comic Sans");
            french.Add("comicSansDescription", "Activé la police Comic Sans");

            //Componet: 
            //
            english.Add("enlargeFontDescription", "Enlarge font");
            german.Add("enlargeFontDescription", "Schriftart vergrössern");
            polish.Add("enlargeFontDescription", "Powiększ czcionkę");
            french.Add("enlargeFontDescription", "Agrandir la police");

            //Componet: 
            //
            english.Add("selectGifDesc", "Select a loading gif for the mod preview window.");
            german.Add("selectGifDesc", "Wähle ein Lade-Gif fuer das Vorschaufenster des Mods.");
            polish.Add("selectGifDesc", "Załaduj gif w oknie podglądu.");
            french.Add("selectGifDesc", "Choisissez un GIF de chargement pour l'apercu des mods");

            //Componet: 
            //
            english.Add("saveLastConfigInstall", "If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            german.Add("saveLastConfigInstall", "Wenn dies ausgewählt ist, lädt das Installationsprogramm die zuletzt installierte Config im Auswahlfenster, die Du verwendet hast.");
            polish.Add("saveLastConfigInstall", "Przy zaznaczeniu, instalator załaduje ostatnią użytą konfigurację w oknie wyboru modyfikacji.");
            french.Add("saveLastConfigInstall", "Si cette option est sélectionnée, L'installateur affichera, lors de la fenêtre de sélection, la denière configuration que vous avez utilisé");

            //Componet:
            //
            english.Add("saveUserDataDesc", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            german.Add("saveUserDataDesc", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten (wie Sitzungsstatistiken aus früheren Gefechten)");
            polish.Add("saveUserDataDesc", "Przy zaznaczeniu, instalator zachowa pliki danych użytkownika (takie jak statystyki sesji z poprzednich bitew)");
            french.Add("saveUserDataDesc", "Si cette option est sélectionnée, l'installateur vas sauvegarder les données créé par l'utilisateur (Comme les stats de la session des batailles précédentes");

            //Componet: 
            //
            english.Add("cleanUninstallDescription", "Selected - All mods will be erased\nNot Selected - Only Modpack installed mods will be erased");
            german.Add("cleanUninstallDescription", "Ausgewählt - Alle Mods werden gelöscht\nNicht ausgewählt - Nur Mods, die vom Modpack installiert wurden, werden gelöscht");
            polish.Add("cleanUninstallDescription", "Zaznaczone - Wszystkie mody zostaną usunięte\nNie zaznaczone - Tylko zainstalowane mody z paczki zostaną usunięte");
            french.Add("cleanUninstallDescription", "Sélectionné - Tout les mods vont être supprimé\nNon sélectionné - Seulement les mods installé par le modpack vont être supprimé");

            //Componet: 
            //
            english.Add("darkUIDesc", "Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            german.Add("darkUIDesc", "Auf dunklen UI Modus umschalten. Nützlich für die Arbeit mit dem Modpack in der Nacht.");
            polish.Add("darkUIDesc", "Zmień interfejs na ciemny. Przydatneprzy pracy z paczką w nocy.");
            french.Add("darkUIDesc", "Activer l'interface sombre. Utile pour utilisé le modpack pendant la nuit");

            //Componet: 
            //
            english.Add("failedToDownload_1", "Failed to download:");
            german.Add("failedToDownload_1", "Fehler beim Herunterladen:");
            polish.Add("failedToDownload_1", "Ściąganie zakończone niepowodzeniem, plik:");
            french.Add("failedToDownload_1", "Échec de téléchargement:");

            //Componet: 
            //
            english.Add("failedToDownload_2", "If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            german.Add("failedToDownload_2", "Wenn du weisst, welcher Mod das ist, deaktiviere ihn und alles sollte funktionieren. Es wird bald behoben. Starte das Programm neu nach dem beenden.");
            polish.Add("failedToDownload_2", "Jeśli wiesz który to mod, odznacz go i wszystko powinno byćw porządku. Wkrótce naprawimy błąd. Zrestartuj, jeśli problem pojawia się ponownie.");
            french.Add("failedToDownload_2", "Si vous savez quel mod est la cause, déséléectionnez celui-ci. Un corrigé vas être disponible bientôt. Redémarrez ceci a la fermeture");

            //Component: initial download managerInfo.dat
            //
            english.Add("failedManager_version", "Failed to get 'manager_version.xml'\n\nApplication will be terminated.");
            german.Add("failedManager_version", "Fehler beim lesen der 'manager_version.xml' Datei.\n\nProgramm wird abgebrochen.");
            polish.Add("failedManager_version", "Nie udało się uzyskać 'manager_version.xml'\n\nApplication zostanie zakończona.");
            french.Add("failedManager_version", "Impossible d'obtenir 'manager_version.xml'\n\nL'application sera terminée.");

            //Componet: 
            //
            english.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            german.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
            polish.Add("fontsPromptInstallHeader", "Uprawnienia administratora, by zainstalować czcionki?");
            french.Add("fontsPromptInstallHeader", "Admin pour installer les polices?");

            //Componet: 
            //
            english.Add("fontsPromptInstallText", "Do you have admin rights?");
            german.Add("fontsPromptInstallText", "Hast Du Admin-Rechte?");
            polish.Add("fontsPromptInstallText", "Czy masz uprawnienia administratora?");
            french.Add("fontsPromptInstallText", "Avez-vous les droits d'administrateur?");

            //Componet: 
            //
            english.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            german.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
            polish.Add("fontsPromptError_1", "Niepowodzenie przy instalacji czcionek. Niektóre modyfikacje mogą nie działać prawidłowo. Czcionki znajdują się w ");
            french.Add("fontsPromptError_1", "Impossible d'installer les polices. Certain mods peut mal fonctionner. Les polices sont situé dans ");

            //Componet: 
            //
            english.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            german.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe der Relhax Manager erneut als Administrator aus.");
            polish.Add("fontsPromptError_2", "\\_fonts. Albo zainstalujesz je własnoręcznie, albo uruchom jako administrator.");
            french.Add("fontsPromptError_2", "\\_fonts. Installez les polices manuellement ou redémarrez avec les droits Administrateur");

            //Componet: 
            //
            english.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            german.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
            polish.Add("cantDownloadNewVersion", "Niepowodzenie przy pobieraniu nowej wersji.");
            french.Add("cantDownloadNewVersion", "Échec du téléchargement des mise à jours. Fermeture.");

            //Componet: 
            //
            english.Add("failedCreateUpdateBat", "Unable to create update process.\n\nPlease manually delete the file:\n{0}\n\nrename file:\n{1}\nto:\n{2}\n\nDirectly jump to the folder?");
            german.Add("failedCreateUpdateBat", "Updateprozess kann leider nicht erstellt werden.\n\nLösche bitte diese Datei von Hand:\n{0}\n\nbenennte diese Datei:\n{1}\nin diese um:\n{2}\n\nDirekt zum Ordner springen?");
            polish.Add("failedCreateUpdateBat", "Nie można zaktualizować.\n\nProszę ręcznie usunąć plik:\n{0}\n\nrzmienić nazwę pliku:\n{1}\nna:\n{2}\n\nCzy chcesz otworzyć lokalizację pliku?");
            french.Add("failedCreateUpdateBat", "fichier:\n{0}\n\nrenamefile:\n{1}\nto:\n{2}\n\nAfficher le dossier?");

            //Componet: 
            //
            english.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            german.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
            polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
            french.Add("cantStartNewApp", "Échec du lancement de l'application, mais elle est situé dans \n");

            //Componet: 
            //
            english.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            german.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutzen Sie die 'erzwinge manuelle' Option");
            polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Proszę wybrać opcję ręcznego znajdowania ścieżki gry.");
            french.Add("autoDetectFailed", "Échec de la détection automatique. Utilisez l'option 'Forcer détection manuel'");

            //Componet: MainWindow_Load
            //
            english.Add("anotherInstanceRunning", "CRITICAL: Another Instance of the modpack is already running");
            german.Add("anotherInstanceRunning", "KRITISCH: Eine weitere Instanz des modpack läuft bereits");
            polish.Add("anotherInstanceRunning", "BŁĄD KRYTYCZNY: Inna instancja modpack jest uruchomiona");
            french.Add("anotherInstanceRunning", "CRITIQUE: Une autre instance de modpack est en cours d`éxecution");
      

            //Componet: 
            //
            english.Add("skipUpdateWarning", "WARNING: You are skipping updating. Database Compatability is not guarenteed");
            german.Add("skipUpdateWarning", "WARNUNG: Sie überspringen die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
            polish.Add("skipUpdateWarning", "OSTRZEŻENIE: Pomijasz aktualizację! Może wystąpić niezgodność wersji.");
            french.Add("skipUpdateWarning", "ATTENTION: Vous ignorez la mise à jour. Compatibilité de la base de données non garanti ");

            //Componet: 
            //
            english.Add("patchDayMessage", "The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            german.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten. Wenn Sie ein Datenbankmanager sind, fügen Sie bitte das Befehlsargument hinzu");
            polish.Add("patchDayMessage", "Paczka nie działa ze względu na testy i aktualizację modyfikacji. Przepraszamy za utrudnienia. Jeśli zarządzasz bazą danych, proszę dodać odpowiednią komendę");
            french.Add("patchDayMessage", "Le pack mod est actuellement indisponible aujourd'hui pour tester et mettre à jour les mods. Désolé pour le dérangement. Si vous êtes un gestionnaire de base de données, ajoutez l'argument de commande.");

            //Componet: 
            //
            english.Add("configNotExist", "ERROR: {0} does NOT exist, loading in regular mode");
            german.Add("configNotExist", "FEHLER: {0} existiert nicht, lädt im regulären Modus");
            polish.Add("configNotExist", "BŁĄD: {0} nie istnieje, ładowanie podstawowego trybu");
            french.Add("configNotExist", "ERREUR: {0} n'existe pas, chargement en mode normal");

            //Componet: 
            //
            english.Add("autoAndClean", "ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            german.Add("autoAndClean", "FEHLER: Die saubere Installation ist abgewählt/deaktiviert. Du musst diese Option auswählen und die Anwendung für die automatische Installation neu starten, damit sie funktioniert. Lädt im regulären Modus.");
            polish.Add("autoAndClean", "BŁĄD: wyłączono czystą instalację. Musisz ją włączyć i ponownie uruchomić aplikację, by automatyczna instalacja zadziałała. Ładowanie w trybie podstawowym.");
            french.Add("autoAndClean", "ERREUR: Installation propre est désactivé. Vous devez sélectionner ceci et redémarrer l'application pour l'installation automatique. Chargement en mode normal");

            //Componet: 
            //
            english.Add("autoAndFirst", "ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            german.Add("autoAndFirst", "FEHLER: Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulären Modus");
            polish.Add("autoAndFirst", "BŁĄD: Pierwsze ładowanie nie może być automatyczną instalacją, ładowanie w trybie podstawowym");
            french.Add("autoAndFirst", "ERREUR: Le premier lancement ne peut pas être un mode d'installation automatique, chargement en mode normal");

            //Componet: 
            //
            english.Add("confirmUninstallHeader", "Confirmation");
            german.Add("confirmUninstallHeader", "Bestätigung");
            polish.Add("confirmUninstallHeader", "Potwierdź");
            french.Add("confirmUninstallHeader", "Confirmation");

            //Componet: 
            //
            english.Add("confirmUninstallMessage", "Confirm you wish to uninstall?");
            german.Add("confirmUninstallMessage", "Möchtest du wirklich die Mods deinstallieren?");
            polish.Add("confirmUninstallMessage", "Potwierdzić deinstalację?");
            french.Add("confirmUninstallMessage", "Confirmez que vous voulez désinstaller?");

            //Componet: 
            //
            english.Add("uninstallingText", "Uninstalling...");
            german.Add("uninstallingText", "Deinstalliere...");
            polish.Add("uninstallingText", "Deinstalacja w toku...");
            french.Add("uninstallingText", "Désinstallation...");

            //Component:
            //progress message
            english.Add("uninstallingFile", "Uninstalling file");
            german.Add("uninstallingFile", "Deinstalliere Datei");
            polish.Add("uninstallingFile", "Odinstalowanie pliku");
            french.Add("uninstallingFile", "Désinstallation du fichier");

            //Component: uninstallfinished messagebox
            //
            english.Add("uninstallFinished", "Uninstallation of Mods finished.");
            german.Add("uninstallFinished", "Deinstallation der Mods beendet.");
            polish.Add("uninstallFinished", "Deinstalacja (modyfikacji) zakończona");
            french.Add("uninstallFinished", "Désinstallation des Mods terminé");

            //Componet: 
            //
            english.Add("specialMessage1", "If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system. It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            german.Add("specialMessage1", "Wenn Du dies siehst, bedeutet dies, dass Du eine bestimmte Computer-Konfiguration hast, die von einem Fehler betroffen ist, den ich nicht auf meinem Entwicklersystem replizieren kann. Es ist harmlos, aber wenn du dein relHaxLog an mich schicken könntest, kann ich es beheben und du wirst diese Nachricht zukuenftig nicht mehr sehen");
            polish.Add("specialMessage1", "Jeśli to widzisz, to znaczy, że masz specificzną konfigurację komputera afektowany przez bug, który nie mogę kopiować na moim systemie. Jest nieszkodliwy, ale jeśli możesz mi przesłać relHaxLog to postaram się naprawić błąd, abyś nie widział tej wiadomości w przyszłości");
            french.Add("specialMessage1", "Si vous voyez ceci, cela signifie que vous avez une configuration d'ordinateur spécifique qui est affectée par un bug que je ne peux pas répliquer sur mon système de développeur. Ce n'est pas grave. Si vous pouvez envoyer votre relHaxLog, je peux le réparer et vous pouvez arrêter de voir ce message.");

            //Componet: 
            //
            english.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file and folder security permissions are incorrect");
            german.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder Ihre Datei- und Ordnersicherheitsberechtigungen sind falsch");
            polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest uruchomione, albo twój plik i folder mają nieprawidłowe zabezpieczenia dostępu");
            french.Add("extractionErrorMessage", "Erreur lors de la supression du dossier res_mods ou un/plusieur mods. Sois que World of Tanks est en cours d`éxecution ou les permissions de sécuriter sont incorrect.");

            //Componet: 
            //
            english.Add("extractionErrorHeader", "Error");
            german.Add("extractionErrorHeader", "Fehler");
            polish.Add("extractionErrorHeader", "Błąd");
            french.Add("extractionErrorHeader", "Erreur");

            //Componet: 
            //
            english.Add("deleteErrorHeader", "close out of folders");
            german.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            polish.Add("deleteErrorHeader", "zamknij foldery");
            french.Add("deleteErrorHeader", "Fermez les dossiers");

            //Componet: 
            //
            english.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            german.Add("deleteErrorMessage", "Bitte schließen Sie alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicken Sie auf OK, um fortzufahren.");
            polish.Add("deleteErrorMessage", "Proszę zamknij folder mods lub res_mods (lub podfoldery), a następnie kliknij kontynuację.");
            french.Add("deleteErrorMessage", "Veuillez fermer les fenêtre res_mods ou mods (Ou tout sous-dossiers) et cliquez Ok pour continuer");

            //Componet: 
            //
            english.Add("noUninstallLogMessage", "The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            german.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            polish.Add("noUninstallLogMessage", "Plik logów zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chciałbyś usunąć modyfikacje?");
            french.Add("noUninstallLogMessage", "Le ficher log contenant la liste des fichiers installé (installedRelhaxFiles.log) n'existe pas. Voulez vous supprimez tout les mods?");

            //Componet: 
            //
            english.Add("noUninstallLogHeader", "Remove all mods");
            german.Add("noUninstallLogHeader", "Entferne alle Mods");
            polish.Add("noUninstallLogHeader", "Usuń wszystkie modyfikacje");
            french.Add("noUninstallLogHeader", "Supprimer tout les mods");

            //Component: appDataFolderError
            //If the appDataFolder is not parsed properly
            english.Add("appDataFolderError", "The app data folder for WoT was not found. Clearing WoT cache will be skipped. Please report this to the developer team.");
            german.Add("appDataFolderError", "Der App-Datenordner für WoT wurde nicht gefunden. Das Löschen von WoT-Cache wird übersprungen. Bitte melden Sie dies dem Entwicklerteam.");
            polish.Add("appDataFolderError", "Nie znaleziono foderu app data dla WoT. Czyszczenie folderu cache zostanie pominięte. Prosimy zgłosić problem naszym deweloperom.");
            french.Add("appDataFolderError", "Le dossier App Data pour WoT n'as pas pus être trouvé. Nettoyage du cache vas être ignoré. Veuillez avertir l`équipe de développement");
            //Component: clearCacheCB
            //
            english.Add("clearCacheCB", "Clear WoT cache data");
            german.Add("clearCacheCB", "Cache-Daten für WoT löschen");
            polish.Add("clearCacheCB", "Usuń dane WoT cache");
            french.Add("clearCacheCB", "Nettoyer le dossier de Cache WoT");

            //Component: clearCachCBExplanation
            //
            english.Add("clearCachCBExplanation", "Clear the WoT cache app data directory. Performs the same task as the similar option that was in OMC.");
            german.Add("clearCachCBExplanation", "Löschen Sie das WoT-Cache-App-Datenverzeichnis. Führt die gleiche Aufgabe wie die ähnliche Option aus, die in OMC war.");
            polish.Add("clearCachCBExplanation", "Usuń dane aplikacji z lokacji WoT cache. Działa na podobnej zasadzie, jak kiedyś opcja z paczki OMC.");
            french.Add("clearCachCBExplanation", "Nettoyer le dossier cache WoT. Effectue la même tâche que l'option similaire qui était dans OMC.");

            //Component: clearLogFilesCBExplanation
            //
            english.Add("clearLogFilesCBExplanation", "Clear the WoT log files, (python.log), as well as xvm log files (xvm.log) and pmod log files (pmod.log)");
            german.Add("clearLogFilesCBExplanation", "Löschen der WoT Protokolldatei, sowie XVM und PMOD Protokolldatei");
            polish.Add("clearLogFilesCBExplanation", "Wyczyść logi WoTa (python.log), XVM'a (xvm.log) i pmod'ów (pmod.log).");
            french.Add("clearLogFilesCBExplanation", "Effacez les fichiers logs WoT (python.log), ainsi que les fichiers logs xvm (xvm.log) et les fichiers logs pmod (pmod.log)");

            //Component: disableColorsCBExplanation
            //
            english.Add("disableColorsCBExplanation", "Disable the changing of colors when toggling the selection of a mod or config");
            german.Add("disableColorsCBExplanation", "Deaktiviere das Ändern der Farbe, wenn Modifikationen oder Konfigurationen gewählt werden.");
            polish.Add("disableColorsCBExplanation", "Wyłącz zmianę kolorów podczas wyboru modyfikacji lub konfiguracji.");
            french.Add("disableColorsCBExplanation", "Désactiver le changement de couleurs lors de la sélection d'un mod ou config");

            //Component: notifyIfSameDatabaseCBExplanation
            //
            english.Add("notifyIfSameDatabaseCBExplanation", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods.");
            german.Add("notifyIfSameDatabaseCBExplanation", "Dich benachrichtigen: Die letzte verwendete Datenbank ist diesselbe, d.h. es gibts keine Aktualisierungen & Veränderungen.");
            polish.Add("notifyIfSameDatabaseCBExplanation", "Powiadom, jeśli ostatnia zainstalowana wersja bazy danych jest taka sama. Jeśli tak, to nie ma potrzeby aktualizacji modyfikacji.");
            french.Add("notifyIfSameDatabaseCBExplanation", "Avertir si votre dernière version de base de données installée est identique. Si c'est le cas, cela signifie qu'il n'y a pas de mise à jour de mods.");

            //Component: ShowInstallCompleteWindowCBExplanation
            //
            english.Add("ShowInstallCompleteWindowCBExplanation", "Show a window upon installation completion with popular operations to perform after modpack installation, such as launching the game, going to the xvm website, etc.");
            german.Add("ShowInstallCompleteWindowCBExplanation", "Zeigte am Ende der Installation ein Auswahlfenster mit nützlichen Befehlen an, wie: starte das Spiel, gehe zur XVM Webseite, usw ...");
            polish.Add("ShowInstallCompleteWindowCBExplanation", "Po zakończeniu instalacji otwórz okno dalszych akcji.");
            french.Add("ShowInstallCompleteWindowCBExplanation", "Afficher une fenêtre lors de l'achèvement de l'installation avec des opérations populaires à effectuer après l'installation de Modpack, telles que le lancement du jeu, le site Web de XVM, etc.");

            //Component: ShowInstallCompleteWindowCBExplanation
            //
            english.Add("CreateShortcutsCBExplanation", "When selected, it will create shortcut icons on your desktop for mods that are exe files (like WWIIHA configuration)");
            german.Add("CreateShortcutsCBExplanation", "Wenn diese Option aktiviert ist, werden bei der Installation die Verknüpfungen für \"World of Tanks\", \"World of Tanks launcher\" und, wenn bei der Installation aktiviert, auch andere Verknüpfungen zu Konfigurationsprogrammen erstellt (z.B. WWIIHA Konfiguration)");
            polish.Add("CreateShortcutsCBExplanation", "Kiedy zaznaczone, utworzone zostaną skróty na pulpicie do modyfikacji z plikami exe (np. konfiguracja WWIIHA)");
            french.Add("CreateShortcutsCBExplanation", "Une fois sélectionné, L'installation créera des icônes de raccourci sur votre bureau pour les mods qui ont des fichiers .exe (comme la configuration WWIIHA)");

            //Section: Messages from ModSelectionList

            //Componet: testModeDatabaseNotFound
            //
            english.Add("testModeDatabaseNotFound", "CRITICAL: TestMode Database not found at:\n{0}");
            german.Add("testModeDatabaseNotFound", "KRITISCH: Die Datanbank für den Testmodus wurde nicht gefunden:\n{0}");
            polish.Add("testModeDatabaseNotFound", "BŁĄD KRYTYCZNY: Baza danych Trybu Testowego nie znaleziona w lokalizacji:\n{0}");
            french.Add("testModeDatabaseNotFound", "CRITIQUE: Impossible de trouver la base de données du mode de test situé a: \n{0}");

            //Componet: 
            //
            english.Add("duplicateMods", "CRITICAL: Duplicate mod name detected");
            german.Add("duplicateMods", "KRITISCH: Duplizierter Modname wurde erkannt");
            polish.Add("duplicateMods", "BŁĄD KRYTYCZNY: Wykryto zduplikowaną nazwę modyfikacji");
            french.Add("duplicateMods", "CRITIQUE: Détection de mods en double");

            //Componet: 
            //
            english.Add("databaseReadFailed", "CRITICAL: Failed to read database\n\nsee Logfile for detailed info");
            german.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden\n\nin der Protokolldatei stehen weitere Informationen zu diesem Fehler");
            polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się wczytać bazy danych");
            french.Add("databaseReadFailed", "CRITIQUE: Impossible de lire la base de données");

            //Componet: 
            //
            english.Add("configSaveSuccess", "Config Saved Successfully");
            german.Add("configSaveSuccess", "Konfiguration wurde erfolgreich gespeichert");
            polish.Add("configSaveSuccess", "Udało się zapisać konfigurację");
            french.Add("configSaveSuccess", "Succès sauvegarde de la configuration");

            //Componet: 
            //
            english.Add("selectConfigFile", "Select a user preference file to load");
            german.Add("selectConfigFile", "Wählen Sie die benutzerdefinierte Datei aus, die geladen werden soll");
            polish.Add("selectConfigFile", "Wybierz plik preferencji do wczytania");
            french.Add("selectConfigFile", "Sélectionnez un fichier de préférences utilisateur à charger");

            //Componet: 
            //
            english.Add("configLoadFailed", "The preference file could not be loaded, loading in standard mode");
            german.Add("configLoadFailed", "Die Konfigurationsdatei konnte nicht geladen werden, lade im Standard Modus");
            polish.Add("configLoadFailed", "Nie można wczytać pliku knfiguracji, ładowanie trybu podstawowego");
            french.Add("configLoadFailed", "Impossible de charge le fichier de préférences. Chargement en mode normal");

            //Componet: 
            //
            english.Add("modNotFound", "The mod, \"{0}\" was not found in the modpack. It could have been renamed or removed.");
            german.Add("modNotFound", "Der Mod, \"{0}\" wurde im Modpack nicht gefunden. Er könnte umbenannt oder entfernt worden sein.");
            polish.Add("modNotFound", "Modyfikacja \"{0}\" nie została znaleziona w paczce. Sprawdź, czy nie została usunięta lub zmieniona nazwa.");
            french.Add("modNotFound", "Impossible de trouver le mod, \"{0}\" dans le mod pack. Il est possible qu`il sois supprimé ou changé de nom");

            //Componet: 
            //
            english.Add("configNotFound", "The config \"{0}\" was not found for mod \"{1}\". It could have been renamed or removed.");
            german.Add("configNotFound", "Die Config \"{0}\" wurde nicht für den Mod gefunden \"{1}\". Er könnte umbenannt oder entfernt worden sein.");
            polish.Add("configNotFound", "Konfiguracja \"{0}\" nie została znaleziona dla modyfikacji \"{1}\". Sprawdź, czy nie została usunięta lub zmieniona nazwa.");
            french.Add("configNotFound", "Impossible de trouver la configuration \"{0}\" pour le mod \"{1}\". Il est possible qu`il sois supprimé ou changé de nom");

            //Componet: 
            //
            english.Add("modDeactivated", "The mod \"{0}\" is currently deactivated in the modpack and could not to be selected to install.");
            german.Add("modDeactivated", "Der Mod \"{0}\" ist derzeit im Modpack deaktiviert und steht für die Installation nicht zur Verfügung.");
            polish.Add("modDeactivated", "Modyfikacja \"{0}\" jest obecnie zdezaktywowana w paczce i nie może zostać wybrana.");
            french.Add("modDeactivated", "Le mod est actuellement désactivé dans le mod pack et ne seras pas installé");

            //Componet: 
            //
            english.Add("configDeactivated", "The config \"{0}\" of \"{1}\" is currently deactivated in the modpack and could not to be selected to install.");
            german.Add("configDeactivated", "Die Konfiguration \"{0}\" von \"{1}\" ist derzeit im Modpack deaktiviert und steht für die Installation nicht zur Verfügung.");
            polish.Add("configDeactivated", "Konfiguracja \"{0}\" z \"{1}\" jest obecnie zdezaktywowana w paczce i nie może zostać wybrana.");
            french.Add("configDeactivated", "La configuration \"{0}\" du mod \"{1}\" est actuellement désactivé dans le mod pack et ne seras pas installé");

            //Componet: 
            //
            english.Add("modsNotFoundTechnical", "The following mods could not be found and were most likely removed. There are only technical names available:\n{0}");
            german.Add("modsNotFoundTechnical", "Die folgenden Modifikationen können nicht gefunden werden und wurden wahrscheinlich entfernt/gelöscht. Es sind leider nur technische Namen verfügbar:\n{0}");
            polish.Add("modsNotFoundTechnical", "Następujące modyfikacje nie zostały znalezione, najprawdopodobniej zostały usunięte:\n{0}");
            french.Add("modsNotFoundTechnical", "Les mods suivants n'ont pas pu être trouvés et ont probablement été supprimés. Il n'y a que des noms techniques disponibles:\n{0}");

            //Componet: 
            //
            english.Add("oldSavedConfigFile", "The saved preferences file your are using is in an outdated format and will be inaccurate in the future. Convert it to the new format? (A backup of the old format will be made)");
            german.Add("oldSavedConfigFile", "Die Konfigurationsdatei die benutzt wurde, wird in Zukunft immer ungenauer werden. Soll auf das neue Standardformat umgestellt werden? (Eine Sicherung des alten Formats erfolgt)");
            polish.Add("oldSavedConfigFile", "Zapisana konfiguracja jest w przestarzałym formacie i może powodować nieścisłości. Czy chcesz przekonwertować ją na nowszy zapis?");
            french.Add("oldSavedConfigFile", "Le fichier de préférences que vous avez choisis est un format obsolète et seras inexact dans le future. Convertire au nouveau format?");

            //Componet: 
            //
            english.Add("prefrencesSet", "Preferences Set");
            german.Add("prefrencesSet", "Bevorzugte Einstellungen");
            polish.Add("prefrencesSet", "Preferowane Ustawienia");
            french.Add("prefrencesSet", "Préférences Enregistrées");

            //Componet: 
            //
            english.Add("selectionsCleared", "Selections Cleared");
            german.Add("selectionsCleared", "Auswahl gelöscht");
            polish.Add("selectionsCleared", "Usunięto Zaznaczenia");
            french.Add("selectionsCleared", "Sélections effacées");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your file and folder security permissions");
            german.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn Sie dies wieder sehen, müssen Sie Ihre Datei- und Ordnersicherheitsberechtigungen reparieren");
            polish.Add("patchingSystemDeneidAccessMessage", "Nie uzyskano dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli widzisz to ponownie, to zmień ustawienia pozwoleń dostępu do folderów.");
            french.Add("patchingSystemDeneidAccessMessage", "Le système de patching s'est vu refuser l'accès au dossier de patch. Réessayez en tant qu'administrateur. Si vous voyez ceci à nouveau, assurez vous que vos permissions de sécurités de dossiers et de fichiers son suffisantes.");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessHeader", "Access Denied");
            german.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");
            french.Add("patchingSystemDeneidAccessHeader", "Accès refusé");

            //Componet: 
            //
            english.Add("databaseNotFound", "Database not found at supplied URL");
            german.Add("databaseNotFound", "Datenbank nicht an der angegebenen URL gefunden");
            polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");
            french.Add("databaseNotFound", "Base de données introuvable à L'URL fournie  ");

            //Componet:
            //
            english.Add("WoTRunningHeader", "WoT is Running");
            german.Add("WoTRunningHeader", "WoT wird gerade ausgeführt.");
            polish.Add("WoTRunningHeader", "WoT jest uruchomiony");
            french.Add("WoTRunningHeader", "WoT est en cours d`éxecution");

            //Componet:
            //
            english.Add("WoTRunningMessage", "Please close World of Tanks to continue");
            german.Add("WoTRunningMessage", "Um Fortzufahren, schliesse bitte World of Tanks.");
            polish.Add("WoTRunningMessage", "Proszę zamknąć World of Tanks, aby kontynuować");
            french.Add("WoTRunningMessage", "Veuillez fermer World of Tanks pour continuer");

            //Componet:
            //
            english.Add("InstallingTo", "Installing to {0}");
            german.Add("InstallingTo", "Installiere nach {0}");
            polish.Add("InstallingTo", "Instalowanie w {0}");
            french.Add("InstallingTo", "Installation à {0}");

            //Componet:
            //
            english.Add("zipReadingErrorHeader", "Incomplete Download");
            german.Add("zipReadingErrorHeader", "Unvollständiger Download");
            polish.Add("zipReadingErrorHeader", "Ściąganie niekompletne");
            french.Add("zipReadingErrorHeader", "Téléchargement incomplet");

            //Componet:
            //
            english.Add("zipReadingErrorMessage1", "The zip file");
            german.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            polish.Add("zipReadingErrorMessage1", "Plik skomresowany formatu ZIP ");
            french.Add("zipReadingErrorMessage1", "Le fichier ZIP");

            //Componet:
            //
            english.Add("zipReadingErrorMessage2", "could not be read, most likely due to an incomplete download. Please try again.");
            german.Add("zipReadingErrorMessage2", "Konnte nicht gelesen werden, da es höchstwahrscheinlich ein unvollständiger Download ist. Bitte versuche es später nochmal.");
            polish.Add("zipReadingErrorMessage2", "Nie można odczytać, prawdopodobnie niekompletność pobranych plików. Proszę spróbować ponownie");
            french.Add("zipReadingErrorMessage2", "n'as pas pus être lus, probablement un téléchargement incomplet. Veuillez réeissayer.");

            //Componet:
            //
            english.Add("zipReadingErrorMessage3", "Could not be read.");
            german.Add("zipReadingErrorMessage3", "Konnte nicht gelesen werden.");
            polish.Add("zipReadingErrorMessage3", "Nie można odczytać.");
            french.Add("zipReadingErrorMessage3", "n'as pas pus être lus");

            //Componet: Mod Selection view Group Box
            //
            english.Add("ModSelectionListViewSelection", "Selection View");
            german.Add("ModSelectionListViewSelection", "Darstellungsart");
            polish.Add("ModSelectionListViewSelection", "Widok wyborów");
            french.Add("ModSelectionListViewSelection", "Affichage de sélection");

            //Componet: Mod selection view default (relhax)
            //
            english.Add("selectionDefault", "Default");
            german.Add("selectionDefault", "Standard");
            polish.Add("selectionDefault", "Domyślne");
            french.Add("selectionDefault", "Normal");

            //Componet: Mod selection view legacy (OMC)
            //
            english.Add("selectionLegacy", "OMC Legacy");
            german.Add("selectionLegacy", "OMC (Baumstruktur)");
            polish.Add("selectionLegacy", "OMC Rozwijana lista");
            french.Add("selectionLegacy", "OMC Legacy");

            //Componet: Mod selection explanation
            //
            english.Add("selectionViewMode", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            german.Add("selectionViewMode", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            polish.Add("selectionViewMode", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            french.Add("selectionViewMode", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");

            //Componet: Detected client version
            //
            english.Add("detectedClientVersion", "Detected Client version");
            german.Add("detectedClientVersion", "Erkannte Client Version");
            polish.Add("detectedClientVersion", "Wykryta wersja klienta gry");
            french.Add("detectedClientVersion", "Version du client détecté");

            //Componet: Supported client versions
            //
            english.Add("supportedClientVersions", "Supported Clients");
            german.Add("supportedClientVersions", "Unterstützte Clients");
            polish.Add("supportedClientVersions", "Wspomagane wersje klienta gry");
            french.Add("supportedClientVersions", "Clients supportés");

            //Componet: Supported clients notice
            //
            english.Add("supportNotGuarnteed", "This client version is not offically supported. Mods may not work.");
            german.Add("supportNotGuarnteed", "Diese Client Version wird (noch) nicht offiziell unterstützt. Die Mods könnten nicht funktionieren oder sogar Dein World of Tanks beschädigen.");
            polish.Add("supportNotGuarnteed", "Ta wersja klienta gry nie jest oficjalnie wspomagana. Modyfikacje mogą nie działać prawidłowo.");
            french.Add("supportNotGuarnteed", "Ce client n'est pas supporté officiellement. Les mods risque de ne pas fonctionner.");

            //Componet: Failed To Delete folder
            //
            english.Add("folderDeleteFailed", "Failed to delete the folder");
            german.Add("folderDeleteFailed", "Löschen des Verzeichnis fehlgeschlagen");
            polish.Add("folderDeleteFailed", "Próba usunięcia folderu zakończona niepowodzeniem");
            french.Add("folderDeleteFailed", "Échec de la suppression du dossier");

            //Componet: Failed To Delete file
            //
            english.Add("fileDeleteFailed", "Failed to delete the file");
            german.Add("fileDeleteFailed", "Löschen der Datei fehlgeschlagen");
            polish.Add("fileDeleteFailed", "Próba usunięcia pliku zakończona niepowodzeniem");
            french.Add("fileDeleteFailed", "Échec de la supression du fichier");

            //Componet: OldFilesToDelete
            //Window header
            english.Add("foundOldFilesHeader", "Old Files Question");
            german.Add("foundOldFilesHeader", "alte Dateien löschen");
            polish.Add("foundOldFilesHeader", "Zapytanie o starsze wersje plików");
            french.Add("foundOldFilesHeader", "Question pour les fichiers anciens (français seulement)");

            //Componet: Found zip files to delete 1
            //
            english.Add("foundOldFilesDelete1", "The installer has found the following files that are old and can be deleted");
            german.Add("foundOldFilesDelete1", "Wir haben folgende veralteten Dateien gefunden die gelöscht werden können");
            polish.Add("foundOldFilesDelete1", "Instalator znalazł następujące stare pliki, które mogą zostać usunięte");
            french.Add("foundOldFilesDelete1", "L'installateur a trouvé les vieux fichiers suivants qui peuvent être supprimé");

            //Componet: Found zip files to delete 2
            //
            english.Add("foundOldFilesDelete2", "Would you like to delete them?");
            german.Add("foundOldFilesDelete2", "Möchtest du das sie gelöscht werden?");
            polish.Add("foundOldFilesDelete2", "Czy chcesz je usunąć?");
            french.Add("foundOldFilesDelete2", "Voulez vous les supprimés");

            //Componet: Expand current tab option
            //
            english.Add("expandAllButton", "Expand Current Tab");
            german.Add("expandAllButton", "Erweitere alle Einträge der aktuellen Registerkarte");
            polish.Add("expandAllButton", "Rozwiń bieżącą kartę");
            french.Add("expandAllButton", "Elargir l'onglet");

            //Componet: Colapse current tab option
            //
            english.Add("colapseAllButton", "Collapse Current Tab");
            german.Add("colapseAllButton", "Reduziere alle Einträge der aktuellen Registerkarte");
            polish.Add("colapseAllButton", "Zwiń bieżącą kartę");
            french.Add("colapseAllButton", "Réduire l'onglet");

            //Componet: font_MouseEnter
            //
            english.Add("font_MouseEnter", "Select a scale mode to use.");
            german.Add("font_MouseEnter", "Wähle einen Skalierungsgrad.");
            polish.Add("font_MouseEnter", "Wybierz tryb skali, który ma zostać użyty.");
            french.Add("font_MouseEnter", "Sélectionnez un mode d'échelle à utiliser.");

            //Componet: selectionView_MouseEnter
            //
            english.Add("selectionView_MouseEnter", "Select a mod selection list view.");
            german.Add("selectionView_MouseEnter", "wähle eine der Listenansichten.");
            polish.Add("selectionView_MouseEnter", "Wybierz listę wyboru modyfikacji.");
            french.Add("selectionView_MouseEnter", "Sélectionnez une vue de liste de sélection de mod.");

            //Componet: language_MouseEnter
            //
            english.Add("language_MouseEnter", "Select your preferred language.");
            german.Add("language_MouseEnter", "wähle Deine bevorzugte Sprache");
            polish.Add("language_MouseEnter", "Wybierz preferowany język.");
            french.Add("language_MouseEnter", "Sélectionnez votre langue préféré");

            //Componet: fontSizeGB
            //
            english.Add("fontSizeGB", "Scaling Mode");
            german.Add("fontSizeGB", "Skalierungsmodus");
            polish.Add("fontSizeGB", "Tryb skalowania");
            french.Add("fontSizeGB", "Mode d'échelle");

            //Componet: expandNodesDefault
            //
            english.Add("expandNodesDefault", "Expand all");
            german.Add("expandNodesDefault", "Alle erweitern");
            polish.Add("expandNodesDefault", "Rozwiń wszystkie");
            french.Add("expandNodesDefault", "Développer tout");

            //Component: expandAllDesc
            //
            english.Add("expandAllDesc", "Select this to have all options automatically expand. It applies for the Legacy Selection only.");
            german.Add("expandAllDesc", "Erweitere alle Einträge auf allen Registerkarten automatisch. Nur bei Ansicht als Baumstruktur.");
            polish.Add("expandAllDesc", "Zaznacz to, aby wszystkie opcje zostały automatycznie rozwinięte. Dotyczy tylko opcji Legacy Selection.");
            french.Add("expandAllDesc", "Sélectionnez cette option pour que toutes les options s'élargis automatiquement. S'applique uniquement à la Sélection Legacy");

            //Component: disableBordersCB
            //
            english.Add("disableBordersCB", "Disable borders");
            german.Add("disableBordersCB", "Begrenzungen deaktivieren");
            polish.Add("disableBordersCB", "Wyłącz obramowanie");
            french.Add("disableBordersCB", "Désactiver les bordures");

            //Component: disableBordersDesc
            //
            english.Add("disableBordersDesc", "Disable the black borders around each mod and config sublevel.");
            german.Add("disableBordersDesc", "Deaktiviere die schwarzen Ränder um jeden Mod und Config sublevel.");
            polish.Add("disableBordersDesc", "Wyłącz czarne obwiednie wokół każdego mod i config podpoziomu.");
            french.Add("disableBordersDesc", "Désactiver les bordures noires autour de chaque mods et sous-niveau de configuration");

            //Component: moveOutOfTanksLocation
            //
            english.Add("moveOutOfTanksLocation", "The modpack can not be run from the World_of_Tanks directory. Please move the application and try again.");
            german.Add("moveOutOfTanksLocation", "Das Modpack kann nicht aus dem World_of_Tanks Verzeichnis laufen. Bitte verschiebe die Anwendung in ein anderes Verzeichnis und versuchen Sie es erneut.");
            polish.Add("moveOutOfTanksLocation", "Modpack nie może być uruchomiony z katalogu World_of_Tanks. Przenieś aplikację i spróbuj ponownie.");
            french.Add("moveOutOfTanksLocation", "Le Mod pack ne peut pas être éxecuté a partir du dossier de World of Tanks. Veuillez déplacer l`application dans un autre dossier et réessayer");

            //Section: Messages from MainWindow

            //Component: Current database is same as last installed database (body)
            //
            english.Add("DatabaseVersionsSameBody", "The database has not changed since your last installation. Therefore there are no updates to your current mods selection. Continue anyway?");
            german.Add("DatabaseVersionsSameBody", "Die Datenbank  wurde seit Deiner letzten Installation nicht verändert. Daher gibt es keine Aktuallisierungen zu Deinen aktuellen Modifikationen. Trotzdem fortfahren?");
            polish.Add("DatabaseVersionsSameBody", "Baza danych nie została zmieniona od ostatniej instalacji, nie ma żadych aktualizacji dla wybranych uprzednio modyfikacji. Czy kontynuować?");
            french.Add("DatabaseVersionsSameBody", "La base de données n'a pas changé depuis votre dernière installation. Par conséquent, il n'y a pas de mise à jour pour votre sélection  de mods. Continuer de toute façon?");

            //Component: Current database is same as last installed database (header)
            //
            english.Add("DatabaseVersionsSameHeader", "Database version is the same");
            german.Add("DatabaseVersionsSameHeader", "Datenbank Version ist identisch");
            polish.Add("DatabaseVersionsSameHeader", "Wersja bazy danych jest taka sama");
            french.Add("DatabaseVersionsSameHeader", "La version de la base de données est la même");

            //Section saveConfig
            //
            english.Add("selectWhereToSave", "Select where to save user prefs");
            german.Add("selectWhereToSave", "Bitte wähle wo die Konfiguation gespeichert werden soll");
            polish.Add("selectWhereToSave", "Wybór lokalizacji zapisu preferencji użytkownika");
            french.Add("selectWhereToSave", "Sélectionnez la location pour enregistrer les préférences utilisateur");

            //Section addModTreeview
            //
            english.Add("updated", "updated");
            german.Add("updated", "aktualisiert");
            polish.Add("updated", "zaktualizowane");
            french.Add("updated", "Mis à jours");

            //Section: SelectionViewer

            //Component: SelectConfigLabel
            //The label in the window informing the user to select a config option to use
            english.Add("SelectConfigLabel", "Select a config to load");
            german.Add("SelectConfigLabel", "Wähle eine Konfiguation zum Laden aus");
            polish.Add("SelectConfigLabel", "Wybierz konfigurację do załadowania");
            french.Add("SelectConfigLabel", "Sélectionnez une configuration à charger");

            //Component: localFile
            //The text in the first radioButton in the selection viewer, for the user to select their own personal config file to load
            english.Add("localFile", "Local File");
            german.Add("localFile", "Lokale Datei");
            polish.Add("localFile", "Plik lokalny");
            french.Add("localFile", "Fichier local");

            //Component: radioButtonToolTip
            //
            english.Add("createdAt", "Created at: {0}");
            german.Add("createdAt", "Erstellt am: {0}");
            polish.Add("createdAt", "Utworzono w: {0}");
            french.Add("createdAt", "Créé à: {0}");

            //Section: Installer Messages

            //Componet: extractingPackage
            //
            english.Add("extractingPackage", "Extracting package");
            german.Add("extractingPackage", "Entpacke Paket");
            polish.Add("extractingPackage", "Wypakowywanie paczki");
            french.Add("extractingPackage", "Extraction du package");

            //Componet: extractingPackage
            //
            english.Add("file", "File");
            german.Add("file", "Datei");
            polish.Add("file", "Plik");
            french.Add("file", "Fichier");

            //Componet: extractingPackage
            //
            english.Add("size", "Size");
            german.Add("size", "Größe");
            polish.Add("size", "Rozmiar");
            french.Add("size", "Taille");

            //Componet: backupModFile
            //
            english.Add("backupModFile", "Backing up mod file");
            german.Add("backupModFile", "Mod Dateien sichern");
            polish.Add("backupModFile", "Stwórz kopię zapasową pliku modyfikacji");
            french.Add("backupModFile", "Sauvegarde du fichier mod");

            //Componet: backupUserdatas
            //
            english.Add("backupUserdatas", "Backing up user data");
            german.Add("backupUserdatas", "Benutzerdaten sichern");
            polish.Add("backupUserdatas", "Stwórz kopię zapasową danych użytkownika");
            french.Add("backupUserdatas", "Sauvegarde des données utilisateur");

            //Componet: deletingFiles
            //
            english.Add("deletingFiles", "Deleting files");
            german.Add("deletingFiles", "Lösche Dateien");
            polish.Add("deletingFiles", "Usuwanie plików");
            french.Add("deletingFiles", "Suppression de fichiers");

            //Componet: deletingWOTCache
            //
            english.Add("deletingWOTCache", "Deleting WoT cache");
            german.Add("deletingWOTCache", "WoT Zwischenspeicher löschen");
            polish.Add("deletingWOTCache", "Usuwanie cache WoTa");
            french.Add("deletingWOTCache", "Suppression du cache WoT");

            //Componet: restoringUserData
            //
            english.Add("restoringUserData", "Restoring user data");
            german.Add("restoringUserData", "Benutzerdaten wiederherstellen");
            polish.Add("restoringUserData", "Przywracanie danych użytkownika");
            french.Add("restoringUserData", "Restoration des données utilisateur");

            //Componet: patchingFile
            //
            english.Add("patchingFile", "Patching file");
            german.Add("patchingFile", "Datei wird geändert");
            polish.Add("patchingFile", "Aktualizowanie plików");
            french.Add("patchingFile", "Patch du fichier");

            //Componet: extractingUserMod
            //
            english.Add("extractingUserMod", "Extracting user mod");
            german.Add("extractingUserMod", "Benutzer Modifikationen entpacken");
            polish.Add("extractingUserMod", "Wypakowywanie modyfikacji");
            french.Add("extractingUserMod", "Extraction des mods utilisateur");

            //Componet: userPatchingFile
            //
            english.Add("userPatchingFile", "User patching file");
            german.Add("userPatchingFile", "Benutzer Datei wird geändert");
            polish.Add("userPatchingFile", "Aktualizowanie plików użytkownika");
            french.Add("userPatchingFile", "Fichier utilisateur patch");

            //Componet: installingUserFonts
            //
            english.Add("installingUserFonts", "Installing user fonts");
            german.Add("installingUserFonts", "Benutzer Schriftsätze installieren");
            polish.Add("installingUserFonts", "Instalowanie czcionek użytkownika");
            french.Add("installingUserFonts", "Installation des polices utilisateur");

            //Componet: CheckDatabase
            //
            english.Add("checkDatabase", "Checking the database for outdated or no longer needed files");
            german.Add("checkDatabase", "Durchsuche das Dateiarchive nach veralteten oder nicht mehr benötigten Dateien");
            polish.Add("checkDatabase", "Trwa przeszukiwanie w bazie danych przedawnionych i niepotrzebnych plików");
            french.Add("checkDatabase", "Vérification de la base de données pour les fichiers périmés ou non nécessaires");

            //Component: 
            //function checkForOldZipFiles() 
            english.Add("parseDownloadFolderFailed", "Parsing the \"{0}\" folder failed.");
            german.Add("parseDownloadFolderFailed", "Durchsehen des \"{0}\" Verzeichnisses ist fehlgeschlagen.");
            polish.Add("parseDownloadFolderFailed", "Pobieranie informacji o folderze \"{0}\" zakończone niepowodzeniem");
            french.Add("parseDownloadFolderFailed", "L'analyse du dossier \"{0}\" a échoué.");

            //Componet: installation finished
            //
            english.Add("installationFinished", "Installation is finished");
            german.Add("installationFinished", "Die Installation ist abgeschlossen");
            polish.Add("installationFinished", "Instalacja jest zakończona");
            french.Add("installationFinished", "L'installation est terminée");


            //Section: InstallFinished Components
            //Componet: InstallCompleteLabel
            //
            english.Add("InstallCompleteLabel", "The Installation is complete. Would you like to...");
            german.Add("InstallCompleteLabel", "Installation ist beendet. Willst du...");
            polish.Add("InstallCompleteLabel", "Instalacja jest zakończona. Czy chciałbyś...");
            french.Add("InstallCompleteLabel", "L'installation est terminée. Voudriez-vous...");

            //Componet: StartTanksButton
            //
            english.Add("StartTanksButton", "Start the game? (WorldofTanks.exe)");
            german.Add("StartTanksButton", "Das Spiel starten? (WorldofTanks.exe)");
            polish.Add("StartTanksButton", "Uruchomić grę? (WorldofTanks.exe)");
            french.Add("StartTanksButton", "Démarrez le jeu? (WorldofTanks.exe)");

            //Componet: StartWoTLauncherButton
            //
            english.Add("StartWoTLauncherButton", "Start the game launcher? (WoTLauncher.exe)");
            german.Add("StartWoTLauncherButton", "Den Spiel Launcher starten? (WoTLauncher.exe)");
            polish.Add("StartWoTLauncherButton", "Uruchomić launcher? (WoTLauncher.exe)");
            french.Add("StartWoTLauncherButton", "Démarrez le lanceur de jeux? (WoTLauncher.exe)");

            //Componet: StartXVMStatButton
            //
            english.Add("StartXVMStatButton", "Open your web browser to the xvm statistics login website?");
            german.Add("StartXVMStatButton", "Öffne Deinen Browser auf der XVM Statistik Login Webseite?");
            polish.Add("StartXVMStatButton", "Otworzyć stronę statystyk XVM ?");
            french.Add("StartXVMStatButton", "Ouvrir votre navigateur Web vers le site de connexion aux statistiques xvm?");

            //Componet: CloseApplicationButton
            //
            english.Add("CloseApplicationButton", "Close the application?");
            german.Add("CloseApplicationButton", "Anwendung schließen");
            polish.Add("CloseApplicationButton", "Zamknij aplikację or Zamknij program");
            french.Add("CloseApplicationButton", "Fermer l'application");

            //Componet: StartXVMStatButton_Click
            //localisation to which page you will jump
            english.Add("xvmUrlLocalisation", "en");
            german.Add("xvmUrlLocalisation", "de");
            polish.Add("xvmUrlLocalisation", "en");
            french.Add("xvmUrlLocalisation", "fr");

            //Componet: loadingGifpreview
            //GifPreview.cs
            english.Add("loadingGifpreview", "Loading Picture Preview");
            german.Add("loadingGifpreview", "Ladebild Vorschau");
            polish.Add("loadingGifpreview", "TODO");
            french.Add("loadingGifpreview", "TODO");
        }
    }
}
