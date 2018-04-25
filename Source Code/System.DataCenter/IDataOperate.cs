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

        #region ����
        /// <summary>
        /// �쳣��Ϣ
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

        #region SQL PLUS����
        #region ��ȡ���ݿ�ܹ�
        /// <summary>
        /// ��ȡ���ݿ��б�
        /// </summary>
        void GetDatabase();

        /// <summary>
        /// ��ȡ���ݿ�ṹ������ͼ�����������洢����
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
        /// ��ȡ��������
        /// </summary>
        void GetData_Type();

        /// <summary>
        /// ��ȡ�������������
        /// </summary>
        void GetCollation();
        #endregion

        #region �����
        /// <summary>
        /// ���������
        /// </summary>        
        string Alter_Table_AddColumn(CustomData[,] mContent, int i);

        /// <summary>
        /// ��ɾ������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Drop(CustomData[,] mContent, int i);

        /// <summary>
        /// ���޸Ĳ���
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Alter(CustomData[,] mContent, int i);

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i);

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddForeignKey(CustomData[,] mContent, int i);

        /// <summary>
        /// �����Ĭ��ֵ
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddDefault(CustomData[,] mContent, int i);

        /// <summary>
        /// �����Լ��
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddCheck(CustomData[,] mContent, int i);

        /// <summary>
        /// �����UniqueԼ��
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        string Alter_Table_AddUnique(CustomData[,] mContent, int i);

        /// <summary>
        /// �������չ����
        /// </summary>
        /// <param name="mContent"></param>        
        /// <returns></returns>
        string Alter_Table_AddExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// ���޸���չ����
        /// </summary>
        /// <param name="mContent"></param>       
        string Alter_Table_ModExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// ��ɾ����չ����
        /// </summary>
        /// <param name="mContent"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        string Alter_Table_DelExtend(CustomData[,] mContent, int i);
        #endregion

        #region
        /// <summary>
        /// ���������洢���̡�����������
        /// </summary>
        /// <returns></returns>
        string Rename(CustomData[,] mContent);

        /// <summary>
        /// ɾ�����洢���̡�����������
        /// </summary>
        /// <returns></returns>
        string Drop_Object(CustomData[,] mContent);
        #endregion
        #endregion
    }

    /// <summary>
    /// ���ݴ���ӿ�
    /// </summary>
    public interface IDataOperate
    {
        #region ��������
        /// <summary>
        /// �������ݿ�
        /// </summary>
        /// <param name="bUsedTrans"></param>
        /// <returns></returns>
        bool Connect(bool bUsedTrans);

        /// <summary>
        /// �Ͽ����ݿ�
        /// </summary>
        void DisConnected();

        /// <summary>
        /// ִ������
        /// </summary>
        /// <param name="bTransCommit">�Ƿ�ִ������</param>
        void DoTrans(bool bTransCommit);

        /// <summary>
        /// ִ��SQL���ʽ
        /// </summary>
        /// <param name="strQuery">SQL���ʽ</param>
        void ExecuteQuery(string strQuery);

        /// <summary>
        /// ִ��SQL�洢����
        /// </summary>
        /// <param name="strProc">�Ƿ��з���ֵ</param>
        /// <param name="strProc">�洢��������</param>
        /// <param name="pParams">�洢���̲�������</param>
        void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams);

        /// <summary>
        /// ��ȡִ�н��
        /// </summary>
        /// <param name="eRecordStyle">�����ʽ</param>
        void GetResult(RecordStyle eRecordStyle);
        #endregion

        #region ����
        /// <summary>
        /// Ӱ������
        /// </summary>
        int AffectRow
        {
            get;
        }

        /// <summary>
        /// �쳣��Ϣ
        /// </summary>
        string Message
        {
            get;
        }

        /// <summary>
        /// �汾��
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// ���ݼ�
        /// </summary>
        object RecordData
        {
            get;
        }

        /// <summary>
        /// ִ�г�ʱ���룩
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

        #region SQL PLUS����
        #region ��ȡ���ݿ�ܹ�
        /// <summary>
        /// ��ȡ���ݿ��б�
        /// </summary>
        void GetDatabase();

        /// <summary>
        /// ��ȡ���ݿ�ṹ������ͼ�����������洢����
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
        /// ��ȡ��������
        /// </summary>
        void GetData_Type();

        /// <summary>
        /// ��ȡ�������������
        /// </summary>
        void GetCollation();
        #endregion

        #region �����
        /// <summary>
        /// ���������
        /// </summary>        
        string Alter_Table_AddColumn(CustomData[,] mContent, int i);

        /// <summary>
        /// ��ɾ������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Drop(CustomData[,] mContent, int i);

        /// <summary>
        /// ���޸Ĳ���
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_Alter(CustomData[,] mContent, int i);

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i);

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddForeignKey(CustomData[,] mContent, int i);

        /// <summary>
        /// �����Ĭ��ֵ
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddDefault(CustomData[,] mContent, int i);

        /// <summary>
        /// �����Լ��
        /// </summary>
        /// <param name="mContent"></param>
        string Alter_Table_AddCheck(CustomData[,] mContent, int i);

        /// <summary>
        /// �����UniqueԼ��
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        string Alter_Table_AddUnique(CustomData[,] mContent, int i);

        /// <summary>
        /// �������չ����
        /// </summary>
        /// <param name="mContent"></param>        
        /// <returns></returns>
        string Alter_Table_AddExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// ���޸���չ����
        /// </summary>
        /// <param name="mContent"></param>       
        string Alter_Table_ModExtend(CustomData[,] mContent, int i);

        /// <summary>
        /// ��ɾ����չ����
        /// </summary>
        /// <param name="mContent"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        string Alter_Table_DelExtend(CustomData[,] mContent, int i);
        #endregion

        #region
        /// <summary>
        /// ���������洢���̡�����������
        /// </summary>
        /// <returns></returns>
        string Rename(CustomData[,] mContent);

        /// <summary>
        /// ɾ�����洢���̡�����������
        /// </summary>
        /// <returns></returns>
        string Drop_Object(CustomData[,] mContent);
        #endregion
        #endregion
    }

    /// <summary>
    /// MSSQL ������
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
    /// MYSQL ������
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
