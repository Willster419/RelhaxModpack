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
        /// Loads all Spanish translation dictionaries. Should only be done once (at application start)
        /// </summary>
        private static void LoadTranslationsSpanish()
        {
            #region General expressions
            Spanish.Add("yes", "Sí");
            Spanish.Add("no", "No");
            Spanish.Add("cancel", "Cancelar");
            Spanish.Add("delete", "Eliminar");
            Spanish.Add("warning", "ATENCIÓN");
            Spanish.Add("critical", "CRÍTICO");

            Spanish.Add("information", "Información");
            Spanish.Add("select", "Seleccionar");
            Spanish.Add("abort", "Abortar");
            Spanish.Add("error", "Error");
            Spanish.Add("retry", "Reintentar");
            Spanish.Add("ignore", "Ignorar");
            Spanish.Add("lastUpdated", "Última actualización: ");
            Spanish.Add("stepsComplete", "Operaciones completadas");
            Spanish.Add("allFiles", "Todos los archivos");
            Spanish.Add("GoogleTranslateLanguageKey", "es");
            Spanish.Add("at", "en");

            Spanish.Add("seconds", "segundo(s)");
            Spanish.Add("minutes", "minuto(s)");
            Spanish.Add("hours", "hora(s)");
            Spanish.Add("days", "día(s)");
            Spanish.Add("next", "Siguiente");
            Spanish.Add("ContinueButton", "Continuar");
            Spanish.Add("previous", "Anterior");
            Spanish.Add("close", "Cerrar");
            Spanish.Add("none", "Ninguna");
            #endregion

            #region Application messages
            Spanish.Add("appFailedCreateLogfile", "La aplicación no ha podido abrir un archivo de registro. Compruebe sus permisos de archivo o mueva la aplicación a una carpeta con permisos de escritura");
            Spanish.Add("failedToParse", "No se ha podido analizar el archivo");
            Spanish.Add("failedToGetDotNetFrameworkVersion", "No se ha podido obtener la versión de la instalación de .Net Framework. Esto puede indicar un problema de permisos, o un antivirus puede estar bloqueando la obtención.");
            Spanish.Add("invalidDotNetFrameworkVersion", "La versión instalada de .NET Framework es anterior a 4.8. Relhax Modpack requiere la versión 4.8 o superior para funcionar. " +
                " ¿Quiere abrir un vínculo para obtener la última versión de .NET Framework?");
            #endregion

            #region Tray Icon
            Spanish.Add("MenuItemRestore", "Restaurar");
            Spanish.Add("MenuItemCheckUpdates", "Comprobar actualizaciones");
            Spanish.Add("MenuItemAppClose", Spanish["close"]);
            Spanish.Add("newDBApplied", "Aplicada nueva versión de la base de datos");
            #endregion

            #region Main Window
            Spanish.Add("InstallModpackButton", "Comenzar selección de Mods");
            Spanish.Add("selectWOTExecutable", "Seleccione su ejecutable de WoT (WorldOfTanks.exe)");
            Spanish.Add("InstallModpackButtonDescription", "Seleccione los Mods que quiere instalar a su cliente de WoT");
            Spanish.Add("UninstallModpackButton", "Desinstalar Relhax Modpack");
            Spanish.Add("UninstallModpackButtonDescription", "Elimina *todos* los Mods installados en su cliente de WoT");
            Spanish.Add("ViewNewsButton", "Ver noticias de actualizaciones");
            Spanish.Add("ViewNewsButtonDescription", "Ver noticias sobre actualizaciones de la aplicación, base de datos, y otros");
            Spanish.Add("ForceManuelGameDetectionText", "Forzar detección manual del cliente");
            Spanish.Add("ForceManuelGameDetectionCBDescription", "Esta opción es utilizada para forzar una detección manual de la" +
                    "ruta de instalación de World of Tanks. Marque esta casilla si tiene problemas encontrando el juego automáticamente.");
            Spanish.Add("LanguageSelectionTextblock", "Selección de idioma");
            Spanish.Add("LanguageSelectionTextblockDescription", "Seleccione su idioma de preferencia.\nSi encuentra alguna traducción faltante o error de traducción, infórmenos sobre ella.");
            Spanish.Add("Forms_ENG_NAButtonDescription", "Acceder a la página en inglés del foro de 'World of Tanks' del servidor de NA");
            Spanish.Add("Forms_ENG_EUButtonDescription", "Acceder a la página en inglés del foro de 'World of Tanks' del servidor de EU");
            Spanish.Add("Forms_GER_EUButtonDescription", "Acceder a la página en alemán del foro de 'World of Tanks' del servidor de EU");
            Spanish.Add("SaveUserDataText", "Guardar datos del usuario");
            Spanish.Add("SaveUserDataCBDescription", "Si esta opción está seleccionada, el instalador guardará datos creados por el usuario" +
                " (como estadísticas de sesión de batallas anteriores)");
            Spanish.Add("CleanInstallText", "Instalación limpia (recomendado)");
            Spanish.Add("CleanInstallCBDescription", "Esta opción recomendada desinstalará instalaciones anteriores antes de instalar la nueva");
            Spanish.Add("BackupModsText", "Crear una copia de seguridad de la carpeta actual de Mods");
            Spanish.Add("BackupModsSizeLabelUsed", "Copias de seguridad: {0}  Tamaño: {1}");
            Spanish.Add("backupModsSizeCalculating", "Calculando el tamaño de las copias de seguridad...");
            Spanish.Add("BackupModsCBDescription", "Seleccione esta opción para crear una copia de seguridad de los Mods actualmente instalados. " +
                    "Será almacenada en la carpeta 'RelHaxModBackup' como archivo zip, nombrado por una timestamp.");
            Spanish.Add("BackupModsSizeLabelUsedDescription", Spanish["BackupModsCBDescription"]);
            Spanish.Add("SaveLastInstallText", "Guardar selección de la instalación anterior");
            Spanish.Add("SaveLastInstallCBDescription", "Si está activada, el instalador aplicará automáticamente su última selección utilizada");
            Spanish.Add("MinimizeToSystemTrayText", "Minimizar a la bandeja del sistema");
            Spanish.Add("MinimizeToSystemTrayDescription", "Si está activada, la aplicación continuará funcionando en la bandeja del sistema al hacer clic en el botón de cerrar");
            Spanish.Add("VerboseLoggingText", "Registro detallado");
            Spanish.Add("VerboseLoggingCBDescription", "Activa más mensajes en el archivo de registro. Útil para reportar bugs");
            Spanish.Add("AllowStatsGatherText", "Permitir recoleccón de estadísticas sobre el uso de Mods");
            Spanish.Add("AllowStatsGatherCBDescription", "Permite al instalador subir datos estadísticos anónimos al servidor sobre Mods seleccionados. Esto nos permite priorizar el soporte");
            Spanish.Add("DisableTriggersText", "Desactivar Desencadenantes");
            Spanish.Add("DisableTriggersCBDescription", "Permitir los Desencadenantes puede acelerar la instalación al ejecutar algunas tareas (como crear los iconos de contorno) durante la extracción " +
                "después de que todos los recursos para la operación estén disponibles. Se desactiva automáticamente si se detectan Mods del Usuario");
            Spanish.Add("appDataFolderNotExistHeader", "No se ha detectado la carpeta de caché de WoT");
            Spanish.Add("CancelDownloadInstallButton", Spanish["cancel"]);
            Spanish.Add("appDataFolderNotExist", "El instalador no ha podido detectar la carpeta de caché de WoT. ¿Continuar la instalación sin limpiar la caché?");
            Spanish.Add("viewAppUpdates", "Ver las últimas actualicaciones de la aplicación");
            Spanish.Add("viewDBUpdates", "Ver las últimas actualizaciones de la base de datos");
            Spanish.Add("EnableColorChangeDefaultV2Text", "Activar cambio de color");
            Spanish.Add("EnableColorChangeDefaultV2CBDescription", "Activa el cambio de color al des/seleccionar un Mod o configuración");
            Spanish.Add("EnableColorChangeLegacyText", "Activar cambio de color");
            Spanish.Add("EnableColorChangeLegacyCBDescription", "Activa el cambio de color al des/seleccionar un Mod o configuración");
            Spanish.Add("ShowOptionsCollapsedLegacyText", "Colapsar todas las opciones al iniciar");
            Spanish.Add("ShowOptionsCollapsedLegacyCBDescription", "Cuando está marcada, todas las opciones de la lista de selección (excepto las de nivel categoría) serán colapsadas");
            Spanish.Add("ClearLogFilesText", "Limpiar archivos de registro");
            Spanish.Add("ClearLogFilesCBDescription", "Limpia los archivos de registro del WoT (python.log), XVM (xvm.log), y PMOD (pmod.log)");
            Spanish.Add("CreateShortcutsText", "Crear accesos directos en el escritorio");
            Spanish.Add("CreateShortcutsCBDescription", "Si está seleccionado, creará accesos directos en el escritorio para los mods que sean archivos .exe (p. ej. cofiguración de WWIIHA)");
            Spanish.Add("DeleteOldPackagesText", "Eliminar paquetes de archivos antiguos");
            Spanish.Add("DeleteOldPackagesCBDescription", "Elimina los archivos zip que ya no vayan a ser utilizados por el instalador en la carpeta \"RelHaxDownloads\" para liberar espacio en disco");
            Spanish.Add("MinimalistModeText", "Modo minimalista");
            Spanish.Add("MinimalistModeCBDescription", "Cuando esté habilitado, el modpack excluirá ciertos paquetes no requeridos de la instalación, como el botón del modpack o los archivos del tema visual Relhax");
            Spanish.Add("AutoInstallText", "Habilitar instalación automática");
            Spanish.Add("AutoInstallCBDescription", "Cuando se establece un archivo de selección y fecha abajo, el instalador buscará automáticamente actualizaciones a los Mods instalados y las aplicará");
            Spanish.Add("OneClickInstallText", "Habilitar instalación en un clic");
            Spanish.Add("AutoOneclickShowWarningOnSelectionsFailText", "Mostrar una advertencia si el documento de selección tiene errores al cargar");
            Spanish.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Cuando se utiliza instalación automática o en un clic, mostrar una advertencia para cancelar si ocurre algún error al aplicar el archivo de selección");
            Spanish.Add("ForceEnabledText", "Forzar habilitación de todos los paquetes [!]");
            Spanish.Add("ForceEnabledCBDescription", "Fuerza la habilitación de todos los paquetes. Puede causar problemas de inestabilidad severa de la instalación");
            Spanish.Add("ForceVisibleText", "Forzar visibilidad de todos los paquetes [!]");
            Spanish.Add("ForceVisibleCBDescription", "Fuerza a todos los paquetes a ser visibles. Puede causar problemas de inestabilidad severa de la instalación");
            Spanish.Add("LoadAutoSyncSelectionFileText", "Cargar archivo de selección");
            Spanish.Add("LoadAutoSyncSelectionFileDescription", "Carga el archivo de mods seleccionados para usar las funciones de instalación automática o en un clic.");
            Spanish.Add("AutoSyncCheckFrequencyTextBox", "Frecuencia: cada");
            Spanish.Add("DeveloperSettingsHeader", "Opciones de desarrollador [!]");
            Spanish.Add("DeveloperSettingsHeaderDescription", "¡Las opciones a continuación pueden causar problemas o inestabilidad!.\n¡Por favor, no las utilice a menos que sepa lo que está haciendo!");
            Spanish.Add("ApplyCustomScalingText", "Escalado de la aplicación");
            Spanish.Add("ApplyCustomScalingTextDescription", "Aplica esta escala de visualización a las ventanas del instalador");
            Spanish.Add("EnableCustomFontCheckboxText", "Habilitar fuente personalizada");
            Spanish.Add("EnableCustomFontCheckboxTextDescription", "Habilita una fuente personalizada instalada en su sistema en la mayoría de las ventanas de la aplicación");
            Spanish.Add("LauchEditorText", "Iniciar editor de la base de datos");
            Spanish.Add("LauchEditorDescription", "Inicia el editor de la base de datos desde aquí, en lugar de desde la línea de comandos");
            Spanish.Add("LauchPatchDesignerText", "Iniciar diseñador de parche");
            Spanish.Add("LauchPatchDesignerDescription", "Ejecutar el diseñador de parches desde aquí, en lugar de desde la línea de comandos");
            Spanish.Add("LauchAutomationRunnerText", "Ejecutar el automatizador");
            Spanish.Add("LauchAutomationRunnerDescription", "Ejecutar el automatizador desde aquí, en lugar de desde la línea de comandos");
            Spanish.Add("InstallWhileDownloadingText", "Extraer durante la descarga");
            Spanish.Add("InstallWhileDownloadingCBDescription", "Cuando está habilitada, el instalador extraerá cada archivo zip tan pronto como se descargue" +
                " en lugar de esperar a que todos los archivos sean descargados para la extracción.");
            Spanish.Add("MulticoreExtractionCoresCountLabel", "Núcleos detectados: {0}");
            Spanish.Add("MulticoreExtractionCoresCountLabelDescription", "Número de núcleos lógicos (hilos) de CPU detectados en su sistema");
            Spanish.Add("SaveDisabledModsInSelectionText", "Conservar los mods deshabilitados cuando se guarde la selección");
            Spanish.Add("SaveDisabledModsInSelectionDescription", "Cuando un mod sea rehabilitado, será seleccionado desde su archivo de selección");
            Spanish.Add("AdvancedInstallationProgressText", "Mostrar ventana de instalación avanzada");
            Spanish.Add("AdvancedInstallationProgressDescription", "Muestra una ventana de instalación avanzada durante la extracción, útil cuando la extración multinúcleo está habilitada");
            Spanish.Add("ThemeSelectText", "Seleccione tema:");
            Spanish.Add("ThemeDefaultText", "Estándar");
            Spanish.Add("ThemeDefaultDescription", "Tema por defecto");
            Spanish.Add("ThemeDarkText", "Oscuro");
            Spanish.Add("ThemeDarkDescription", "Tema oscuro");
            Spanish.Add("ThemeCustomText", "Personalizado");
            Spanish.Add("ThemeCustomDescription", "Tema personalizado");
            Spanish.Add("DumpColorSettingsButtonText", "Guardar configuración de colores");
            Spanish.Add("OpenColorPickerButtonText", "Abrir selector de colores");
            Spanish.Add("OpenColorPickerButtonDescription", "Abre el selector de colores, permitiéndole crear su propio tema.");
            Spanish.Add("DumpColorSettingsButtonDescription", "Crea un documento XML con todos los componentes que pueden tener aplicado un color personalizado, para crear un tema personalizado");
            Spanish.Add("MulticoreExtractionText", "Modo de extración multinúcleo");
            Spanish.Add("MulticoreExtractionCBDescription", "El instaladór utilizará un método de extracción paralela. Extraerá varios archivos zip al mismo tiempo, " +
                " reduciendo el tiempo de instalación. SÓLO para discos SSD.");
            Spanish.Add("UninstallDefaultText", "Estándar");
            Spanish.Add("UninstallQuickText", "Rápida");
            Spanish.Add("ExportModeText", "Modo de exportación");
            Spanish.Add("ExportModeCBDescription", "El modo de exportación le permitirá seleccionar una carpeta y versión de WoT a la que exportar la instalación de Mods. Sólo para usuarios avanzados." +
                " Tenga en cuenta que NO desempaquetará archivos XML o archivos de parche (proporcionados por el juego), ni creará los archivos de tipo atlas. Habrá instrucciones en el directorio exportado.");
            Spanish.Add("ViewCreditsButtonText", "Ver créditos");
            Spanish.Add("ViewCreditsButtonDescription", "¡Ver todas las increíbles personas y proyectos que apoyan el modpack!");
            Spanish.Add("ExportWindowDescription", "Seleccione la versión de WoT para la que quiere exportar");
            Spanish.Add("HelperText", "¡Bienvenido a RelHax Modpack!" +
                "\nHe intentado hacer el Modpack tan sencillo como ha sido posible, pero aún así pueden surgir dudas. Mantenga el ratón sobre una opción para obtener una explicación." +
                "\n¡Gracias por usar Relhax, espero que lo disfrute! - Willster419");
            Spanish.Add("helperTextShort", "¡Bienvenido a RelHax Modpack!");
            Spanish.Add("NotifyIfSameDatabaseText", "Informar si no hay una nueva base de datos disponible (sólo estable)"); //"Informar si no hay nueva base de datos"
            Spanish.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Mostrar una notificación si la última instalación tiene la misma versión de la base de datos." +
                " De ser así, significa que no hay ninguna actualización para ningún Mod");
            Spanish.Add("NotifyIfSameDatabaseCBDescription", "Notificar si la última instalación tiene la misma versión de la base de datos que la actual. De ser así, significa que no ha habido ninguna actualización de ningún mod.\n" +
                "Esto solo funciona con la base de datos estable.");
            Spanish.Add("ShowInstallCompleteWindowText", "Ver ventana de instalación completada avanzada");
            Spanish.Add("ShowInstallCompleteWindowCBDescription", "Muestra una ventana al completar la instalación con opciones comunes tras la instalación del Modpack," +
                " tales como iniciar el juego, visitar la página web del XVM, etc.");
            Spanish.Add("applicationVersion", "Versión de la aplicación");
            Spanish.Add("databaseVersion", "Última base de datos");
            Spanish.Add("ClearCacheText", "Limpiar datos de caché de WoT");
            Spanish.Add("ClearCacheCBDescription", "Limpia la caché de WoT en el directorio %APPDATA%. Realiza la misma operación que la opción similar en el OMC Modpack");
            Spanish.Add("UninstallDefaultDescription", "La desinstalación estándar eliminará todos los archivos en los directorios de Mods del juego, incluyendo accesos directos y archivos de caché");
            Spanish.Add("UninstallQuickDescription", "La desinstalación rápida sólo eliminará archivos en los directorios de Mods del juego. No eliminará archivos" +
                " del Modpack, accesos directos o archivos de caché");
            Spanish.Add("DiagnosticUtilitiesButton", "Utilidades de diagnóstico");
            Spanish.Add("DiagnosticUtilitiesButtonDescription", "Informar de un error, intentar una reparación del cliente de WG, etc.");
            Spanish.Add("UninstallModeGroupBox", "Modo de desinstalación:");
            Spanish.Add("UninstallModeGroupBoxDescription", "Seleccione el modo de desinstalación a utilizar");
            Spanish.Add("FacebookButtonDescription", "Ir a nuestra página de Faceboook");
            Spanish.Add("DiscordButtonDescription", "Ir a nuestro servidor de Discord");
            Spanish.Add("TwitterButtonDescription", "Ir a nuestra página de Twitter");
            Spanish.Add("SendEmailButtonDescription", "Envíanos un e-mail (soporte del modpack no)");
            Spanish.Add("HomepageButtonDescription", "Visita nuestra página web");
            Spanish.Add("DonateButtonDescription", "Donaciones para el desarrollo");
            Spanish.Add("FindBugAddModButtonDescription", "¿Ha encontrado un error? ¿Quiere que un mod sea añadido? Informa aquí");
            Spanish.Add("SelectionViewGB", "Vista de selección");
            Spanish.Add("SelectionDefaultText", "Por defecto");
            Spanish.Add("SelectionLayoutDescription", "Selecciona un modo de la lista de selección.\nPor defecto: modo de Relhax.\nLegacy: lista en árbol de OMC");
            Spanish.Add("SelectionDefaultDescription", "Selecciona un modo de la lista de selección\nPor defecto: modo de Relhax\nLegacy: lista en árbol de OMC");
            Spanish.Add("SelectionLegacyDescription", "Selecciona un modo de la lista de selección\nPor defecto: modo de Relhax\nLegacy: lista en árbol de OMC");
            Spanish.Add("LanguageSelectionGBDescription", "Seleccione su idioma preferido");
            Spanish.Add("EnableBordersDefaultV2Text", "Habilitar bordes");
            Spanish.Add("EnableBordersLegacyText", "Habilitar bordes");
            Spanish.Add("EnableBordersDefaultV2CBDescription", "Habilita los bordes negros alrededor de cada mod y subnivel de configuración");
            Spanish.Add("EnableBordersLegacyCBDescription", "Habilitar los bordes negros alrededor de cada mod y subnivel de configuarción");
            Spanish.Add("UseBetaDatabaseText", "Utilizar la base de datos en beta");
            Spanish.Add("UseBetaDatabaseCBDescription", "Utiliza la última versión en beta de la base de datos. La estabilidad de los mods no está garantizada");
            Spanish.Add("UseBetaApplicationText", "Utilizar la aplicación en beta");
            Spanish.Add("UseBetaApplicationCBDescription", "Utiliza la última versión en beta de la aplicación. Las traducciones y estabilidad de la aplicación no están garantizadas");
            Spanish.Add("SettingsTabIntroHeader", "¡Bienvenido!");
            Spanish.Add("SettingsTabSelectionViewHeader", "Vista de selección");
            Spanish.Add("SettingsTabInstallationSettingsHeader", "Opciones de Instalación");
            Spanish.Add("SettingsTabApplicationSettingsHeader", "Opciones de la aplicación");
            Spanish.Add("SettingsTabAdvancedSettingsHeader", "Opciones avanzadas");
            Spanish.Add("MainWindowSelectSelectionFileToLoad", "Seleccione archivo de selección a cargar");
            Spanish.Add("verifyUninstallHeader", "Confirmación");
            Spanish.Add("verifyUninstallVersionAndLocation", "¿Confirma que desea desinstalar mods del directorio de WoT\n\n{0}\n\nutilizando el método de desinstalación '{1}'?");
            Spanish.Add("failedVerifyFolderStructure", "La aplicación no ha podido crear la estructura de carpetas requerida. Compruebe sus permisos de archivos o mueva la aplicación a una carpeta con permisos de escritura.");
            Spanish.Add("failedToExtractUpdateArchive", "La aplicación no ha podido extraer los archivos de actualización. Compruebe sus permisos de archivos y antivirus.");
            Spanish.Add("downloadingUpdate", "Descargando actualización de la apliación");
            //(removed components, disabled components, etc.)
            Spanish.Add("AutoOneclickSelectionErrorsContinueBody", "Hubo problemas cargando el archivo de selección (probablemente paquetes eliminados/deshabilitados, etc.)." +
                "\n¿Quiere continuar igualmente?");
            Spanish.Add("AutoOneclickSelectionErrorsContinueHeader", "Problemas cargando el archivo de selección");
            Spanish.Add("noAutoInstallWithBeta", "El modo de instalación automática no puede ser utilizado con la base de datos en beta");
            Spanish.Add("autoInstallWithBetaDBConfirmBody", "La autoinstalación será habilitada con la base de datos beta. La base de datos beta es actualizada frecuentemente," +
                " y puede resultar en múltiples instalaciones en un día. ¿Está seguro de que quiere hacer esto?");
            Spanish.Add("autoInstallWithBetaDBConfirmHeader", Spanish["verifyUninstallHeader"]);
            Spanish.Add("ColorDumpSaveFileDialog", "Seleccione dónde quiere guardar el archivo de personalización de colores");
            //"branch" is this context is git respoitory branches
            Spanish.Add("loadingBranches", "Cargando ramas");
            //"branch" is this context is git respoitory branches
            Spanish.Add("failedToParseUISettingsFile", "No se ha podido aplicar el tema. Compruebe el archivo de registro para más detalles. Habilite \"Registro Verboso\" para información adicional.");
            Spanish.Add("UISettingsFileApplied", "Tema aplicado");
            Spanish.Add("failedToFindWoTExe", "No se ha podido localizar la instalación del cliente de WoT. Por favor, envíe un informe de errores al desarrollador.");
            Spanish.Add("failedToFindWoTVersionXml", "No se ha podido obtener información de la versión instalada de WoT. Compruebe que el archivo 'version.xml' existe en el directorio 'World_of_Tanks'.");
            #endregion

            #region ModSelectionList
            Spanish.Add("ModSelectionList", "Lista de selección");
            Spanish.Add("ContinueButtonLabel", "Instalar");
            Spanish.Add("CancelButtonLabel", Spanish["cancel"]);
            Spanish.Add("HelpLabel", "Haga clic derecho en un componente de selección para abrir una ventana de vista previa");
            Spanish.Add("LoadSelectionButtonLabel", "Cargar selección");
            Spanish.Add("SaveSelectionButtonLabel", "Guardar selección");
            Spanish.Add("SelectSelectionFileToSave", "Guardar archivo de selección");
            Spanish.Add("ClearSelectionsButtonLabel", "Limpiar selección");
            Spanish.Add("SearchThisTabOnlyCB", "Buscar sólo en esta pestaña");
            Spanish.Add("searchComboBoxInitMessage", "Buscar un paquete...");
            Spanish.Add("SearchTBDescription", "También puede buscar varias partes del nombre, separadas por un * (asterisco).\n Por ejemplo: config*willster419 mostrará como resultado: Willster419\'s Config");
            Spanish.Add("InstallingAsWoTVersion", "Instalando como versión de WoT: {0}");
            Spanish.Add("UsingDatabaseVersion", "Usando base de datos: {0} ({1})");
            Spanish.Add("userMods", "Mods del usuario");
            Spanish.Add("FirstTimeUserModsWarning", "Esta pestaña es para seleccionar archivos zip en el directorio \"RelhaxUserMods\". Deben ser archivos zip, y derían usar un directorio raíz del directorio \"World_of_Tanks\"");
            Spanish.Add("downloadingDatabase", "Descargando base de datos");
            Spanish.Add("readingDatabase", "Leyendo base de datos");
            Spanish.Add("loadingUI", "Cargando interfaz del usuario");
            Spanish.Add("verifyingDownloadCache", "Verificando la integridad de los archivos de ");
            Spanish.Add("InstallProgressTextBoxDescription", "El progreso de una instalación será mostrado aquí");
            Spanish.Add("testModeDatabaseNotFound", "CRÍTICO: No se ha encontrado base de datos del modo de testeo en:\n{0}");
            Spanish.Add("duplicateMods", "CRÍTICO: Detectada ID de paquete duplicada");
            Spanish.Add("databaseReadFailed", "CRÍTICO: No se ha podido leer la base de datos\n\nVer archivo de registro para información detallada");
            Spanish.Add("configSaveSuccess", "Selección guardada correctamente");
            Spanish.Add("selectConfigFile", "Busca un archivo de selección para cargar");
            Spanish.Add("configLoadFailed", "El archivo de selección no pudo ser cargado, cargando en modo estándar");
            Spanish.Add("modNotFound", "El paquete (ID = \"{0}\") no se ha encontrado en la base de datos. Puede haber sido renombrado o eliminado");
            Spanish.Add("modDeactivated", "Los siguientes paquetes están actualmente desactivados en el modpack y no han podido ser seleccionados para instalar");
            Spanish.Add("modsNotFoundTechnical", "Los siguientes paquetes no han sido encontrados, y han sido probablemente eliminados");
            Spanish.Add("modsBrokenStructure", "Los siguientes paquetes han sido deshabilitados debido a modificaciones en la estructura de paquetes. Deberá volver a seleccionarlos si quiere instalarlos.");
            Spanish.Add("packagesUpdatedShouldInstall", "Los siguientes paquetes han sido actualizados desde la última carga de este archivo de selección. Su archivo de selección ha sido actualizado con los cambios (y se ha creado una copia de seguridad de un sólo uso). " +
                "Si ésta es su instalación actual, y quiere conservarla, se recomienda instalar/actualizar después de ver este mensaje.");
            Spanish.Add("selectionFileIssuesTitle", "Mensajes de carga de la selección");
            Spanish.Add("selectionFormatOldV2", "Este formato de archivo de selección es de legado (V2) y será actualizado a V3. Una copia de seguridad de tipo V2 será creada.");
            Spanish.Add("oldSavedConfigFile", "El archivo de configuración que está utilizando tiene un formato antiguo y no será preciso en el futuro. ¿Convertir al nuevo formato? (Se guardará una copia de seguridad del formato original)");
            Spanish.Add("prefrencesSet", "Preferencias guardadas");
            Spanish.Add("selectionsCleared", "Selecciones limpiadas");
            Spanish.Add("failedLoadSelection", "No se ha podido cargar la selección");
            Spanish.Add("unknownselectionFileFormat", "Versión desconocida del archivo de selección");
            Spanish.Add("ExpandAllButton", "Expandir pestaña actual");
            Spanish.Add("CollapseAllButton", "Colapsar pestaña actual");
            Spanish.Add("InstallingTo", "Instalando en: {0}");
            Spanish.Add("selectWhereToSave", "Seleccionar dónde guardar el archivo de selección");
            Spanish.Add("updated", "actualizado");
            Spanish.Add("disabled", "deshabilitado");
            Spanish.Add("invisible", "invisible");
            Spanish.Add("SelectionFileIssuesDisplay", "Errores al aplicar el archivo de selección");
            Spanish.Add("selectionFileIssues", Spanish["SelectionFileIssuesDisplay"]);
            Spanish.Add("selectionFileIssuesHeader", "Por favor, lea los siguientes mensajes relacionados con su archivo de selección");
            Spanish.Add("VersionInfo", "Actualizacón de la aplicación");
            Spanish.Add("VersionInfoYesText", Spanish["yes"]);
            Spanish.Add("VersionInfoNoText", Spanish["no"]);
            Spanish.Add("NewVersionAvailable", "Nueva versión disponible");
            Spanish.Add("HavingProblemsTextBlock", "Si tiene problemas actualizando, por favor");
            Spanish.Add("ManualUpdateLink", "Haga clic aquí");
            Spanish.Add("loadingApplicationUpdateNotes", "Cargando notas de la actualización de la aplicación...");
            Spanish.Add("failedToLoadUpdateNotes", "No se han podido cargar las notas de actualización de la aplicación");
            Spanish.Add("ViewUpdateNotesOnGoogleTranslate", "Ver en Traductor de Google");
            Spanish.Add("VersionInfoAskText", "¿Quiere actualizar ahora?");
            Spanish.Add("SelectDownloadMirrorTextBlock", "Seleccionar servidor de descarga");
            Spanish.Add("SelectDownloadMirrorTextBlockDescription", "Este servidor será utilizado sólo para las descargas de paquetes");
            Spanish.Add("downloadMirrorUsaDefault", "relhaxmodpack.com, Dallas, EE.UU.");
            Spanish.Add("downloadMirrorDe", "clanverwaltung.de, Frankfurt, Alemania");
            #endregion

            #region Installer Messages
            Spanish.Add("Downloading", "Descargando");
            Spanish.Add("patching", "Parcheando");
            Spanish.Add("done", "Hecho");
            Spanish.Add("cleanUp", "Limpiando recursos");
            Spanish.Add("idle", "En espera");
            Spanish.Add("status", "Estado:");
            Spanish.Add("canceled", "Cancelado");
            Spanish.Add("appSingleInstance", "Comprobando instancia única");
            Spanish.Add("checkForUpdates", "Comprobando actualizaciones");
            Spanish.Add("verDirStructure", "Verificando estructura del directorio");
            Spanish.Add("loadingSettings", "Cargando opciones");
            Spanish.Add("loadingTranslations", "Cargando traducciones");
            Spanish.Add("loading", "Cargando");
            Spanish.Add("of", "de");
            Spanish.Add("failedToDownload1", "No se ha podido descargar el paquete");
            Spanish.Add("failedToDownload2", "¿Quiere reintentar la descarga, abortar la instalación, o continuar?");
            Spanish.Add("failedToDownloadHeader", "Fallo en la descarga");
            Spanish.Add("failedManager_version", "La versión beta actual de la aplicación está anticuada y debe ser actualizada. No hay una versión Beta nueva funcionando actualmente");
            Spanish.Add("fontsPromptInstallHeader", "¿Administrador para instalar fuentes?");
            Spanish.Add("fontsPromptInstallText", "¿Tiene permisos de administrador para instalar fuentes?");
            Spanish.Add("fontsPromptError_1", "No se han podido instalar fuentes. Algunos mods pueden no funcionar correctamente. Fuentes disponibles en ");
            Spanish.Add("fontsPromptError_2", "\\_fonts. Puede instalarlas usted mismo o volver a ejecutar la instalación como administrador");
            Spanish.Add("cantDownloadNewVersion", "No ha sido posible descargar la nueva versión. Cerrando el programa.");
            Spanish.Add("failedCreateUpdateBat", "No ha sido posible crear el proceso de actualización.\n\nPor favor, elimine manualmente el archivo:\n{0}\n\nrenombre el archivo:\n{1}\na:\n{2}\n¿Ir al directorio?");
            Spanish.Add("cantStartNewApp", "No ha sido posible arrancar la aplicación, pero se encuentra en \n");
            Spanish.Add("autoDetectFailed", "La detección automática ha fallado. Por favor, use la opción de 'forzar detección manual'");
            Spanish.Add("anotherInstanceRunning", "Hay otra instancia de Relhax Manager ejecutándose");
            Spanish.Add("closeInstanceRunningForUpdate", "Por favor, cierre TODAS las instancias de Relhax Manager en ejecución antes de poder actualizar.");
            Spanish.Add("skipUpdateWarning", "Está saltándose la actualización. La compatibilidad de la base de datos no está garantizada");
            Spanish.Add("patchDayMessage", "El modpack está actualmente inactivo para testear y actualizar mods durante el día de parche. Sentimos las inconveniencias cusadas.");
            Spanish.Add("configNotExist", "{0} NO existe, cargando en modo normal");
            Spanish.Add("autoAndFirst", "La primera vez no se puede cargar en modo autoinstalación, cargando en modo normal");
            Spanish.Add("confirmUninstallHeader", "Confirmación");
            Spanish.Add("confirmUninstallMessage", "Confirme que quiere desinstalar mods de la instalación de WoT\n\n{0}\n\n¿usando el método de desinstalación '{1}'?");
            Spanish.Add("uninstallingText", "Desinstalando...");
            Spanish.Add("uninstallingFile", "Desinstalando archivo");
            Spanish.Add("uninstallFinished", "Desinstalación de los mods terminada.");
            Spanish.Add("uninstallFail", "La desinstalación ha fallado. Puede intentar otro modo de desinstalación o enviar un informe de error.");
            Spanish.Add("extractionErrorMessage", "Error eliminando las carpetas 'res_mods' o 'mods'. World of Tanks está en ejecución o bien" +
                " sus permisos de seguridad de archivos y carpetas son incorrectos.");
            Spanish.Add("extractionErrorHeader", Spanish["error"]);
            Spanish.Add("deleteErrorHeader", "Cierre las carpetas");
            Spanish.Add("deleteErrorMessage", "Por favor, cierre las carpetas 'mods' y 'res_mods' (y sus subcarpetas) en el explorador, y pulse OK para continuar.");
            Spanish.Add("noUninstallLogMessage", "El archivo de registro que contiene la lista de archivos instalados (installedRelhaxFiles.log) no existe. ¿Quiere eliminar todos los mods en su lugar?");
            Spanish.Add("noUninstallLogHeader", "Eliminar todos los mods");
            Spanish.Add("moveOutOfTanksLocation", "El modpack no puede ser ejecutado desde el directorio de World_of_Tanks. Por favor, mueva la aplicación y vuelva a intentarlo.");
            Spanish.Add("moveAppOutOfDownloads", "La aplicación ha detectado que está siendo ejecutanda desde la carpeta 'Descargas'. Esto no se recomienda dado que la aplicación crea varios archivos y carpetas " +
                "que pueden ser difíciles de encontrar en una carpeta 'Descargas' grande. Recomendamos que mueva la aplicación y todos los archivos y carpetas 'Relhax' a una nueva carpeta.");
            Spanish.Add("DatabaseVersionsSameBody", "La base de datos no ha cambiado desde su última instalación. Por tanto no hay actualizaciones para su selección de mods actual. ¿Continuar de todas formas?");
            Spanish.Add("DatabaseVersionsSameHeader", "La versión de la base de datos es idéntica.");
            Spanish.Add("databaseNotFound", "No se ha encontrado base de datos en la URL especificada");
            Spanish.Add("detectedClientVersion", "Versión del cliente detectada");
            Spanish.Add("supportedClientVersions", "Versiones del cliente soportadas");
            Spanish.Add("supportNotGuarnteed", "Esta versión del cliente no está oficialmente soportada. Algunos mods pueden no funcionar.");
            Spanish.Add("couldTryBeta", "Si el juego ha sido recientemente actualizado, el equipo está trabajando en proporcionarle soporte. Puede intentar usar la base de datos en beta.");
            Spanish.Add("missingMSVCPLibrariesHeader", "No se han podido cargar las librerías necesarias");
            Spanish.Add("missingMSVCPLibraries", "No se han podido cargar las librerías de procesamiento de imágenes de iconos de contorno. Esto puede indicar que le falta un paquete requerido .dll de Microsoft");
            Spanish.Add("openLinkToMSVCP", "¿Quiere abrir su navegador en la página de descarga del paquete?");
            Spanish.Add("noChangeUntilRestart", "Esta opción no tendrá efecto hasta reiniciar la aplicación");
            Spanish.Add("installBackupMods", "Haciendo una copia de seguridad del mod");
            Spanish.Add("installBackupData", "Haciendo una copia de seguridad de la configuración del usuario");
            Spanish.Add("installClearCache", "Eliminando la caché del WoT");
            Spanish.Add("installClearLogs", "Eliminando archivos de registro");
            Spanish.Add("installCleanMods", "Limpiando carpetas de mods");
            Spanish.Add("installExtractingMods", "Extrayendo paquete");
            Spanish.Add("installZipFileEntry", "Entrada de archivo");
            Spanish.Add("installExtractingCompletedThreads", "Completados hilos de extracción");
            Spanish.Add("installExtractingOfGroup", "del grupo de instalación");
            Spanish.Add("extractingUserMod", "Extrayendo paquete de usuario");
            Spanish.Add("installRestoreUserdata", "Restableciendo configuración del usuario");
            Spanish.Add("installXmlUnpack", "Desempaquetando archivo XML");
            Spanish.Add("installPatchFiles", "Parcheando archivo");
            Spanish.Add("installShortcuts", "Creando accesos directos");
            Spanish.Add("installContourIconAtlas", "Creando archivo de Atlas");
            Spanish.Add("installFonts", "Instalando fuentes");
            Spanish.Add("installCleanup", "Limpiando");
            Spanish.Add("AtlasExtraction", "Extrayendo archivo de Atlas");
            Spanish.Add("copyingFile", "Copiando archivo");
            Spanish.Add("deletingFile", "Eliminando archivo");
            Spanish.Add("scanningModsFolders", "Escaneando carpetas de mods...");
            Spanish.Add("file", "Archivo");
            Spanish.Add("size", "Tamaño");
            Spanish.Add("checkDatabase", "Comprobando la base de datos para archivos anticuados o no necesarios");
            Spanish.Add("parseDownloadFolderFailed", "El análisis de la carpeta \"{0}\" ha fallado.");
            Spanish.Add("installationFinished", "Instalación finalizada");
            Spanish.Add("deletingFiles", "Eliminando archivos");
            Spanish.Add("uninstalling", "Desinstalando");
            Spanish.Add("zipReadingErrorHeader", "Descarga incompleta");
            Spanish.Add("zipReadingErrorMessage1", "El archivo zip");
            Spanish.Add("zipReadingErrorMessage3", "no ha podido ser leído.");
            Spanish.Add("patchingSystemDeneidAccessMessage", "Se ha denegado el acceso del sistema de parcheo a la carpeta del parche. Vuelva a intentarlo como Administrador. Si vuelve a ver este mensaje," +
                " tiene que corregir los permisos de seguridad de sus archivos y carpetas.");
            Spanish.Add("patchingSystemDeneidAccessHeader", "Acceso denegado");
            Spanish.Add("folderDeleteFailed", "No se ha podido eliminar la carpeta");
            Spanish.Add("fileDeleteFailed", "No se ha podido eliminar el archivo");
            Spanish.Add("DeleteBackupFolder", "Copias de seguridad");
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            Spanish.Add("installFailed", "La instalación ha fallado en los siguientes puntos");
            #endregion

            #region Install finished window
            Spanish.Add("InstallFinished", "Instalación completada");
            Spanish.Add("InstallationCompleteText", "La instalación ha sido completada. Quiere...");
            Spanish.Add("InstallationCompleteStartWoT", "¿Iniciar el juego?");
            Spanish.Add("InstallationCompleteStartGameCenter", "¿Iniciar WG Game Center?");
            Spanish.Add("InstallationCompleteOpenXVM", "¿Abrir su explorador en la página de inicio de sesión de las estadísticas de XVM?");
            Spanish.Add("InstallationCompleteCloseThisWindow", "¿Cerrar esta ventana?");
            Spanish.Add("InstallationCompleteCloseApp", "¿Cerrar la aplicación?");
            Spanish.Add("xvmUrlLocalisation", "en"); //? No Spanish on XVM website. Mistake? @Nullmaruzero
            Spanish.Add("CouldNotStartProcess", "No se ha podido iniciar el proceso");
            #endregion

            #region Diagnostics
            Spanish.Add("Diagnostics", "Diagnósticos");
            Spanish.Add("DiagnosticsMainTextBox", "Puede utilizar las opciones a continuación para intentar diagnosticar o resolver problemas");
            Spanish.Add("LaunchWoTLauncher", "Inicia el lanzador de World of Tanks en el modo de validación de integridad");
            Spanish.Add("CollectLogInfo", "Recoger los archivos de registro en un archivo zip para informar de un problema");
            Spanish.Add("CollectLogInfoButtonDescription", "Recopila todos los archivos de registro necesarios en un archivo ZIP.\nEsto le facilita informar de un problema");
            Spanish.Add("DownloadWGPatchFilesText", "Descargar los archivos de parche de WG para cualquier cliente mediante HTTP");
            Spanish.Add("DownloadWGPatchFilesButtonDescription", "Le guiará e instalará los archivos de parche para los juegos de Wargaming (WoT, WoWs, WoWp) mediante HTTP para que los pueda instalar más tarde.\n" +
                "Particularmente útil para quien no puede utilizar el protocolo estándar P2P de Wargaming Game Center (WGC)");
            Spanish.Add("SelectedInstallation", "Instalación seleccionada actualmente:");
            Spanish.Add("SelectedInstallationNone", "(" + Spanish["none"].ToLower() + ")");
            Spanish.Add("collectionLogInfo", "Reuniendo los archivos de registro...");
            Spanish.Add("startingLauncherRepairMode", "Inicia WoTLauncher en el modo de validación de integridad...");
            Spanish.Add("failedCreateZipfile", "No se ha podido crear el archivo zip ");
            Spanish.Add("launcherRepairModeStarted", "Modo de reparación iniciado correctamente");
            Spanish.Add("ClearDownloadCache", "Limpiar la caché de descarga");
            Spanish.Add("ClearDownloadCacheDatabase", "Elimina el archivo de caché de descarga");
            Spanish.Add("ClearDownloadCacheDescription", "Elimina todos los archivos en la carpeta \"RelhaxDownloads\"");
            Spanish.Add("ClearDownloadCacheDatabaseDescription", "Elimina el archivo de base de datos XML. Esto causará que la integridad de todos los archivos zip vuelva a ser comprobada.\nTodos los archivos inválidos volverán a ser descargados si son seleccionados en su próxima instalación.");
            Spanish.Add("clearingDownloadCache", "Limpiando caché de descarga");
            Spanish.Add("failedToClearDownloadCache", "No se ha podido limpiar la caché de descarga");
            Spanish.Add("cleaningDownloadCacheComplete", "Caché de descarga limpiada");
            Spanish.Add("clearingDownloadCacheDatabase", "Eliminando archivo de caché de la base de datos XML");
            Spanish.Add("failedToClearDownloadCacheDatabase", "No se ha podido eliminar el archivo de caché de la base de datos XML");
            Spanish.Add("cleaningDownloadCacheDatabaseComplete", "Archivo de caché de la base de datos XML eliminado");
            Spanish.Add("ChangeInstall", "Cambia la instalación de WoT seleccionada actualmente");
            Spanish.Add("ChangeInstallDescription", "Esto cambiará qué archivos de registro serán añadidos en el archivo zip de diagnóstico");
            Spanish.Add("zipSavedTo", "Archivo zip guardado en: ");
            Spanish.Add("selectFilesToInclude", "Seleccione los archivos a incluir en el informe de errores");
            Spanish.Add("TestLoadImageLibraries", "Carga de prueba de las librerías de procesamiento de imágenes tipo atlas");
            Spanish.Add("TestLoadImageLibrariesButtonDescription", "Comprueba las librerías de procesamiento de las imágenes tipo atlas");
            Spanish.Add("loadingAtlasImageLibraries", "Cargando librerías de procesamiento de imágenes de atlas");
            Spanish.Add("loadingAtlasImageLibrariesSuccess", "Librerías de procesamiento de imágenes de atlas cargadas correctamente");
            Spanish.Add("loadingAtlasImageLibrariesFail", "No se han podido cargar las librerías de procesamiento de imágenes de atlas");
            Spanish.Add("CleanupModFilesText", "Limpiar archivos de mods en rutas incorrectas");
            Spanish.Add("CleanupModFilesButtonDescription", "Elimina cualquier mod en carpetas como win32 y win64 que puedan causar conflictos al cargar");
            Spanish.Add("cleanupModFilesCompleted", "Limpieza de mods completada");
            Spanish.Add("CleanGameCacheText", "Limpar caché del juego");
            Spanish.Add("cleanGameCacheProgress", "Limpiando caché del juego");
            Spanish.Add("cleanGameCacheSuccess", "Caché del juego limpiada con éxito");
            Spanish.Add("cleanGameCacheFail", "No se ha podido limpar los archivos de caché del juego");
            Spanish.Add("TrimRelhaxLogfileText", "Recortar el archivo de registro relhax.log a las últimas 3 ejecuciones (asumiendo que existan entradas de encabezamiento/pie de página.");
            Spanish.Add("trimRelhaxLogProgress", "Recortando el archivo de registro de Relhax.");
            Spanish.Add("trimRelhaxLogSuccess", "Archivo de registro de Relhax recortado correctamente.");
            Spanish.Add("trimRelhaxLogFail", "No se ha podido recortar correctamente el archivo de registro de Relhax.");
            #endregion

            #region Wot Client install selection
            Spanish.Add("WoTClientSelection", "Selección de cliente de WoT");
            Spanish.Add("ClientSelectionsTextHeader", "Las siguientes instalaciones de cliente fueron detectadas automáticamente");
            Spanish.Add("ClientSelectionsCancelButton", Spanish["cancel"]);
            Spanish.Add("ClientSelectionsManualFind", "Selección manual");
            Spanish.Add("ClientSelectionsContinueButton", Spanish["select"]);
            Spanish.Add("AddPicturesZip", "Añadir los archivos a ZIP");
            Spanish.Add("DiagnosticsAddSelectionsPicturesLabel", "Añada archivos adicionales aquí (archivos de selección, imágenes, etc.)");
            Spanish.Add("DiagnosticsAddFilesButton", "Añadir archivos");
            Spanish.Add("DiagnosticsRemoveSelectedButton", "Eliminar seleccionados");
            Spanish.Add("DiagnosticsContinueButton", Spanish["ContinueButton"]);
            Spanish.Add("cantRemoveDefaultFile", "No se puede eliminar un archivo que debe ser añadido por defecto.");
            #endregion

            #region Preview Window
            Spanish.Add("Preview", "Previsualización");
            Spanish.Add("noDescription", "No hay descripción disponible");
            Spanish.Add("noUpdateInfo", "No hay información de actualización disponible");
            Spanish.Add("noTimestamp", "No hay marca de tiempo disponible");
            Spanish.Add("PreviewNextPicButton", Spanish["next"]);
            Spanish.Add("PreviewPreviousPicButton", Spanish["previous"]);
            Spanish.Add("DevUrlHeader", "Links de los desarrolladores");
            Spanish.Add("dropDownItemsInside", "Artículos en el interior");
            Spanish.Add("popular", "popular");
            Spanish.Add("previewEncounteredError", "La ventana de previsualización ha encontrado un error. No se ha podido mostrar previsualización");
            Spanish.Add("popularInDescription", "Este paquete es popular");
            Spanish.Add("controversialInDescription", "Este paquete es polémico");
            Spanish.Add("encryptedInDescription", "Este paquete está encriptado y no se puede verificar que no contenga virus.");
            Spanish.Add("fromWgmodsInDescription", "La fuente de este paquete es el portal de WGmods (wgmods.net)");
            #endregion

            #region Developer Selection Window
            Spanish.Add("DeveloperSelectionsViewer", "Visor de selecciones");
            Spanish.Add("DeveloperSelectionsTextHeader", "Selección a cargar");
            Spanish.Add("DeveloperSelectionsCancelButton", Spanish["cancel"]);
            Spanish.Add("DeveloperSelectionsLocalFile", "Archivo local");
            Spanish.Add("DeveloperSelectionsContinueButton", "Seleccionar");
            Spanish.Add("failedToParseSelections", "No se han podido analizar las selecciones");
            Spanish.Add("lastModified", "Modificado por última vez");
            #endregion

            #region Advanced Installer Window
            Spanish.Add("AdvancedProgress", "Progreso detallado del instalador");
            Spanish.Add("PreInstallTabHeader", "Operaciones pre-instalación");
            Spanish.Add("ExtractionTabHeader", "Extracción");
            Spanish.Add("PostInstallTabHeader", "Operaciones post-instalación");
            Spanish.Add("AdvancedInstallBackupData", "Hacer una copia de seguridad de los datos de los mods");
            Spanish.Add("AdvancedInstallClearCache", "Limpiar caché de WoT");
            Spanish.Add("AdvancedInstallClearLogs", "Limpiar archivos de registro");
            Spanish.Add("AdvancedInstallClearMods", "Desinstalar instalación anterior");
            Spanish.Add("AdvancedInstallInstallMods", "Hilo de instalación");
            Spanish.Add("AdvancedInstallUserInstallMods", "Instalación del usuario");
            Spanish.Add("AdvancedInstallRestoreData", "Restaurar configuración");
            Spanish.Add("AdvancedInstallXmlUnpack", "Desempaquetador de XML");
            Spanish.Add("AdvancedInstallPatchFiles", "Archivos de parche");
            Spanish.Add("AdvancedInstallCreateShortcuts", "Crear accesos directos");
            Spanish.Add("AdvancedInstallCreateAtlas", "Crear atlases");
            Spanish.Add("AdvancedInstallInstallFonts", "Instalar fuentes");
            Spanish.Add("AdvancedInstallTrimDownloadCache", "Recortar la caché de descarga");
            Spanish.Add("AdvancedInstallCleanup", "Limpieza");
            #endregion

            #region News Viewer
            Spanish.Add("NewsViewer", "Novedades");
            Spanish.Add("application_Update_TabHeader", "Aplicación");
            Spanish.Add("database_Update_TabHeader", "Base de datos");
            Spanish.Add("ViewNewsOnGoogleTranslate", "Ver en el Traductor de Google");
            #endregion

            #region Loading Window
            Spanish.Add("ProgressIndicator", "Cargando");
            Spanish.Add("LoadingHeader", "Cargando, por favor espere");
            #endregion

            #region First Load Acknowledgements
            Spanish.Add("FirstLoadAcknowledgments", "Primera ejecución — Acuerdo de Licencia"); // Me neither (LordFelix), so i'll follow PL procedure
            Spanish.Add("AgreementLicense", "He leído y acepto el ");
            Spanish.Add("LicenseLink", "Acuerdo de licencia");
            Spanish.Add("AgreementSupport1", "Entiendo que puedo recibir soporte en el ");
            Spanish.Add("AgreementSupportDiscord", "Discord");
            Spanish.Add("AgreementHoster", "Entiendo que Relhax sólo es un servicio de alojamiento e instalación de mods, y Relhax no mantiene cada mod incluido en este modpack");
            Spanish.Add("AgreementAnonData", "Entiendo que Relhax V2 recoge datos anónimos de uso para mejorar la aplicación, lo cual puede ser deshabilitado en la pestaña de opciones avanzadas");
            Spanish.Add("V2UpgradeNoticeText", "Parece que está ejecutando la actualización de V1 a V2 por primera vez.\n" +
                "Pulsar continuar resultará en una actualización de la estructura de archivos que no puede ser revertida. Se recomienda crear una copia de seguridad de su carpeta V1 antes de continuar");
            Spanish.Add("upgradingStructure", "Actualizando estructura de archivos y carpetas de V1");
            #endregion

            #region Export Mode
            Spanish.Add("ExportModeSelect", "Seleccione cliente de WoT para exportación");
            Spanish.Add("selectLocationToExport", "Seleccione la carpeta para exportar la instalación de mods");
            Spanish.Add("ExportSelectVersionHeader", "Por favor, seleccione la versión del cliente de WoT para la que quiere exportar");
            Spanish.Add("ExportContinueButton", Spanish["ContinueButton"]);
            Spanish.Add("ExportCancelButton", Spanish["cancel"]);
            Spanish.Add("ExportModeMajorVersion", "Versión de la carpeta online");
            Spanish.Add("ExportModeMinorVersion", "Versón de WoT");
            #endregion

            #region Asking to close WoT
            Spanish.Add("AskCloseWoT", "WoT está en ejecución");
            Spanish.Add("WoTRunningTitle", "WoT está en ejecución");
            Spanish.Add("WoTRunningHeader", "Parece que su instalación de WoT está abierta. Por favor, ciérrela para poder continuar");
            Spanish.Add("WoTRunningCancelInstallButton", "Cancelar instalación");
            Spanish.Add("WoTRunningRetryButton", "Volver a detectar");
            Spanish.Add("WoTRunningForceCloseButton", "Forzar el cierre del juego");
            #endregion

            #region Scaling Confirmation
            Spanish.Add("ScalingConfirmation", "Confirmar escalado");
            Spanish.Add("ScalingConfirmationHeader", "El valor de escala ha sido cambiado. ¿Quiere conservarlo?");
            Spanish.Add("ScalingConfirmationRevertTime", "Revirtiendo cambios en {0} segundo(s)");
            Spanish.Add("ScalingConfirmationKeep", "Mantener");
            Spanish.Add("ScalingConfirmationDiscard", "Descartar");
            #endregion

            #region Game Center download utility
            Spanish.Add("GameCenterUpdateDownloader", "Descarga de actualizaciones para Wargaming Game Center");
            Spanish.Add("GcDownloadStep1Header", "Seleccione cliente de juego");
            Spanish.Add("GcDownloadStep1TabDescription", "Seleccione el cliente de Wargaming para el que recolectar datos (WoT, WoWs, WoWp)");
            Spanish.Add("GcDownloadStep1SelectClientButton", "Seleccione cliente");
            Spanish.Add("GcDownloadStep1CurrentlySelectedClient", "Cliente actualmente seleccionado: {0}");
            Spanish.Add("GcDownloadStep1NextText", Spanish["next"]);
            Spanish.Add("GcDownloadStep1GameCenterCheckbox", "Comprobar actualizaciones de Game Center en su lugar");
            Spanish.Add("GcDownloadSelectWgClient", "Seleccione cliente de WG para el que buscar actualizaciones");
            Spanish.Add("ClientTypeValue", "Ninguno");
            Spanish.Add("LangValue", Spanish["ClientTypeValue"]);
            Spanish.Add("GcMissingFiles", "Su cliente no tiene los siguientes archivos XML de definiciones");
            Spanish.Add("GcDownloadStep2Header", "Cerrar Game Center");
            Spanish.Add("GcDownloadStep2TabDescription", "Ciera el Game Center (la aplicación detectará el cierre)");
            Spanish.Add("GcDownloadStep2GcStatus", "Game Center está {0}");
            Spanish.Add("GcDownloadStep2GcStatusOpened", "abierto");
            Spanish.Add("GcDownloadStep2GcStatusClosed", "cerrado");
            Spanish.Add("GcDownloadStep2PreviousText", Spanish["previous"]);
            Spanish.Add("GcDownloadStep2NextText", Spanish["next"]);
            Spanish.Add("GcDownloadStep3Header", "Obtener información de actualización");
            Spanish.Add("GcDownloadStep3TabDescription", "Obteniendo lista de archivos de parche a descargar");
            Spanish.Add("GcDownloadStep3NoFilesUpToDate", "No se han encontrado archivos de parche para descargar");
            Spanish.Add("GcDownloadStep3PreviousText", Spanish["previous"]);
            Spanish.Add("GcDownloadStep3NextText", Spanish["next"]);
            Spanish.Add("GcDownloadStep4Header", "Descargar archivos de actualización");
            Spanish.Add("GcDownloadStep4TabDescription", "Descargando archivos de parche...");
            Spanish.Add("GcDownloadStep4DownloadingCancelButton", Spanish["cancel"]);
            Spanish.Add("GcDownloadStep4DownloadingText", "Descargando parche {0} de  {1}: {2}");
            Spanish.Add("GcDownloadStep4DownloadComplete", "Descarga de paquetes completada");
            Spanish.Add("GcDownloadStep4PreviousText", Spanish["previous"]);
            Spanish.Add("GcDownloadStep4NextText", Spanish["next"]);
            Spanish.Add("GcDownloadStep5Header", "¡Completado!");
            Spanish.Add("GcDownloadStep5TabDescription", "¡El proceso se ha completado! WG Game Center debería detectar los archivos en su próxima ejecución");
            Spanish.Add("GcDownloadStep5CloseText", Spanish["close"]);
            Spanish.Add("FirstLoadSelectLanguage", "Selección de Idioma");
            Spanish.Add("SelectLanguageHeader", "Por favor, seleccione el idioma");
            Spanish.Add("SelectLanguagesContinueButton", Spanish["ContinueButton"]);
            Spanish.Add("Credits", "Créditos de Relhax Modpack");
            Spanish.Add("creditsProjectLeader", "Líder de proyecto");
            Spanish.Add("creditsDatabaseManagers", "Administradores de la base de datos");
            Spanish.Add("creditsTranslators", "Traductores");
            Spanish.Add("creditsusingOpenSourceProjs", "Relhax Modpack usa los siguientes proyectos de código abierto");
            Spanish.Add("creditsSpecialThanks", "Agradecimientos especiales");
            Spanish.Add("creditsGrumpelumpf", "Líder de proyecto de OMC Modpack, nos permitió retomar Relhax desde donde él lo dejó");
            Spanish.Add("creditsRkk1945", "El primer beta tester que trabajó conmigo durante meses para poner en marcha el proyecto");
            Spanish.Add("creditsRgc", "Sponsor del modpack y miembro del primer grupo de beta testers");
            Spanish.Add("creditsBetaTestersName", "Nuestro equipo de beta testers");
            Spanish.Add("creditsBetaTesters", "Testeo continuado e informe de bugs en la aplicación antes de su publicación");
            Spanish.Add("creditsSilvers", "Ayuda con la comunicación con la comunidad y en redes sociales");
            Spanish.Add("creditsXantier", "Soporte inicial de IT y preparación el servidor");
            Spanish.Add("creditsSpritePacker", "Desarrollo del algoritmo de empaquetado de hojas de sprites y portado a .NET");
            Spanish.Add("creditsWargaming", "Crear un método sencillo para automatizar el sistema de mods");
            Spanish.Add("creditsUsersLikeU", "Usuarios como usted");
            #endregion

            #region Conflicting Packages Dialog
            Spanish.Add("ConflictingPackageDialog", "Conflicto de paquetes");
            Spanish.Add("conflictingPackageMessageOptionA", "Opción A");
            Spanish.Add("conflictingPackageMessageOptionB", "Opción B");
            Spanish.Add("conflictingPackageMessagePartA", "Ha seleccionado el paquete \"{0}\", pero entra en conflicto con los siguientes paquetes:");
            Spanish.Add("conflictingPackagePackageOfCategory", "- {0}, de la categoría {1}");
            Spanish.Add("conflictingPackageMessagePartB", Spanish["conflictingPackageMessageOptionA"] + ": Seleccionar \"{0}\", lo que deseleccionará todos los paquetes en conflicto");
            Spanish.Add("conflictingPackageMessagePartC", Spanish["conflictingPackageMessageOptionB"] + ": No seleccionar \"{0}\", lo que mantendrá todos los paquetes en conflicto");
            Spanish.Add("conflictingPackageMessagePartD", "Cerrar la ventana seleccionará la opción B");
            Spanish.Add("conflictingPackageMessagePartE", "Por favor, elija una opción");
            #endregion

            #region End of Life announcement
            Spanish.Add("EndOfLife", "Fin de desarrollo de Relhax");
            Spanish.Add("CloseWindowButton", Spanish["close"]);
            Spanish.Add("WoTForumAnnouncementsTextBlock", "Post del foro de anuncios de WoT:");
            Spanish.Add("endOfLifeMessagePart1", "El 20 de abril de 2022, el Modpack Relhax cesó su desarrollo. Quiero dar un agradecimiento personal a todos nuestros colaboradores y usuarios por unos exitosos 5+ años.");
            Spanish.Add("endOfLifeMessagePart2a", "El 1 de enero de 2017, me propuse un desafío de no sólo recrear el Modpack OMC en una interfaz moderna, si no también crear el sistema de instalación de paquetes más rápido que haya existido en ningún Modpack.");
            Spanish.Add("endOfLifeMessagePart2b", "Empecé como un equipo de 4, con 3 miembros de OMC que querían contribuir al proyecto. Durante 4 años, he diseñado, contruído y reconstruido la aplicación del Modpack de la nada, gastando decenas de miles de horas.");
            Spanish.Add("endOfLifeMessagePart2c", "En cierto punto, el equipo creció a más de 8 personas procedentes de la mayoría de regiones de los servidores de WoT. Durante el proceso, crecí como programador, aprendí prácticas comunes en el desarrollo de software, y me especialicé en manipular operaciones concurrentes y multi-threading en aplicaciones.");
            Spanish.Add("endOfLifeMessagePart2d", "Gané experiencia a través del proyecto, y tuve la oportunidad de interactuar con una gran comunidad de modding. Me permitió colaborar de nuevo con el grupo Relic Gaming Community, al que me uní en 2014.");
            Spanish.Add("endOfLifeMessagePart3a", "A fecha de este año, finalmente completé mi trabajo de diseñar el instalador más optimizado y eficiente que podía hacer para la comunidad.");
            Spanish.Add("endOfLifeMessagePart3b", "Viendo el proyecto alcanzar su objetivo original, y mi interés en el juego (y el proyecto) disminuir, decidí cerrar el proyecto.");
            Spanish.Add("endOfLifeMessagePart3c", "Ha sido una decisión difícil, pero no quería seguir apoyando un proyecto que ya no tenía interés de mantener.");
            Spanish.Add("endOfLifeMessagePart3d", "Creo se habría reflejado de mala forma en la calidad del producto, y no sería justo para los usuarios finales. Quería cerrar el proyecto mientras aún estaba en buen estado.");
            Spanish.Add("endOfLifeMessagePart4", "De nuevo, gracias a todos. Han sido 5+ divertidos años, y lo echaré de menos.");
            #endregion
        }
    }
}
