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
        LogWindow t;
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
        public void SetParent(LogWindow parent)
        {
            t = parent;
        }
        public void SetParameter(string game_title,string message)
        {
            Massage.Text = message;
            //checks.Content = ;
            TextBlock tb = new TextBlock();
            tb.Text = game_title + "の処理で、この選択を記憶する";
            tb.TextWrapping = TextWrapping.WrapWithOverflow;

            checks.Name = "checkBox";
            checks.Content = tb;

            //grid.Children.Add(checks);

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            t.SetCheck((bool)checks.IsChecked);
        }
    }
}
