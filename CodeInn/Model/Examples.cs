using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Model
{
    public class Examples : ParentClass
    {
        public string CreationDate { get; set; }
        public Examples()
        {
            //empty constructor
        }
        public Examples(string name, string description, string content)
        {
            Name = name;
            Description = description;
            Content = content;
            CreationDate = DateTime.Now.ToString();
        }
    }
}
