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
    /// The menu page to retrieve and display Lessons
    /// </summary>
    public sealed partial class TipViewer : Page
    {
        private NavigationHelper navigationHelper;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private StatusBarProgressIndicator progressbar;
        
        ObservableCollection<Tips> DB_TipList = new ObservableCollection<Tips>();
        public TipViewer()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadTips_Loaded;
        }

        private void ReadTips_Loaded(object sender, RoutedEventArgs e)
        {
            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                msgbox.ShowAsync();
                Frame.Navigate(typeof(Views.Settings));
                return;
            }

            ReadTips dbtips = new ReadTips();
            DB_TipList = dbtips.GetAllTips();
            listBox.ItemsSource = DB_TipList.OrderByDescending(i => i.Id).ToList();
        }
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
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
                return;
            }

            var lastcheck = localSettings.Containers["userInfo"].Values["lastchecktips"].ToString();
            Debug.WriteLine(System.Uri.EscapeUriString(lastcheck));
            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/query/data?Timestamp=" + System.Uri.EscapeUriString(lastcheck) + "&Table=Tips"));

            var result = await response.Content.ReadAsStringAsync();

            result = result.Trim(new Char[] { '"' });
            Debug.WriteLine(result);

            DatabaseTip Db_Helper = new DatabaseTip();
            try
            {
                List<Tips> newprobs = JsonConvert.DeserializeObject<List<Tips>>(result);
                foreach (Tips prob in newprobs)
                {
                    try
                    {
                        Db_Helper.InsertTip(prob);
                    }
                    catch
                    {
                        Debug.WriteLine("DB error for item of id: " + prob.Id);
                    }
                }
                localSettings.Containers["userInfo"].Values["lastchecktips"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                progressbar.Text = "New items";
            }
            catch
            {
                Debug.WriteLine("No new items");
                progressbar.Text = "No New items";
            }
            finally
            {
                ReadTips dbtips = new ReadTips();
                DB_TipList = dbtips.GetAllTips();
                listBox.ItemsSource = DB_TipList.OrderByDescending(i => i.Id).ToList();
            }
            progressbar.HideAsync();
        }

        private void Refresh_Tips(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
        }

    }
}
