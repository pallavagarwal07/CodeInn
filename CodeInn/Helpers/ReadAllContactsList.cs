using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class ReadAllContactsList
    {
        DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
        public ObservableCollection<Contacts> GetAllContacts()
        {
            return Db_Helper.ReadContacts();
        }
    }
}
