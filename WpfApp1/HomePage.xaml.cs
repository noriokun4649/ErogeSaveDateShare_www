using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    /// HomePage.xaml の相互作用ロジック
    /// </summary>
    public partial class HomePage : Page
    {
        private ResourceDictionary dict;
        static string users = "";
        public static string file_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/エロゲのセーブデータ共有したったｗｗｗ/eroge_game_lists.csv";
        public static string folder_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/エロゲのセーブデータ共有したったｗｗｗ";
        public HomePage()
        {
            InitializeComponent();
            Color_mode.IsChecked = Properties.Settings.Default.color;
            ColorChange();
            checkbox_drop.IsChecked = Properties.Settings.Default.dropbox_snyc;
            checkbox_pc.IsChecked = Properties.Settings.Default.pc_snyc;
            string ac = Properties.Settings.Default.AccessToken;
            if (ac != "")
            {
                var task = Task.Run((Func<Task>)HomePage.Run);
                task.Wait();
                text_now.Text = users;
                delete_link.IsEnabled = true;
                link.IsEnabled = false;
            }
            else
            {
                text_now.Text = "リンクされていません";
                delete_link.IsEnabled = false;
                link.IsEnabled = true;
            }
            try
            {
                if (!File.Exists(folder_path))
                {
                    Directory.CreateDirectory(folder_path);

                    if (!File.Exists(file_path))
                    {
                        File.CreateText(file_path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }
        }

        public static async Task Run()
        {
            using (var dbx = new DropboxClient(Properties.Settings.Default.AccessToken))
            {
                try
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    users = full.Name.DisplayName;
                    //dbx.FileRequests.
                }
                catch (WebException exs)
                {
                    MessageBox.Show("ネットワークエラーが発生しました。\n\n" + exs.Message, "エロゲのセーブデータ共有したったｗｗｗ", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = "DropBox接続エラー";
                }
                catch (HttpRequestException exx)
                {
                    MessageBox.Show("HTTPリクエストに問題が発生しました。コンピュータがインターネットに接続されているか確認してください。\n\n" + exx.Message, "エロゲのセーブデータ共有したったｗｗｗ", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = "DropBox接続エラー";
                }
                catch (InvalidOperationException exss)
                {
                    MessageBox.Show("無効な呼び出しが発生しました。\n\n" + exss.Message, "エロゲのセーブデータ共有したったｗｗｗ", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = "DropBox接続エラー";
                }
                catch (ArgumentException ers)
                {
                    MessageBox.Show("問題が発生しました。\n\n" + ers.Message, "エロゲのセーブデータ共有したったｗｗｗ", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = "DropBox接続エラー";
                }
                catch (Exception ext)
                {
                    MessageBox.Show("エラーが発生しました。\n\n" + ext.Message, "エロゲのセーブデータ共有したったｗｗｗ", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = "DropBox接続エラー";
                }
            }
        }

        private void Color_mode_Checked(object sender, RoutedEventArgs e)
        {
            ColorChange();
        }
        private void ColorChange()
        {
            if (dict == null)
            {
                dict = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            if (Color_mode.IsChecked == true)
            {
                dict.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
                Properties.Settings.Default.color = true;
            }
            else
            {
                dict.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");
                Properties.Settings.Default.color = false;
            }
            Properties.Settings.Default.Save();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;

            Properties.Settings.Default.pc_snyc = (bool)c.IsChecked;
            Properties.Settings.Default.Save();
            if (c.IsChecked == true)
            {
                MessageBox.Show("PC上のファイルは更新日時を、DropBox上のファイルはアップロードされた日時をもとに判断するため、ゲーム内でのセーブ日時と噛み合わない場合がありますがご了承ください。", "注意事項", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;

            Properties.Settings.Default.dropbox_snyc = (bool)c.IsChecked;
            Properties.Settings.Default.Save();
            if (c.IsChecked == true)
            {
                MessageBox.Show("PC上のファイルは更新日時を、DropBox上のファイルはアップロードされた日時をもとに判断するため、ゲーム内でのセーブ日時と噛み合わない場合がありますがご了承ください。", "注意事項", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult result = MessageBox.Show("DropBoxとリンクを行います。\nリンクには最新のInternetExplorerが\nインストールされている必要があります\n\nリンクをしますか？",
            "DropBoxリンク",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.dropbox_conectings = true;
                var win_log = new LoginForm("y56r29wopnmikhs")
                {
                    Title = "DropBoxとリンクする"
                };
                win_log.RefreshEvent += delegate (object senders, EventArgs e2)
                {
                    var task = Task.Run((Func<Task>)MainWindow.Run);
                    task.Wait();
                    text_now.Text = users;
                    delete_link.IsEnabled = true;
                    link.IsEnabled = false;
                };
                win_log.ShowDialog();

            }
        }

        private void delete_link_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("本当にリンクを解除しますか？",
            "DropBoxリンク",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.AccessToken = "";
                Properties.Settings.Default.Uid = "";
                Properties.Settings.Default.Save();
                MessageBox.Show("リンクを解除しました", "DropBoxリンク", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Properties.Settings.Default.AccessToken == "")
                {
                    text_now.Text = "リンクされていません";
                    delete_link.IsEnabled = false;
                    link.IsEnabled = true;
                }


            }
            else if (result == MessageBoxResult.No)
            {

            }
        }
    }
}
