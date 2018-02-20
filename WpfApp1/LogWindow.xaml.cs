﻿using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp1
{
    //<ListBox x:Name="log_box" Margin="10,10,10,0" HorizontalContentAlignment="Stretch" VerticalContentAlignment="Stretch" Grid.Row="3" Height="133" VerticalAlignment="Top"/>
    /// <summary>
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window, IDisposable
    {
        int types;//0 はダウンロード 1はアップロード
        DropboxClient client;
        private CancellationTokenSource cts;
        Boolean Action_now = false;

        public LogWindow(DropboxClient client, int type)//0 はダウンロード 1はアップロード
        {
            InitializeComponent();
            this.client = client;
            this.types = type;
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

        private async Task Uploades(DropboxClient client, string[] input_data, CancellationToken token)
        {
            close_button.IsEnabled = false;
            Action_now = true;
            try
            {
                if (input_data == null)
                {
                    using (StreamReader stream = new StreamReader(MainWindow.file_path))
                    {
                        string line;
                        while ((line = stream.ReadLine()) != null)
                        {
                            String[] lins = line.Split(',');
                            log_box.Items.Add("ゲームタイトル→" + lins[0] + "のアップロードを開始しました");
                            await CreateFolder(client, "/" + lins[0]);
                            if (lins.Length > 1)
                            {
                                if (File.Exists(lins[1]))//ファイル
                                {
                                   token.ThrowIfCancellationRequested();
                                    string names = Path.GetFileName(lins[1]);
                                    await Upload(client, lins[0], names, lins[1]);
                                }
                                else if (Directory.Exists(lins[1]))//フォルダ
                                {
                                    var s = await ListFolder(client, "/" + lins[0]);
                                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
                                    string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);
                                    foreach (String f in files)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        string names = Path.GetFileName(f);
                                        int index1 = Array.IndexOf(s[0], names);
                                        if (0 <= index1)
                                        {
                                            DateTime time_local = File.GetLastWriteTime(f);
                                            DateTime times_dro = DateTime.Parse(s[1][index1]);
                                            int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも

                                            //Console.WriteLine("ファイル名" + f + " ローカル更新日:" + time_local + " 泥更新日:" + times_dro + "判定" + time_if);

                                            MessageBoxResult result;
                                            if (time_if == 0)
                                            {
                                                result = MessageBoxResult.No;
                                                log_box.Items.Add("ファイル" + f + "は更新されてません＾u＾");
                                            }
                                            else if (time_if > 0)
                                            {
                                                //MessageBox.Show(f + " はPC上のセーブデータがDropBox上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxにアップロード");
                                                result = MessageBoxResult.Yes;
                                            }
                                            else
                                            {
                                                if (Properties.Settings.Default.dropbox_snyc)
                                                {
                                                    result = MessageBox.Show(f + " はDropBox上のセーブデータがPC上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxにアップロード",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Information);
                                                }
                                                else
                                                {
                                                    result = MessageBoxResult.Yes;
                                                }
                                            }

                                            if (result == MessageBoxResult.Yes)
                                            {
                                                log_box.Items.Add(f + " をPC上のデータで上書きします");
                                                await Upload(client, lins[0], names, f);
                                            }
                                            else
                                            {
                                                log_box.Items.Add(f + " の上書きはスキップされました");
                                            }
                                        }
                                        else
                                        {
                                            //Console.WriteLine("クラウドにはこのデータないですね");
                                            await Upload(client, lins[0], names, f);
                                        }
                                    }
                                    string fors = Path.GetDirectoryName(lins[1]);
                                    //MessageBox.Show(fors+"のアップロードを完了しました");
                                    log_box.Items.Add("ゲームタイトル→" + lins[0] + "のアップロードを完了しました");
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
                            token.ThrowIfCancellationRequested();
                            string names = Path.GetFileName(input_data[1]);
                            await Upload(client, input_data[0], names, input_data[1]);
                        }
                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            var s = await ListFolder(client, "/" + input_data[0]);



                            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");

                            string[] files = System.IO.Directory.GetFiles(input_data[1], "*", System.IO.SearchOption.AllDirectories);


                            foreach (String f in files)
                            {
                                token.ThrowIfCancellationRequested();
                                string names = Path.GetFileName(f);
                                int index1 = Array.IndexOf(s[0], names);
                                if (0 <= index1)
                                {

                                    DateTime time_local = File.GetLastWriteTime(f);
                                    DateTime times_dro = DateTime.Parse(s[1][index1]);
                                    int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも

                                    //Console.WriteLine("ファイル名"+f+" ローカル更新日:"+time_local+ " 泥更新日:"+times_dro+ "判定"+time_if);

                                    MessageBoxResult result;
                                    if (time_if == 0)
                                    {
                                        result = MessageBoxResult.No;
                                        log_box.Items.Add("ファイル" + f + "は更新されてません＾u＾");
                                    }
                                    else if (time_if > 0)
                                    {
                                        //MessageBox.Show(f + " はPC上のセーブデータがDropBox上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxにアップロード");
                                        result = MessageBoxResult.Yes;
                                    }
                                    else
                                    {
                                        if (Properties.Settings.Default.dropbox_snyc)
                                        {
                                            result = MessageBox.Show(f + " はDropBox上のセーブデータがPC上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxにアップロード",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Information);
                                        }
                                        else
                                        {

                                            result = MessageBoxResult.Yes;
                                        }
                                    }

                                    if (result == MessageBoxResult.Yes)
                                    {
                                        log_box.Items.Add(f + " をPC上のデータで上書きします");
                                        await Upload(client, input_data[0], names, f);
                                    }
                                    else
                                    {
                                        log_box.Items.Add(f + " の上書きはスキップされました");
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine("クラウドにはこのデータないですね");
                                    await Upload(client, input_data[0], names, f);
                                }
                            }

                            string fors = Path.GetDirectoryName(input_data[1]);
                            //MessageBox.Show(fors+"のアップロードを完了しました");
                            log_box.Items.Add("ゲームタイトル→" + input_data[0] + "のアップロードを完了しました");
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
                Action_now = false;
                close_button.IsEnabled = true;
            }

        }


        private async Task Downloadse(DropboxClient client, string[] input_data, CancellationToken token)
        {
            close_button.IsEnabled = false;
            Action_now = true;
            try
            {
                if (input_data == null)
                {
                    using (StreamReader stream = new StreamReader(MainWindow.file_path))
                    {
                        string line;
                        while ((line = stream.ReadLine()) != null)
                        {
                            String[] lins = line.Split(',');

                            log_box.Items.Add("ゲームタイトル→" + lins[0] + "のダウンロードを開始しました");

                            if (lins.Length > 1)
                            {
                                if (File.Exists(lins[1]))//ファイル
                                {
                                    token.ThrowIfCancellationRequested();
                                    string names = Path.GetFileName(lins[1]);
                                    Console.WriteLine(lins[0] + " " + names + " " + lins[1]);
                                    await Download(client, lins[0], names, lins[1]);
                                }

                                else if (Directory.Exists(lins[1]))//フォルダ
                                {
                                    var s = await ListFolder(client, "/" + lins[0]);

                                    //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);

                                    for (int i = 0; s[0].Length > i; i++)//s の二次配列
                                    {
                                        token.ThrowIfCancellationRequested();
                                        Console.WriteLine(s[0][i] + "更新日：" + s[1][i]);
                                        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
                                        String file_location = lins[1] + @"\" + s[0][i];
                                        DateTime time_local = File.GetLastWriteTime(file_location);
                                        DateTime times_dro = DateTime.Parse(s[1][i]);
                                        int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも
                                        Console.WriteLine(time_if);
                                        MessageBoxResult result;
                                        if (time_if == 0)
                                        {
                                            result = MessageBoxResult.No;
                                            log_box.Items.Add("ファイル:" + s[0][i] + "は更新されてません＾u＾");
                                        }
                                        else if (time_if > 0)
                                        {
                                            if (Properties.Settings.Default.pc_snyc)
                                            {
                                                result = MessageBox.Show(s[0][i] + " はPC上のセーブデータがDropBox上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxからダウンロード",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Information);
                                            }
                                            else
                                            {
                                                result = MessageBoxResult.Yes;
                                            }
                                        }
                                        else
                                        {
                                            result = MessageBoxResult.Yes;
                                            //MessageBox.Show("DropBox上のセーブデータがPC上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？");
                                        }

                                        if (result == MessageBoxResult.Yes)
                                        {

                                            log_box.Items.Add(s[0][i] + " の上書きをします");
                                            string names = Path.GetFileName(s[0][i]);
                                            string dire_names = Path.GetFileName(Path.GetDirectoryName(input_data[1]));

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
                                }
                                else
                                {
                                    log_box.Items.Add("保存先ディレクトリ(" + lins[1] + ")が見つからないためダウンロード処理を中断しました");
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
                            token.ThrowIfCancellationRequested();
                            string names = Path.GetFileName(input_data[1]);
                            Console.WriteLine(input_data[0] + " " + names + " " + input_data[1]);
                            await Download(client, input_data[0], names, input_data[1]);
                        }

                        else if (Directory.Exists(input_data[1]))//フォルダ
                        {
                            string[][] s = await ListFolder(client, "/" + input_data[0]);

                            //string[] files = System.IO.Directory.GetFiles(lins[1], "*", System.IO.SearchOption.AllDirectories);

                            for (int i = 1; s[0].Length > i; i++)//s の二次配列
                            {
                                token.ThrowIfCancellationRequested();
                                Console.WriteLine(s[0][i] + "更新日：" + s[1][i]);
                                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");

                                String file_location = input_data[1] + @"\" + s[0][i];

                                DateTime time_local = File.GetLastWriteTime(file_location);
                                DateTime times_dro = DateTime.Parse(s[1][i]);
                                int time_if = time_local.CompareTo(times_dro);//PC上のデータはドロップボックスのデータよりも
                                Console.WriteLine(time_if);
                                MessageBoxResult result;
                                if (time_if == 0)
                                {
                                    result = MessageBoxResult.No;
                                    log_box.Items.Add("ファイル:" + s[0][i] + "は更新されてません＾u＾");
                                }
                                else if (time_if > 0)
                                {
                                    if (Properties.Settings.Default.pc_snyc)
                                    {
                                        result = MessageBox.Show(s[0][i] + " はPC上のセーブデータがDropBox上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？", "DropBoxからダウンロード",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        result = MessageBoxResult.Yes;
                                    }
                                }
                                else
                                {
                                    result = MessageBoxResult.Yes;
                                    //MessageBox.Show("DropBox上のセーブデータがPC上のセーブデータよりも更新日時が新しいようです。本当に上書きしますか？");
                                }

                                if (result == MessageBoxResult.Yes)
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
                        }
                        else
                        {
                            log_box.Items.Add("保存先ディレクトリ(" + input_data[1] + ")が見つからないためダウンロード処理を中断しました");
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
                close_button.IsEnabled = true;
                Action_now = false;
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
                //MessageBox.Show(path + "を作成しました",
                //"メッセージ",
                //MessageBoxButton.OK,
                //MessageBoxImage.Information);
                log_box.Items.Add("フォルダ" + path + "を作成しました");
            }
            catch (ApiException<CreateFolderError> ex)
            {
                string a = ex.Message;
                log_box.Items.Add(path + "の作成に問題が発生しました。すでにフォルダが存在している可能性があります\n\n理由：" + a);
                /*
                log_box.ScrollIntoView(path + "の作成に問題が発生しました。すでにフォルダが存在している可能性があります");
                MessageBox.Show(path + "の作成に問題が発生しました\n\nすでにフォルダが存在している可能性があります",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                */
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
                using (FileStream fileStream = new FileStream(fileContent, FileMode.Open))
                {
                    var response = await client.Files.UploadAsync("/" + folder + "/" + fileName, WriteMode.Overwrite.Instance, body: fileStream);
                    Console.WriteLine("Uploaded Id {0} Rev {1}", response.Id, response.Rev);
                    //MessageBox.Show(fileName + "のアップロードを完了しました",
                    //"メッセージ",
                    //MessageBoxButton.OK,
                    //MessageBoxImage.Information);
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
                close_button.IsEnabled = true;
                Action_now = false;

            }
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
                close_button.IsEnabled = true;
                Action_now = false;
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                log_box.Items.Add(ex2.Message);
                close_button.IsEnabled = true;
                Action_now = false;
            }
        }

        private void Close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
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
                        cts = new CancellationTokenSource();
                        await Downloadse(client, texts, cts.Token);
                    }
                    else
                    {
                        cts = new CancellationTokenSource();
                        await Uploades(client, texts, cts.Token);
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //一括
            if (types == 0)
            {
                cts = new CancellationTokenSource();
                await Downloadse(client, null, cts.Token);
            }
            else
            {
                cts = new CancellationTokenSource();
                await Uploades(client, null, cts.Token);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var win = new DropBoxList
            {
                Owner = this
            };
            win.ShowDialog();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Action_now)
            {
                MessageBoxResult res = MessageBox.Show("このウィンドウを閉じると実行中のすべての処理は中断されます。\n\nこのウィンドウを閉じますか？", "確認メッセージ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    cts.Cancel();

                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
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
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
