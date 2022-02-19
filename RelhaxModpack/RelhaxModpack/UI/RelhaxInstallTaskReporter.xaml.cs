using RelhaxModpack.Settings;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// States of the Task reporter in display to the UI
    /// </summary>
    public enum TaskReportState
    {
        /// <summary>
        /// Not used for this installation
        /// </summary>
        Inactive,

        /// <summary>
        /// In use for this installation
        /// </summary>
        Active,

        /// <summary>
        /// In use for this installation and the task completed
        /// </summary>
        Complete,

        /// <summary>
        /// An error has occurred on that task
        /// </summary>
        Error
    }

    /// <summary>
    /// Interaction logic for RelhaxInstallTaskReporter.xaml
    /// </summary>
    public partial class RelhaxInstallTaskReporter : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Create an instance of the RelhaxInstallTaskReporter and init the UI side of it
        /// </summary>
        public RelhaxInstallTaskReporter(string uniqueID)
        {
            InitializeComponent();
            //dynamically create the tags and then apply the theme to itself
            MainBorder.Tag = string.Format("{0}_{1}", uniqueID, MainBorder.Name);
            TaskName.Tag = string.Format("{0}_{1}", uniqueID,TaskName.Name);
            TaskStatus.Tag = string.Format("{0}_{1}", uniqueID, TaskStatus.Name);
            TaskProgress1.Tag = string.Format("{0}_{1}", uniqueID, TaskProgress1.Name);
            TaskProgress2.Tag = string.Format("{0}_{1}", uniqueID,TaskProgress2.Name);
        }

        #region Properties

        /// <summary>
        /// Flag for when the object has been fully constructed by the UI Dispatcher.
        /// </summary>
        /// <remarks>Due to the multi-threaded nature of the progress reporting, progress may be reported before the reporting UI objects are fully constructed.
        /// This results in null exceptions. By using a flag to determine if the object is fully created, the reporting progresses won't try to update properties of null objects</remarks>
        public bool LoadedAfterApply { get; set; } = false;

        private TaskReportState _reportState = TaskReportState.Inactive;

        /// <summary>
        /// Controls the UI state of the thread reporter
        /// </summary>
        public TaskReportState ReportState
        {
            get
            { return _reportState; }
            set
            {
                _reportState = value;
                if(value == TaskReportState.Active)
                {
                    IsEnabled = true;
                }
                else
                {
                    IsEnabled = false;
                    if(value == TaskReportState.Error)
                    {
                        Background = new SolidColorBrush(Colors.Red);
                    }
                    else if (value == TaskReportState.Complete)
                    {
                        Background = new SolidColorBrush(Colors.Green);
                    }
                }
            }
        }

        private bool _isSubProgressActive = false;

        /// <summary>
        /// Toggle if the second progress bar should be visible
        /// </summary>
        /// <remarks>Some tasks (like zip file extraction) have "sub-tasks" that take enough time where tracking their progress is warranted.
        /// For example, a zip file has many files to extract. That's the main task. However, each file has bytes to extract. If the file to extract
        /// is large, a subtask to track the extraction progress could be useful</remarks>
        public bool IsSubProgressActive
        {
            get { return _isSubProgressActive; }
            set
            {
                _isSubProgressActive = value;
                OnPropertyChanged(nameof(IsSubProgressActive));
            }
        }

        private string _taskTitle = string.Empty;

        /// <summary>
        /// The name of this task to display
        /// </summary>
        public string TaskTitle
        {
            get { return _taskTitle; }
            set
            {
                _taskTitle = value;
                OnPropertyChanged(nameof(TaskTitle));
            }
        }

        private string _taskText = string.Empty;

        /// <summary>
        /// The main reporting description of this task. Supports string formatting
        /// </summary>
        public string TaskText
        {
            get { return _taskText; }
            set
            {
                _taskText = value;
                OnPropertyChanged(nameof(TaskText));
            }
        }

        private int _taskMinimum = 0;
        /// <summary>
        /// The minimum value for the main task progress bar
        /// </summary>
        public int TaskMinimum
        {
            get { return _taskMinimum; }
            set
            {
                _taskMinimum = value;
                OnPropertyChanged(nameof(TaskMinimum));
            }
        }

        private int _taskMaximum = 0;

        /// <summary>
        /// The maximum value for the main task progress bar
        /// </summary>
        /// <remarks>This value can be changed for, example, the number of patches to run</remarks>
        public int TaskMaximum
        {
            get { return _taskMaximum; }
            set
            {
                _taskMaximum = value;
                OnPropertyChanged(nameof(TaskMaximum));
            }
        }

        private int _taskValue = 0;

        /// <summary>
        /// The current progress of this task. Value must be between Maximum and Minimum.
        /// </summary>
        public int TaskValue
        {
            get { return _taskValue; }
            set
            {
                _taskValue = value;
                OnPropertyChanged(nameof(TaskValue));
            }
        }

        private int _subTaskMininum = 0;

        /// <summary>
        /// The minimum value for the subtask progress bar
        /// </summary>
        public int SubTaskMinimum
        {
            get { return _subTaskMininum; }
            set
            {
                _subTaskMininum = value;
                OnPropertyChanged(nameof(SubTaskMinimum));
            }
        }

        private int _subTaskMaximum = 100;

        /// <summary>
        /// The maximum value for the subtask progress bar
        /// </summary>
        public int SubTaskMaximum
        {
            get { return _subTaskMaximum; }
            set
            {
                _subTaskMaximum = value;
                OnPropertyChanged(nameof(SubTaskMaximum));
            }
        }

        private int _subTaskValue = 0;

        /// <summary>
        /// The current value of the subtask progress. Must be between the maximum and minimum
        /// </summary>
        public int SubTaskValue
        {
            get { return _subTaskValue; }
            set
            {
                _subTaskValue = value;
                OnPropertyChanged(nameof(SubTaskValue));
            }
        }
        #endregion

        #region Property changed code
        //https://stackoverflow.com/questions/34651123/wpf-binding-a-background-color-initializes-but-not-updating
        /// <summary>
        /// Event to trigger when an internal property is changed. It forces a UI update
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to invoke the PropertyChanged event to update the UI
        /// </summary>
        /// <param name="propertyName">The name of the property that changed, to update it's UI binding</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
