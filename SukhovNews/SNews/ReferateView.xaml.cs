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

namespace SNews
{
    /// <summary>
    /// Логика взаимодействия для ReferateView.xaml
    /// </summary>
    public partial class ReferateView : Window
    {
        TCPClient client;

        private string URL;
        private string refServIP;
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

            
            
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            label.Content = "Степень сжатия " + (int)slider.Value + "%";
        }

        
       

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            byte compressPercent = (byte)(int)slider.Value;


            int port = Int32.Parse(ConfigurationManager.AppSettings["refServerPort"].ToString());
            TCPClient refSever = new TCPClient(refServIP, port);
            string url = URL;

            string message = url + "|" + compressPercent;

            MetaData md = new MetaData(MetaData.Roles.client, MetaData.Actions.refNews, MetaData.ContentTypes.link, message);
            refSever.Send(message, md);
            string response = refSever.ReceiveSyncData(0);
            textBlock.Text = response;
        }

        
    }
}
