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
        #region ����/��������
        public DataOperate()
        {
        }
        #endregion

        #region IDataOperate Members
        #region
        /// <summary>
        /// �������ݿ�
        /// </summary>
        /// <param name="bUsedTrans"></param>
        /// <returns></returns>
        public abstract bool Connect(bool bUsedTrans);

        /// <summary>
        /// �Ͽ����ݿ�
        /// </summary>
        public abstract void DisConnected();

        /// <summary>
        /// ִ������
        /// </summary>
        /// <param name="bTransCommit"></param>
        public abstract void DoTrans(bool bTransCommit);

        /// <summary>
        /// ִ��SQL���
        /// </summary>
        /// <param name="strQuery"></param>
        public abstract void ExecuteQuery(string strQuery);

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <param name="strProc"></param>
        /// <param name="bReturn"></param>
        /// <param name="pParams"></param>
        public abstract void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams);

        /// <summary>
        /// ��ȡ��¼��
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

        #region ǿ��ת��
        public abstract IMSSQLOperate SQLServer();
        public abstract IMYSQLOperate MYServer();
        #endregion
        #endregion

        #region SQL PLUS
        #region ��ȡ���ݿ�ܹ�
        /// <summary>
        /// ��ȡ���ݿ��б�
        /// </summary>
        public abstract void GetDatabase();

        /// <summary>
        /// ��ȡ���ݿ�ṹ������ͼ�����������洢����
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
        /// ��ȡ��������
        /// </summary>
        public abstract void GetData_Type();

        /// <summary>
        /// ��ȡ�������������
        /// </summary>
        public abstract void GetCollation();
        #endregion

        #region �����
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
