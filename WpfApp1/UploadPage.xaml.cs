using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// UploadPage.xaml の相互作用ロジック
    /// </summary>
    public partial class UploadPage : Page, IDisposable
    {
        DropboxClient client;
        private CancellationTokenSource cts;

        bool Action_now = false;
        bool Action_check = false;
        bool Action_check_result;
        public UploadPage()
        {
            InitializeComponent();
            client = new DropboxClient(Properties.Settings.Default.AccessToken);
            try
            {
                using (StreamReader stream = new StreamReader(MainWindow.file_path))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        game_list.Items.Add(line.Split(','));
                    }
                }
            }
            catch (Exception es)
            {
                MessageBox.Show(es.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
        }
       
        private async Task Uploades(DropboxClient client, string[] input_data, CancellationToken token)
        {
            Action_now = true;
            //wind.ActionNow1 = true;
            try
            {
                if (input_data == null)
                {
                    int line_count = File.ReadAllLines(MainWindow.file_path).Length;
                    loading.Maximum = line_count;
                    int count = 1;
                    using (StreamReader stream = new StreamReader(MainWindow.file_path))
                    {
                        string line;
                        while ((line = stream.ReadLine()) != null)
                        {
                            loading.Value = count++;
                            load_text.Text = line_count + "タイトル中" + (count - 1) + "タイトル処理済み";
                            String[] lins = line.Split(',');
                            log_box.Items.Add("ゲームタイトル→" + lins[0] + "のアップロードを開始しました");
                            await CreateFolder(client, "/" + lins[0]);
                            if (lins.Length > 1)
                            {
                                if (File.Exists(lins[1]))//ファイル
                                {
                                    var fol_list = await ListFolder(client, "/" + lins[0]);

                                    token.ThrowIfCancellationRequested();
                                    string names = Path.GetFileName(lins[1]);
                                    int index1 = Array.IndexOf(fol_list[0], names);
                                    if (0 <= index1)
                                    {
                                        DateTime time_local = File.GetLastWriteTime(lins[1]);
                                        DateTime times_dro = DateTime.Parse(fol_list[1][1]);
                                        int time_if = times_dro.CompareTo(time_local);//PC上のデータはドロップボックスのデータよりも

                                        bool? result = If_Result(names, lins[0], time_if, true);

                                        if (result == true)
                                        {
                                            log_box.Items.Add(names + " をPC上のデータで上書きします");
                                            await Upload(client, lins[0], names,"", lins[1]);
                                        }
                                        else
                                        {
                                            log_box.Items.Add(names + " の上書きはスキップされました");
                                        }
                                    }
                                    else
                                    {
                                        //Console.WriteLine("クラウドにはこのデータないですね");
                                        await Upload(client, lins[0], names, "",lins[1]);
                                    }
                                }
                                else if (Directory.Exists(lins[1]))//フォルダ
                                {
                                    var s = await ListFolder(client, "/" + lins[0]);
                                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
                                    string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                                    int child_leng = files.Length;
                                    loading_chilled.Maximum = child_leng;
                                    int chile_count = 1;
                                    foreach (String f in files)
                                    {
                                        string subfolder = f.Replace(lins[1], "");
                                        loading_chilled.Value = chile_count++;
                                        load_chil_text.Text = lins[0] + "のファイル" + child_leng + "件中" + (chile_count - 1) + "件処理済み";
                                        token.ThrowIfCancellationRequested();
                                        string names = Path.GetFileName(f);
                                        int index1 = Array.IndexOf(s[0], names);
                                        if (0 <= index1)
                                        {
                                            DateTime time_local = File.GetLastWriteTime(f);
                                            DateTime times_dro = DateTime.Parse(s[1][index1]);
                                            int time_if = times_dro.CompareTo(time_local);//PC上のデータはドロップボックスのデータよりも

                                            //Console.WriteLine("ファイル名" + f + " ローカル更新日:" + time_local + " 泥更新日:" + times_dro + "判定" + time_if);

                                            bool? result = If_Result(names, lins[0], time_if, true);

                                            if (result == true)
                                            {
                                                log_box.Items.Add(names + " をPC上のデータで上書きします");
                                                await Upload(client, lins[0], names, subfolder, f);
                                            }
                                            else
                                            {
                                                log_box.Items.Add(names + " の上書きはスキップされました");
                                            }
                                        }
                                        else
                                        {
                                            //Console.WriteLine("クラウドにはこのデータないですね");
                                            await Upload(client, lins[0], names, subfolder, f);
                                        }
                                    }
                                    string fors = Path.GetDirectoryName(lins[1]);
                                    //MessageBox.Show(fors+"のアップロードを完了しました");
                                    log_box.Items.Add("ゲームタイトル→" + lins[0] + "のアップロードを完了しました");
                                    Action_check = false;
                                }
                            }
                        }
                        log_box.Items.Add("すべてのアップロードが完了しました");
                    }
                }
                else
                {
                    log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のアップロードを開始しました");
                    await CreateFolder(client, "/" + input_data[0]);
                    if (input_data.Length > 1)
                    {
                        if (File.Exists(input_data[1]))//ファイル
                        {
                            var fol_list = await ListFolder(client, "/" + input_data[0]);
                            token.ThrowIfCancellationRequested();
                            string names = Path.GetFileName(input_data[1]);
                            int index1 = Array.IndexOf(fol_list[0], names);
                            if (0 <= index1)
                            {
                                DateTime time_local = File.GetLastWriteTime(input_data[1]);
                                DateTime times_dro = DateTime.Parse(fol_list[1][1]);
                                int time_if = times_dro.CompareTo(time_local);//PC上のデータはドロップボックスのデータよりも

                                bool? result = If_Result(names, input_data[0], time_if, true);

                                if (result == true)
                                {
                                    log_box.Items.Add(names + " をPC上のデータで上書きします");
                                    await Upload(client, input_data[0], names,"", input_data[1]);
                                }
                                else
                                {
                                    log_box.Items.Add(names + " の上書きはスキップされました");
                                }
                            }
                            else
                            {
                                //Console.WriteLine("クラウドにはこのデータないですね");
                                await Upload(client, input_data[0], names,"" ,input_data[1]);
                            }
                        }
                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            var s = await ListFolder(client, "/" + input_data[0]);


                            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");

                            string[] files = System.IO.Directory.GetFiles(input_data[1], "*", System.IO.SearchOption.AllDirectories);
                            int child_leng = files.Length;
                            loading_chilled.Maximum = child_leng;
                            int chile_count = 1;

                            foreach (String f in files)
                            {
                                string subfolder = f.Replace(input_data[1],"");
                                loading_chilled.Value = chile_count++;
                                load_chil_text.Text = input_data[0] + "のファイル" + child_leng + "件中" + (chile_count - 1) + "件処理済み";
                                token.ThrowIfCancellationRequested();
                                string names = Path.GetFileName(f);
                                int index1 = Array.IndexOf(s[0], names);
                                if (0 <= index1)
                                {
                                    DateTime time_local = File.GetLastWriteTime(f);
                                    DateTime times_dro = DateTime.Parse(s[1][index1]);
                                    int time_if = times_dro.CompareTo(time_local);//PC上のデータはドロップボックスのデータよりも

                                    Console.WriteLine("ファイル名" + f + " ローカル更新日:" + time_local + " 泥更新日:" + times_dro + "判定" + time_if);

                                    bool? result = If_Result(names, input_data[0], time_if, true);

                                    if (result == true)
                                    {
                                        log_box.Items.Add(names + " をPC上のデータで上書きします");
                                        await Upload(client, input_data[0], names, subfolder , f);
                                    }
                                    else
                                    {
                                        log_box.Items.Add(names + " の上書きはスキップされました");
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine("クラウドにはこのデータないですね");
                                    await Upload(client, input_data[0], names, subfolder, f);
                                }
                            }

                            string fors = Path.GetDirectoryName(input_data[1]);
                            log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のアップロードを完了しました");
                            Action_check = false;
                        }
                    }
                    log_box.Items.Add("すべてのアップロードが完了しました");
                }
            }

            catch (BadInputException exs)
            {
                log_box.Items.Add("問題が発生しました。詳細はメッセージウィンドウを確認してください。");
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (HttpRequestException exa)
            {
                log_box.Items.Add("問題が発生しました。詳細はメッセージウィンドウを確認してください。");
                MessageBox.Show("HTTPリクエストに問題が発生しました。コンピュータがインターネットに接続されているか確認してください。\n" + exa.Message,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (OperationCanceledException)
            {
                log_box.Items.Add("アップロード処理はユーザによってキャンセルされました。");
            }
            catch (Exception es)
            {
                log_box.Items.Add("問題が発生しました。詳細はメッセージウィンドウを確認してください。");
                MessageBox.Show("エラー\n" + es.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            finally
            {
                //wind.ActionNow1 = false;
                Action_now = false;
                Action_check = false;
                loading.Value = 0;
                loading_chilled.Value = 0;
                load_text.Text = "";
                load_chil_text.Text = "";
            }

        }

        private async Task<string[][]> ListFolder(DropboxClient client, string path)
        {
            var list = await client.Files.ListFolderAsync(path);
            string datase = "";
            string datase_time = "";
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                datase = datase + "," + item.Name;

                TimeZoneInfo jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                DateTime now_jst = TimeZoneInfo.ConvertTime(item.AsFile.ServerModified, jst);

                datase_time = datase_time + "," + now_jst.ToString();
            }

            string[][] rest = new string[][] { datase.Split(','), datase_time.Split(',') };
            Console.WriteLine(datase_time.Split(',').Length);
            Console.WriteLine(datase + " : " + datase_time);
            if (list.HasMore)
            {
                //Console.WriteLine("   ...");
            }
            return rest;
        }

        private async Task<CreateFolderResult> CreateFolder(DropboxClient client, string path)
        {
            var folder = new CreateFolderResult();
            try
            {
                Console.WriteLine("--- Creating Folder ---");
                var folderArg = new CreateFolderArg(path);
                folder = await client.Files.CreateFolderV2Async(folderArg);
                Console.WriteLine("Folder: " + path + " created!");
                log_box.Items.Add("フォルダ" + path + "を作成しました");
            }
            catch (ApiException<CreateFolderError> ex)
            {
                string a = ex.Message;
                log_box.Items.Add(path + "の作成に問題が発生しました。すでにフォルダが存在している可能性があります\n\n理由：" + a);
            }
            return folder;
        }

        private async Task Upload(DropboxClient client, string folder, string fileName,string subfolder ,string fileContent)
        {
            subfolder = subfolder.Replace("\\", "/");//なんとなくスラッシュへ書き換え
            if (subfolder.Equals(""))//サブフォルダがなかった場合、ファイルネームを代入
            {
                subfolder = "/" + fileName;
            }
            Console.WriteLine(subfolder);
            log_box.Items.Add(fileName + "のアップロードを開始しました");
            try
            {
                Console.WriteLine("Upload file...");
                using (FileStream fileStream = new FileStream(fileContent, FileMode.Open))
                {
                    //var response = await client.Files.UploadAsync("/" + folder + "/" + fileName, WriteMode.Overwrite.Instance, body: fileStream);
                    var response = await client.Files.UploadAsync("/" + folder + subfolder , WriteMode.Overwrite.Instance, body: fileStream);
                    Console.WriteLine("Uploaded Id {0} Rev {1}", response.Id, response.Rev);
                    log_box.Items.Add(fileName + "のアップロードを完了しました");
                }
            }
            catch (Exception ex2)
            {
                log_box.Items.Add(fileName + "のアップロードに問題が発生しました" + ex2.Message);
                MessageBox.Show(fileName + "のアップロードに問題が発生しました\n\n" + ex2.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                Action_now = false;
                //wind.ActionNow1 = false;

            }
        }
        private bool? If_Result(string file_name, string folder_name, int if_time, bool if_mode)
        {
            bool? result;
            if (if_time == 0)
            {
                result = false;
                log_box.Items.Add("ファイル:" + file_name + "は更新されてません＾u＾");
            }
            else if (if_time > 0)
            {
                if (Properties.Settings.Default.pc_snyc)
                {
                    log_box.Items.Add(file_name + (!if_mode ? " はPC上のセーブデータがDropBox上" : " はDropBox上のセーブデータがPC上") + "のセーブデータよりも更新日時が新しいようです。");
                    if (!Action_check)
                    {

                        var windpw1 = new Window1();
                        windpw1.SetUpParent(this);
                        windpw1.SetParameter(folder_name, file_name + (!if_mode ? " はPC上のセーブデータがDropBox上" : " はDropBox上のセーブデータがPC上") + "のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？");
                        windpw1.Title = !if_mode ? "DropBoxからダウンロード" : "DropBoxへアップロード";
                        result = windpw1.ShowDialog();
                        if (Action_check)
                        {
                            Action_check_result = (bool)result;
                        }
                    }
                    else
                    {
                        result = Action_check_result;
                    }
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                result = true;
                //MessageBox.Show("DropBox上のセーブデータがPC上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？");
            }
            return result;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //一括
            if (Action_now != true)
            {
                cts = new CancellationTokenSource();
                await Uploades(client, null, cts.Token);
            }
            else
            {
                MessageBox.Show("現在実行中の処理があります。終了を待つか、キャンセルしてから実行してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void SetCheck(Boolean par)
        {
            Action_check = par;
        }

        private async void Selct_button_Click(object sender, RoutedEventArgs e)
        {
            if (Action_now != true)
            {
                //選択中
                int indexs = game_list.SelectedIndex;
                if (indexs >= 0)
                {
                    string[] texts = (string[])game_list.Items.GetItemAt(indexs);
                    if (texts.Length > 1)
                    {
                        cts = new CancellationTokenSource();
                        await Uploades(client, texts, cts.Token);
                    }
                    else
                    {
                        MessageBox.Show("選択中のゲームタイトルが不正です\n\n処理を中断します", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("選択中のゲームタイトルが不正です\n\n処理を中断します", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("現在実行中の処理があります。終了を待つか、キャンセルしてから実行してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Game_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (cts != null)
                {
                    // dispose managed resources
                    cts.Dispose();
                }
                if (client != null)
                {
                    client.Dispose();
                }
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private void Log_box_LayoutUpdated(object sender, EventArgs e)
        {
            if (action_select.IsChecked == true)
            {
                if (this.log_box.Items.Count >= 1)
                {
                    this.log_box.ScrollIntoView(this.log_box.Items[this.log_box.Items.Count - 1]);
                }
            }
        }
    }
}
