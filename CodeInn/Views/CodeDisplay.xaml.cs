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
        ParentClass navParam;
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
            navParam = e.Parameter as ParentClass;
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

        void webView4_ScriptNotify(object sender, NotifyEventArgs e)
        {
        }

        private class SolutionClass
        {
            string solution { get; set; }
        }

        private async void viewsolution(object sender, RoutedEventArgs e)
        {
            var lis = new List<string>();

            var messageDialog = new MessageDialog("Remember that you would lose 8 points if you view this solution (unless the question has already been solved correctly by you).");

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(
                "Continue",
                new UICommandInvokedHandler(this.CommandInvokedHandler_none)));
            messageDialog.Commands.Add(new UICommand(
                "Close"));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler_none(IUICommand command)
        {
            displaySolution();
        }

        private async void displaySolution()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var username = localSettings.Containers["userInfo"].Values["userName"].ToString();
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/query/solution?username=" + System.Uri.EscapeUriString(username) + "&Id=" + navParam.Id));
            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(result);

            try
            {
                List<SolutionClass> udata = JsonConvert.DeserializeObject<List<SolutionClass>>(result);
            }
            catch
            {
                Debug.WriteLine("error");
            }

            List<string> lis = new List<string>();
            lis.Add(navParam.Content);
            var returnstatus = await webView4.InvokeScriptAsync("setText", lis);
            Debug.WriteLine(returnstatus);
        }
    }
}