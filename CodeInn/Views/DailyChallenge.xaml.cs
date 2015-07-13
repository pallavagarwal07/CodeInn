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

namespace CodeInn.Views
{
    /// <summary>
    /// The menu page to retrieve and display Examples
    /// </summary>
    public sealed partial class DailyChallenge : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private StatusBarProgressIndicator progressbar;
        private bool isTimeRunning = false;
        ObservableCollection<Examples> DB_ExampleList = new ObservableCollection<Examples>();
        public DailyChallenge()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            if (!isTimeRunning)
            {
                this.WriteTime();
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
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DailyChallenge clickedExample = (DailyChallenge)(sender as ListBox).SelectedItem;
            //var navCont = new CodeEditorContext(clickedExample, "Examples");
            //Frame.Navigate(typeof(CodeEditor), navCont);
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
                await Task.Delay(50);
            }
            //AutoSubmit();
        }

        private void Refresh_Challenge(object sender, RoutedEventArgs e)
        {
            GetDataFromWeb();
        }

    }
}
