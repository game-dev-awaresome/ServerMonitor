using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Threading;

using System.Global;
using System.Define;
using System.DataPacket;
using Windows.Network;
using MySql.Data.MySqlClient;

namespace System.DataCenter
{
    /// <summary>
    /// MYSQL������
    /// </summary>
    public class MYSQLOperate : DataOperate, IMYSQLOperate
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="strConnect"></param>
        public MYSQLOperate(string strConnect)
        {
            this._strConnect = strConnect;

            this.iAffectRow = -1;
            this.iTimeOut = 0;
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
                MySqlCommand pCallBackCmd = null;
                MySqlDataReader pCallBackReader = null;
                TaskService pCallService = TaskService.MESSAGE;

                IAsyncResult AsyncResult = (IAsyncResult)state;

                try
                {
                    byte[] pSendBuffer;
                    SocketPacket pSendPacket = null;
                    CustomDataCollection pRealTimeData = null;

                    RecordDataCallback pRealTimeCallback = (RecordDataCallback)AsyncResult.AsyncState;

                    pCallService = pRealTimeCallback.Service;
                    pSendChannels = pRealTimeCallback.Channels;
                    pCallBackCmd = (MySqlCommand)pRealTimeCallback.Command;

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
                catch (MySqlException ex)
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
                    }
                }
            }
        }
        #endregion
        #endregion

        #region ˽���ֶ�
        private bool _bTrans;
        private string _strConnect;

        private int iAffectRow;
        private int iTimeOut;
        private string strErrorMsg;
        private string strTransName;
        private object pExecuteResult;

        private MySqlConnection pConnection;
        private MySqlTransaction pTranscation;
        private MySqlCommand pCommand;
        private MySqlParameter pReturnParam;
        private TaskService eService;
        private ISocketConnection SocketConnection;
        #endregion

        #region IMYSQLOperate Members
        //public TaskService Service
        //{
        //    get
        //    {
        //        return this.eService;
        //    }
        //    set
        //    {
        //        this.eService = value;
        //    }
        //}

        //public ISocketConnection Channels
        //{
        //    get
        //    {
        //        return this.SocketConnection;
        //    }
        //    set
        //    {
        //        this.SocketConnection = value;
        //    }
        //}
        #endregion

        #region IDataOperate Members
        public override bool Connect(bool bUsedTrans)
        {
            bool bResult = false;
            this._bTrans = bUsedTrans;

            try
            {
                this.pConnection = new MySqlConnection(this._strConnect);
                this.pConnection.Open();

                if (this._bTrans)
                {
                    //Random pRandom = new Random();
                    //this.strTransName = pRandom.Next().ToString();

                    //this.pTranscation = this.pConnection.BeginTransaction(this.strTransName);
                }

                bResult = true;
            }
            catch (Exception ex)
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
            //try
            //{
            //    if (this._bTrans)
            //    {
            //        if (bTransCommit)
            //        {
            //            this.pTranscation.Commit();
            //        }
            //        else
            //        {
            //            this.pTranscation.Rollback(this.strTransName);
            //        }
            //    }
            //}
            //catch (MySqlException ex)
            //{
            //    this.pTranscation.Rollback(this.strTransName);

            //    this.iAffectRow = -1;
            //    this.strErrorMsg = ex.Message;
            //}
        }

        public override void ExecuteQuery(string strQuery)
        {
            this.pCommand = new MySqlCommand(strQuery, this.pConnection);
            //this.pCommand.CommandType = CommandType.Text;

            if (this.iTimeOut > 0)
            {
                //this.pCommand.CommandTimeout = this.iTimeOut;
            }

            if (this._bTrans)
            {
                //this.pCommand.Transaction = this.pTranscation;
            }

            //this.pCommand.Prepare();
        }

        public override void ExecuteQuery(bool bReturn, string strProc, DbParameter[] pParams)
        {
            this.pCommand = new MySqlCommand(strProc, pConnection);
            this.pCommand.CommandType = CommandType.StoredProcedure;

            //����ִ�г�ʱʱ��
            if (this.iTimeOut > 0)
            {
                this.pCommand.CommandTimeout = this.iTimeOut;
            }

            //������
            foreach (MySqlParameter pParameter in pParams)
            {
                this.pCommand.Parameters.Add(pParameter);
            }

            //�Ƿ��������ֵ
            if (bReturn)
            {
                this.pReturnParam = new MySqlParameter();
                this.pReturnParam.Direction = ParameterDirection.ReturnValue;
                this.pCommand.Parameters.Add(this.pReturnParam);
            }

            //�Ƿ�ִ������
            if (this._bTrans)
            {
                this.pCommand.Transaction = this.pTranscation;
            }

            this.pCommand.Prepare();
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
                        MySqlDataAdapter pDataAdapter = new MySqlDataAdapter(this.pCommand);
                        pDataAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pExecuteResult = pDataSet;

                        //pDataSet.Clear();
                        break;
                    #endregion
                    #region DataTable�����
                    case RecordStyle.DATATABLE:
                        MySqlDataAdapter pTableAdapter = new MySqlDataAdapter(this.pCommand);
                        pTableAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pExecuteResult = DataUtilities.MergeTable(pDataSet);

                        //pDataSet.Clear();
                        break;
                    #endregion
                    #region XML������
                    case RecordStyle.XML:
                        MemoryStream pXMLStream = new MemoryStream();

                        MySqlDataReader pXMLReader = this.pCommand.ExecuteReader(CommandBehavior.CloseConnection);

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
                        //this.pCommand.BeginExecuteReader(new AsyncCallback(DataFileCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region ʵʱ����
                    case RecordStyle.REALTIME:
                        //this.pCommand.BeginExecuteReader(new AsyncCallback(RealTimeCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region δ֪�����ʽ
                    default:
                        throw new ArgumentException("��ȡ��¼��ʧ�ܣ�", "������Ч�ļ�¼��");
                    #endregion
                }
            }
            #region
            catch (InvalidOperationException ex)
            {
                this.iAffectRow = -1;
                this.strErrorMsg = ex.Message;
            }
            catch (MySqlException ex)
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
            }
            #endregion
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

        #region ǿ��ת��
        public override IMSSQLOperate SQLServer()
        {
            return null;
        }

        public override IMYSQLOperate MYServer()
        {
            return (this as IMYSQLOperate);
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
            //string Str_Pro = MYSQL_SqlConstant.Str_GetDatabase;
            //this.ExecuteQuery(Str_Pro);
            //this.GetResult(RecordStyle.DATASET);
            
            DataTable dt = this.pConnection.GetSchema("DataBases");
            dt.Columns.Remove("CATALOG_NAME");
            DataTable b = new DataTable();
            b.Columns.Add("DB_Name");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                object[] a = new object[1]; a[0] = dr[0].ToString();
                b.Rows.Add(a);
            }
            this.pExecuteResult = b;
            this.iAffectRow = this.pConnection.GetSchema("DataBases").Rows.Count;
        }

        /// <summary>
        /// ��ȡ���ݿ�ṹ������ͼ�����������洢����
        /// </summary>
        public override void GetDatabase_Structure(CustomData[,] mContent)
        {
            //������Ϸ�ʽ
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �����ݱ�
            GetTable_MySql(mContent);
            DataTable Table_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Table_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, Table_List.Rows[i]["NAME_KIND"].ToString());
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Table_List.Rows[i]["Table_Name"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Table_List.Rows[i]["CREATE_TIME"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Table_List.Rows[i]["UPDATE_TIME"]);
                _Result.AddRows();
            }
            #endregion

            #region �󶨱��ֶ�
            GetColumn_MySql(mContent);
            DataTable Column_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Column_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Column_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "F");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Column_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, Column_List.Rows[i]["ORDINAL_POSITION"].ToString());
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, Column_List.Rows[i]["COLUMN_DEFAULT"].ToString());
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, Column_List.Rows[i]["IS_NULLABLE"].ToString());
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, Column_List.Rows[i]["DATA_TYPE"].ToString());
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString());
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_OCTET_LENGTH"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_PRECISION"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_SCALE"].ToString());
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Column_List.Rows[i]["COLLATION_NAME"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ������
            GetKey_MySql(mContent);
            DataTable Key_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Key_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Key_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, Key_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "K");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Key_List.Rows[i]["INDEX_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "PRIMARY KEY");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ��Ψһ��Լ��
            GetUQKey_MySql(mContent);
            DataTable UQKey_List = (DataTable)this.pExecuteResult;
                        
            for (int i = 0; i < UQKey_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, UQKey_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, UQKey_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "K");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, UQKey_List.Rows[i]["CONSTRAINT_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "UNIQUE");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ��DefaultԼ��-��ʱû��
            //GetDefault_MySql(mContent);
            //DataTable Default_List = (DataTable)this.pExecuteResult;

            //for (int i = 0; i < Column_List.Rows.Count; i++)
            //{
            //    _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Default_List.Rows[i]["TABLE_NAME"]);
            //    _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
            //    _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, Default_List.Rows[i]["COLUMN_NAME"]);
            //    _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, Default_List.Rows[i]["DEFAULT_VALUE"]);
            //    _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "D");
            //    _Result.Add(DataField.NAME_ID, DataFormat.STRING, Default_List.Rows[i]["CONSTRAINT_NAME"]);
            //    _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "DEFAULT");
            //    _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
            //    _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
            //    _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
            //    _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
            //    _Result.AddRows();
            //}
            #endregion

            #region ��CheckԼ��
            GetCheck_MySql(mContent);
            DataTable Check_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Check_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Check_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "C");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Check_List.Rows[i]["CONSTRAINT_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "CHECK");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region �󶨴洢����
            GetProcedure_MySql(mContent);
            DataTable Procedure_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Procedure_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "P");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Procedure_List.Rows[i]["SPECIFIC_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Procedure_List.Rows[i]["ROUTINE_DEFINITION"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Procedure_List.Rows[i]["CREATED"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Procedure_List.Rows[i]["LAST_ALTERED"]);
                _Result.AddRows();
            }
            #endregion

            #region �󶨺���
            GetFunction_MySql(mContent);
            DataTable Function_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Function_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "FC");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Function_List.Rows[i]["SPECIFIC_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Function_List.Rows[i]["ROUTINE_DEFINITION"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Function_List.Rows[i]["CREATED"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Function_List.Rows[i]["LAST_ALTERED"]);
                _Result.AddRows();
            }
            #endregion

            #region �󶨴�����/����������
            GetTrigger_MySql(mContent);
            DataTable Trigger_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Trigger_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Trigger_List.Rows[i]["EVENT_OBJECT_TABLE"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "TR");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Trigger_List.Rows[i]["TRIGGER_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }

            for (int i = 0; i < Trigger_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Trigger_List.Rows[i]["EVENT_OBJECT_TABLE"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "TR_TEXT");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Trigger_List.Rows[i]["TRIGGER_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Trigger_List.Rows[i]["ACTION_STATEMENT"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Table_List(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �����ݱ�
            GetTable_MySql(mContent);
            DataTable Table_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Table_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, Table_List.Rows[i]["NAME_KIND"].ToString());
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Table_List.Rows[i]["Table_Name"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Table_List.Rows[i]["CREATE_TIME"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Table_List.Rows[i]["UPDATE_TIME"]);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_View_List(CustomData[,] mContent)
        {            
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region ����ͼ
            GetView_MySql(mContent);
            DataTable Table_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Table_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, Table_List.Rows[i]["NAME_KIND"].ToString());
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Table_List.Rows[i]["Table_Name"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Table_List.Rows[i]["CREATE_TIME"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Table_List.Rows[i]["UPDATE_TIME"]);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Proc_List(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �󶨴洢����
            GetProcedure_MySql(mContent);
            DataTable Procedure_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Procedure_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "P");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Procedure_List.Rows[i]["SPECIFIC_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Procedure_List.Rows[i]["ROUTINE_DEFINITION"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Procedure_List.Rows[i]["CREATED"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Procedure_List.Rows[i]["LAST_ALTERED"]);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Function_List(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �󶨺���
            GetFunction_MySql(mContent);
            DataTable Function_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Function_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "FC");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Function_List.Rows[i]["SPECIFIC_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Function_List.Rows[i]["ROUTINE_DEFINITION"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, Function_List.Rows[i]["CREATED"]);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, Function_List.Rows[i]["LAST_ALTERED"]);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Table(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �󶨱��ֶ�
            GetColumn_MySql(mContent);
            DataTable Column_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Column_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Column_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "F");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Column_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, Column_List.Rows[i]["ORDINAL_POSITION"].ToString());
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, Column_List.Rows[i]["COLUMN_DEFAULT"].ToString());
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, Column_List.Rows[i]["IS_NULLABLE"].ToString());
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, Column_List.Rows[i]["DATA_TYPE"].ToString());
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString());
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_OCTET_LENGTH"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_PRECISION"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_SCALE"].ToString());
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Column_List.Rows[i]["COLLATION_NAME"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ������
            GetKey_MySql(mContent);
            DataTable Key_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Key_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Key_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, Key_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "K");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Key_List.Rows[i]["INDEX_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "PRIMARY KEY");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ��Ψһ��Լ��
            GetUQKey_MySql(mContent);
            DataTable UQKey_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < UQKey_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, UQKey_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, UQKey_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "K");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, UQKey_List.Rows[i]["CONSTRAINT_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "UNIQUE");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region ��DefaultԼ��-��ʱû��
            //GetDefault_MySql(mContent);
            //DataTable Default_List = (DataTable)this.pExecuteResult;

            //for (int i = 0; i < Column_List.Rows.Count; i++)
            //{
            //    _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Default_List.Rows[i]["TABLE_NAME"]);
            //    _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
            //    _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, Default_List.Rows[i]["COLUMN_NAME"]);
            //    _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, Default_List.Rows[i]["DEFAULT_VALUE"]);
            //    _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "D");
            //    _Result.Add(DataField.NAME_ID, DataFormat.STRING, Default_List.Rows[i]["CONSTRAINT_NAME"]);
            //    _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "DEFAULT");
            //    _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
            //    _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
            //    _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
            //    _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
            //    _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
            //    _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
            //    _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
            //    _Result.AddRows();
            //}
            #endregion

            #region ��CheckԼ��
            GetCheck_MySql(mContent);
            DataTable Check_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Check_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Check_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "C");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Check_List.Rows[i]["CONSTRAINT_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "CHECK");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            #region �󶨴�����/����������
            GetTrigger_MySql(mContent);
            DataTable Trigger_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Trigger_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Trigger_List.Rows[i]["EVENT_OBJECT_TABLE"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "TR");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Trigger_List.Rows[i]["TRIGGER_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }

            for (int i = 0; i < Trigger_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Trigger_List.Rows[i]["EVENT_OBJECT_TABLE"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "TR_TEXT");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Trigger_List.Rows[i]["TRIGGER_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, "N/A");
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, "0");
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Trigger_List.Rows[i]["ACTION_STATEMENT"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_View(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region �󶨱��ֶ�
            GetColumn_MySql(mContent);
            DataTable Column_List = (DataTable)this.pExecuteResult;

            for (int i = 0; i < Column_List.Rows.Count; i++)
            {
                _Result.Add(DataField.NAMES_Layer, DataFormat.STRING, Column_List.Rows[i]["TABLE_NAME"].ToString());
                _Result.Add(DataField.IS_IDENTITY, DataFormat.STRING, "0");
                _Result.Add(DataField.SEED_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.INCREMENT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.COLUMN_NAME, DataFormat.STRING, "N/A");
                _Result.Add(DataField.DEFAULT_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.NAME_KIND, DataFormat.STRING, "F");
                _Result.Add(DataField.NAME_ID, DataFormat.STRING, Column_List.Rows[i]["COLUMN_NAME"].ToString());
                _Result.Add(DataField.CONSTRAINT_TYPE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.ORDINAL_POSITION, DataFormat.STRING, Column_List.Rows[i]["ORDINAL_POSITION"].ToString());
                _Result.Add(DataField.COLUMN_DEFAULT, DataFormat.STRING, Column_List.Rows[i]["COLUMN_DEFAULT"].ToString());
                _Result.Add(DataField.IS_NULLABLE, DataFormat.STRING, Column_List.Rows[i]["IS_NULLABLE"].ToString());
                _Result.Add(DataField.DATA_TYPE, DataFormat.STRING, Column_List.Rows[i]["DATA_TYPE"].ToString());
                _Result.Add(DataField.CHARACTER_MAXIMUM_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_MAXIMUM_LENGTH"].ToString());
                _Result.Add(DataField.CHARACTER_OCTET_LENGTH, DataFormat.STRING, Column_List.Rows[i]["CHARACTER_OCTET_LENGTH"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_PRECISION"].ToString());
                _Result.Add(DataField.NUMERIC_PRECISION_RADIX, DataFormat.STRING, "0");
                _Result.Add(DataField.NUMERIC_SCALE, DataFormat.STRING, Column_List.Rows[i]["NUMERIC_SCALE"].ToString());
                _Result.Add(DataField.DATETIME_PRECISION, DataFormat.STRING, "0");
                _Result.Add(DataField.COLLATION_NAME, DataFormat.STRING, Column_List.Rows[i]["COLLATION_NAME"].ToString());
                _Result.Add(DataField.EXTEND_VALUE, DataFormat.STRING, "N/A");
                _Result.Add(DataField.CREATE_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.Add(DataField.ALTER_DATA, DataFormat.DATETIME, DateTime.Now);
                _Result.AddRows();
            }
            #endregion

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Proc(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            this.pExecuteResult = _Result;
        }

        public override void GetDatabase_Function(CustomData[,] mContent)
        {
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            this.pExecuteResult = _Result;
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        public override void GetData_Type()
        {
            
        }

        /// <summary>
        /// ��ȡ�������������
        /// </summary>
        public override void GetCollation()
        {
            
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
            string Str_Sql = "ALTER TABLE '" + mContent[i, 6].Content.ToString() + "' DROP ";
            if (mContent[i, 8].Content.ToString() == "0")//�ֶ�
            {
                Str_Sql = Str_Sql + "COLUMN" + mContent[i, 9].Content.ToString();
            }
            else//Լ���������
            {
                
            }

            return Str_Sql;
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

            Str_Sql = "ALTER TABLE " + mContent[i, 6].Content.ToString() + " CHANGE " + mContent[i,9].Content.ToString() + " " + mContent[0,10].Content.ToString();
        }

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i)
        {
            //ALTER TABLE 'test_b' MODIFY COLUMN 'a' INTEGER(11) NOT NULL PRIMARY KEY
            string Str_Sql = "ALTER TABLE " + mContent[i, 6].Content.ToString() + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " PRIMARY KEY ([" + mContent[i, 9].Content.ToString() + "])";
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
            string Str_Sql = "ALTER TABLE '" + mContent[0, 5].Content.ToString() + "' RENAME AS  '" + mContent[0, 6].Content.ToString() ;
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

        #region ����еĵ�������
        /// <summary>
        /// ��ȡMySql�������б�
        /// </summary>
        public void GetTable_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetTable;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡMySql��ͼ�����б�
        /// </summary>
        public void GetView_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetView;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡMySql���ֶ�
        /// </summary>
        public void GetColumn_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetColumn;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡ�������
        /// </summary>
        public void GetKey_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetKey;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡ���Ψһ��Լ��
        /// </summary>
        public void GetUQKey_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetUQKey;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ���
        /// </summary>
        public void GetForeignKey_MySql(CustomData[,] mContent)
        {
        }

        /// <summary>
        /// DefaultԼ��
        /// </summary>
        public void GetDefault_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetDefault;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ��ȡCheckԼ��
        /// </summary>
        public void GetCheck_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetCheck;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            Str_Sql = Str_Sql.Replace("$table_name", mContent[0, 7].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// �洢����
        /// </summary>
        public void GetProcedure_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetProcedure;
            Str_Sql = Str_Sql.Replace("$DbName", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ����
        /// </summary>
        public void GetFunction_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetFunction;
            Str_Sql = Str_Sql.Replace("$DbName", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// ������
        /// </summary>
        public void GetTrigger_MySql(CustomData[,] mContent)
        {
            //string Str_Sql = MYSQL_SqlConstant.Str_GetTrigger;
            //Str_Sql = Str_Sql.Replace("$DbName", mContent[0, 4].Content.ToString());
            //this.ExecuteQuery(Str_Sql);
            //this.GetResult(RecordStyle.DATATABLE);
        }
        #endregion
    }
}
