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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Window2.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class Main2 : Window
    {
        private List<Uri> _uriList = new List<Uri>() {
            new Uri("HomePage.xaml",UriKind.Relative),
            new Uri("AddPage.xaml",UriKind.Relative),
            new Uri("UploadPage.xaml",UriKind.Relative),
            new Uri("DownloadPage.xaml",UriKind.Relative),
            new Uri("DropBoxListPage.xaml",UriKind.Relative),
            new Uri("AppInfoPage.xaml",UriKind.Relative)
        };
        private NavigationService _navi;
        public Main2()
        {
            InitializeComponent();
            _navi = this.frame.NavigationService;
            
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[0]);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[0]);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[1]);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[2]);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[3]);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[4]);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _navi.Navigate(_uriList[5]);
        }
    }
}
