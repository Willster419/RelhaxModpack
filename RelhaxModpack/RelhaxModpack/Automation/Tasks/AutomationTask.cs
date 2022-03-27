using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// An AutomationTask represents an action to occur during an automation sequence, used for checking for and updating database package entries.
    /// </summary>
    public abstract class AutomationTask : IXmlSerializable, IComponentWithID
    {
        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(ID) };
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public virtual string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }

        /// <summary>
        /// Gets a dictionary that maps a unique task command name in xml to the type of task object in the code.
        /// </summary>
        public static Dictionary<string, Type> TaskTypeMapper { get; } = new Dictionary<string, Type>()
        {
            //download tasks
            { DownloadStaticTask.TaskCommandName, typeof(DownloadStaticTask) },
            { DownloadHtmlTask.TaskCommandName, typeof(DownloadHtmlTask) },
            { DownloadBrowserTask.TaskCommandName, typeof(DownloadBrowserTask) },

            //database tasks (load/save)
            { LoadDatabaseTask.TaskCommandName, typeof(LoadDatabaseTask) },
            { SaveDatabaseTask.TaskCommandName, typeof(SaveDatabaseTask) },

            //package transfer tasks
            { PackageDownloadTask.TaskCommandName, typeof(PackageDownloadTask) },
            { PackageUploadTask.TaskCommandName, typeof(PackageUploadTask) },

            //package property tasks
            { UpdatePackagePropertyTask.TaskCommandName, typeof(UpdatePackagePropertyTask) },
            { RetrievePackagePropertyTask.TaskCommandName, typeof(RetrievePackagePropertyTask) },

            //file tasks
            { FileCopyTask.TaskCommandName, typeof(FileCopyTask) },
            { FileDeleteTask.TaskCommandName, typeof(FileDeleteTask) },
            { FileMoveTask.TaskCommandName, typeof(FileMoveTask) },
            { FileExistsTask.TaskCommandName, typeof(FileExistsTask) },

            //compare tasks
            { StartCompareTask.TaskCommandName, typeof(StartCompareTask) },
            { EndCompareTask.TaskCommandName, typeof(EndCompareTask) },
            { FileCompareTask.TaskCommandName, typeof(FileCompareTask) },
            { FileCompareInverseTask.TaskCommandName, typeof(FileCompareInverseTask) },
            { DirectoryCompareTask.TaskCommandName, typeof(DirectoryCompareTask) },
            { DirectoryCompareCountTask.TaskCommandName, typeof(DirectoryCompareCountTask) },
            { DirectoryCompareInverseTask.TaskCommandName, typeof(DirectoryCompareInverseTask) },

            //macro tasks
            { MacroDeleteTask.TaskCommandName, typeof(MacroDeleteTask) },
            { MacroCreateTask.TaskCommandName, typeof(MacroCreateTask) },
            { MacroStringSplitMacroTask.TaskCommandName, typeof(MacroStringSplitMacroTask) },
            { MacroStringSplitFilenameTask.TaskCommandName, typeof(MacroStringSplitFilenameTask) },
            { MacroStringSplitHtmlTask.TaskCommandName, typeof(MacroStringSplitHtmlTask) },
            { MacroStringSplitBrowserTask.TaskCommandName, typeof(MacroStringSplitBrowserTask) },
            { MacroSubstringMacroTask.TaskCommandName, typeof(MacroSubstringMacroTask) },
            { MacroSubstringFilenameTask.TaskCommandName, typeof(MacroSubstringFilenameTask) },
            { MacroSubstringHtmlTask.TaskCommandName, typeof(MacroSubstringHtmlTask) },
            { MacroSubstringBrowserTask.TaskCommandName, typeof(MacroSubstringBrowserTask) },
            { MacroStringInputMacroJsonTask.TaskCommandName, typeof(MacroStringInputMacroJsonTask) },

            //directory tasks
            { DirectoryCreateTask.TaskCommandName, typeof(DirectoryCreateTask) },
            { DirectoryListTask.TaskCommandName, typeof(DirectoryListTask) },
            { DirectoryCopyTask.TaskCommandName, typeof(DirectoryCopyTask) },
            { DirectoryDeleteTask.TaskCommandName, typeof(DirectoryDeleteTask) },
            { DirectoryMoveTask.TaskCommandName, typeof(DirectoryMoveTask) },

            //browser session tasks
            { StartBrowserSessionTask.TaskCommandName, typeof(StartBrowserSessionTask) },
            { EndBrowserSessionTask.TaskCommandName, typeof(EndBrowserSessionTask) },
            { BrowserSessionSetHeaderTask.TaskCommandName, typeof(BrowserSessionSetHeaderTask) },
            { BrowserSessionRemoveHeaderTask.TaskCommandName, typeof(BrowserSessionRemoveHeaderTask) },
            { BrowserSessionGetTask.TaskCommandName, typeof(BrowserSessionGetTask) },
            { BrowserSessionPostTask.TaskCommandName, typeof(BrowserSessionPostTask) },
            { BrowserSessionDownloadFileTask.TaskCommandName, typeof(BrowserSessionDownloadFileTask) },

            //automation import tasks
            { TaskImportTask.TaskCommandName, typeof(TaskImportTask) },
            { MacroImportTask.TaskCommandName, typeof(MacroImportTask) },

            //other tasks
            { ShellExecuteTask.TaskCommandName, typeof(ShellExecuteTask) }
        };

        /// <summary>
        /// The xml attribute that is used for the dictionary type mapper for creating task instances.
        /// </summary>
        public const string AttributeNameForMapping = "Command";
        #endregion //Xml serialization

        /// <summary>
        /// A list of macros that should be ignored from running macro parse operations on.
        /// </summary>
        public static readonly string[] SpecialCaseIgnoreMacro = new string[]
        {
            "last_download_filename"
        };

        /// <summary>
        /// Gets or sets the automation sequence.
        /// </summary>
        public AutomationSequence AutomationSequence { get; set; }

        /// <summary>
        /// Gets the automation sequencer.
        /// </summary>
        public AutomationSequencer AutomationSequencer { get { return AutomationSequence?.AutomationSequencer; } }

        /// <summary>
        /// Gets the automation runner window instance.
        /// </summary>
        public DatabaseAutomationRunner DatabaseAutomationRunner { get { return AutomationSequence?.DatabaseAutomationRunner; } }

        /// <summary>
        /// Gets the automation runner settings.
        /// </summary>
        public AutomationRunnerSettings AutomationSettings { get { return AutomationSequence?.AutomationRunnerSettings; } }

        /// <summary>
        /// Gets a list of all macros in the automation sequence instance.
        /// </summary>
        /// <seealso cref="AutomationSequence.AllMacros"/>
        public List<AutomationMacro> Macros { get { return AutomationSequence?.AllMacros; } }

        /// <summary>
        /// Gets the automation compare manager.
        /// </summary>
        public AutomationCompareManager AutomationCompareManager { get { return AutomationSequence?.AutomationCompareManager; } }

        /// <summary>
        /// Gets the browser session manager.
        /// </summary>
        protected BrowserSessionManager BrowserSessionManager { get { return AutomationSequence?.BrowserSessionManager; } }

        /// <summary>
        /// Gets the error message associated with the error of executing the task.
        /// </summary>
        public string ErrorMessage { get; protected set; } = string.Empty;

        /// <summary>
        /// Gets the exit code after execution of the task.
        /// </summary>
        public AutomationExitCode ExitCode { get; protected set; } = 0;

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        /// <seealso cref="TaskTypeMapper"/>
        public abstract string Command { get; }

        /// <summary>
        /// Get or set a unique ID tag for this task.
        /// </summary>
        /// <remarks>This is not required, but is useful when debugging issues during sequence execution. The ID of the task is printed in the log file.</remarks>
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// Gets the ID name of this automation task.
        /// </summary>
        /// <seealso cref="ID"/>
        public string ComponentInternalName { get { return ID; } }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public abstract void ValidateCommands();

        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public abstract void ProcessMacros();

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public abstract Task RunTask();

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public abstract void ProcessTaskResults();

        /// <summary>
        /// Evaluates the exit code after each phase of the task's execution.
        /// </summary>
        /// <param name="taskState">The phase/state of the task (e.g. ValidateCommands)</param>
        /// <returns>True if the exit code is none, false otherwise (including if default)</returns>
        public virtual bool EvaluateResults(string taskState)
        {
            switch (ExitCode)
            {
                case AutomationExitCode.None:
                    Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Task: {0} ID: {1} State: {2}, ExitCode: {3}", Command, ID, taskState, ExitCode);
                    return true;

                case AutomationExitCode.ComparisonNoFilesToUpdate:
                    Logging.Info("The task {0} (ID {1}) reported exit code {2}. The Sequence will stop, but a success will be reported", Command, ID, ExitCode.ToString());
                    return true;

                default:
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Error in task {0} execution! ID: {1} Exit code: {2}, ErrorMessage: {3}", Command, ID, ExitCode, string.IsNullOrEmpty(ErrorMessage) ? "(empty)" : ErrorMessage);
                    return false;
            }
        }

        /// <summary>
        /// "true" version means that the test being true is "bad".
        /// </summary>
        /// <param name="test"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        protected virtual bool ValidateCommandTrue(bool test, string formattedString)
        {
            return ValidateForExitTrue(test, AutomationExitCode.ValidateCommandsFail, string.Format("Exit Code {0}: {1}", (int)AutomationExitCode.ValidateCommandsFail, formattedString));
        }

        /// <summary>
        /// "false" version means that the test being false is "bad".
        /// </summary>
        /// <param name="test"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        protected virtual bool ValidateCommandFalse(bool test, string formattedString)
        {
            bool test_ = !test;
            return ValidateCommandTrue(test_, formattedString);
        }

        /// <summary>
        /// Validates that the given argument string's value isn't null or empty.
        /// </summary>
        /// <param name="argName">The name/xml argument of the task's field.</param>
        /// <param name="arg">The task argument to validate.</param>
        /// <returns>True if the string is null or empty, false otherwise.</returns>
        protected virtual bool ValidateCommandStringNullEmptyTrue(string argName, string arg)
        {
            return ValidateCommandTrue(string.IsNullOrEmpty(arg), string.Format("The arg {0} is empty string", nameof(argName)));
        }

        /// <summary>
        /// "true" version means that the test being true is "bad".
        /// </summary>
        /// <param name="test"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        protected virtual bool ProcessTaskResultTrue(bool test, string formattedString)
        {
            return ValidateForExitTrue(test, AutomationExitCode.ProcessResultsFail, string.Format("Exit Code {0}: {1}", (int)AutomationExitCode.ValidateCommandsFail, formattedString));
        }

        /// <summary>
        /// "false" version means that the test being false is "bad".
        /// </summary>
        /// <param name="test"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        protected virtual bool ProcessTaskResultFalse(bool test, string formattedString)
        {
            bool test_ = !test;
            return ValidateForExitTrue(test_, AutomationExitCode.ProcessResultsFail, string.Format("Exit Code {0}: {1}", (int)AutomationExitCode.ValidateCommandsFail, formattedString));
        }

        /// <summary>
        /// Evaluates if a logical result is true or false. If true, an error is reported.
        /// </summary>
        /// <param name="test">The logic to run.</param>
        /// <param name="exitCode">If the logic returns true, the exit code to set for the task.</param>
        /// <param name="formattedString">If the logic returns true, the error message to set for the task.</param>
        /// <returns>True if the logic result is true, false otherwise.</returns>
        /// <seealso cref="AutomationExitCode"/>
        protected virtual bool ValidateForExitTrue(bool test, AutomationExitCode exitCode, string formattedString)
        {
            if (test)
            {
                ExitCode = exitCode;
                ErrorMessage = formattedString;
            }
            return test;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <remarks>The method will run the following pre-defined methods:
        /// ProcessMacros(), ValidateCommands(), RunTask(), ProcessTaskResults.
        /// Each method, or 'phase' of the task contains a try-catch and an evaluation of the results to determine if the next phase should run.</remarks>
        public async Task Execute()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: Task start", Command);

            try
            {
                ProcessMacros();
            }
            catch (Exception ex)
            {
                ExitCode = AutomationExitCode.ExecuteException;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            if (!EvaluateResults(nameof(ProcessMacros)))
                return;

            try
            {
                ValidateCommands();
            }
            catch (Exception ex)
            {
                ExitCode = AutomationExitCode.ExecuteException;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            if (!EvaluateResults(nameof(ValidateCommands)))
                return;

            try
            {
                await RunTask();
            }
            catch (Exception ex)
            {
                ExitCode = AutomationExitCode.ExecuteException;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            if (!EvaluateResults(nameof(RunTask)))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessTaskResults() start", Command);
            try
            {
                ProcessTaskResults();
            }
            catch (Exception ex)
            {
                ExitCode = AutomationExitCode.ExecuteException;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            if (!EvaluateResults(nameof(ProcessTaskResults)))
                return;

            if (this is IDisposable disposable)
                disposable.Dispose();
        }

        /// <summary>
        /// Process any escape characters used within the task argument.
        /// </summary>
        /// <param name="argName">The name of the task argument.</param>
        /// <param name="arg">The value of the task argument.</param>
        /// <returns>The processed argument.</returns>
        /// <remarks>This currently processes escaped characters \{ and \}.</remarks>
        protected static string ProcessEscapeCharacters(string argName, string arg)
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Processing arg escape characters '{0}'", argName);

            //replace the escape characters for "{" and "}"
            arg = arg.Replace("\\{", "{");
            arg = arg.Replace("\\}", "}");

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "After escape processing: {0}", arg);
            return arg;
        }

        /// <summary>
        /// Find and replace any macros that exist in a task argument with their macro values.
        /// </summary>
        /// <param name="argName">The name of the task argument.</param>
        /// <param name="arg">The value task argument.</param>
        /// <param name="macros">The list of macros to try to find and replace in the task argument.</param>
        /// <returns>The macro replaced string.</returns>
        public static string ProcessMacro(string argName, string arg, List<AutomationMacro> macros)
        {
            //set property value to temp, making new variable
            string temp = arg;
            //use that new temp value's reference to pass into the real ProcessMacro
            ProcessMacro(argName, ref temp, macros);
            //and return the string value
            return temp;
        }

        /// <summary>
        /// Find and replace any macros that exist in a string with their macro values.
        /// </summary>
        /// <param name="argName">The name of the task argument.</param>
        /// <param name="arg">The value task argument.</param>
        /// <returns>The macro replaced string.</returns>
        public string ProcessMacro(string argName, string arg)
        {
            //set property value to temp, making new variable
            string temp = arg;
            //use that new temp value's reference to pass into the real ProcessMacro
            ProcessMacro(argName, ref temp, this.Macros);
            //and return the string value
            return temp;
        }

        /// <summary>
        /// Find and replace any macros that exist in a string with their macro values.
        /// </summary>
        /// <param name="argName">The name of the task argument.</param>
        /// <param name="arg">The value task argument.</param>
        /// <param name="macros">The list of macros to try to find and replace in the task argument.</param>
        /// <param name="recursionLevel">The iteration of replacing macros inside of macros. Normally this isn't set unless inside this function.</param>
        protected static void ProcessMacro(string argName, ref string arg, List<AutomationMacro> macros, int recursionLevel = 0)
        {
            string recursiveLevelString = string.Empty;
            if (recursionLevel > 0)
                recursiveLevelString = string.Format("(recursive level {0})", recursionLevel);
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Processing arg '{0}' {1}", argName, recursiveLevelString);

            //run regex on the arg to get the type of replacement to do. if it's recursive, then we need to process the inner one first
            Match result = Regex.Match(arg, AutomationMacro.MacroReplaceRegex);
            if (!result.Success)
            {
                //check if any "{" exist at all
                Match startBracketsMatch = Regex.Match(arg, "{");
                if (startBracketsMatch.Captures.Count > 0)
                {
                    Match endBracketsMatch = Regex.Match(arg, "}");

                    Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Macros were detected in the argument, but the syntax was incorrect. Most likely is the number of start and end brackets are unbalanced.");
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.None, "Examine the number of brackets starting and ending in the argument, and try again. For debug, here's what was parsed:");

                    Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Argument value: {0}", arg);
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
                    return;
                }
                Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "The argument {0} has no macros, continue", argName);
                return;
            }

            int inner1Count = result.Groups[AutomationMacro.RegexGroupInner1].Captures.Count;
            int inner2Count = result.Groups[AutomationMacro.RegexGroupInner2].Captures.Count;
            int inner3Count = result.Groups[AutomationMacro.RegexGroupInner3].Captures.Count;

            //verify that 2 and 3 have the same number
            if (inner2Count != inner3Count)
            {
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Inner2Count ({0}) != Inner3Count ({1})! The macro engine is not designed for this!", inner2Count, inner3Count);
                throw new NotImplementedException();
            }
            else if (inner2Count > inner1Count)
            {
                int countDifference = inner2Count - inner1Count;
                //this means that the regex must recurse x levels (the difference) into the string to solve the inner values first
                Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "Inner2Count ({0}) > Inner1Count ({1}), required to recuse {2} levels to solve inner macros", inner2Count, inner1Count, countDifference);

                //use the matches of inner1 to send each section into the regex engine again
                int captureCount = 0;
                foreach (Capture capture in result.Groups[AutomationMacro.RegexGroupInner1].Captures)
                {
                    string capturedValue = capture.Value;
                    Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "Running regex on Inner1Count: {0}", capturedValue);
                    //get a count of how many "{" characters exist in this string. If it's 1, then just run this string through and call it good
                    //if it's more then 1, then need to parse out the extra level via a new greedy regex
                    int numStarts = capturedValue.Count(ch_ => ch_.Equals('{'));
                    if (numStarts > 1)
                    {
                        Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "This match is {0} level of brackets, split out the brackets before recursively processing", numStarts);
                        //we need to use the new value as the 'starting value', as if it didn't have a macro around it. for example, consider:
                        //name_{use_{date}_val}_thing
                        //after we resolve {date}, it will become part of the name for {use_{date}_val}.
                        //the way to do this is to treat {use_{date}_val} as a word by itself, i.e. 'use_{date}_val'
                        Regex subRegex = new Regex(@"{.+}");
                        Match sectionMatch = subRegex.Match(capturedValue);

                        //following the example above, we have the section {use_{date}_val}
                        //strip off the brackets and send it through
                        string splitValue = sectionMatch.Value;
                        splitValue = splitValue.Remove(0, 1);
                        splitValue = splitValue.Remove(splitValue.Length - 1, 1);

                        //we now have 'use_{date}_val', send that through the macro engine
                        string innerResult = ProcessMacro(string.Format("{0}_capture{1}_level{2}", argName, captureCount, countDifference), splitValue, macros);

                        //use_the_date_val, if {date} = the_date. put the brackets back on
                        innerResult = "{" + innerResult + "}";

                        //{use_the_date_val}, now do the final replace of that macro
                        Regex replaceRegex2 = new Regex(sectionMatch.Value);
                        capturedValue = replaceRegex2.Replace(capturedValue, innerResult, 1);
                    }
                    else if (numStarts == 0)
                    {
                        throw new BadMemeException("whoa. didn't see that coming.");
                    }
                    else
                    {
                        Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "This match is 1 level of brackets, perform direct recursive replacement");
                    }

                    string processedValue = ProcessMacro(string.Format("{0}_capture{1}_level{2}", argName, captureCount, countDifference), capturedValue, macros);
                    Regex replaceRegex = new Regex(capture.Value);
                    arg = replaceRegex.Replace(arg, processedValue, 1);
                    captureCount++;
                }
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
                    AutomationMacro resultMacro = macros.Find(macro => macro.Name.Equals(capture.Value));
                    if (resultMacro == null)
                    {
                        if (!SpecialCaseIgnoreMacro.Contains(capture.Value))
                        {
                            Logging.Warning(Logfiles.AutomationRunner, LogOptions.None, "The macro with name '{0}', does not exist, skipping. (Is this intended?)", capture.Value);
                        }
                        continue;
                    }

                    string macroValue = resultMacro.Value;

                    Match recursiveCheck = Regex.Match(macroValue, AutomationMacro.MacroReplaceRegex);
                    if (recursiveCheck.Success)
                    {
                        Logging.Debug("A macro was resolved to another macro, run the macro replacement code again");
                        string temp = macroValue;
                        ProcessMacro(argName, ref temp, macros, recursionLevel+1);
                        macroValue = temp;
                    }

                    //perform a single replace on the specified location of the string
                    //https://stackoverflow.com/a/6372134/3128017
                    Regex replaceRegex = new Regex("{" + capture.Value + "}");
                    arg = replaceRegex.Replace(arg, macroValue, 1);
                    Logging.Debug(Logfiles.AutomationRunner, LogOptions.None, "A single replace was done on the argument. Result: {0}", arg);
                }
            }

            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "After processing: {0}", arg);
        }
    }
}
