using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInn.Model
{
    public class CodeEditorContext
    {
        public ParentClass displayedObject;
        public string tableName;

        public CodeEditorContext(ParentClass objectName, string table)
        {
            displayedObject = objectName;
            tableName = table;
        }
    }
}
