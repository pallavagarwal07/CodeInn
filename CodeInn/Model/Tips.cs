using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Model
{
    public class Tips : ParentClass
    {
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
