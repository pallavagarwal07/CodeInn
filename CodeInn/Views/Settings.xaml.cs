using CodeInn.Helpers;
using CodeInn.Model;
using CodeInn.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using SQLite;
using Windows.Storage;
using System.Diagnostics;
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Newtonsoft.Json;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CodeInn.Views
{
    /// <summary>
    /// Settings page accessed from HubPage
    /// </summary>
    public sealed partial class Settings : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        public Settings()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            PopulateIfLoggedIn();
        }

        public void PopulateIfLoggedIn()
        {
            if (localSettings.Containers.ContainsKey("userInfo"))
            {
                username.Text = localSettings.Containers["userInfo"].Values["userName"].ToString();
                email.Text = localSettings.Containers["userInfo"].Values["userEmail"].ToString();
                passwordbox.Password = localSettings.Containers["userInfo"].Values["userPass"].ToString();
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            DatabaseLesson Db_Helper = new DatabaseLesson(); 
            if (username.Text != "" & email.Text != "")
            {
                Db_Helper.InsertLesson(new Lessons(username.Text, email.Text, "Lorem Ipsum"));
                Frame.Navigate(typeof(LessonViewer));
            }
            else
            {
                MessageDialog messageDialog = new MessageDialog("Please fill two fields");
                await messageDialog.ShowAsync();
            }
        }

        private static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "CodeInnDatabase.sqlite"));
        private SQLiteConnection dbConn;

        private void recreateTables<T>()
        {
			using (dbConn = new SQLiteConnection(DB_PATH))
			{
				try
				{
					dbConn.DropTable<T>();
                    Debug.WriteLine("Deleted " + typeof(T).Name);
				}
				finally
				{
					dbConn.CreateTable<T>();
				}
			}
			Debug.WriteLine("Created " + typeof(T).Name);
        }

        private class gotdata
        {
            public string PPsolved { get; set; }
            public int Points { get; set; }
        }

        private async Task populateSolvedData()
        {
            var client = new HttpClient();
            string result;
            try
            {
                var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/users/getuserdata?Username=" + System.Uri.EscapeUriString(username.Text)));
                result = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                localSettings.DeleteContainer("userInfo");
                MessageDialog messageDialog = new MessageDialog("Unable to get response from server. Please login again.");
                messageDialog.ShowAsync();
                return;
            }

            Debug.WriteLine(result);
            try
            {
                List<gotdata> udata = JsonConvert.DeserializeObject<List<gotdata>>(result);
                List<int> values = new List<int>();

                foreach (string value in udata[0].PPsolved.Split(','))
                {
                    if(value != "")
                    {
                        values.Add(Convert.ToInt32(value));
                        Debug.WriteLine("Adding " + value + " to list");
                    }
                }
                string serialized = JsonConvert.SerializeObject(values);
                localSettings.Containers["userInfo"].Values["PPsolved"] = serialized;
                localSettings.Containers["userInfo"].Values["Points"] = udata[0].Points;

                Debug.WriteLine("Points: " + (int)localSettings.Containers["userInfo"].Values["Points"]);
            }
            catch
            {
                localSettings.DeleteContainer("userInfo");
                MessageDialog messageDialog = new MessageDialog("There was an error parsing user data. Please login again.");
                messageDialog.ShowAsync();
            }
        }

        private async void Login(object sender, RoutedEventArgs e)
        {
            LoadingBar.IsEnabled = true;
            LoadingBar.Visibility = Visibility.Visible;

            if (username.Text == "" & email.Text == "")
            {
                MessageDialog messageDialog = new MessageDialog("Please fill atleast one of email/username fields");
                await messageDialog.ShowAsync();
                return;
            }
            if (passwordbox.Password == "")
            {
                MessageDialog messageDialog = new MessageDialog("Please enter your password");
                await messageDialog.ShowAsync();
                return;
            }

            var client = new Windows.Web.Http.HttpClient();
            HttpStringContent content = new HttpStringContent(
                    "{ \"Username\": \"" + username.Text + "\", \"Password\": \"" + passwordbox.Password + "\" }",
                    UnicodeEncoding.Utf8,
                    "application/json");

            Debug.WriteLine(content);

            var uri = new Uri("http://codeinn-acecoders.rhcloud.com:8000/users/login");

            var response = await client.PostAsync(uri, content);

            var result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                Debug.WriteLine("Accepted");
                MessageDialog messageDialog = new MessageDialog(result);
                messageDialog.ShowAsync();
            }
            else
            {
                Debug.WriteLine("Rejected " + response.StatusCode);
                MessageDialog messageDialog = new MessageDialog(result);
                await messageDialog.ShowAsync();
                LoadingBar.IsEnabled = false;
                LoadingBar.Visibility = Visibility.Collapsed;
                return;
            }

            // Query online database first to check if the user is valid.
            recreateTables<Problems>();
            recreateTables<Examples>();
            recreateTables<Tips>();
            recreateTables<Lessons>();

            Windows.Storage.ApplicationDataContainer container =
                localSettings.CreateContainer("userInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);

            if (localSettings.Containers.ContainsKey("userInfo"))
            {
                localSettings.Containers["userInfo"].Values["userName"] = username.Text;
                localSettings.Containers["userInfo"].Values["userEmail"] = email.Text;
                localSettings.Containers["userInfo"].Values["userPass"] = passwordbox.Password;
                localSettings.Containers["userInfo"].Values["lastcheckexamples"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastcheckproblems"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastchecklessons"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastchecktips"] = "2014-01-01T01:01:01Z";
                double timeSpent = 0;
                localSettings.Containers["userInfo"].Values["timeSpent"] = ((double)timeSpent) / 60;
            }

            await populateSolvedData();

            LoadingBar.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Collapsed;
        }

        private async void Signup(object sender, RoutedEventArgs e)
        {
            LoadingBar.IsEnabled = true;
            LoadingBar.Visibility = Visibility.Visible;

            if (username.Text == "" || email.Text == "")
            {
                MessageDialog messageDialog = new MessageDialog("Please fill email and username.");
                await messageDialog.ShowAsync();
                return;
            }
            if (passwordbox.Password == "")
            {
                MessageDialog messageDialog = new MessageDialog("Please enter your password");
                await messageDialog.ShowAsync();
                return;
            }

            Windows.Storage.ApplicationDataContainer container =
               localSettings.CreateContainer("userInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);

            // Adding user to online database
			var client = new Windows.Web.Http.HttpClient();
			HttpStringContent content = new HttpStringContent(
					"{ \"Username\": \"" + username.Text + "\", \"Email\": \"" + email.Text + "\", \"Password\": \"" + passwordbox.Password + "\" }",
					UnicodeEncoding.Utf8,
					"application/json");

            Debug.WriteLine(content);

			var uri = new Uri("http://codeinn-acecoders.rhcloud.com:8000/users/signup");

			var response = await client.PostAsync(uri, content);

			var result = await response.Content.ReadAsStringAsync();

			if (response.StatusCode == HttpStatusCode.Ok)
			{
				Debug.WriteLine("Accepted");
                MessageDialog messageDialog = new MessageDialog(result);
                messageDialog.ShowAsync();
			}
			else
			{
				Debug.WriteLine("Rejected " + response.StatusCode);
                MessageDialog messageDialog = new MessageDialog(result);
                await messageDialog.ShowAsync();
                LoadingBar.IsEnabled = false;
                LoadingBar.Visibility = Visibility.Collapsed;
                return;
			}

            recreateTables<Problems>();
            recreateTables<Examples>();
            recreateTables<Tips>();
            recreateTables<Lessons>();

            if (localSettings.Containers.ContainsKey("userInfo"))
            {
                localSettings.Containers["userInfo"].Values["userName"] = username.Text;
                localSettings.Containers["userInfo"].Values["userEmail"] = email.Text;
                localSettings.Containers["userInfo"].Values["userPass"] = passwordbox.Password;
                localSettings.Containers["userInfo"].Values["lastcheckexamples"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastcheckproblems"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastchecklessons"] = "2014-01-01T01:01:01Z";
                localSettings.Containers["userInfo"].Values["lastchecktips"] = "2014-01-01T01:01:01Z";
                double timeSpent = 0;
                localSettings.Containers["userInfo"].Values["timeSpent"] = ((double)timeSpent) / 60;
            }

            await populateSolvedData();

            LoadingBar.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Collapsed;
        }
    }
}
