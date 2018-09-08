using System;
using System.Text;
using System.IO;

namespace RelhaxModpack
{
    /// <summary>
    /// Represenets an instance of a logfile used for writing important logging information to a log
    /// </summary>
    public class Logfile : IDisposable
    {
        /// <summary>
        /// The path to the file the log is writing to
        /// </summary>
        public string Filepath { get; private set; }
        /// <summary>
        /// The name of the file that the log is writing to
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// The date and time format for writing each line in the logfile
        /// </summary>
        public string Timestamp { get; private set; }
        //The filestream object to write/create the logfile. Requires disposal support
        private FileStream fileStream;
        /// <summary>
        /// Create an instance of the logfile
        /// </summary>
        /// <param name="filePath">The path to the file to create/open</param>
        /// <param name="timestamp">the date and time format to write for each log line</param>
        public Logfile(string filePath, string timestamp)
        {
            Filepath = filePath;
            Filename = Path.GetFileName(Filepath);
            Timestamp = timestamp;
        }
        /// <summary>
        /// Initializes the logfile
        /// </summary>
        /// <returns>True if sucessfull initialization, false otherwise</returns>
        public bool Init()
        {
            if (fileStream != null)
                fileStream.Dispose();
            fileStream = null;
            try
            {
                fileStream = new FileStream(Logging.ApplicationLogFilename, FileMode.Append, FileAccess.Write);
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Writes a line of text to the log file with the date and timestamp, and level of severity of the message
        /// </summary>
        /// <param name="message">The line to write</param>
        /// <param name="logLevel">The level of severity of the log message. Default is info level.</param>
        public void Write(string message, LogLevel logLevel = LogLevel.Info)
        {
            //check for empty filepaths or messages first
            if (string.IsNullOrEmpty(message))
                return;
            if (string.IsNullOrEmpty(Filepath))
                throw new BadMemeException("You're bad at logfiles");
            if (fileStream == null)
                throw new BadMemeException("You're still bad at logfiles");
            string logMessageLevel = string.Empty;
            switch(logLevel)
            {
                case LogLevel.Debug:
                    logMessageLevel = "DEBUG: ";
                    break;
                case LogLevel.Warning:
                    logMessageLevel = "WARNING: ";
                    break;
                case LogLevel.Error:
                    logMessageLevel = "ERROR: ";
                    break;
                case LogLevel.Exception:
                    logMessageLevel = "EXCEPTION: ";
                    break;
                case LogLevel.ApplicationHalt:
                    logMessageLevel = "CRITICAL APPLICATION FAILURE: ";
                    break;
            }
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            string formattedDateTime = DateTime.Now.ToString(Timestamp);
            message = string.Format("{0}   {1}{2}\r\n", formattedDateTime, logMessageLevel, message);
            fileStream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetByteCount(message));
            fileStream.Flush();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Dispose Managed and Unmanaged rescources used for the logfiles
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                fileStream.Dispose();
                fileStream = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Logfile() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose Managed and Unmanaged rescources used for the logfiles
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
