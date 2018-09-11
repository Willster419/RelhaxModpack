using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack
{
    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// A generic Progress Indicator window to be used for when loading things on a UI thread
        /// Use this in a using statement
        /// </summary>
        public static Windows.ProgressIndicator ProgressIndicator;
        /// <summary>
        /// Return the entire assembely version
        /// </summary>
        /// <returns>The entire assembely version string (major, minor, build, revision)</returns>
        public static string GetApplicationVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        /// <summary>
        /// Return the date and time in EN-US form, the time that the application was built
        /// </summary>
        /// <returns>the application build date and time in EN-US form</returns>
        public static string GetCompileTime()
        {
            return CiInfo.BuildTag + " (EN-US date format)";
        }

        public static List<Visual> GetAllWindowComponents(Window window, bool includeWindow)
        {
            //https://stackoverflow.com/questions/874380/wpf-how-do-i-loop-through-the-all-controls-in-a-window
            List<Visual> allWindowComponents = new List<Visual>();
            if (includeWindow)
                allWindowComponents.Add(window);
            if (VisualTreeHelper.GetChildrenCount(window) > 0)
                GetAllWindowComponents(window, allWindowComponents);
            return allWindowComponents;
        }

        private static void GetAllWindowComponents(Visual v, List<Visual> allWindowComponents)
        {
            int ChildrenComponents = VisualTreeHelper.GetChildrenCount(v);
            for (int i = 0; i < ChildrenComponents; i++)
            {
                Visual subV = (Visual)VisualTreeHelper.GetChild(v, i);
                allWindowComponents.Add(subV);
                if (VisualTreeHelper.GetChildrenCount(subV) > 0)
                    GetAllWindowComponents(subV, allWindowComponents);
            }
        }
    }
}
