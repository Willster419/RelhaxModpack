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

namespace RelhaxWPFConvert
{
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

        public void IntenseMethodThatTakesCPUTime()
        {

        }
        #endregion
    }
}
