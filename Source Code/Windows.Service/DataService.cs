using System;
using System.Collections.Generic;
using System.Text;

using System.Global;
using System.DataCenter;
using System.Data.Common;

namespace Windows.Service
{
    public class DataService : CustomClass//, IDataService
    {
        #region
        public DataService(DataBase eDataBase, string strAddress, string strDataName, string strUserName, string strPassword)
        {
        
        }
        #endregion

        #region
        #endregion

        #region
        #endregion

        #region
        #endregion

        #region 变量定义
        private bool bUseTrans;
        private string strConnect;
        private string strMessage;
        private IDataOperate DataOperate;
        #endregion

        #region IDataService Members

        public void OnConnect()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnDisConnected()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnQuery(string strQuery)
        {
            this.DataCenter.ExecuteQuery(strQuery);
        }

        public void OnQuery(bool bReturn, string strProc, DbParameter[] pParams)
        {
            this.DataCenter.ExecuteQuery(bReturn, strProc, pParams);
        }

        public void OnResult(bool bAutoTrans, RecordStyle eRecordStyle)
        {
            try
            {
                this.DataCenter.GetResult(eRecordStyle);

                if (this.bUseTrans)
                {
                    this.DataCenter.DoTrans(true);
                }
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;
            }
        }

        public string Message
        {
            get
            {
                return this.strMessage;
            }
        }

        public IDataOperate DataCenter
        {
            get
            {
                return this.DataOperate;
            }
        }
        #endregion
    }
}
