﻿using CodeInn.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage; 
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using CodeInn.Model;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using Windows.UI.Popups;
using Newtonsoft.Json;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CodeInn
{
    /// <summary>
    /// CodeEditor page.
    /// </summary>
    public sealed partial class CodeEditor : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private string navC;
        private ParentClass displayedObject;
        private string tableName;
        private TextBlock pname;
        private TextBlock pdesc;
        private WebView webv;
        private TextBox inpbox;
        private TextBlock outbox;
        private StatusBarProgressIndicator progressbar;
        private Windows.Storage.ApplicationDataContainer localSettings;
        private int timeSpent;

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

        public CodeEditor()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += CodeEditor_Hiding;
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += CodeEditor_Showing;
        }

        void CodeEditor_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Debug.WriteLine("Got Focus!!!");
          //webv.Height = (490 / 2);
        }

        void CodeEditor_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Debug.WriteLine("Lost Focus!!");
        //  webv.Height = 491;
        }

        // Copies the file "html\html_example2.html" from this package's installed location to 
        // a new file "NavigateToState\test.html" in the local state folder.  When this is 
        // done, enables the 'Load HTML' button. 
        async void populateContent()
        {
            startTimeCalculation();
            string navContext = Base64Decode(displayedObject.Content) + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
            navC = navContext;
            StorageFolder stateFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("NavigateToState", CreationCollisionOption.OpenIfExists);
            StorageFile htmlFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\test.html");
            StorageFile jQuery = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\jquery.js");
            StorageFolder htmlFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("html\\ace");

            await htmlFile.CopyAsync(stateFolder, "test.html", NameCollisionOption.ReplaceExisting);
            await jQuery.CopyAsync(stateFolder, "jquery.js", NameCollisionOption.ReplaceExisting);
            await CopyFolderAsync(htmlFolder, stateFolder, "ace");
            string url = "ms-appx-web:///html/test.html";
            var codesection = HubEditor;
            webv = FindChildControl<WebView>(codesection, "webView1") as WebView;
            webv.Navigate(new Uri(url));

			pname = FindChildControl<TextBlock>(HubQuestion, "probname") as TextBlock;
			pdesc = FindChildControl<TextBlock>(HubQuestion, "probdesc") as TextBlock;
			pname.Text = displayedObject.Name;
            pdesc.Text = displayedObject.Description;

            inpbox = FindChildControl<TextBox>(HubInOut, "inpbox") as TextBox;
            outbox = FindChildControl<TextBlock>(HubInOut, "outbox") as TextBlock;
		}

        async Task CopyFolderAsync(StorageFolder source, StorageFolder destinationContainer, string desiredName = null)
        {
            StorageFolder destinationFolder = null;
            destinationFolder = await destinationContainer.CreateFolderAsync(
                desiredName ?? source.Name, CreationCollisionOption.ReplaceExisting);

            foreach (var file in await source.GetFilesAsync())
            {
                await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting);
            }
            foreach (var folder in await source.GetFoldersAsync())
            {
                await CopyFolderAsync(folder, destinationFolder);
            }
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
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
            displayedObject = (e.Parameter as CodeEditorContext).displayedObject;
            tableName = (e.Parameter as CodeEditorContext).tableName;
            populateContent();

            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (tableName == "Scratchpad" || tableName == "Examples" || tableName == "LessonExample")
            {
                CommandBar bottomCommandBar = this.BottomAppBar as CommandBar;
                AppBarButton b = bottomCommandBar.PrimaryCommands[2] as AppBarButton;
                b.Click -= Verify;
                bottomCommandBar.PrimaryCommands.RemoveAt(2);
            }

            progressbar = StatusBar.GetForCurrentView().ProgressIndicator;

            this.navigationHelper.OnNavigatedTo(e);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            displayedObject.Content = Base64Encode(edContent);
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        async void webView1_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string[] _params = new string[] {navC};
            await sender.InvokeScriptAsync("hello", _params);
        }

        async void startTimeCalculation()
        {
            while(true)
            {
                timeSpent += 5;
                if(timeSpent > 2)
                {
                    if (!localSettings.Containers.ContainsKey("userInfo"))
                    {
                        MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                        await msgbox.ShowAsync();
                        Frame.Navigate(typeof(Views.Settings));
                        return;
                    }
                    else
                    {
                        localSettings.CreateContainer("userInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);
                        double oldVal = (double)localSettings.Containers["userInfo"].Values["timeSpent"];
                        localSettings.Containers["userInfo"].Values["timeSpent"] = ((double)timeSpent) / 60 + oldVal;
                        Debug.WriteLine("Time Spent in Code updated to " + ((double)timeSpent)/60 + oldVal);
                    }
                    timeSpent = 0;
                }
                await Task.Delay(5000);
            }
        }

        private async void Compile(object sender, RoutedEventArgs e)
        {
            progressbar.Text = "Compiling code";
            progressbar.ShowAsync();

            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            var bytes = Encoding.UTF8.GetBytes(edContent);
            var base64 = System.Uri.EscapeUriString(Convert.ToBase64String(bytes)).Replace("+", "%2B").Replace("=", "%3D");
            Debug.WriteLine(base64);

            var cts = new CancellationTokenSource();
            var client = new HttpClient();

            try
            {
                cts.CancelAfter(7500);
                var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/api/compile?Content=" + base64));
                var result = await response.Content.ReadAsStringAsync();
                outbox.Text = result;
            }
            catch
            {
                outbox.Text = "Timeout";
            }
            progressbar.HideAsync();
            CodeHub.ScrollToSection(HubInOut);
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
            progressbar.Text = "Running code";
            progressbar.ShowAsync();

            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            var bytes = Encoding.UTF8.GetBytes(edContent);
            var base64 = System.Uri.EscapeUriString(Convert.ToBase64String(bytes)).Replace("+", "%2B").Replace("=", "%3D");
            Debug.WriteLine(base64);

            var cts = new CancellationTokenSource();
            var client = new HttpClient();

            try
            {
                cts.CancelAfter(7500);
                var response = await client.GetAsync(new Uri("http://codeinn-acecoders.rhcloud.com:8000/api/run?Content=" + base64 + "&Input=" + inpbox.Text));
                var result = await response.Content.ReadAsStringAsync();
                outbox.Text = result;
            }
            catch
            {
                outbox.Text = "Timeout";
            }
            progressbar.HideAsync();
            CodeHub.ScrollToSection(HubInOut);
        }

        private async void Verify(object sender, RoutedEventArgs e)
        {
            progressbar.Text = "Verifying code";
            progressbar.ShowAsync();

            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            var bytes = Encoding.UTF8.GetBytes(edContent);
            var base64 = System.Uri.EscapeUriString(Convert.ToBase64String(bytes)).Replace("+", "%2B").Replace("=", "%3D");
            Debug.WriteLine(base64);

            var cts = new CancellationTokenSource();
            var client = new HttpClient();

            if (!localSettings.Containers.ContainsKey("userInfo"))
            {
                MessageDialog msgbox = new MessageDialog("Please log-in first. Go to settings from the main menu.");
                await msgbox.ShowAsync();
                return;
            }

            var username_basic = localSettings.Containers["userInfo"].Values["userName"].ToString();
            var username = System.Uri.EscapeUriString(username_basic).Replace("+", "%2B").Replace("=", "%3D");

            var ifSolved = false;
            try
            {
                cts.CancelAfter(7500);
                var uri = new Uri("http://codeinn-acecoders.rhcloud.com:8000/api/verify?Content=" + base64 + "&Id=" + displayedObject.Id + "&Table=" + tableName + "&username=" + username);
                var response = await client.GetAsync(uri);
                var result = await response.Content.ReadAsStringAsync();
                outbox.Text = result;
                if (response.StatusCode == HttpStatusCode.OK)
                    ifSolved = true;
            }
            catch
            {
                outbox.Text = "Timeout. Try again later.";
            }

            if(ifSolved)
            {
                var serialized = localSettings.Containers["userInfo"].Values["PPsolved"].ToString();
                var listofsolved = JsonConvert.DeserializeObject<List<int>>(serialized);
                listofsolved.Add(displayedObject.Id);
                string serialized_new = JsonConvert.SerializeObject(listofsolved);
                localSettings.Containers["userInfo"].Values["PPsolved"] = serialized_new;
            }

            progressbar.HideAsync();
            CodeHub.ScrollToSection(HubInOut);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private async void viewcode(object sender, RoutedEventArgs e)
        {
            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            displayedObject.Content = Base64Encode(edContent);
            Frame.Navigate(typeof(Views.CodeDisplay), new CodeEditorContext(displayedObject, tableName));
        }


    }
}
