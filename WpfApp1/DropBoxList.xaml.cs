using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// DropBoxList.xaml の相互作用ロジック
    /// </summary>
    public partial class DropBoxList : Window 
    {
        public DropBoxList()
        {
                InitializeComponent();
                DropboxClient client = new DropboxClient(Properties.Settings.Default.AccessToken);
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
                int size_fol = list.Entries.Count;
                load_file.Maximum = size_fol;
                int counts_fol = 1;
                // show folders then files
                foreach (var item in list.Entries.Where(i => i.IsFolder))
                {
                    //Console.WriteLine("D  {0}/", item.Name);
                    //ListBox_drop.Items.Add("フォルダ "+ item.Name+"/");
                    load_file.Value = counts_fol;

                    var list2 = await client.Files.ListFolderAsync("/" + item.Name);

                    int size = list2.Entries.Count;
                    int counts = 1;


                    if (size_fol > counts_fol)
                    {
                        listview.Items.Add(new string[] { "ゲームタイトル", "├" + item.Name, "情報なし" });
                    }
                    else
                    {
                        listview.Items.Add(new string[] { "ゲームタイトル", "└" + item.Name, "情報なし" });
                    }

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
                        if (size > counts)
                        {
                            listview.Items.Add(new string[] { "ファイル", "　├" + item_file.Name, now_jst.ToString() });
                        }
                        else
                        {
                            listview.Items.Add(new string[] { "ファイル", "　└" + item_file.Name, now_jst.ToString() });
                        }
                        counts++;
                    }
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
                    listview.Items.Add(new string[] { "ERROR", "DropBoxAPIの制限でこれ以上の読み込みができません", "" });

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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
    }
}
