using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

using System.Global;
using System.Define;
using Windows.Network;
using System.DataPacket;

namespace System.DataCenter
{
    public interface IDataService
    {
        bool OnConnect(bool bUsedTrans);
        void OnDisConnected();
        void OnQuery(string strQuery);
        void OnQuery(bool bReturn, string strProc, DbParameter[] pParams);
        void OnResult(RecordStyle eRecordStyle);

        #region 属性
        /// <summary>
        /// 异常信息
        /// </summary>
        string Message
        {
            get;
        }

        IDataOperate DataCenter
        {
            get;
        }
        #endregion

        #region SQL PLUS方法
        #region 获取数据库架构
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        void GetDatabase();

        /// <summary>
        /// 获取数据库结构－表、视图、触发器、存储过程
        /// </summary>
        void GetDatabase_Structure(CustomData[,] mContent);

        void GetDatabase_Table_List(CustomData[,] mContent);
        void GetDatabase_View_List(CustomData[,] mContent);
        void GetDatabase_Proc_List(CustomData[,] mContent);
        void GetDatabase_Function_List(CustomData[,] mContent);

        void GetDatabase_Table(CustomData[,] mContent);
        void GetDatabase_View(CustomData[,] mContent);
        void GetDatabase_Proc(CustomData[,] mContent);
        void GetDatabase_Function(CustomData[,] mContent);

        /// <summary>
        /// 获取数据类型
        /// </summary>
        void GetData_Type();

        /// <summary>
        /// 获取二进制排序规则
        /// </summary>
        void GetCollation();
        #endregion

        #region 表操作
        /// <summary>
        /// 表－添加新列
        /// </summary>        
        string Alter_Table_AddColumn(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－删除操作
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Drop(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－修改操作
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Alter(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加主键
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加外键
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddForeignKey(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加默认值
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddDefault(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加约束
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddCheck(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加Unique约束
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        string Alter_Table_AddUnique(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加扩展属性
        /// </summary>
        /// <param name="mContent"></param>        
        /// <returns></returns>
        string Alter_Table_AddExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－修改扩展属性
        /// </summary>
        /// <param name="mContent"></param>       
        string Alter_Table_ModExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－删除扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        string Alter_Table_DelExtend(CustomData[,] mContent, int i);
        #endregion

        #region
        /// <summary>
        /// 重命名－存储过程、触发器、表
        /// </summary>
        /// <returns></returns>
        string Rename(CustomData[,] mContent);

        /// <summary>
        /// 删除－存储过程、触发器、表
        /// </summary>
        /// <returns></returns>
        string Drop_Object(CustomData[,] mContent);
        #endregion
        #endregion
    }

    /// <summary>
    /// 数据处理接口
    /// </summary>
    public interface IDataOperate
    {
        #region 基本操作
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="bUsedTrans"></param>
        /// <returns></returns>
        bool Connect(bool bUsedTrans);

        /// <summary>
        /// 断开数据库
        /// </summary>
        void DisConnected();

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="bTransCommit">是否执行事务</param>
        void DoTrans(bool bTransCommit);

        /// <summary>
        /// 执行SQL表达式
        /// </summary>
        /// <param name="strQuery">SQL表达式</param>
        void ExecuteQuery(string strQuery);

        /// <summary>
        /// 执行SQL存储过程
        /// </summary>
        /// <param name="strProc">是否有返回值</param>
        /// <param name="strProc">存储过程名称</param>
        /// <param name="pParams">存储过程参数集合</param>
        void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams);

        /// <summary>
        /// 获取执行结果
        /// </summary>
        /// <param name="eRecordStyle">结果样式</param>
        void GetResult(RecordStyle eRecordStyle);
        #endregion

        #region 属性
        /// <summary>
        /// 影响行数
        /// </summary>
        int AffectRow
        {
            get;
        }

        /// <summary>
        /// 异常信息
        /// </summary>
        string Message
        {
            get;
        }

        /// <summary>
        /// 版本号
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// 数据集
        /// </summary>
        object RecordData
        {
            get;
        }

        /// <summary>
        /// 执行超时（秒）
        /// </summary>
        int ExecuteTimeOut
        {
            get;
            set;
        }

        object ProcReturn
        {
            get;
        }

        ISocketConnection Channels
        {
            get;
            set;
        }

        TaskService Service
        {
            get;
            set;
        }
        #endregion

        IMSSQLOperate SQLServer();
        IMYSQLOperate MYServer();

        #region SQL PLUS方法
        #region 获取数据库架构
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        void GetDatabase();

        /// <summary>
        /// 获取数据库结构－表、视图、触发器、存储过程
        /// </summary>
        void GetDatabase_Structure(CustomData[,] mContent);

        void GetDatabase_Table_List(CustomData[,] mContent);
        void GetDatabase_View_List(CustomData[,] mContent);
        void GetDatabase_Proc_List(CustomData[,] mContent);
        void GetDatabase_Function_List(CustomData[,] mContent);

        void GetDatabase_Table(CustomData[,] mContent);
        void GetDatabase_View(CustomData[,] mContent);
        void GetDatabase_Proc(CustomData[,] mContent);
        void GetDatabase_Function(CustomData[,] mContent);

        /// <summary>
        /// 获取数据类型
        /// </summary>
        void GetData_Type();

        /// <summary>
        /// 获取二进制排序规则
        /// </summary>
        void GetCollation();
        #endregion

        #region 表操作
        /// <summary>
        /// 表－添加新列
        /// </summary>        
        string Alter_Table_AddColumn(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－删除操作
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Drop(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－修改操作
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Alter(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加主键
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加外键
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddForeignKey(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加默认值
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddDefault(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加约束
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddCheck(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加Unique约束
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        string Alter_Table_AddUnique(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－添加扩展属性
        /// </summary>
        /// <param name="mContent"></param>        
        /// <returns></returns>
        string Alter_Table_AddExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－修改扩展属性
        /// </summary>
        /// <param name="mContent"></param>       
        string Alter_Table_ModExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// 表－删除扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        string Alter_Table_DelExtend(CustomData[,] mContent, int i);
        #endregion

        #region
        /// <summary>
        /// 重命名－存储过程、触发器、表
        /// </summary>
        /// <returns></returns>
        string Rename(CustomData[,] mContent);

        /// <summary>
        /// 删除－存储过程、触发器、表
        /// </summary>
        /// <returns></returns>
        string Drop_Object(CustomData[,] mContent);
        #endregion
        #endregion
    }

    /// <summary>
    /// MSSQL 处理方法
    /// </summary>
    public interface IMSSQLOperate : IDataOperate
    {
        TaskService Service
        {
            get;
            set;
        }

        ISocketConnection Channels
        {
            get;
            set;
        }
    }

    /// <summary>
    /// MYSQL 处理方法
    /// </summary>
    public interface IMYSQLOperate : IDataOperate
    {
        TaskService Service
        {
            get;
            set;
        }

        ISocketConnection Channels
        {
            get;
            set;
        }
    }
}
