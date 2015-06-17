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
    public class DatabaseProblem
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
                        dbConn.CreateTable<Problems>();
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
        public Problems ReadProblem(int problemid)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingproblem = dbConn.Query<Problems>("select * from Problems where Id =" + problemid).FirstOrDefault();
                return existingproblem;
            }
        }

        // Retrieve the all problem list from the database. 
        public ObservableCollection<Problems> ReadProblems()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<Problems> myCollection = dbConn.Table<Problems>().ToList<Problems>();
                ObservableCollection<Problems> ProblemsList = new ObservableCollection<Problems>(myCollection);
                return ProblemsList;
            }
        }

        //--------------------CREATE--------------------//
        public void InsertProblem(Problems newobj)
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
