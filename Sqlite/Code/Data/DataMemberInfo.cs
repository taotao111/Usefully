using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Code.External.Engine.Sqlite.Data
{

    public class DataMemberDescription
    {
        internal Attribute classAttr;

        internal DataMemberInfo[] members;

        public Attribute ClassAttribute
        {
            get { return classAttr; }
            set { classAttr = value; }
        }

        public DataMemberInfo[] Members
        {
            get { return members; }
            set { members = value; }
        }


    }

    public class DataMemberInfo
    {
        private string memberName;
        public FieldInfo fInfo;
        public PropertyInfo pInfo;
        private Type memberType;
        private Type converterType;
        private object converterParameter;

     

        private Attribute attr;
        private string lowerMemberName;

        public string MemberName { get { return memberName; } }

        public Type MemberType { get { return memberType; } }
        public Type ConverterType { get { return converterType; } }
        public object ConverterParameter
        {
            get { return converterParameter; }
        }
        public FieldInfo Field { get { return fInfo; } }

        public PropertyInfo Property { get { return pInfo; } }

        public Attribute MemberAttribute { get { return attr; } }

        public string LowerMemberName { get { return lowerMemberName; } }


        static Dictionary<Type, Dictionary<Type, DataMemberDescription>> cacheTypes = new Dictionary<Type, Dictionary<Type, DataMemberDescription>>();


        public static DataMemberDescription GetDataMembers(Type objType, Type classAttrType, Type memberAttrType, Type ignoreAttrType)
        {

            DataMemberDescription result;
            Dictionary<Type, DataMemberDescription> cacheMembers;
            if (cacheTypes.TryGetValue(objType, out cacheMembers))
            {
                if (cacheMembers.TryGetValue(memberAttrType, out result))
                    return result;
            }

            var classAttr = (Attribute)objType.GetCustomAttributes(classAttrType, true).FirstOrDefault();

            result = new DataMemberDescription();
            result.classAttr = classAttr;


            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.SetProperty;

            MemberInfo[] members = objType.GetMembers(bindingFlags);


            List<DataMemberInfo> dataMembers = new List<DataMemberInfo>();
            DataMemberAttribute mAttr;
            DataMemberInfo dataMember;
            foreach (MemberInfo mInfo in members)
            {

                if (!(mInfo.MemberType == MemberTypes.Property || mInfo.MemberType == MemberTypes.Field))
                    continue;

                //自动生成属性
                if (mInfo.MemberType == MemberTypes.Field && mInfo.Name.StartsWith("<"))
                    continue;

                //忽略属性
                if (ignoreAttrType != null && mInfo.GetCustomAttributes(ignoreAttrType, false).Length > 0)
                    continue;



                DataMemberAttribute[] mAttrs = (DataMemberAttribute[])mInfo.GetCustomAttributes(memberAttrType, false);


                if (mAttrs.HasElement())
                {
                    mAttr = mAttrs[0];
                    if (mAttr.Ignore)
                        continue;
                }
                else
                    mAttr = null;


                dataMember = new DataMemberInfo();



                FieldInfo fInfo = mInfo as FieldInfo;
                if (fInfo != null)
                {
                    dataMember.fInfo = fInfo;
                    dataMember.memberName = fInfo.Name;
                    dataMember.memberType = fInfo.FieldType;
                }
                else
                {
                    PropertyInfo pInfo = mInfo as PropertyInfo;
                    dataMember.pInfo = pInfo;
                    dataMember.memberName = pInfo.Name;
                    dataMember.memberType = pInfo.PropertyType;
                }


                if (mAttr != null)
                {
                    if (!string.IsNullOrEmpty(mAttr.Name))
                    {
                        dataMember.memberName = mAttr.Name;
                    }

                    if (mAttr.ConverterType != null)
                    {
                        dataMember.converterType = mAttr.ConverterType;
                        dataMember.converterParameter = mAttr.ConverterParameter;
                    }
                    dataMember.attr = mAttr;
                }

                dataMember.lowerMemberName = dataMember.MemberName.ToStringOrEmpty().ToLower();

                dataMembers.Add(dataMember);

            }

            result.members = dataMembers.ToArray();

            if (!cacheTypes.ContainsKey(objType))
                cacheTypes[objType] = new Dictionary<Type, DataMemberDescription>();

            cacheTypes[objType][memberAttrType] = result;

            return result;
        }



    }

}
