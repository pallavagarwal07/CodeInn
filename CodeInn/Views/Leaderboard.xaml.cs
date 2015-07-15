using CodeInn.Common;
using CodeInn.Helpers;
using CodeInn.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeInn.Views
{
    /// <summary>
    /// The menu page to retrieve and display the online leaderboard
    /// </summary>
    public sealed partial class Leaderboard : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private StatusBarProgressIndicator progressbar;
        
        ObservableCollection<LeaderboardItem> leaderList = new ObservableCollection<LeaderboardItem>();
        public Leaderboard()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadProblems_Loaded;
        }

        private void ReadProblems_Loaded(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        async private void GetDataFromWeb()
        {
            progressbar.Text = "Fetching new data";
            progressbar.ShowAsync();

            var client = new HttpClient();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                Frame.Navigate(typeof(Settings));
                return;
            }

            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/leaderboard"));

            var result = await response.Content.ReadAsStringAsync();

            result = result.Trim(new Char[] { '"' });
            Debug.WriteLine(result);

            try
            {
                List<LeaderboardItem> leaders = JsonConvert.DeserializeObject<List<LeaderboardItem>>(result);
                progressbar.Text = "New items";
                listBox.ItemsSource = leaders.OrderByDescending(i => i.Points).ToList();
            }
            catch
            {
                progressbar.Text = "Error";
            }
            progressbar.HideAsync();
        }
    }
}
