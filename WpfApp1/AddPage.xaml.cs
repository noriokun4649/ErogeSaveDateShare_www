using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfApp1
{
    /// <summary>
    /// AddPage.xaml の相互作用ロジック
    /// </summary>
    public partial class AddPage : Page
    {
        public AddPage()
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
                    if (IsSpace(game_title.Text, file_box.Text))
                    {
                        game_view.Items.RemoveAt(indexs);
                        game_view.Items.Insert(indexs, new string[] { game_title.Text, file_box.Text });
                        game_title.Text = "";
                        file_box.Text = "";

                    }
                }
                else
                {
                    snack_mes.Content = "ゲームタイトルに「,」を使うことはできません";
                    snack.IsActive = true;
                    //MessageBox.Show("ゲームタイトルに「,」を使うことはできません","エラー",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }
        private bool IsConflict(string data)
        {
            string[][] address = this.game_view.Items.Cast<string[]>().ToArray();
            var addres = new ArrayList();
            for (int i = 0; i < address.Length; i++)
            {
                addres.Add(address[i][0]);
            }
            if (addres.Contains(data))
            {
                snack_mes.Content = "ゲームタイトルが競合しています。";
                snack.IsActive = true;
                //MessageBox.Show("ゲームタイトルが競合しています。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            return false;
        }

        private bool IsSpace(string data)
        {
            return data.Replace(" ", "").Replace("　", "").Equals("");
        }

        private bool IsSpace(string title, string path)
        {
            if (IsSpace(title))
            {
                snack_mes.Content = "ゲームタイトルは空白にできません。";
                snack.IsActive = true;
                //MessageBox.Show("空白は設定できません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }else if (IsSpace(path))
            {
                snack_mes.Content = "セーブデータのパスを指定してください。";
                snack.IsActive = true;
                return false;
            }
            return true;
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (IsSpace(game_title.Text, file_box.Text) && !IsConflict(game_title.Text))
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
                    snack_mes.Content = "ゲームタイトルに「,」を使うことはできません";
                    snack.IsActive = true;
                    //MessageBox.Show("ゲームタイトルに「,」を使うことはできません","エラー",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int counts = game_view.Items.Count;
                if (counts > 0)
                {
                    using (StreamWriter stream = new StreamWriter(MainWindow.file_path))
                    {

                        for (int i = 0; i < counts; i++)
                        {
                            string[] texts = (string[])game_view.Items.GetItemAt(i);
                            if (!texts[1].Equals(""))
                            {
                                stream.WriteLine(texts[0] + "," + texts[1]);
                            }
                            else
                            {
                                throw new Exception("不正なデータがあります。\n\n「" + texts[0] + "」にセーブデータ場所が設定されていません。");
                            }
                        }
                    }
                }
                else
                {
                    using (StreamWriter stream = new StreamWriter(MainWindow.file_path))
                    {
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

        private void snack_mes_ActionClick(object sender, RoutedEventArgs e)
        {
            snack.IsActive = false;
        }
    }
}
