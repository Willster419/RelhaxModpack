using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace RelhaxWPFConvert
{
    public enum MediaType
    {
        Unknown = 0,
        Picture = 1,
        Webpage = 2,
        MediaFile = 3,
        HTML = 4
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckBox_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void CheckBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);
            HitTestResult result = VisualTreeHelper.HitTest((UIElement)sender,pt);
        }

        private void ContentControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ComboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            testBox1.CheckboxDisabledColor = Colors.Green;
        }

        #region Task Reporting

        //scruct for task reporting
        public struct CustomProgress
        {
            public string update;
            public int value;
        }

        private async void BackgroundTaskStartButton_Click(object sender, RoutedEventArgs e)
        {
            //create the progress object and access the async method
            Progress<CustomProgress> progress = new Progress<CustomProgress>();
            progress.ProgressChanged += Progress_ProgressChanged;
            await PerformTaskAsync(progress);
            BackgroundTaskReport.Text = "Complete";
        }

        private void Progress_ProgressChanged(object sender, CustomProgress e)
        {
            BackgroundTaskReport.Text = e.update;
            BackgroundTaskProgress.Value = e.value;
        }

        //custom async method
        public Task PerformTaskAsync(IProgress<CustomProgress> progress)
        {
            Task t = new Task(IntenseMethodThatTakesCPUTime);
            Task t2 = Task.Run(() => 
            {
                CustomProgress prog = new CustomProgress();
                prog.update = "Processing wait 0 of 10";
                prog.value = 0;
                progress.Report(prog);
                for(int i = 0; i < 10; i++)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.7.2
                    System.Threading.Thread.Sleep(500);
                    prog.update = string.Format("Processing wait {0} of 10", i+1);
                    prog.value = i + 1;
                    progress.Report(prog);
                }
            });
            return t2;
        }
        #endregion

        #region Dispatcher scheduling on UI thread (for UI loading...)
        private void ForgroundTaskStartButton_Click(object sender, RoutedEventArgs e)
        {
            ForgroundTaskReport.Text = "Processing wait 0 of 10";
            ForgroundTaskProgress.Value = 0;
            IntenseMethodThatTakesCPUTime();
            BackgroundTaskReport.Text = "Complete";
        }

        public void IntenseMethodThatTakesCPUTime()
        {
            for (int i = 0; i < 10; i++)
            {
                //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.7.2
                System.Threading.Thread.Sleep(500);
                ForgroundTaskReport.Text = string.Format("Processing wait {0} of 10", i + 1);
                ForgroundTaskProgress.Value = i+1;
                AllowUIToUpdate();
            }
        }
        //https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread
        //https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this
        void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }
        #endregion

        #region Task reporting with internal UI work
        private void HybradTaskStartButton_Click(object sender, RoutedEventArgs e)
        {
            //create the progress object and access the async method
            Progress<CustomProgress> progress = new Progress<CustomProgress>();
            progress.ProgressChanged += Progress_ProgressChanged2;
            PerformTaskAsync2(progress);
            //BackgroundTaskReport.Text = "Complete";
        }

        private void Progress_ProgressChanged2(object sender, CustomProgress e)
        {
            HybridTaskReport.Text = e.update;
            HybridTaskProgress.Value = e.value;
        }

        //custom async method
        public Task PerformTaskAsync2(IProgress<CustomProgress> progress)
        {
            Task t = new Task(IntenseMethodThatTakesCPUTime);
            Task t2 = Task.Run(() =>
            {
                CustomProgress prog = new CustomProgress();
                prog.update = "Processing wait 0 of 10";
                prog.value = 0;
                progress.Report(prog);
                for (int i = 0; i < 10; i++)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.7.2
                    System.Threading.Thread.Sleep(500);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ContentPresenter cp = new ContentPresenter();
                    });
                    //doing the below will cause it to crash (can only be done on UI thread onyl)
                    //ContentPresenter cp = new ContentPresenter();
                    prog.update = string.Format("Processing wait {0} of 10", i + 1);
                    prog.value = i + 1;
                    progress.Report(prog);
                }
                //this is also not allowed
                //BackgroundTaskReport.Text = "Complete";
            });
            return t2;
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            headerModifyTest.Header = "VALUE";
        }
    }
}
