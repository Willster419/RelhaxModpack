using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// A utility class to handle UI interaction
    /// </summary>
    public static class UiUtils
    {
        /// <summary>
        /// Get a list of all visual components in the window
        /// </summary>
        /// <param name="window">The window to get the list of</param>
        /// <param name="includeWindow">if the list should include the window itself</param>
        /// <returns>A list of type FrameowrkElement of all components</returns>
        public static List<FrameworkElement> GetAllWindowComponentsVisual(Window window, bool includeWindow)
        {
            //https://stackoverflow.com/questions/874380/wpf-how-do-i-loop-through-the-all-controls-in-a-window
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            if (VisualTreeHelper.GetChildrenCount(window) > 0)
                GetAllWindowComponentsVisual(window, windowComponents);
            return windowComponents;
        }

        /// <summary>
        /// Get a list of all logical components in the window
        /// </summary>
        /// <param name="window">The window to get the list of</param>
        /// <param name="includeWindow">if the list should include the window itself</param>
        /// <returns>A list of type FrameowrkElement of all components</returns>
        public static List<FrameworkElement> GetAllWindowComponentsLogical(Window window, bool includeWindow)
        {
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            GetAllWindowComponentsLogical(window, windowComponents);
            return windowComponents;
        }

        /// <summary>
        /// Get a list of all logical components in the window
        /// </summary>
        /// <param name="rootElement">The element to get the list of logical items from</param>
        /// <param name="addRoot">If this rootElement should be added to the list</param>
        /// <returns></returns>
        public static List<FrameworkElement> GetAllWindowComponentsLogical(FrameworkElement rootElement, bool addRoot)
        {
            List<FrameworkElement> components = new List<FrameworkElement>();
            if (addRoot)
                components.Add(rootElement);
            GetAllWindowComponentsLogical(rootElement, components);
            return components;
        }

        //A recursive method for navigating the visual tree
        private static void GetAllWindowComponentsVisual(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            int ChildrenComponents = VisualTreeHelper.GetChildrenCount(v);
            for (int i = 0; i < ChildrenComponents; i++)
            {
                DependencyObject dep = VisualTreeHelper.GetChild(v, i);
                if (!(dep is FrameworkElement))
                {
                    continue;
                }
                FrameworkElement subV = (FrameworkElement)VisualTreeHelper.GetChild(v, i);
                allWindowComponents.Add(subV);
                if (subV is TabControl tabControl)
                {
                    foreach (FrameworkElement tabVisual in tabControl.Items)
                    {
                        allWindowComponents.Add(tabVisual);
                        GetAllWindowComponentsLogical(tabVisual, allWindowComponents);
                    }
                }
                int childrenCount = VisualTreeHelper.GetChildrenCount(subV);
                if (childrenCount > 0)
                    GetAllWindowComponentsVisual(subV, allWindowComponents);
            }
        }

        //Gets any logical components that are not currently shown (like elements behind a tab)
        private static void GetAllWindowComponentsLogical(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            //NOTE: v has been added
            //have to use var here cause i got NO CLUE what type it is #niceMeme
            if (v == null)
            {
                Logging.Error("parameter \"v\" is null, skipping");
                return;
            }
            var children = LogicalTreeHelper.GetChildren(v);
            //Type temp = children.GetType();
            foreach (var child in children)
            {
                //Type temp2 = child.GetType();
                if (child is FrameworkElement childVisual)
                {
                    allWindowComponents.Add(childVisual);
                    GetAllWindowComponentsLogical(childVisual, allWindowComponents);
                }
            }
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        public static bool PointWithinScreen(int x, int y)
        {
            return PointWithinScreen(new System.Drawing.Point(x, y));
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="p">The point to check</param>
        public static bool PointWithinScreen(System.Drawing.Point p)
        {
            //if either x or y are negative it's an invalid location
            if (p.X < 0 || p.Y < 0)
                return false;
            int totalWidth = 0, totalHeight = 0;
            foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                totalWidth += s.Bounds.Width;
                totalHeight += s.Bounds.Height;
            }
            if (totalWidth > p.X && totalHeight > p.Y)
                return true;
            return false;
        }


        /// <summary>
        /// Injects a Dispatcher frame followed by an idle backgrouned operation to allow for the UI to update during an intensive operation on the UI thread
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread 
        /// <para>and https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this </para></remarks>
        public static void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);

            //EDIT:
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        /// <summary>
        /// Applies vector based application scaling to the specified window
        /// </summary>
        /// <param name="window">The window to apply scaling to</param>
        /// <param name="scaleValue">The amount of scaling, in a multiplication factor, to apply to the window from</param>
        public static void ApplyApplicationScale(Window window, double scaleValue)
        {
            //input filtering
            if (scaleValue < Settings.MinimumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to small, setting to 1", scaleValue.ToString("N"));
                scaleValue = Settings.MinimumDisplayScale;
            }
            if (scaleValue > Settings.MaximumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to large, setting to 3", scaleValue.ToString("N"));
                scaleValue = Settings.MaximumDisplayScale;
            }

            //scale internals
            (window.Content as FrameworkElement).LayoutTransform = new ScaleTransform(scaleValue, scaleValue, 0, 0);

            //scale window itself
            if (window is MainWindow mw)
            {
                mw.Width = mw.OriginalWidth * scaleValue;
                mw.Height = mw.OriginalHeight * scaleValue;
            }
            else if (window is RelhaxWindow rw)
            {
                rw.Width = rw.OriginalWidth * scaleValue;
                rw.Height = rw.OriginalHeight * scaleValue;
            }
            else
                throw new BadMemeException("you should probably make me a RelhaxWindow if you want to use this feature");
        }

        public static FontFamily DefaultFontFamily = null;

        public static FontFamily CustomFontFamily = null;

        /// <summary>
        /// Applies the given FontFamily font type to the window
        /// </summary>
        /// <param name="window">The window to apply the font to</param>
        /// <param name="font">The font to apply to the window</param>
        /// <exception cref="ArgumentNullException">When any argument is null</exception>
        public static void ApplyFontToWindow(Window window, FontFamily font)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if(DefaultFontFamily == null)
            {
                DefaultFontFamily = window.FontFamily;
            }

            string[] fontName = font.Source.Split('#');
            string fontname_ = string.Empty;
            if (fontName.Length > 1)
                fontname_ = fontName[1];
            else
                fontname_ = fontName[0];

            Logging.Debug(LogOptions.MethodName, "Applying font '{0}' to window {1}", fontname_, window.Title);
            window.FontFamily = font;
            foreach(FrameworkElement element in GetAllWindowComponentsLogical(window,true))
            {
                if(element is Control control)
                    control.FontFamily = font;
            }
        }
    }
}
