using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class AutomationTask : IXmlSerializable, IComponentWithID
    {
        #region Xml serialization
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(ID) };
        }

        public virtual string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }

        public static Dictionary<string, Type> TaskTypeMapper { get; } = new Dictionary<string, Type>()
        {
            { DownloadStaticTask.TaskCommandName, typeof(DownloadStaticTask) }
        };

        public const string AttributeNameForMapping = "Command";
        #endregion //Xml serialization

        protected Stopwatch ExecutionTimeStopwatch = new Stopwatch();

        public AutomationSequence AutomationSequence { get; set; }

        public DatabaseAutomationRunner DatabaseAutomationRunner { get { return AutomationSequence.DatabaseAutomationRunner; } }

        public List<AutomationMacro> Macros { get { return AutomationSequence.MacrosListForTask; } }

        public string ErrorMessage { get; protected set; } = string.Empty;

        public int ExitCode { get; protected set; } = 0;

        public abstract string Command { get; }

        public string ID { get; set; } = string.Empty;

        public long ExecutionTimeProcessMacrosMs { get; protected set; } = 0;

        public long ExecutionTimeValidateCommandsMs { get; protected set; } = 0;

        public long ExecutionTimeRunTaskMs { get; protected set; } = 0;

        public long ExecutionTimeProcessTaskResultsMs { get; protected set; } = 0;

        public long ExecutionTimeMs
        {
            get
            {
                return ExecutionTimeValidateCommandsMs + ExecutionTimeRunTaskMs + ExecutionTimeProcessTaskResultsMs;
            }
        }

        public string ComponentInternalName { get { return ID; } }

        public virtual void PreProcessingHook()
        {
            //stub
        }

        public abstract void ValidateCommands();

        public abstract void ProcessMacros();

        public abstract Task RunTask();

        public abstract void ProcessTaskResults();

        public virtual bool EvaluateResults(string state)
        {
            if (ExitCode != 0)
            {
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Error in task {0} execution! Exit code {1}, ErrorMessage: {2}", Command, ExitCode, string.IsNullOrEmpty(ErrorMessage) ? "(empty)" : ErrorMessage);
                return false;
            }
            else if (ExitCode == -1)
            {
                Logging.AutomationRunner("BadMemeException: ExitCode result is -1. This could indicate an error with the task API. Please report this error to the developer.", LogLevel.Exception);
                Logging.GetLogfile(Logfiles.AutomationRunner).Write(ErrorMessage);
                return false;
            }
            else
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Task: {0} State: {1}, ExitCode: {2}", Command, state, ExitCode);
                return true;
            }
        }

        public async Task Execute()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: Task start");

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessMacros() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                ProcessMacros();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeProcessMacrosMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ValidateCommands() finish, ExecutionTime: {1}", Command, ExecutionTimeProcessMacrosMs.ToString());
            if (!EvaluateResults("ProcessMacros"))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ValidateCommands() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                ValidateCommands();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeValidateCommandsMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ValidateCommands() finish, ExecutionTime: {1}", Command, ExecutionTimeValidateCommandsMs.ToString());
            if (!EvaluateResults("ValidateCommands"))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: RunTask() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                await RunTask();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeRunTaskMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: RunTask() finish, ExecutionTime: {1}", Command, ExecutionTimeRunTaskMs.ToString());
            if (!EvaluateResults("RunTask"))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessTaskResults() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                ProcessTaskResults();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeProcessTaskResultsMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessTaskResults() finish, ExecutionTime: {1}", Command, ExecutionTimeProcessTaskResultsMs.ToString());
            ExecutionTimeStopwatch.Stop();
            if (!EvaluateResults("RunTask"))
                return;
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Finished task {0}: Task end, ExecutionTimeMs: {1}", Command, ExecutionTimeMs.ToString());
        }

        protected string ProcessMacro(string argName, string arg)
        {
            //set property value to temp, making new variable
            string temp = arg;
            //use that new temp value's reference to pass into the real ProcessMacro
            ProcessMacro(argName, ref temp);
            //and return the string value
            return temp;
        }

        protected void ProcessMacro(string argName, ref string arg)
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Processing arg '{0}'", argName);
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Before processing: '{0}'", arg);

            //run regex on the arg to get the type of replacement to do. if it's recursive, then we need to process the inner one first
            Match result = Regex.Match(arg, AutomationMacro.MacroReplaceRegex);
            if (!result.Success)
            {
                //check if any "{" exist at all
                Match startBracketsMatch = Regex.Match(arg, "{");
                if (startBracketsMatch.Captures.Count > 0)
                {
                    Match endBracketsMatch = Regex.Match(arg, "}");
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Macros were detected in the command argument, but the syntax was incorrect. Most likely is the number of start and end brackets are unbalanced.");
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.None, "Examine the number of brackets starting and ending in the command, and try again. For debug, here's what was parsed:");
                    Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Command value: {0}", arg);
                    Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Start brackets count: {0}", startBracketsMatch.Captures.Count);
                    foreach (Capture capture in startBracketsMatch.Captures)
                    {
                        Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Capture location in string: {0}", capture.Index);
                    }
                    Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "End brackets count: {0}", endBracketsMatch.Captures.Count);
                    foreach (Capture capture in endBracketsMatch.Captures)
                    {
                        Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Capture location in string: {0}", capture.Index);
                    }
                }
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "The command {0} has no macros, continue", argName);
                return;
            }

            int inner1Count = result.Groups[AutomationMacro.RegexGroupInner1].Captures.Count;
            int inner2Count = result.Groups[AutomationMacro.RegexGroupInner2].Captures.Count;
            int inner3Count = result.Groups[AutomationMacro.RegexGroupInner3].Captures.Count;

            //verify that 2 and 3 have the same number
            if (inner2Count != inner3Count)
            {
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Inner2Count ({0}) != Inner3Count ({1})! The macro engine is not designed for this!", inner2Count, inner3Count);
                throw new NotImplementedException("soon tm");
                //return;
            }
            else if (inner2Count > inner1Count)
            {
                int countDifference = inner2Count - inner1Count;
                //this means that the regex must recurse x levels (the difference) into the string to solve the inner values first
                Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "Inner2Count ({0}) > Inner1Count ({1}), required to recuse {2} levels to solve inner macros", inner2Count, inner1Count, countDifference);

            }
            else
            {
                //macros are at the same level, we can just replace as we see them
                //use inner3 as the method to determine what's inside
                foreach (Capture capture in result.Groups[AutomationMacro.RegexGroupInner3].Captures)
                {
                    if (string.IsNullOrEmpty(capture.Value))
                        continue;

                    Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "Processing macro {0}, string location {1}, length {2}", capture.Value, capture.Index, capture.Length);
                    AutomationMacro resultMacro = Macros.Find(macro => macro.Name.Equals(capture.Value));
                    if (resultMacro == null)
                    {
                        Logging.Warning(Logfiles.AutomationRunner, LogOptions.None, "The macro with name '{0}', does not exist, skipping. (Is this intended?)", capture.Value);
                        continue;
                    }

                    //perform a single replace on the specified location of the string
                    //https://stackoverflow.com/a/6372134/3128017
                    Regex replaceRegex = new Regex("{" + capture.Value + "}");
                    arg = replaceRegex.Replace(arg, resultMacro.Value, 1);
                    Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "A single replace was done on the command string. Result: {0}", arg);
                }
            }

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "After processing: {0}", arg);
        }
    }
}
