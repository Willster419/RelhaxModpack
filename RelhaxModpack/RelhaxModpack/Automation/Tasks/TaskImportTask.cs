using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class TaskImportTask : ImportTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "import_task";

        public override string Command { get { return TaskCommandName; } }

        protected override string XpathExpression { get { return AutomationSequence.AutomationSequenceTaskDefinitionsXpath; } }

        protected List<AutomationTask> AutomationTasks { get { return AutomationSequence.AutomationTasks; } }

        protected override void CreateList()
        {
            objectList = new List<AutomationTask>();
        }

        protected override bool ParseToList()
        {
            return CommonUtils.SetListEntries(objectList, ID, automationTaskHolder.Elements(), AttributeNameForMapping, TaskTypeMapper);
        }

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
