using System;
using System.Text;
using System.IO;
using System.Windows;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Common;

namespace RelhaxModpack
{
    /// <summary>
    /// Delegate for allowing method callback when the logfile writes to disk
    /// </summary>
    /// <param name="sender">The logfile instance object</param>
    /// <param name="e">The message and log level event argument</param>
    public delegate void LoggingMessageWrite(object sender, LogMessageEventArgs e);

    /// <summary>
    /// Represents an instance of a log file used for writing important logging information to a log
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
        /// The date and time format for writing each line in the log file
        /// </summary>
        public string Timestamp { get; private set; }

        /// <summary>
        /// Flag for if the log file is receiving redirections from other log file
        /// </summary>
        public bool IsRedirecting { get; set; } = false;

        /// <summary>
        /// Returns true if the fileStream is not null and can be written to, false otherwise
        /// </summary>
        public bool CanWrite { get { return fileStream == null ? false : true; } }

        /// <summary>
        /// Gets or sets if this log file will write lots of diagnostic messages to the log file.
        /// </summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// The fileStream object to write/create the log file. Requires disposal support
        /// </summary>
        private FileStream fileStream;

        /// <summary>
        /// The event for when the logfile is written to
        /// </summary>
        public event LoggingMessageWrite OnLogfileWrite;

        private object lockerObject = new object();

        /// <summary>
        /// Create an instance of the log file
        /// </summary>
        /// <param name="filePath">The path to the file to create/open</param>
        /// <param name="timestamp">the date and time format to write for each log line</param>
        /// <param name="verbose">Flag if the logfile will be outputting diagnostic info</param>
        /// <remarks>The verbose value will be ignored if the Application is not a beta or alpha build.</remarks>
        public Logfile(string filePath, string timestamp, bool verbose)
        {
            Filepath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Filename = Path.GetFileName(Filepath);
            Timestamp = timestamp;
            VerboseLogging = verbose;
        }

        /// <summary>
        /// Initializes the log file
        /// </summary>
        /// <returns>True if successful initialization, false otherwise</returns>
        public bool Init(bool displayErrorIfFail)
        {
            if (fileStream != null)
                fileStream.Dispose();
            fileStream = null;
            try
            {
                fileStream = new FileStream(Filepath, FileMode.Append, FileAccess.Write);
            }
            catch (Exception ex)
            {
                if ((VerboseLogging || ApplicationConstants.ApplicationVersion != ApplicationVersions.Stable) && displayErrorIfFail)
                {
                    MessageBox.Show(ex.ToString());
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a line of text to the log file with the date and timestamp, and severity level of the message
        /// </summary>
        /// <param name="message">The line to write</param>
        /// <param name="logLevel">The level of severity of the log message</param>
        public void Write(string message, LogLevel logLevel)
        {
            //only stable application distributions should be able to *not* log debug messages
            if (ApplicationConstants.ApplicationVersion == ApplicationVersions.Stable && logLevel == LogLevel.Debug && !VerboseLogging)
                return;

            string logMessageLevel = string.Empty;
            switch(logLevel)
            {
                case LogLevel.Debug:
                    logMessageLevel = "DEBUG: ";
                    break;
                case LogLevel.Info:
                    logMessageLevel = "INFO: ";
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
            message = string.Format("{0}   {1}{2}", formattedDateTime, logMessageLevel, message);
            Write(message);
            OnLogfileWrite?.Invoke(this, new LogMessageEventArgs() { LogLevel = logLevel, Message = message });
        }

        /// <summary>
        /// Writes a line of text to the log file with the date and timestamp, and severity level of the message
        /// </summary>
        /// <param name="message">The message to write to the file</param>
        public void Write(string message)
        {
            //check for empty file paths or messages first
            if (string.IsNullOrEmpty(message))
                return;
            if (string.IsNullOrEmpty(Filepath))
                throw new BadMemeException("You're bad at log files");
            if (fileStream == null)
                return;

            message += Environment.NewLine;

            //actually write message to log
            lock (lockerObject)
            {
                fileStream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetByteCount(message));
                fileStream.Flush();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Dispose Managed and Unmanaged resources used for the log files
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
                if(fileStream != null)
                    fileStream.Dispose();
                fileStream = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose Managed and Unmanaged resources used for the log files
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
