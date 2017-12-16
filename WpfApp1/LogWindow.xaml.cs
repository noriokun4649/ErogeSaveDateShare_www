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

        int types;//0 はダウンロード 1はアップロード
        DropboxClient client;

        public LogWindow(DropboxClient client, int type)//0 はダウンロード 1はアップロード
        {
            InitializeComponent();
            this.client = client;
            this.types = type;
            try
            {
                StreamReader stream = new StreamReader(MainWindow.file_path);
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = stream.ReadLine()) != null)
                {
                    game_list.Items.Add(line.Split(','));
                }
                stream.Close();
            }
            catch (Exception es)
            {
                MessageBox.Show(es.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            if (types == 0)
            {
                this.Title = "ダウンロードタスクのログView";
                titletext.Text = "ダウンロード";
                type_text.Text = "ダウンロードの実行方法";
            }
            else
            {
                this.Title = "アップロードタスクのログView";
                titletext.Text = "アップロード";
                type_text.Text = "アップロードの実行方法";
            }
        }
        private async Task Uploades(DropboxClient client, string[] input_data)
        {
            close_button.IsEnabled = false;

            try
            {

                if (input_data == null)
                {
                    StreamReader stream = new StreamReader(MainWindow.file_path);
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = stream.ReadLine()) != null)
                    {
                        String[] lins = line.Split(',');
                        log_box.Items.Add("ゲームタイトル→" + lins[0] + "のアップロードを開始しました");
                        log_box.ScrollIntoView("ゲームタイトル→" + lins[0] + "のアップロードを開始しました");
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
                                log_box.ScrollIntoView(fors + "のアップロードを完了しました");

                            }

                        }
                    }
                    log_box.Items.Add("すべてのアップロードが完了しました");
                    log_box.ScrollIntoView("すべてのアップロードが完了しました");
                    stream.Close();
                    close_button.IsEnabled = true;
                }
                else
                {
                    log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のアップロードを開始しました");
                    log_box.ScrollIntoView("ゲームタイトル→" + input_data[0] + "のアップロードを開始しました");
                    await CreateFolder(client, "/" + input_data[0]);
                    if (input_data.Length > 1)
                    {
                        if (File.Exists(input_data[1]))//ファイル
                        {
                            string names = Path.GetFileName(input_data[1]);
                            await Upload(client, input_data[0], names, input_data[1]);
                        }
                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            string[] files = System.IO.Directory.GetFiles(input_data[1], "*", System.IO.SearchOption.AllDirectories);
                            foreach (String f in files)
                            {
                                string names = Path.GetFileName(f);
                                await Upload(client, input_data[0], names, f);
                            }
                            string fors = Path.GetDirectoryName(input_data[1]);
                            //MessageBox.Show(fors+"のアップロードを完了しました");
                            log_box.Items.Add(fors + "のアップロードを完了しました");
                            log_box.ScrollIntoView(fors + "のアップロードを完了しました");

                        }


                    }
                    log_box.Items.Add("すべてのアップロードが完了しました");
                    log_box.ScrollIntoView("すべてのアップロードが完了しました");
                    close_button.IsEnabled = true;
                }
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


        private async Task Downloadse(DropboxClient client,string[] input_data)
        {
            close_button.IsEnabled = false;
            try
            {
                if (input_data == null) { 
                StreamReader stream = new StreamReader(MainWindow.file_path);
                string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.

                while ((line = stream.ReadLine()) != null)
                {
                    String[] lins = line.Split(',');

                        log_box.Items.Add("ゲームタイトル→"+lins[0]+"のダウンロードを開始しました");
                        log_box.ScrollIntoView("ゲームタイトル→" + lins[0] + "のダウンロードを開始しました");

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
                            log_box.ScrollIntoView(fors + "のダウンロードを完了しました");
                        }
                        else
                        {
                            log_box.Items.Add("保存先ディレクトリ("+lins[1]+")が見つからないためダウンロード処理を中断しました");
                        }
                    }
                }
                log_box.Items.Add("すべてのダウンロードが完了しました");
                log_box.ScrollIntoView("すべてのダウンロードが完了しました");
                stream.Close();
                close_button.IsEnabled = true;
                }
                else
                {
                    if (input_data.Length > 1)
                    {

                        log_box.Items.Add("ゲームタイトル→" + input_data[0]+"のダウンロードを開始しました");
                        log_box.ScrollIntoView("ゲームタイトル→" + input_data[0]+"のダウンロードを開始しました");
                        if (File.Exists(input_data[1]))//ファイル
                        {
                            string names = Path.GetFileName(input_data[1]);
                            Console.WriteLine(input_data[0] + " " + names + " " + input_data[1]);
                            await Download(client, input_data[0], names, input_data[1]);
                        }


                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            var s = await ListFolder(client, "/" + input_data[0]);

                            //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                            foreach (String f in s)
                            {
                                string names = Path.GetFileName(f);
                                string dire_names = Path.GetFileName(Path.GetDirectoryName(input_data[1]));
                                Console.WriteLine(input_data[0] + " " + names + " " + input_data[1] + @"\" + f);
                                if (names != "")
                                {
                                    await Download(client, input_data[0], names, input_data[1] + @"\" + f);
                                }
                            }
                            string fors = Path.GetDirectoryName(input_data[1]);
                            //MessageBox.Show(fors + "のダウンロードを完了しました");
                            log_box.Items.Add(fors + "のダウンロードを完了しました");
                            log_box.ScrollIntoView(fors + "のダウンロードを完了しました");
                        }
                        else
                        {
                            log_box.Items.Add("保存先ディレクトリ(" + input_data[1] + ")が見つからないためダウンロード処理を中断しました");
                        }
                        log_box.Items.Add("すべてのダウンロードが完了しました");
                        log_box.ScrollIntoView("すべてのダウンロードが完了しました");
                        close_button.IsEnabled = true;
                    }
                }
            }
            catch (Exception es)
            {
                MessageBox.Show("ファイルろーーどえらぁ" + es,
            "エラー",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
                log_box.Items.Add("ファイルろーーどえらぁ" + es);
                log_box.ScrollIntoView("ファイルろーーどえらぁ" + es);
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
                log_box.Items.Add("フォルダ" + path + "を作成しました");
                log_box.ScrollIntoView("フォルダ" + path + "を作成しました");
            }
            catch (ApiException<CreateFolderError> ex)
            {
                close_button.IsEnabled = true;
                log_box.Items.Add(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります");
                log_box.ScrollIntoView(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります");
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
            log_box.ScrollIntoView(fileName + "のアップロードを開始しました");
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
                log_box.ScrollIntoView(fileName + "のアップロードを完了しました");
                fileStream.Close();
            }
            catch (Exception ex2)
            {
                close_button.IsEnabled = true;
                log_box.Items.Add(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message);
                log_box.ScrollIntoView(log_box.Items.Count - 1);
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
            log_box.ScrollIntoView(file + "のダウンロードを開始しました");

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
                    log_box.ScrollIntoView(file + "のダウンロードが完了しました");
                    
                }
            }
            catch (ApiException<DownloadError> ex)
            {
                close_button.IsEnabled = true;
                MessageBox.Show(ex.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex.ToString());
                log_box.ScrollIntoView(log_box.Items.Count - 1);
            }
            catch (Exception ex2)
            {
                close_button.IsEnabled = true;
                MessageBox.Show(ex2.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex2.ToString());
                log_box.ScrollIntoView(log_box.Items.Count - 1);
                
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //選択中

            int indexs = game_list.SelectedIndex;
            if (indexs >= 0)
            {
                string[] texts = (string[])game_list.Items.GetItemAt(indexs);
                if (texts.Length > 1)
                {
                    //se^bubasyo aru baa i
                    if (types == 0)
                    {
                        Downloadse(client, texts);
                    }
                    else
                    {
                        Uploades(client,texts);
                    }
                    
                }
                else
                {
                    MessageBox.Show("選択中のゲームタイトルが不正です\n\n処理を中断します");
                }
            }
            else
            {
                MessageBox.Show("選択中のゲームタイトルが不正です\n\n処理を中断します");
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //一括
            if (types == 0)
            {
                Downloadse(client,null);
            }
            else
            {
                Uploades(client,null);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var win = new DropBoxList();
            win.Owner = this;
            win.ShowDialog();
        }

        private void game_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indexs = game_list.SelectedIndex;
            if (indexs >= 0)
            {
                selct_button.IsEnabled = true;
            }
            else
            {
                selct_button.IsEnabled = false;
            }
        }
    }
}
