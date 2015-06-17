using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class ReadProblems
    {
        DatabaseProblem Db_Helper = new DatabaseProblem();
        public ObservableCollection<Problems> GetAllProblems()
        {
            return Db_Helper.ReadProblems();
        }
    }
}
