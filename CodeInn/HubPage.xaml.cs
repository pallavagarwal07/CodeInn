using CodeInn.Common;
using CodeInn.Data;
using CodeInn.Model;
using CodeInn.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace CodeInn
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var MainMenuGroups = await MainMenuSource.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = MainMenuGroups;
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
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            //var groupId = ((MainMenuGroup)e.ClickedItem).UniqueId;
            //if (!Frame.Navigate(typeof(SectionPage), groupId))
            //{
            //    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            //}
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((MainMenuItem)e.ClickedItem).UniqueId;
            if (itemId == "Lessons" && !Frame.Navigate(typeof(LessonViewer), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            else if (itemId == "Examples" && !Frame.Navigate(typeof(ExampleViewer), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            else if (itemId == "Tips" && !Frame.Navigate(typeof(TipViewer), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            else if (itemId == "Scratchpad" && !Frame.Navigate(typeof(CodeEditor), new Problems(0, "Scratchpad", "Code and run anything you want. Go ahead, try new things.", "I2luY2x1ZGUgPGlvc3RyZWFtPg0KDQp1c2luZyBuYW1lc3BhY2Ugc3RkOw0KDQppbnQgbWFpbigpIHsNCg0KCS8vIFR5cGUgeW91ciBjb2RlIGhlcmUNCg0KCXJldHVybiAwOw0KfQ0K", "Admin")))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            else if (itemId == "Problems" && !Frame.Navigate(typeof(ProblemViewer), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            else if (itemId == "Progress" && !Frame.Navigate(typeof(LessonViewer2), new Lessons("Basics", "Intended to teach the basics of a C++ program.", "IyBDKysNCkMrKyBpcyBhIHN0YXRpY2FsbHkgdHlwZWQsIGNvbXBpbGVkLCBnZW5lcmFsLXB1cnBvc2UsIGNhc2Utc2Vuc2l0aXZlIHByb2dyYW1taW5nIGxhbmd1YWdlIHRoYXQgc3VwcG9ydHMgcHJvY2VkdXJhbCwgb2JqZWN0LW9yaWVudGVkLCBhbmQgZ2VuZXJpYyBwcm9ncmFtbWluZy4gQysrIGZ1bGx5IHN1cHBvcnRzIHRoZSBmb3VyIHBpbGxhcnMgb2Ygb2JqZWN0LW9yaWVudGVkIGRldmVsb3BtZW50Og0KICAtIEVuY2Fwc3VsYXRpb24NCiAgLSBEYXRhIGhpZGluZw0KICAtIEluaGVyaXRhbmNlDQogIC0gUG9seW1vcnBoaXNtDQoNCiMjIyMgQysrIFByb2dyYW0gU3RydWN0dXJlDQpIZXJlIGlzIGEgY29kZSB3aGljaCB3aWxsIGhlbHAgeW91IHVuZGVyc3RhbmQgdGhlIGVsZW1lbnRzIG9mIGEgQysrIGNvZGUNCmBgYGNwcA0KI2luY2x1ZGUgPGlvc3RyZWFtPg0KdXNpbmcgbmFtZXNwYWNlIHN0ZDsNCg0KaW50IG1haW4oKSAvL3Byb2dyYW0gZXhlY3V0aW9uIGJlZ2lucyBoZXJlDQp7DQogICAgY291dCA8PCAiSGVsbG8gV29ybGQ7IC8vcHJpbnRzICJIZWxsbyBXb3JsZFwhIg0KICAgIHJldHVybiAwOw0KfQ0KYGBgDQo8YnIgLz4NClRoZSBhYm92ZSBjb2RlIGhhcyB0aGUgZm9sbG93aW5nIGNvbXBvbmVudHM6DQogIC0gYCNpbmNsdWRlYCBzdGF0ZW1lbnRzOiB0byBpbmNsdWRlIG5lY2Vzc2FyeSBsaWJyYXJpZXMgcmVxdWlyZWQgYnkgdGhlIHByb2dyYW07IGluIHRoaXMgY2FzZSwgImlvc3RyZWFtIi4gT3RoZXIgY29tbW9ubHkgdXNlZCBsaWJyYXJpZXMgaW5jbHVkZSAibWF0aC5oIiwgImFsZ29yaXRobXMuaCIsIGV0Yy4NCiAgLSBgdXNpbmcgbmFtZXNwYWNlIHN0ZGA6IHNwZWNpZmllcyB0aGUgY29tcGlsZXIgdG8gdXNlIHRoZSBzdGFuZGFyZCBuYW1lc3BhY2UuIFlvdSB3aWxsIGxlYXJuIGFib3V0IG5hbWVzcGFjZXMgbGF0ZXIgaW4gZGV0YWlsLg0KICAtIGBpbnQgbWFpbigpYDogaXQgaXMgYSBmdW5jdGlvbiwgZnJvbSB3aGljaCB0aGUgZXhlY3V0aW9uIG9mIGEgQysrIHByb2dyYW0gc3RhcnRzLiBpbnQgaXMgdGhlIHJldHVybiB0eXBlLCBhbmQgbWFpbiBpcyB0aGUgZnVuY3Rpb24ncyBuYW1lLiBBZGRpbmcgJygpJyBzcGVjaWZpZXMgdGhhdCB0aGF0IHNvbWUgbmFtZSBpcyB0byBiZSB0cmVhdGVkIGxpa2UgYSBmdW5jdGlvbi4gWW91IHdpbGwgbGVhcm4gYWJvdXQgZnVuY3Rpb25zIGluIHRoZSBuZXh0IGZldyBsZXNzb25zDQogIC0gYGNvdXQgPDwgIkhlbGxvIFdvcmxkXCEiYDogY291dCBzdGFuZHMgZm9yIGNvbnNvbGUgb3V0cHV0LiBUaGlzIHN0YXRlbWVudCBwcmludHMgdGhlIHZhbHVlKGluIHRoaXMgY2FzZSwgIkhlbGxvIFdvcmxkISIpIHRvIHRoZSBjb25zb2xlLg0KICAtIGByZXR1cm4gMGA6IHNwZWNpZmllcyB0aGUgcmV0dXJuIHZhdWUgb2YgdGhlIGZ1bmN0aW9uLiBTaW5jZSB0aGUgcmV0dXJuIHR5cGUgb2YgdGhlIGZ1bmN0aW9uIHdhcyAnaW50JywgYW4gaW50ZWdlciB2YWx1ZSBoYWQgdG8gYmUgcmV0dXJuZWQNCg0KUmVtYXJrczoNCiAgLSBBbGwgQysrIHN0YXRlbWVudHMgbXVzdCBiZSB0ZXJtaW5hdGVkIHVzaW5nIGEgc2VtaWNvbG9uICg7KQ0KICAtIENvbW1lbnRzIGFyZSB0aG9zZSBzdGF0ZW1lbnRzIHdoaWNoIGFyZSBpZ25vcmVkIGJ5IHRoZSBjb21waWxlci4gVGhleSBhcmUgdXNlZCB0byBpbmNyZWFzZSB0aGUgcmVhZGFiaWxpdHkgb2YgdGhlIHByb2dyYW0uIFNpbmdsZSBsaW5lIGNvbW1lbnRzIHN0YXJ0IHdpdGggIi8vIg0KDQojIyMjIyBFeGVjdWl0bmcgdGhlIHByb2dyYW0NClRvIHJ1biB0aGUgYWJvdmUgY29kZQ0KICAtIENvcHkgaXQgaW4gYSB0ZXh0IGVkaXRvcg0KICAtIFNhdmUgdGhlIHByb2dyYW0gYXMgIm15X2ZpcnN0X2NvZGUuY3BwIg0KICAtIE9wZW4gdGhlIGNvbW1hbmQgcHJvbXB0IC8gdGVybWluYWwsIGdvIHRvIHRoZSBkaXJlY3Rvcnkgd2hlcmUgeW91IGhhdmUgc2F2ZWQgdGhlIGZpbGUgdHlwZSB0aGUgZm9sbG93aW5nIGNvbW1hbmQgImcrKyBteV9maXJzdF9jb2RlLmNwcCIuIFRoaXMgd2lsbCBjb21waWxlIHlvdXIgY29kZSwgYW5kIGdpdmUgb3V0IGNvbXBpbGF0aW9uIGVycm9ycywgaWYgYW55LiBBbiBleGVjdXRhYmxlIGZpbGUgbmFtZWQgImEub3V0IiBoYXMgbm90IGJlZW4gY3JlYXRlZC4NCiAgLSBSdW4gdGhlIGZpbGUgImEub3V0IiB1c2luZyB0aGUgZm9sbG93aW5nIGNvbW1hbmQgIi4vYS5vdXQiDQoNCllvdSB3aWxsIHNlZSB0aGUgb3V0cHV0ICJIZWxsbyBXb3JsZCEiIG9uIHRoZSBzY3JlZW4uDQo=")))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void GoTo_Settings(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Settings), "Main"))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void GoTo_Help(object sender, RoutedEventArgs e)
        {

        }

        private void GoTo_Credits(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Credits)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void Contribute(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Contribute), "Main"))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
