using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A MacroStringInputMacroJsonTask allows for creation of a macro by using an already existing macro as input text for a JsonPath search operation.
    /// </summary>
    public class MacroStringInputMacroJsonTask : MacroStringInputMacroTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_string_input_macro_json";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The JsonPath argument to use for parsing.
        /// </summary>
        public string Jsonpath { get; set; }

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Jsonpath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            Jsonpath = ProcessMacro(nameof(Jsonpath), Jsonpath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(Jsonpath), Jsonpath))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
        }

        /// <summary>
        /// Runs a jsonPath search operation to get a string result saved to stringReturnValue
        /// </summary>
        /// <returns>True if the implementation was successful, false otherwise.</returns>
        protected override bool GetStringReturnValue()
        {
            //parse it into json for jsonpath evaluation
            //load the settings
            JsonLoadSettings settings = new JsonLoadSettings()
            {
                //ignore comments and load line info
                //"jsOn DoeSnT sUpPorT coMmAs"
                CommentHandling = CommentHandling.Ignore,
                LineInfoHandling = LineInfoHandling.Load
            };
            JToken root;

            //attempt to load the json text to serialized form
            try
            {
                root = JToken.Parse(inputMacroText, settings);
            }
            catch (JsonReaderException j)
            {
                Logging.Error(LogOptions.ClassName, "Failed to parse macro {0} as json!", InputMacroName);
                Logging.Info(j.ToString());
                return false;
            }

            JToken jsonPathresult = null;
            try
            {
                jsonPathresult = root.SelectToken(Jsonpath);
            }
            catch (Exception exResults)
            {
                Logging.Error(LogOptions.ClassName, "Error with jsonPath: {0}", Jsonpath);
                Logging.Error(exResults.ToString());
                return false;
            }

            //log some info about the return value
            Logging.Info("Result type: {0}", jsonPathresult.Type.ToString());
            Logging.Info("Result value: {0}", jsonPathresult.ToString());

            stringReturnValue = jsonPathresult.ToString();
            return true;
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
