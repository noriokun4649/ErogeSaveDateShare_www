using System;
using System.Windows;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using System.Linq;

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
            save.IsEnabled = false;
            delete.IsEnabled = false;
            try
            {
                StreamReader stream = new StreamReader(MainWindow.file_path);
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = stream.ReadLine()) != null)
                {
                    ListBox1.Items.Add(line);
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /// 項目を削除します
            // 選択項目がない場合は処理をしない
            if (ListBox1.SelectedItems.Count == 0)
                return;

            // 選択された項目を削除

            ListBox1.Items.RemoveAt(ListBox1.SelectedIndex);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int indexs = ListBox1.SelectedIndex;
            if (indexs >= 0)
            {
                if (!game_title.Text.Contains(","))
                {
                    ListBox1.Items.RemoveAt(indexs);
                    ListBox1.Items.Insert(indexs, game_title.Text + "," + file_box.Text);
                }
                else
                {
                    MessageBox.Show("ゲームタイトルに「,」を使うことはできません",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int counts = ListBox1.Items.Count;
            if (counts > 0)
            {
                try
                {
                    StreamWriter stream = new StreamWriter(MainWindow.file_path);

                    for (int i = 0; i < counts; i++)
                    {
                        stream.WriteLine(ListBox1.Items.GetItemAt(i));
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }
            }
            this.Close();

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("セーブデータの可能性が高いファイル", "*.sav;*.ksd;*.sd;*.dat;*.tbl;*.sys"));
            dialog.Filters.Add(new CommonFileDialogFilter("savファイル", "*.sav"));
            dialog.Filters.Add(new CommonFileDialogFilter("ksd ファイル", "*.ksd"));
            dialog.Filters.Add(new CommonFileDialogFilter("sdファイル", "*.sd"));
            dialog.Filters.Add(new CommonFileDialogFilter("datファイル", "*.dat"));
            dialog.Filters.Add(new CommonFileDialogFilter("tblファイル", "*.tbl"));
            dialog.Filters.Add(new CommonFileDialogFilter("sysファイル", "*.sys"));
            dialog.Filters.Add(new CommonFileDialogFilter("すべてのファイル", "*.*"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.file_box.Text = dialog.FileName;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog("フォルダーの選択")
            {

                // 選択形式をフォルダースタイルにする IsFolderPicker プロパティを設定
                IsFolderPicker = true
            };
            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                file_box.Text = dialog.FileName;
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if ((game_title.Text != "") & (file_box.Text != ""))
            {
                if (!game_title.Text.Contains(","))
                {
                    ListBox1.Items.Add(game_title.Text + "," + file_box.Text);
                    game_title.Text = "";
                    file_box.Text = "";
                }
                else
                {
                    MessageBox.Show("ゲームタイトルに「,」を使うことはできません",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                }
            }
        }

        private void ListBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            int indexs = ListBox1.SelectedIndex;
            if (indexs >= 0)
            {
                save.IsEnabled = true;
                delete.IsEnabled = true;
                file_box.Text = "";
                game_title.Text = "";
                string text = (string)ListBox1.Items.GetItemAt(indexs);
                string[] texts = text.Split(',');
                game_title.Text = texts[0];
                if (texts.Length > 1)
                {
                    file_box.Text = texts[1];
                }
            }
            else
            {
                save.IsEnabled = false;
                delete.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int counts = ListBox1.Items.Count;
            if (counts > 0)
            {
                try
                {
                    StreamWriter stream = new StreamWriter(MainWindow.file_path);

                    for (int i = 0; i < counts; i++)
                    {
                        stream.WriteLine(ListBox1.Items.GetItemAt(i));
                    }
                    stream.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    StreamWriter stream = new StreamWriter(MainWindow.file_path);
                    stream.Close();
                }
                catch (Exception exs)
                {
                    MessageBox.Show(exs.Message,
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                }

            }
        }

        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                foreach (var s in files)
                {
                    ListBox1.Items.Add(Path.GetFileNameWithoutExtension(s));

                }
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var win = new DropBoxList();
            win.Owner = this;
            win.ShowDialog();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            ListBox1.Items.Clear();
        }
    }
}
