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
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window
    {


        public LogWindow(DropboxClient client, int type)//0 はダウンロード 1はアップロード
        {
            InitializeComponent();
            if (type == 0)
            {
                this.Title = "ダウンロードタスクのログView";
                Downloadse(client);
            }
            else
            {
                this.Title = "アップロードタスクのログView";
                Uploades(client);
            }
        }
        private async Task Uploades (DropboxClient client)
        {
            close_button.IsEnabled = false;

            try
            {
                StreamReader stream = new StreamReader(MainWindow.file_path);
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = stream.ReadLine()) != null)
                {
                    String[] lins = line.Split(',');
                    await CreateFolder(client, "/" + lins[0]);
                    if (lins.Length > 1)
                    {
                        if (File.Exists(lins[1]))//ファイル
                        {
                            string names = Path.GetFileName(lins[1]);
                            await Upload(client, lins[0], names, lins[1]);
                        }
                        else if (Directory.Exists(lins[1]))//フォルダ
                        {
                            string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                            foreach (String f in files)
                            {
                                string names = Path.GetFileName(f);
                                await Upload(client, lins[0], names, f);
                            }
                            string fors = Path.GetDirectoryName(lins[1]);
                            //MessageBox.Show(fors+"のアップロードを完了しました");
                            log_box.Items.Add(fors + "のアップロードを完了しました");
                        }

                    }
                }
                log_box.Items.Add("すべてのアップロードが完了しました");
                stream.Close();
                close_button.IsEnabled = true;
            }
            catch (Exception es)
            {
                close_button.IsEnabled = true;

                MessageBox.Show("ファイルろーーどえらぁ" + es,
            "エラー",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
                log_box.Items.Add("ファイルろーーどえらぁ" + es);
            }

        }


        private async Task Downloadse(DropboxClient client)
        {
            close_button.IsEnabled = false;
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
                            await Download(client, lins[0], names, lins[1]);
                        }


                        else if (Directory.Exists(lins[1]))//フォルダ
                        {
                            var s = await ListFolder(client, "/" + lins[0]);

                            //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                            foreach (String f in s)
                            {
                                string names = Path.GetFileName(f);
                                string dire_names = Path.GetFileName(Path.GetDirectoryName(lins[1]));
                                Console.WriteLine(lins[0] + " " + names + " " + lins[1] + @"\" + f);
                                if (names != "")
                                {
                                    await Download(client, lins[0], names, lins[1] + @"\" + f);
                                }
                            }
                            string fors = Path.GetDirectoryName(lins[1]);
                            //MessageBox.Show(fors + "のダウンロードを完了しました");
                            log_box.Items.Add(fors + "のダウンロードを完了しました");
                        }
                    }
                }
                log_box.Items.Add("すべてのダウンロードが完了しました");
                stream.Close();
                close_button.IsEnabled = true;
            }
            catch (Exception es)
            {
                MessageBox.Show("ファイルろーーどえらぁ" + es,
            "エラー",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
                log_box.Items.Add("ファイルろーーどえらぁ" + es);
                close_button.IsEnabled = true;
            }
        }


        private async Task<string[]> ListFolder(DropboxClient client, string path)
        {
            //Console.WriteLine("--- Files ---");
            var list = await client.Files.ListFolderAsync(path);
            string datase = "";
            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                //Console.WriteLine("F  {0}/", item.Name);
                datase = datase + "," + item.Name;
            }
            string[] rest = datase.Split(',');


            if (list.HasMore)
            {
                //Console.WriteLine("   ...");
            }
            return rest;
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
                log_box.Items.Add(path + "を作成しました");
            }
            catch (ApiException<CreateFolderError> ex)
            {
                close_button.IsEnabled = true;
                log_box.Items.Add(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります");
                MessageBox.Show(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            return folder;
        }


        private async Task Upload(DropboxClient client, string folder, string fileName, string fileContent)
        {
            //MessageBox.Show(fileName + "のアップロードを開始しました",
            //"メッセージ",
            //MessageBoxButton.OK,
            //MessageBoxImage.Information);
            log_box.Items.Add(fileName + "のアップロードを開始しました");
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
                log_box.Items.Add(fileName + "のアップロードを完了しました");
                fileStream.Close();
            }
            catch (Exception ex2)
            {
                close_button.IsEnabled = true;
                log_box.Items.Add(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message);
                MessageBox.Show(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }
        }



        private async Task Download(DropboxClient client, string folder, string file, string out_path)
        {
            Console.WriteLine("Download file...");
            //MessageBox.Show(file + "のダウンロードを開始しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
            log_box.Items.Add(file + "のダウンロードを開始しました");

            try
            {
                using (var response = await client.Files.DownloadAsync("/" + folder + "/" + file))
                {
                    Console.WriteLine("Downloaded {0} Rev {1}", response.Response.Name, response.Response.Rev);
                    Stream x = await response.GetContentAsStreamAsync();
                    FileStream fileStream = new FileStream(out_path, FileMode.Create);
                    await x.CopyToAsync(fileStream);
                    fileStream.Close();
                    x.Close();
                    //MessageBox.Show(file + "のダウンロードが完了しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
                    log_box.Items.Add(file + "のダウンロードが完了しました");

                }
            }
            catch (ApiException<DownloadError> ex)
            {
                close_button.IsEnabled = true;
                MessageBox.Show(ex.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex.ToString());
            }
            catch (Exception ex2)
            {
                close_button.IsEnabled = true;
                MessageBox.Show(ex2.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex2.ToString());
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
