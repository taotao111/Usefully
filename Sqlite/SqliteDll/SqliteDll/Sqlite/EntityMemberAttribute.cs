using Code.External.Engine.Sqlite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class EntityMemberAttribute : Code.External.Engine.Sqlite.Data.DataMemberAttribute
{
    public bool ID { get; set; }

    public EntityMemberAttribute(string name)
        : base(name)
    {

    }
    public EntityMemberAttribute()
    {

    }

}