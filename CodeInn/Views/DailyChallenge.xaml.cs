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
using Windows.Networking;
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
using System.Globalization;

namespace CodeInn.Views
{
    /// <summary>
    /// The menu page to retrieve and display Examples
    /// </summary>
    public sealed partial class DailyChallenge : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool isTimeRunning = false;
        private bool toRefresh = false;
        private Problems displayedObject;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private StatusBarProgressIndicator progressbar;

        public DailyChallenge()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
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
            this.WriteTime();
            progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

#endregion

        async private void prepareData()
        {
            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                Frame.Navigate(typeof(Settings));
                return;
            }

            if (!localSettings.Containers.ContainsKey("dailyChallenge"))
            {
                Debug.WriteLine("needed to create");
                localSettings.CreateContainer("dailyChallenge", Windows.Storage.ApplicationDataCreateDisposition.Always);
                localSettings.Containers["dailyChallenge"].Values["Date"] = "2004-01-01T01:01:01.000Z";
                toRefresh = true;
            }
            else
            {
                string lastDate = localSettings.Containers["dailyChallenge"].Values["Date"].ToString();

                string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                DateTime lastchallengedate = DateTime.ParseExact(lastDate, format, CultureInfo.InvariantCulture);

                if (lastchallengedate.Date < DateTime.Now.Date)
                {
                    toRefresh = true;
                }
            }

            Debug.WriteLine("To refresh = " + toRefresh);

            if (toRefresh)
            {
                progressbar.Text = "Fetching new data";
                progressbar.ShowAsync();

                var client = new HttpClient();
                var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/query/daily"));
                var result = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(result);

                try
                {
                    // The server sends a single challenge so the list would be singly-occupied.
                    List<Problems> dailychall = JsonConvert.DeserializeObject<List<Problems>>(result);
                    foreach (Problems prob in dailychall)
                    {
                        localSettings.Containers["dailyChallenge"].Values["Date"] = prob.CreationDate;
                        localSettings.Containers["dailyChallenge"].Values["Content"] = prob.Content;
                        localSettings.Containers["dailyChallenge"].Values["Name"] = prob.Name;
                        localSettings.Containers["dailyChallenge"].Values["Description"] = prob.Description;
                        localSettings.Containers["dailyChallenge"].Values["Id"] = prob.Id;
                        displayedObject = prob;
                    }
                }
                catch
                {
                    Debug.WriteLine("Internal error.");
                    progressbar.Text = "Error";
                }
                progressbar.HideAsync();
            }
            else
            {
                string name = localSettings.Containers["dailyChallenge"].Values["Name"].ToString();
                string content = localSettings.Containers["dailyChallenge"].Values["Content"].ToString();
                string description = localSettings.Containers["dailyChallenge"].Values["Description"].ToString();
                displayedObject = new Problems(0, name, description, content, "Admin");
                //await Task.Delay(1000);
            }

            populateContent();
        }

        private void populateContent()
        {
            TextBlock tn = FindChildControl<TextBlock>(HubQuestion, "name") as TextBlock;
            TextBlock td = FindChildControl<TextBlock>(HubQuestion, "desc") as TextBlock;

            tn.Text = displayedObject.Name;
            td.Text = displayedObject.Description;
        }

        async private void WriteTime()
        {
            DateTime currentDate = DateTime.Now;
            DateTime tomorrow = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day + 1);
            double totalTime = (tomorrow.Ticks - currentDate.Ticks) / 10000000.0 / 60;
            TimeRemaining.Text = "Minutes to submission: " + String.Format("{0:0.000}", totalTime);
            isTimeRunning = true;
            while(totalTime > 0)
            {
                currentDate = DateTime.Now;
                totalTime = (tomorrow.Ticks - currentDate.Ticks) / 10000000.0 / 60;
                TimeRemaining.Text = "Minutes to submission: " + String.Format("{0:0.000}", totalTime);
                await Task.Delay(100);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CodeEditor), new CodeEditorContext(displayedObject, "DailyChallenge"));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.prepareData();
        }

    }
}
