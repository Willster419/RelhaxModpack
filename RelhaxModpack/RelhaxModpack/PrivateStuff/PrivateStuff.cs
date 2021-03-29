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

        //https://stackoverflow.com/questions/9418404/cant-connect-to-ftp-553-file-name-not-allowed
        #region bigmods FTP modpack addresses
        //path of modpack acct is <REDACTED>
        public const string BigmodsFTPModpackRoot = null;
        public const string BigmodsFTPModpackInstallStats = null;
        public const string BigmodsFTPModpackMedias = null;
        public const string BigmodsFTPModpackRelhaxModpack = null;
        public const string BigmodsFTPRootWoT = null;
        public const string BigmodsFTPModpackResources = null;
        public const string BigmodsFTPModpackManager = null;
        public const string BigmodsFTPModpackDatabase = null;
        #endregion

        #region bigmods FTP user addresses
        //path of users acct is <REDACTED>
        public const string BigmodsFTPUsersRoot = null;
        public const string BigmodsFTPUsersMedias = null;
        #endregion

        #region bigmods HTTP all addresses
        public const string BigmodsEnterDownloadStatPHP = null;
        public const string BigmodsCreateDatabasePHP = null;
        public const string BigmodsCreateManagerInfoPHP = null;
        public const string BigmodsCreateUpdatePackagesPHP = null;
        public const string BigmodsCreateModInfoPHP = null;
        public const string BigmodsTriggerManualMirrorSyncPHP = null;
        public const string BigmodsModpackUpdaterKey = null;
        #endregion

        #region server credentials
        public static NetworkCredential BigmodsNetworkCredentialScripts = null;
        public static NetworkCredential BigmodsNetworkCredentialPrivate = null;
        public static NetworkCredential BigmodsNetworkCredential = null;
        #endregion
    }
#pragma warning restore CS1591
}