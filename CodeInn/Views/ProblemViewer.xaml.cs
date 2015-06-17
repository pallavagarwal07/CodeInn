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
    /// The menu page to retrieve and display Lessons
    /// </summary>
    public sealed partial class ProblemViewer : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        ObservableCollection<Problems> DB_ProblemList = new ObservableCollection<Problems>();
        public ProblemViewer()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadProblems_Loaded;
        }

        private void ReadProblems_Loaded(object sender, RoutedEventArgs e)
        {
            ReadProblems dbproblems = new ReadProblems();
            DB_ProblemList = dbproblems.GetAllProblems();
            listBox.ItemsSource = DB_ProblemList.OrderByDescending(i => i.Id).ToList();
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

            var lastcheck = localSettings.Containers["userInfo"].Values["lastcheck"].ToString();
            Debug.WriteLine(System.Uri.EscapeUriString(lastcheck));
            var response = await client.GetAsync(new Uri("http://ws.varstack.com/time.php?Timestamp=" + System.Uri.EscapeUriString(lastcheck) + "&Table=Problems&Category=easy"));

            while (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                response = await client.GetAsync(response.Headers.Location);
            }
            var result = await response.Content.ReadAsStringAsync();
            result = result.Replace("\"", string.Empty);
            Debug.WriteLine(result);

            List<Problems> newprobs = JsonConvert.DeserializeObject<List<Problems>>(result);
            foreach(Problems prob in newprobs)
            {
                DatabaseProblem Db_Helper = new DatabaseProblem();
                Db_Helper.InsertProblem(prob);
            }

            localSettings.Containers["userInfo"].Values["lastcheck"] = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss");
        }

        private void Refresh_Lessons(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
            ReadProblems dbproblems = new ReadProblems();
            DB_ProblemList = dbproblems.GetAllProblems();
            listBox.ItemsSource = DB_ProblemList.OrderByDescending(i => i.Id).ToList();
        }

    }
}