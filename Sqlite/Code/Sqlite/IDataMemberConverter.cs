using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace Code.External.Engine.Sqlite
{
    public interface IConverter
    {
        bool CanConvert(Type fromType, Type toType);
        object ConvertTo(Type fromType, object value, Type toType);
    }

    public interface IDataConverter
    {
        bool CanConvert(Type fromType, Type toType);

        object ConvertToDBValue(Type valueType, object value, Type dbType);

        object ConvertToValue(object dbValue, Type valueType);
    }


    public class ArrayDataConverter : IDataConverter
    {
        private string separator = ",";

        public string Separator
        {
            get { return separator; }
            set { separator = value; }
        }

        public ArrayDataConverter()
        {

        }

        public bool CanConvert(Type fromType, Type toType)
        {

            if (fromType == typeof(string) && (toType == typeof(int[]) || toType == typeof(string[]) || toType == typeof(float[])||toType == typeof(double[]) ))
            {
                return true;
            }
            if (toType == typeof(string) && (fromType == typeof(int[]) || fromType == typeof(string[]) || fromType == typeof(float[]) || fromType == typeof(double[])))
            {
                return true;
            }

            return false;
        }

        public object ConvertToDBValue(Type valueType, object value, Type dbType)
        {
            if (value == null)
                return DBNull.Value;


            Type elemType = valueType.GetElementType();

            MethodInfo mGetValue = valueType.GetArrayGetValueMethod();

            int length = (int)valueType.GetProperty("Length").GetValueUnity(value, null);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                object item = mGetValue.Invoke(value, new object[] { i });

                string dbItemValue = SqliteDatabase.ToDBValue(elemType, item).ToStringOrEmpty();
                if (i > 0)
                    sb.Append(separator);

                sb.Append(dbItemValue);

            }
            return sb.ToString();
        }

        public object ConvertToValue(object dbValue, Type valueType)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return null;

            string str = dbValue.ToStringOrEmpty();
            if (string.IsNullOrEmpty(str))
                return null;


            string[] items = str.Split(new string[] { separator }, int.MaxValue, StringSplitOptions.None);
            int length = items.Length;
            Type itemType = valueType.GetElementType();
            object array = valueType.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { length });
            var setValue = valueType.GetArraySetValueMethod();
            for (int i = 0; i < length; i++)
            {
                object val = items[i];
                val = SqliteDatabase.ValueOfType(itemType, val);
                setValue.Invoke(array, new object[] { val, i });
            }

            return array;
        }



    }

}