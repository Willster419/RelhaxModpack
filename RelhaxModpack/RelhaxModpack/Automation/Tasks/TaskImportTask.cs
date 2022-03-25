using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Import a list of tasks from a sequence xml document by inserting them in to the list of tasks to execute.
    /// </summary>
    public class TaskImportTask : ImportTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "import_task";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The Xpath search pattern to use for getting a list of xml macro objects.
        /// </summary>
        protected override string XpathExpression { get { return AutomationSequence.AutomationSequenceTaskDefinitionsXpath; } }

        /// <summary>
        /// The list of tasks executing during this sequence.
        /// </summary>
        protected List<AutomationTask> AutomationTasks { get { return AutomationSequence.AutomationTasks; } }

        /// <summary>
        /// Initializes objectList to hold AutomationTask objects.
        /// </summary>
        protected override void CreateList()
        {
            objectList = new List<AutomationTask>();
        }

        /// <summary>
        /// Parses the xml objects from task elements to task object instances.
        /// </summary>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        protected override bool ParseToList()
        {
            return CommonUtils.SetListEntries(objectList, ID, automationTaskHolder.Elements(), AttributeNameForMapping, TaskTypeMapper);
        }

        /// <summary>
        /// Perform and post-processing on the created tasks and insert them into the list of tasks to run.
        /// </summary>
        protected override void ImportList()
        {
            Logging.Debug("Configuring tasks");

            List<AutomationTask> taskList = objectList as List<AutomationTask>;

            foreach (AutomationTask task in taskList)
            {
                task.AutomationSequence = this.AutomationSequence;
            }

            int currentTaskIndex = AutomationTasks.IndexOf(this);
            Logging.Debug("Currently on task index {0}, inserting tasks", currentTaskIndex);
            AutomationTasks.InsertRange(currentTaskIndex + 1, taskList);
        }
    }
}
