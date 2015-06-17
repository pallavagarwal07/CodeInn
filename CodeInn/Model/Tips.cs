﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Model
{
    public class Tips
    {
        //The Id property is marked as the Primary Key
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string CreationDate { get; set; }
        public Tips()
        {
            //empty constructor
        }
        public Tips(string name, string description, string content)
        {
            Name = name;
            Description = description;
            Content = content;
            CreationDate = DateTime.Now.ToString();
        }
    }
}
