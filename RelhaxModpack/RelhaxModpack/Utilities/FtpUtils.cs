using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities
{
    /// <summary>
    /// A Utility class for handling FTP connections
    /// </summary>
    public static class FtpUtils
    {
        /// <summary>
        /// Create an FTP folder
        /// </summary>
        /// <param name="addressWithDirectory">The complete path to the folder to create</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static void FtpMakeFolder(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        /// <summary>
        /// Create an FTP folder
        /// </summary>
        /// <param name="addressWithDirectory">The complete path to the folder to create</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static async Task FtpMakeFolderAsync(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse webResponse = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        /// <summary>
        /// Get a list of files currently in an FTP folder
        /// </summary>
        /// <param name="address">The complete path to the FTP folder</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The list of files on the server, as well as the current directory "." and parent directory ".." characters</returns>
        public static string[] FtpListFilesFolders(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string temp = reader.ReadToEnd();
                    return temp.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        /// <summary>
        /// Get a list of files currently in an FTP folder
        /// </summary>
        /// <param name="address">The complete path to the FTP folder</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The list of files on the server, as well as the current directory "." and parent directory ".." characters</returns>
        public static async Task<string[]> FtpListFilesFoldersAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string temp = reader.ReadToEnd();
                    return temp.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        /// <summary>
        /// Delete a file on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static void FtpDeleteFile(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        /// <summary>
        /// Delete a file on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static async Task FtpDeleteFileAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        /// <summary>
        /// Delete a folder on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static void FtpDeleteFolder(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
            folderRequest.Credentials = credentials;
            List<string> files = FtpListFilesFolders(address, credentials).ToList();
            files.RemoveAll(m => m.Equals("..") || m.Equals("."));
            if (files.Count > 0)
            {
                string address_ = address;
                if (address.Last().Equals('/'))
                    address_ = address;
                else
                    address_ = address + "/";

                foreach (string filename in files)
                {
                    FtpDeleteFile(address_ + filename, credentials);
                }
            }
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        /// <summary>
        /// Delete a folder on an FTP server
        /// </summary>
        /// <param name="address">The complete path to the FTP file to delete</param>
        /// <param name="credentials">The FTP server credentials</param>
        public static async Task FtpDeleteFolderAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
            folderRequest.Credentials = credentials;
            //https://stopbyte.com/t/how-to-remove-a-non-empty-folder-on-an-ftp/294/2
            //must be done recursively cause it won't delete non-empty folders
            List<string> files = (await FtpListFilesFoldersAsync(address, credentials)).ToList();
            files.RemoveAll(m => m.Equals("..") || m.Equals("."));
            if (files.Count > 0)
            {
                string address_ = address;
                if (address.Last().Equals('/'))
                    address_ = address;
                else
                    address_ = address + "/";

                foreach (string filename in files)
                {
                    await FtpDeleteFileAsync(address_ + filename, credentials);
                }
            }
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        /// <summary>
        /// Get a file size of an FTP file
        /// </summary>
        /// <param name="address">The complete path to the FTP file</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The size of the file in bytes</returns>
        public static long FtpGetFilesize(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                result = response.ContentLength;
            }
            return result;
        }

        /// <summary>
        /// Get a file size of an FTP file
        /// </summary>
        /// <param name="address">The complete path to the FTP file</param>
        /// <param name="credentials">The FTP server credentials</param>
        /// <returns>The size of the file in bytes</returns>
        public static async Task<long> FtpGetFilesizeAsync(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse) await request.GetResponseAsync())
            {
                result = response.ContentLength;
            }
            return result;
        }

        /// <summary>
        /// Trigger an Rsync update of our download mirror(s) to get the latest versions of our zip files from our main download mirror.
        /// </summary>
        /// <returns>True if the triggering of an rsync update was successful, fail otherwise.</returns>
        public static async Task<bool> TriggerMirrorSyncAsync()
        {
            using (PatientWebClient client = new PatientWebClient()
            { Credentials = PrivateStuff.BigmodsNetworkCredentialScripts, Timeout = 100000 })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsTriggerManualMirrorSyncPHP);
                    Logging.Info(result.Replace("<br />", "\n"));
                    if (result.ToLower().Contains("trigger=1"))
                        return true;
                }
                catch (WebException wex)
                {
                    Logging.Error("Failed to run trigger manual sync script");
                    Logging.Exception(wex.ToString());
                    return false;
                }
            }
            return false;
        }
    }
}
