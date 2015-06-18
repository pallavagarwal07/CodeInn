using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class ReadTips
    {
        DatabaseTip Db_Helper = new DatabaseTip();
        public ObservableCollection<Tips> GetAllTips()
        {
            return Db_Helper.ReadTips();
        }
    }
}
