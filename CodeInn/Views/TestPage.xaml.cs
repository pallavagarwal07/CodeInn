using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CodeInn
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ReadDataFromWeb();
        }

        public class Example
        {
            public string id { get; set; }
            public string category { get; set; }
            public string text { get; set; }
            public string tags { get; set; }
        }

        async private void ReadDataFromWeb()
        {
            var client = new HttpClient(); // Add: using System.Net.Http;
            var response = await client.GetAsync(new Uri("http://117.197.52.66:8888/query.php?table=Questions&Category=easy"));
            var result = await response.Content.ReadAsStringAsync();
            result = result.Replace("\"", string.Empty);
            
            test.Text = result;
            List<Example> example = JsonConvert.DeserializeObject<List<Example>>(result);
            box.Text = example[0].text;
        }
    }
}
