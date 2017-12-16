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
        string[] rest;

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

            if (result == MessageBoxResult.Yes)
            {
                DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
                try
                {
                    StreamReader stream = new StreamReader(MainWindow.file_path);
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = stream.ReadLine()) != null)
                    {
                        String[] lins = line.Split(',');
                        CreateFolder(client, "/" + lins[0]);
                        if (lins.Length > 1)
                        {
                            if (File.Exists(lins[1]))//ファイル
                            {
                                string names = Path.GetFileName(lins[1]);
                                Upload(client, lins[0], names, lins[1]);
                            }
                            else if (Directory.Exists(lins[1]))//フォルダ
                            {
                                string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                                foreach (String f in files)
                                {
                                    string names = Path.GetFileName(f);
                                    Upload(client, lins[0], names, f);
                                }
                                string fors = Path.GetDirectoryName(lins[1]);
                                //MessageBox.Show(fors+"のアップロードを完了しました");
                            }

                        }
                    }
                    stream.Close();
                }
                catch (Exception es)
                {
                    MessageBox.Show("ファイルろーーどえらぁ" + es,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }

                //
            }
            else if (result == MessageBoxResult.No)
            {

            }

        }
        private async Task Upload(DropboxClient client, string folder, string fileName, string fileContent)
        {
            //MessageBox.Show(fileName + "のアップロードを開始しました",
                //"メッセージ",
                //MessageBoxButton.OK,
                //MessageBoxImage.Information);
            try
            {
                Console.WriteLine("Upload file...");
                FileStream fileStream = new FileStream(fileContent, FileMode.Open);
                var response = await client.Files.UploadAsync("/" + folder + "/" + fileName, WriteMode.Overwrite.Instance, body: fileStream);
                Console.WriteLine("Uploaded Id {0} Rev {1}", response.Id, response.Rev);
                //MessageBox.Show(fileName + "のアップロードを完了しました",
                //"メッセージ",
                //MessageBoxButton.OK,
                //MessageBoxImage.Information);
                fileStream.Close();
            }
            catch (Exception ex2)
            {
                MessageBox.Show(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }
        }
        private async Task<string[]> ListFolder(DropboxClient client, string path)
        {
            Console.WriteLine("--- Files ---");
            var list = await client.Files.ListFolderAsync(path);
            string datase = "";
            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                Console.WriteLine("F  {0}/", item.Name);
                datase = datase + "," + item.Name;
            }
            rest = datase.Split(',');

            
            if (list.HasMore)
            {
                Console.WriteLine("   ...");
            }
            return rest;
        }


        private async Task Download(DropboxClient client, string folder, string file,string out_path)
        {
            Console.WriteLine("Download file...");
            //MessageBox.Show(file + "のダウンロードを開始しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                using (var response = await client.Files.DownloadAsync("/" + folder + "/" + file))
                {
                    Console.WriteLine("Downloaded {0} Rev {1}", response.Response.Name, response.Response.Rev);
                    Stream x = await response.GetContentAsStreamAsync();
                    FileStream fileStream = new FileStream(out_path,FileMode.Create);
                    await x.CopyToAsync(fileStream);
                    fileStream.Close();
                    x.Close();
                    //MessageBox.Show(file + "のダウンロードが完了しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);

                }
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
        private async Task Download_setting(DropboxClient client, string folder, string file)
        {
            Console.WriteLine("Download file...");
            //MessageBox.Show(file + "のダウンロードを開始しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                using (var response = await client.Files.DownloadAsync("/" + folder + "/" + file))
                {
                    FileStream fileStream = new FileStream(file_path, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fileStream);
                    sw.Write(await response.GetContentAsStringAsync());
                    sw.Close();
                    fileStream.Close();
                    //MessageBox.Show(file + "のダウンロードが完了しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);

                }
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
        private async Task<FolderMetadata> CreateFolder(DropboxClient client, string path)
        {
            var folder = new FolderMetadata();
            try
            {
                Console.WriteLine("--- Creating Folder ---");
                var folderArg = new CreateFolderArg(path);
                folder = await client.Files.CreateFolderAsync(folderArg);
                Console.WriteLine("Folder: " + path + " created!");
                //MessageBox.Show(path + "を作成しました",
                //"メッセージ",
                //MessageBoxButton.OK,
                //MessageBoxImage.Information);
            }
            catch (ApiException<CreateFolderError> ex)
            {
                MessageBox.Show(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            return folder;
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("PCにDropBox上のセーブデータをダウンロードします。\n\nPC上のセーブデータは上書きされます。続行しますか？",
            "PCへダウンロード",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);
            DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StreamReader stream = new StreamReader(MainWindow.file_path);
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = stream.ReadLine()) != null)
                    {
                        String[] lins = line.Split(',');
                        if (lins.Length > 1)
                        {
                            if (File.Exists(lins[1]))//ファイル
                            {
                                string names = Path.GetFileName(lins[1]);
                                Console.WriteLine(lins[0] + " " + names + " " + lins[1]);
                                Download(client, lins[0], names, lins[1]);
                            }


                            else if (Directory.Exists(lins[1]))//フォルダ
                            {
                                var s = await ListFolder(client, "/"+lins[0]);

                                //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                                foreach (String f in s)
                                {
                                    string names = Path.GetFileName(f);
                                    string dire_names = Path.GetFileName( Path.GetDirectoryName(lins[1]));
                                    Console.WriteLine(lins[0] + " "+ names+" " + lins[1] + @"\" + f);
                                    if(names != "")
                                    {
                                        Download(client, lins[0], names, lins[1] + @"\" + f);
                                    }
                                }
                                string fors = Path.GetDirectoryName(lins[1]);
                                //MessageBox.Show(fors + "のダウンロードを完了しました");
                            }
                        }
                    }
                    stream.Close();
                }
                catch (Exception es)
                {
                    MessageBox.Show("ファイルろーーどえらぁ" + es,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }
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
                Upload(client, "setings", Path.GetFileName(file_path), file_path);


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

                Download_setting(client, "setings", Path.GetFileName(file_path));
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
