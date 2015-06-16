using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class ReadLessons
    {
        DatabaseLesson Db_Helper = new DatabaseLesson();
        public ObservableCollection<Lessons> GetAllLessons()
        {
            return Db_Helper.ReadLessons();
        }
    }
}
