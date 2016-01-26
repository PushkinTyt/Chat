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
using System.Windows.Shapes;
using CommunicationTools;
using System.Configuration;
using System.IO;
using System.Threading;

namespace SNews
{
    /// <summary>
    /// Логика взаимодействия для ReferateView.xaml
    /// </summary>
    public partial class ReferateView : Window
    {
        Thread threadForRef=null;
        bool Debag=false;
        private string URL;
        private string refServIP;
        public ReferateView()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                Properties.Resources.icon.Save(memory);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                Icon = bitmapImage;
            }
            InitializeComponent();


            slider.Value = 0;

            textBlock.Text = "Запуск без запроса на реферирования";

            Debag = true;
            

        }
        ///запус потока
        private void threadForRef_start(byte compressPercent)
        {
            
            textBlock.Dispatcher.Invoke(new Action(() => textBlock.Text = "Статья на сжатии..."));
            button.Dispatcher.Invoke(new Action(() => button.Visibility = System.Windows.Visibility.Hidden));
            int port = Int32.Parse(ConfigurationManager.AppSettings["refServerPort"].ToString());
            TCPClient refSever = new TCPClient(refServIP, port);
            string url = URL;

            string message = url + "|" + compressPercent;

            MetaData md = new MetaData(MetaData.Roles.client, MetaData.Actions.refNews, MetaData.ContentTypes.link, message);
            refSever.Send(message, md);
            string response = refSever.ReceiveSyncData(0);
            textBlock.Dispatcher.Invoke(new Action (() => textBlock.Text = response));
            button.Dispatcher.Invoke(new Action(() => button.Visibility = System.Windows.Visibility.Visible));
        }
        
        public ReferateView(string URL, string refServIP)
        {
            
            using (MemoryStream memory = new MemoryStream())
            {
                Properties.Resources.icon.Save(memory);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                Icon = bitmapImage;
            }
            this.URL = URL;
            this.refServIP = refServIP;
            InitializeComponent();


            slider.Value = 50;
            byte compressPercent = (byte)(int)slider.Value;
            threadForRef = new Thread(() =>
            {
                threadForRef_start(compressPercent);
            });
            threadForRef.Start();


        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            label.Content = "Степень сжатия " + (int)slider.Value + "%";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            byte compressPercent = (byte)(int)slider.Value;
            threadForRef = new Thread(() =>
            {
                threadForRef_start(compressPercent);
            });
            threadForRef.Start();
        }
    }
}
