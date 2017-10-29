using Microsoft.Win32;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "全てのファイル(*.*)|SAVEファイル(*.sav)";
            if (dialog.ShowDialog() == true)
            {
                this.file_box.Text = dialog.FileName;
            }
            else
            {
                this.file_box.Text = "キャンセルされました";
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "○○するフォルダーを選択してください。あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.Cancel)
            {
                // [キャンセル] をクリックされた場合は終了
                return;
            }
            // フォルダーパスを取得
            string fileName = folderBrowserDialog.SelectedPath;
            // ダイアログのインスタンスを破棄する
            folderBrowserDialog.Dispose();
        }
    }
}
