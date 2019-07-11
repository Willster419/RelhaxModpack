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

namespace RelhaxModpack.UIComponents
{
    public enum TaskReportState
    {
        Inactive,
        Active,
        Complete,
        Error
    }

    /// <summary>
    /// Interaction logic for RelhaxInstallTaskReporter.xaml
    /// </summary>
    public partial class RelhaxInstallTaskReporter : UserControl, INotifyPropertyChanged
    {
        public RelhaxInstallTaskReporter()
        {
            InitializeComponent();
        }

        #region Properties

        private TaskReportState _reportState = TaskReportState.Inactive;
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
        public int TaskValue
        {
            get { return _taskValue; }
            set
            {
                _taskValue = value;
                OnPropertyChanged(nameof(TaskValue));
            }
        }

        public int _subTaskMininum = 0;
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
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handle = PropertyChanged;
            if (handle != null)
                handle(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
