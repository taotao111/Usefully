using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Code.External.Engine.Sqlite
{
    public static partial class Extensions
    {
        public static string ToStringOrEmpty(this object source)
        {
            string resullt;

            if (source != null)
            {
                resullt = source.ToString();
                if (resullt == null)
                    resullt = string.Empty;
            }
            else
                resullt = string.Empty;


            return resullt;
        }
        public static string ToStringOrDefault(this object source)
        {
            if (source == null)
                return null;
            return source.ToString();
        }
        public static string ToStringOrDefault(this object source, string defaultValue)
        {
            string result = ToStringOrDefault(source);

            if (string.IsNullOrEmpty(result))
                result = defaultValue;

            return result;
        }
        /// <summary>
        /// IOS
        /// </summary>        　
        public static void SetValueUnity(this PropertyInfo source, object obj, object value, object[] index)
        {
            source.GetSetMethod(true).Invoke(obj, new object[] { value });
        }
        /// <summary>
        /// IOS
        /// </summary>         
        public static object GetValueUnity(this PropertyInfo source, object obj, object[] index)
        {
            object value; 
            value = source.GetGetMethod(true).Invoke(obj, null); 
            return value;
        }
    }
}
