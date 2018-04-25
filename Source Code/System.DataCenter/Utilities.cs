using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;

using System.Define;
using System.DataPacket;
using Windows.Network;

namespace System.DataCenter
{
    public static class DataUtilities
    {
        #region 数据合并
        public static DataTable MergeTable(DataSet pDataSet)
        {
            DataTable pResultTable = new DataTable();

            foreach (DataTable pDataTable in pDataSet.Tables)
            {
                pResultTable.Merge(pDataTable);
            }

            return pResultTable;
        }
        #endregion

        #region 存储过程参数缓存
        /// <summary>
        /// 保存存储过程参数
        /// </summary>
        /// <param name="strKey">存储过程参数表名称</param>
        /// <param name="pCommandParms">存储过程参数</param>
        public static void SetParameters(string strCacheKey, params DbParameter[] pCommandParms)
        {
            //DataUtilities.pParamsCache[strCacheKey] = pCommandParms;
        }

        /// <summary>
        /// 获取存储过程参数缓存
        /// </summary>
        /// <param name="strCacheKey">索引</param>
        /// <returns></returns>
        public static DbParameter[] GetParameters(string strCacheKey)
        {
            return null;
            //DbParameter[] pCacheParams = (DbParameter[])DataUtilities.pParamsCache[strCacheKey];

            //if (pCacheParams == null)
            //{
            //    return null;
            //}
            //else
            //{
            //    return (DbParameter[])pCacheParams.Clone();
            //}
        }
        #endregion

        #region 字段定义
        private static Hashtable pParamsCache = Hashtable.Synchronized(new Hashtable());
        #endregion
    }

    /// <summary>
    /// 数据回调类
    /// </summary>
    internal class RecordDataCallback
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SocketConnection"></param>
        /// <param name="eService"></param>
        /// <param name="pCommand"></param>
        public RecordDataCallback(ISocketConnection SocketConnection, TaskService eService, DbCommand pCommand)
        {
            this._SocketConnection = SocketConnection;
            this._eService = eService;
            this._pCommand = pCommand;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 连接管理
        /// </summary>
        public DbCommand Command
        {
            get
            {
                return this._pCommand;
            }
        }

        public TaskService Service
        {
            get
            {
                return this._eService;
            }
        }

        public ISocketConnection Channels
        {
            get
            {
                return this._SocketConnection;
            }
        }
        #endregion

        #region 变量定义
        private DbCommand _pCommand;
        private TaskService _eService;
        private ISocketConnection _SocketConnection;
        #endregion
    }
}
