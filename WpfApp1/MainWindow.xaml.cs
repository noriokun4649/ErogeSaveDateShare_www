using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static String file_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/エロゲのセーブデータ共有したったｗｗｗ/eroge_game_lists.csv";
        public static String folder_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/エロゲのセーブデータ共有したったｗｗｗ";
        static String users = "";
        bool isAcces = false;

        public MainWindow()
        {
            InitializeComponent();
            checkbox_drop.IsChecked = Properties.Settings.Default.dropbox_snyc;
            checkbox_pc.IsChecked = Properties.Settings.Default.pc_snyc;
            String ac = Properties.Settings.Default.AccessToken;
            if (ac != "")
            {
                isAcces = true;
                var task = Task.Run((Func<Task>)MainWindow.Run);
                task.Wait();
                text_now.Text = users;
                connect.Content = "連携を解除する";
            }
            else
            {
                isAcces = false;
                text_now.Text = "連携されていません";
                connect.Content = "DropBoxと連携する";
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
                var full = await dbx.Users.GetCurrentAccountAsync();
                users = full.Name.DisplayName;
                //dbx.FileRequests.
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isAcces)
            {
                MessageBoxResult result = MessageBox.Show("DropBoxとすでに連携されています。\n\n連携を解除しますか？",
            "DropBox連携",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.AccessToken = "";
                    Properties.Settings.Default.Uid = "";
                    Properties.Settings.Default.Save();

                    var win_log = new LoginForm("y56r29wopnmikhs")
                    {
                        Title = "DropBoxと連携する"
                    };
                    MessageBox.Show("連携を解除しました", "DropBox連携", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (Properties.Settings.Default.AccessToken == "")
                    {
                        text_now.Text = "連携されていません";
                        isAcces = false;
                        connect.Content = "DropBoxと連携する";
                    }


                }
                else if (result == MessageBoxResult.No)
                {

                }

            }
            else
            {

                MessageBoxResult result = MessageBox.Show("DropBoxと連携を行います。\n連携には最新のInternetExplorerが\nインストールされている必要があります\n\n連携をしますか？",
                "DropBox連携",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.dropbox_conectings = true;
                    var win_log = new LoginForm("y56r29wopnmikhs")
                    {
                        Title = "DropBoxと連携する"
                    };
                    win_log.RefreshEvent += delegate (object senders, EventArgs e2)
                    {


                        var task = Task.Run((Func<Task>)MainWindow.Run);
                        task.Wait();
                        text_now.Text = users;
                        connect.Content = "連携を解除する";

                    };
                    win_log.ShowDialog();

                }
                else if (result == MessageBoxResult.No)
                {

                }
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;

            Properties.Settings.Default.pc_snyc = (bool)c.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;

            Properties.Settings.Default.dropbox_snyc = (bool)c.IsChecked;
            Properties.Settings.Default.Save();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var win = new AddWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("DropBox上に現在のPCに保存されているセーブデータをアップロードします。\n\nDropBox上のセーブデータは上書きされます。続行しますか？",
            "DropBoxへアップロード",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);
            DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
            if (result == MessageBoxResult.Yes)
            {
                
                var window = new LogWindow(client,1);
                window.Owner = this;
                window.Show();
            }
            else if (result == MessageBoxResult.No)
            {

            }

        }
        private async Task Upload(DropboxClient client,string fileName, string fileContent)
        {
            //MessageBox.Show(fileName + "のアップロードを開始しました",
                //"メッセージ",
                //MessageBoxButton.OK,
                //MessageBoxImage.Information);
            try
            {
                //Console.WriteLine("Upload file...");
                FileStream fileStream = new FileStream(fileContent, FileMode.Open);
                var response = await client.Files.UploadAsync("/" + fileName, WriteMode.Overwrite.Instance, body: fileStream);
                //Console.WriteLine("Uploaded Id {0} Rev {1}", response.Id, response.Rev);
                MessageBox.Show("設定のアップロードが完了しました。\n\nこの設定を引継ぎたいコンピュータで「設定のダウンロード」を押し、表示されるメッセージに従ってください。", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
                fileStream.Close();
            }
            catch (BadInputException exs)
            {
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            
            catch (Exception ex2)
            {
                MessageBox.Show(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }
        }

        
        private async Task Download_setting(DropboxClient client, string file)
        {
            //Console.WriteLine("Download file...");
            //MessageBox.Show(file + "のダウンロードを開始しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                using (var response = await client.Files.DownloadAsync("/" + file))
                {
                    FileStream fileStream = new FileStream(file_path, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fileStream);
                    sw.Write(await response.GetContentAsStringAsync());
                    sw.Close();
                    fileStream.Close();
                    MessageBox.Show("設定のダウンロードが完了しました。\n\nゲーム追加・管理にてこのコンピュータに合わせたセーブデータ場所を設定しなおしてください。", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (BadInputException exs)
            {
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
 "無効なHTTPリクエスト",
 MessageBoxButton.OK,
 MessageBoxImage.Error);
            }
            catch (ApiException<DownloadError> ex)
            {
                MessageBox.Show(ex.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("PCにDropBox上のセーブデータをダウンロードします。\n\nPC上のセーブデータは上書きされます。続行しますか？",
            "PCへダウンロード",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);
            DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);

            if (result == MessageBoxResult.Yes)
            {
                var window = new LogWindow(client,0);
                window.Owner = this;
                window.Show();
            }
            else if (result == MessageBoxResult.No)
            {
            }
        }
        


        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("DropBox上に現在のPCの設定をアップロードします。\n\nDropBox上の設定は上書きされます。続行しますか？",
                "DropBoxへアップロード",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
                Console.WriteLine(Path.GetPathRoot(file_path) + Path.GetFileName(file_path) + file_path);
                Upload(client, Path.GetFileName(file_path), file_path);


            }
            else if (result == MessageBoxResult.No)
            {

            }


        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("PCにDropBox上の設定をダウンロードします。\n\nPC上にある現在の設定は上書きされます。続行しますか？",
            "PCへダウンロード",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {

                DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
                FileMetadata fileMetadata = new FileMetadata();

                Download_setting(client, Path.GetFileName(file_path));
            }
            else if (result == MessageBoxResult.No)
            {

            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Window win = new AppInfo();
            win.Owner = this;
            win.ShowDialog();
        }
    }
}
