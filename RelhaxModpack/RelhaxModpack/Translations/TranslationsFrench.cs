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
        /// Loads all French translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsFrench()
        {
            #region General expressions
            French.Add("yes", "Oui");
            French.Add("no", "Non");
            French.Add("cancel", "Annuler");
            French.Add("delete", "Supprimer");
            French.Add("warning", "ATTENTION");
            French.Add("critical", "CRITIQUE");

            French.Add("information", "information");
            French.Add("select", "Sélectionner");
            French.Add("abort", "Abandonner");
            French.Add("error", "Erreur");
            French.Add("retry", "Réessayer");
            French.Add("ignore", "Ignorer");
            French.Add("lastUpdated", "Dernière mise à jour : ");
            French.Add("stepsComplete", "tâches terminées");
            French.Add("allFiles", "Tous les fichiers");
            French.Add("GoogleTranslateLanguageKey", "fr");
            French.Add("at", "à");

            French.Add("seconds", "secondes");
            French.Add("minutes", "minutes");
            French.Add("hours", "heures");
            French.Add("days", "jours");
            French.Add("next", "Suivant");
            French.Add("ContinueButton", "Continuer");
            French.Add("previous", "Précedent");
            French.Add("close", "Fermer");
            French.Add("none", "Aucun");
            #endregion

            #region Application messages
            French.Add("appFailedCreateLogfile", "L'application n'a pas réussi à ouvrir un fichier journal. Vérifiez vos autorisations de fichiers ou déplacez l'application vers un dossier avec accès en écriture.");
            French.Add("failedToParse", "Echec de l'analyse du fichier");
            French.Add("failedToGetDotNetFrameworkVersion", "Échec de l'obtention de la version installée de .NET Framework. Cela pourrait indiquer un problème d'autorisations ou votre antivirus pourrait le bloquer.");
            French.Add("invalidDotNetFrameworkVersion", "La version installée de .NET Framework est inférieure à 4.8. Relhax Modpack nécessite la version 4.8 ou supérieure pour fonctionner. " +
                "Voulez-vous ouvrir un lien pour obtenir la dernière version ?");
            #endregion

            #region Tray Icon
            French.Add("MenuItemRestore", "Restaurer");
            French.Add("MenuItemCheckUpdates", "Vérifier les mises à jour");
            French.Add("MenuItemAppClose", French["close"]);
            French.Add("newDBApplied", "Nouvelle version de base de données appliquée");
            #endregion

            #region Main Window
            French.Add("InstallModpackButton", "Commencer la sélection des mods");
            French.Add("selectWOTExecutable", "Sélectionnez votre exécutable de WoT (WorldOfTanks.exe)");
            French.Add("InstallModpackButtonDescription", "Sélectionnez les mods que vous souhaitez installer sur votre client WoT");
            French.Add("UninstallModpackButton", "Désinstaller Relhax Modpack");
            French.Add("UninstallModpackButtonDescription", "Supprimer *tous* les mods installés sur votre client WoT");
            French.Add("ViewNewsButton", "Voir les notes de mises à jour");
            French.Add("ViewNewsButtonDescription", "Afficher les actualités sur l'application, la base de données et autres actualités");
            French.Add("ForceManuelGameDetectionText", "Forcer la détection manuelle");
            French.Add("ForceManuelGameDetectionCBDescription", "Cette option force une détection manuelle de l'emplacement du jeu. " +
                    "Cochez-la si vous rencontrez des problèmes pour localiser automatiquement le jeu.");
            French.Add("LanguageSelectionTextblock", "Sélection de la langue");
            French.Add("LanguageSelectionTextblockDescription", "Sélectionner votre langue.\nSi vous rencontrez des traductions manquantes ou des erreurs, n'hésitez pas à nous en informer.");
            French.Add("Forms_ENG_NAButtonDescription", "Accéder à la page anglophone du forum 'World of Tanks' pour le serveur NA");
            French.Add("Forms_ENG_EUButtonDescription", "Accéder à la page anglophone du forum 'World of Tanks' pour le serveur EU");
            French.Add("Forms_GER_EUButtonDescription", "Accéder à la page allemande du forum 'World of Tanks' pour le serveur EU");
            French.Add("SaveUserDataText", "Sauvegarder les données utilisateur");
            French.Add("SaveUserDataCBDescription", "Si cette option est sélectionnée, l'installateur va sauvegarder les données de l'utilisateur" +
                " (comme les statistiques de la session des batailles précédentes");
            French.Add("CleanInstallText", "Installation propre (recommandée)");
            French.Add("CleanInstallCBDescription", "Cette option recommandée va désinstaller votre installation précédente avant d'installer la nouvelle");
            French.Add("BackupModsText", "Sauvegarder le dossier des mods");
            French.Add("BackupModsSizeLabelUsed", "Sauvegardes: {0}  Taille: {1}");
            French.Add("backupModsSizeCalculating", "Calcul de la taille des sauvegardes...");
            French.Add("BackupModsCBDescription", "Sélectionnez cette option pour faire une sauvegarde de votre installation actuelle." +
                     "Les sauvegardes sont stockées dans le dossier 'RelHaxModBackup' en tant que zip nommé par un horodatage." );
            French.Add("BackupModsSizeLabelUsedDescription", French["BackupModsCBDescription"]);
            French.Add("SaveLastInstallText", "Sauvegarder la denière configuration");
            French.Add("SaveLastInstallCBDescription", "Si cette option est cochée, l'installateur appliquera automatiquement votre dernière sélection utilisée");
            French.Add("MinimizeToSystemTrayText", "Réduire dans la barre d'état système");
            French.Add("MinimizeToSystemTrayDescription", "Si cette option est cochée, l'application va continuer de s'éxécuter dans la barre d'état système");
            French.Add("VerboseLoggingText", "Enregistrement détaillé");
            French.Add("VerboseLoggingCBDescription", "Activer plus de messages de journalisation dans le fichier journal. Utile pour signaler des bugs");
            French.Add("AllowStatsGatherText", "Autoriser la collecte de statistiques sur l'utilisation du mod");
            French.Add("AllowStatsGatherCBDescription", "Autoriser l'installateur à upload des statistiques de données anonymes au serveur sur la sélection de mods. Cela nous permet de hiérarchiser le support");
            French.Add("DisableTriggersText", "Désactiver les déclencheurs");
            French.Add("DisableTriggersCBDescription", "Autoriser les déclencheurs peut accélérer l’installation en exécutant certaines tâches (comme la création d’icônes de contour) au cours de l’extraction "+
                 "une fois que toutes les ressources requises pour cette tâche sont prêtes. Ceci est automatiquement désactivé si des mods utilisateur sont détectés");
            French.Add("appDataFolderNotExistHeader", "Impossible de détecter le dossier de cache de WoT");
            French.Add("CancelDownloadInstallButton", French["cancel"]);
            French.Add("appDataFolderNotExist", "L'installateur n'a pas pu détecter le dossier de cache de WoT. Continuer l'installation sans nettoyer le cache de WoT ?");
            French.Add("viewAppUpdates", "Afficher les dernières mises à jour de l'application");
            French.Add("viewDBUpdates", "Afficher les dernières mises à jour de la base de données");
            French.Add("EnableColorChangeDefaultV2Text", "Activer le changement de couleurs");
            French.Add("EnableColorChangeDefaultV2CBDescription", "Activer le changement de couleurs lors de la sélection d'un mod ou d'une configuration");
            French.Add("EnableColorChangeLegacyText", "Activer le changement de couleurs");
            French.Add("EnableColorChangeLegacyCBDescription", "Activer le changement de couleurs lors de la sélection d'un mod ou d'une configuration");
            French.Add("ShowOptionsCollapsedLegacyText", "Afficher les options réduites au démarrage");
            French.Add("ShowOptionsCollapsedLegacyCBDescription", "Lorsque cette option est cochée, toutes les options de la liste de sélection (sauf au niveau de la catégorie) seront réduites.");
            French.Add("ClearLogFilesText", "Effacer les fichiers logs");
            French.Add("ClearLogFilesCBDescription", "Effacer les fichiers logs de WoT (python.log), ainsi que les fichiers logs de xvm (xvm.log) et les fichiers logs de pmod (pmod.log)");
            French.Add("CreateShortcutsText", "Créer des raccourcis sur le bureau");
            French.Add("CreateShortcutsCBDescription", "Lorsque cette option est cochée, l'installation créera des icônes de raccourci sur votre bureau pour les mods qui ont des" +
                " fichiers .exe (comme la configuration WWIIHA)");
            French.Add("DeleteOldPackagesText", "Supprimer les anciens packs de fichiers");
            French.Add("DeleteOldPackagesCBDescription", "Supprime tous les fichiers zip qui ne sont plus utilisés par l'installateur dans le dossier \"RelhaxDownloads\" pour libérer de la place sur le disque dur");
            French.Add("MinimalistModeText", "Mode minimaliste");
            French.Add("MinimalistModeCBDescription", "Si cette option est cochée, le modpack exclura l'installation de certains packs qui ne sont pas requis, comme le bouton du modpack ou les fichiers du thème Relhax.");
            French.Add("AutoInstallText", "Activer l'installation automatique");
            French.Add("AutoInstallCBDescription", "Quand un fichier de sélection et une heure sont définis ci-dessous, l'installateur va automatiquement chercher les mises à jour de vos mods et les appliquer");
            French.Add("OneClickInstallText", "Activer l'installation en un clic");
            French.Add("AutoOneclickShowWarningOnSelectionsFailText", "Afficher un avertissement si le fichier de sélection contient des erreurs lorsqu'il est chargé");
            French.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Lors de l'utilisation de l'installation automatique ou en un clic, affiche un avertissement "+
                "pour annuler l'installation si une erreur apparait lors de l'application du fichier de sélection");
            French.Add("ForceEnabledText", "Forcer l'activation de tous les packs [!]");
            French.Add("ForceEnabledCBDescription", "Provoque l'activation de tous les packs. Peut causer de lourds problèmes de stabilité de votre installation");
            French.Add("ForceVisibleText", "Forcer 10la visibilité de tous les packs [!]");
            French.Add("ForceVisibleCBDescription", "Provoque l'activation de la visibilité de tous les packs. Peut causer de lourds problèmes de stabilité de votre installation");
            French.Add("LoadAutoSyncSelectionFileText", "Charger le fichier de sélection");
            French.Add("LoadAutoSyncSelectionFileDescription", "Chargement de votre fichier de sélection de mods pour utiliser l'installation en un clic ainsi que l'installation automatique");
            French.Add("AutoSyncCheckFrequencyTextBox", "Fréquence: tous les");
            French.Add("DeveloperSettingsHeader", "Options pour les développeurs [!]");
            French.Add("DeveloperSettingsHeaderDescription", "Les options ci-dessous peuvent causer des problèmes de stabilité ! \nS'il vous plaît, ne les utilisez pas si vous ne savez pas ce que vous faites !");
            French.Add("ApplyCustomScalingText", "Mise à l'echelle de l'application");
            French.Add("ApplyCustomScalingTextDescription", "Appliquer la mise à l'échelle de l'affichage dans les fenêtres du programme d'installation");
            French.Add("EnableCustomFontCheckboxText", "Activer la police personnalisée");
            French.Add("EnableCustomFontCheckboxTextDescription", "Activer l'utilisation d'une police personnalisée installée sur votre système dans la plupart des fenêtres d'application");
            French.Add("LauchEditorText", "Lancer l'éditeur de la base de données");
            French.Add("LauchEditorDescription", "Lancer l'éditeur de la base de données d'ici, au lieu de le lancer depuis une ligne de commande");
            French.Add("LauchPatchDesignerText", "Lancer le Patch Designer");
            French.Add("LauchPatchDesignerDescription", "Lance le concepteur de patch ici, au lieu d’une ligne de commande");
            French.Add("LauchAutomationRunnerText", "Lancer l'exécuteur d'automatisation");
            French.Add("LauchAutomationRunnerDescription", "Lancer l’exécuteur d’automatisation ici, au lieu d’une ligne de commande");
            French.Add("InstallWhileDownloadingText", "Extraire pendant le téléchargement");
            French.Add("InstallWhileDownloadingCBDescription", "Si cette option est activée, l'installateur va extraire un fichier zip dès qu'il est téléchargé, au lieu" +
                " d'attendre que chaque fichier zip soit téléchargé pour l'extraction.");
            French.Add("MulticoreExtractionCoresCountLabel", "Coeurs détectés: {0}");
            French.Add("MulticoreExtractionCoresCountLabelDescription", "Nombre de coeurs logiques de processeur (threads) détectés sur votre système");
            French.Add("SaveDisabledModsInSelectionText", "Garder les mods désactivés pendant la sauvegarde de la sélection");
            French.Add("SaveDisabledModsInSelectionDescription", "Quand un mod est réactivé, il sera sélectionné depuis votre sélection de fichiers");
            French.Add("AdvancedInstallationProgressText", "Voir la fenêtre de progression de l'installation");
            French.Add("AdvancedInstallationProgressDescription", "Montre une fenêtre de progression de l'installation pendant l'extraction, utile quand vous avez l'extraction multicoeurs activée");
            French.Add("ThemeSelectText", "Sélectionner un thème");
            French.Add("ThemeDefaultText", "Standard");
            French.Add("ThemeDefaultDescription", "Thème standard");
            French.Add("ThemeDarkText", "Sombre");
            French.Add("ThemeDarkDescription", "Thème sombre");
            French.Add("ThemeCustom", "Personnalisé");
            French.Add("ThemeCustomDescription", "Thème personnalisé");
            French.Add("DumpColorSettingsButtonText", "Enregistrer les paramètres de couleurs actuels");
            French.Add("OpenColorPickerButtonText", "Ouvrir la pipette de couleurs");
            French.Add("OpenColorPickerButtonDescription", "Ouvrir la pipette de couleurs, vous autorisant à créer votre propre thème.");
            French.Add("DumpColorSettingsButtonDescription", "Ecrit un document xml de tous les composants qui peuvent avoir une couleur personnalisée, afin de faire un thème personnalisé");
            French.Add("MulticoreExtractionText", "Mode d'extraction multicoeurs");
            French.Add("MulticoreExtractionCBDescription", "Le programme d'installation utilise une méthode d'extraction parallèle. Il va extraire plusieurs fichiers" +
                " zip en même temps, réduisant ainsi le temps d'installation. Pour les disques SSD SEULEMENT.");
            French.Add("UninstallDefaultText", "Standard");
            French.Add("UninstallQuickText", "Rapide");
            French.Add("ExportModeText", "Mode d'exportation");
            French.Add("ExportModeCBDescription", "Le mode d'exportation vous permettra de sélectionner un dossier et la version de WoT vers lesquels vous souhaitez exporter votre installation de mods. Pour les utilisateurs avancés uniquement. "+
                "Notez que cela ne va PAS: décompresser les fichiers xml du jeu, les fichiers de correctifs (fournis par le jeu) ou créer les fichiers d'atlas. Les instructions se trouvent dans le répertoire d'exportation.");
            French.Add("ViewCreditsButtonText", "Voir les crédits");
            French.Add("ViewCreditsButtonDescription", "Voir toutes les personnes et les projets géniaux qui prennent en charge le modpack !");
            French.Add("ExportWindowDescription", "Sélection de la version de WoT que vous souhaitez exporter");
            French.Add("HelperText", "Bienvenue dans le Modpack Relhax !"+
                "\nJ'ai essayé de faire le modpack le plus simple possible, mais des questions peuvent toujours survenir. Survolez un paramètre pour voir une explication." +
                "\nMerci d'utiliser Relhax, j'espère que vous l'apprécierez ! - Willster419");
            French.Add("helperTextShort", "Bienvenue dans le Modpack Relhax !");
            French.Add("NotifyIfSameDatabaseText", "Informer si aucune nouvelle base de données n'est disponible (base de données stable uniquement)");
            French.Add("NotifyIfSameDatabaseCBDescription", "Vous informe si votre dernière version de base de données installée est identique. Si c'est le cas, cela signifie qu'il n'y a pas de mise à jour des mods. Cela ne marche qu'avec la base de données stable.");
            French.Add("ShowInstallCompleteWindowText", "Montrer la fenêtre d'installation terminée" );
            French.Add("ShowInstallCompleteWindowCBDescription", "Afficher une fenêtre lors de l'achèvement de l'installation avec des opérations populaires à" +
                " effectuer après l'installation du modpack, telles que le lancement du jeu, le site web de XVM, etc.");
            French.Add("applicationVersion", "Version de l'application");
            French.Add("databaseVersion", "Dernière base de données");
            French.Add("ClearCacheText", "Nettoyer le cache de WoT");
            French.Add("ClearCacheCBDescription", "Nettoyer le dossier de cache de WoT. Effectue la même tâche que l'option similaire qui était dans OMC.");
            French.Add("UninstallDefaultDescription", "La méthode de désinstallation par défaut va supprimer tous les fichiers dans le dossier du jeu, incluant les" +
                " raccourcies et le fichers de cache appdata");
            French.Add("UninstallQuickDescription", "La méthode de désinstallation rapide va uniquement supprimer les fichiers dans le dossier" +
                " \"mod\" du jeu. Ceci ne supprimera pas les raccourcis ou les fichiers de cache appdata créés par le modpack");
            French.Add("DiagnosticUtilitiesButton", "Utilitaires de diagnostic");
            French.Add("DiagnosticUtilitiesButtonDescription", "Signaler un bug, tenter une réparation du client WG, etc.");
            French.Add("UninstallModeGroupBox", "Mode de désinstallation:");
            French.Add("UninstallModeGroupBoxDescription", "Sélectionner le mode d'installation à utiliser");
            French.Add("FacebookButtonDescription", "Page Facebook");
            French.Add("DiscordButtonDescription", "Serveur Discord");
            French.Add("TwitterButtonDescription", "Page Twitter");
            French.Add("SendEmailButtonDescription", "Nous envoyer un E-mail (Pas de support du modpack)");
            French.Add("HomepageButtonDescription", "Visiter notre site web");
            French.Add("DonateButtonDescription", "Donation pour aider au développement");
            French.Add("FindBugAddModButtonDescription", "Tu as trouvé un bug ? Tu veux qu'un mod soit ajouté ? Signale le ici !");
            French.Add("SelectionViewGB", "Vue de sélection");
            French.Add("SelectionDefaultText", "Standard");
            French.Add("SelectionLayoutDescription", "Sélectionnez un style de liste pour la sélection\nNormal: liste de sélection Relhax\nLegacy: liste de vue arbre OMC");
            French.Add("SelectionDefaultDescription", "Sélectionnez un style de liste pour la sélection\nNormal: liste de sélection Relhax\nLegacy: liste de vue arbre OMC");
            French.Add("SelectionLegacyDescription", "Sélectionnez un style de liste pour la sélection\nNormal: liste de sélection Relhax\nLegacy: liste de vue arbre OMC");
            French.Add("LanguageSelectionGBDescription", "Sélectionnez votre langue préférée");
            French.Add("EnableBordersDefaultV2Text", "Activer les bordures");
            French.Add("EnableBordersLegacyText", "Activer les bordures");
            French.Add("EnableBordersDefaultV2CBDescription", "Activer les bordures noires autour de chaque mod et sous-niveau de configuration.");
            French.Add("EnableBordersLegacyCBDescription", "Activer les bordures noires autour de chaque mod et sous-niveau de configuration.");
            French.Add("UseBetaDatabaseText", "Utiliser la base de données beta");
            French.Add("UseBetaDatabaseCBDescription", "Utiliser la dernière base de données beta. La stabilité des mods n'est pas garantie");
            French.Add("UseBetaApplicationText", "Utiliser l'application beta");
            French.Add("UseBetaApplicationCBDescription", "Utiliser la dernière version beta de l'application. Les traductions et la stabilité de l'application ne sont pas garanties");
            French.Add("SettingsTabIntroHeader", "Bienvenue !");
            French.Add("SettingsTabSelectionViewHeader", "Vue de sélection");
            French.Add("SettingsTabInstallationSettingsHeader", "Options d'installation");
            French.Add("SettingsTabApplicationSettingsHeader", "Options de l'application");
            French.Add("SettingsTabAdvancedSettingsHeader", "Avancé");
            French.Add("MainWindowSelectSelectionFileToLoad", "Sélectionner le fichier de sélection à charger");
            French.Add("verifyUninstallHeader", "Confirmation");
            French.Add("verifyUninstallVersionAndLocation", "Confirmez que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nEn utilisant la méthode de désinstallation '{1}'?");
            French.Add("failedVerifyFolderStructure", "L'application n'a pas réussi à créer la structure de dossiers requise. Vérifiez vos autorisations de fichiers ou déplacez l'application vers un dossier avec accès en écriture");
            French.Add("failedToExtractUpdateArchive", "L'application n'a pas réussi à extraire les fichiers de mise à jour. Vérifiez vos autorisations de fichiers et votre antivirus.");
            French.Add("downloadingUpdate", "Téléchargement de la mise à jour");
            //(removed components, disabled components, etc.)
            French.Add("AutoOneclickSelectionErrorsContinueBody", "Des problèmes sont survenus lors du chargement du fichier de sélection (un package a probablement" +
                " été désactivé/supprimé, etc...). \nVoulez-vous tout de même continuer?");
            French.Add("AutoOneclickSelectionErrorsContinueHeader", "Problèmes lors du chargement du fichier de sélection");
            French.Add("noAutoInstallWithBeta", "L'installation automatique ne peut pas être utilisée avec la base de données beta");
            French.Add("autoInstallWithBetaDBConfirmBody", "L'installation automatique sera activée avec la base de données bêta. Cette base de données est mise à jour fréquemment" +
                " et peut provoquer plusieurs mises à jour le même jour. Êtes-vous sûr de vouloir faire ça ?");
            French.Add("autoInstallWithBetaDBConfirmHeader", French["verifyUninstallHeader"]);
            //"branch" is this context is git respoitory branches
            French.Add("loadingBranches", "Chargement des branches");
            //"branch" is this context is git respoitory branches
            French.Add("failedToParseUISettingsFile", "Échec de l'application du thème. Consultez le journal pour plus de détails. Activez la \"journalisation détaillée\" pour plus d'informations.");
            French.Add("UISettingsFileApplied", "Thème appliqué");
            French.Add("failedToFindWoTExe", "Impossible d'obtenir l'emplacement d'installation du client WoT. Veuillez envoyer un rapport de bug au développeur.");
            French.Add("failedToFindWoTVersionXml", "Impossible d'obtenir les informations sur la version d'installation du client WoT. Vérifiez si le fichier 'version.xml' existe dans le répertoire 'World_of_Tanks'.");
            #endregion

            #region ModSelectionList
            French.Add("ModSelectionList", "Liste de sélection");
            French.Add("ContinueButtonLabel", "Installer");
            French.Add("CancelButtonLabel", French["cancel"]);
            French.Add("HelpLabel", "Clic droit sur un composant de la sélection pour voir une fenêtre de prévisualisation");
            French.Add("LoadSelectionButtonLabel", "Charger une configuration");
            French.Add("SaveSelectionButtonLabel", "Sauvegarder une configuration");
            French.Add("SelectSelectionFileToSave", "Sauvegarder le fichier de sélection");
            French.Add("ClearSelectionsButtonLabel", "Réinitialiser la sélection");
            French.Add("SearchThisTabOnlyCB", "Rechercher dans cet onglet uniquement");
            French.Add("searchComboBoxInitMessage", "Rechercher un package...");
            French.Add("SearchTBDescription", "Vous pouvez également rechercher plusieurs parties de nom, séparées par un * (astérisque).\nPar exemple: config*willster419" +
                " affichera comme résultat de la recherche: Config de Willster419");
            French.Add("InstallingAsWoTVersion", "Installation en tant que version de WoT: {0}");
            French.Add("UsingDatabaseVersion", "Utilisation de la base de données : {0} ({1})");
            French.Add("userMods", "Mods utilisateur");
            French.Add("FirstTimeUserModsWarning", "Cet onglet sert à sélectionner les fichiers zip que vous placez dans le dossier \"RelhaxUserMods\". Ils doivent être des fichiers zip et doivent utiliser un dossier de répertoire racine du répertoire \"World_of_Tanks\"");
            French.Add("downloadingDatabase", "Téléchargement de la base de données");
            French.Add("readingDatabase", "Chargement de la base de données");
            French.Add("loadingUI", "Chargement de l'interface");
            French.Add("verifyingDownloadCache", "Vérification de l'intégrité de ");
            French.Add("InstallProgressTextBoxDescription", "La progression d'une installation sera affichée ici");
            French.Add("testModeDatabaseNotFound", "CRITIQUE: Impossible de trouver la base de données du mode de test situé à : \n{0}");
            French.Add("duplicateMods", "CRITIQUE: Duplication d'identifiant de package détectée");
            French.Add("databaseReadFailed", "CRITIQUE: Impossible de lire la base de données");
            French.Add("configSaveSuccess", "Sélection sauvergardée avec succès");
            French.Add("selectConfigFile", "Trouver un fichier de sélection à charger");
            French.Add("configLoadFailed", "Le fichier de sélection ne peut pas être chargé, chargement en mode standard");
            French.Add("modNotFound", "Le package (ID = \"{0}\") n'a pas été trouvé dans la base de données. Il peut avoir été renommé ou supprimé");
            French.Add("modDeactivated", "Le package (ID = \"{0}\") est actuellement désactivé dans le modpack et ne peut pas être sélectionné pour l'installation");
            French.Add("modsNotFoundTechnical", "Les packs suivants sont introuvables et ont probablement été supprimés");
            French.Add("modsBrokenStructure", "Les packs suivants ont été désactivés suite à une modification dans la structure du package. Vous devez la revérifier si vous voulez les installer");
            French.Add("packagesUpdatedShouldInstall", "Les packs suivants ont été mis à jour depuis le dernier chargement de ce fichier de sélection. Votre fichier de sélection a été mis à jour avec les modifications (une sauvegarde ponctuelle a également été effectuée)." +
                "S'il s'agit de votre installation actuelle et que vous souhaitez la conserver, il est recommandé d'installer / mettre à jour après avoir vu ce message.");
            French.Add("selectionFileIssuesTitle", "Sélection de messages de chargement");
            French.Add("selectionFormatOldV2", "Ce format de fichier de sélection est hérité (V2) et sera mis à niveau vers la V3. Une sauvegarde de la V2 sera créée.");
            French.Add("oldSavedConfigFile", "Le fichier de préférences que vous avez choisi est dans un format obsolète et sera inexact dans le futur. Convertir au nouveau format ?");
            French.Add("prefrencesSet", "Préférences enregistrées");
            French.Add("selectionsCleared", "Sélections effacées");
            French.Add("failedLoadSelection", "Echec du chargement de la sélection");
            French.Add("unknownselectionFileFormat", "Version du fichier sélectionné inconnue");
            French.Add("ExpandAllButton", "Elargir l'onglet actuel");
            French.Add("CollapseAllButton", "Réduire l'onglet actuel");
            French.Add("InstallingTo", "Installation à : {0}");
            French.Add("selectWhereToSave", "Sélectionnez où sauvegarder le fichier de sélection");
            French.Add("updated", "mis à jour");
            French.Add("disabled", "désactivé");
            French.Add("invisible", "invisible");
            French.Add("SelectionFileIssuesDisplay", "Erreurs lors de l'application du fichier de sélection");
            French.Add("selectionFileIssues", French["SelectionFileIssuesDisplay"]);
            French.Add("selectionFileIssuesHeader", "S’il vous plaît, lisez le message suivant au sujet de votre sélection de fichiers");
            French.Add("VersionInfo", "Mise à jour de l'application");
            French.Add("VersionInfoYesText", French["yes"]);
            French.Add("VersionInfoNoText", French["no"]);
            French.Add("NewVersionAvailable", "Nouvelle version disponible");
            French.Add("HavingProblemsTextBlock", "Si vous avez des problèmes avec la mise à jour, s'il vous plaît");
            French.Add("ManualUpdateLink", "Cliquez ici");
            French.Add("loadingApplicationUpdateNotes", "Chargement des notes de mise à jour de l'application...");
            French.Add("failedToLoadUpdateNotes", "Échec du chargement des notes de mise à jour de l'application");
            French.Add("ViewUpdateNotesOnGoogleTranslate", "Voir les notes de mise à jour sur Google Traduction");
            French.Add("VersionInfoAskText", "Voulez-vous faire la mise à jour maintenant ?");
            French.Add("SelectDownloadMirrorTextBlock", "Sélectionnez un miroir de téléchargement");
            French.Add("SelectDownloadMirrorTextBlockDescription", "Ce miroir va être utilisé uniquement pour le téléchargement de packs");
            French.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Dallas, USA");
            French.Add("downloadMirrorDe", "clanverwaltung.de, Francfort, Allemagne");
            #endregion

            #region Installer Messages
            French.Add("Downloading", "Téléchargement");
            French.Add("patching", "Correction");
            French.Add("done", "Terminé");
            French.Add("cleanUp", "Nettoyer les ressources");
            French.Add("idle", "Neutre");
            French.Add("status", "Statut :");
            French.Add("canceled", "Annulé");
            French.Add("appSingleInstance", "Vérification d'instance unique");
            French.Add("checkForUpdates", "Vérification de mises à jour");
            French.Add("verDirStructure", "Vérification de la structure du dossier");
            French.Add("loadingSettings", "Chargement des paramètres");
            French.Add("loadingTranslations", "Chargement des traductions");
            French.Add("loading", "Chargement");
            French.Add("of", "de");
            French.Add("failedToDownload1", "Echec du téléchargement du package");
            French.Add("failedToDownload2", "Voulez-vous essayer à nouveau le téléchargement, annuler l'installation, ou continuer ?");
            French.Add("failedToDownloadHeader", "Échec de téléchargement");
            French.Add("failedManager_version", "L'application beta actuelle est dépassée et doit être mise à jour. Pas de nouvelle version beta en ligne actuellement.");
            French.Add("fontsPromptInstallHeader", "Administrateur pour installer les polices ?");
            French.Add("fontsPromptInstallText", "Avez-vous les droits d'administrateur installer des polices ?");
            French.Add("fontsPromptError_1", "Impossible d'installer les polices. Certains mods peuvent mal fonctionner. Les polices sont situées dans ");
            French.Add("fontsPromptError_2", "\\_fonts. Installez les polices manuellement ou redémarrez avec les droits d'administrateur");
            French.Add("cantDownloadNewVersion", "Impossible de télécharger la nouvelle version, fermeture.");
            French.Add("failedCreateUpdateBat", "Impossible de créer le processus de mise à jour. Veuillez supprimer manuellement le fichier :\n{0}\n\nrenommer le fichier :\n{1}\nen :\n{2}\n\nAller directement au dossier ?");
            French.Add("cantStartNewApp", "Impossible de démarrer l'application, mais elle est située dans \n");
            French.Add("autoDetectFailed", "Échec de la détection automatique. Utilisez l'option 'Forcer détection manuel'");
            French.Add("anotherInstanceRunning", "Une autre instance de Relhax Manager est déjà en cours d`éxecution");
            French.Add("closeInstanceRunningForUpdate", "Merci de fermer toutes les instances de Relhax Manager avant que nous puissions procéder à la mise à jour");
            French.Add("skipUpdateWarning", "Vous ignorez la mise à jour. Compatibilité de la base de données non garantie");
            French.Add("patchDayMessage", "Le modpack est actuellement indisponible aujourd'hui pour tester et mettre à jour les mods. Désolé pour le dérangement." +
              " Si vous êtes un gestionnaire de base de données, ajoutez l'argument de commande");
            French.Add("configNotExist", "{0} n'existe pas, chargement en mode normal");
            French.Add("autoAndFirst", "Le premier lancement ne peut pas être un mode d'installation automatique, chargement en mode normal");
            French.Add("confirmUninstallHeader", "Confirmation");
            French.Add("confirmUninstallMessage", "Confirmez que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nEn utilisant la méthode de désinstallation '{1}'?");
            French.Add("uninstallingText", "Désinstallation...");
            French.Add("uninstallingFile", "Désinstallation du fichier");
            French.Add("uninstallFinished", "Désinstallation des mods terminée");
            French.Add("uninstallFail", "La désinstallation a échoué. Vous pouvez essayer un autre mode de désinstallation ou envoyer un signalement.");
            French.Add("extractionErrorMessage", "Erreur lors de la supression du dossier res_mods ou un/plusieurs mods. Soit World of Tanks est en" +
                " cours d`éxecution ou les permissions de sécurité sont incorrectes");
            French.Add("extractionErrorHeader", French["error"]);
            French.Add("deleteErrorHeader", "fermer les dossiers");
            French.Add("deleteErrorMessage", "Veuillez fermer les fenêtres res_mods ou mods (ou tous les sous-dossiers) et cliquer sur Ok pour continuer");
            French.Add("noUninstallLogMessage", "Le ficher log contenant la liste des fichiers installés (installedRelhaxFiles.log) n'existe pas. Voulez vous supprimer tous les mods ?");
            French.Add("noUninstallLogHeader", "Supprimer tous les mods");
            French.Add("moveOutOfTanksLocation", "Le modpack ne peut pas être éxecuté a partir du dossier de World of Tanks. Veuillez déplacer l`application" +
                " dans un autre dossier et réessayer");
            French.Add("moveAppOutOfDownloads", "L'application a détecté qu'elle est lancée à partir du dossier 'Téléchargements'. Ceci n'est pas recommandé car l'application crée plusieurs dossiers et fichiers " +
                "qui peuvent être difficiles à trouver dans un grand dossier 'Téléchargements'. Vous devriez déplacer l'application et tous les fichiers ainsi que dossiers 'Relhax' dans un nouveau dossier.");
            French.Add("DatabaseVersionsSameBody", "La base de données n'a pas changé depuis votre dernière installation. Par conséquent, il n'y a pas de mise à jour pour votre sélection" +
                " de mods. Continuer quand même ?");
            French.Add("DatabaseVersionsSameHeader", "La version de la base de données est identique");
            French.Add("databaseNotFound", "Base de données introuvable à l'URL fournie");
            French.Add("detectedClientVersion", "Version détectée du client");
            French.Add("supportedClientVersions", "Clients supportés");
            French.Add("supportNotGuarnteed", "Ce client n'est pas supporté officiellement. Les mods risquent de ne pas fonctionner.");
            French.Add("couldTryBeta", "Si un patch du jeu a récemment été déployé, l'équipe travaille afin de rendre le modpack compatible. Vous pouvez essayer d'utiliser la base de données beta.");
            French.Add("missingMSVCPLibrariesHeader", "Echec du chargement des bibliothèques requises");
            French.Add("missingMSVCPLibraries", "Échec du chargement des bibliothèques de traitement d'icône de contour. Cela peut indiquer qu'il vous manque un package Microsoft dll requis.");
            French.Add("openLinkToMSVCP", "Voulez-vous ouvrir votre navigateur à la page de téléchargement du package ?");
            French.Add("noChangeUntilRestart", "Cette option ne prendra effet qu'au redémarrage de l'application");
            French.Add("installBackupMods", "Sauvegarde du fichier mod");
            French.Add("installBackupData", "Sauvegarde des données utilisateur");
            French.Add("installClearCache", "Suppression du cache de WoT");
            French.Add("installClearLogs", "Suppression des fichiers de logs");
            French.Add("installCleanMods", "Nettoyage du dossier des mods");
            French.Add("installExtractingMods", "Extraction du package");
            French.Add("installZipFileEntry", "Entrée de fichier");
            French.Add("installExtractingCompletedThreads", "Fils d'extraction terminés");
            French.Add("installExtractingOfGroup", "du groupe d'installation");
            French.Add("extractingUserMod", "Extraction de package utilisateur");
            French.Add("installRestoreUserdata", "Restauration des données utilisateur");
            French.Add("installXmlUnpack", "Décompression du fichier XML");
            French.Add("installPatchFiles", "Correction du fichier");
            French.Add("installShortcuts", "Création de raccourcis");
            French.Add("installContourIconAtlas", "Création du fichier Atlas");
            French.Add("installFonts", "Installation des polices");
            French.Add("installCleanup", "Nettoyage");
            French.Add("AtlasExtraction", "Extraction du fichier Atlas");
            French.Add("copyingFile", "Copie du fichier");
            French.Add("deletingFile", "Supression du fichier");
            French.Add("scanningModsFolders", "Scan des dossiers mods...");
            French.Add("file", "Fichier");
            French.Add("size", "Taille");
            French.Add("checkDatabase", "Vérification de la base de données pour les fichiers périmés ou non nécessaires");
            French.Add("parseDownloadFolderFailed", "L'analyse du dossier \"{0}\" a échoué.");
            French.Add("installationFinished", "L'installation est terminée");
            French.Add("deletingFiles", "Suppression de fichiers");
            French.Add("uninstalling", "Désinstallation");
            French.Add("zipReadingErrorHeader", "Téléchargement incomplet");
            French.Add("zipReadingErrorMessage1", "Le fichier zip");
            French.Add("zipReadingErrorMessage3", "n'a pas pu être lu.");
            French.Add("patchingSystemDeneidAccessMessage", "Le système de correctifs s'est vu refuser l'accès au dossier des correctifs. Réessayez en tant qu'administrateur. Si vous voyez ceci" +
                " à nouveau, vous devez corriger les autorisations de sécurité de vos fichiers et dossiers");
            French.Add("patchingSystemDeneidAccessHeader", "Accès refusé");
            French.Add("folderDeleteFailed", "Échec de la suppression du dossier");
            French.Add("fileDeleteFailed", "Échec de la supression du fichier");
            French.Add("DeleteBackupFolder", "Sauvegardes");
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            French.Add("installFailed", "L'installation a échoué aux étapes suivantes");
            #endregion

            #region Install finished window
            French.Add("InstallFinished", "Installation terminée");
            French.Add("InstallationCompleteText", "L'installation est terminée. Voulez-vous...");
            French.Add("InstallationCompleteStartWoT", "Démarrer le jeu ? (WorldofTanks.exe)");
            French.Add("InstallationCompleteStartGameCenter", "Démarrer WG Game Center ?");
            French.Add("InstallationCompleteOpenXVM", "Ouvrir le site de xvm ?");
            French.Add("InstallationCompleteCloseThisWindow", "Fermer cette fenêtre ?");
            French.Add("InstallationCompleteCloseApp", "Fermer l'application ?");
            French.Add("xvmUrlLocalisation", "fr");
            French.Add("CouldNotStartProcess", "Impossible de démarrer le processus");
            #endregion

            #region Diagnostics
            French.Add("Diagnostics", "Diagnostics");
            French.Add("DiagnosticsMainTextBox", "Vous pouvez utiliser les options ci-dessous pour essayer de diagnostiquer ou résoudre les soucis que vous avez");
            French.Add("LaunchWoTLauncher", "Lancer le launcher de World of Tanks en mode vérification d'intégrité");
            French.Add("CollectLogInfo", "Recueillir les fichiers logs dans un fichier zip pour signaler un problème");
            French.Add("CollectLogInfoButtonDescription", "Recueille tous les fichiers logs nécessaires dans un seul fichier zip. \nCela vous permet de signaler un problème plus facilement.");
            French.Add("DownloadWGPatchFilesText", "Télécharger les fichiers de patch WG pour n'importe quel client WG via HTTP");
            French.Add("DownloadWGPatchFilesButtonDescription", "Vous guide et télécharge les fichiers de correctifs pour les jeux Wargaming (WoT, WoWs, WoWp) sur HTTP afin que vous puissiez les installer plus tard. \n" +
                "Particulièrement utile pour les personnes qui ne peuvent pas utiliser le protocole P2P par défaut de Wargaming Game Center.");
            French.Add("SelectedInstallation", "Installation actuellement sélectionnée :");
            French.Add("SelectedInstallationNone", "(" + French["none"].ToLower() + ")");
            French.Add("collectionLogInfo", "Collection des fichiers logs...");
            French.Add("startingLauncherRepairMode", "Lancement de WoTLauncher en mode de vérification d'intégrité...");
            French.Add("failedCreateZipfile", "Erreur lors de la création du fichier zip ");
            French.Add("launcherRepairModeStarted", "Mode de réparation démarré avec succès");
            French.Add("ClearDownloadCache", "Effacer le cache de téléchargement");
            French.Add("ClearDownloadCacheDatabase", "Supprimer le fichier de base de données de cache de téléchargement");
            French.Add("ClearDownloadCacheDescription", "Supprimer tous les fichiers dans le répertoire \"RelhaxDownloads\"");
            French.Add("ClearDownloadCacheDatabaseDescription", "Supprimer le fichier de base de données XML. L’intégrité de tous les fichiers zip sera à nouveau vérifiée. \nTous les fichiers non valides seront re-téléchargés s’ils sont sélectionnés lors de votre prochaine installation.");
            French.Add("clearingDownloadCache", "Suppression du cache de téléchargement");
            French.Add("failedToClearDownloadCache", "Echec du nettoyage du cache de téléchargement");
            French.Add("cleaningDownloadCacheComplete", "Nettoyage du cache de téléchargement terminé");
            French.Add("clearingDownloadCacheDatabase", "Suppression du fichier de cache de base de données XML");
            French.Add("failedToClearDownloadCacheDatabase", "Echec du nettoyage du cache de téléchargement de la base de données terminé");
            French.Add("cleaningDownloadCacheDatabaseComplete", "Fichier de cache de la base de données XML supprimé");
            French.Add("ChangeInstall", "Modifier l'installation de WoT actuellement sélectionnée");
            French.Add("ChangeInstallDescription", "Cela changera les fichiers logs qui seront ajoutés au fichier zip de diagnostic");
            French.Add("zipSavedTo", "Fichier zip sauvegardé à : ");
            French.Add("selectFilesToInclude", "Sélectionner les fichiers à inclure dans le signalement du bug");
            French.Add("TestLoadImageLibraries", "Test de chargement des bibliothèques de traitement d'images atlas");
            French.Add("TestLoadImageLibrariesButtonDescription", "Teste les bibliothèques de traitement d'images d'atlas");
            French.Add("loadingAtlasImageLibraries", "Chargement des bibliothèques de traitement d'images atlas");
            French.Add("loadingAtlasImageLibrariesSuccess", "Chargement des bibliothèques de traitement d'images atlas réussi");
            French.Add("loadingAtlasImageLibrariesFail", "Echec du chargement des bibliothèques de traitement d'images atlas");
            French.Add("CleanupModFilesText", "Nettoyer les fichiers de mods placés à des endroits incorrects");
            French.Add("CleanupModFilesButtonDescription", "Supprimer tous les mods se situant dans des dossiers comme win32 et win64 qui pourraient causer des confits de chargement");
            French.Add("cleanupModFilesCompleted", "Nettoyage des fichiers de mods terminé");
            French.Add("CleanGameCacheText", "Effacer les fichiers de cache du jeu");
            French.Add("cleanGameCacheProgress", "Effacement des fichiers de cache du jeu");
            French.Add("cleanGameCacheSuccess", "Les fichiers de cache du jeu ont été effacés avec succès");
            French.Add("cleanGameCacheFail", "Echec de la suppression des fichiers de cache du jeu");
            French.Add("TrimRelhaxLogfileText", "Couper le fichier log Relhax aux 3 derniers lancements");
            French.Add("trimRelhaxLogProgress", "Découpage du fichier log Relhax");
            French.Add("trimRelhaxLogSuccess", "Découpe du fichier log Relhax avec succès");
            French.Add("trimRelhaxLogFail", "Échec de la découpe du fichier journal Relhax");
            #endregion

            #region Wot Client install selection
            French.Add("WoTClientSelection", "Sélection du client WoT");
            French.Add("ClientSelectionsTextHeader", "Les installations client suivantes ont été automatiquement détectées");
            French.Add("ClientSelectionsCancelButton", French["cancel"]);
            French.Add("ClientSelectionsManualFind", "Sélection manuelle");
            French.Add("ClientSelectionsContinueButton", French["select"]);
            French.Add("AddPicturesZip", "Ajouter des fichiers au zip");
            French.Add("DiagnosticsAddSelectionsPicturesLabel", "Ajouter n'importe quel fichier additionnel ici (votre fichier de sélection, photo, etc.)");
            French.Add("DiagnosticsAddFilesButton", "Ajouter des fichiers");
            French.Add("DiagnosticsRemoveSelectedButton", "Enlever la sélection");
            French.Add("DiagnosticsContinueButton", French["ContinueButton"]);
            French.Add("cantRemoveDefaultFile", "Impossible de supprimer un fichier à ajouter par défaut.");
            #endregion

            #region Preview Window
            French.Add("Preview", "Aperçu");
            French.Add("noDescription", "Pas de description fournie");
            French.Add("noUpdateInfo", "Aucune information de mise à jour fournie");
            French.Add("noTimestamp", "Pas d'horodatage fourni");
            French.Add("PreviewNextPicButton", French["next"]);
            French.Add("PreviewPreviousPicButton", French["previous"]);
            French.Add("DevUrlHeader", "Liens développeur");
            French.Add("dropDownItemsInside", "Eléments à l'intérieur");
            French.Add("popular", "Populaire");
            French.Add("previewEncounteredError", "Prévisualisation des erreurs rencontrées");
            French.Add("popularInDescription", "C'est un package populaire");
            French.Add("controversialInDescription", "C'est un package controversé");
            French.Add("encryptedInDescription", "C'est un package chiffré, qui ne peut pas être soumis à une vérification antivirus");
            French.Add("fromWgmodsInDescription", "La source de cette archive est disponible sur le site WGMods Portal (wgmods.net)");
            #endregion

            #region Developer Selection Window
            French.Add("DeveloperSelectionsViewer", "Visionneuse de sélections");
            French.Add("DeveloperSelectionsTextHeader", "Sélection à charger");
            French.Add("DeveloperSelectionsCancelButton", French["cancel"]);
            French.Add("DeveloperSelectionsLocalFile", "Fichier local");
            French.Add("DeveloperSelectionsContinueButton", "Selectionner");
            French.Add("failedToParseSelections", "Échec de l'analyse des sélections");
            French.Add("lastModified", "Dernière modification");
            #endregion

            #region Advanced Installer Window
            French.Add("AdvancedProgress", "Progression avancée du programme d'installation");
            French.Add("PreInstallTabHeader", "Tâches de pré-installation");
            French.Add("ExtractionTabHeader", "Extraction");
            French.Add("PostInstallTabHeader", "Tâches de post-insallation");
            French.Add("AdvancedInstallBackupData", "Sauvegarder les données du mod");
            French.Add("AdvancedInstallClearCache", "Effacer le cache de WoT");
            French.Add("AdvancedInstallClearLogs", "Effacer les fichiers logs");
            French.Add("AdvancedInstallClearMods", "Désinstaller l'installation précédente");
            French.Add("AdvancedInstallInstallMods", "Fil d'installation");
            French.Add("AdvancedInstallUserInstallMods", "Installation utilisateur");
            French.Add("AdvancedInstallRestoreData", "Restaurer les données");
            French.Add("AdvancedInstallXmlUnpack", "Décompresseur XML");
            French.Add("AdvancedInstallPatchFiles", "Fichiers de correctifs");
            French.Add("AdvancedInstallCreateShortcuts", "Créer des raccourcis");
            French.Add("AdvancedInstallCreateAtlas", "Créer des atlas");
            French.Add("AdvancedInstallInstallFonts", "Installer des polices");
            French.Add("AdvancedInstallTrimDownloadCache", "Réduire le cache de téléchargement");
            French.Add("AdvancedInstallCleanup", "Nettoyer");
            #endregion

            #region News Viewer
            French.Add("NewsViewer", "Visionneuse de nouvelles");
            French.Add("application_Update_TabHeader", "Nouvelles de l'application");
            French.Add("database_Update_TabHeader", "Nouvelles de la base de données");
            French.Add("ViewNewsOnGoogleTranslate", "Voir des nouvelles sur Google Traduction");
            #endregion

            #region Loading Window
            French.Add("ProgressIndicator", "Chargement");
            French.Add("LoadingHeader", "Chargement, veuillez patienter");
            #endregion

            #region First Load Acknowledgements
            French.Add("FirstLoadAcknowledgments", "Remerciement de première charge");
            French.Add("AgreementLicense", "J'ai lu et accepté le ");
            French.Add("LicenseLink", "Accord de licence");
            French.Add("AgreementSupport1", "Je comprends que je peux recevoir de l’aide sur les ");
            French.Add("AgreementSupportDiscord", "Discord");
            French.Add("AgreementHoster", "Je comprends que Relhax est un hébergement de mods et un service d'installation. Relhax ne gère pas tous les mods de ce Modpack");
            French.Add("AgreementAnonData", "Je comprends que Relhax V2 collecte des données d'utilisation anonymes pour améliorer l'application, et peut être désactivée dans les options avancées");
            French.Add("V2UpgradeNoticeText", "Il semble que vous exécutiez une mise à niveau de V1 à V2 pour la première fois.\n" +
                "Appuyer sur Continuer entraînera une mise à niveau de la structure de fichiers qui ne peut pas être annulée. Il est recommandé de faire une sauvegarde de votre dossier V1 avant de continuer.");
            French.Add("upgradingStructure", "Mise à niveau de la structure des fichiers et des dossiers V1");
            #endregion

            #region Export Mode
            French.Add("ExportModeSelect", "Sélectionner la version du client de WoT pour exporter");
            French.Add("selectLocationToExport", "Sélectionner le dossier où exporter l'installation de mods");
            French.Add("ExportSelectVersionHeader", "Merci de sélectionner la version de votre client WoT pour laquelle vous voulez exporter");
            French.Add("ExportContinueButton", French["ContinueButton"]);
            French.Add("ExportCancelButton", French["cancel"]);
            French.Add("ExportModeMajorVersion", "Version du dossier en ligne");
            French.Add("ExportModeMinorVersion", "Version de WoT");
            #endregion

            #region Asking to close WoT
            French.Add("AskCloseWoT", "WoT est en cours d`éxecution");
            French.Add("WoTRunningTitle", "WoT est en cours d`éxecution");
            French.Add("WoTRunningHeader", "Il semblerait que vous installation de WoT soit actuellement ouverte. Merci de la fermer avant que nous puissions procéder.");
            French.Add("WoTRunningCancelInstallButton", "Annuler l'installation");
            French.Add("WoTRunningRetryButton", "Détecter à nouveau");
            French.Add("WoTRunningForceCloseButton", "Forcer la fermeture du jeu");
            #endregion

            #region Scaling Confirmation
            French.Add("ScalingConfirmation", "Confirmation de mise à l'échelle");
            French.Add("ScalingConfirmationHeader", "La valeur d'échelle a changé. Voulez-vous la garder ?");
            French.Add("ScalingConfirmationRevertTime", "Retour dans {0} seconde(s)");
            French.Add("ScalingConfirmationKeep", "Garder");
            French.Add("ScalingConfirmationDiscard", "Jeter");
            #endregion

            #region Game Center download utility
            French.Add("GameCenterUpdateDownloader", "Téléchargeur de mises à jour du Game Center");
            French.Add("GcDownloadStep1Header", "Sélectionner le client du jeu");
            French.Add("GcDownloadStep1TabDescription", "Sélectionner le client Wargaming pour collecter les informations pour (WoT, Wows, WoWp)");
            French.Add("GcDownloadStep1SelectClientButton", "Sélectionner un client");
            French.Add("GcDownloadStep1CurrentlySelectedClient", "Client actuellement sélectionné : {0}");
            French.Add("GcDownloadStep1NextText", French["next"]);
            French.Add("GcDownloadStep1GameCenterCheckbox", "Chercher plutôt des mises à jour du Game Center");
            French.Add("GcDownloadSelectWgClient", "Sélectionner le client WG");
            French.Add("ClientTypeValue", "Aucun");
            French.Add("LangValue", French["ClientTypeValue"]);
            French.Add("GcMissingFiles", "Il manque à votre client les fichiers de définition XML suivants");
            French.Add("GcDownloadStep2Header", "Fermer le Game Center");
            French.Add("GcDownloadStep2TabDescription", "Fermer le Game Center (l'application va détecter la fermeture");
            French.Add("GcDownloadStep2GcStatus", "Le Game Center est {0}");
            French.Add("GcDownloadStep2GcStatusOpened", "ouvert");
            French.Add("GcDownloadStep2GcStatusClosed", "fermé");
            French.Add("GcDownloadStep2PreviousText", French["previous"]);
            French.Add("GcDownloadStep2NextText", French["next"]);
            French.Add("GcDownloadStep3Header", "Obtenir des informations de mises à jour");
            French.Add("GcDownloadStep3TabDescription", "Obtention de la liste des fichiers de correctifs à télécharger");
            French.Add("GcDownloadStep3NoFilesUpToDate", "Aucun fichier de correctifs à télécharger (à jour)");
            French.Add("GcDownloadStep3PreviousText", French["previous"]);
            French.Add("GcDownloadStep3NextText", French["next"]);
            French.Add("GcDownloadStep4Header", "Télécharger les fichiers de mises à jour");
            French.Add("GcDownloadStep4TabDescription", "Téléchargement des fichiers de correctifs");
            French.Add("GcDownloadStep4DownloadingCancelButton", French["cancel"]);
            French.Add("GcDownloadStep4DownloadingText", "Téléchargement de correctif {0} de {1}: {2}");
            French.Add("GcDownloadStep4DownloadComplete", "Téléchargement des packages terminé !");
            French.Add("GcDownloadStep4PreviousText", French["previous"]);
            French.Add("GcDownloadStep4NextText", French["next"]);
            French.Add("GcDownloadStep5Header", "Terminé !");
            French.Add("GcDownloadStep5TabDescription", "Le processus est terminé. Le Game Center devrait détecter les fichiers quand il sera ouvert.");
            French.Add("GcDownloadStep5CloseText", French["close"]);
            French.Add("FirstLoadSelectLanguage", "Sélection de la langue");
            French.Add("SelectLanguageHeader", "S'il vous plaît, sélectionnez votre langue");
            French.Add("SelectLanguagesContinueButton", French["ContinueButton"]);
            French.Add("Credits", "Crédits Relhax Modpack");
            French.Add("creditsProjectLeader", "Directeur de projet");
            French.Add("creditsDatabaseManagers", "Gérants de la base de données");
            French.Add("creditsTranslators", "Traducteurs");
            French.Add("creditsusingOpenSourceProjs", "Relhax Modpack utilise les projets Open Source suivants");
            French.Add("creditsSpecialThanks", "Remerciement spécial");
            French.Add("creditsGrumpelumpf", "Chef de projet OMC, nous a permis de reprendre Relhax là où il s'était arrêté");
            French.Add("creditsRkk1945", "Le premier beta-testeur qui a travaillé avec moi pendant des mois pour lancer le projet");
            French.Add("creditsRgc", "Parrainer le modpack et être mon premier groupe de testeurs beta");
            French.Add("creditsBetaTestersName", "Notre équipe de beta-testeurs");
            French.Add("creditsBetaTesters", "Continuer à tester et signaler les problèmes dans l'application avant sa mise en ligne");
            French.Add("creditsSilvers", "Aide à la pub sur les réseaux sociaux et à la communauté");
            French.Add("creditsXantier", "Assistance informatique initiale et mise en place de notre serveur");
            French.Add("creditsSpritePacker", "Développement de l'algorithme Sprite Sheet Packer et de son portage vers .NET");
            French.Add("creditsWargaming", "Créer un système de modding facile à automatiser");
            French.Add("creditsUsersLikeU", "Des utilisateurs comme vous");
            #endregion

            #region Conflicting Packages Dialog
            French.Add("ConflictingPackageDialog", "Boîte de dialogue des packages en conflit");
            French.Add("conflictingPackageMessageOptionA", "Option A");
            French.Add("conflictingPackageMessageOptionB", "Option B");
            French.Add("conflictingPackageMessagePartA", "Vous avez selectionné des packages \"{0}\", mais cela rentre en conflit avec les package(s) suivants:");
            French.Add("conflictingPackageMessagePartB", French["conflictingPackageMessageOptionA"] + ":  sélectionnez \"{0}\", pour décocher tous les  package(s) en conflits");
            French.Add("conflictingPackageMessagePartC", French["conflictingPackageMessageOptionB"] + ": Ne sélectionnez pas\"{0}\", Qui continuera à créer des conflits de package(s)");
            French.Add("conflictingPackageMessagePartD", "Fermer la fenêtre sélectionnera l'option B");
            French.Add("conflictingPackageMessagePartE", "Veuillez sélectionner une option s'il vous plaît");
            #endregion

            #region End of Life announcement
            French.Add("EndOfLife", "Relhax End of Life");
            French.Add("CloseWindowButton", French["close"]);
            French.Add("WoTForumAnnouncementsTextBlock", "Poste d’annonce sur le forum WoT:");
            French.Add("endOfLifeMessagePart1", "Le 20 Avril 2022, Le Modpack Relhax a été mis hors-service. Je veux remercier tout les contributeurs et utilisateurs pour ses 5+ années de succès !");
            French.Add("endOfLifeMessagePart2a", "Le premier Janvier 2017, je me suis fixé un challenge : Ne pas uniquement recréer le modpack OMC avec une interface utilisateur plus moderne mais aussi pour créer le système d’installation de package la plus rapide de tous les modpacks qui ont existés");
            French.Add("endOfLifeMessagePart2b", "J’ai commencer avec une équipe de 4, en prenant 3 membres d’OMC qui voulaient contribuer au projet. Durant la course des 4 dernières années, j’ai designé, construit et re-construit une application de modpack depuis Scratch, en y passant des dizaines de milliers d’heures");
            French.Add("endOfLifeMessagePart2c", "A un moment, l’équipe est arrivée à 8 membres, de différents serveurs de WoT. Pendant le processus, j’ai évolué en tant que programmeur,j’ai aussi appris les pratiques logicielles courantes et je me suis spécialisé dans le multithreading des applications et la gestion des opérations simultanées.");
            French.Add("endOfLifeMessagePart2d", "J’ai gagné de l’expérience à travers ce projet, j’ai interagit avec une excellente communauté de moddeur. Cela m’a permis de contribuer à nouveau à la Relic Gaming Community que j’ai rejoins en 2014.");
            French.Add("endOfLifeMessagePart3a", "Depuis cette année, j’ai enfin terminé mon travail de design sur le plus optimisé et le plus efficace des installateurs que j’aurai pu créer pour cette communauté.");
            French.Add("endOfLifeMessagePart3b", "Voir le projet atteindre mon but originel, and voir mes intérêts sur le jeu (et sur le projet) diminuer, j’ai décider de stopper ce projet.");
            French.Add("endOfLifeMessagePart3c", "C’était une décision très difficile à prendre, mais je ne veux pas continuer à supporter un projet pour lequel je n’éprouve plus aucun intérêt");
            French.Add("endOfLifeMessagePart3d", "I think it would have reflected poorly on the quality of product, and it would not be fair to end users. I wanted to close the project while it was still in a healthy state.");
            French.Add("endOfLifeMessagePart4", "Encore une fois, merci à vous tous. C’était 5+ années bien amusantes, et cela me manquera.");
            #endregion
        }
    }
}
