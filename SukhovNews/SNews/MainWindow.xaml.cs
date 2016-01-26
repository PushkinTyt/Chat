using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Rss;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CommunicationTools;
using System.Net;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace SNews
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<RssChannel> rssChanels;
        private UDPClient broadCast;
        private string ipDispatcher;
        private CB.DailyInfoSoapClient CBServis;
        //private ObservableCollection<RssItem> listViewCollection;
        public MainWindow()
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
            
            string fullPath = AppDomain.CurrentDomain.BaseDirectory;
            InitializeComponent();
            
            CBServis = new CB.DailyInfoSoapClient();
            try
            {
                System.Data.DataSet Curs = CBServis.GetCursOnDate(System.DateTime.Now);
                TextUS.Content = Curs.Tables[0].Rows[9].ItemArray[2].ToString();
                TextEVR.Content = Curs.Tables[0].Rows[10].ItemArray[2].ToString();
            }
            catch (Exception )
            {
                TextUS.Content = "Не доступно";
                TextEVR.Content = TextUS.Content;


            }



            ipDispatcher = "";

            rssChanels = this.LoadChannelsFromFile("RssChannels.bin");
            Logger.Write("загружены из файла rss каналы");
            this.UpdateChannels();
            //listViewCollection = new ObservableCollection<RssItem>(rssChanels[0].Articles);
            lvArticles.ItemsSource = rssChanels[0].Articles;


            // аля неудачная попытка сделать красивый интерфейс
            // create buttons for articles categories
            //for (int i = 0; i < rssChanels.Count; i++)
            //{
            //    var btnCateg = new Button() { Name = "btnCat" + i, Content = rssChanels[i].Category };
            //    btnCateg.Click += RssCateg_Click;
            //    btnCateg.Uid = i.ToString();
            //    btnCateg.Style = this.FindResource("channelBtn") as Style;
            //    CategoryWrap.Children.Add(btnCateg);
            //}

            foreach (RssChannel channel in rssChanels)
            {
                cmbCategoryList.Items.Add(channel.Category);
            }
            cmbCategoryList.SelectedIndex = 0;
            broadCast = new UDPClient();
            broadCast.onMessage += this.UDP_Receive;
            broadCast.Start();
            Logger.Write("запущен поток на прослушку UDP");
        }

        /// <summary>
        /// срабатывает когда получен udp пакет от диспетчера
        /// </summary>
        private void UDP_Receive(IPEndPoint endPoint, string message)
        {
            
            Logger.Write(String.Format("принят пакет от диспетчера с адресом {0} содержимое сообщения: '{1}'",endPoint.Address.ToString(), message) );
            this.ipDispatcher = endPoint.Address.ToString();
            broadCast.Stop();
        }
            
        // show rss items in Listview
        private void RssCateg_Click( object sender, EventArgs e)
        {
            var locSender = (Button)sender;
            int id = Int32.Parse(locSender.Uid);
            lvArticles.ItemsSource = rssChanels[id].Articles;

        }

        /// <summary>
        /// Обновление списка каналов в ListView
        /// </summary>
        private void UpdateChannels_Click(object sender, EventArgs e)
        {
            this.UpdateChannels();
        }

        /// <summary>
        /// загрузка списка каналов из файла (сериализованный бинарник)
        /// </summary>
        /// <param name="путь к файлу"></param>
        /// <returns>коллекция List RssChannel</returns>
        private List<RssChannel> LoadChannelsFromFile(string fName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            List<RssChannel> rss;
            using (FileStream fs = new FileStream(fName, FileMode.Open))
            {
                rss = (List<RssChannel>)bf.Deserialize(fs);
            }

            return rss;
        }


        /// <summary>
        /// Обновление списка каналов
        /// </summary>
        private void UpdateChannels()
        {
            try
            {
                foreach (RssChannel channel in rssChanels)
                {
                    channel.Update();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void cmbCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lvArticles.ItemsSource = rssChanels[cmbCategoryList.SelectedIndex].Articles;
        }

        private void fullTextArticle_Click(object sender, EventArgs e)
        {
            if (lvArticles.SelectedIndex < 0)
            {
                MessageBox.Show("выберите статью");
                return;
            }
            string url = rssChanels[cmbCategoryList.SelectedIndex].Articles[lvArticles.SelectedIndex].link;
            System.Diagnostics.Process.Start(url);
            //HtmlParser hp = new HtmlParser(url);
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            // ... Create a new BitmapImage.
            using (MemoryStream memory = new MemoryStream())
            {
                Properties.Resources.Us.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                imageUS.Source = bitmapImage;
            }
            using (MemoryStream memory = new MemoryStream())
            {
                Properties.Resources.ev.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                imageEVR.Source = bitmapImage;
            }
            
            

        }

        private void CBSersicView_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextUS.Content = "Загрузка...";
            TextEVR.Content = "Загрузка...";
            try
            {
                System.Data.DataSet Curs = CBServis.GetCursOnDate(System.DateTime.Now);
                TextUS.Content = Curs.Tables[0].Rows[9].ItemArray[2].ToString();
                TextEVR.Content = Curs.Tables[0].Rows[10].ItemArray[2].ToString();
            }
            catch (Exception)
            {
                TextUS.Content = "Не доступно";
                TextEVR.Content = TextUS.Content;
             
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(broadCast != null)
            {
                broadCast.Stop();
            }
        }

       


        // GABARGE:
        //foreach (RssChannel rssChannel in rssChanels)
        //{
        //    Debug.Print(String.Format("категория: {0} url: {1}", rssChannel.Category, rssChannel.Url));
        //}

        //using (StreamReader sr = new StreamReader("rss.txt"))
        //{
        //    while (!sr.EndOfStream)
        //    {
        //        string curLine = sr.ReadLine();
        //        //Regex re = new Regex("(.*),(.*)");

        //        Match match = Regex.Matches(curLine, "(.*),(.*)")[0];
        //        rssChanels.Add(new RssChannel(match.Groups[2].Value, match.Groups[1].Value));
        //    }
        //}
        //foreach (RssChannel rssChannel in rssChanels)
        //{
        //    Debug.Print(String.Format("категория: {0} url: {1}", rssChannel.Category, rssChannel.Url));
        //}

        //BinaryFormatter sf = new BinaryFormatter();
        //using (FileStream fs = new FileStream("RssChannels.bin", FileMode.Create))
        //{
        //    sf.Serialize(fs, rssChanels);
        //}
    }
}
