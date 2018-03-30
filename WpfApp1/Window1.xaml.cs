using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        UploadPage u;
        DownloadPage d;
        public Window1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        public void SetUpParent(UploadPage parent)
        {
            u = parent;
        }
        public void SetDownParent(DownloadPage parent)
        {
            d = parent;
        }
        public void SetParameter(string game_title,string message)
        {
            Massage.Text = message;
            //checks.Content = ;
            TextBlock tb = new TextBlock
            {
                Text = game_title + "の処理で、この選択を記憶する",
                TextWrapping = TextWrapping.WrapWithOverflow
            };

            checks.Name = "checkBox";
            checks.Content = tb;
            //grid.Children.Add(checks);

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (u != null)
            {
                u.SetCheck((bool)checks.IsChecked);
            }
            if (d != null)
            {
                d.SetCheck((bool)checks.IsChecked);
            }
            
        }
    }
}
