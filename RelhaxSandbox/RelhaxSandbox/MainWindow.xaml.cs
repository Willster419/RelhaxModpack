using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
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
using System.Net;
using TeximpNet;
using TeximpNet.Compression;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Runtime.Remoting.Contexts;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Documents;
using System.Xml;
using System.Windows.Markup;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Text;
using Microsoft.Win32;
using System.Linq;
using FontFamily = System.Windows.Media.FontFamily;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ComponentModel;

namespace RelhaxSandbox
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private TaskbarManager taskbarInstance;

        public MainWindow()
        {
            InitializeComponent();
        }

        private double dpiFactor = 0;

        #region Hit testing

        private void ScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);
            HitTestResult result = VisualTreeHelper.HitTest((UIElement)sender,pt);
        }
        #endregion

        #region Right click preview testing

        private void ContentControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ComboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void CheckBox_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void CheckBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //checkbox testing
            testBox1.CheckboxDisabledColor = Colors.Green;

            //init UI color settings
            UISettings.InitUIBrushes();

            //get value from resource dictionary
            bool test = (bool)Application.Current.Resources["ApplyColorSettings"];

            //task bar stuff
            //https://www.fluxbytes.com/csharp/how-to-display-a-progressbar-in-taskbar-in-c/
            if (TaskbarManager.IsPlatformSupported)
            {
                taskbarInstance = TaskbarManager.Instance;
                taskbarInstance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
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

        #region Bitmap DDS testing

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);


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
                    //TestImageDisplay.Source = source;
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
            TeximpNet.Unmanaged.NvTextureToolsLibrary library = TeximpNet.Unmanaged.NvTextureToolsLibrary.Instance;
            library.LoadLibrary("nvtt.dll");

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

        #region Dialog not blocking

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
        #endregion

        #region NAudio Testing to load from URL and pass in stream

        private async void LoadNAudio_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://bigmods.relhaxmodpack.com/Medias/Audio/6th_Sense/relhax_spotted_4.wav";
            string extension = Path.GetExtension(url);
            using (WebClient client = new WebClient())
            using (MemoryStream audioStream = new MemoryStream(await client.DownloadDataTaskAsync(url)))
            using (WaveFileReader reader = new WaveFileReader(audioStream))
            using (WaveOutEvent wo = new WaveOutEvent())
            {
                wo.Init(reader);
                wo.Play();
            }
        }
        #endregion

        #region JsonNet Testing with loading and keeping comments

        private void JsonTest_Click(object sender, RoutedEventArgs e)
        {
            JsonLoadSettings settings = new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load,
                LineInfoHandling = LineInfoHandling.Load
            };
            string JsonFromFile = File.ReadAllText("input.json");
            //Parse() does not keep comments
            JToken objectt = JToken.Parse(JsonFromFile,settings);

            //output
            string newJson = objectt.ToString(Newtonsoft.Json.Formatting.Indented, null);
            //toString() will now allow for output of previous formatting
            File.WriteAllText("output.json", newJson);
        }
        #endregion

        #region Display Scaling
        private void ScallingTest_Click(object sender, RoutedEventArgs e)
        {
            //scale application from setting (TEST)
            //https://stackoverflow.com/questions/44683626/wpf-application-same-size-at-every-system-scale-scale-independent
            if(dpiFactor == 0)
            {
                dpiFactor = System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            }
            if (dpiFactor != 1.5F)
            {
                //Change scale of window content
                //https://stackoverflow.com/questions/5022397/scale-an-entire-wpf-window
                //https://stackoverflow.com/questions/44683626/wpf-application-same-size-at-every-system-scale-scale-independent
                dpiFactor = 1.5F;
                (this.Content as FrameworkElement).LayoutTransform = new ScaleTransform(dpiFactor, dpiFactor, 0, 0);
                Width *= dpiFactor;
                Height *= dpiFactor;
            }
        }
        #endregion

        #region Task Reporting with Cancellation and Reporting
        private async void TaskCancelTestingButton_Click(object sender, RoutedEventArgs e)
        {
            //progress
            Progress<CustomProgress> progress = new Progress<CustomProgress>();
            progress.ProgressChanged += Progress_ProgressChangedCancel;

            //cancel token
            tokenSource2 = new CancellationTokenSource();
            ct = tokenSource2.Token;

            bool result = await PerformTaskWithCancelAsync(progress, ct);
            TaskCancelTestingBlock.Text = result? "Complete" : "Failed";
        }

        public Task<bool> PerformTaskWithCancelAsync(IProgress<CustomProgress> progress, CancellationToken ct)
        {
            Task<bool> t2 = Task.Run(() =>
            {
                //progress
                CustomProgress prog = new CustomProgress
                {
                    update = "Processing wait 0 of 10",
                    value = 0
                };
                progress.Report(prog);

                //run task that takes up lots of stuffs
                for (int i = 0; i < 10; i++)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netframework-4.7.2
                    for (int j = 0; j < 10; j++)
                    {
                        Thread.Sleep(50);
                    }
                    ct.ThrowIfCancellationRequested();
                    prog.update = string.Format("Processing wait {0} of 10", i + 1);
                    prog.value = i + 1;
                    progress.Report(prog);
                }
            }).ContinueWith(task =>
            {
                if (task.Status == TaskStatus.Faulted)
                {
                    return false;
                }
                return true;
            });
            return t2;
        }

        private void TaskCancelTestingCancelButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource2.Cancel();
        }

        private void Progress_ProgressChangedCancel(object sender, CustomProgress e)
        {
            TaskCancelTestingBlock.Text = e.update;
            TaskCancelTestingProgress.Value = e.value;
        }

        #endregion

        #region Hyperlink Design testing
        private void ChangeHyperlinkTextLikeTranslations_Click(object sender, RoutedEventArgs e)
        {
            hyperLinkText.Text = "new text";
            CustomHyperlink.Text = "another test";
        }
        #endregion

        #region Image pan and zoom
        private void ImageDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            
        }
        #endregion

        private void ToggleDisableButton_Click(object sender, RoutedEventArgs e)
        {
            Control[] HighlightControls = new Control[]
            {
                HighlightCheckbox,
                HighlightRadioButton,
                HighlightTabControl,
                HighlightTabItem1,
                HighlightTabItem2,
                HighlitCombobox,
                HighlightTextButton1,
                HighlightTextButton2,
                HighlightTextButton3
            };

            FrameworkElement[] FrameworkControls = new FrameworkElement[]
            {
                TestContentPresenter
            };

            foreach(Control control in HighlightControls)
            {
                if (control.IsEnabled)
                    control.IsEnabled = false;
                else
                    control.IsEnabled = true;
            }

            foreach(FrameworkElement element in FrameworkControls)
            {
                if (element.IsEnabled)
                    element.IsEnabled = false;
                else
                    element.IsEnabled = true;
            }
        }

        private void ToggleDarkUIButton_Click(object sender, RoutedEventArgs e)
        {
            if (UISettings.ThemeDefault)
            {
                UISettings.ThemeDefault = false;
                HighlightTestingTabItemGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 50, 50));
                HighlightCheckbox.Background = new SolidColorBrush(Colors.Black);
                HighlightRadioButton.Background = new SolidColorBrush(Colors.Black);
                HighlightRadioButton2.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                UISettings.ThemeDefault = true;
                HighlightTestingTabItemGrid.Background = new SolidColorBrush(Colors.Transparent);
                HighlightCheckbox.Background = new SolidColorBrush(Colors.Transparent);
                HighlightRadioButton.Background = new SolidColorBrush(Colors.Transparent);
                HighlightRadioButton2.Background = new SolidColorBrush(Colors.Transparent);
            }
            UISettings.ToggleUIBrushes();
        }

        #region Description Document Loading testing
        private void FlowDocumentLoadButton_Click(object sender, RoutedEventArgs e)
        {
            //https://stackoverflow.com/questions/2830987/convert-xaml-to-flowdocument-to-display-in-richtextbox-in-wpf
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.flowdocumentscrollviewer?view=netframework-4.8
            //https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/flow-document-overview
            FlowDocument document = XamlReader.Parse(File.ReadAllText("SampleDescriptionFlowDocument.txt")) as FlowDocument;
            FlowDocumentViewer.Document = document;
        }

        private void WpfDocumentLoadButton_Click(object sender, RoutedEventArgs e)
        {
            object document = XamlReader.Parse(File.ReadAllText("SampleDescriptionWPFDocument.txt"));
            WpfDocumentViewer.Content = document;
        }
        #endregion

        private void SetTaskbarProgress_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TaskbarProgressSet.Text,out int result))
            {
                if (result > 0 && 100 > result)
                {
                    taskbarInstance.SetProgressState(TaskbarProgressBarState.Normal);
                    taskbarInstance.SetProgressValue(result, 100);
                }
            }
        }

        #region browser testing
        private async void AutoUpdateWGClickIEWPF_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AutoUpdateWGURLTextboxIEWPF.Text))
                return;
            AutoUpdateWGInfoIEWPF.Text = "Getting info...";

            bool browserLoaded = false;

            IEBrowse.LoadCompleted += (sendahh, endArgs) =>
            {
                browserLoaded = true;
            };

            //https://stackoverflow.com/questions/1298255/how-do-i-suppress-script-errors-when-using-the-wpf-webbrowser-control
            dynamic activeX = this.IEBrowse.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.IEBrowse, new object[] { });

            activeX.Silent = true;

            using (System.Windows.Forms.WebBrowser bro = new System.Windows.Forms.WebBrowser())
                SetRegistryKey(System.Diagnostics.Process.GetCurrentProcess().ProcessName, bro.Version.Major);

            IEBrowse.Navigate(AutoUpdateWGURLTextboxIEWPF.Text);

            while (!browserLoaded)
                await Task.Delay(500);

            var doc = IEBrowse.Document as mshtml.HTMLDocument;

            string s = doc.body.outerHTML;

            //http://blog.olussier.net/2010/03/30/easily-parse-html-documents-in-csharp/

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(s);
            HeadersBlockWPF.Text = s;
            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            string version = string.Empty;
            string downloadURL = string.Empty;
            if (clientVersionNode != null)
            {
                HtmlNode nodeTest = clientVersionNode[3];
                HtmlNode versionNode = nodeTest.ChildNodes[0].ChildNodes[1];
                version = versionNode.InnerText;
            }


            if (clientVersionNode != null)
            {
                HtmlNode downloadUrlNode = node.SelectSingleNode(@"//a[contains(@class, 'ModDetails_hidden')]");
                downloadURL = downloadUrlNode.Attributes["href"].Value;
            }

            AutoUpdateWGInfoIEWPF.Text = string.Format("For client: {0}, download link: {1}", version, downloadURL);
        }

        private void AutoUpdateWGClickIEWinForms_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AutoUpdateWGURLTextboxIEWIN.Text))
                return;
            AutoUpdateWGInfoIEWIN.Text = "Getting info...";

            SetRegistryKey(System.Diagnostics.Process.GetCurrentProcess().ProcessName, TestBrowse.Version.Major);

            TestBrowse.ScriptErrorsSuppressed = true;

            TestBrowse.DocumentCompleted += TestBrowse_DocumentCompleted;

            //run browser enough to get scripts parsed to get download link
            TestBrowse.Navigate(AutoUpdateWGURLTextboxIEWIN.Text);
        }

        private async void TestBrowse_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            await Task.Delay(2000);

            HtmlDocument document = new HtmlDocument();
            string htmlText = TestBrowse.Document.Body.OuterHtml;
            document.LoadHtml(htmlText);
            HeadersBlockWIN.Text = htmlText;
            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            string version = string.Empty;
            string downloadURL = string.Empty;
            if (clientVersionNode != null)
            {
                HtmlNode nodeTest = clientVersionNode[3];
                HtmlNode versionNode = nodeTest.ChildNodes[0].ChildNodes[1];
                version = versionNode.InnerText;
            }

            if (clientVersionNode != null)
            { 
                HtmlNode downloadUrlNode = node.SelectSingleNode(@"//a[contains(@class, 'ModDetails_hidden')]");
                downloadURL = downloadUrlNode.Attributes["href"].Value;
            }

            AutoUpdateWGInfoIEWIN.Text = string.Format("For client: {0}, download link: {1}", version, downloadURL);
        }

        private void AutoUpdateWGClickEDGE_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AutoUpdateWGURLTextboxEDGE.Text))
                return;
            AutoUpdateWGInfoEDGE.Text = "Getting info...";

            TestBrowseEdge.NavigationCompleted += TestBrowseEdge_NavigationCompleted;

            TestBrowseEdge.Navigate(AutoUpdateWGURLTextboxEDGE.Text);

            //how to post example. First arg is the url to post to, third arg is content and 4th arg is headers
            //TestBrowseEdge.Navigate(new Uri("string"), HttpMethod.Post, null, null);
        }

        private async void TestBrowseEdge_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            await Task.Delay(2000);

            HtmlDocument document = new HtmlDocument();
            string html = TestBrowseEdge.InvokeScript("eval", new string[] { "document.documentElement.outerHTML;" });
            document.LoadHtml(html);
            HeadersBlockEDGE.Text = html;

            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            string version = string.Empty;
            string downloadURL = string.Empty;
            if (clientVersionNode != null)
            {
                HtmlNode nodeTest = clientVersionNode[3];
                HtmlNode versionNode = nodeTest.ChildNodes[0].ChildNodes[1];
                version = versionNode.InnerText;
            }

            if (clientVersionNode != null)
            {
                HtmlNode downloadUrlNode = node.SelectSingleNode(@"//a[contains(@class, 'ModDetails_hidden')]");
                downloadURL = downloadUrlNode.Attributes["href"].Value;
            }

            AutoUpdateWGInfoEDGE.Text = string.Format("For client: {0}, download link: {1}", version, downloadURL);
        }

        private void SetRegistryKey(string exeName, int IEVersion)
        {
            //MessageBox.Show("IE version to set as " + IEVersion);
            //https://weblog.west-wind.com/posts/2011/May/21/Web-Browser-Control-Specifying-the-IE-Version#Using-the-X--UA--Compatible-HTML-Meta-Tag
            //https://stackoverflow.com/questions/17922308/use-latest-version-of-internet-explorer-in-the-webbrowser-control

            int registryToSet = 0;

            if (IEVersion >= 11)
                registryToSet = 11001;
            else if (IEVersion == 10)
                registryToSet = 10001;
            else if (IEVersion == 9)
                registryToSet = 9999;
            else if (IEVersion == 8)
                registryToSet = 8888;
            else
                registryToSet = 7000;

            //SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION
            string[] keys = new string[]
            {
                @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
            };

            foreach (string key in keys)
            {
                using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (Key.GetValue(exeName + ".exe") == null)
                        Key.SetValue(exeName + ".exe", registryToSet, RegistryValueKind.DWord);
                    else if (((int)Key.GetValue(exeName + ".exe")) != registryToSet)
                        Key.SetValue(exeName + ".exe", registryToSet, RegistryValueKind.DWord);
                }
                /*
                 * this method requires admin
                using (RegistryKey Key = Registry.LocalMachine.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (Key.GetValue(exeName + ".exe") == null)
                        Key.SetValue(exeName + ".exe", registryToSet, RegistryValueKind.DWord);
                    else if (((int)Key.GetValue(exeName + ".exe")) != registryToSet)
                        Key.SetValue(exeName + ".exe", registryToSet, RegistryValueKind.DWord);
                }
                */
            }
        }
        #endregion

        #region font selecting testing
        private void FontSelectionCombobox_Loaded_1(object sender, RoutedEventArgs e)
        {
            string fontsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            List<Typeface> fonts = Fonts.GetTypefaces(fontsfolder).ToList();
            FontSelectionCombobox.Items.Clear();
            foreach(Typeface font in fonts)
            {
                FontSelectionCombobox.Items.Add(new TextBlock()
                {
                    FontFamily = font.FontFamily,
                    FontStretch = font.Stretch,
                    FontStyle = font.Style,
                    FontWeight = font.Weight,
                    Text = font.FontFamily.ToString()
                });
            }
        }

        private void FontSelectionCombobox_Loaded_2(object sender, RoutedEventArgs e)
        {
            string fontsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            List<FontFamily> fonts = Fonts.GetFontFamilies(fontsfolder).ToList();
            FontSelectionCombobox.Items.Clear();
            foreach (FontFamily font in fonts)
            {
                FontSelectionCombobox.Items.Add(new TextBlock()
                {
                    FontFamily = font,
                    Text = font.Source.Split('#')[1]
                });
            }
        }
        #endregion

        #region i hate comboboxes
        bool loaded = false;
        int selectionBackup = -1;

        private void SelectionTestingCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            if(!loaded)
            {
                //select the first item when loading
                SelectionTestingCombobox.Tag = false;
                SelectionTestingCombobox.SelectedIndex = 0;
                loaded = true;
            }
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBoxItem_Unselected(object sender, RoutedEventArgs e)
        {

        }

        private void SelectionTestingCombobox_DropDownOpened(object sender, EventArgs e)
        {
            selectionBackup = SelectionTestingCombobox.SelectedIndex;
            SelectionTestingCombobox.Tag = false;
            SelectionTestingCombobox.SelectedIndex = -1;
        }

        private void SelectionTestingCombobox_DropDownClosed(object sender, EventArgs e)
        {
            if (SelectionTestingCombobox.SelectedIndex == -1)
            {
                SelectionTestingCombobox.Tag = false;
                SelectionTestingCombobox.SelectedIndex = selectionBackup;
            }
        }

        private void SelectionTestingCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selectionTag = (bool)SelectionTestingCombobox.Tag;
            if(!selectionTag)
            {
                SelectionTestingCombobox.Tag = true;
                return;
            }
            else
            {

            }
        }

        private void SelectionTestingComboboxWithCommit_SelectionCommitted(object source, SelectionChangedEventArgs e)
        {

        }
        #endregion

        TestSubWindow window;
        private void MemoryLeakTestingButton_Click(object sender, RoutedEventArgs e)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
            window = new TestSubWindow();
            window.ShowDialog();
            //don't need to call Close() to free up memory. It won't.
            // only setting it to null will allow it to be collected
            //window.Close();
            window = null;
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();

            //in this current configuration, the TestSubWindow does *not* show up in the memory profiler
        }

        private void DownloadManagerTestingButton_Click(object sender, RoutedEventArgs e)
        {
            using (DownloadManager manager = new DownloadManager())
            {
                manager.DownloadPackages("https://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.exe", "RelhaxModpack.exe");
                DownloadManagerTestingHashResult.Text = "MD5 hash is " + manager.Hash;
            }
        }

        private void ThreadingTestButton_Click(object sender, RoutedEventArgs e)
        {
            Thread myThread = new Thread(() => {
                bool access = ThreadingTestButton.Dispatcher.CheckAccess();
                //the next line will fail with the exception: 
                //System.InvalidOperationException: 'The calling thread cannot access this object because a different thread owns it.'
                //only the thread that created the object can 
                //ThreadingTestButton.Content = "test";


                //an interesting note is that one could create a window, show it, and invoke the dispatcher to run
                //such that it tells the dispatcher of *this* thread to 'idle' (wait for UI input)
                //while it's technically multithreading, it doesn't allow parallelism within a window itself I.E:

                TestSubWindow tempWindow = new TestSubWindow();
                tempWindow.Show();
                System.Windows.Threading.Dispatcher.Run();

                //https://stackoverflow.com/a/1111485/3128017
                //https://stackoverflow.com/a/8669719/3128017

            });

            myThread.SetApartmentState(ApartmentState.STA);
            myThread.IsBackground = true;
            myThread.Start();
        }

        #region Browser session testing
        private void BrowserSessionTestStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BrowserSessionTestUrlTextbox.Text))
                return;

            BrowserSessionTestResultsTextbox.Text = "Starting browser...";

            SetRegistryKey(System.Diagnostics.Process.GetCurrentProcess().ProcessName, TestBrowse.Version.Major);

            SessionTestWebBrowser.ScriptErrorsSuppressed = true;

            SessionTestWebBrowser.DocumentCompleted += SessionTestWebBrowser_DocumentCompleted; ;

            SessionTestWebBrowser.Navigate(BrowserSessionTestUrlTextbox.Text);
        }

        private bool running = false;
        private async void SessionTestWebBrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (running)
                return;
            running = true;

            //get htmlpath to the button
            await Task.Delay(2000);

            string htmlText = SessionTestWebBrowser.Document.Body.OuterHtml;
            string downloadUrl = GetHtmlNode(htmlText, @"//a[contains(@class, 'button download-file-button') and contains(text(), 'Download file')]/@href", "href");
            downloadUrl = "https://disk.yandex.com" + downloadUrl;
            SessionTestWebBrowser.Navigate(downloadUrl);
            using (WebClient client = new WebClient() { })
            {
                if (File.Exists("LampLights.zip"))
                    File.Delete("LampLights.zip");
                ApplyHeaders(client);
                //keep-alive is set by default
                //TODO: cookies listed in string, probably need to parse
                client.Headers.Add(HttpRequestHeader.Cookie, SessionTestWebBrowser.Document.Cookie);
                //await client.DownloadFileTaskAsync(downloadUrl, "LampLights.zip");
            }

            running = false;
        }

        private async void HttpClientTestButtonYandex_Click(object sender, RoutedEventArgs e)
        {
            //https://stackoverflow.com/questions/45831967/http-call-in-c-sharp-is-returning-garbage-data-in-the-response
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            using (HttpClient client = new HttpClient(handler, true))
            {
                ApplyHeaders(client);
                HttpResponseMessage response = await client.GetAsync("https://disk.yandex.com/d/jBxXcD2fc6mLT");
                response.EnsureSuccessStatusCode();
                string responseBody1 = await response.Content.ReadAsStringAsync();
                File.WriteAllText("HttpClientResponse1.html", responseBody1);
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                //now parse the response for the download link
                string downloadUrl = GetHtmlNode(responseBody1, @"//a[contains(@class, 'button download-file-button') and contains(text(), 'Download file')]/@href", "href");
                downloadUrl = "https://disk.yandex.com" + downloadUrl;
                response = await client.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                if (File.Exists("LampLights.zip"))
                    File.Delete("LampLights.zip");
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (FileStream filestream = new FileStream("LampLights.zip", FileMode.Create))
                {
                    await (stream as MemoryStream).CopyToAsync(filestream);
                    filestream.Flush();
                }
                //string responseBody2 = await response.Content.ReadAsStringAsync();
                //File.WriteAllText("HttpClientResponse2.html", responseBody2);

            }
        }

        private async void HttpClientTestButtonGdrive_Click(object sender, RoutedEventArgs e)
        {
            CookieContainer cookieContainer = new CookieContainer()
            {

            };
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            using (HttpClient client = new HttpClient(handler, true))
            {
                ApplyHeaders(client);
                //https://wotsite.net/ozvuchka-dlya-world-of-tanks/12454-
                HttpResponseMessage response = await client.GetAsync("https://drive.google.com/uc?id=1ZNPXp2QhO5NjwAV50ges-hedSSgYOg1u&export=download");
                response.EnsureSuccessStatusCode();
                string responseHeaders = response.Headers.ToString();
                string responseBody1 = await response.Content.ReadAsStringAsync();
                File.WriteAllText("HttpClientResponseGdrive1.html", responseBody1);

                string downloadUrl = GetHtmlNode(responseBody1, @"//a[contains(@href, '1ZNPXp2QhO5NjwAV50ges-hedSSgYOg1u') and contains(@id, 'uc-download-link')]//@href", "href");
                downloadUrl = downloadUrl.Replace("&amp;", "&");
                downloadUrl = "https://drive.google.com" + downloadUrl;
                File.WriteAllText("directDownloadUrl.txt", downloadUrl);
                if (File.Exists("The Fast and the Furious.rar"))
                    File.Delete("The Fast and the Furious.rar");

                // OLD METHOD
                /*
                //response = await client.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (FileStream filestream = new FileStream("The Fast and the Furious.rar", FileMode.Create))
                {
                    await (stream as MemoryStream).CopyToAsync(filestream);
                    filestream.Flush();
                }
                */

                // NEW STREAM METHOD (allows progress report)
                response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                Stream fileDownload = await response.Content.ReadAsStreamAsync();
                long contentLength = (long)response.Content.Headers.ContentLength;
                using (FileStream filestream = new FileStream("The Fast and the Furious.rar", FileMode.Create))
                {
                    byte[] buffer = new byte[4096];
                    int ammountRead = await fileDownload.ReadAsync(buffer, 0, 4096);
                    while (ammountRead != 0)
                    {
                        await filestream.WriteAsync(buffer, 0, ammountRead);
                        ammountRead = await fileDownload.ReadAsync(buffer, 0, 4096);
                    }
                    await filestream.FlushAsync();
                }
            }
        }

        class MyWebClient : WebClient
        {
            CookieContainer c = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri u)
            {
                var r = (HttpWebRequest)base.GetWebRequest(u);
                r.CookieContainer = c;
                return r;
            }
        }

        //this is for showing how to do it via WebClient for a SO user: https://stackoverflow.com/questions/34323143/downloading-large-google-drive-files-with-webclient-in-c-sharp
        private async void WebClientTestButtonGdrive_Click(object sender, RoutedEventArgs e)
        {
            using (MyWebClient client = new MyWebClient())
            {
                //set headers (we don't actually need to for gdrive)
                //client.Headers.Add(HttpRequestHeader.Accept, "image/gif, image/jpeg, image/pjpeg, application/x-ms-application, application/xaml+xml, application/x-ms-xbap, */*");
                //client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US");
                //client.Headers.Add("Ua-Cpu", "AMD64");
                //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                //client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.2; Win64; x64; Trident/7.0; rv:11.0) like Gecko");
                //client.Headers.Add(HttpRequestHeader.Cookie, SessionTestWebBrowser.Document.Cookie);

                //get the warning page
                string htmlPage = await client.DownloadStringTaskAsync("https://drive.google.com/uc?id=1ZNPXp2QhO5NjwAV50ges-hedSSgYOg1u&export=download");

                //use HtmlAgilityPack to get the url with the confirm parameter in the url
                string downloadUrl = GetHtmlNode(htmlPage, @"//a[contains(@href, '1ZNPXp2QhO5NjwAV50ges-hedSSgYOg1u') and contains(@id, 'uc-download-link')]//@href", "href");
                downloadUrl = "https://drive.google.com" + downloadUrl;

                //download the file
                if (File.Exists("The Fast and the Furious.rar"))
                    File.Delete("The Fast and the Furious.rar");
                await client.DownloadFileTaskAsync(downloadUrl, "The Fast and the Furious.rar");
            }
        }

        private async void HttpClientTestButtonKR_Click(object sender, RoutedEventArgs e)
        {
            CookieContainer cookieContainer = new CookieContainer()
            {

            };
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            using (HttpClient client = new HttpClient(handler, true))
            {
                ApplyHeaders(client);
                HttpResponseMessage response = await client.GetAsync("https://koreanrandom.com/forum/login/");
                response.EnsureSuccessStatusCode();
                string responseBody1 = await response.Content.ReadAsStringAsync();
                string csrfKey = GetHtmlNode(responseBody1, @"//input[contains(@type, 'hidden') and contains(@name, 'csrfKey')]//@value", "value");
                string postString = string.Format("csrfKey={0}&auth={1}&password={2}&_processLogin=usernamepassword&_processLogin=usernamepassword", csrfKey, HttpClientTestUsernameBox.Text, HttpClientTestPasswordBox.Text);
                HttpContent httpContent = new StringContent(postString, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync("https://koreanrandom.com/forum/login/", httpContent);
                response = await client.GetAsync("https://koreanrandom.com/forum/topic/37259-%D0%BF%D1%80%D0%B8%D1%86%D0%B5%D0%BB-%D0%B1%D0%B5%D0%B7-%D1%81%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D1%8F-%D1%81%D1%80%D0%B5%D0%B4%D1%81%D1%82%D0%B2%D0%B0%D0%BC%D0%B8-xvm");
                responseBody1 = await response.Content.ReadAsStringAsync();
                File.WriteAllText("HttpClientResponseKR.html", responseBody1);
                string attachmentLink = GetHtmlNode(responseBody1, @"//a[@data-fileid = '147700']//@href", "href");
                //NOTE: if trying to download the attachment while not being logged in, it will fail on an exception representing http error 403
                //best practice to ALWAYS use EnsureSuccessStatusCode();
                //also does it for WebClient
                string pythonExtension = await client.GetStringAsync(attachmentLink);
                File.WriteAllText("targetInfo.py", pythonExtension);
            }
        }

        private void ApplyHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.ParseAdd("image/gif, image/jpeg, image/pjpeg, application/x-ms-application, application/xaml+xml, application/x-ms-xbap, */*");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");
            client.DefaultRequestHeaders.Add("Ua-Cpu", "AMD64");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.2; Win64; x64; Trident/7.0; rv:11.0) like Gecko");
        }

        private void ApplyHeaders(WebClient client)
        {
            client.Headers.Add(HttpRequestHeader.Accept, "image/gif, image/jpeg, image/pjpeg, application/x-ms-application, application/xaml+xml, application/x-ms-xbap, */*");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US");
            client.Headers.Add("Ua-Cpu", "AMD64");
            client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.2; Win64; x64; Trident/7.0; rv:11.0) like Gecko");
        }

        private string GetHtmlNode(string htmlPage, string htmlPath, string attribute)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlPage);
            HtmlNode node = document.DocumentNode;
            HtmlNode urlNode = node.SelectSingleNode(htmlPath);
            if (urlNode == null)
                return null;
            string downloadUrl = urlNode.Attributes[attribute].Value;
            downloadUrl = downloadUrl.Replace("&amp;", "&");
            return downloadUrl;
        }
        #endregion

        #region dark theme testing in a way that doesn't suck
        private bool _darkTheme = false;
        public bool DarkTheme
        {
            get { return _darkTheme; }
            set
            {
                _darkTheme = value;
                OnPropertyChanged(nameof(DarkTheme));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void darkThemeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            DarkTheme = (bool)darkThemeCheckbox.IsChecked;
        }
        #endregion
    }
}
