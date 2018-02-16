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
                using (StreamReader stream = new StreamReader(MainWindow.file_path))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = stream.ReadLine()) != null)
                    {
                        game_view.Items.Add(line.Split(','));
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /// 項目を削除します
            // 選択項目がない場合は処理をしない
            if (game_view.SelectedItems.Count == 0)
                return;
            // 選択された項目を削除
            game_view.Items.RemoveAt(game_view.SelectedIndex);
            file_box.Text = "";
            game_title.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int indexs = game_view.SelectedIndex;
            if (indexs >= 0)
            {
                if (!game_title.Text.Contains(","))
                {
                    game_view.Items.RemoveAt(indexs);
                    game_view.Items.Insert(indexs, new string[] { game_title.Text, file_box.Text });
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
            int counts = game_view.Items.Count;
            if (counts > 0)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(MainWindow.file_path))
                    {

                        for (int i = 0; i < counts; i++)
                        {
                            stream.WriteLine(game_view.Items.GetItemAt(i));
                        }
                    }
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
                    game_view.Items.Add(new string[] { game_title.Text, file_box.Text });
                    game_title.Text = "";
                    file_box.Text = "";
                    game_view.SelectedItems.Clear();

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int counts = game_view.Items.Count;
            if (counts > 0)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(MainWindow.file_path))
                    {

                        for (int i = 0; i < counts; i++)
                        {
                            string[] texts = (string[])game_view.Items.GetItemAt(i);
                            stream.WriteLine(texts[0] + "," + texts[1]);
                        }
                    }

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
                    using (StreamWriter stream = new StreamWriter(MainWindow.file_path))
                    {
                    }
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


        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var win = new DropBoxList
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            game_view.Items.Clear();
        }



        private void Game_view_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int indexs = game_view.SelectedIndex;
            if (indexs >= 0)
            {
                save.IsEnabled = true;
                delete.IsEnabled = true;
                file_box.Text = "";
                game_title.Text = "";
                string[] texts = (string[])game_view.Items.GetItemAt(indexs);
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

        private void Game_view_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                foreach (var s in files)
                {
                    game_view.Items.Add(new string[] { Path.GetFileNameWithoutExtension(s), "" });
                }
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Game_view_DragEnter(object sender, DragEventArgs e)
        {

        }
    }
}
