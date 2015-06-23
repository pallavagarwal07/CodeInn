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
            listBox.ItemsSource = DB_LessonList.OrderBy(i => i.Id).ToList();//Binding DB data to LISTBOX and Latest contact ID can Display first. 
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

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lessons clickedProblem = (Lessons)(sender as ListBox).SelectedItem;
            Frame.Navigate(typeof(LessonViewer2), clickedProblem);
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


        async private void ReadDataFromWeb()
        {
            var client = new HttpClient();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                return;
            }

            var lastcheck = localSettings.Containers["userInfo"].Values["lastchecklessons"].ToString();
            Debug.WriteLine(System.Uri.EscapeUriString(lastcheck));
            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/api/query?Timestamp=" + System.Uri.EscapeUriString(lastcheck) + "&Table=Lessons"));

            var result = await response.Content.ReadAsStringAsync();

            result = result.Trim(new Char[] { '"' });
            Debug.WriteLine(result);
            
            DatabaseLesson Db_Helper = new DatabaseLesson();     
            try
            {
                List<Lessons> newless = JsonConvert.DeserializeObject<List<Lessons>>(result);
                foreach (Lessons less in newless)
                {
                    try
                    {
                        Db_Helper.InsertLesson(less);
                    }
                    catch
                    {
                        Debug.WriteLine("DB error for item of id: " + less.Id);
                    }
                }
                localSettings.Containers["userInfo"].Values["lastchecklessons"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            catch
            {
                Debug.WriteLine("No new items");
            }
            finally
            {
                ReadLessons dblessons = new ReadLessons();
                DB_LessonList = dblessons.GetAllLessons();
                listBox.ItemsSource = DB_LessonList.OrderByDescending(i => i.Id).ToList();
            }
        }

        private void Refresh_Lessons(object sender, RoutedEventArgs e)
        {
            ReadDataFromWeb();
        }

    }
}