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
using Windows.Storage;

namespace CodeInn.Views
{
    /// <summary>
    /// The menu page to display Lesson data
    /// </summary>
    public sealed partial class CodeDisplay : Page
    {
        Lessons navParam;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        ObservableCollection<Lessons> DB_LessonList = new ObservableCollection<Lessons>();

        async void createHtmlFileInLocalState()
        {
            StorageFolder stateFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("NavigateToState", CreationCollisionOption.OpenIfExists);
            StorageFile lesson = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\codeView.html");
            StorageFile highlight = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\highlight.pack.js");
            StorageFile style = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\arta.css");
            StorageFile marked = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\marked.min.js");

            await lesson.CopyAsync(stateFolder, "lesson.html", NameCollisionOption.ReplaceExisting);
            await highlight.CopyAsync(stateFolder, "highlight.pack.js", NameCollisionOption.ReplaceExisting);
            await style.CopyAsync(stateFolder, "arta.css", NameCollisionOption.ReplaceExisting);
            await marked.CopyAsync(stateFolder, "marked.min.js", NameCollisionOption.ReplaceExisting);
            string url = "ms-appx-web:///html/codeView.html";
            webView4.Navigate(new Uri(url));
        }

        public CodeDisplay()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
            navParam = e.Parameter as Lessons;
            createHtmlFileInLocalState();
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void webView4_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var lis = new List<string>();
            lis.Add(navParam.Content);
            var returnstatus = await webView4.InvokeScriptAsync("setText", lis);
            Debug.WriteLine(returnstatus);
        }
    }
}