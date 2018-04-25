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
    /// MYSQL处理方法
    /// </summary>
    public class MYSQLOperate : DataOperate, IMYSQLOperate
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
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
        /// 释放资源
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

        #region 方法定义
        #region 回调方法
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

        #region 处理方法
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

                            pRealTimeData.Add("暂无数据");

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

        #region 私有字段
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

            //设置执行超时时间
            if (this.iTimeOut > 0)
            {
                this.pCommand.CommandTimeout = this.iTimeOut;
            }

            //填充参数
            foreach (MySqlParameter pParameter in pParams)
            {
                this.pCommand.Parameters.Add(pParameter);
            }

            //是否包含返回值
            if (bReturn)
            {
                this.pReturnParam = new MySqlParameter();
                this.pReturnParam.Direction = ParameterDirection.ReturnValue;
                this.pCommand.Parameters.Add(this.pReturnParam);
            }

            //是否执行事务
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
                    #region 影响行数
                    case RecordStyle.NONE:
                        this.iAffectRow = this.pCommand.ExecuteNonQuery();
                        break;
                    #endregion
                    #region 自定义结构 --
                    case RecordStyle.STRUCT:
                        throw new ArgumentException("获取记录集失败：", "不是有效的记录集");
                    #endregion
                    #region DataSet结果集
                    case RecordStyle.DATASET:
                        MySqlDataAdapter pDataAdapter = new MySqlDataAdapter(this.pCommand);
                        pDataAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pExecuteResult = pDataSet;

                        //pDataSet.Clear();
                        break;
                    #endregion
                    #region DataTable结果集
                    case RecordStyle.DATATABLE:
                        MySqlDataAdapter pTableAdapter = new MySqlDataAdapter(this.pCommand);
                        pTableAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pExecuteResult = DataUtilities.MergeTable(pDataSet);

                        //pDataSet.Clear();
                        break;
                    #endregion
                    #region XML数据流
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
                    #region 自定义数据文件
                    case RecordStyle.DATAFILE:
                        //this.pCommand.BeginExecuteReader(new AsyncCallback(DataFileCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region 实时数据
                    case RecordStyle.REALTIME:
                        //this.pCommand.BeginExecuteReader(new AsyncCallback(RealTimeCallback), new RecordDataCallback(this.SocketConnection, this.eService, this.pCommand), CommandBehavior.CloseConnection);
                        break;
                    #endregion
                    #region 未知结果样式
                    default:
                        throw new ArgumentException("获取记录集失败：", "不是有效的记录集");
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

        #region 强制转换
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

        #region SQL PLUS方法
        #region 获取数据库架构
        /// <summary>
        /// 获取数据库列表
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
        /// 获取数据库结构－表、视图、触发器、存储过程
        /// </summary>
        public override void GetDatabase_Structure(CustomData[,] mContent)
        {
            //采用组合方式
            CustomDataCollection _Result = new CustomDataCollection(StructType.CUSTOMDATA);

            #region 绑定数据表
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

            #region 绑定表字段
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

            #region 绑定主键
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

            #region 绑定唯一行约束
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

            #region 绑定Default约束-暂时没有
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

            #region 绑定Check约束
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

            #region 绑定存储过程
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

            #region 绑定函数
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

            #region 绑定触发器/触发器内容
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

            #region 绑定数据表
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

            #region 绑定视图
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

            #region 绑定存储过程
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

            #region 绑定函数
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

            #region 绑定表字段
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

            #region 绑定主键
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

            #region 绑定唯一行约束
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

            #region 绑定Default约束-暂时没有
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

            #region 绑定Check约束
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

            #region 绑定触发器/触发器内容
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

            #region 绑定表字段
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
        /// 获取数据类型
        /// </summary>
        public override void GetData_Type()
        {
            
        }

        /// <summary>
        /// 获取二进制排序规则
        /// </summary>
        public override void GetCollation()
        {
            
        }
        #endregion

        #region 表－操作
        /// <summary>
        /// 表－添加新列
        /// </summary>
        public override string Alter_Table_AddColumn(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " ADD " + mContent[i, 9].Content.ToString() + " " + mContent[i, 11].Content.ToString();

            if ("N/A" != mContent[i, 12].Content.ToString())//数据精度
            {
                Str_Sql += " " + mContent[i, 12].Content.ToString();
            }

            if ("N/A" != mContent[i, 14].Content.ToString())//排序规则
            {
                Str_Sql += " COLLATE " + mContent[i, 14].Content.ToString();
            }

            if ("N/A" != mContent[i, 13].Content.ToString())//是否为空
            {
                Str_Sql += " " + mContent[i, 13].Content.ToString();
            }

            if ("" != mContent[i, 15].Content.ToString())//约束
            {
                Str_Sql += " CONSTRAINT " + mContent[i, 15].Content.ToString();
            }

            if ("" != mContent[i, 16].Content.ToString())//默认值
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
        /// 表－删除操作
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_Drop(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE '" + mContent[i, 6].Content.ToString() + "' DROP ";
            if (mContent[i, 8].Content.ToString() == "0")//字段
            {
                Str_Sql = Str_Sql + "COLUMN" + mContent[i, 9].Content.ToString();
            }
            else//约束、主外键
            {
                
            }

            return Str_Sql;
        }

        /// <summary>
        /// 表－修改操作
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_Alter(CustomData[,] mContent, int i)
        {
            string Str_Sql = string.Empty;
            string str_columnname = mContent[i, 9].Content.ToString();
            if (mContent[i, 10].Content.ToString() != "N/A")//重命名
            {
                Str_Sql = "EXEC sp_rename '[" + mContent[i, 5].Content.ToString() + "].[" + mContent[i, 6].Content.ToString() + "].[" + mContent[i, 9].Content.ToString() + "]', '" + mContent[i, 10].Content.ToString() + "', 'COLUMN';";
                str_columnname = mContent[i, 10].Content.ToString();
            }
            if (mContent[i, 9].Content.ToString() != "N/A")
            {
                Str_Sql += SqlConstant.Str_AlterTable_Alter;
                Str_Sql = Str_Sql.Replace("column_name{", str_columnname);//列名
                Str_Sql = Str_Sql.Replace("[ type_schema_name. ] type_name", mContent[i, 11].Content.ToString());//数据类型

                string a = mContent[i, 12].Content.ToString() == "N/A" ? "" : mContent[i, 12].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ ( { precision [ , scale ] | max | xml_schema_collection } ) ]", a);//数据精度

                string b = mContent[i, 13].Content.ToString() == "N/A" ? "" : mContent[i, 13].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ NULL | NOT NULL ]", b);//是否为空

                string c = mContent[i, 14].Content.ToString() == "N/A" ? "" : mContent[i, 14].Content.ToString();
                Str_Sql = Str_Sql.Replace("[ COLLATE collation_name ]", c);//排序规则

                Str_Sql = Str_Sql.Replace("| {ADD | DROP } { ROWGUIDCOL | PERSISTED }}}", "");

                Str_Sql = Str_Sql.Replace("[ database_name ] table_name{", mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString());
            }
            return Str_Sql;

            Str_Sql = "ALTER TABLE " + mContent[i, 6].Content.ToString() + " CHANGE " + mContent[i,9].Content.ToString() + " " + mContent[0,10].Content.ToString();
        }

        /// <summary>
        /// 表－添加主键
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddPrimaryKey(CustomData[,] mContent, int i)
        {
            //ALTER TABLE 'test_b' MODIFY COLUMN 'a' INTEGER(11) NOT NULL PRIMARY KEY
            string Str_Sql = "ALTER TABLE " + mContent[i, 6].Content.ToString() + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " PRIMARY KEY ([" + mContent[i, 9].Content.ToString() + "])";
            return Str_Sql;
        }

        /// <summary>
        /// 表－添加外键
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
        /// 表－添加默认值
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddDefault(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString();
            Str_Sql += " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " DEFAULT " + mContent[i, 16].Content.ToString() + " FOR " + mContent[i, 9].Content.ToString();
            return Str_Sql;
        }

        /// <summary>
        /// 表－添加CHECK约束
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
        /// 表－添加Unique约束
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddUnique(CustomData[,] mContent, int i)
        {
            string Str_Sql = "ALTER TABLE " + mContent[i, 5].Content.ToString() + "." + mContent[i, 6].Content.ToString() + " ADD CONSTRAINT " + mContent[i, 15].Content.ToString() + " UNIQUE ([" + mContent[i, 9].Content.ToString() + "])";
            return Str_Sql;
        }

        /// <summary>
        /// 表－添加扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_AddExtend(CustomData[,] mContent, int i)
        {
            string Str_Sql = "EXEC sp_addextendedproperty 'MS_Description', '" + mContent[i, 23].Content.ToString() + "', 'schema', 'dbo', 'table', '" + mContent[i, 6].Content.ToString() + "', 'column', '" + mContent[i, 9].Content.ToString() + "'";
            return Str_Sql;
        }

        /// <summary>
        /// 表－修改扩展属性
        /// </summary>
        /// <param name="mContent"></param>
        public override string Alter_Table_ModExtend(CustomData[,] mContent, int i)
        {
            string Str_Sql = "EXEC sp_updateextendedproperty 'MS_Description', '" + mContent[i, 23].Content.ToString() + "', 'schema', 'dbo', 'table', '" + mContent[i, 6].Content.ToString() + "', 'column', '" + mContent[i, 9].Content.ToString() + "'";
            return Str_Sql;
        }

        /// <summary>
        /// 表－删除扩展属性
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
        /// 重命名
        /// </summary>
        /// <param name="mContent"></param>
        public override string Rename(CustomData[,] mContent)
        {
            string Str_Sql = "ALTER TABLE '" + mContent[0, 5].Content.ToString() + "' RENAME AS  '" + mContent[0, 6].Content.ToString() ;
            return Str_Sql;
        }

        /// <summary>
        /// 删除
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

        #region 组合中的单个方法
        /// <summary>
        /// 获取MySql表名的列表
        /// </summary>
        public void GetTable_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetTable;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// 获取MySql视图名的列表
        /// </summary>
        public void GetView_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetView;
            Str_Sql = Str_Sql.Replace("$TABLE_SCHEMA", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// 获取MySql表字段
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
        /// 获取表的主键
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
        /// 获取表的唯一性约束
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
        /// 外键
        /// </summary>
        public void GetForeignKey_MySql(CustomData[,] mContent)
        {
        }

        /// <summary>
        /// Default约束
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
        /// 获取Check约束
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
        /// 存储过程
        /// </summary>
        public void GetProcedure_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetProcedure;
            Str_Sql = Str_Sql.Replace("$DbName", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// 函数
        /// </summary>
        public void GetFunction_MySql(CustomData[,] mContent)
        {
            string Str_Sql = MYSQL_SqlConstant.Str_GetFunction;
            Str_Sql = Str_Sql.Replace("$DbName", mContent[0, 4].Content.ToString());
            this.ExecuteQuery(Str_Sql);
            this.GetResult(RecordStyle.DATATABLE);
        }

        /// <summary>
        /// 触发器
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
