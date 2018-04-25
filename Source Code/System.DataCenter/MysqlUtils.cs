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

        #region 公共方法
        /// <summary>
        /// 联接数据库
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
        /// 执行 SQl 语句 -- 部分不支持事务处理
        /// </summary>
        /// <param name="strQuery">执行的 SQL 语句</param>
        public void ExecuteQuery(string strQuery)
        {
            this.pCommand = new MySqlCommand(strQuery, this.pConnection);
            this.pCommand.CommandTimeout = 600000;
            this.pCommand.CommandType = CommandType.Text;
        }

        /// <summary>
        /// 获取记录集
        /// </summary>
        /// <param name="eRecordStyle">记录集样式</param>
        public void GetResult(RecordStyle eRecordStyle)
        {
            DataSet pDataSet = new DataSet();
            MySqlDataAdapter pDataAdapter = null;

            try
            {
                //枚举纪录集
                switch (eRecordStyle)
                {
                    #region 影响行数
                    case RecordStyle.NONE:
                        this.iAffectRow = this.pCommand.ExecuteNonQuery();
                        break;
                    #endregion                    
                    #region DataSet 式数据集
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

        #region 属性
        /// <summary>
        /// 影响行数
        /// </summary>
        public int AffectRow
        {
            get
            {
                return this.iAffectRow;
            }
        }

        /// <summary>
        /// 数据集
        /// </summary>
        public object Data
        {
            get
            {
                return this.pDataResult;
            }
        }


        /// <summary>
        /// 错误信息
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
