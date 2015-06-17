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

        private void Login(object sender, RoutedEventArgs e)
        {
            // Query online database first to check if the user is valid.
            if (localSettings.Containers.ContainsKey("userInfo"))
            {
                localSettings.Containers["userInfo"].Values["userName"] = username.Text;
                localSettings.Containers["userInfo"].Values["userEmail"] = email.Text;
                localSettings.Containers["userInfo"].Values["userPass"] = passwordbox.Password;
            }
        }

        private void Signup(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer container =
               localSettings.CreateContainer("userInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);

            // Check online database to check if info is valid.
            // Then write info online.
            if (localSettings.Containers.ContainsKey("userInfo"))
            {
                localSettings.Containers["userInfo"].Values["userName"] = username.Text;
                localSettings.Containers["userInfo"].Values["userEmail"] = email.Text;
                localSettings.Containers["userInfo"].Values["userPass"] = passwordbox.Password;
            }            
        }
    }
}
