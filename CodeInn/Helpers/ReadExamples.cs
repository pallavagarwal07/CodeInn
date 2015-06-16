using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class ReadExamples
    {
        DatabaseExample Db_Helper = new DatabaseExample();
        public ObservableCollection<Examples> GetAllLessons()
        {
            return Db_Helper.ReadExamples();
        }
    }
}
