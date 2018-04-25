using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;

using System.Global;
using System.Define;
using System.DataPacket;
using Windows.Network;

namespace System.DataCenter
{
    public class DataService : IDataService
    {
        #region 构造函数
        public DataService(DataBase eDataBase, string strConnect)            
        {
            this._strConnect = strConnect;
            this._eDataBase = eDataBase;
            _pSocketConnection = null;
        }

        public DataService(DataBase eDataBase, string strConnect, ISocketConnection pSocketConnection, TaskService eService)
        {
            this._strConnect = strConnect;
            this._eDataBase = eDataBase;
            this._pSocketConnection = pSocketConnection;
            this._eService = eService;
        }

        private DataBase _eDataBase;
        private string _strConnect;
        public DataOperate pDataOperate;
        private string _strMessage;
        private ISocketConnection _pSocketConnection;
        private TaskService _eService;

        #endregion

        #region IDataService Members
        public bool OnConnect(bool bUsedTrans)
        {
            bool bResult = false;
            switch (this._eDataBase)
            {
                //case DataBase.ODBC:
                //    this.pDataOperate = new ODBCProcess(this, this._strConnect, iTimeOut);
                //    bResult = this.pDataOperate.Connect(true);
                //    break;
                //case DataBase.OLEDB:
                //    this.pDataOperate = new OLEProcess(this, this._strConnect, iTimeOut);
                //    bResult = this.pDataOperate.Connect(true);
                //    break;
                case DataBase.MSSQL:
                    try
                    {
                        SqlConnection mycn = new SqlConnection(this._strConnect);
                        mycn.Open();
                        string check = mycn.ServerVersion.Substring(0, 2);
                        if (check == "08")//sql 2000
                        {
                            this.pDataOperate = new MSSQLOperate_Early(this._strConnect);
                            pDataOperate.Channels = this._pSocketConnection;
                            pDataOperate.Service = this._eService;
                            bResult = this.pDataOperate.Connect(bUsedTrans);
                        }
                        else
                        {
                            this.pDataOperate = new MSSQLOperate(this._strConnect);
                            pDataOperate.Channels = this._pSocketConnection;
                            pDataOperate.Service = this._eService;
                            bResult = this.pDataOperate.Connect(bUsedTrans);
                        }
                    }
                    catch (Exception ex)
                    { }                    
                    break;
                case DataBase.MYSQL:
                    this.pDataOperate = new MYSQLOperate(this._strConnect);
                    bResult = this.pDataOperate.Connect(true);
                    break;
                //case DataBase.ORACLE:
                //    this.pDataOperate = new OracleProcess(this, this._strConnect, iTimeOut);
                //    bResult = this.pDataOperate.Connect(true);
                //    break;
                case DataBase.DB2:
                case DataBase.FILE:
                    break;
            }

            return bResult;
        }

        public void OnDisConnected()
        {
            this.pDataOperate.DisConnected();
        }

        public void OnQuery(string strQuery)
        {
            this.pDataOperate.ExecuteQuery(strQuery);
        }

        public void OnQuery(bool bReturn, string strProc, DbParameter[] pParams)
        {
            this.pDataOperate.ExecuteQuery(bReturn, strProc, pParams);
        }

        public void OnResult(RecordStyle eRecordStyle)
        {
            this.pDataOperate.GetResult(eRecordStyle);
        }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Message
        {
            get
            {
                return this.DataCenter.Message;
            }
        }

        public IDataOperate DataCenter
        {
            get
            {
                return pDataOperate;
            }
        }
        #endregion

        #region SQL PLUS方法
        #region 获取数据库架构
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        public void GetDatabase()
        {
            this.pDataOperate.GetDatabase();
        }

        /// <summary>
        /// 获取数据库结构－表、视图、触发器、存储过程
        /// </summary>
        public void GetDatabase_Structure(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Structure(mContent);
        }

        public void GetDatabase_Table_List(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Table_List(mContent);
        }

        public void GetDatabase_View_List(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_View_List(mContent);
        }

        public void GetDatabase_Proc_List(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Proc_List(mContent);
        }

        public void GetDatabase_Function_List(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Function_List(mContent);
        }

        public void GetDatabase_Table(CustomData[,] mContent) 
        {
            this.pDataOperate.GetDatabase_Table(mContent);
        }

        public void GetDatabase_View(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_View(mContent);
        }

        public void GetDatabase_Proc(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Proc(mContent);
        }

