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

        private void checks_Checked(object sender, RoutedEventArgs e)
        {
           CheckBox checks = (CheckBox)sender;
          t.SetCheck((bool)checks.IsChecked);
        }
    }
}
