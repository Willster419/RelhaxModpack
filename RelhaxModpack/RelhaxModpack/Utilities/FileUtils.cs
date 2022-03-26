using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Common;

namespace RelhaxModpack.Utilities
{
    /// <summary>
    /// A Utility class to deal with Files and Folders
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// A list of file size constructs from bytes to Yotabytes
        /// </summary>
        /// <remarks>{ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" }</remarks>
        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Multiply by this to convert bytes to megabytes
        /// </summary>
        public const long BYTES_TO_MBYTES = 1048576;

        #region Hash Utilities
        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static string CreateMD5Hash(string inputFile)
        {
            //return if arg is null or empty
            if (string.IsNullOrWhiteSpace(inputFile))
                return "-1";

            //return if the file does not exist
            if (!File.Exists(inputFile))
                return "-1";

            FileStream stream = null;
            string result = string.Empty;
            using (stream = File.OpenRead(inputFile))
            {
                result = CreateMD5Hash(stream);
            }

            if (result.Equals("-1"))
            {
                Logging.Error("Failed to check MD5 of file " + inputFile);
            }
            return result;
        }

        /// <summary>
        /// Creates an MD5 hash calculation from and stream object
        /// </summary>
        /// <param name="stream">The stream object to calculate from</param>
        /// <returns>The MD5 calculated hash</returns>
        /// <exception cref="ArgumentNullException"/>
        public static string CreateMD5Hash(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            //Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();
            MD5 md5Hash;
            try
            {
                using (md5Hash = MD5.Create())
                {
                    //Convert the input string to a byte array and compute the hash
                    byte[] data = md5Hash.ComputeHash(stream);
                    stream.Close();

                    //Loop through each byte of the hashed data 
                    //and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return "-1";
            }

            //Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static async Task<string> CreateMD5HashAsync(string inputFile)
        {
            //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            //https://stackoverflow.com/questions/38423472/what-is-the-difference-between-task-run-and-task-factory-startnew
            /*
                in the .NET Framework 4.5 Developer Preview, we’ve introduced the new Task.Run method. This in no way obsoletes Task.Factory.StartNew,
                but rather should simply be thought of as a quick way to use Task.Factory.StartNew without needing to specify a bunch of parameters.
                It’s a shortcut. In fact, Task.Run is actually implemented in terms of the same logic used for Task.Factory.StartNew, just passing in
                some default parameters. When you pass an Action to Task.Run:

                'Task.Run(someAction);'

                it's exactly equivalent to:

                'Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);'
             */
            return await Task.Run(() => CreateMD5Hash(inputFile));
        }

        /// <summary>
        /// Creates an MD5 hash calculation from and stream object
        /// </summary>
        /// <param name="stream">The stream object to calculate from</param>
        /// <returns>The MD5 calculated hash</returns>
        /// <exception cref="ArgumentNullException"/>
        public static async Task<string> CreateMD5HashAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return await Task.Run(() => CreateMD5Hash(stream));
        }
        #endregion

        #region Zip file utils
        /// <summary>
        /// Gets a zip file entry in the form of a string
        /// </summary>
        /// <param name="zipFilename">The path to the file in the zip</param>
        /// <param name="archivedFilename">the path to the zip file</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetStringFromZip(string zipFilename, string archivedFilename, string password = "")
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/92a36534-0f01-4425-ab63-c5f8830d64ae/help-please-with-dotnetzip-extracting-data-form-ziped-file?forum=csharpgeneral
            if (!File.Exists(zipFilename))
            {
                Logging.Error("Zip file {0} not found", zipFilename);
                return null;
            }
            using (ZipFile zip = ZipFile.Read(zipFilename))
                return GetStringFromZip(zip, archivedFilename, password);
        }

        /// <summary>
        /// Gets the string contents of a text based file inside a zip file
        /// </summary>
        /// <param name="zip">The zipfile to extract the entry from</param>
        /// <param name="archivedFilename">The archive path to the entry</param>
        /// <param name="password">The password to use when extracting the entry. Leave blank for no password</param>
        /// <returns></returns>
        public static string GetStringFromZip(ZipFile zip, string archivedFilename, string password = "")
        {
            //make sure the entry exists in the stream first
            if (!zip.ContainsEntry(archivedFilename))
            {
                Logging.Error("entry {0} does not exist in given zip file", archivedFilename);
                return null;
            }

            using (MemoryStream ms = new MemoryStream() { Position = 0 })
            using (StreamReader sr = new StreamReader(ms))
            {
                ZipEntry e = zip[archivedFilename];

                //if a password is provided, then use it for extraction
                if (!string.IsNullOrWhiteSpace(password))
                    e.ExtractWithPassword(ms, password);
                else
                    e.Extract(ms);

                //read stream
                ms.Position = 0;
                return sr.ReadToEnd();
            }
        }
        #endregion

