using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CodeInn.Model
{
    public class ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public object Background { get; set; }

        public ListItem(int id, string name, string description, bool solved)
        {
            Id = id;
            Name = name;
            Description = description;
            if (solved)
                Background = Application.Current.Resources["TransparentBlue"] as SolidColorBrush;
            else
                Background = Application.Current.Resources["Transparenty"] as SolidColorBrush;
        }
    }
}
