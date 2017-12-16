using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Linq;
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
            ListFolder(client, "");
        }
       

        private async Task<ListFolderResult> ListFolder(DropboxClient client, string path)
        {
            Console.WriteLine("--- Files ---");
            var list = await client.Files.ListFolderAsync(path);
            int size_fol = list.Entries.Count;
            int counts_fol = 1;
            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                Console.WriteLine("D  {0}/", item.Name);
                //ListBox_drop.Items.Add("フォルダ "+ item.Name+"/");

                var list2 = await client.Files.ListFolderAsync("/"+item.Name);

                int size = list2.Entries.Count;
                int counts = 1;


                if (size_fol > counts_fol) 
                {
                    ListBox_drop.Items.Add("フォルダ ├" + item.Name);
                }
                else
                {
                    ListBox_drop.Items.Add("フォルダ └" + item.Name);
                }

                foreach (var item_file in list2.Entries.Where(i => i.IsFile))
                {
                    var file = item_file.AsFile;
                    // Asia/Tokyo タイムゾーンの情報を取得
                    TimeZoneInfo jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    // 変換元DateTimeのKindプロパティが指すタイムゾーンから、指定したタイムゾーンに変換
                    DateTime now_jst = TimeZoneInfo.ConvertTime(file.ServerModified, jst);
                    

                    Console.WriteLine("F{0,8} {1}",
                        file.Size,
                        item_file.Name);
                    Console.WriteLine("C"+counts + " S"+size);
                    if (size > counts ) {
                        ListBox_drop.Items.Add("ファイル 　" + "├" + item_file.Name + " - 更新日:" + now_jst.ToString());
                    }
                    else {
                        ListBox_drop.Items.Add("ファイル 　" + "└" + item_file.Name + " - 更新日:" + now_jst.ToString());
                    }
                    counts++;
                }
                counts_fol++;

            }
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
                ListBox_drop.Items.Add("DropBoxAPIの制限でこれ以上の読み込みができません");

            }
            return list;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
    }
}
