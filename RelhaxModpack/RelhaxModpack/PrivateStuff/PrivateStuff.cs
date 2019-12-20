using System.Net;

namespace RelhaxModpack
{
#pragma warning disable CS1591
    public static class PrivateStuff
    {
        #region discord stuff
        public const string DiscordBotClientID = null;
        public const ulong DiscordModChannelID = 0;
        public const ulong DiscordAnnouncementsChannelID = 0;
        #endregion

        #region bigmods FTP modpack addresses
        public const string BigmodsFTPModpackRoot = null;
        public const string BigmodsFTPModpackMedias = null;
        public const string BigmodsFTPModpackRelhaxModpack = null;
        public const string BigmodsFTPRootWoT = null;
        public const string BigmodsFTPModpackResources = null;
        public const string BigmodsFTPModpackManager = null;
        public const string BigmodsFTPModpackDatabase = null;
        #endregion

        #region bigmods FTP users addresses
        public const string BigmodsFTPUsersRoot = null;
        public const string BigmodsFTPUsersMedias = null;
        #endregion

        #region bigmods HTTP addresses
        public const string BigmodsEnterDownloadStatPHP = null;
        public const string BigmodsCreateDatabasePHP = null;
        public const string BigmodsCreateManagerInfoPHP = null;
        public const string BigmodsCreateUpdatePackagesPHP = null;
        #endregion

        #region wotmods FTP all addresses
        public const string FTPModpackRoot = null;
        public const string FTPRescourcesRoot = null;
        public const string FTPManagerInfoRoot = null;
        public const string ModInfosLocation = null;
        public const string ModInfoBackupsFolderLocation = null;
        public const string KeyAddress = null;
        #endregion

        #region wotmods HTTP all addresses
        public const string CreateUpdatePackagesPHP = null;
        public const string CreateManagerInfoPHP = null;
        public const string CreateModInfoPHP = null;
        #endregion

        #region server credentials
        public static NetworkCredential BigmodsNetworkCredentialScripts = null;
        public static NetworkCredential BigmodsNetworkCredential = null;
        public static NetworkCredential WotmodsNetworkCredential = null;
        #endregion
    }
#pragma warning restore CS1591
}