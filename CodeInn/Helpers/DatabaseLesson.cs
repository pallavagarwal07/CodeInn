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
    public class DatabaseLesson
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
        public Lessons ReadLesson(int lessonid)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existinglesson = dbConn.Query<Lessons>("select * from Lessons where Id =" + lessonid).FirstOrDefault();
                return existinglesson;
            }
        }

        // Retrieve the all lesson list from the database. 
        public ObservableCollection<Lessons> ReadLessons()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<Lessons> myCollection = dbConn.Table<Lessons>().ToList<Lessons>();
                ObservableCollection<Lessons> LessonsList = new ObservableCollection<Lessons>(myCollection);
                return LessonsList;
            }
        }

        //--------------------CREATE--------------------//
        public void InsertLesson(Lessons newobj)
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
