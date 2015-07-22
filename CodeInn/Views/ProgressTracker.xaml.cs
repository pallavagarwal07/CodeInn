using CodeInn.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CodeInn.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressTracker : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private StatusBarProgressIndicator progressbar;
        private string username = "";
        private string time = "";

        public ProgressTracker()
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

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
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
            progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            doTasks();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void doTasks()
        {
            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                Frame.Navigate(typeof(Settings));
                return;
            }
            username = localSettings.Containers["userInfo"].Values["userName"].ToString();
            double t = (double)localSettings.Containers["userInfo"].Values["timeSpent"];
            time = String.Format("{0:0}", t);
            await populateSolvedData();
        }

        private class gotdata
        {
            public string PPsolved { get; set; }
            public int Points { get; set; }
        }

        private async Task populateSolvedData()
        {
            progressbar.Text = "Fetching data";
            progressbar.ShowAsync();

            var client = new HttpClient();
            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/users/getuserdata?Username=" + System.Uri.EscapeUriString(username)));
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                List<gotdata> udata = JsonConvert.DeserializeObject<List<gotdata>>(result);
                List<int> values = new List<int>();
                int count = -1;
                foreach (string value in udata[0].PPsolved.Split(','))
                {
                    count = count + 1;
                    if (value != "")
                    {
                        values.Add(Convert.ToInt32(value));
                        Debug.WriteLine("Adding " + value + " to list");
                    }
                }
                string serialized = JsonConvert.SerializeObject(values);
                localSettings.Containers["userInfo"].Values["PPsolved"] = serialized;
                localSettings.Containers["userInfo"].Values["Points"] = udata[0].Points;

                Debug.WriteLine("Points: " + (int)localSettings.Containers["userInfo"].Values["Points"]);

                var ubox = FindChildControl<TextBlock>(LayoutRoot, "username_box") as TextBlock;
                var pbox = FindChildControl<TextBlock>(LayoutRoot, "points_box") as TextBlock;
                var sbox = FindChildControl<TextBlock>(LayoutRoot, "pp_box") as TextBlock;
                var tbox = FindChildControl<TextBlock>(LayoutRoot, "time_box") as TextBlock;
                ubox.Text = username;
                pbox.Text = "Points: " + udata[0].Points.ToString();
                tbox.Text = "Time spent coding: " + time;
                sbox.Text = "Practice problems solved: " + count.ToString();
            }
            catch
            {
                Debug.WriteLine("error");
            }
            progressbar.HideAsync();
        }

    }
}
