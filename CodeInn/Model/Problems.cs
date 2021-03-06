﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Model
{
    public class Problems : ParentClass
    {
        public string CreationDate { get; set; }
        public string Author { get; set; }
        public Problems()
        {
            //empty constructor
        }
        public Problems(int id, string name, string description, string content, string author)
        {
            Id = id;
            Name = name;
            Description = description;
            Content = content;
            CreationDate = DateTime.Now.ToString();
            Author = author;
        }
    }
}