        #region File Name and Size Utils
        /// <summary>
        /// Calculates and returns the size magnitude of the file (kilo, mega, giga...)
        /// </summary>
        /// <param name="bytes">The file size in bytes</param>
        /// <param name="decimalPlaces">The number of decimal places to maintain in the result</param>
        /// <param name="sizeSuffix">If it should return the byte symbol with the size amount (KB, MB, etc.)</param>
        /// <param name="ignoreSizeWarningIf0">If set to true, the application log will not show values about the passed in value for size calculation being 0. 
        /// File of 0 size, for example.</param>
        /// <returns>The string representation to decimalPlaces of the file size optionally with the bytes parameter</returns>
        public static string SizeSuffix(ulong bytes, uint decimalPlaces = 1, bool sizeSuffix = false, bool ignoreSizeWarningIf0 = false)
        {
            if (bytes == 0)
            {
                if (!ignoreSizeWarningIf0)
                    Logging.Warning("SizeSuffix value is 0 (is this the intent?)");
                if (sizeSuffix)
                    return "0.0 bytes";
                else
                    return "0.0";
            }

            if (bytes < 1000)
            {
                if (sizeSuffix)
                    return string.Format("{0:n" + decimalPlaces + "} {1}", 0.1, SizeSuffixes[1]);
                else
                    return string.Format("{0:n" + decimalPlaces + "}", 0.1);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(bytes, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, (int)decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            if (sizeSuffix)
                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            else
                return string.Format("{0:n" + decimalPlaces + "}", adjustedSize);
        }

        /// <summary>
        /// Gets the size of the file in bytes
        /// </summary>
        /// <param name="filepath">The string path to the file</param>
        /// <returns>The size of the file in bytes</returns>
        /// <remarks>This is a wrapper for the FileInfo.Length property</remarks>
        public static long GetFilesize(string filepath)
        {
            //https://stackoverflow.com/questions/1380839/how-do-you-get-the-file-size-in-c
            return new FileInfo(filepath).Length;
        }

        /// <summary>
        /// Checks if a filename has invalid characters and replaces them with underscores
        /// </summary>
        /// <param name="fileName">The filename to replace characters from</param>
        /// <returns>The filename with valid characters</returns>
        public static string GetValidFilename(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
        #endregion

        #region File and Directory Management
        /// <summary>
        /// Attempts to move a file from source to destination with numRetrys, with a file timeout of timeout
        /// </summary>
        /// <param name="source">The source file to move</param>
        /// <param name="destination">The destination of the file to move</param>
        /// <param name="numRetrys">The number of fail retries if it failes to move the file</param>
        /// <param name="timeout">The timeout, in milliseconds, to wait between faliures</param>
        /// <returns>True if the file was moved, false otherwise</returns>
        /// <remarks>This method does NOT work to move a file across physical drives. 
        /// This method does NOT check if the destination file already exists.</remarks>
        public static bool FileMove(string source, string destination, uint numRetrys = 3, uint timeout = 100)
        {
            bool overallSuccess = true;
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            uint retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    File.Move(source, destination);
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog(string.Format("move file {0} -> {1}, retryCount={2}, message:\n{3}", source, destination, retryCounter, ex.Message),
                        Logfiles.Application, LogLevel.Warning);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to move file {0} -> {1}", source, destination);
                        overallSuccess = false;
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Tries to delete a file from the given path
        /// </summary>
        /// <param name="file">The file to delete</param>
        /// <param name="numRetrys">The number of retires if an exception is encountered</param>
        /// <param name="timeout">The number of milliseconds between retries</param>
        /// <returns>True is the file operation was successful, false otherwise</returns>
        public static bool FileDelete(string file, uint numRetrys = 3, uint timeout = 100)
        {
            bool overallSuccess = true;
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            uint retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    File.Delete(file);
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                        Logfiles.Application, LogLevel.Warning);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete file {0}", file);
                        overallSuccess = false;
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Deletes files in a directory
        /// </summary>
        /// <param name="folderPath">The directory to delete files in</param>
        /// <param name="deleteSubfolders">Toggle if the method should recursively look inside directory</param>
        /// <param name="deleteRoot">Toggle if the method should delete the folderPath directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <returns>True if the complete operation was a success, false otherwise</returns>
        public static bool DirectoryDelete(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            bool overallSuccess = true;
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            uint retryCounter;
            foreach (string file in Directory.GetFiles(folderPath, pattern, SearchOption.TopDirectoryOnly))
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        File.Delete(file);
                        retryCounter = numRetrys;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                            Logfiles.Application, LogLevel.Warning);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if (retryCounter == numRetrys)
                        {
                            Logging.Error("retries = counter, fully failed to delete file {0}", file);
                            overallSuccess = false;
                        }
                    }
                }
            }
            //if deleting the sub directories
            if (deleteSubfolders)
            {
                foreach (string dir in Directory.GetDirectories(folderPath, pattern, SearchOption.TopDirectoryOnly))
                {
                    if (!DirectoryDelete(dir, deleteSubfolders, true, numRetrys, timeout))
                        overallSuccess = false;
                }
            }
            //delete the folder as well (if requested)
            if (deleteRoot)
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        Directory.Delete(folderPath);
                        retryCounter = numRetrys;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0} (empty folder), retryCount={1}, message:\n{2}", folderPath, retryCounter, ex.Message),
                            Logfiles.Application, LogLevel.Error);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if (retryCounter == numRetrys)
                        {
                            Logging.Debug("retries = counter, fully failed to delete file {0}", folderPath);
                            overallSuccess = false;
                        }
                    }
                }
            }
            return overallSuccess;
        }

        /// <summary>
        /// Async wrapper around DirectoryDelete() method. Deletes files in a directory
        /// </summary>
        /// <param name="folderPath">The directory to delete files in</param>
        /// <param name="deleteSubfolders">Toggle if the method should recursively look inside directory</param>
        /// <param name="deleteRoot">Toggle if the method should delete the folderPath directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <returns>True if the complete operation was a success, false otherwise</returns>
        public static async Task<bool> DirectoryDeleteAsync(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            return await Task.Run(() => DirectoryDelete(folderPath, deleteSubfolders, deleteRoot, numRetrys, timeout, pattern));
        }

        /// <summary>
        /// Move a directory and its files to a new location. Works across drive letters.
        /// </summary>
        /// <param name="source">The source path of the directory to move from</param>
        /// <param name="destination">The destination path of the directory to move to</param>
        /// <param name="recursive">Toggle if the sub-folders and files should be moved as well</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        /// <remarks>The DirectoryMove method works across drive letters and other physical separate drives because it deletes and re-creates folders rather then trying to move them</remarks>
        public static void DirectoryMove(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //make the destination if it does not already exist
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            //DirectoryMove works by getting a directory list of all directories in the source to create,
            //then making the directories, moving the files, and then deleting the old directories
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            //create them at the target
            foreach (string fullPath in directoreisToCreate)
            {
                //trim out the base path so we only have the new path left
                string partPath = fullPath.Substring(source.Length + 1);
                string newPath = Path.Combine(destination, partPath);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            //move the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in filesToMove)
            {
                string partPath = file.Substring(source.Length + 1);
                string newPath = Path.Combine(destination, partPath);
                File.Move(file, newPath);
            }

            //delete all the other old empty source directories
            directoreisToCreate.Sort();
            directoreisToCreate.Reverse();
            foreach (string fullPath in directoreisToCreate)
            {
                if (Directory.Exists(fullPath))
                {
                    if (Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories).Count() > 0)
                        throw new BadMemeException("waaaaaaa?");
                    Directory.Delete(fullPath);
                }
            }
        }

        /// <summary>
        /// Copy a directory and its files to a new location
        /// </summary>
        /// <param name="source">The source path of the directory to move from</param>
        /// <param name="destination">The destination path of the directory to move to</param>
        /// <param name="recursive">Toggle if the sub-folders and files should be moved as well</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="pattern">The pattern of files to search for in a directory</param>
        public static void DirectoryCopy(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            //create them at the target
            foreach (string s in directoreisToCreate)
            {
                if (!Directory.Exists(Path.Combine(destination, s)))
                {
                    Directory.CreateDirectory(Path.Combine(destination, s));
                }
            }
            //copy the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in filesToMove)
            {
                File.Copy(Path.Combine(source, file), Path.Combine(destination, file));
            }
        }

        /// <summary>
        /// Return a list of files from a directory, including their paths.
        /// </summary>
        /// <param name="directoryPath">The directory to search for files</param>
        /// <param name="option">Specifies to search this top directory or subdirectories to the Directory.GetFiles() method</param>
        /// <param name="includeDirectoryRoot">Toggle if the directoryPath should be included in the list of files</param>
        /// <param name="filesOnly">Toggle if the returned list should be pre-filtered for only have files (no directories)</param>
        /// <param name="searchPattern">The search pattern for finding files in a directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="applydirectoryAttributeToRoot">Toggle if the "Normal" file property as assigned to these files at the same time</param>
        /// <returns>The list of files if the search operation was successful, otherwise null</returns>
        public static string[] FileSearch(string directoryPath, SearchOption option, bool includeDirectoryRoot, bool filesOnly, string searchPattern = "*",
            uint timeout = 5, uint numRetrys = 3, bool applydirectoryAttributeToRoot = true)
        {
            //filter input
            if (numRetrys == 0)
            {
                Logging.Warning("numRetrys needs to be larger than 0! setting to 1");
                numRetrys++;
            }

            //loop for how many times to try (in case the OS herped a derp, for example)
            while (numRetrys > 0)
            {
                //if a timout is requested, then sleep the thread
                if (timeout > 0)
                    System.Threading.Thread.Sleep((int)timeout);

                //put it in a try catch block
                try
                {
                    //verify the folder to search exists
                    if (!Directory.Exists(directoryPath))
                    {
                        Logging.WriteToLog(string.Format("Path {0} does not exist!", directoryPath), Logfiles.Application, LogLevel.Warning);
                        return null;
                    }

                    //apply the directory attribute to the directory folder
                    if (applydirectoryAttributeToRoot)
                        File.SetAttributes(directoryPath, FileAttributes.Directory);

                    //do the actual file search
                    List<string> files = Directory.GetFiles(directoryPath, searchPattern, option).ToList();

                    //if requested, filter out any folders
                    if (filesOnly)
                        files = files.Where(item => File.Exists(item)).ToList();

                    //if requested, add the directory root to the list
                    if (includeDirectoryRoot)
                        files.Insert(0, directoryPath);

                    return files.ToArray();
                }
                catch (Exception e)
                {
                    //decreate the number of times we will retry to get the files
                    numRetrys--;
                    if (numRetrys == 0)
                    {
                        //give up; report it and move on
                        Logging.WriteToLog(string.Format("Failed to get files fo directory {0}\n{1}", Path.GetFullPath(directoryPath), e.ToString()),
                            Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("Failed to get files for direcotry {0}\nThis is attempt {1} of 0",
                            Path.GetFullPath(directoryPath), numRetrys), Logfiles.Application, LogLevel.Warning);
                    }
                }
            }
            Logging.WriteToLog("Code should not reach this point: FileUtils.FileSearch()", Logfiles.Application, LogLevel.Error);
            return null;
        }

        /// <summary>
        /// Return a list of directories from a directory
        /// </summary>
        /// <param name="directoryPath">The directory to search for files</param>
        /// <param name="option">Specifies to search this top directory or subdirectories to the Directory.GetFiles() method</param>
        /// <param name="includeDirectoryRoot">Toggle if the directoryPath should be included in the list of files</param>
        /// <param name="searchPattern">The search pattern for finding files in a directory</param>
        /// <param name="numRetrys">The number of retires to delete a file entry before failing</param>
        /// <param name="timeout">The time in milliseconds between retries</param>
        /// <param name="applydirectoryAttributeToRoot">Toggle if the "Normal" file property as assigned to these files at the same time</param>
        /// <returns>The list of files if the search operation was successful, otherwise null</returns>
        public static string[] DirectorySearch(string directoryPath, SearchOption option, bool includeDirectoryRoot, string searchPattern = "*",
            uint timeout = 5, uint numRetrys = 3, bool applydirectoryAttributeToRoot = true)
        {
            //filter input
            if (numRetrys == 0)
            {
                Logging.Warning("numRetrys needs to be larger than 0! setting to 1");
                numRetrys++;
            }

            //loop for how many times to try (in case the OS herped a derp, for example)
            while (numRetrys > 0)
            {
                //if a timout is requested, then sleep the thread
                if (timeout > 0)
                    System.Threading.Thread.Sleep((int)timeout);

                //put it in a try catch block
                try
                {
                    //verify the folder to search exists
                    if (!Directory.Exists(directoryPath))
                    {
                        Logging.WriteToLog(string.Format("Path {0} does not exist!", directoryPath), Logfiles.Application, LogLevel.Warning);
                        return null;
                    }

                    //apply the directory attribute to the directory folder
                    if (applydirectoryAttributeToRoot)
                        File.SetAttributes(directoryPath, FileAttributes.Directory);

                    //add the directory path itself to the search
                    List<string> directories = Directory.GetDirectories(directoryPath, searchPattern, option).ToList();

                    //filter out any files
                    directories = directories.Where(item => Directory.Exists(item)).ToList();

                    //if requested, add the directory root to the list
                    if (includeDirectoryRoot)
                        directories.Insert(0, directoryPath);

                    return directories.ToArray();
                }
                catch (Exception e)
                {
                    //decreate the number of times we will retry to get the files
                    numRetrys--;
                    if (numRetrys == 0)
                    {
                        //give up; report it and move on
                        Logging.WriteToLog(string.Format("Failed to get files fo directory {0}\n{1}", Path.GetFullPath(directoryPath), e.ToString()),
                            Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("Failed to get files for direcotry {0}\nThis is attempt {1} of 0",
                            Path.GetFullPath(directoryPath), numRetrys), Logfiles.Application, LogLevel.Warning);
                    }
                }
            }
            Logging.WriteToLog("Code shuld not reach this point: FileUtils.DirectorySearch()", Logfiles.Application, LogLevel.Error);
            return null;
        }

        /// <summary>
        /// Applies the "Normal" file attribute to a file
        /// </summary>
        /// <param name="file">The file to apply normal attributes to</param>
        public static void ApplyNormalFileProperties(string file)
        {
            //check to make sure it's eithor a file or folder
            if (!File.Exists(file) && !Directory.Exists(file))
            {
                Logging.WriteToLog("file/folder does not exist " + file, Logfiles.Application, LogLevel.Error);
                return;
            }
            try
            {
                FileAttributes attribute = File.GetAttributes(file);
                if (attribute != FileAttributes.Normal)
                {
                    Logging.WriteToLog(string.Format("file {0} has FileAttribute {1}, setting to FileAttributes.Normal",
                        file, attribute.ToString()), Logfiles.Application, LogLevel.Debug);
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
            catch (Exception e)
            {
                Logging.WriteToLog("Failed to apply normal attribute\n" + e.ToString(), Logfiles.Application, LogLevel.Exception);
                return;
            }
        }

        /// <summary>
        /// Deletes any empty directories from a given path
        /// </summary>
        /// <param name="startLocation">The location to start from. Includes deleting empty directories from this point</param>
        /// <param name="recursive">Toggle to check inside the starting location for empty folders</param>
        /// <param name="numRetrys">The number of times the method should retry after receiving an exception</param>
        /// <param name="timeout">The time to wait between retries</param>
        /// <returns>True if the operation completed successfully, false otherwise</returns>
        public static bool ProcessEmptyDirectories(string startLocation, bool recursive, uint numRetrys = 3, uint timeout = 100)
        {
            //if the root does not exist then stop now
            if (!Directory.Exists(startLocation))
            {
                Logging.Warning("start location {0} does not exist, skipping", startLocation);
                return true;
            }

            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }

            uint retryCounter = 0;
            if (recursive)
            {
                //get the list of all directories inside it, no need to recursively process
                List<string> directories = DirectorySearch(startLocation, SearchOption.AllDirectories, false).ToList().Where(direct => Directory.Exists(direct)).ToList();

                //sort and reverse the list to make longer paths on top to simulate recursively deleting from all the way down to up
                directories.Sort();
                directories.Reverse();

                //now can delete for each folder
                foreach (string directory in directories)
                {
                    retryCounter = 0;
                    while (retryCounter < numRetrys)
                    {
                        try
                        {
                            if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                            {
                                Logging.Debug("Deleting empty directory {0}", directory);
                                Directory.Delete(directory, false);
                            }
                            retryCounter = numRetrys;
                        }
                        catch (Exception ex)
                        {
                            Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", directory, retryCounter, ex.Message);
                            retryCounter++;
                            System.Threading.Thread.Sleep((int)timeout);
                            if (retryCounter == numRetrys)
                            {
                                Logging.Error("retries = counter, fully failed to delete directory {0}", directory);
                                return false;
                            }
                        }
                    }
                }
            }

            //and process the root
            retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    if (Directory.GetFiles(startLocation).Length == 0 && Directory.GetDirectories(startLocation).Length == 0)
                    {
                        Logging.Debug("Deleting empty directory {0}", startLocation);
                        Directory.Delete(startLocation, false);
                    }
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", startLocation, retryCounter, ex.Message);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete directory {0}", startLocation);
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Other
        /// <summary>
        /// Removes the directory character and Wots 'win32' and/or 'win64' directories if it exists in the string
        /// </summary>
        /// <param name="wotPath">The path to the WoT exe</param>
        /// <returns>The absolute directory path to the World_of_Tanks folder</returns>
        /// <remarks>This is for in case the user specifies the WoT exe inside the win32 and/or win64 folders</remarks>
        public static string RemoveWoT32bit64bitPathIfExists(string wotPath)
        {
            return wotPath.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
        }

        /// <summary>
        /// Copies a file from one path or in an archive to a destination
        /// </summary>
        /// <param name="package">The zip archive to extract the file from</param>
        /// <param name="sourceCompletePath">The complete path to the file. Could be a path on disk, or a path in a zip archive</param>
        /// <param name="destinationCompletePath">The complete path to copy the destination file to</param>
        public static void Unpack(string package, string sourceCompletePath, string destinationCompletePath)
        {
            string destinationFilename = Path.GetFileName(destinationCompletePath);
            string destinationDirectory = Path.GetDirectoryName(destinationCompletePath);

            //if the package entry is empty, then it's just a file copy
            if (string.IsNullOrWhiteSpace(package))
            {
                if (File.Exists(sourceCompletePath))
                {
                    File.Copy(sourceCompletePath, destinationCompletePath);
                    Logging.Info("file copied");
                }
                else
                    Logging.Error("the file at path '{0} does not exist", sourceCompletePath);
            }
            else
            {
                if (!File.Exists(package))
                {
                    Logging.Error("packagefile does not exist, skipping");
                    return;
                }
                using (ZipFile zip = new ZipFile(package))
                {
                    //get the files that match the specified path from the Xml entry
                    string zipPath = sourceCompletePath.Replace(@"\", @"/");
                    ZipEntry[] matchingEntries = zip.Where(zipp => zipp.FileName.Equals(zipPath)).ToArray();
                    Logging.Debug("matching zip entries: {0}", matchingEntries.Count());
                    if (matchingEntries.Count() > 0)
                    {
                        foreach (ZipEntry entry in matchingEntries)
                        {
                            //change the name to the destination
                            entry.FileName = destinationFilename;

                            //extract to disk and log
                            entry.Extract(destinationDirectory, ExtractExistingFileAction.DoNotOverwrite);
                            Logging.Info("entry extracted: {0}", destinationFilename);
                        }
                    }
                    else
                        Logging.Warning("no matching zip entries for file: {0}", zipPath);
                }
            }
        }
        #endregion

        #region Special Folder stuff
        //https://stackoverflow.com/a/21953690/3128017
        private static string[] _knownFolderGuids = new string[]
            {
                "{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
                "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
                "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
                "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
                "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
                "{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
                "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
                "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
                "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
                "{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
                "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
            };

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetSpecialFolderPath(KnownFolder knownFolder)
        {
            return GetSpecialFolderPath(knownFolder, false);
        }

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetSpecialFolderPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetSpecialFolderPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);
        }

        private static string GetSpecialFolderPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
        {
            int result = SHGetKnownFolderPath(new Guid(_knownFolderGuids[(int)knownFolder]), (uint)flags, new IntPtr(defaultUser ? -1 : 0), out IntPtr outPath);
            if (result >= 0)
            {
                string path = Marshal.PtrToStringUni(outPath);
                Marshal.FreeCoTaskMem(outPath);
                return path;
            }
            else
            {
                throw new ExternalException("Unable to retrieve the known folder path. It may not be available on this system.", result);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        #endregion
    }
}
