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
    public class MacroStringInputMacroJsonTask : MacroStringInputMacroTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_string_input_macro_json";

        public override string Command { get { return TaskCommandName; } }

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
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            Jsonpath = ProcessMacro(nameof(Jsonpath), Jsonpath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(Jsonpath), Jsonpath))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        protected override async Task<bool> GetStringReturnValue()
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

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