        public void GetDatabase_Function(CustomData[,] mContent)
        {
            this.pDataOperate.GetDatabase_Function(mContent);
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public void GetData_Type()
        {
            this.pDataOperate.GetData_Type();
        }

        /// <summary>
        /// 获取二进制排序规则
        /// </summary>
        public void GetCollation()
        {
            this.pDataOperate.GetCollation();
        }
        #endregion

        #region 表操作
        /// <summary>
        /// 表－添加新列
        /// </summary>
        public string Alter_Table_AddColumn(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddColumn(mContent, i);
        }

        /// <summary>
        /// 表－删除操作
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_Drop(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_Drop(mContent, i);
        }

        /// <summary>
        /// 表－修改操作
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_Alter(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_Alter(mContent, i);
        }

        /// <summary>
        /// 表－添加主键
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddPrimaryKey(mContent, i);
        }

        /// <summary>
        /// 表－添加外键
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_AddForeignKey(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddForeignKey(mContent, i);
        }

        /// <summary>
        /// 表－添加默认值
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_AddDefault(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddDefault(mContent, i);
        }

        /// <summary>
        /// 表－添加约束
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_AddCheck(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddCheck(mContent, i);
        }

        /// <summary>
        /// 表－添加Unique约束
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        public string Alter_Table_AddUnique(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddUnique(mContent, i);
        }

        /// <summary>
        /// 表－添加扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_AddExtend(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_AddExtend(mContent, i);
        }

        /// <summary>
        /// 表－修改扩展属性
        /// </summary>
        /// <param name="mContent"></param>       
        public string Alter_Table_ModExtend(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_ModExtend(mContent, i);
        }
        /// <summary>
        /// 表－删除扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        public string Alter_Table_DelExtend(CustomData[,] mContent, int i)
        {
            return pDataOperate.Alter_Table_DelExtend(mContent, i);
        }
        #endregion       

        #region
        public string Rename(CustomData[,] mContent)
        {
            return pDataOperate.Rename(mContent);
        }

        public string Drop_Object(CustomData[,] mContent)
        {
            return pDataOperate.Drop_Object(mContent);
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// 连接字符串处理
    /// </summary>
    public class DataSqlConn
    {
        /// <summary>
        /// 获取对应的连接字符串
        /// </summary>
        /// <param name="eDataBase"></param>
        /// <param name="strip"></param>
        /// <param name="strdatabase"></param>
        /// <param name="struser"></param>
        /// <param name="strpwd"></param>
        /// <returns></returns>
        public static string strConn(DataBase eDataBase, string strip, string strdatabase, string struser, string strpwd)
        {

            string str_conn = null;

            switch (eDataBase)
            {
                case DataBase.ODBC:
                    break;
                case DataBase.OLEDB:
                    break;
                case DataBase.MSSQL:
                    str_conn = "Connect Timeout=3000;Server=" + strip + ",1433;Database=" + strdatabase + ";Uid=" + struser + ";pwd=" + strpwd;
                    break;
                case DataBase.MYSQL:
                    //str_conn = "Connect Timeout=3000;Server=" + strip + ";Database = itemdb; Uid=" + struser + ";pwd=" + strpwd;
                    str_conn = "Connect Timeout=3000;Server=" + strip + "; Uid=" + struser + ";pwd=" + strpwd;
                    break;
                case DataBase.ORACLE:
                    break;
                case DataBase.DB2:
                case DataBase.FILE:
                    break;
            }

            return str_conn;
        }

        /// <summary>
        /// 验证sql语句
        /// </summary>
        /// <returns></returns>
        public static bool Check_Sql(string str_sql)
        {
            bool result = false;
            if (str_sql.ToLower().StartsWith("select") && !str_sql.ToLower().Contains("update") && !str_sql.ToLower().Contains("delete") && !str_sql.ToLower().Contains("truncate"))
            {   
                
                result = true;
            }
            else if (str_sql.ToLower().Contains("update") || str_sql.ToLower().Contains("delete") || str_sql.ToLower().Contains("truncate"))
            {
                if (str_sql.ToLower().Contains("where") && !str_sql.ToLower().Contains("in") && !str_sql.ToLower().Contains("not in"))
                {
                    result = Check_Equal(str_sql);
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 验证SQL语句WHERE条件的等号两边是否相同
        /// </summary>
        /// <param name="str_sql"></param>
        /// <returns></returns>
        private static bool Check_Equal(string str_sql)
        {
            bool result = false;
            string[] a = str_sql.Split('=');
            int uuu = a.Length - 1;
            if (uuu > 1)
            {
                for (int i = 0; i < uuu; i++)
                {
                    string aa = a[i].Trim();
                    string bb = a[i + 1].Trim();

                    string[] aaa = aa.Split(' ');
                    string[] bbb = bb.Split(' ');

                    string aaaa = aaa[aaa.Length - 1].Trim();
                    string bbbb = bbb[0].Trim();
                    if (aaaa != bbbb)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        return result;
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
    }
}
