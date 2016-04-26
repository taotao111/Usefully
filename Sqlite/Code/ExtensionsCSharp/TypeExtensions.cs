using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Code.External.Engine.Sqlite
{
    public static partial class Extensions
    {
        public static MethodInfo GetArraySetValueMethod(this Type type)
        {
            return type.GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
        }
        public static MethodInfo GetArrayGetValueMethod(this Type type)
        {
            return type.GetMethod("GetValue", new Type[] { typeof(int) });
        }
    }
}
