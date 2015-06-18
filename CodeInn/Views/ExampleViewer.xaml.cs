using CodeInn.Common;
using CodeInn.Helpers;
using CodeInn.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// The menu page to retrieve and display Examples
    /// </summary>
    public sealed partial class ExampleViewer : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        ObservableCollection<Examples> DB_ExampleList = new ObservableCollection<Examples>();
        public ExampleViewer()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadExamples_Loaded;
        }

        private void ReadExamples_Loaded(object sender, RoutedEventArgs e)
        {
            ReadExamples dbproblems = new ReadExamples();
            DB_ExampleList = dbproblems.GetAllExamples();
            listBox.ItemsSource = DB_ExampleList.OrderByDescending(i => i.Id).ToList();
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        async private void GetDataFromWeb()
        {
            var client = new HttpClient();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                return;
            }

            var lastcheck = localSettings.Containers["userInfo"].Values["lastcheckexamples"].ToString();
            Debug.WriteLine(System.Uri.EscapeUriString(lastcheck));
            var response = await client.GetAsync(new Uri("http://ws.varstack.com/time.php?Timestamp=" + System.Uri.EscapeUriString(lastcheck) + "&Table=Examples&Category=easy"));

            while (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                response = await client.GetAsync(response.Headers.Location);
            }
            var result = await response.Content.ReadAsStringAsync();
            result = result.Replace("\"", string.Empty);
            Debug.WriteLine(result);

            try
            {
                List<Examples> newex = JsonConvert.DeserializeObject<List<Examples>>(result);

                foreach (Examples ex in newex)
                {
                    DatabaseExample Db_Helper = new DatabaseExample();
                    Db_Helper.InsertExample(ex);
                }

                localSettings.Containers["userInfo"].Values["lastcheckexamples"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                Debug.WriteLine("No new items");
            }
            finally
            {
                ReadExamples dbproblems = new ReadExamples();
                DB_ExampleList = dbproblems.GetAllExamples();
                listBox.ItemsSource = DB_ExampleList.OrderByDescending(i => i.Id).ToList();
            }
        }

        private void Refresh_Examples(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
        }

    }
}