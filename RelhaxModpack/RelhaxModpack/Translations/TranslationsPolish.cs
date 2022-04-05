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
        /// Loads all Polish translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsPolish()
        {
            #region General expressions
            Polish.Add("yes", "Tak");
            Polish.Add("no", "Nie");
            Polish.Add("cancel", "Anuluj");
            Polish.Add("delete", "Usuń");
            Polish.Add("warning", "OSTRZEŻENIE");
            Polish.Add("critical", "OSTRZEŻENIE"); // Not as "critical error" but info. Dayum... @Nullmaruzero
            Polish.Add("information", "Informacja");
            Polish.Add("select", "Wybierz");
            Polish.Add("abort", "Przerwij");
            Polish.Add("error", "Błąd");
            Polish.Add("retry", "Ponów");
            Polish.Add("ignore", "Ignoruj");
            Polish.Add("lastUpdated", "Ostatnio zaktualizowano: ");
            Polish.Add("stepsComplete", "zadania zakończone");
            Polish.Add("allFiles", "Wszystkie pliki");
            Polish.Add("GoogleTranslateLanguageKey", "pl");
            Polish.Add("at", "—"); // No short way for that. ALTERNATIVE: "z prędkością" (with [the] speed [of]) but it's clumsy and redundant. @Nullmaruzero
            Polish.Add("seconds", "sekund(y)");
            Polish.Add("minutes", "minut(y)");
            Polish.Add("hours", "godzin(y)");
            Polish.Add("days", "dni");
            Polish.Add("next", "Dalej");
            Polish.Add("ContinueButton", "Kontynuuj");
            Polish.Add("previous", "Wstecz");
            Polish.Add("close", "Zamknij");
            Polish.Add("none", "Brak");
            #endregion

            #region Application messages
            Polish.Add("appFailedCreateLogfile", "Aplikacja nie mogła otworzyć pliku dziennika. Sprawdź uprawnienia dostępu do pliku lub przenieś aplikację do folderu z dostępem do zapisu.");
            Polish.Add("failedToParse", "Plik nie mógł zostać przetworzony");
            Polish.Add("failedToGetDotNetFrameworkVersion", "Brak zainstalowanego .NET Framework. Może to oznaczać problem z prawami dostępu lub konflikt z oprogramowaniem antywirusowym blokującym dostęp.");
            Polish.Add("invalidDotNetFrameworkVersion", "Zainstalowana wersja .NET Framework jest za stara. Modpack wymaga .NET Framework w wersji 4.8 lub wyższej.\n" +
                "Czy chcesz przejść do strony pobierania najnowszej wersji .NET Framework?");
            #endregion

            #region Tray Icon
            Polish.Add("MenuItemRestore", "Przywróć");
            Polish.Add("MenuItemCheckUpdates", "Sprawdź aktualizacje");
            Polish.Add("MenuItemAppClose", Polish["close"]);
            Polish.Add("newDBApplied", "Zastosowano nową bazę danych");
            #endregion

            #region Main Window
            Polish.Add("InstallModpackButton", "Wybierz mody do instalacji");
            Polish.Add("selectWOTExecutable", "Wybierz plik wykonywalny WOT (WorldOfTanks.exe)");
            Polish.Add("InstallModpackButtonDescription", "Wybierz mody, które chcesz zainstalować w swoim kliencie WoT.");
            Polish.Add("UninstallModpackButton", "Odinstaluj Modpack Relhax");
            Polish.Add("UninstallModpackButtonDescription", "Usuń *wszystkie* zainstalowane mody.");
            Polish.Add("ViewNewsButton", "Wiadomości o aktualizacjach");
            Polish.Add("ViewNewsButtonDescription", "Przeczytaj wiadomości dot. aktualizacji aplikacji, bazy modów i innych.");
            Polish.Add("ForceManuelGameDetectionText", "Wymuś ręczny wybór lokacji gry");
            Polish.Add("ForceManuelGameDetectionCBDescription", "Wymusza ręczny wybór lokacji gry World of Tanks przez użytkownika.\n" +
                    "Zaznacz, jeśli występują problemy z automatycznym znalezieniem właściwej ścieżki gry.");
            Polish.Add("LanguageSelectionTextblock", "Język:");
            Polish.Add("LanguageSelectionTextblockDescription", "Zmienia język aplikacji.\nJeśli napotkasz brakujące lub błędne tłumaczenia, możesz nam je zgłosić.");
            Polish.Add("Forms_ENG_NAButtonDescription", "Przejdź do anglojęzycznego forum 'World of Tanks' dla serwerów NA.");
            Polish.Add("Forms_ENG_EUButtonDescription", "Przejdź do anglojęzycznego forum 'World of Tanks' dla serwerów EU.");
            Polish.Add("Forms_GER_EUButtonDescription", "Przejdź do niemieckojęzycznego forum 'World of Tanks' dla serwerów EU.");
            Polish.Add("SaveUserDataText", "Zapisz ustawienia użytkownika");
            Polish.Add("SaveUserDataCBDescription", "Zachowuje dane użytkownika (takie jak statystyki sesji z poprzednich bitew) przy instalacji modów.");
            Polish.Add("CleanInstallText", "Czysta instalacja (zalecane)");
            Polish.Add("CleanInstallCBDescription", "Odinstalowuje poprzednią instalację przed zainstalowaniem nowej. (ZALECANE)");
            Polish.Add("BackupModsText", "Stwórz kopię zapasową modów"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            Polish.Add("BackupModsSizeLabelUsed", "Kopie: {0}  Rozmiar: {1}");
            Polish.Add("backupModsSizeCalculating", "Obliczanie...");
            Polish.Add("BackupModsCBDescription", "Tworzy kopię zapasową wszystkich obecnie zainstalowanych modów i ich ustawień.\n" +
                     "Zostaną one spakowane w archiwum ZIP z sygnaturą czasową w nazwie i umieszczone w folderze „RelHaxModBackup”.");
            Polish.Add("BackupModsSizeLabelUsedDescription", Polish["BackupModsCBDescription"]);
            Polish.Add("SaveLastInstallText", "Zapamiętaj ostatnią kolekcję");
            Polish.Add("SaveLastInstallCBDescription", "Zapisuje mody wybrane przy instalacji i automatycznie przywraca je na liście wyboru przy kolejnej instalacji.");
            Polish.Add("MinimizeToSystemTrayText", "Minimalizuj do zasobnika systemowego");
            Polish.Add("MinimizeToSystemTrayDescription", "Minimalizuje aplikację do zasobnika systemowego zamiast ją zamykać.");
            Polish.Add("VerboseLoggingText", "Rozszerzone rejestrowanie zdarzeń");
            Polish.Add("VerboseLoggingCBDescription", "Zapisuje pełne komunikaty zdarzeń do pliku dziennika. Przydatne przy zgłaszaniu błędów.");
            Polish.Add("AllowStatsGatherText", "Wysyłaj anonimowe statystyki użytkowania");
            Polish.Add("AllowStatsGatherCBDescription", "Zezwala na zbieranie i wysyłanie anonimowych danych dot. wybieranych modów.\nDzięki temu możemy lepiej określić kierunek naszego wsparcia.");
            Polish.Add("DisableTriggersText", "Wyłącz wyzwalacze");
            Polish.Add("DisableTriggersCBDescription", "Przyśpiesza instalację wykonując pomniejsze zadania podczas wyodrębniania plików, " +
                 "gdy wszystkie wymagane czynności dla głównego zadania zostały już zakończone.\nWyzwalacze są automatycznie wyłączane przy instalowaniu własnych modów użytkownika.");
            Polish.Add("appDataFolderNotExistHeader", "Nie udało się znaleźć folderu pamięci podręcznej WoT");
            Polish.Add("CancelDownloadInstallButton", Polish["cancel"]);
            Polish.Add("appDataFolderNotExist", "Instalator nie wykrył folderu pamięci podręcznej cache. Kontynuować bez opróżnienia folderu cache?");
            Polish.Add("viewAppUpdates", "Wyświetl ostatnie aktualizacje aplikacji");
            Polish.Add("viewDBUpdates", "Wyświetl ostatnie aktualizacje bazy danych");
            Polish.Add("EnableColorChangeDefaultV2Text", "Podświetlanie zmian");
            Polish.Add("EnableColorChangeDefaultV2CBDescription", "Włącza podświatlenie wybranych i zmienionych elementów.");
            Polish.Add("EnableColorChangeLegacyText", "Podświetlanie zmian");
            Polish.Add("EnableColorChangeLegacyCBDescription", "Włącza podświetlenie wybranych i zmienionych elementów.");
            Polish.Add("ShowOptionsCollapsedLegacyText", "Zwiń wszystkie opcje");
            Polish.Add("ShowOptionsCollapsedLegacyCBDescription", "Zwija wszystkie elementy drzewka wyboru na liście (z wyjątkiem kategorii).");
            Polish.Add("ClearLogFilesText", "Wyczyść pliki dziennika");
            Polish.Add("ClearLogFilesCBDescription", "Czyści pliki dziennika WoT (python.log), XVM'a (xvm.log) oraz pmod-ów (pmod.log).");
            Polish.Add("CreateShortcutsText", "Utwórz skróty na pulpicie");
            Polish.Add("CreateShortcutsCBDescription", "Tworzy na pulpicie skróty do modyfikacji, które są plikami wykonywalnymi EXE (np. konfigurator moda WWIIHA).");
            Polish.Add("DeleteOldPackagesText", "Usuń stare pakiety");
            Polish.Add("DeleteOldPackagesCBDescription", "Zwalnia miejsce na dysku usuwając stare i nieużywane archiwa ZIP z folderu „RelhaxDownloads”.");
            Polish.Add("MinimalistModeText", "Tryb minimalistyczny");
            Polish.Add("MinimalistModeCBDescription", "Pomija instalację niektórych dodatkowych pakietów, takich jak motywy graficzne Relhax czy link do nas w garażu.");
            Polish.Add("AutoInstallText", "Automatyczna instalacja");
            Polish.Add("AutoInstallCBDescription", "Regularnie sprawdza dostępność aktualizacji modów i automatycznie je instaluje według pliku kolekcji i częstotliwości wybranych poniżej.");
            Polish.Add("OneClickInstallText", "Włącz instalację na kliknięcie");
            Polish.Add("ForceEnabledCB", "Wymuś włącznie wszystkich pakietów [!]");
            Polish.Add("AutoOneclickShowWarningOnSelectionsFailText", "Ostrzeż w przypadku błędów"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            Polish.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Ostrzega i umożliwia przerwanie w przypadku wystąpienia błędów z plikiem kolekcji.");
            Polish.Add("ForceEnabledText", "Odblokuj wszystkie pakiety [!]"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            Polish.Add("ForceEnabledCBDescription", "Odblokowuje wszystkie dezaktywowane pakiety.\nUWAGA: Może prowadzić do poważnych problemów ze stabilnością instalacji!");
            Polish.Add("ForceVisibleText", "Pokazuj wszystkie pakiety [!]"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            Polish.Add("ForceVisibleCBDescription", "Wyświetla na liście wyboru wszystkie ukryte pakiety i mody.\nUWAGA: Może prowadzić do poważnych problemów ze stabilnością instalacji!");
            Polish.Add("LoadAutoSyncSelectionFileText", "Wczytaj plik kolekcji");
            Polish.Add("LoadAutoSyncSelectionFileDescription", "Wczytuje wybrany plik kolekcji modów.\nUmożliwia używanie funkcji Instalacji na Kliknięcie oraz Automatycznej Instalacji.");
            Polish.Add("AutoSyncCheckFrequencyTextBox", "Częstotliwość: co");
            Polish.Add("DeveloperSettingsHeader", "Opcje programisty [!]");
            Polish.Add("DeveloperSettingsHeaderDescription", "Poniższe opcje mogą powodować problemy i niestabilność instalacji.\nUżywaj tylko jeśli wiesz co robisz!");
            Polish.Add("ApplyCustomScalingText", "Skalowanie aplikacji");
            Polish.Add("ApplyCustomScalingTextDescription", "Zastosuj skalowanie ekranu do okien instalatora.");
            Polish.Add("EnableCustomFontCheckboxText", "Użyj własnej czcionki");
            Polish.Add("EnableCustomFontCheckboxTextDescription", "Pozwala wybrać czcionkę używaną w większości okien spośród listy zainstalowanych w systemie czcionek");
            Polish.Add("LauchEditorText", "Uruchom edytor bazy danych");
            Polish.Add("LauchEditorDescription", "Uruchomi edytor bazy danych stąd, zamiast z wiersza poleceń.");
            Polish.Add("LauchPatchDesignerText", "Uruchom kreator łatek");
            Polish.Add("LauchPatchDesignerDescription", "Uruchomi kreator łatek stąd, zamiast z wiersza poleceń.");
            Polish.Add("LauchAutomationRunnerText", "Uruchom menedżera automatyzacji");
            Polish.Add("LauchAutomationRunnerDescription", "Uruchomi menedżera automatyzacji stąd, zamiast z wiersza poleceń.");
            Polish.Add("InstallWhileDownloadingText", "Wypakuj podczas pobierania");
            Polish.Add("InstallWhileDownloadingCBDescription", "Wyodrębnia każdy pakiet zaraz po jego pobraniu, zamiast po pobraniu wszystkich pakietów.");
            Polish.Add("MulticoreExtractionCoresCountLabel", "Rdzenie CPU: {0}");
            Polish.Add("MulticoreExtractionCoresCountLabelDescription", "Liczba rdzeni logicznych procesora (wątków) wykrytych w systemie.");
            Polish.Add("SaveDisabledModsInSelectionText", "Zapamiętaj niedostępne mody");
            Polish.Add("SaveDisabledModsInSelectionDescription", "Zaznacza poprzednio wybrane mody, które zostały przez nas zablokowane. (Jak tylko będą znów dostępne.)");
            Polish.Add("AdvancedInstallationProgressText", "Wyświetl szczegółowy podgląd instalacji");
            Polish.Add("AdvancedInstallationProgressDescription", "Wyświetla szczegółowe okno procesu instalacji. Przydatne przy włączonym wyodrębnianiu wielordzeniowym.");
            Polish.Add("ThemeSelectText", "Wybierz motyw:");
            Polish.Add("ThemeDefaultText", "Domyślny");
            Polish.Add("ThemeDefaultDescription", "Domyślny schemat kolorów.");
            Polish.Add("ThemeDarkText", "Ciemny");
            Polish.Add("ThemeDarkDescription", "Ciemny schemat kolorów.");
            Polish.Add("ThemeCustomText", "Własny");
            Polish.Add("ThemeCustomDescription", "Własny, niestandardowy schemat kolorów.");
            Polish.Add("DumpColorSettingsButtonText", "Zapisz obecny schemat kolorów");
            Polish.Add("OpenColorPickerButtonText", "Ustawienia schematu kolorów");
            Polish.Add("OpenColorPickerButtonDescription", "Otwiera narzędzie do wybierania kolorów.\nUmożliwia tworzenie własnych schematów kolorów.");
            Polish.Add("DumpColorSettingsButtonDescription", "Tworzy plik XML ze wszystkimi elementami z opcją niestandardowego koloru, aby umożliwić stworzenie własnego motywu.");
            Polish.Add("MulticoreExtractionText", "Wyodrębnianie wielordzeniowe");
            Polish.Add("MulticoreExtractionCBDescription", "Metoda wydorębniania równoległego – skraca czas instalacji wypakowując kilka archiwów ZIP jednocześnie.\n" +
                "Opcja przeznaczona TYLKO dla dysków SSD!");
            Polish.Add("UninstallDefaultText", "Standardowy");
            Polish.Add("UninstallQuickText", "Szybki");
            Polish.Add("ExportModeText", "Tryb eksportu");
            Polish.Add("ExportModeCBDescription", "Pozwala na wybór folderu i docelowej wersji WoT celem eksportu instalacji. Tylko dla zaawansowanych użytkowników!\n" +
                "UWAGA: Pomija to wyodrębnianie plików XML gry, patchy i tworzenie plików atlas. Instrukcje są dostępne w folderze docelowym po wyeksportowaniu.");
            Polish.Add("ViewCreditsButtonText", "Autorzy i współtwórcy Relhax Modpack"); // Authors and co-creators* of Relhax Modpack. (*also used as contributors) @Nullmaruzero
            Polish.Add("ViewCreditsButtonDescription", "Lista wszystkich wspaniałych ludzi i projektów, dzięki którym powstał ten modpack.");
            Polish.Add("ExportWindowDescription", "Wybierz wersję docelową klienta WoT:");
            Polish.Add("HelperText", "Witaj w Modpacku Relhax!\n\nMoim celem było stworzenie paczki modów tak prostej, jak to tylko możliwe,\n" +
                "ale jeśli nadal czujesz się nieco zagubiony, po prostu najedź kursorem na dowolną opcję i przeczytaj jej opis.\n\n" +
                "Dzięki za wybranie tego modpacka, mam nadzieję, że Ci się spodoba! — Willster419");
            Polish.Add("helperTextShort", "Witamy w paczce Relhax!");
            Polish.Add("NotifyIfSameDatabaseText", "Powiadom o braku nowej bazy danych*");//"Powiadom o braku nowej bazy danych"
            Polish.Add("NotifyIfSameDatabaseCBDescription", "Wyświetla powiadomienie kiedy używana jest najnowsza wersja bazy danych.\nOznacza to brak dostępnych aktualizacji modów.\n" +
                "Opcja dostępna tylko dla stabilnej wersji bazy danych (nie BETA).");
            Polish.Add("ShowInstallCompleteWindowText", "Szczegółowe podsumowanie instalacji");
            Polish.Add("ShowInstallCompleteWindowCBDescription", "Po zakończeniu instalacji wyświetla okno z częstymi kolejnymi krokami.");
            Polish.Add("applicationVersion", "Wersja");
            Polish.Add("databaseVersion", "Baza danych");
            Polish.Add("ClearCacheText", "Wyczyść pamięć podręczną WoT");
            Polish.Add("ClearCacheCBDescription", "Usuwa pliki z folderu pamięci podręcznej cache WoT. Działa tak samo jak opcja dostępna kiedyś w modpacku OMC.");
            Polish.Add("UninstallDefaultDescription", "Domyślna dezinstalacja usunie wszystkie pliki w folderze modów i powiązane pliki, także skróty" +
                " oraz pamięć podręczną aplikacji.");
            Polish.Add("UninstallQuickDescription", "Szybka dezinstalacja usuwa tylko pliki w folderze modów, pomijając skróty oraz pamięć podręczną aplikacji.");
            Polish.Add("DiagnosticUtilitiesButton", "Diagnostyka");
            Polish.Add("DiagnosticUtilitiesButtonDescription", "Zgłoś błąd, spróbuj naprawić klienta gry, itp.");
            Polish.Add("UninstallModeGroupBox", "Tryb Dezinstalacji:");
            Polish.Add("UninstallModeGroupBoxDescription", "Wybiera metodę dezinstalcji modów.");
            Polish.Add("FacebookButtonDescription", "Nasz Facebook");
            Polish.Add("DiscordButtonDescription", "Nasz Discord");
            Polish.Add("TwitterButtonDescription", "Nasz Twitter");
            Polish.Add("SendEmailButtonDescription", "Nasz email (nie dotyczy wsparcia technicznego)");
            Polish.Add("HomepageButtonDescription", "Odwiedz naszą strone");
            Polish.Add("DonateButtonDescription", "Dotacja na dalszy rozwój");
            Polish.Add("FindBugAddModButtonDescription", "Znalazłeś błąd? Chcesz dodać moda? Zgłoś się tutaj!");
            Polish.Add("SelectionViewGB", "Widok kolekcji"); //? This fella does not seem to be used in V2. @Nullmaruzero
            Polish.Add("SelectionDefaultText", "Domyślny");
            Polish.Add("SelectionLayoutDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            Polish.Add("SelectionDefaultDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            Polish.Add("SelectionLegacyDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            Polish.Add("LanguageSelectionGBDescription", "Wybierz preferowany język.");
            Polish.Add("EnableBordersDefaultV2Text", "Obramowanie elementów");
            Polish.Add("EnableBordersLegacyText", "Obramowanie elementów");
            Polish.Add("EnableBordersDefaultV2CBDescription", "Włącza czarne obramowanie modów i opcji konfiguracji.");
            Polish.Add("EnableBordersLegacyCBDescription", "Włącza czarne obramowanie modów i opcji konfiguracji.");
            Polish.Add("UseBetaDatabaseText", "Używaj wersji BETA bazy danych");
            Polish.Add("UseBetaDatabaseCBDescription", "Używa najnowszej wersji rozwojowej (beta) bazy danych. Stabilość modów nie jest gwarantowana!");
            Polish.Add("UseBetaApplicationText", "Używaj wersji BETA aplikacji");
            Polish.Add("UseBetaApplicationCBDescription", "Używa najnowszej wersji rozwojowej (beta) aplikacji. Stabilność oraz pełne tłumaczenie nie są gwarantowane!");
            Polish.Add("SettingsTabIntroHeader", "Witaj!");
            Polish.Add("SettingsTabSelectionViewHeader", "Opcje Widoku");
            Polish.Add("SettingsTabInstallationSettingsHeader", "Opcje Instalacji");
            Polish.Add("SettingsTabApplicationSettingsHeader", "Opcje Aplikacji");
            Polish.Add("SettingsTabAdvancedSettingsHeader", "Zaawansowane");
            Polish.Add("MainWindowSelectSelectionFileToLoad", "Wybierz plik kolekcji do wczytania");
            Polish.Add("verifyUninstallHeader", "Potwierdzenie");
            Polish.Add("verifyUninstallVersionAndLocation", "Czy chcesz odinstalować wszystkie mody z WoT?\n\n{0}\n\nWybrany tryb dezinstalacji: {1}");
            Polish.Add("failedVerifyFolderStructure", "Aplikacja nie mogła utworzyć wymaganej struktury folderów. Sprawdź swoje uprawnienia lub przenieś aplikację do folderu z prawami do zapisu.");
            Polish.Add("failedToExtractUpdateArchive", "Aplikacja nie mogła wyodrębnić plików aktualizacji. Sprawdź swoje uprawnienia oraz oprogramowanie antywirusowe.");
            Polish.Add("downloadingUpdate", "Pobieranie aktualizacji");
            //(removed components, disabled components, etc.)
            Polish.Add("AutoOneclickSelectionErrorsContinueBody", "Wystąpiły problemy ze wczytywaniem pliku kolekcji (prawdobodobnie dezaktywowane/usunięte pakiety itp.)" +
                "\nCzy mimo to, chcesz kontynuować?");
            Polish.Add("AutoOneclickSelectionErrorsContinueHeader", "Błędy wczytywania pliku kolekcji");
            Polish.Add("noAutoInstallWithBeta", "Automatyczna instalacja jest niedostępna dla rozwojowej bazy danych (BETA).");
            Polish.Add("autoInstallWithBetaDBConfirmBody", "Automatyczna Instalacja będzie dostępna dla testowej bazy danych (BETA), która jest często aktualizowana —" +
                " może to prowadzić do kilku aktualizacji w ciągu jednego dnia!\nCzy mimo to chcesz kontynuować?");
            Polish.Add("autoInstallWithBetaDBConfirmHeader", Polish["verifyUninstallHeader"]);
            Polish.Add("ColorDumpSaveFileDialog", "Wybierz miejsce zapisu pliku schematu kolorów");
            //"branch" is this context is git respoitory branches
            Polish.Add("loadingBranches", "Ładowanie gałęzi");
            //"branch" is this context is git respoitory branches
            Polish.Add("failedToParseUISettingsFile", "Nie udało się zastosować motywu. Szczegóły znajdziesz w pliku dziennika. Włącz \"Rozszerzone rejestrowanie zdarzeń\" dla dodatkowych informacji.");
            Polish.Add("UISettingsFileApplied", "Motyw zastosowany pomyślnie.");
            Polish.Add("failedToFindWoTExe", "Nie udało się odnaleźć folderu instalacji klienta WoT. Prosimy zgłosić ten błąd twórcom.");
            Polish.Add("failedToFindWoTVersionXml", "Nie udało się ustalić wersji klienta WoT. Sprawdź, czy plik 'version.xml' znajduje się w folderze gry (domyślnie: World_of_Tanks).");
            #endregion

            #region ModSelectionList
            Polish.Add("ModSelectionList", "Lista Wyboru Modów");
            Polish.Add("ContinueButtonLabel", "Zainstaluj");
            Polish.Add("CancelButtonLabel", Polish["cancel"]);
            Polish.Add("HelpLabel", "Wyświetl opis dowolnego elementu, klikając na niego prawym przyciskiem myszy.");
            Polish.Add("LoadSelectionButtonLabel", "Wczytaj kolekcję");
            Polish.Add("SaveSelectionButtonLabel", "Zapisz kolekcję");
            Polish.Add("SelectSelectionFileToSave", "Zapisz plik kolekcji");
            Polish.Add("ClearSelectionsButtonLabel", "Wyczyść pola wyboru");
            Polish.Add("SearchThisTabOnlyCB", "Szukaj tylko w tej zakładce");
            Polish.Add("searchComboBoxInitMessage", "Szukaj modów...");
            Polish.Add("SearchTBDescription", "Użyj gwiazdki (*), aby wyszukać wiele członów nazwy.\nPRZYKŁAD:\n\"config * willster419\" ZWRÓCI: " +
                " \"Willster419\'s Config\"");
            Polish.Add("InstallingAsWoTVersion", "Instalacja dla WoT w wersji: {0}");
            Polish.Add("UsingDatabaseVersion", "Aktywna baza danych: {0} ({1})");
            Polish.Add("userMods", "Własne mody"); // Changed to "own/custom mods" since it conveys a more detailed semantic meaning than "user mods". @Nullmaruzero
            Polish.Add("FirstTimeUserModsWarning", "Ta zakładka umożliwia wybór modów, które możesz umieścić w folderze \"RelhaxUserMods\"" +
                " Muszą to być archiwa ZIP używające struktury folderów takiej samej jak folder gry (domyślnie: World_of_Tanks).");
            Polish.Add("downloadingDatabase", "Pobieranie bazy danych");
            Polish.Add("readingDatabase", "Przetwarzanie bazy danych");
            Polish.Add("loadingUI", "Ładowanie interfejsu");
            Polish.Add("verifyingDownloadCache", "Sprawdzanie integralności plików: ");
            Polish.Add("InstallProgressTextBoxDescription", "Postęp instalacji będzie wyświetlany tutaj.");
            Polish.Add("testModeDatabaseNotFound", "BŁĄD KRYTYCZNY: Baza danych Trybu Testowego nie została znaleziona w lokacji:\n{0}");
            Polish.Add("duplicateMods", "OSTRZEŻENIE: Wykryto zduplikowany identyfikator pakietu!");
            Polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się przetworzyć bazy danych!\n\nSzczegóły znajdziesz w pliku dziennika.");
            Polish.Add("configSaveSuccess", "Kolekcja zapisana pomyślnie.");
            Polish.Add("selectConfigFile", "Wybierz plik kolekcji do wczytania");
            Polish.Add("configLoadFailed", "Nie udało się załadować pliku kolekcji. Otwieranie w trybie standardowym.");
            Polish.Add("modNotFound", "Pakiet (ID = „{0}”) nie został znaleziony w bazie danych. Mógł zostać zmieniony lub usunięty.");
            Polish.Add("modDeactivated", "Poniższe pakiety zostały na chwilę obecną dezaktywowane i nie mogą być zainstalowane");
            Polish.Add("modsNotFoundTechnical", "Nie udało się znaleźć poniższych pakietów, prawdopodobnie zostały usunięte");
            Polish.Add("modsBrokenStructure", "Poniższe pakiety zostały na chwilę obecną dezaktywowane z powodu zmian w strukturze pakietu. Jeśli chcesz je zainstalować, musisz je zaznaczyć ponownie.");
            Polish.Add("packagesUpdatedShouldInstall", "Od czasu ostatniego użycia tego pliku kolekcji, poniższe pakiety zostały zaktualizowane. Twój plik kolekcji został automatycznie zaktualizowany (utworzono też jego jednorazową kopię zapasową)." +
                "Jeśli jest to Twoja bieżąca instalacja i chcesz ją zachować, zaleca się instalację/aktualizację po tej wiadomości.");
            Polish.Add("selectionFileIssuesTitle", "Uwaga"); // Following RU pattern.
            Polish.Add("selectionFormatOldV2", "Format tego pliku kolekcji jest przestarzały (V2), zostanie stworzona jego kopia zapasowa i zostanie on zaktualizowany do V3.");
            Polish.Add("oldSavedConfigFile", "Używana konfiguracja jest w przestarzałym formacie i będzie niekompatybilna w przyszłości. Czy chcesz przekonwertować ją na nowy format?");
            Polish.Add("prefrencesSet", "Ustawienia zastosowane pomyślnie.");
            Polish.Add("selectionsCleared", "Odznaczono wszystkie elementy.");
            Polish.Add("failedLoadSelection", "Nie udało się wczytać kolekcji");
            Polish.Add("unknownselectionFileFormat", "Nieznany format pliku kolekcji");
            Polish.Add("ExpandAllButton", "Rozwiń bieżącą kartę");
            Polish.Add("CollapseAllButton", "Zwiń bieżącą kartę");
            Polish.Add("InstallingTo", "Folder instalacji: {0}");
            Polish.Add("selectWhereToSave", "Wybierz miejsce zapisu swojej kolekcji");
            Polish.Add("updated", "zaktualizowano");
            Polish.Add("disabled", "dezaktywowany");
            Polish.Add("invisible", "ukryty");
            Polish.Add("SelectionFileIssuesDisplay", "Błędy z zastosowaniem pliku kolekcji");
            Polish.Add("selectionFileIssues", Polish["SelectionFileIssuesDisplay"]);
            Polish.Add("selectionFileIssuesHeader", "Proszę zapoznać się z poniższymi informacjami dot. pliku kolekcji.");
            Polish.Add("VersionInfo", "Aktualizacja Aplikacji");
            Polish.Add("VersionInfoYesText", Polish["yes"]);
            Polish.Add("VersionInfoNoText", Polish["no"]);
            Polish.Add("NewVersionAvailable", "Nowa wersja dostępna");
            Polish.Add("HavingProblemsTextBlock", "Jeśli masz problem z aktualizacją,");
            Polish.Add("ManualUpdateLink", "kliknij tutaj");
            Polish.Add("loadingApplicationUpdateNotes", "Wczytywanie historii wersji aplikacji...");
            Polish.Add("failedToLoadUpdateNotes", "Nie udało się wczytać dziennika zmian");
            Polish.Add("ViewUpdateNotesOnGoogleTranslate", "Wyświetl w Tłumaczu Google");
            Polish.Add("VersionInfoAskText", "Czy chcesz zaktualizować teraz?");
            Polish.Add("SelectDownloadMirrorTextBlock", "Wybierz serwer pobierania");
            Polish.Add("SelectDownloadMirrorTextBlockDescription", "Wybrany serwer będzie używany tylko do pobierania pakietów.");
            Polish.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Dallas, USA");
            Polish.Add("downloadMirrorDe", "clanverwaltung.de, Frankfurt, Niemcy");
            #endregion

            #region Installer Messages
            Polish.Add("Downloading", "Pobieranie");
            Polish.Add("patching", "Patchowanie");
            Polish.Add("done", "Ukończono");
            Polish.Add("cleanUp", "Oczyszczanie zasobów");
            Polish.Add("idle", "Bezczynny");
            Polish.Add("status", "Stan:");
            Polish.Add("canceled", "Anulowano");
            Polish.Add("appSingleInstance", "Sprawdzanie pojedynczej instancji");
            Polish.Add("checkForUpdates", "Szukanie aktualizacji");
            Polish.Add("verDirStructure", "Sprawdzanie struktury katalogów");
            Polish.Add("loadingSettings", "Wczytywanie ustawnień");
            Polish.Add("loadingTranslations", "Wczytywanie tłumaczeń");
            Polish.Add("loading", "Wczytywanie");
            Polish.Add("of", "z");
            Polish.Add("failedToDownload1", "Nie udało się pobrać pakietu");
            Polish.Add("failedToDownload2", "Spróbować pobrać ponownie, przerwać instalację czy kontynuować?");
            Polish.Add("failedToDownloadHeader", "Ściąganie zakończone niepowodzeniem");
            Polish.Add("failedManager_version", "Aktualna wersja rozwojowa (BETA) aplikacji jest nieaktualna i musi zostać zaktualizowana do wersji stabilnej. Brak nowej wersji beta w trybie online.");
            Polish.Add("fontsPromptInstallHeader", "Uprawnienia Administratora");
            Polish.Add("fontsPromptInstallText", "Czy posiadasz uprawnienia administratora, aby zainstalować czcionki?");
            Polish.Add("fontsPromptError_1", "Instalacja czcionek nie powiodła się. Niektóre mody mogą nie działać prawidłowo. Czcionki znajdują się w ");
            Polish.Add("fontsPromptError_2", "\\_fonts. Zainstaluj je ręcznie lub uruchom aplikację jako administrator.");
            Polish.Add("cantDownloadNewVersion", "Nie udało się pobrać nowej wersji, zamykanie aplikacji.");
            Polish.Add("failedCreateUpdateBat", "Nie można utworzyć procesu aktualizatora.\n\nProszę ręcznie usunąć plik:\n{0}\n\nZmienić nazwę pliku:\n{1}\nna:\n{2}\n\nCzy chcesz otworzyć lokalizację pliku?");
            Polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
            Polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Użyj opcji ręcznego wybierania lokacji gry.");
            Polish.Add("anotherInstanceRunning", "Inna instancja Relhax Manager jest już uruchomiona");
            Polish.Add("closeInstanceRunningForUpdate", "Proszę zamknąć WSZYSTKIE działające instancje Relhax Modpack przed dalszym procesem aktualizacji.");
            Polish.Add("skipUpdateWarning", "Pomijasz aktualizację! Może wystąpić niekompatybilność bazy danych.");
            Polish.Add("patchDayMessage", "Paczka na chwilę obecną jest nieaktywna z powodu prac konserwacyjnych i aktualizacji modów dla nowej wersji WoT. Przepraszamy za utrudnienia." +
                " Jeśli jesteś zarządcą bazy danych, uruchom aplikację z odpowiednim argumentem wiersza poleceń"); // ASSUMED: Command line parameter (added to the shortcut).
            Polish.Add("configNotExist", "{0} nie istnieje, wczytywanie trybu normalnego");
            Polish.Add("autoAndFirst", "Pierwsze uruchomienie nie może być automatyczną instalacją, wczytywanie trybu normalnego");
            Polish.Add("confirmUninstallHeader", "Potwierdzenie");
            Polish.Add("confirmUninstallMessage", "Potwierdź odinstalowanie modów z WoT\n\n{0}\n\nUżyć metody '{1}'?");
            Polish.Add("uninstallingText", "Dezinstalacja w toku...");
            Polish.Add("uninstallingFile", "Odinstalowywanie pliku");
            Polish.Add("uninstallFinished", "Dezinstalacja modów zakończona");
            Polish.Add("uninstallFail", "Nie udało się odinstalować. Spróbuj innej metody dezinstalacji lub poinformuj nas o tym błędzie.");
            Polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest obecnie uruchomione," +
                " albo uprawnienia dostępu do plików i folderów są ustawione nieprawidłowo.");
            Polish.Add("extractionErrorHeader", Polish["error"]);
            Polish.Add("deleteErrorHeader", "Zamknij foldery");
            Polish.Add("deleteErrorMessage", "Proszę zamknąć folder mods lub res_mods (lub podfoldery), a następnie kliknij OK, żeby kontynuować.");
            Polish.Add("noUninstallLogMessage", "Plik dziennika zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chcesz usunąć wszystkie modyfikacje?");
            Polish.Add("noUninstallLogHeader", "Usuń wszystkie mody");
            Polish.Add("moveOutOfTanksLocation", "Modpack nie może być uruchamiany z katalogu World_of_Tanks. Przenieś aplikację i spróbuj ponownie.");
            Polish.Add("moveAppOutOfDownloads", "Wykryto, że aplikacja została uruchomiona z folderu 'Pobrane'. " +
                "Nie jest to zalecane z racji tworzenia przez aplikację wielu plików i folderów, które mogą być trudne do znalezienia w przypadku folderu 'Pobrane' z dużą ilością plików. " +
                "Zaleca się przeniesienie aplikacji wraz z plikami do nowego/osobnego folderu.");
            Polish.Add("DatabaseVersionsSameBody", "Baza danych nie została zaktualizowana od ostatniej instalacji — nie ma żadych aktualizacji dla ostatnio zainstalowanych modów.\n" +
                "Czy nadal chcesz kontynuować?");
            Polish.Add("DatabaseVersionsSameHeader", "Brak aktualizacji zainstalowanych modów.");
            Polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");
            Polish.Add("detectedClientVersion", "Wykryta wersja klienta");
            Polish.Add("supportedClientVersions", "Wspierane wersje klienta");
            Polish.Add("supportNotGuarnteed", "Ta wersja klienta gry nie jest oficjalnie wspierana. Mody mogą działać nieprawidłowo.\n"); // Line-break to fit with 'couldTryBeta' below. @Nullmaruzero
            Polish.Add("couldTryBeta", "Jeśli niedawo została wydana nowa wersja lub aktualizacja WoT, będziemy potrzebować trochę czasu na aktualizację paczki.\n\nW międzyczasie możesz spróbować użyć wersji rozwojowej (BETA) bazy danych.");
            Polish.Add("missingMSVCPLibrariesHeader", "Nie udało się wczytać wymaganych bibliotek");
            Polish.Add("missingMSVCPLibraries", "Nie udało się wczytać bibliotek odpowiedzialnych za przetwarzanie obrazów ikon. Prawdopodobnie wymagane biblioteki DLL nie są zainstalowane.");
            Polish.Add("openLinkToMSVCP", "Czy chcesz przejsć do strony pobierania wymaganego pakietu?");
            Polish.Add("noChangeUntilRestart", "Zmiany zostaną zastosowane po ponownym uruchomieniu aplikacji.");
            Polish.Add("installBackupMods", "Tworzenie kopii zapasowej pliku");
            Polish.Add("installBackupData", "Tworzenie kopii zapasowej danych użytkownika");
            Polish.Add("installClearCache", "Usuwanie pamięci podręcznej WoT");
            Polish.Add("installClearLogs", "Usuwanie plików dziennika");
            Polish.Add("installCleanMods", "Oczyszczanie folderu modów");
            Polish.Add("installExtractingMods", "Wyodrębnianie pakietu");
            Polish.Add("installZipFileEntry", "Element"); // Saw it used as a progress counter, it fits in this context. @Nullmaruzero
            Polish.Add("installExtractingCompletedThreads", "Zakończone procesy wyodrębniania");
            Polish.Add("installExtractingOfGroup", "z grupy instalacyjnej");
            Polish.Add("extractingUserMod", "Wyodrębnianie paczek użytkownika");
            Polish.Add("installRestoreUserdata", "Przywracanie danych użytkownika");
            Polish.Add("installXmlUnpack", "Wyodrębnianie plików XML");
            Polish.Add("installPatchFiles", "Aktualizowanie plików");
            Polish.Add("installShortcuts", "Tworzenie skrótów");
            Polish.Add("installContourIconAtlas", "Tworzenie pliku Atlas");
            Polish.Add("installFonts", "Instalowanie czcionek");
            Polish.Add("installCleanup", "Czyszczenie");
            Polish.Add("AtlasExtraction", "Wyodrębnianie pliku Atlas");
            Polish.Add("copyingFile", "Kopiowanie plików");
            Polish.Add("deletingFile", "Usuwanie plików");
            Polish.Add("scanningModsFolders", "Analizowanie folderu modów");
            Polish.Add("file", "Plik");
            Polish.Add("size", "Rozmiar");
            Polish.Add("checkDatabase", "Skanowanie bazy danych w poszukiwaniu nieaktualnych i niepotrzebnych plików");
            Polish.Add("parseDownloadFolderFailed", "Przetwarzanie informacji o folderze \"{0}\" nie powiodło się.");
            Polish.Add("installationFinished", "Instalacja zakończona");
            Polish.Add("deletingFiles", "Usuwanie plików");
            Polish.Add("uninstalling", "Dezinstalacja w toku");
            Polish.Add("zipReadingErrorHeader", "Pobieranie niekompletne");
            Polish.Add("zipReadingErrorMessage1", "Archiwum ZIP ");
            Polish.Add("zipReadingErrorMessage3", "nie można odczytać.");
            Polish.Add("patchingSystemDeneidAccessMessage", "Aktualizator nie mógł uzyskać dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli problem się powtarza, zmień uprawnienia" +
                " dostępu do plików i folderów.");
            Polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");
            Polish.Add("folderDeleteFailed", "Próba usunięcia folderu zakończona niepowodzeniem");
            Polish.Add("fileDeleteFailed", "Próba usunięcia pliku zakończona niepowodzeniem");
            Polish.Add("DeleteBackupFolder", "Kopie zapasowe");
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            Polish.Add("installFailed", "Instalacja niepowiodła się przy następujących krokach");
            #endregion

            #region Install finished window
            Polish.Add("InstallFinished", "Instalacja Zakończona");
            Polish.Add("InstallationCompleteText", "Instalacja zakończona. Czy chcesz teraz...");
            Polish.Add("InstallationCompleteStartWoT", "Uruchomić grę? (WorldofTanks.exe)");
            Polish.Add("InstallationCompleteStartGameCenter", "Uruchomić WG Game Center?");
            Polish.Add("InstallationCompleteOpenXVM", "Przejść do strony logowania moda XVM?");
            Polish.Add("InstallationCompleteCloseThisWindow", "Zamknąć to okno?");
            Polish.Add("InstallationCompleteCloseApp", "Zamknąć aplikację?");
            Polish.Add("xvmUrlLocalisation", "en");
            Polish.Add("CouldNotStartProcess", "Nie udało się uruchomić procesu");
            #endregion

            #region Diagnostics
            Polish.Add("Diagnostics", "Diagnostyka");
            Polish.Add("DiagnosticsMainTextBox", "Poniższe opcje pomogą zdiagnozować i rozwiązać napotkane problemy.");
            Polish.Add("LaunchWoTLauncher", "Uruchom WoT w trybie sprawdzania integralności.");
            Polish.Add("CollectLogInfo", "Zapisz pliki dziennika, aby zgłosić problem");
            Polish.Add("CollectLogInfoButtonDescription", "Tworzy archiwum ZIP ze wszystkimi plikami dziennika, ułatwiając zgłaszanie błędów.");
            Polish.Add("DownloadWGPatchFilesText", "Pobieranie aktualizacji dla gier WG (HTTP)");
            Polish.Add("DownloadWGPatchFilesButtonDescription", "Pobiera pliki aktualizacji dla gier Wargaming za pomocą protokołu HTTP celem późniejszej instalacji w WGC.\n" +
                "Szczególnie przydatne dla osób, które nie mogą używać wbudowanego w Wargaming Game Center protokołu P2P.");
            Polish.Add("SelectedInstallation", "Aktywna instalacja WoT:");
            Polish.Add("SelectedInstallationNone", "(" + Polish["none"].ToLower() + ")");
            Polish.Add("collectionLogInfo", "Zbieranie plików dziennika...");
            Polish.Add("startingLauncherRepairMode", "Uruchamianie launchera WOT w trybie sprawdzania integralności plików...");
            Polish.Add("failedCreateZipfile", "Nie udało się stworzyć archiwum ZIP ");
            Polish.Add("launcherRepairModeStarted", "Uruchomiono tryb naprawy");
            Polish.Add("ClearDownloadCache", "Usuń tymczasowe pliki pobierania");
            Polish.Add("ClearDownloadCacheDatabase", "Usuń pliki tymczasowe bazy danych");
            Polish.Add("ClearDownloadCacheDescription", "Usuń wszystkie pliki w folderze \"RelhaxDownloads\"");
            Polish.Add("ClearDownloadCacheDatabaseDescription", "Usuń plik XML bazy danych. Spowoduje to ponowne sprawdzenie integralności wszystkich archiwów ZIP.\nWszystkie uszkodzone pliki zostaną ponownie pobrane, jeśli zostaną wybrane w następnej instalacji");
            Polish.Add("clearingDownloadCache", "Czyszczenie pamięci podręcznej pobierania");
            Polish.Add("failedToClearDownloadCache", "Czyszczenie pamięci podręcznej pobierania zakończone niepowodzeniem");
            Polish.Add("cleaningDownloadCacheComplete", "Wyczyszczono pamieć podręczną");
            Polish.Add("clearingDownloadCacheDatabase", "Usuwanie pamięci podręcznej bazy danych");
            Polish.Add("failedToClearDownloadCacheDatabase", "Czyszczenie pamięci podręcznej bazy danych zakończone niepowodzeniem");
            Polish.Add("cleaningDownloadCacheDatabaseComplete", "Wyczyszczono pamięć podręczną bazy danych");
            Polish.Add("ChangeInstall", "Zmień wybraną powyżej instalację WoT");
            Polish.Add("ChangeInstallDescription", "Zmieni to pliki dziennika dodawane do diagnostycznego archiwum ZIP");
            Polish.Add("zipSavedTo", "Archiwum ZIP zapisano w: ");
            Polish.Add("selectFilesToInclude", "Wybierz pliki, które będą dołączone do zgłoszenia błędu");
            Polish.Add("TestLoadImageLibraries", "Test bibliotek przetwarzania plików Atlas");
            Polish.Add("TestLoadImageLibrariesButtonDescription", "Próbuje wczytać biblioteki przetwarzania obrazów Atlas.\n" +
                "Ewentualne błędy mogą być oznakiem braku wymaganego oprogramowania w systemie."); //=> Potential (load) errors may indicate missing dependencies in your system.
            Polish.Add("loadingAtlasImageLibraries", "Wczytywanie bibliotek przetwarzania obrazów Atlas");
            Polish.Add("loadingAtlasImageLibrariesSuccess", "Wczytywanie bibliotek przetwarzania obrazów Atlas zakończone sukcesem");
            Polish.Add("loadingAtlasImageLibrariesFail", "Wczytywanie bibliotek przetwarzania obrazów Atlas zakończone niepowodzeniem");
            Polish.Add("CleanupModFilesText", "Usuń mody z niewłaściwych folderów");
            Polish.Add("CleanupModFilesButtonDescription", "Czyści wszelkie mody z folderów takich jak win32 i win64 mogące powodować problemy z wczytywaniem.");
            Polish.Add("cleanupModFilesCompleted", "Oczyszczanie modów z niewłaściwych lokalizacji zakończone pomyślnie.");
            Polish.Add("CleanGameCacheText", "Wyczyść pamięc podręczną gry");
            Polish.Add("cleanGameCacheProgress", "Czyszczenie pamięci podręcznej gry");
            Polish.Add("cleanGameCacheSuccess", "Czyszczenie pamięci podręcznej gry zakończone sukcesem");
            Polish.Add("cleanGameCacheFail", "Nie udało się wyczyścić pamięci podręcznej gry");
            Polish.Add("TrimRelhaxLogfileText", "Zachowaj w pliku dziennika tylko 3 ostatnie uruchomienia");
            Polish.Add("trimRelhaxLogProgress", "Ograniczanie zakresu pliku dziennika");
            Polish.Add("trimRelhaxLogSuccess", "Ograniczanie zakresu pliku dziennika zakończone powodzeniem");
            Polish.Add("trimRelhaxLogFail", "Nie udało się ograniczyć zakresu pliku dziennika");
            #endregion

            #region Wot Client install selection
            Polish.Add("WoTClientSelection", "Wybór klienta WOT");
            Polish.Add("ClientSelectionsTextHeader", "Lista automatycznie wykrytych instalacji WOT");
            Polish.Add("ClientSelectionsCancelButton", Polish["cancel"]);
            Polish.Add("ClientSelectionsManualFind", "Wybór Ręczny");
            Polish.Add("ClientSelectionsContinueButton", Polish["select"]);
            Polish.Add("AddPicturesZip", "Dodaj pliki do archiwum ZIP");
            Polish.Add("DiagnosticsAddSelectionsPicturesLabel", "Dodaj wszelkie dodatkowe pliki (plik kolekcji, obrazy, itp.)");
            Polish.Add("DiagnosticsAddFilesButton", "Dodaj pliki");
            Polish.Add("DiagnosticsRemoveSelectedButton", "Usuń zaznaczone");
            Polish.Add("DiagnosticsContinueButton", Polish["ContinueButton"]);
            Polish.Add("cantRemoveDefaultFile", "Nie można usuwać pliku dodawanego domyślnie");
            #endregion

            #region Preview Window
            Polish.Add("Preview", "Podgląd");
            Polish.Add("noDescription", "Brak opisu");
            Polish.Add("noUpdateInfo", "Brak informacji o aktualizacji");
            Polish.Add("noTimestamp", "Brak sygnatury czasowej");
            Polish.Add("PreviewNextPicButton", "Następny");
            Polish.Add("PreviewPreviousPicButton", "Poprzedni");
            Polish.Add("DevUrlHeader", "Linki twórców");
            Polish.Add("dropDownItemsInside", "Zawiera elementy");
            Polish.Add("popular", "popularne");
            Polish.Add("previewEncounteredError", "Okno podglądu napotkało błąd. Nie udało się wygenerować podglądu.");
            Polish.Add("popularInDescription", "Popularny pakiet");
            Polish.Add("controversialInDescription", "Kontrowersyjny pakiet");
            Polish.Add("encryptedInDescription", "Pakiet zaszyfrowany, nie można go przeskanować pod kątem potencjalnych wirusów.");
            Polish.Add("fromWgmodsInDescription", "Ten pakiet pochodzi z portalu WGMods (wgmods.net)");
            #endregion

            #region Developer Selection Window
            Polish.Add("DeveloperSelectionsViewer", "Podgląd wyborów");
            Polish.Add("DeveloperSelectionsTextHeader", "Kolekcja do wczytania");
            Polish.Add("DeveloperSelectionsCancelButton", Polish["cancel"]);
            Polish.Add("DeveloperSelectionsLocalFile", "Plik lokalny");
            Polish.Add("DeveloperSelectionsContinueButton", "Wybierz");
            Polish.Add("failedToParseSelections", "Nie udało się przetworzyć kolekcji");
            Polish.Add("lastModified", "Ostatnia modyfikacja");
            #endregion

            #region Advanced Installer Window
            Polish.Add("AdvancedProgress", "Szczegółowy Podgląd Instalacji");
            Polish.Add("PreInstallTabHeader", "Przygotowywanie"); // => "PREPARATION(s)", otherwise it's long and clumsy. @Nullmaruzero
            Polish.Add("ExtractionTabHeader", "Wyodrębnianie");
            Polish.Add("PostInstallTabHeader", "Zadania poinstalacyjne");
            Polish.Add("AdvancedInstallBackupData", "Tworzenie kopii zapasowej ustawień modów");
            Polish.Add("AdvancedInstallClearCache", "Czyszczenie pamięci podręcznej WoT");
            Polish.Add("AdvancedInstallClearLogs", "Czyszczenie plików dziennika");
            Polish.Add("AdvancedInstallClearMods", "Dezinstalacja poprzedniej instalacji");
            Polish.Add("AdvancedInstallInstallMods", "Wątek"); // Saw it used as a label with a counter (1, 2, 3...). @Nullmaruzero
            Polish.Add("AdvancedInstallUserInstallMods", "Instalowanie modów użytkownika");
            Polish.Add("AdvancedInstallRestoreData", "Przywracanie danych");
            Polish.Add("AdvancedInstallXmlUnpack", "Wyodrębnianie XML");
            Polish.Add("AdvancedInstallPatchFiles", "Aplikowanie poprawek (patchy)");
            Polish.Add("AdvancedInstallCreateShortcuts", "Tworzenie skrótów");
            Polish.Add("AdvancedInstallCreateAtlas", "Tworzenie plików Atlas");
            Polish.Add("AdvancedInstallInstallFonts", "Instalowanie czcionek");
            Polish.Add("AdvancedInstallTrimDownloadCache", "Ograniczanie pamięci podręcznej pobierania");
            Polish.Add("AdvancedInstallCleanup", "Czyszczenie");
            #endregion

            #region News Viewer
            Polish.Add("NewsViewer", "Przegląd Wiadomości");
            Polish.Add("application_Update_TabHeader", "Aplikacja");
            Polish.Add("database_Update_TabHeader", "Baza danych");
            Polish.Add("ViewNewsOnGoogleTranslate", "Wyświetl w Tłumaczu Google");
            #endregion

            #region Loading Window
            Polish.Add("ProgressIndicator", "Wczytywanie");
            Polish.Add("LoadingHeader", "Wczytywanie, proszę czekać");
            #endregion

            #region First Load Acknowledgements
            Polish.Add("FirstLoadAcknowledgments", "Pierwsze Uruchomienie — Umowa Licencyjna"); // Can't think of a gracious way to do this one. @Nullmaruzero
            Polish.Add("AgreementLicense", "Przeczytałem/am i zgadzam się z postanowieniami ");
            Polish.Add("LicenseLink", "Umowy Licencyjnej");
            Polish.Add("AgreementSupport1", "Rozumiem, że mogę uzyskać wsparcie techniczne na dedykowanym ");
            Polish.Add("AgreementSupportDiscord", "Discord");
            Polish.Add("AgreementHoster", "Rozumiem, że Relhax to platforma instalacji modów oraz ich hosting, a zespół Relhax nie jest odpowiedzialny za rozwój wszystkich oferowanych modów.");
            Polish.Add("AgreementAnonData", "Rozumiem, że Relhax V2 gromadzi i wysyła anonimowe dane użytkowania celem poprawy aplikacji, oraz że funkcję tę mogę wyłączyć w dowolnym momencie w zakładce 'Zaawansowane'.");
            Polish.Add("V2UpgradeNoticeText", "Wygląda na to, że przeprowadzasz aktualizację z wersji V1 na V2 po raz pierwszy.\n" +
                "Kliknięcie 'Kontynuuj' nieodwracalnie przekonwertuje strukturę plików.\nPrzed kontynuacją zaleca się stworzenie kopii zapasowej folderu z wersją V1."); //Line-breaks, guessing it will be more readable. @Nullmaruzero
            Polish.Add("upgradingStructure", "Konwertowanie struktury plików i folderów V1");
            #endregion

            #region Export Mode
            Polish.Add("ExportModeSelect", "Wybierz klienta WoT dla eksportu");
            Polish.Add("selectLocationToExport", "Wybierz folder docelowy eksportu instalacji");
            Polish.Add("ExportSelectVersionHeader", "Wybierz wersję docelową klienta WoT");
            Polish.Add("ExportContinueButton", Polish["ContinueButton"]);
            Polish.Add("ExportCancelButton", Polish["cancel"]);
            Polish.Add("ExportModeMajorVersion", "Wersja online folderu");
            Polish.Add("ExportModeMinorVersion", "Wersja WOT");
            #endregion

            #region Asking to close WoT
            Polish.Add("AskCloseWoT", "WoT jest uruchomiony!");
            Polish.Add("WoTRunningTitle", "WoT jest już uruchomiony");
            Polish.Add("WoTRunningHeader", "Wygląda na to, że World of Tanks jest obecnie uruchomione. Przed kontynuowaniem wymagane jest zamknięcie gry.");
            Polish.Add("WoTRunningCancelInstallButton", "Przerwij instalację");
            Polish.Add("WoTRunningRetryButton", "Ponów");
            Polish.Add("WoTRunningForceCloseButton", "Wymuś zamknięcie gry");
            #endregion

            #region Scaling Confirmation
            Polish.Add("ScalingConfirmation", "Zmiana Skalowania");
            Polish.Add("ScalingConfirmationHeader", "Ustawienia skalowania zostały zmienione. Czy chcesz je zachować?");
            Polish.Add("ScalingConfirmationRevertTime", "Automatyczne przywrócenie za {0} sekund(y)");
            Polish.Add("ScalingConfirmationKeep", "Zachowaj");
            Polish.Add("ScalingConfirmationDiscard", "Porzuć");
            #endregion

            #region Game Center download utility
            Polish.Add("GameCenterUpdateDownloader", "Pobieranie aktualizacji gier dla Wargaming Game Center");
            Polish.Add("GcDownloadStep1Header", "Wybór Klienta Gry");
            Polish.Add("GcDownloadStep1TabDescription", "Wybierz grę WG, dla której chcesz pobrać aktualizacje (WoT, WoWs, WoWp)");
            Polish.Add("GcDownloadStep1SelectClientButton", "Wybierz klienta gry");
            Polish.Add("GcDownloadStep1CurrentlySelectedClient", "Obecnie wybrany klient: {0}");
            Polish.Add("GcDownloadStep1NextText", Polish["next"]);
            Polish.Add("GcDownloadStep1GameCenterCheckbox", "Sprawdź aktualizacje tylko dla Wargaming Game Center");
            Polish.Add("GcDownloadSelectWgClient", "Wybierz klienta docelowej gry WG, dla której chcesz pobrać aktualizacje");
            Polish.Add("ClientTypeValue", "Brak");
            Polish.Add("LangValue", Polish["ClientTypeValue"]);
            Polish.Add("GcMissingFiles", "Twój klient nie posiada następujących plików definicji XML");
            Polish.Add("GcDownloadStep2Header", "Zamykanie WG Game Center");
            Polish.Add("GcDownloadStep2TabDescription", "Proszę zamknąć WG Game Center (aplikacja automatycznie wykryje zamknięcie).");
            Polish.Add("GcDownloadStep2GcStatus", "Game Center jest {0}");
            Polish.Add("GcDownloadStep2GcStatusOpened", "uruchomione");
            Polish.Add("GcDownloadStep2GcStatusClosed", "zamknięte");
            Polish.Add("GcDownloadStep2PreviousText", Polish["previous"]);
            Polish.Add("GcDownloadStep2NextText", Polish["next"]);
            Polish.Add("GcDownloadStep3Header", "Informacje o aktualizacji");
            Polish.Add("GcDownloadStep3TabDescription", "Przetwarzanie listy plików aktualizacji do pobrania");
            Polish.Add("GcDownloadStep3NoFilesUpToDate", "Brak plików do pobrania, wszystkie są aktualne.");
            Polish.Add("GcDownloadStep3PreviousText", Polish["previous"]);
            Polish.Add("GcDownloadStep3NextText", Polish["next"]);
            Polish.Add("GcDownloadStep4Header", "Pobieranie Aktualizacji");
            Polish.Add("GcDownloadStep4TabDescription", "Pobieranie plików aktualizacji...");
            Polish.Add("GcDownloadStep4DownloadingCancelButton", Polish["cancel"]);
            Polish.Add("GcDownloadStep4DownloadingText", "Pobieranie aktualizacji {0} z {1}: {2}");
            Polish.Add("GcDownloadStep4DownloadComplete", "Pobieranie plików aktualizacji zakończone sukcesem!");
            Polish.Add("GcDownloadStep4PreviousText", Polish["previous"]);
            Polish.Add("GcDownloadStep4NextText", Polish["next"]);
            Polish.Add("GcDownloadStep5Header", "Zakończono!");
            Polish.Add("GcDownloadStep5TabDescription", "Proces zakończony sukcesem! WG Game Center powinno wykryć pobrane pliki aktualizacji przy uruchomieniu.");
            Polish.Add("GcDownloadStep5CloseText", Polish["close"]);
            Polish.Add("FirstLoadSelectLanguage", "Wybór Języka");
            Polish.Add("SelectLanguageHeader", "Wybierz język aplikacji");
            Polish.Add("SelectLanguagesContinueButton", Polish["ContinueButton"]);
            Polish.Add("Credits", "Lista zasług Relhax Modpack");
            Polish.Add("creditsProjectLeader", "Przewodnictwo Projektu");
            Polish.Add("creditsDatabaseManagers", "Administracja Bazy Danych");
            Polish.Add("creditsTranslators", "Tłumaczenia");
            Polish.Add("creditsusingOpenSourceProjs", "Otwarte oprogramowanie wykorzystywane w modpacku");
            Polish.Add("creditsSpecialThanks", "Specjalne podziękowania dla");
            Polish.Add("creditsGrumpelumpf", "Lidera projektu OMC, za umożliwienie nam kontynuacji projektu po swoim odejściu");
            Polish.Add("creditsRkk1945", "Pierwszego beta-testera, który pracował ze mną nad uruchomieniem projektu przez miesiące");
            Polish.Add("creditsRgc", "Za sponsorowanie modpacka oraz zostanie moją pierwszą grupą beta-testerów");
            Polish.Add("creditsBetaTestersName", "Naszych Beta-Testerów");
            Polish.Add("creditsBetaTesters", "Za nieustanne testowanie i zgłaszanie błędów zanim aplikacja trafi do wszystkich użytkowników");
            Polish.Add("creditsSilvers", "Za pomoc w komunikacji ze społecznością i pozyskiwanie nowych kontaktów");
            Polish.Add("creditsXantier", "Za początkowe wsparcie informatyczne oraz konfigurację naszego serwera");
            Polish.Add("creditsSpritePacker", "Za stworzenie algorytmu kompresji sprite'ów i konwersję na platformę .NET");
            Polish.Add("creditsWargaming", "Za stworzenie łatwego w automatyzacji systemu modów");
            Polish.Add("creditsUsersLikeU", "Dla użytkowników takich jak Ty");
            #endregion

            #region Conflicting Packages Dialog
            Polish.Add("ConflictingPackageDialog", "Konflikt Modów");
            Polish.Add("conflictingPackageMessageOptionA", "Opcja A");
            Polish.Add("conflictingPackageMessageOptionB", "Opcja B");
            Polish.Add("conflictingPackageMessagePartA", "Wybrano pakiet \"{0}\", który konfliktuje z innymi opcjami, które wybrano:");
            Polish.Add("conflictingPackagePackageOfCategory", "- {0} w kategorii {1}");
            Polish.Add("conflictingPackageMessagePartB", Polish["conflictingPackageMessageOptionA"] + ": Wybierz \"{0}\" i pomiń konfliktujące z tym pakiety");
            Polish.Add("conflictingPackageMessagePartC", Polish["conflictingPackageMessageOptionB"] + ": Zostaw konfliktujące pakiety, ale pomiń \"{0}\"");
            Polish.Add("conflictingPackageMessagePartD", "Zamknięcie tego okna spowoduje automatyczny wybór Opcji B");
            Polish.Add("conflictingPackageMessagePartE", "Którą opcję wybierasz?");
            #endregion

            #region End of Life announcement
            Polish.Add("EndOfLife", "Koniec Wsparcia dla Relhax");
            Polish.Add("CloseWindowButton", Polish["close"]);
            Polish.Add("WoTForumAnnouncementsTextBlock", "Posty z ogłoszeniem na forum WOT:");
            Polish.Add("endOfLifeMessagePart1", "20/04/2022 projekt Relhax Modpack zakończył swoją działalność. " +
                "Serdecznie dziękuję wszystkim naszym kontrybutorom i użytkownikom za ostatnie ponad pięć lat!");
            Polish.Add("endOfLifeMessagePart2a", "1 stycznia 2017 postanowiłem, że podejmę się wyzwania nie tylko odtworzenia modpacka OMC, z nowoczesnym interfejsem, " +
                "ale także stworzenia najszybszego systemu instalacji modów WOT, jaki kiedykolwiek istniał.");
            Polish.Add("endOfLifeMessagePart2b", "Zaczynałem z trzema innymi osobami w zespole - członkami OMC, którzy chcieli pomagać przy projekcie. " +
                "Przez następne 4 lata projektowałem, tworzyłem i przerabiałem modpacka od deski do deski, spędzając nad tym dziesiątki tysięcy godzin.");
            Polish.Add("endOfLifeMessagePart2c", "Po pewnym czasie nasz zespół rozrósł się do ponad ośmiu osób z większości globalnych serwerów WOT. " +
                "W tym czasie niesamowicie rozwijałem się jako programista, poznawałem standardy tworzenia oprogramowania oraz świetnie opanowałem temat wielowątkowości aplikacji.");
            Polish.Add("endOfLifeMessagePart2d", "Dzięki temu projektowi zdobyłem masę doświadczenia oraz poznałem kulisy społeczności moderskiej. " +
                "Wiedzę tę spożytkowałem wsparciem grupy Relic Gaming Community, której byłem członkiem od 2014.");
            Polish.Add("endOfLifeMessagePart3a", "W tym roku udało mi się zakończyć prace nad najbardziej zoptymizowanym i najszybszym modpackiem, jaki mogłem stworzyć dla tej społeczności.");
            Polish.Add("endOfLifeMessagePart3b", "Widząc, że wszystkie moje oryginalne cele i założenia dot. projektu zostały osiągnięte oraz moje zanikające zainteresowanie grą, zdecydowałem się zakończyć projekt.");
            Polish.Add("endOfLifeMessagePart3c", "Nie była to łatwa decyzja, ale nie chciałem kontynuować pracy i wsparcia projektu, którego utrzymywanie nie sprawiało mi już radości.");
            Polish.Add("endOfLifeMessagePart3d", "Myślę, że odbiłoby się to na jakości modpacka, co byłoby nie fair w stosunku do jego użytkowników. " +
                "Pragnąłem wygasić projekt kiedy znajdował się jeszcze w dobrej kondycji.");
            Polish.Add("endOfLifeMessagePart4", "Jeszcze raz dziękuję Wam wszystkim bardzo serdecznie. To było świetne 5 lat i będzie mi tego brakowało.");
            #endregion
        }
    }
}