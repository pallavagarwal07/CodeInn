using SQLite;
using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Helpers
{
    public class DatabaseTip
    {
        SQLiteConnection dbConn;

        //Create Tabble 
        public async Task<bool> onCreate(string DB_PATH)
        {
            try
            {
                if (!CheckFileExists(DB_PATH).Result)
                {
                    using (dbConn = new SQLiteConnection(DB_PATH))
                    {
                        dbConn.CreateTable<Examples>();
                        dbConn.CreateTable<Tips>();
                        dbConn.CreateTable<Problems>();
                        dbConn.CreateTable<Lessons>();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //--------------------READ---------------------//
        public Tips ReadTip(int tipid)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingtip = dbConn.Query<Tips>("select * from Tips where Id =" + tipid).FirstOrDefault();
                return existingtip;
            }
        }

        // Retrieve the all tip list from the database. 
        public ObservableCollection<Tips> ReadTips()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<Tips> myCollection = dbConn.Table<Tips>().ToList<Tips>();
                ObservableCollection<Tips> TipsList = new ObservableCollection<Tips>(myCollection);
                return TipsList;
            }
        }

        //--------------------CREATE--------------------//
        public void InsertTip(Tips newobj)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Insert(newobj);
                });
            }
        }
    }
}
