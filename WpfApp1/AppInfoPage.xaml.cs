using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// AppInfoPage.xaml の相互作用ロジック
    /// </summary>
    public partial class AppInfoPage : Page
    {
        public AppInfoPage()
        {
            InitializeComponent(); System.Reflection.Assembly assembly = Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName asmName = assembly.GetName();
            System.Version versions = asmName.Version;
            version.Text = "Version V." + versions.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)//blog
        {

            try
            {
                System.Diagnostics.Process.Start("https://noriokun4649.blog.fc2.com/");
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//twitter
        {
            try
            {
                System.Diagnostics.Process.Start("https://twitter.com/noriokun_blog");
            }
            catch { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)//Mail
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)//Git
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/noriokun4649/ErogeSaveDateShare_www");
            }
            catch { }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)//Drop
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/dropbox/dropbox-sdk-dotnet");
            }
            catch { }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)//meta
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/ButchersBoy/MaterialDesignInXamlToolkit");
            }
            catch { }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)//win
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/aybe/Windows-API-Code-Pack-1.1");
            }
            catch { }
        }
    }
}
