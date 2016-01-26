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

namespace SNews
{
    /// <summary>
    /// Логика взаимодействия для ReferateView.xaml
    /// </summary>
    public partial class ReferateView : Window
    {
        TCPClient client;

        private string URL;
        public ReferateView(string URL, string refServIP, int port)
        {
            this.URL = URL;
            InitializeComponent();

        }
    }
}
