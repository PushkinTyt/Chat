using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SNews
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //System.Threading.Mutex mut;
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // запуск одной копии приложения
            //bool createdNew;
            //string mutName = "Приложение";
            //mut = new System.Threading.Mutex(true, mutName, out createdNew);
            //if (!createdNew)
            //{
            //    Shutdown();
            //}
            

            
        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

        

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Logger.Close();
        }
    }
}
