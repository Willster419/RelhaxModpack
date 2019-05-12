using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;
using TeximpNet.DDS;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TeximpNet;
using TeximpNet.Compression;

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
                CustomProgress prog2 = new CustomProgress() { value = 8 };
                progress.Report(prog);
                for(int i = 0; i < 10; i++)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.7.2
                    System.Threading.Thread.Sleep(500);
                    prog.update = string.Format("Processing wait {0} of 10", i+1);
                    prog.value = i + 1;
                    if (i == 4)
                        progress.Report(prog2);
                    else
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


        #region Bitmap DDS testing

        private void DdsToBitmap_Click(object sender, RoutedEventArgs e)
        {
            //check if it's actually a dds file
            bool isItADDSFile = DDSFile.IsDDSFile("damageIndicator.dds");
            Bitmap bmp = null;

            //helpful links
            //https://docs.microsoft.com/en-us/windows/desktop/api/dxgiformat/ne-dxgiformat-dxgi_format#dxgi-format-bc3-unorm
            //https://docs.microsoft.com/en-us/windows/desktop/direct3d11/texture-block-compression-in-direct3d-11#bc1-bc2-and-b3-formats

            //best method found to use mode 1 and direct=true
            bool directBitmap = true;

            //TEXIMP DDSFILE
            //this is an Importer and says nothing about bitmap objects. i don't think it's designed to do this (load directly and make bitmap out of)
            //TEXIMP SURFACE
            //https://bitbucket.org/Starnick/teximpnet/src/acf2d0a8d7f6?at=master
            //format of image is Rgba32
            using (Surface surface = Surface.LoadFromFile("damageIndicator.dds", ImageLoadFlags.Default))
            {
                surface.FlipVertically();

                if (directBitmap)
                {
                    //https://stackoverflow.com/questions/16478449/convert-intptr-to-bitmapimage
                    bmp = new Bitmap(surface.Width, surface.Height, surface.Pitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, surface.DataPtr);
                }
                else
                {
                    //stride is rowpitch
                    //length of array is height * stride/pitch
                    int size = surface.Height * surface.Pitch;
                    byte[] managedArrayy = new byte[size];

                    //https://stackoverflow.com/questions/5486938/c-sharp-how-to-get-byte-from-intptr
                    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.copy?view=netframework-4.8#System_Runtime_InteropServices_Marshal_Copy_System_IntPtr_System_Byte___System_Int32_System_Int32_
                    Marshal.Copy(surface.DataPtr, managedArrayy, 0, size);
                    BitmapSource source = BitmapSource.Create(surface.Width, surface.Height, 96.0, 96.0, PixelFormats.Bgra32, null, managedArrayy, surface.Pitch);
                    bmp = BitmapFromSource(source);
                    TestImageDisplay.Source = source;
                }

                bmp.Save("damageIndicator.png", System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();
            }
        }

        //https://stackoverflow.com/questions/3751715/convert-system-windows-media-imaging-bitmapsource-to-system-drawing-image
        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private void BitmapToDds_Click(object sender, RoutedEventArgs e)
        {
            BitmapData bmpData = null;
            using (Bitmap bmp2 = new Bitmap("damageIndicator.png"))
            {
                // Lock the bitmap's bits. 
                //https://stackoverflow.com/questions/28655133/difference-between-bitmap-and-bitmapdata
                //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=netframework-4.8#System_Drawing_Imaging_BitmapData_Scan0
                Rectangle rect = new Rectangle(0, 0, bmp2.Width, bmp2.Height);
                bmpData = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                using (Compressor compressor = new Compressor())
                using (Surface surfaceFromRawData = Surface.LoadFromRawData(ptr, bmp2.Width, bmp2.Height, bmpData.Stride, true))
                {
                    //compress to dds
                    compressor.Compression.Format = CompressionFormat.DXT5;
                    compressor.Input.AlphaMode = AlphaMode.None;
                    compressor.Input.GenerateMipmaps = false;
                    compressor.Input.ConvertToNormalMap = false;
                    compressor.Input.SetData(surfaceFromRawData);
                    compressor.Process("damageIndicator2.dds");
                }
            }
        }
        #endregion

        #region dialog not blocking
        private void DialogNotBlocksButton_Click(object sender, RoutedEventArgs e)
        {
            IsDialogReturnedButton.Text = "The dialog has not returned";
            TestSubWindow testSubWindow = new TestSubWindow();
            testSubWindow.Owner = GetWindow(this);
            testSubWindow.Visibility = Visibility.Hidden;
            testSubWindow.Hide();
            testSubWindow.ShowDialog();
            IsDialogReturnedButton.Text = "The dialog has returned";
            testSubWindow.WindowState = WindowState.Normal;
            testSubWindow.Show();
        }
        #endregion

        private void DialogBlocksButton_Click(object sender, RoutedEventArgs e)
        {
            IsDialogReturnedButton.Text = "The dialog has not returned";
            TestSubWindow testSubWindow = new TestSubWindow();
            testSubWindow.Owner = GetWindow(this);
            //testSubWindow.Visibility = Visibility.Hidden;
            //setting visability stops the dialog from blocking
            testSubWindow.Hide();
            testSubWindow.ShowDialog();
            IsDialogReturnedButton.Text = "The dialog has returned";
            testSubWindow.WindowState = WindowState.Normal;
            //testSubWindow.Show();
        }
    }
}
