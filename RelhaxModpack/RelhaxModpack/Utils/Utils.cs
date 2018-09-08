using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utils
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
    }
}
