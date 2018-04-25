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
        #region ���ݺϲ�
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

        #region �洢���̲�������
        /// <summary>
        /// ����洢���̲���
        /// </summary>
        /// <param name="strKey">�洢���̲���������</param>
        /// <param name="pCommandParms">�洢���̲���</param>
        public static void SetParameters(string strCacheKey, params DbParameter[] pCommandParms)
        {
            //DataUtilities.pParamsCache[strCacheKey] = pCommandParms;
        }

        /// <summary>
        /// ��ȡ�洢���̲�������
        /// </summary>
        /// <param name="strCacheKey">����</param>
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

        #region �ֶζ���
        private static Hashtable pParamsCache = Hashtable.Synchronized(new Hashtable());
        #endregion
    }

    /// <summary>
    /// ���ݻص���
    /// </summary>
    internal class RecordDataCallback
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
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

        #region ���Զ���
        /// <summary>
        /// ���ӹ���
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

        #region ��������
        private DbCommand _pCommand;
        private TaskService _eService;
        private ISocketConnection _SocketConnection;
        #endregion
    }
}
