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
    public abstract class DataOperate : CustomClass , IDataOperate
    {
        #region 构造/析构函数
        public DataOperate()
        {
        }
        #endregion

        #region IDataOperate Members
        #region
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="bUsedTrans"></param>
        /// <returns></returns>
        public abstract bool Connect(bool bUsedTrans);

        /// <summary>
        /// 断开数据库
        /// </summary>
        public abstract void DisConnected();

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="bTransCommit"></param>
        public abstract void DoTrans(bool bTransCommit);

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="strQuery"></param>
        public abstract void ExecuteQuery(string strQuery);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="strProc"></param>
        /// <param name="bReturn"></param>
        /// <param name="pParams"></param>
        public abstract void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams);

        /// <summary>
        /// 获取记录集
        /// </summary>
        /// <param name="eRecordStyle"></param>
        public abstract void GetResult(RecordStyle eRecordStyle);        

        public abstract int AffectRow
        {
            get;
        }

        public abstract string Message
        {
            get;
        }

        public abstract string Version
        {
            get;
        }

        public abstract object RecordData
        {
            get;
        }

        public abstract int ExecuteTimeOut
        {
            get;
            set;
        }

        public abstract object ProcReturn
        {
            get;
        }

        public abstract ISocketConnection Channels
        {
            get;
            set;
        }

        public abstract TaskService Service
        {
            get;
            set;
        }
        #endregion

        #region 强制转换
        public abstract IMSSQLOperate SQLServer();
        public abstract IMYSQLOperate MYServer();
        #endregion
        #endregion

        #region SQL PLUS
        #region 获取数据库架构
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        public abstract void GetDatabase();

        /// <summary>
        /// 获取数据库结构－表、视图、触发器、存储过程
        /// </summary>
        public abstract void GetDatabase_Structure(CustomData[,] mContent);

        public abstract void GetDatabase_Table_List(CustomData[,] mContent);
        public abstract void GetDatabase_View_List(CustomData[,] mContent);
        public abstract void GetDatabase_Proc_List(CustomData[,] mContent);
        public abstract void GetDatabase_Function_List(CustomData[,] mContent);

        public abstract void GetDatabase_Table(CustomData[,] mContent);
        public abstract void GetDatabase_View(CustomData[,] mContent);
        public abstract void GetDatabase_Proc(CustomData[,] mContent);
        public abstract void GetDatabase_Function(CustomData[,] mContent);

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public abstract void GetData_Type();

        /// <summary>
        /// 获取二进制排序规则
        /// </summary>
        public abstract void GetCollation();
        #endregion

        #region 表操作
        public abstract string Alter_Table_AddColumn(CustomData[,] mContent, int i);
        public abstract string Alter_Table_Drop(CustomData[,] mContent, int i);
        public abstract string Alter_Table_Alter(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddForeignKey(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddDefault(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddCheck(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddUnique(CustomData[,] mContent, int i);
        public abstract string Alter_Table_AddExtend(CustomData[,] mContent, int i);
        public abstract string Alter_Table_ModExtend(CustomData[,] mContent, int i);
        public abstract string Alter_Table_DelExtend(CustomData[,] mContent, int i);
        #endregion

        #region
        public abstract string Rename(CustomData[,] mContent);
        public abstract string Drop_Object(CustomData[,] mContent);
        #endregion
        #endregion
    }
}
