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
    public sealed partial class LessonViewer : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        ObservableCollection<Lessons> DB_LessonList = new ObservableCollection<Lessons>();
        public LessonViewer()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadLessons_Loaded;
        }

        private void ReadLessons_Loaded(object sender, RoutedEventArgs e)
        {
            ReadLessons dblessons = new ReadLessons();
            DB_LessonList = dblessons.GetAllLessons();//Get all DB contacts 
            listBox.ItemsSource = DB_LessonList.OrderByDescending(i => i.Id).ToList();//Binding DB data to LISTBOX and Latest contact ID can Display first. 
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


        async private void ReadDataFromWeb()
        {
            var client = new HttpClient(); // Add: using System.Net.Http;
            var response = await client.GetAsync(new Uri("http://117.197.50.43:8888/time.php%5c?Timestamp%5C=2015-06-13%2020%3A39%3A46%5C&Table%5C=Questions%5C&Category%5C=easy"));

            //http://117.197.50.43:8888/time.php?Timestamp=2015-06-13+20%3A39%3A46&Table=Questions&Category=easy

            while (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                response = await client.GetAsync(response.Headers.Location);
            }
            var result = await response.Content.ReadAsStringAsync();
            result = result.Replace("\"", string.Empty);

            // Doesn't work right now because the output cannot be parsed as Lessons
            List<Lessons> newlessons = JsonConvert.DeserializeObject<List<Lessons>>(result);
            Debug.WriteLine(newlessons[0].Content);
        }

        private void Refresh_Lessons(object sender, RoutedEventArgs e)
        {
            //ReadDataFromWeb();
            ReadLessons dblessons = new ReadLessons();
            DB_LessonList = dblessons.GetAllLessons();
            listBox.ItemsSource = DB_LessonList.OrderByDescending(i => i.Id).ToList();//Binding DB data to LISTBOX and Latest lessons can Display first.             
        }

    }
}