using CodeInn.Common;
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
        private TextBlock pname;
        private TextBlock pdesc;
        private WebView webv;
        private TextBox inpbox;
        private TextBlock outbox;

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
        }

        // Copies the file "html\html_example2.html" from this package's installed location to 
        // a new file "NavigateToState\test.html" in the local state folder.  When this is 
        // done, enables the 'Load HTML' button. 
        async void populateContent()
        {
            string navContext = Base64Decode(displayedObject.Content);
            navC = navContext;
            StorageFolder stateFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("NavigateToState", CreationCollisionOption.OpenIfExists);
            StorageFile htmlFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("html\\test.html");
            StorageFolder htmlFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("html\\ace");

            await htmlFile.CopyAsync(stateFolder, "test.html", NameCollisionOption.ReplaceExisting);
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

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
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
            displayedObject = e.Parameter as ParentClass;
            populateContent();
            this.navigationHelper.OnNavigatedTo(e);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        async void webView1_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string[] _params = new string[] {navC};
            await sender.InvokeScriptAsync("hello", _params);
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
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
            CodeHub.ScrollToSection(HubInOut);
        }

        private async void Compile(object sender, RoutedEventArgs e)
        {
            var edContent = await webv.InvokeScriptAsync("getContent", new List<string>());
            var bytes = Encoding.UTF8.GetBytes(edContent);
            var base64 = System.Uri.EscapeUriString(Convert.ToBase64String(bytes)).Replace("+","%2B").Replace("=","%3D");
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
            CodeHub.ScrollToSection(HubInOut);
        }
    }
}
