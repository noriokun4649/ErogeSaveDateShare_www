using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    /// <summary>
    /// AppInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class AppInfo : Window
    {
        public AppInfo()
        {
            InitializeComponent();
            System.Reflection.Assembly assembly = Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName asmName = assembly.GetName();
            System.Version versions = asmName.Version;
            version.Text = "Version V."+ versions.ToString();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://restsharp.org");
            }
            catch { }
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/dropbox/dropbox-sdk-dotnet");
            }
            catch { }
        }
        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://noriokun4649.blog.fc2.com/");
            }
            catch { }
        }

        private void Hyperlink_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://twitter.com/noriokun_blog");
            }
            catch { }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://sites.google.com/view/eroge-save-date-share-www/");
            }
            catch { }
        }
    }
}
