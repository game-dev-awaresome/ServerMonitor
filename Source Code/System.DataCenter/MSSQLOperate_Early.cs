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
    /// <summary>
    /// SQL ������
    /// </summary>
    public class MSSQLOperate_Early : DataOperate, IMSSQLOperate
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="strConnect"></param>
        public MSSQLOperate_Early(string strConnect)
        {
            this._strConnect = strConnect;

            this.iAffectRow = -1;
            this.iTimeOut = 3000;
            this.strErrorMsg = String.Empty;
            this.strTransName = String.Empty;
        }

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                if (null != this.pCommand)
                {
                    this.pCommand.Dispose();
                    this.pCommand = null;
                }

                if (null != this.pConnection)
                {
                    this.pConnection.Dispose();
                    this.pConnection = null;
                }

                this.iAffectRow = 0;
                this.iTimeOut = 0;
                this.strErrorMsg = string.Empty;
                this.strTransName = string.Empty;
                this.pExecuteResult = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region ˽���ֶ�
        private bool _bTrans;
        private string _strConnect;

        private int iAffectRow;
        private int iTimeOut;
        private string strErrorMsg;
        private string strTransName;
        private object pExecuteResult;

        private SqlConnection pConnection;
        private SqlTransaction pTranscation;
        private SqlCommand pCommand;
        private SqlParameter pReturnParam;
        private TaskService eService;
        private ISocketConnection SocketConnection;
        #endregion

        #region ��������
        #region �ص�����
        private void DataFileCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DataFileProcessing), ar);
            }
        }

        private void RealTimeCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RealTimeProcessing), ar);
            }
        }
        #endregion

        #region ������
        private void DataFileProcessing(object state)
        {

        }

        private void RealTimeProcessing(object state)
        {
            if (!this.Disposed)
            {
                ISocketConnection pSendChannels = null;
                SqlCommand pCallBackCmd = null;
                SqlDataReader pCallBackReader = null;
                TaskService pCallService = this.eService;//TaskService.MESSAGE;

                IAsyncResult AsyncResult = (IAsyncResult)state;

                try
                {
                    byte[] pSendBuffer;
                    SocketPacket pSendPacket = null;
                    CustomDataCollection pRealTimeData = null;

                    RecordDataCallback pRealTimeCallback = (RecordDataCallback)AsyncResult.AsyncState;

                    pCallService = pRealTimeCallback.Service;
                    pSendChannels = pRealTimeCallback.Channels;
                    pCallBackCmd = (SqlCommand)pRealTimeCallback.Command;

                    if (null != this.SocketConnection)
                    {
                        pCallBackReader = pCallBackCmd.EndExecuteReader(AsyncResult);

                        if (pCallBackReader.HasRows)
                        {
                            while (pCallBackReader.Read())
                            {
                                pRealTimeData = new CustomDataCollection(StructType.CUSTOMDATA);

                                for (int i = 0; i < pCallBackReader.FieldCount; i++)
                                {
                                    pRealTimeData.Add(DefineUtilities.ToDataField(pCallBackReader.GetName(i)), DefineUtilities.ToDataFormat(pCallBackReader.GetFieldType(i).Name), pCallBackReader[i]);
                                }

                                pRealTimeData.AddRows();

                                pSendPacket = new SocketPacket(pCallService, pRealTimeData);

                                pSendBuffer = pSendPacket.CoalitionInfo();
                                this.SocketConnection.OnSend(pSendBuffer);

                                pRealTimeData.Clear();
                                pSendPacket.Dispose();

                                pRealTimeData = null;
                                pSendBuffer = null;
                            }
                        }
                        else
                        {
                            pRealTimeData = new CustomDataCollection(StructType.CUSTOMDATA);

                            pRealTimeData.Add("��������");

                            pSendPacket = new SocketPacket(pCallService, pRealTimeData);

                            pSendBuffer = pSendPacket.CoalitionInfo();
                            this.SocketConnection.OnSend(pSendBuffer);

                            pRealTimeData.Clear();
                            pSendPacket.Dispose();

                            pRealTimeData = null;
                            pSendBuffer = null;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    this.strErrorMsg = ex.Message;
                }
                catch (Exception ex)
                {
                    this.strErrorMsg = ex.Message;
                }
                finally
                {
                    try
                    {
                        pCallBackReader.Close();

                        pCallBackReader.Dispose();
                        pCallBackCmd.Dispose();

                        pCallBackReader = null;
                        pCallBackCmd = null;
                    }
                    catch (Exception ex)
                    {
                        this.strErrorMsg = ex.Message;

                        CustomDataCollection pRealTimeData = new CustomDataCollection(StructType.CUSTOMDATA);

                        pRealTimeData.Add(strErrorMsg);

                        SocketPacket pSendPacket = new SocketPacket(pCallService, pRealTimeData);

                        byte[] pSendBuffer = pSendPacket.CoalitionInfo();
                        this.SocketConnection.OnSend(pSendBuffer);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region IDataOperate Members
        #region
        public override bool Connect(bool bUsedTrans)
        {
            bool bResult = false;
            this._bTrans = bUsedTrans;

            try
            {
                this.pConnection = new SqlConnection(this._strConnect);//pConnection.ServerVersion
                this.pConnection.Open();

                if (this._bTrans)
                {
                    Random pRandom = new Random();
                    this.strTransName = pRandom.Next().ToString();

                    this.pTranscation = this.pConnection.BeginTransaction(this.strTransName);
                }

                bResult = true;
            }
            catch (SqlException ex)
            {
                this.strErrorMsg = ex.Message;

                bResult = false;
            }

            return bResult;
        }

        public override void DisConnected()
        {
            if (this.pConnection.State == ConnectionState.Open)
            {
                this.pConnection.Close();
            }
        }

        public override void DoTrans(bool bTransCommit)
        {
            try
            {
                if (this._bTrans)
                {
                    if (bTransCommit)
                    {
                        this.pTranscation.Commit();
                    }
                    else
                    {
                        this.pTranscation.Rollback(this.strTransName);
                    }
                }
            }
            catch (SqlException ex)
            {
                this.pTranscation.Rollback(this.strTransName);

                this.iAffectRow = -1;
                this.strErrorMsg = ex.Message;
            }
        }

        public override void ExecuteQuery(string strQuery)
        {
            this.pCommand = new SqlCommand(strQuery, this.pConnection);
            this.pCommand.CommandType = CommandType.Text;

            if (this.iTimeOut > 0)
            {
                this.pCommand.CommandTimeout = this.iTimeOut;
            }

            if (this._bTrans)
            {
                this.pCommand.Transaction = this.pTranscation;
            }

            //this.pCommand.Prepare();
        }

        public override void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams)
        {
            this.pCommand = new SqlCommand(strProc, pConnection);
            this.pCommand.CommandType = CommandType.StoredProcedure;

            SqlParameter[] _ppp = (SqlParameter[])pParams;

            //����ִ�г�ʱʱ��
            if (this.iTimeOut > 0)
            {
                this.pCommand.CommandTimeout = this.iTimeOut;
            }

            //������
            try
            {
                foreach (SqlParameter pParameter in _ppp)
                {
                    this.pCommand.Parameters.Add(pParameter);
                }
            }
            catch (Exception we)
            {
            }

            //�Ƿ��������ֵ
            if (bReturn)
            {
                this.pReturnParam = new SqlParameter();
                this.pReturnParam.Direction = ParameterDirection.ReturnValue;
                this.pCommand.Parameters.Add(this.pReturnParam);
            }

            //�Ƿ�ִ������
            if (this._bTrans)
            {
                this.pCommand.Transaction = this.pTranscation;
            }

            //this.pCommand.Prepare();
        }

        public override void GetResult(RecordStyle eRecordStyle)
        {
            DataSet pDataSet = new DataSet();
            try
            {
                switch (eRecordStyle)
                {
                    #region Ӱ������
                    case RecordStyle.NONE:
                        this.iAffectRow = this.pCommand.ExecuteNonQuery();
                        break;
                    #endregion
                    #region �Զ���ṹ --
                    case RecordStyle.STRUCT:
                        throw new ArgumentException("��ȡ��¼��ʧ�ܣ�", "������Ч�ļ�¼��");
                    #endregion
                    #region DataSet�����
                    case RecordStyle.DATASET:
                        SqlDataAdapter pDataAdapter = new SqlDataAdapter(this.pCommand);
                        //pCommand.StatementCompleted += new StatementCompletedEventHandler(pCommand_StatementCompleted);
                        pDataAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pExecuteResult = pDataSet;

                        //pDataSet.Clear();                        
                        break;
                    #endregion
                    #region DataTable�����
                    case RecordStyle.DATATABLE:
                        SqlDataAdapter pTableAdapter = new SqlDataAdapter(this.pCommand);
                        pTableAdapter.Fill(pDataSet);

                        this.iAffectRow = 0;
                        this.pExecuteResult = DataUtilities.MergeTable(pDataSet);

                        pDataSet.Clear();
                        break;
                    #endregion
                    #region XML������
                    case RecordStyle.XML:
                        MemoryStream pXMLStream = new MemoryStream();

                        SqlDataReader pXMLReader = this.pCommand.ExecuteReader(CommandBehavior.CloseConnection);

                        DataTable pXMLTable = new DataTable();
                        pXMLTable.Load(pXMLReader, LoadOption.Upsert);
                        pXMLTable.WriteXml(pXMLStream);

                        this.iAffectRow = -1;
                        this.pExecuteResult = pXMLStream.GetBuffer();

                        pXMLStream.Close();
                        pXMLReader.Close();
                        break;
                    #endregion
                    #region �Զ��������ļ�
                    case RecordStyle.DATAFILE:
                        this.pCommand.BeginExecuteReader(new AsyncCallback(DataFileCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region ʵʱ����
                    case RecordStyle.REALTIME:
                        this.pCommand.BeginExecuteReader(new AsyncCallback(RealTimeCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region δ֪�����ʽ
                    default:
                        throw new ArgumentException("��ȡ��¼��ʧ�ܣ�", "������Ч�ļ�¼��");
                    #endregion
                }
            }
            catch (InvalidOperationException ex)
            {
                this.iAffectRow = -1;
                this.strErrorMsg = ex.Message;
            }
            catch (SqlException ex)
            {
                this.iAffectRow = -1;
                this.strErrorMsg = ex.Message;
            }
            catch (Exception ex)
            {
                this.iAffectRow = -1;
                this.strErrorMsg = ex.Message;
            }
            finally
            {
                pDataSet = null;
                this.pCommand.Dispose();
                this.pCommand = null;
            }
        }

        void pCommand_StatementCompleted(object sender, StatementCompletedEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            int affectRow = e.RecordCount;
        }

        public override int AffectRow
        {
            get
            {
                return this.iAffectRow;
            }
        }

        public override string Message
        {
            get
            {
                return this.strErrorMsg;
            }
        }

        public override string Version
        {
            get
            {
                return this.pConnection.ServerVersion;
            }
        }

        public override object RecordData
        {
            get
            {
                return this.pExecuteResult;
            }
        }

        public override int ExecuteTimeOut
        {
            get
            {
                return this.iTimeOut;
            }
            set
            {
                this.iTimeOut = value;
            }
        }

        public override object ProcReturn
        {
            get
            {
                return pReturnParam.Value;
            }
        }

        public override ISocketConnection Channels
        {
            get
            {
                return this.SocketConnection;
            }
            set
            {
                this.SocketConnection = value;
            }
        }

        public override TaskService Service
        {
            get
            {
                return this.eService;
            }
            set
            {
                this.eService = value;
            }
        }
        #endregion

        #region ǿ��ת��
        public override IMSSQLOperate SQLServer()
        {
            return (this as IMSSQLOperate);
        }

        public override IMYSQLOperate MYServer()
        {
            return null;
        }
        #endregion
        #endregion

        #region SQL PLUS����
        #region ��ȡ���ݿ�ܹ�
        /// <summary>
        /// ��ȡ���ݿ��б�
        /// </summary>
        public override void GetDatabase()
        {
            string Str_Pro = MSSQL_SqlConstant.Str_2000_GetDatabase;
            this.ExecuteQuery(Str_Pro);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡ���ݿ�ṹ������ͼ�����������洢����
        /// </summary>
        public override void GetDatabase_Structure(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_Structure;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Table_List(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetTableList;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_View_List(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetViewList;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Proc_List(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetProcList;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Function_List(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetFunctionList;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Table(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetTable;
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_View(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetView;
            Str_Sql = Str_Sql.Replace("$view_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Proc(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetProc;
            Str_Sql = Str_Sql.Replace("$proc_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        public override void GetDatabase_Function(CustomData[,] mContent)
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetFunction;
            Str_Sql = Str_Sql.Replace("$function_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        public override void GetData_Type()
        {
            string Str_Sql = MSSQL_SqlConstant.Str_2000_GetDatatype;
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }

        /// <summary>
        /// ��ȡ�������������
        /// </summary>
        public override void GetCollation()
        {
            string Str_Sql = "SELECT name AS COLLATION_NAME FROM :: fn_helpcollations()";
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATASET);
        }
        #endregion

        #region ������
        /// <summary>
        /// ���������
        /// </summary>        
        public override string Alter_Table_AddColumn(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " ADD " + mContent[i, 9].Content.ToString() + " " + mContent[i, 11].Content.ToString();

            if ("N/A" != mContent[i, 12].Content.ToString())//���ݾ���
            {
                Str_Sql += " " + mContent[i, 12].Content.ToString();
            }

            if ("N/A" != mContent[i, 14].Content.ToString())//�������
            {
                Str_Sql += " COLLATE " + mContent[i, 14].Content.ToString();
            }

            if ("N/A" != mContent[i, 13].Content.ToString())//�Ƿ�Ϊ��
            {
                Str_Sql += " " + mContent[i, 13].Content.ToString();
            }

            if ("" != mContent[i, 15].Content.ToString())//Լ��
            {
                Str_Sql += " CONSTRAINT " + mContent[i, 15].Content.ToString();
            }

            if ("" != mContent[i, 16].Content.ToString())//Ĭ��ֵ
            {
                Str_Sql += " DEFAULT " + mContent[i, 16].Content.ToString();
            }

            if ("N/A" != mContent[i, 17].Content.ToString())//WITH VALUES
            {
                Str_Sql += " WITH VALUES";
            }

            if ("N/A" != mContent[i, 22].Content.ToString())//IDENTITY
            {
                Str_Sql += " IDENTITY " + mContent[i, 22].Content.ToString();
            }

            if ("N/A" != mContent[i, 21].Content.ToString())
            {
                Str_Sql += " NOT FOR REPLICATION ";
            }

            return Str_Sql;

            //this.ExecuteQuery(Str_Sql);
            //this.GetResult(RecordStyle.NONE);
        }

        /// <summary>
        /// ��ɾ������
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_Drop(CustomData[,] mContent, int i)
        {
            string Str_Sql = SqlConstant.Str_AlterTable_DROP;
            if (mContent[i, 8].Content.ToString() == "0")//�ֶ�
            {
                Str_Sql = Str_Sql.Replace("[ CONSTRAINT ] constraint_name  [ WITH ( <drop_clustered_constraint_option> [ ,...n ] ) ]", "");
                Str_Sql = Str_Sql.Replace("| COLUMN column_name", " COLUMN " + mContent[i, 9].Content.ToString());
            }
            else//Լ���������
            {
                Str_Sql = Str_Sql.Replace("[ CONSTRAINT ] constraint_name  [ WITH ( <drop_clustered_constraint_option> [ ,...n ] ) ]", " CONSTRAINT " + mContent[i, 15].Content.ToString());
                Str_Sql = Str_Sql.Replace("| COLUMN column_name", "");
            }

            Str_Sql = Str_Sql.Replace("[ database_name ] table_name", mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString());

            return Str_Sql;
            //this.ExecuteQuery(Str_Sql);
            //this.GetResult(RecordStyle.NONE);
        }

        /// <summary>
        /// ���޸Ĳ���
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_Alter(CustomData[,] mContent, int i)
        {
            string Str_Sql = string.Empty;
            string str_columnname = mContent[i, 9].Content.ToString();
            if (mContent[i, 10].Content.ToString() != "N/A")//������
            {
                Str_Sql = "EXEC sp_rename '[" + mContent[i, 5].Content.ToString() + "].[" + mContent[i, 6].Content.ToString() + "].[" + mContent[i, 9].Content.ToString() + "]', '" + mContent[i, 10].Content.ToString() + "', 'COLUMN';";
                str_columnname = mContent[i, 10].Content.ToString();
            }
            if (mContent[i, 9].Content.ToString() != "N/A")
            {
                Str_Sql += SqlConstant.Str_AlterTable_Alter;
                Str_Sql = Str_Sql.Replace("column_name{", str_columnname);//����
                Str_Sql = Str_Sql.Replace("[ type_schema_name. ] type_name", mContent[i, 11].Content.ToString());//��������

                string a = mContent[i, 12].Content.ToString() == "N/A" ? "" : mContent[i, 12].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ ( { precision [ , scale ] | max | xml_schema_collection } ) ]", a);//���ݾ���

                string b = mContent[i, 13].Content.ToString() == "N/A" ? "" : mContent[i, 13].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ NULL | NOT NULL ]", b);//�Ƿ�Ϊ��

                string c = mContent[i, 14].Content.ToString() == "N/A" ? "" : mContent[i, 14].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ COLLATE collation_name ]", c);//�������

                Str_Sql = Str_Sql.Replace("| {ADD | DROP } { ROWGUIDCOL | PERSISTED }}}", "");

                Str_Sql = Str_Sql.Replace("[ database_name ] table_name{", mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString());
            }
            return Str_Sql;
        }

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " PRIMARY KEY ([" + mContent[i, 9].Content.ToString() + "])";
            return Str_Sql;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddForeignKey(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString();
            Str_Sql += " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " FOREIGN KEY ([" + mContent[i, 9].Content.ToString() + "])";
            Str_Sql += " REFERENCES " + mContent[i, 5].Content.ToString() + "." + mContent[i, 18].Content.ToString() + "(" + mContent[i, 19].Content.ToString() + ")";
            return Str_Sql;
        }

        /// <summary>
        /// �����Ĭ��ֵ
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddDefault(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString();
            Str_Sql += " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " DEFAULT " + mContent[i, 16].Content.ToString() + " FOR " + mContent[i, 9].Content.ToString();
            return Str_Sql;
        }

        /// <summary>
        /// �����CHECKԼ��
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddCheck(CustomData[,] mContent, int i)
        {
            string str_check = string.Empty;
            if ("N/A" != mContent[i, 20].Content.ToString())
            {
                str_check = " WITH NOCHECK ";
            }

            string str_replication = string.Empty;
            if ("N/A" != mContent[i, 21].Content.ToString())
            {
                str_replication = " NOT FOR REPLICATION ";
            }
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " " + str_check + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " CHECK " + str_replication + " (" + mContent[i, 24].Content.ToString() + ")";
            return Str_Sql;
        }

        /// <summary>
        /// �����UniqueԼ��
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddUnique(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " UNIQUE ([" + mContent[i, 9].Content.ToString() + "])";
            return Str_Sql;
        }

        /// <summary>
        /// �������չ����
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddExtend(CustomData[,] mContent, int i)
        {
            string Str_Sql = "EXEC sp_addextendedproperty 'MS_Description', '" + mContent[i, 23].Content.ToString() + "', 'schema', 'dbo', 'table', '" + mContent[i, 6].Content.ToString() + "', 'column', '" + mContent[i, 9].Content.ToString() + "'";
            return Str_Sql;
        }

        /// <summary>
        /// ���޸���չ����
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_ModExtend(CustomData[,] mContent, int i)
        {
            string Str_Sql = "EXEC sp_updateextendedproperty 'MS_Description', '" + mContent[i, 23].Content.ToString() + "', 'schema', 'dbo', 'table', '" + mContent[i, 6].Content.ToString() + "', 'column', '" + mContent[i, 9].Content.ToString() + "'";
            return Str_Sql;
        }

        /// <summary>
        /// ��ɾ����չ����
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_DelExtend(CustomData[,] mContent, int i)
        {
            string Str_Sql = "EXEC sp_dropextendedproperty 'MS_Description', 'schema', 'dbo', 'table', '" + mContent[i, 6].Content.ToString() + "', 'column', '" + mContent[i, 9].Content.ToString() + "'";
            return Str_Sql;
        }
        #endregion

        #region
        /// <summary>
        /// ������
        /// </summary>
        /// <param name="mContent"></param>
        public override string Rename(CustomData[,] mContent)
        {
            string Str_Sql = "EXEC sp_rename '" + mContent[0, 5].Content.ToString() + "', '" + mContent[0, 6].Content.ToString() + "', 'OBJECT'";
            return Str_Sql;
        }

        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="mContent"></param>
        /// <returns></returns>
        public override string Drop_Object(CustomData[,] mContent)
        {
            string Str_Sql = "DROP " + mContent[0, 6].Content.ToString() + " " + mContent[0, 5].Content.ToString() + "." + mContent[0, 7].Content.ToString();
            return Str_Sql;
        }
        #endregion
        #endregion
    }
}
