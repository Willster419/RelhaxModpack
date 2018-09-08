using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack
{
    /// <summary>
    /// Application return error codes
    /// </summary>
    public enum ReturnCodes
    {
        /// <summary>
        /// No error occured
        /// </summary>
        Sucess = 0,
        /// <summary>
        /// Error with logfile creation
        /// </summary>
        LogfileError = 1
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //when application is brought to forground
        private void Application_Activated(object sender, EventArgs e)
        {

        }

        //when application is sent to background (not active window)?
        private void Application_Deactivated(object sender, EventArgs e)
        {

        }

        //when application is closing (cannot be stopped)
        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

        //when application is starting for first time
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init loggine here
            if(!Logging.Logging.InitApplicationLogging())
            {
                MessageBox.Show("The application failed to open a logfile. Eithor check your file permissions or move the application to a folder with write access");
                this.Shutdown((int)ReturnCodes.LogfileError);
            }
            //parse command line arguements here

            //load external libraries here

        }
    }
}
