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
    public class DatabaseExample
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
        public Examples ReadExamples(int id)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existing = dbConn.Query<Examples>("select * from Examples where Id =" + id).FirstOrDefault();
                return existing;
            }
        }

        // Retrieve the all contact list from the database. 
        public ObservableCollection<Examples> ReadExamples()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<Examples> myCollection = dbConn.Table<Examples>().ToList<Examples>();
                ObservableCollection<Examples> ExamplesList = new ObservableCollection<Examples>(myCollection);
                return ExamplesList;
            }
        }


        //--------------------CREATE--------------------//
        public void InsertExample(Examples newobj)
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
