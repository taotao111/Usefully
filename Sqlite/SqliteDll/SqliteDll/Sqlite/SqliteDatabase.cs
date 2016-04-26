using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using Code.External.Engine.Sqlite.Data;

namespace Code.External.Engine.Sqlite
{
    public abstract class SqliteDatabase : IDisposable
    {
        private string name;
        private int version;
        private const int dbVersion = 3;
        private const string OpenDbFormat = "URI=file:{0}";
        private const string OpenDbAndVersionFormat = "URI=file:{0},version={1}";
        /// <summary>
        /// SQL连接
        /// </summary>
        private SqliteConnection conn;
        /// <summary>
        /// 当前的数据库连接
        /// </summary>
		public SqliteConnection connection
		{
			get
			{
				return conn;
			}
		}
        private bool isOpened;
        private SqliteTransaction trans;
        private int transGroup;
        private const string MetadataTableName = "_sys_metadata";
        private const string Metadata_Version = "_db_version_";
        private const string Metadata_Owner_ID = "_db_owner_id";
        private Exception lastException;
        private static List<IDataConverter> converters;
        private bool isMemoryDB;
        private bool isOpenByMemory;
        /// <summary>
        /// 是否是内存数据库，否则为文件数据库
        /// </summary>
        public bool IsMemoryDB
        {
            get { return isMemoryDB; }
        }
        private bool runInMemory;
        public bool RuntInMemory
        {
            get { return runInMemory; }
        }
        private bool changed;
        public bool debug;
        /// <summary>
        /// 标识数据库内容是否改变
        /// </summary>
        public bool Changed
        {
            get { return changed; }
        }
        /// <summary>
        /// 清除改变
        /// </summary>
        public void ClearChanged()
        {
            changed = false;
        }
        static SqliteDatabase()
        {
            if (converters == null)
            {
                converters = new List<IDataConverter>();
                converters.Add(new ArrayDataConverter());
            }
        }
        public SqliteDatabase(string dst, string src)
        {

            this.name = dst;
            InitDatabase(dst, src);

        }
        public SqliteDatabase(string name, int version)
        {
            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentNullException("name");

            switch (Application.platform)
            {
                case RuntimePlatform.OSXWebPlayer:
                case RuntimePlatform.WindowsWebPlayer:
                    throw new Exception("sqlite not support platform :" + Application.platform);


            }

            this.name = name;
            this.version = version;

            isMemoryDB = name.IndexOf(":memory") >= 0;

            if (!isMemoryDB)
            {
                InitDatabase(name, version);
            }
        }
        private SqliteDatabase()
        {
        }
        public int Version { get { return version; } }
        public string Name { get { return name; } }
        public Exception LastError
        {
            get { return lastException; }
        }
        public bool HasError
        {
            get { return lastException != null; }
        }
        private void SetLastError(Exception ex)
        {
            lastException = ex;
        }
        private void SetLastError(Exception ex, string sqlText, object[] parameters)
        {
            lastException = ex;

            string error = sqlText;

            if (lastException != null)
                error += "\n" + lastException.Message;

            if (parameters.HasElement())
            {
                error += "\n";

                for (int i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    if (i > 0)
                        error += ",";
                    error += p.ToStringOrEmpty();
                }
            }

            Debug.LogError(error);
        }
        public string OwnerID
        {
            get { return GetMetadata<string>(Metadata_Owner_ID); }
            set
            {
                string oldId = OwnerID;
                if (oldId != value)
                {
                    SetMetadata(Metadata_Owner_ID, value);
                    OnOwnerIDChanged(oldId, value);

                }
            }
        }
        protected virtual void OnOwnerIDChanged(string oldId, string ownerId)
        {

        }
        protected virtual void UpgradeDatabase(int oldVersion, int newVersion)
        {
            CreateDatabase();
        }
        protected virtual void DowngradeDatabase(int oldVersion, int newVersion)
        {
        }
        protected abstract void CreateDatabase();
        private void CopyDatabase(string srcPath, string dstPath)
        {
            if (File.Exists(dstPath))
                File.Delete(dstPath);

            File.Copy(srcPath, dstPath);
            Debug.Log("copy database:" + dstPath);
            //CreateDatabase();
        }
        int GetDatabaseVersion(string name)
        {
            int ver = -1;
            try
            {
                //using (SqliteConnection conn = new SqliteConnection(OpenDbAndVersionFormat.FormatString(name, dbVersion)))
                using (SqliteConnection conn = new SqliteConnection(OpenDbFormat.FormatString(name)))
                {
                    conn.Open();

                    ver = GetMetadata<int>(conn, Metadata_Version, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("GetDatabaseVersion open");
            }
            return ver;
        }
        void InitDatabase(string dst, string src)
        {
#if UNITY_LWJ
            Debug.Log("init database:" + dst);
#endif
            //if (File.Exists(dst))
            // {
            string cmdText;
            int currentVersion = -1;
            version = -1;
            try
            {
                Debug.Log("1");
                version = GetDatabaseVersion(src);
                Debug.Log("2");
                currentVersion = GetDatabaseVersion(dst);

                Debug.Log("3");
                if (currentVersion < version)
                {
                    CopyDatabase(src, dst);
                    UpgradeDatabase(currentVersion, version);
                }
                else if (currentVersion > version)
                {
                    DowngradeDatabase(currentVersion, version);
                }
                Debug.Log("4");
                Close();
                Debug.Log("5");
            }
            catch (Exception ex)
            {
                Debug.LogError("Sqlite create error: " + ex.Message);
                throw ex;


            }
            //  }
            //   else
            //  {

            // CopyDatabase(src, dst);

            //  }

            //Open();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetMetadata<T>(string name, T value)
        {
            SetMetadata(conn, name, value);
        }
        public void SetMetadata<T>(SqliteConnection conn, string name, T value)
        {
            using (var cmd = new SqliteCommand(conn))
            {
                string sqlText = "delete from " + MetadataTableName + " where name=@name";
                cmd.CommandText = sqlText;
                AttachParameters(cmd, new object[] { name });
                cmd.ExecuteNonQuery();

                sqlText = "INSERT INTO " + MetadataTableName + " (name, value) VALUES (@name, @value)";
                cmd.CommandText = sqlText;
                //?
                //string stringValue = value == null ? null : value.ToString();
                // AttachParameters(cmd, new object[] { name, stringValue });
                AttachParameters(cmd, new object[] { name, value });
                cmd.ExecuteNonQuery();
            }

        }
        /// <summary>
        /// 获得数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetMetadata<T>(string name)
        {

            return GetMetadata<T>(conn, name, default(T));
        }
        public T GetMetadata<T>(string name, T defaultValue)
        {
            return GetMetadata<T>(conn, name, defaultValue);
        }
        private T GetMetadata<T>(SqliteConnection conn, string name, T defaultValue)
        {
            T value;
            string sqlText = "select value from " + MetadataTableName + " where name=@name";
            using (var cmd = new SqliteCommand(sqlText, conn))
            {
                AttachParameters(cmd, new object[] { name });
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        object val = reader.GetValue(0);
                        value = ToValue<T>(reader.GetValue(0), defaultValue);
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                //value = ExecuteScalar<T>(cmd, defaultValue);
            }

            return value;
        }
        public void DeleteMetadata(SqliteConnection conn, string name)
        {
            using (var cmd = new SqliteCommand(conn))
            {
                string sqlText = "delete from " + MetadataTableName + " where name=@name";
                cmd.CommandText = sqlText;
                AttachParameters(cmd, new object[] { name });
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteAllMetadata(SqliteConnection conn)
        {
            using (var cmd = new SqliteCommand(conn))
            {
                string sqlText = "delete from " + MetadataTableName;
                cmd.CommandText = sqlText;
                cmd.ExecuteNonQuery();
            }
        }
        void InitDatabase(string name, int version)
        {
            //Debug.Log(name + " " + version);

            string cmdText;
            try
            {
                Open();
            }
            catch (Exception ex)
            {
                Debug.LogError("file open fail,file error");
                try
                {
                    // Debug.Log("delete file:" + name);
                    if (File.Exists(name))
                        File.Delete(name);
                }
                catch (Exception ex2)
                {
                    Debug.LogException(ex2);
                    throw ex2;
                }
            }
            try
            {
                Open();



                cmdText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=@name";

                if (ExecuteScalar<int>(cmdText, MetadataTableName) <= 0)
                {

                    BeginTransaction();

                    cmdText = "CREATE TABLE " + MetadataTableName + " ([name] NVARCHAR(50), [value] NVARCHAR(100))";

                    ExecuteNonQuery(cmdText);

                    DeleteAllMetadata(conn);

                    SetMetadata(Metadata_Version, version);

#if UNITY_LWJ
                    Debug.Log("create database");
#endif
                    CreateDatabase();

                    Commit();

                }
                else
                {
                    int currentVersion;

                    currentVersion = GetMetadata<int>(Metadata_Version);

                    if (currentVersion != version)
                    {
                        BeginTransaction();

                        DeleteAllMetadata(conn);

                        if (currentVersion < version)
                        {
#if UNITY_LWJ
                            Debug.Log("Upgrade database");
#endif
                            UpgradeDatabase(currentVersion, version);
                        }
                        else
                        {
#if UNITY_LWJ
                            Debug.Log("Downgrade database");
#endif
                            DowngradeDatabase(currentVersion, version);
                        }


                        SetMetadata(Metadata_Version, version);

                        Commit();
                    }

                }


                conn.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError("Sqlite create error: " + ex.Message);
                throw ex;
            }
            finally
            {
                Close();
            }


        }
        /// <summary>
        /// 运行在内存
        /// </summary>
        public void OpenFromMemory()
        {

            if (isOpened)
            {
                if (runInMemory)
                    return;

                Close();

            }

            SqliteConnection destConn;

            if (isMemoryDB)
            {
                destConn = new SqliteConnection(name);
                destConn.Open();
                BeginTransaction();

                string cmdText = "CREATE TABLE " + MetadataTableName + " ([name] NVARCHAR(50), [value] NVARCHAR(100))";

                ExecuteNonQuery(cmdText);

                SetMetadata(Metadata_Version, version);

#if UNITY_LWJ
                    Debug.Log("create database");
#endif
                CreateDatabase();

                Commit();
            }
            else
            {
                destConn = new SqliteConnection("Data Source=:memory:");
                destConn.Open();

                using (SqliteConnection sourceConn = new SqliteConnection(OpenDbFormat.FormatString(name, dbVersion)))
                {
                    sourceConn.Open();
                    BackupDatabase(sourceConn, destConn);
                }
            }
            this.conn = destConn;
            //jit
         //   conn.Update += DB_Update;
            runInMemory = true;
            isOpened = true;
        }
        public void BackupDatabase()
        {
            if (isMemoryDB)
                throw new Exception("该数据库为内存数据库，无法保存");
            if (!isOpened)
                return;
            if (!runInMemory)
                return;

            using (SqliteConnection sourceConn = new SqliteConnection(OpenDbFormat.FormatString(name, dbVersion)))
            {
                sourceConn.Open();
                BackupDatabase(conn, sourceConn);
            }

        }
        void DB_Update(object sender, UpdateEventArgs e)
        {
            changed |= true;
            // Debug.Log("Database: " + e.Database + ",Event: " + e.Event + ",RowId: " + e.RowId + ",Table: " + e.Table);
        }
        #region BackupDatabase

        //http://www.sqlite.org/c3ref/backup_finish.html 
        //http://www.sqlite.org/backup.html

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_backup_init(IntPtr dest, string destName, IntPtr source, string sourceName);


        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_step(IntPtr p, int nPage);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_finish(IntPtr p);


        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_remaining(IntPtr p);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_pagecount(IntPtr p);


        static IntPtr GetDBPtr(SqliteConnection conn)
        {
            IntPtr ptr;

            BindingFlags flags = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo fInfo = typeof(SqliteConnection).GetField("_sql", flags);

            object _sql = fInfo.GetValue(conn);
            fInfo = _sql.GetType().GetField("_sql", flags);

            _sql = fInfo.GetValue(_sql);
            fInfo = _sql.GetType().GetField("handle", flags);
            ptr = (IntPtr)fInfo.GetValue(_sql);

            return ptr;
        }

        private static void BackupDatabase(SqliteConnection source, SqliteConnection dest)
        {
            IntPtr ptrSrc = GetDBPtr(source);
            if (ptrSrc == IntPtr.Zero)
                throw new Exception("ptr zero");

            IntPtr ptrDest = GetDBPtr(dest);
            if (ptrDest == IntPtr.Zero)
                throw new Exception("ptr zero");

            IntPtr ptrBk = sqlite3_backup_init(ptrDest, "main", ptrSrc, "main");
            if (ptrBk == IntPtr.Zero)
                throw new Exception("backup error");

            sqlite3_backup_step(ptrBk, -1);
            sqlite3_backup_finish(ptrBk);
        }

        public void BackupDatabase(SqliteDatabase dest)
        {
            bool isCloseFrom = false, isCloseDest = false;
            if (!IsOpened)
            {
                Open();
                isCloseFrom = true;
            }
            if (!dest.IsOpened)
            {
                isCloseDest = true;
            }

            BackupDatabase(conn, dest.conn);

            if (isCloseFrom)
                Close();
            if (isCloseDest)
                dest.Close();

        }

        public void BackupDatabase(string backupPath)
        {
            string filePath = Name;

            if (!File.Exists(filePath))
                throw new Exception("db file not extis: " + filePath);


            Close();

            if (File.Exists(backupPath))
                File.Delete(backupPath);

            File.Copy(filePath, backupPath);

        }

        #endregion BackupDatabase
        public void Recovery(string filePath, string format)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath))
                throw new Exception("db file not exists path:" + filePath);

            byte[] data = File.ReadAllBytes(filePath);

            Recovery(data, format);

        }
        public void Recovery(byte[] file, string format)
        {
            byte[] data;
            if (!string.IsNullOrEmpty(format))
            {
                switch (format.ToLower())
                {
                    case "zip":
                        data = ZipHelper.Unzip(new MemoryStream(file)).FirstOrDefault();
                        break;
                    default:
                        throw new System.NotSupportedException("format " + format);
                }
            }
            else
            {
                data = file;
            }

            Recovery(data);
        }
        public void Recovery(byte[] file)
        {
            if (isMemoryDB)
                throw new Exception("is memory db");

            string filePath = Name;
            if (IsOpened)
                Close();

            if (File.Exists(filePath))
                File.Delete(filePath);

            File.WriteAllBytes(filePath, file);

            if (!isMemoryDB)
            {
                InitDatabase(name, version);
            }
        }
        /// <summary>
        /// 如果执行命令有异常则抛出
        /// </summary>
        public void ThrowIfError()
        {
            if (HasError)
                throw lastException;
        }
        #region MyRegion
        private const string ParamNameRegex = @"(@)([@\w]*)";

        static Dictionary<string, CommandTextRef> cachedCmdTexts = new Dictionary<string, CommandTextRef>();

        private class CommandTextRef
        {
            public string CommandText;

            /// <summary>
            /// @paramName
            /// </summary>
            public string[] rawParamNames;

            /// <summary>
            /// paramName
            /// </summary>
            public string[] paramNames;

        }

        CommandTextRef ParseCommandText(string cmdText)
        {
            CommandTextRef cmdTextRef;
            int i = 0;
            if (!cachedCmdTexts.TryGetValue(cmdText, out cmdTextRef))
            {
                if (debug)
                    Debug.Log("parse cmdText:" + cmdText);
                cmdTextRef = new CommandTextRef();
                cmdTextRef.CommandText = cmdText;

                Regex ex = new Regex(ParamNameRegex);
                MatchCollection mc = ex.Matches(cmdText);
                string[] rawParamNames = new string[mc.Count];
                string[] paramNames = new string[mc.Count];
                foreach (Match m in mc)
                {
                    string rawParamName = m.Value.Trim();
                    if (rawParamNames.Contains(rawParamName))
                        continue;

                    rawParamNames[i] = rawParamName;
                    paramNames[i] = rawParamName.Substring(1);
                    if (debug)
                        Debug.Log("param raw:{0}  name:{1}".FormatString(rawParamName, paramNames[i]));
                    // Debug.Log("name: " + "|" + m.Value + "|");
                    i++;
                }
                cmdTextRef.rawParamNames = rawParamNames;
                cmdTextRef.paramNames = paramNames;
                cachedCmdTexts[cmdText] = cmdTextRef;
            }
            return cmdTextRef;
        }

        protected object[] GetEntityParameterList(string cmdText, object entity)
        {
            if (cmdText == null)
                throw new ArgumentNullException("cmdText");


            CommandTextRef cmdTextRef = ParseCommandText(cmdText);

            if (cmdTextRef.rawParamNames.Length <= 0)
                return new object[0];


            if (entity == null)
                throw new ArgumentNullException("entity");

            Type objType = entity.GetType();

            if (debug)
                Debug.Log("attach param by entity, type:{0}".FormatString(objType));
            DataMemberInfo[] members = GetMembers(objType);



            object[] paramList = new object[cmdTextRef.rawParamNames.Length];


            DataMemberInfo member;
            object value;
            int i = 0;
            foreach (var paramName in cmdTextRef.paramNames)
            {

                member = null;
                for (int j = 0; j < members.Length; j++)
                {
                    if (members[j].LowerMemberName == paramName.ToLowerInvariant())
                    {
                        member = members[j];
                        break;
                    }
                }

                if (member == null)
                    throw new Exception("not find parameter member: {0} ,type: {1} ".FormatString(paramName, objType));

                if (member.Property != null)
                {
                    value = member.Property.GetValueUnity(entity, null);
                }
                else if (member.Field != null)
                {
                    value = member.Field.GetValue(entity);
                }
                else
                {
                    throw new Exception("parameter member {0}, type: {1}, property and field null".FormatString(paramName, objType));
                }

                if (debug)
                    Debug.Log("param index:{0} name:{1}, value:{2}".FormatString(i, paramName, value));

                paramList[i] = value;
                i++;
            }

            return paramList;
        }

        //protected void AttachParametersByEntity(SqliteCommand cmd, object entity)
        //{
        //    if (cmd == null)
        //        throw new ArgumentNullException("cmd");
        //    if (entity == null)
        //        throw new ArgumentNullException("entity");

        //    Type objType = entity.GetType();

        //    if (debug)
        //        Debug.Log("attach param by entity, type:{0}".FormatString(objType));
        //    DataMemberInfo[] members = DataMemberInfo.GetDataMembers(objType);

        //    string cmdText = cmd.CommandText;

        //    CommandTextRef cmdTextRef = ParseCommandText(cmdText);

        //    SqliteParameterCollection coll = cmd.Parameters;
        //    coll.Clear();

        //    SqliteParameter param;
        //    DataMemberInfo member;
        //    object value;
        //    int i = 0;
        //    foreach (var paramName in cmdTextRef.paramNames)
        //    {
        //        param = new SqliteParameter();
        //        param.ParameterName = cmdTextRef.rawParamNames[i];

        //        member = null;
        //        for (int j = 0; j < members.Length; j++)
        //        {
        //            if (members[j].LowerMemberName == paramName.ToLowerInvariant())
        //            {
        //                member = members[j];
        //                break;
        //            }
        //        }

        //        if (member == null)
        //            throw new Exception("not find parameter member: {0} ,type: {1} ".FormatString(paramName, objType));

        //        if (member.Property != null)
        //        {
        //            value = member.Property.GetValueUnity(entity, null);
        //        }
        //        else if (member.Field != null)
        //        {
        //            value = member.Field.GetValue(entity);
        //        }
        //        else
        //        {
        //            throw new Exception("parameter member {0}, type: {1}, property and field null".FormatString(paramName, objType));
        //        }
        //        param.Value = ToDBValue(value);
        //        if (debug)
        //            Debug.Log("param index:{0} name:{1}, value:{2}".FormatString(i, param.ParameterName, param.Value));
        //        coll.Add(param);
        //        i++;
        //    }

        //}

        protected SqliteParameterCollection AttachParameters(SqliteCommand cmd, object[] paramList)
        {
            if (paramList == null || paramList.Length == 0) return null;

            SqliteParameterCollection coll = cmd.Parameters;
            coll.Clear();
            string cmdText = cmd.CommandText;


            CommandTextRef cmdTextRef = ParseCommandText(cmdText);
            int i = 0;


            i = 0;
            Type valueType = null;
            foreach (object value in paramList)
            {

                if (value == null)
                {
                    valueType = null;
                }
                else
                {
                    valueType = value.GetType();
                }


                SqliteParameter parm = new SqliteParameter();

                parm.ParameterName = cmdTextRef.rawParamNames[i];


                parm.Value = ToDBValue(value);



                //Debug.Log(parm.ParameterName + "," + value);
                //switch (valueType.ToString())
                //{

                //    case ("DBNull"):
                //    case ("Char"):
                //    case ("SByte"):
                //    case ("UInt16"):
                //    case ("UInt32"):
                //    case ("UInt64"):
                //        throw new SystemException("Invalid data type");


                //    case ("System.String"):
                //        // parm.DbType =  DbType.String;

                //        //(string)paramList[j];

                //        break;

                //    case ("System.Byte[]"):
                //        //   parm.DbType = DbType.Binary;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = (byte[])paramList[j];

                //        break;

                //    case ("System.Int32"):
                //        //  parm.DbType = DbType.Int32;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = value;// (int)paramList[j];

                //        break;

                //    case ("System.Boolean"):
                //        //  parm.DbType = DbType.Boolean;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = (bool)paramList[j];

                //        break;

                //    case ("System.DateTime"):
                //        //  parm.DbType = DbType.DateTime;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = Convert.ToDateTime(paramList[j]);

                //        break;

                //    case ("System.Double"):
                //        // parm.DbType = DbType.Double;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = Convert.ToDouble(paramList[j]);

                //        break;

                //    case ("System.Decimal"):
                //        // parm.DbType = DbType.Decimal;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = Convert.ToDecimal(paramList[j]);
                //        break;

                //    case ("System.Guid"):
                //        //  parm.DbType = DbType.Guid;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = (System.Guid)(paramList[j]);

                //        break;

                //    case ("System.Object"):
                //        // parm.DbType = DbType.Object;
                //        //parm.ParameterName = paramNames[j];
                //        //parm.Value = paramList[j];

                //        break;

                //    default:
                //        throw new SystemException("Value is of unknown data type");

                //} // end switch
                coll.Add(parm);
                i++;
            }
            return coll;
        }

        #endregion
        public bool IsOpened
        {
            get { return isOpened; }
        }
        public void Open()
        {
            if (isOpened)
                return;
            if (this.conn == null)
            {

                string connStr = null;
#if UNITY_IPHONE1
            SqliteConnectionStringBuilder scsb=new SqliteConnectionStringBuilder();
            //scsb.DataSource=name;
            scsb.Uri=name; 
            connStr=scsb.ToString();
#else
                //connStr = OpenDbAndVersionFormat.FormatString(name, dbVersion);
                connStr = OpenDbFormat.FormatString(name, dbVersion);

#endif
                //Debug.Log("local db connect string:" + connStr);
                SqliteConnection conn = new SqliteConnection(connStr);
                // conn.StateChange += StateChanged;
                //jit
               // conn.Update += DB_Update;
                conn.Open();

                this.conn = conn;
            }

#if UNITY_LWJ
            Debug.Log("Open db");
#endif
            isOpened = true;
            runInMemory = false;
        }
        public void Close()
        {
            if (!isOpened)
                return;

            if (conn != null)
            {
                if (trans != null)
                    Rollback();

                try
                {
                    conn.Close();

                }
                catch (Exception ex) { Debug.LogException(ex); }
                conn = null;

            }

#if UNITY_LWJ
            Debug.Log("Close db");
#endif
            trans = null;
            transGroup = 0;
            isOpened = false;
            runInMemory = false;
        }
        public bool BeginTransaction()
        {

            if (this.trans != null)
            {
                transGroup++;
                return false;
            }


            CheckOpen();

            //#if UNITY_IPHONE
            //		cmd=new SqliteCommand(null,conn);
            //#else

            var trans = conn.BeginTransaction();

            this.trans = trans;
            //#endif
            transGroup = 1;
            //Debug.Log(this + " BeginTransaction");
            return true;
        }
        public void Rollback()
        {
            transGroup--;

            if (transGroup > 0)
                return;

            if (transGroup < 0)
                Debug.LogError("Call Commit, not call BeginTransaction");

            CheckOpen();


            if (trans == null)
                throw new Exception("未开启事务");
            trans.Rollback();

            trans = null;
#if UNITY_LWJ
            Debug.Log(this + " Rollback");
#endif
        }
        public void Commit()
        {
            transGroup--;

            if (transGroup > 0)
                return;

            if (transGroup < 0)
                Debug.LogError("Call Commit, not call BeginTransaction");

            CheckOpen();

            if (trans == null)
                throw new Exception("未开启事务");

            trans.Commit();

            trans = null;
#if UNITY_LWJ
            Debug.Log(this + " Commit");
#endif
        }
        void CheckOpen()
        {
            if (conn == null)
                throw new Exception("没有打开数据库");

        }
        void ExecuteBefore()
        {
            //清理最后的异常
            lastException = null;
        }
        private void DebugInfo(string action, string sqlText, object[] parameters)
        {
            Debug.Log("{0} cmdText:{1} \nparameter:{2}".FormatString(action, sqlText, string.Join(",", parameters.Select(o => o.ToStringOrEmpty()).ToArray())));
        }
        private void EntityDebugInfo(string action, string sqlText, object entity)
        {
            Debug.Log("{0} cmdText:{1} ".FormatString(action, sqlText));
        }
        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="cmdText">sql命令</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public SqliteDataReader ExecuteReader(string cmdText, params object[] parameters)
        {
            return _ExecuteReader(cmdText, parameters);
        }
        private SqliteDataReader _ExecuteReader(string cmdText, object[] parameters)
        {
            if (debug)
                DebugInfo("ExecuteReader", cmdText, parameters);
            ExecuteBefore();
            try
            {
                CheckOpen();
                using (var cmd = new SqliteCommand(cmdText, conn, trans))
                {

                    if (parameters != null && parameters.Length > 0)
                        AttachParameters(cmd, parameters);


                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex, cmdText, parameters);
                return null;
            }

        }
        public IEnumerable<T> ExecuteQuery<T>(string cmdText, params object[] parameters)
        {
            return _ExecuteQuery<T>(cmdText, parameters);
        }
        private IEnumerable<T> _ExecuteQuery<T>(string cmdText, object[] parameters)
        {
            if (debug)
                DebugInfo("ExecuteQuery", cmdText, parameters);
            ExecuteBefore();
            T obj;
            using (var reader = ExecuteReader(cmdText, parameters))
            {
                Type type = typeof(T);

                while (true)
                {
                    try
                    {
                        if (!reader.Read())
                            break;

                        if (type.IsPrimitive || type == typeof(string))
                        {
                            obj = ToValue<T>(reader[0]);
                        }
                        else
                        {
                            obj = Activator.CreateInstance<T>();
                            Fill(reader, obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetLastError(ex, cmdText, parameters);
                        break;
                    }
                    yield return obj;
                }
            }

        }
        public T ExecuteQueryFirst<T>(string cmdText, params object[] parameters)
        {
            return _ExecuteQueryFirst<T>(cmdText, parameters);
        }
        private T _ExecuteQueryFirst<T>(string cmdText, object[] parameters)
        {
            if (debug)
                DebugInfo("ExecuteQueryFirst", cmdText, parameters);
            ExecuteBefore();
            try
            {
                using (var reader = ExecuteReader(cmdText, parameters))
                {
                    if (reader.Read())
                    {
                        T obj = Activator.CreateInstance<T>();
                        Fill(reader, obj);
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex, cmdText, parameters);
                return default(T);
            }

            return default(T);
        }
        public int ExecuteNonQuery(string cmdText, params  object[] parameters)
        {
            return _ExecuteNonQuery(cmdText, parameters);
        }
        private int _ExecuteNonQuery(string cmdText, object[] parameters)
        {
            if (debug)
                DebugInfo("ExecuteNonQuery", cmdText, parameters);
            ExecuteBefore();
            try
            {
                CheckOpen();

                using (var cmd = new SqliteCommand(cmdText, conn, trans))
                {

                    if (parameters.Length > 0)
                        AttachParameters(cmd, parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex, cmdText, parameters);
                return 0;
            }
        }
        public object ExecuteScalar(string cmdText, params object[] parameters)
        {
            return _ExecuteScalar(cmdText, parameters);
        }
        private object _ExecuteScalar(string cmdText, object[] parameters)
        {
            if (debug)
                DebugInfo("ExecuteScalar", cmdText, parameters);
            ExecuteBefore();
            try
            {
                CheckOpen();
                using (var cmd = new SqliteCommand(cmdText, conn, trans))
                {

                    if (parameters.Length > 0)
                        AttachParameters(cmd, parameters);

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex, cmdText, parameters);
                return null;
            }
        }
        public T ExecuteScalar<T>(string cmdText, params object[] parameters)
        {
            return _ExecuteScalar<T>(cmdText, parameters);
        }
        private T _ExecuteScalar<T>(string cmdText, object[] parameters)
        {
            /* if (debug)
                 DebugInfo("ExecuteScalar", sqlText, parameters);
             ExecuteBefore();
             try
             {
                 CheckOpen();
                 using (var cmd = new SqliteCommand(sqlText, conn, trans))
                 {

                     if (parameters.Length > 0)
                         AttachParameters(cmd, parameters);
                     return ExecuteScalar<T>(cmd, default(T));
                 }
             }
             catch (Exception ex)
             {
                 SetLastError(ex, sqlText, parameters);
                 return default(T);
             }*/
            var defaultValue = default(T);
            return ToValue<T>(ExecuteScalar(cmdText, parameters), defaultValue);
        }
        private T ExecuteScalar<T>(SqliteCommand souce, T defaultValue)
        {

            var val = souce.ExecuteScalar();

            return ToValue<T>(val, defaultValue);
        }
        public Result<T> ExecuteScalarAsync<T>(string sqlText, params object[] parameters)
        {
            Result<T> result = new Result<T>();
            result.Routine = MonoBehaviourHelper.ToNonBlocking(() =>
            {
                try
                {
                    object obj = ExecuteScalar(sqlText, parameters);
                    result.Data = ToValue<T>(obj);
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }
            });

            return result;

        }
        public Result<int> ExecuteNonQueryAsync(string sqlText, params object[] parameters)
        {
            Result<int> result = new Result<int>();

            result.Routine = MonoBehaviourHelper.ToNonBlocking(() =>
            {
                try
                {
                    result.Data = ExecuteNonQuery(sqlText, parameters);
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }
            });

            return result;
        }
        //public IEnumerator ExecuteReader2(string sqlText, params object[] parameters)
        //{
        //    yield return WaitExecute();

        //    lastExecuteResult = ExecuteReader(sqlText, parameters);
        //}
        #region Entity
        private static Type classAttrType = typeof(EntityTableAttribute);
        private static Type memberAttrType = typeof(EntityMemberAttribute);
        private static Type memberIgnoreAttrType = typeof(IgnoreEntityMemberAttribute);

        public void EntityDelete(object entity)
        {
            if (entity == null)
                return;
            Type objType = entity.GetType();
            var members = GetEntityMembers(objType);
            var id = members.Where(o => o.ID).FirstOrDefault();
            if (id == null)
                throw new Exception("id null");


        }
        DataMemberDescription GetDataDesc(Type type)
        {
            return DataMemberInfo.GetDataMembers(type, classAttrType, memberAttrType, memberIgnoreAttrType);
        }
        DataMemberInfo[] GetMembers(Type type)
        {
            return GetDataDesc(type).Members;
        }
        EntityMemberAttribute[] GetEntityMembers(Type type)
        {
            return GetDataDesc(type).Members.Where(o => o.MemberAttribute != null).Select(o => (EntityMemberAttribute)o.MemberAttribute).ToArray();
        }

        public int EntityNonQuery(string cmdText, object entity)
        {
            return _ExecuteNonQuery(cmdText, GetEntityParameterList(cmdText, entity));
        }

        public object EntityScalar(string cmdText, object entity)
        {
            return _ExecuteScalar(cmdText, GetEntityParameterList(cmdText, entity));
        }


        public T EntityScalar<T>(string cmdText, object entity)
        {
            return _ExecuteScalar<T>(cmdText, GetEntityParameterList(cmdText, entity));
        }


        public IEnumerable<T> EntityQuery<T>(string cmdText, object entity)
        {
            return _ExecuteQuery<T>(cmdText, GetEntityParameterList(cmdText, entity));
        }
        public T EntityQueryFirst<T>(string cmdText, object entity)
        {
            return _ExecuteQueryFirst<T>(cmdText, GetEntityParameterList(cmdText, entity));
        }
        #endregion
        public static object ToDBValue(Type valueType, object value)
        {
            foreach (var converter in converters)
            {
                if (converter.CanConvert(valueType, typeof(string)))
                {
                    return converter.ConvertToDBValue(valueType, value, typeof(string));
                }
            }

            switch (valueType.ToString())
            {
                case ("System.DateTime"):

                    return TimeZone.CurrentTimeZone.ToUniversalTime(((DateTime)value)).ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return value;
        }
        public static object ToDBValue(object value)
        {
            if (value == null)
                return null;
            Type valueType = value.GetType();



            return ToDBValue(valueType, value);
        }
        public T ToValue<T>(object value)
        {
            return ToValue<T>(value, default(T));
        }
        public T ToValue<T>(object val, T defaultValue)
        {

            return (T)ValueOfType(typeof(T), val, defaultValue);
        }
        public static object ValueOfType(Type type, object dbValue)
        {
            return ValueOfType(type, dbValue, null);
        }
        public static object ValueOfType(Type type, object dbValue, object defaultValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return defaultValue;

            Type valType = dbValue.GetType();

            //Debug.Log(dbValue + "," + valType + "," + (valType == typeof(string)));

            foreach (var converter in converters)
            {
                if (converter.CanConvert(valType, type))
                {

                    return converter.ConvertToValue(dbValue, type);
                }
            }


            if (valType.ToString() == "System.MonoType")
            {
                dbValue = dbValue.ToString();
                valType = typeof(string);
            }


            string strVal;
            if (type != valType)
            {
                if (type.IsEnum)
                {
                    strVal = dbValue.ToStringOrEmpty();
                    ulong n;
                    if (ulong.TryParse(strVal, out n))
                    {
                        return Enum.ToObject(type, n);
                    }
                    else
                    {
                        return Enum.Parse(type, strVal, true);
                    }
                    // return Enum.ToObject(type, Convert.ChangeType(dbValue, typeof(long)));
                }
             
                
                //Debug.Log(type + "," + valType);
                if (type == typeof(DateTime))
                {

                    if (valType == typeof(long))
                    {
#if UNITY_LWJ
                        Debug.Log("long time:" + dbValue);
#endif
                        DateTime dt = DateTime.FromFileTime((long)dbValue);
                        dbValue = TimeZone.CurrentTimeZone.ToLocalTime(dt);
                        return dbValue;
                    }
                    else
                    {
                        dbValue = Convert.ChangeType(dbValue, type);
                    }

                    dbValue = TimeZone.CurrentTimeZone.ToLocalTime((DateTime)dbValue);
                }
                else if (type == typeof(Guid))
                {
                    if (valType == typeof(string))
                    {
                        dbValue = new Guid((string)dbValue);
                    }
                    else
                    {
                        dbValue = Convert.ChangeType(dbValue, type);
                    }
                }
                else
                {
                    //try
                    //{
                    dbValue = Convert.ChangeType(dbValue, type);
                    //}
                    //catch (Exception ex)
                    //{
                    //    val = null;
                    //}
                }
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    dbValue = TimeZone.CurrentTimeZone.ToLocalTime((DateTime)dbValue);
                }
            }



            return dbValue;
        }
        public long LastInsertRowID
        {
            get
            {
                return ExecuteScalar<long>("select last_insert_rowid()");
            }
        }
        public void UpdateChildCountField(string tableName, string idField, string parentIDField, string childCountField, string where = null)
        {
            string cmdText = string.Format("update [{0}]  set {3}=(select count(*) from [{0}] t2 where t2.[{2}]=[{0}].[{1}] ) ", tableName, idField, parentIDField, childCountField);

            if (!string.IsNullOrEmpty(where))
            {
                cmdText += " where " + where;
            }

            ExecuteNonQuery(cmdText);
        }
        //public SqliteCommand BuildSelectCommand(string table, string select, string where, string orderby, string start, int limit, object[] args)
        //{
        //    return BuildSelectCommand(table, select, where, orderby, start, limit, "", args);
        //}
        //public SqliteCommand BuildSelectCommand(string table, string select, string where, string orderby, string start, int limit, string rowid, object[] args)
        //{
        //    if (string.IsNullOrEmpty(select))
        //        select = "*";

        //    if (!string.IsNullOrEmpty(where))
        //        where = " where " + where;
        //    if (string.IsNullOrEmpty(rowid))
        //        rowid = "rowid";

        //    //string cmdText = string.Format("select {1} from {0} {2} {3} limit ( select IFNULL(max(rowid),0) from ( select {6},* from {0}  {2}  {3} ) as t where t.{4}  ) , {5} ",
        //    string cmdText = string.Format("select {1} from {0} {2} {3} limit ( select IFNULL(max(rowid),0) from ( select {6},* from {0}  {2}  {3} ) as t where t.{4}  ) , {5} ",
        //        table, select, where, orderby, start, limit, rowid);

        //    SqliteCommand cmd = new SqliteCommand(cmdText, conn, trans);

        //    AttachParameters(cmd, args);

        //    return cmd;

        //}
        public SqliteCommand BuildSelectCommand(string table, string select, string where, string orderby, int limit, object[] args)
        {
            if (string.IsNullOrEmpty(select))
                select = "*";

            if (!string.IsNullOrEmpty(where))
                where = " where " + where;

            string cmdText = string.Format("select {1} from {0} {2} {3} limit {4}", table, select, where, orderby, limit);

            SqliteCommand cmd = new SqliteCommand(cmdText, conn, trans);

            AttachParameters(cmd, args);

            return cmd;
        }
        public SqliteCommand BuildSelectCommand(string table, string select, string where, string orderby, object[] args)
        {
            if (string.IsNullOrEmpty(select))
                select = "*";

            if (!string.IsNullOrEmpty(where))
                where = " where " + where;

            string cmdText = string.Format("select {1} from {0} {2} {3}", table, select, where, orderby);
            using (SqliteCommand cmd = new SqliteCommand(cmdText, conn, trans))
            {
                AttachParameters(cmd, args);

                return cmd;
            }


        }
        bool CheckType(Type type)
        {
            if (type.IsEnum)
                return true;

            if (type == null)
                return false;
            if (type.IsPrimitive)
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type.IsArray)
                return true;
            return false;
        }
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.SetProperty | BindingFlags.SetField;
        static Dictionary<Type, IDataConverter> converterCached = new Dictionary<Type, IDataConverter>();
        static IDataConverter GetConverter(Type type)
        {
            IDataConverter conv;
            if (converterCached.TryGetValue(type, out conv))
                return conv;

            conv = (IDataConverter)type.GetConstructor(Type.EmptyTypes).Invoke(null);
            converterCached[type] = conv;

            return conv;
        }
        public void Fill(SqliteDataReader reader, object obj)
        {
            if (obj == null)
                return;
            Type objType = obj.GetType();

            DataMemberInfo[] members = GetMembers(objType);
            DataMemberInfo member = null;
            object val = null;
            int fieldCount = reader.FieldCount;

            //bool canConvert = false;
            try
            {
                object[] values = new object[fieldCount];
                int n = reader.GetValues(values);
                if (n != fieldCount)
                {
                    Debug.LogError("n:" + n + "," + fieldCount);
                    throw new Exception("field count:" + fieldCount + ", " + n);
                }

                for (int i = 0; i < fieldCount; i++)
                {
                    string fieldName = reader.GetName(i);

                    fieldName = fieldName.ToLower();

                    member = members.Where(o => o.LowerMemberName == fieldName).FirstOrDefault();
                    if (member == null)
                        continue;
                    //val = reader.GetValue(i); 
                    val = values[i];
                    //canConvert = false;
                    //foreach (var converter in converters)
                    //{
                    //    if (converter.CanConvert(member.MemberType))
                    //    {
                    //        //val = converter.ConvertToValue(val, member.MemberType);
                    //        canConvert = true;
                    //    }
                    //}

                    if (member.ConverterType != null)
                    {
                        var conv = GetConverter(member.ConverterType);
                        val = conv.ConvertToValue(val, member.MemberType);
                    }
                    else
                    {
                        //if (!CheckType(member.MemberType))
                        //    continue;

                        val = ValueOfType(member.MemberType, val);
                    }


                    //val = ValueOfType(member.MemberType, val);

                    if (member.pInfo != null)
                    {
                        member.pInfo.SetValueUnity(obj, val, null);
                    }
                    else if (member.fInfo != null)
                    {
                        member.fInfo.SetValue(obj, val);

                    }


                }

            }
            catch (Exception ex)
            {
                if (member != null)
                {
                    string msg = "";
                    if (member.pInfo != null)
                    {
                        msg = "type:" + member.pInfo.DeclaringType.Name + " property:" + member.pInfo.Name + ", type:" + member.pInfo.PropertyType;
                    }
                    else
                    {
                        msg = "type:" + member.fInfo.DeclaringType.Name + " field:" + member.fInfo.Name + ", type:" + member.fInfo.FieldType;
                    }
                    msg += ", dbValue:" + val + "," + ex;
                    Debug.LogError(msg);
                }
                throw ex;
            }


        }
        public bool CopyError(Result result)
        {

            if (HasError)
            {
                result.Success = false;
                result.Error = LastError.ToStringOrEmpty();
                return true;

            }
            return false;
        }
        #region IDisposable 成员

        public void Dispose()
        {
            Close();
        }

        ~SqliteDatabase()
        {
            if (trans != null)
            {
                Rollback();
            }
            if (conn != null)
            {
                Dispose();
            }
#if UNITY_LWJ
            Debug.Log(GetType() + "~  Dispose");
#endif
        }
        #endregion
    }

    //private class SqliteDataReaderWrap : DbDataReader, IDisposable
    //{
    //    public SqliteDataReaderWrap(SqliteDataReader reader)
    //    {
    //        this.reader = reader;
    //    }
    //    private SqliteDataReader reader;
    //    public override void Close()
    //    {
    //        reader.Close();
    //    }
    //    public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
    //    {
    //        return reader.CreateObjRef(requestedType);
    //    }
    //    public override int Depth
    //    {
    //        get { return reader.Depth; }
    //    }


    //    //void IDisposable.Dispose()
    //    //{
    //    //    if (!reader.IsClosed)
    //    //        reader.Close();
    //    //}

    //    void IDisposable.Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public static class SqliteDatabaseExtensions
    {
        public static Result<bool> ReadAsync(this SqliteDataReader reader)
        {
            Result<bool> result = new Result<bool>();
            result.Routine = MonoBehaviourHelper.ToNonBlocking(() =>
            {
                try
                {
                    result.Data = reader.Read();
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }

            });
            return result;
        }
    }
}




