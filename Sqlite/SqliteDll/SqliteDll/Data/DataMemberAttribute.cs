using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Code.External.Engine.Sqlite.Data
{

    public class DataMemberAttribute : Attribute
    {

        public string Name { get; set; }
        public Type ConverterType { get; set; }
        public object ConverterParameter { get; set; }

        public bool Ignore { get; set; }

        public DataMemberAttribute()
        {

        }

        public DataMemberAttribute(string name)
        {
            this.Name = name;
        }

    }

}
