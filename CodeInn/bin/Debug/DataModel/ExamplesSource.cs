using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace CodeInn.Data
{
    public class ExamplesItem
    {
        public ExamplesItem(String uniqueId, String title, String subtitle, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ExamplesGroup
    {
        public ExamplesGroup(String uniqueId, String title, String subtitle, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.Items = new ObservableCollection<ExamplesItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public ObservableCollection<ExamplesItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// ExamplesSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class ExamplesSource
    {
        private static ExamplesSource _ExamplesSource = new ExamplesSource();

        private ObservableCollection<ExamplesGroup> _groups = new ObservableCollection<ExamplesGroup>();
        public ObservableCollection<ExamplesGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<ExamplesGroup>> GetGroupsAsync()
        {
            await _ExamplesSource.GetExamplesAsync();

            return _ExamplesSource.Groups;
        }

        public static async Task<ExamplesGroup> GetGroupAsync(string uniqueId)
        {
            await _ExamplesSource.GetExamplesAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _ExamplesSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<ExamplesItem> GetItemAsync(string uniqueId)
        {
            await _ExamplesSource.GetExamplesAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _ExamplesSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetExamplesAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/Examples.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                ExamplesGroup group = new ExamplesGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new ExamplesItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Groups.Add(group);
            }
        }
    }
}