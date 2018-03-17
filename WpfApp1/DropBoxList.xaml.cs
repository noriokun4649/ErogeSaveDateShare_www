using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// DropBboxList2.xaml の相互作用ロジック
    /// </summary>
    public partial class DropBoxList : Window , IDisposable
    {
        List<List<String[]>> folders = new List<List<String[]>>();
        DropboxClient client;
        public DropBoxList()
        {
            InitializeComponent();
            client = new DropboxClient(Properties.Settings.Default.AccessToken);
            Lists(client, "");
        }
        private async void Lists(DropboxClient client, string path)
        {

            await ListFolder(client, path);

        }


        private async Task<ListFolderResult> ListFolder(DropboxClient client, string path)
        {

            try
            {
                //Console.WriteLine("--- Files ---");
                var list = await client.Files.ListFolderAsync(path);
                int size_fol = list.Entries.Count-1;
                load_file.Maximum = size_fol;
                int counts_fol = 1;
                loading.Text = "読み込み中...("+counts_fol + "/" + size_fol+")";
                // show folders then files
                foreach (var item in list.Entries.Where(i => i.IsFolder))
                {
                    //Console.WriteLine("D  {0}/", item.Name);
                    //ListBox_drop.Items.Add("フォルダ "+ item.Name+"/");
                    load_file.Value = counts_fol;
                    loading.Text = "読み込み中...(" + counts_fol + "/" + size_fol + ")";
                    var list2 = await client.Files.ListFolderAsync("/" + item.Name);

                    int size = list2.Entries.Count;
                    int counts = 1;


                    folder.Items.Add(new string[] { item.Name });

                    List<String[]> files = new List<string[]>();
                    foreach (var item_file in list2.Entries.Where(i => i.IsFile))
                    {
                        var file = item_file.AsFile;
                        // Asia/Tokyo タイムゾーンの情報を取得
                        TimeZoneInfo jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                        // 変換元DateTimeのKindプロパティが指すタイムゾーンから、指定したタイムゾーンに変換
                        DateTime now_jst = TimeZoneInfo.ConvertTime(file.ServerModified, jst);

                        //client.Files.

                        /*Console.WriteLine("F{0,8} {1}",
                            file.Size,
                            item_file.Name);
                        Console.WriteLine("C"+counts + " S"+size);
                        */
                        files.Add(new string[] { item_file.Name, now_jst.ToString() });
                        counts++;
                    }
                    folders.Add(files);
                    counts_fol++;
                }
                load_file.Value = size_fol;
                loading.Text = "読み込み完了";

                /*
                foreach (var item in list.Entries.Where(i => i.IsFile))
                {
                    var file = item.AsFile;

                    Console.WriteLine("F{0,8} {1}",
                        file.Size,
                        item.Name);
                    ListBox_drop.Items.Add("ファイル " + item.Name +" Size:"+ file.Size);
                }
                */
                if (list.HasMore)
                {
                    Console.WriteLine("   ...");
                    //listview.Items.Add(new string[] { "ERROR", "DropBoxAPIの制限でこれ以上の読み込みができません", "" });

                }
                return list;
            }
            catch (BadInputException exs)
            {
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return null;
            }
            catch (HttpRequestException exa)
            {
                MessageBox.Show("HTTPリクエストに問題が発生しました。コンピュータがインターネットに接続されているか確認してください。\n" + exa.Message,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return null;
            }
            catch (Exception es)
            {
                MessageBox.Show("エラー\n" + es.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return null;
            }
        }

        private void Folder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indexs = folder.SelectedIndex;
            if (indexs >= 0)
            {
                Folder_button.IsEnabled = true;
                fail.Items.Clear();
                foreach (var item in folders[indexs])
                {
                    fail.Items.Add(item);
                }

            }
            else
            {
                Folder_button.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)//フォルダ
        {
            var name = (string[])folder.Items.GetItemAt(folder.SelectedIndex);
            try
            {
                var deletes = await client.Files.DeleteV2Async("/" + name[0]);
                folder.Items.RemoveAt(folder.SelectedIndex);
                fail.Items.Clear();
                MessageBox.Show("フォルダ：" + name[0] + "を削除しました。");
            }
            catch (BadInputException exs)
            {
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (HttpRequestException exa)
            {
                MessageBox.Show("HTTPリクエストに問題が発生しました。コンピュータがインターネットに接続されているか確認してください。\n" + exa.Message,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (DropboxException error)
            {
                MessageBox.Show("フォルダ：" + name[0] + "の削除に失敗しました。\n\n"+ error.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)//ファイル
        {
            var name = (string[])folder.Items.GetItemAt(folder.SelectedIndex);
            var name2 = (string[])fail.Items.GetItemAt(fail.SelectedIndex);
            try
            {
                var deletes = await client.Files.DeleteV2Async("/" + name[0] + "/" + name2[0]);
                fail.Items.RemoveAt(fail.SelectedIndex);
                MessageBox.Show("ファイル：" + name2[0] + "の削除しました。");
            }
            catch (BadInputException exs)
            {
                string masssge = exs.Message.Replace("Invalid authorization value in HTTP header", "HTTPヘッダーの認証項目が無効です。").Replace("Error in call to API function", "API 関数の呼び出しでエラーが発生しました").Replace("oauth2-access-token", "DropBoxの連携が正常に完了してない可能性があります。確認してください。");
                MessageBox.Show("無効なHTTPリクエストです。\n" + masssge,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (HttpRequestException exa)
            {
                MessageBox.Show("HTTPリクエストに問題が発生しました。コンピュータがインターネットに接続されているか確認してください。\n" + exa.Message,
                "無効なHTTPリクエスト",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (DropboxException error)
            {
                MessageBox.Show("フォルダ：" + name2[0] + "の削除に失敗しました。\n\n" + error.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }

        }

            private void Fail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indexs = fail.SelectedIndex;
            File_button.IsEnabled = indexs >= 0;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            fail.Items.Clear();
            folder.Items.Clear();
            DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
            Lists(client, "");
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client !=null)
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

    }

}
