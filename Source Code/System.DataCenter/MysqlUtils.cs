using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

using System.Global;

namespace System.DataCenter
{
    public class MysqlUtils
    {
        public MysqlUtils(string strConn)
        {
            this._strConn = strConn;
        }

        #region ��������
        /// <summary>
        /// �������ݿ�
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            bool bResult = false;

            try
            {
                this.pConnection = new MySqlConnection(this._strConn);
                this.pConnection.Open();

                bResult = true;
            }
            catch (Exception ex)
            {
                this.strErrorMsg = ex.Message;

                bResult = false;
            }

            return bResult;
        }

        /// <summary>
        /// ִ�� SQl ��� -- ���ֲ�֧��������
        /// </summary>
        /// <param name="strQuery">ִ�е� SQL ���</param>
        public void ExecuteQuery(string strQuery)
        {
            this.pCommand = new MySqlCommand(strQuery, this.pConnection);
            this.pCommand.CommandTimeout = 600000;
            this.pCommand.CommandType = CommandType.Text;
        }

        /// <summary>
        /// ��ȡ��¼��
        /// </summary>
        /// <param name="eRecordStyle">��¼����ʽ</param>
        public void GetResult(RecordStyle eRecordStyle)
        {
            DataSet pDataSet = new DataSet();
            MySqlDataAdapter pDataAdapter = null;

            try
            {
                //ö�ټ�¼��
                switch (eRecordStyle)
                {
                    #region Ӱ������
                    case RecordStyle.NONE:
                        this.iAffectRow = this.pCommand.ExecuteNonQuery();
                        break;
                    #endregion                    
                    #region DataSet ʽ���ݼ�
                    case RecordStyle.DATASET:
                        pDataAdapter = new MySqlDataAdapter(pCommand);
                        pDataAdapter.Fill(pDataSet);

                        this.iAffectRow = pDataSet.Tables[0].Rows.Count;
                        this.pDataResult = pDataSet;
                        break;
                    #endregion
                }
            }
            catch (SqlException ex)
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
        #endregion

        #region ����
        /// <summary>
        /// Ӱ������
        /// </summary>
        public int AffectRow
        {
            get
            {
                return this.iAffectRow;
            }
        }

        /// <summary>
        /// ���ݼ�
        /// </summary>
        public object Data
        {
            get
            {
                return this.pDataResult;
            }
        }


        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Message
        {
            get
            {
                return this.strErrorMsg;
            }
        }
        #endregion

        private string _strConn;
        private int iAffectRow;
        private object pDataResult;
        private string strErrorMsg;
        private MySqlConnection pConnection;
        private MySqlCommand pCommand;
    }
}
