using Dropbox.Api;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using WpfApp1.Properties;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private const string RedirectUri = "https://www.dropbox.com/oauth2/authorize/";

        private string oauth2State;

        static private string usernameis;

        public LoginForm(string appKey)
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(new Action<string>(this.Start), appKey);
        }

        public LoginForm()
        {
        }

        public string AccessToken { get; private set; }

        public string UserId { get; private set; }

        public delegate void RefreshEventHandler(object sender, EventArgs e);

        public event RefreshEventHandler RefreshEvent;

        public bool Result { get; private set; }

        private void Start(string appKey)
        {
            this.oauth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, new Uri(RedirectUri), state: oauth2State);
            this.Browser.Navigate(authorizeUri);
        }

        private void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }

            try
            {
                OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(e.Uri);
                if (result.State != this.oauth2State)
                {
                    return;
                }

                this.AccessToken = result.AccessToken;
                this.Uid = result.Uid;
                this.Result = true;
                Settings.Default.AccessToken = AccessToken;
                Settings.Default.Uid = Uid;
                Settings.Default.Save();
                var task = Task.Run((Func<Task>)LoginForm.Run);
                task.Wait();
                var task2 = Task.Run((Func<Task>)MainWindow.Run);
                task2.Wait();
                this.RefreshEvent(this, new EventArgs());
            }
            catch (ArgumentException)
            {
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                e.Cancel = true;
                this.Close();
            }

        }

        static async Task Run()
        {
            using (var dbx = new DropboxClient(Settings.Default.AccessToken))
            {
                var full = await dbx.Users.GetCurrentAccountAsync();
                MessageBox.Show("連携が正常に完了しました。\n\nアカウント：" + full.Name.DisplayName, "DropBox連携", MessageBoxButton.OK, MessageBoxImage.Information);
                usernameis = full.Name.DisplayName;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}