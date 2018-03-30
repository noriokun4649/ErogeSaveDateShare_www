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
    /// DownloadPage.xaml の相互作用ロジック
    /// </summary>
    public partial class DownloadPage : Page, IDisposable
    {
        DropboxClient client;
        private CancellationTokenSource cts;

        bool Action_now = false;
        bool Action_check = false;
        bool Action_check_result;
        Main2 wind;
        public DownloadPage()
        {
            InitializeComponent();
            wind = (Main2)Window.GetWindow(this);
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

        private async Task Downloadse(DropboxClient client, string[] input_data, CancellationToken token)
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

                            log_box.Items.Add("ゲームタイトル→" + lins[0] + "のダウンロードを開始しました");

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
                                        int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも

                                        bool? result = If_Result(names, lins[0], time_if, false);

                                        if (result == true)
                                        {
                                            log_box.Items.Add(names + " の上書きをします");
                                            await Download(client, lins[0], names, lins[1]);
                                            File.SetLastWriteTime(lins[1], times_dro);//ダウンロードしたファイルの更新日時をDropBox上の更新日時に上書き
                                        }
                                        else
                                        {
                                            log_box.Items.Add(names + " の上書きはスキップされました");
                                        }
                                    }
                                    else
                                    {
                                        //Console.WriteLine("クラウドにはこのデータないですね");
                                        await Download(client, lins[0], names, lins[1]);
                                    }
                                }

                                else if (Directory.Exists(lins[1]))//フォルダ
                                {
                                    string[][] s = await ListFolder(client, "/" + lins[0]);

                                    //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                                    int child_leng = s[0].Length;
                                    loading_chilled.Maximum = child_leng;
                                    for (int i = 1; s[0].Length > i; i++)//s の二次配列
                                    {
                                        loading_chilled.Value = i;
                                        load_chil_text.Text = lins[0] + "のファイル" + child_leng + "件中" + (i - 1) + "件処理済み";
                                        token.ThrowIfCancellationRequested();
                                        Console.WriteLine(s[0][i] + "更新日：" + s[1][i]);
                                        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
                                        String file_location = lins[1] + @"\" + s[0][i];
                                        DateTime time_local = File.GetLastWriteTime(file_location);
                                        DateTime times_dro = DateTime.Parse(s[1][i]);
                                        int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも
                                        Console.WriteLine(time_if);
                                        bool? result = If_Result(s[0][i], lins[0], time_if, false);

                                        if (result == true)
                                        {

                                            log_box.Items.Add(s[0][i] + " の上書きをします");
                                            string names = Path.GetFileName(s[0][i]);
                                            string dire_names = Path.GetFileName(Path.GetDirectoryName(lins[1]));

                                            if (names != "")
                                            {
                                                await Download(client, lins[0], names, file_location);
                                                File.SetLastWriteTime(file_location, times_dro);//ダウンロードしたファイルの更新日時をDropBox上の更新日時に上書き
                                            }

                                        }
                                        else
                                        {
                                            log_box.Items.Add(s[0][i] + " の上書きはスキップされました");
                                        }
                                    }
                                    string fors = Path.GetDirectoryName(lins[1]);
                                    //MessageBox.Show(fors + "のダウンロードを完了しました");
                                    log_box.Items.Add("ゲームタイトル→" + lins[0] + "のダウンロードを完了しました");
                                    Action_check = false;
                                }
                                else
                                {
                                    log_box.Items.Add("保存先ディレクトリ(" + lins[1] + ")が見つからないためダウンロード処理を中断しました");
                                    Action_check = false;
                                }
                            }
                        }
                        log_box.Items.Add("すべてのダウンロードが完了しました");
                    }
                }
                else
                {
                    if (input_data.Length > 1)
                    {
                        log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のダウンロードを開始しました");
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
                                int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも

                                bool? result = If_Result(input_data[1], input_data[0], time_if, false);

                                if (result == true)
                                {
                                    log_box.Items.Add(input_data[1] + " の上書きをします");
                                    await Download(client, input_data[0], names, input_data[1]);
                                    File.SetLastWriteTime(input_data[1], times_dro);//ダウンロードしたファイルの更新日時をDropBox上の更新日時に上書き
                                }
                                else
                                {
                                    log_box.Items.Add(input_data[1] + " の上書きはスキップされました");
                                }
                            }
                            else
                            {
                                //Console.WriteLine("クラウドにはこのデータないですね");
                                await Download(client, input_data[0], names, input_data[1]);
                            }
                        }

                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            string[][] s = await ListFolder(client, "/" + input_data[0]);

                            //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                            int child_leng = s[0].Length;
                            loading_chilled.Maximum = child_leng;
                            for (int i = 1; s[0].Length > i; i++)//s の二次配列
                            {
                                loading_chilled.Value = i;
                                load_chil_text.Text = input_data[0] + "のファイル" + child_leng + "件中" + (i - 1) + "件処理済み";
                                token.ThrowIfCancellationRequested();
                                Console.WriteLine(s[0][i] + "更新日：" + s[1][i]);
                                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");

                                String file_location = input_data[1] + @"\" + s[0][i];

                                DateTime time_local = File.GetLastWriteTime(file_location);
                                DateTime times_dro = DateTime.Parse(s[1][i]);
                                int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも
                                Console.WriteLine(time_if);
                                bool? result = If_Result(s[0][i], input_data[0], time_if, false);
                                if (result == true)
                                {

                                    log_box.Items.Add(s[0][i] + " の上書きをします");
                                    string names = Path.GetFileName(s[0][i]);
                                    string dire_names = Path.GetFileName(Path.GetDirectoryName(input_data[1]));

                                    if (names != "")
                                    {
                                        await Download(client, input_data[0], names, file_location);
                                        File.SetLastWriteTime(file_location, times_dro);//ダウンロードしたファイルの更新日時をDropBox上の更新日時に上書き
                                    }

                                }
                                else
                                {
                                    log_box.Items.Add(s[0][i] + " の上書きはスキップされました");
                                }
                            }
                            string fors = Path.GetDirectoryName(input_data[1]);
                            //MessageBox.Show(fors + "のダウンロードを完了しました");
                            log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のダウンロードを完了しました");
                            Action_check = false;
                        }
                        else
                        {
                            log_box.Items.Add("保存先ディレクトリ(" + input_data[1] + ")が見つからないためダウンロード処理を中断しました");
                            Action_check = false;
                        }
                        log_box.Items.Add("すべてのダウンロードが完了しました");
                    }
                }
            }
            catch (ApiException<ListFolderError> exs)
            {
                log_box.Items.Add("問題が発生しました。詳細はメッセージウィンドウを確認してください。");
                MessageBox.Show("ダウンロード元のフォルダーがDropBox上に存在しない可能性があります。\n" + exs.Message,
                "フォルダエラーエラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
                log_box.Items.Add("ダウンロード処理はユーザによってキャンセルされました。");
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
                Action_now = false;
                Action_check = false;
                //wind.ActionNow1 = false;
                loading.Value = 0;
                loading_chilled.Value = 0;
                load_text.Text = "";
                load_chil_text.Text = "";
            }
        }
        private async Task<string[][]> ListFolder(DropboxClient client, string path)
        {
            //Console.WriteLine("--- Files ---");
            var list = await client.Files.ListFolderAsync(path);
            string datase = "";
            string datase_time = "";
            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                //Console.WriteLine("F  {0}/", item.Name);
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
        private async Task Download(DropboxClient client, string folder, string file, string out_path)
        {
            //Console.WriteLine("Download file...");
            //MessageBox.Show(file + "のダウンロードを開始しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
            log_box.Items.Add(file + "のダウンロードを開始しました");
            try
            {
                using (var response = await client.Files.DownloadAsync("/" + folder + "/" + file))
                {
                    //Console.WriteLine("Downloaded {0} Rev {1}", response.Response.Name, response.Response.Rev);
                    using (Stream x = await response.GetContentAsStreamAsync())
                    using (FileStream fileStream = new FileStream(out_path, FileMode.Create))
                    {
                        await x.CopyToAsync(fileStream);
                        //MessageBox.Show(file + "のダウンロードが完了しました", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
                        log_box.Items.Add(file + "のダウンロードが完了しました");
                    }
                }
            }
            catch (ApiException<DownloadError> ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex.Message);
                Action_now = false;
                //wind.ActionNow1 = false;
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex2.Message);
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
                    /*
                    result = MessageBox.Show(s[0][i] + " はPC上のセーブデータがDropBox上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxからダウンロード",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                    */
                    if (!Action_check)
                    {

                        var windpw1 = new Window1();
                        windpw1.SetDownParent(this);
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
                await Downloadse(client, null, cts.Token);
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
                        await Downloadse(client, texts, cts.Token);
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
