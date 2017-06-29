using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LaserHarp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            this.DispatcherUnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void CurrentDomain_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppContext.GetContext().DestroyContext();
            System.Windows.MessageBox.Show(MainWindow,e.Exception.Message);
            MainWindow.Close();
        }
    }
}
