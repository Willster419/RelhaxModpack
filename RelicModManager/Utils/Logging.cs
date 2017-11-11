using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    class Logging
    {
        private const int all = 0;
        private const int manager = 1;
        private const int installer = 2;

        private const int iMaxLogLength = 1500000; // Probably should be bigger, say 2,000,000
        private const int iTrimmedLogLength = -300000; // minimum of how much of the old log to leave

        private static FileStream mfs;             // Manager stream
        private static FileStream ifs;             // Installer stream
        private static string InstalledFilesLogPath = null;
        private static string ManagerLogPath = null;

        private static object _locker = new object();

        public static void Manager(string s)
        {
            try
            {
                lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
                {
                    //the method should automaticly make the file if it's not there
                    if (mfs == null)
                    {
                        if (ManagerLogPath == null) DefineManagerLogFile();
                        CutLogfile(ManagerLogPath);
                        mfs = new FileStream(ManagerLogPath, FileMode.Append, FileAccess.Write);
                    }
                    //if the info text is containing any linefeed/carrieage return, intend the next line with 26 space char
                    s = s.Replace("\n", "\n" + string.Concat(Enumerable.Repeat(" ", 26)));
                    s = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}   {1}\n", DateTime.Now, s);
                    mfs.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
                    mfs.Flush();        // to get every entry directly is important
                }
            }
            catch
            {
                // no target to write to .... create a window?
            }
        }

        public static void Installer(string s)
        {
            try
            {
                lock (_locker)              // avoid that 2 or more threads calling the Log function and writing lines in a mess
                {
                    if (ifs == null)
                    {
                        if (InstalledFilesLogPath == null) DefineInstallerLogFile();
                        //start the entry for the files log
                        ifs = new FileStream(InstalledFilesLogPath, FileMode.Append, FileAccess.Write);
                        string databaseHeader = string.Format("Database Version: {0}\n", Settings.DatabaseVersion);
                        string dateTimeHeader = string.Format("/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */\n", DateTime.Now);
                        ifs.Write(Encoding.UTF8.GetBytes(databaseHeader), 0, Encoding.UTF8.GetByteCount(databaseHeader));
                        ifs.Write(Encoding.UTF8.GetBytes(dateTimeHeader), 0, Encoding.UTF8.GetByteCount(dateTimeHeader));
                    }
                    s = string.Format("{0}\n", s);
                    ifs.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("Installer", ex);
            }
        }

        public static void InstallerFinished()
        {
            mfs.Flush();
            Dispose(installer);
        }

        public static void InstallerGroup(string s)
        {
            Installer(string.Format(@"/*  {0}  */", s));
        }

        private static void DefineManagerLogFile()
        {
            ManagerLogPath = Path.Combine(Application.StartupPath, "RelHaxLog.txt");
        }

        private static void DefineInstallerLogFile()
        {
            InstalledFilesLogPath = Path.Combine(Settings.TanksLocation, "logs", "installedRelhaxFiles.log");
            InstallerBackup();
        }

        private static void InstallerBackup()
        {
            //start the entry for the database version in installedRelhaxFiles.log
            try
            {
                if (InstalledFilesLogPath == null) DefineInstallerLogFile();
                //if a current log and backup log exist
                if (File.Exists(InstalledFilesLogPath + ".bak") && File.Exists(InstalledFilesLogPath))
                {
                    //current becomes backup, backup is deleted
                    File.Delete(InstalledFilesLogPath + ".bak");
                    File.Move(InstalledFilesLogPath, InstalledFilesLogPath + ".bak");
                }
                //only log exists
                else if (File.Exists(InstalledFilesLogPath))
                {
                    //move it to be a backup
                    File.Move(InstalledFilesLogPath, InstalledFilesLogPath + ".bak");
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("InstallerBackup", ex);
            }
        }

        private static void CutLogfile(string strFile)
        {
            try
            {
                // bigger logfile size at testing and developing
                int multi = 1;
                if (Program.testMode) multi = 100;

                FileInfo fi = new FileInfo(strFile);

                Byte[] bytesSavedFromEndOfOldLog = null;

                if (fi.Length > iMaxLogLength* multi) // if the log file length is already too long
                {
                    using (BinaryReader br = new BinaryReader(File.Open(strFile, FileMode.Open)))
                    {
                        // Seek to our required position of what you want saved.
                        br.BaseStream.Seek(iTrimmedLogLength* multi, SeekOrigin.End);

                        // Read what you want to save and hang onto it.
                        bytesSavedFromEndOfOldLog = br.ReadBytes((-1 * iTrimmedLogLength * multi));
                    }
                }
                else
                {
                    return;
                }

                byte[] newLine = System.Text.UTF8Encoding.UTF8.GetBytes(Environment.NewLine);

                FileStream fs = null;
                try
                {
                    // If the log file is less than the max length, just open it at the end to write there
                    if (fi.Length<iMaxLogLength* multi)
                        fs = new FileStream(strFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                    else // If the log file is more than the max length, just open it empty
                    {
                        fs = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                        // https://stackoverflow.com/questions/5266069/streamwriter-and-utf-8-byte-order-marks
                        // Creates the UTF-8 encoding with parameter "encoderShouldEmitUTF8Identifier" set to true
                        Encoding vUTF8Encoding = new UTF8Encoding(true);
                        // Gets the preamble in order to attach the BOM
                        var vPreambleByte = vUTF8Encoding.GetPreamble();
                        // Writes the preamble first
                        fs.Write(vPreambleByte, 0, vPreambleByte.Length);
                    }

                    using (fs)
                    {
                        // If you are trimming the file length, write what you saved. 
                        if (bytesSavedFromEndOfOldLog != null)
                        {
                            Byte[] lineBreak = Encoding.UTF8.GetBytes(string.Format("### {0:yyyy-MM-dd HH:mm:ss} *** *** *** Old Log Start Position *** *** *** *** ###", DateTime.Now));
                            fs.Write(lineBreak, 0, lineBreak.Length);
                            fs.Write(newLine, 0, newLine.Length);
                            fs.Write(bytesSavedFromEndOfOldLog, 0, bytesSavedFromEndOfOldLog.Length);
                            fs.Write(newLine, 0, newLine.Length);
                        }
                    }
                }
                finally
                {
                    fs.Dispose();
                }
            }
            catch
            {
                // Nothing to do...
            }
        }

        public static void Dispose(int i = all)
        {
            if (ifs != null || mfs != null)
            {
                //done with the installer filestream
                if (ifs != null && (i == installer || i == all))
                {
                    ifs.Flush();
                    ifs.Dispose();
                    ifs = null;
                    InstalledFilesLogPath = null;
                }

                //done with the manager filestream
                if (mfs != null && (i == installer || i == all))
                {
                    mfs.Flush();
                    mfs.Dispose();
                    mfs = null;
                    ManagerLogPath = null;
                }

                GC.Collect();
            }
        }
    }
}
