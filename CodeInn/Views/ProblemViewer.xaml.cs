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
    public sealed partial class ProblemViewer : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private StatusBarProgressIndicator progressbar;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        List<Problems> listofitems;

        ObservableCollection<Problems> DB_ProblemList = new ObservableCollection<Problems>();
        ObservableCollection<ListItem> shownCollection = new ObservableCollection<ListItem>();
        public ProblemViewer()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += ReadProblems_Loaded;
        }

        private async void ReadProblems_Loaded(object sender, RoutedEventArgs e)
        {
            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                Frame.Navigate(typeof(Settings));
                return;
            }
            assignToListBox();
        }
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedId = (sender as ListBox).SelectedIndex;
            Problems clickedProblem = (Problems)listofitems[selectedId];
            var navCont = new CodeEditorContext(clickedProblem, "Problems");
            Frame.Navigate(typeof(CodeEditor), navCont);
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

            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                Frame.Navigate(typeof(Settings));
                return;
            }

            var lastcheck = localSettings.Containers["userInfo"].Values["lastcheckproblems"].ToString();
            Debug.WriteLine(System.Uri.EscapeUriString(lastcheck));
            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/query/data?Timestamp=" + System.Uri.EscapeUriString(lastcheck) + "&Table=Problems"));

            var result = await response.Content.ReadAsStringAsync();

            result = result.Trim(new Char[] { '"' });
            Debug.WriteLine(result);

            DatabaseProblem Db_Helper = new DatabaseProblem();
            try
            {
                List<Problems> newprobs = JsonConvert.DeserializeObject<List<Problems>>(result);
                foreach (Problems prob in newprobs)
                {
                    try
                    {
                        Db_Helper.InsertProblem(prob);
                    }
                    catch
                    {
                        Debug.WriteLine("DB error for item of id: " + prob.Id);
                    }
                }
                localSettings.Containers["userInfo"].Values["lastcheckproblems"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                progressbar.Text = "New items";
            }
            catch
            {
                Debug.WriteLine("No new items");
                progressbar.Text = "No New items";
            }
            finally
            {
                assignToListBox();
            }
            progressbar.HideAsync();
        }

        private void assignToListBox()
        {
            ReadProblems dbproblems = new ReadProblems();
            DB_ProblemList = dbproblems.GetAllProblems();
            listofitems = DB_ProblemList.OrderByDescending(i => i.Id).ToList();

            var serialized = localSettings.Containers["userInfo"].Values["PPsolved"].ToString();
            var listofsolved = JsonConvert.DeserializeObject<List<int>>(serialized);

            var requiredList = new List<ListItem>();
            foreach (var items in listofitems)
            {
                if (listofsolved.Contains(items.Id))
                    requiredList.Add(new ListItem(items.Id, items.Name, items.Description, true));
                else
                    requiredList.Add(new ListItem(items.Id, items.Name, items.Description, false));
            }
            listBox.ItemsSource = requiredList;
            shownCollection = new ObservableCollection<ListItem>(requiredList);
        }

        private void Refresh_Problems(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
        }

        private void filterList(object sender, TextChangedEventArgs e)
        {
            listBox.ItemsSource = shownCollection.Where(w => w.Description.ToUpper().Contains(searchBox.Text.ToUpper()));
        }

    }
}
