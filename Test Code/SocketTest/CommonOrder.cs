using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Security.Cryptography;
using System.Data.Common;

using System.Global;
using System.Define;
using System.DataPacket;
using Windows.Network;
using System.DataCenter;


namespace SocketTest
{
    /// <summary>
    /// ��ȡָ��������Ȩ��
    /// </summary>
    public class CommonOrder
    {
        #region ���캯��
        public CommonOrder(int groupid, int verifyid, TaskService ordername, string strConnectAnalyse)
        {
            this._GroupId = groupid;
            this._VerifyId = verifyid;
            this._OrderName = ordername;

            this.strAnalyseConn = strConnectAnalyse;
        }
        #endregion

        #region ����
        public DataSet Parser()
        {
            MSSQLOperate pADOUtils = new MSSQLOperate(strAnalyseConn);
            if (pADOUtils.Connect(false))
            {
                if (_OrderName != TaskService.COMMON_CONNECT && _OrderName != TaskService.COMMON_VERIFY && _OrderName != TaskService.COMMON_DISCONNECT)
                {
                    #region
                    strProcParams = "SP_GET_COMMON_ORDER";
                    paramCache = DataUtilities.GetParameters(strProcParams);

                    if (paramCache == null)
                    {
                        paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                        DataUtilities.SetParameters(strProcParams, paramCache);
                    }
                    paramCache[0].Value = _GroupId;
                    paramCache[1].Value = _VerifyId;
                    paramCache[2].Value = _OrderName;

                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                    pADOUtils.GetResult(RecordStyle.DATASET);
                    DataSet DataSet_User = (DataSet)pADOUtils.RecordData;
                    int Order_Count = pADOUtils.AffectRow;//�û�����Ȩ�޼�¼��

                    strProcParams = "SP_GET_COMMON_GROUP_ORDER";
                    paramCache = DataUtilities.GetParameters(strProcParams);

                    if (paramCache == null)
                    {
                        paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                        DataUtilities.SetParameters(strProcParams, paramCache); 
                    }

                    paramCache[0].Value = _GroupId;
                    paramCache[1].Value = _OrderName;

                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                    pADOUtils.GetResult(RecordStyle.DATASET);
                    DataSet DataSet_Group = (DataSet)pADOUtils.RecordData;
                    int Group_Orders = pADOUtils.AffectRow;//�û�������Ȩ����  
                    #endregion

                    if (0 != Order_Count)
                    {
                        pDataSet = DataSet_User;
                    }
                    else if (0 != Group_Orders)
                    {
                        pDataSet = DataSet_Group;
                    }
                }
            }

            return pDataSet;
        }

        public List<OrderInfo> GetOrder()
        {
            pDataSet = Parser();
            OrderInfo pOrderInfo;
            foreach (DataTable pOrderTable in pDataSet.Tables)
            {
                for (int i = 0; i < pOrderTable.Rows.Count; i++)
                {
                    pOrderInfo = new OrderInfo(pOrderTable.Rows[i]["Competence_Order_Field"].ToString(), (int)pOrderTable.Rows[i]["Competence_Order_Type"], pOrderTable.Rows[i]["Competence_Order_Key"].ToString(), (int)pOrderTable.Rows[i]["Competence_Order_Flag"]);

                    arrOrderUser.Add(pOrderInfo);
                }
            }
            return arrOrderUser;
        }
        #endregion

        #region �����ֶ�
        private int _GroupId;//�û�������id
        private int _VerifyId;//�û��ʺ�id
        private TaskService _OrderName;//������
        private string strProcParams = null;
        private DbParameter[] paramCache;

        private string strAnalyseConn;
        DataSet pDataSet = new DataSet();
        private List<OrderInfo> arrOrderUser = new List<OrderInfo>();
        #endregion
    }

    /// <summary>
    /// ����Ȩ��
    /// </summary>
    public class OrderInfo
    {
        #region ���캯��
        public OrderInfo(string fieldname, int type, string key, int flag)
        {
            this._Field_Name = fieldname;
            this._Type = type;
            this._Key = key;
            this._Flag = flag;
        }
        #endregion

        #region ����
        /// <summary>
        /// �ֶε�Tag_Name
        /// </summary>
        public string Field_Name
        {
            get
            {
                return this._Field_Name;
            }
        }

        /// <summary>
        /// �ֶε��ж�����
        /// </summary>
        public int Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// �ֶιؼ���
        /// </summary>
        public string Key
        {
            get
            {
                return this._Key;
            }
        }

        /// <summary>
        /// �����ǽ�ֹ
        /// </summary>
        public int Flag
        {
            get
            {
                return this._Flag;
            }
        }
        #endregion

        #region  �����ֶ�
        private string _Field_Name;
        private int _Type;
        private string _Key;
        private int _Flag;
        #endregion
    }

    /// <summary>
    /// ��֤�ֶ�
    /// </summary>
    public class CheckOrder
    {
        #region ���캯��
        public CheckOrder(List<OrderInfo> porderinfo, object fieldvalue, object field)
        {
            this.pOrderInfo = porderinfo;
            this.FieldVale = fieldvalue;
            this.Field = field;
        }
        #endregion

        #region ����
        public bool Check_Result()
        {
            foreach (OrderInfo orderinfo in pOrderInfo)
            {
                if (orderinfo.Flag == 0 && orderinfo.Field_Name == "N/A")//�������ֹ������Ҫƥ���ַ�����
                {
                    return false;
                }
                if (orderinfo.Field_Name == Field.ToString())//�����Ƶ��ֶ��������жϵ��ֶ���ͬʱ�������ж�
                {
                    bool pResult = false;
                    if (1 == orderinfo.Type)//����
                    {
                        int check = FieldVale.ToString().IndexOf(orderinfo.Key);
                        if (-1 < check)
                        {
                            pResult = true;
                        }
                        else
                        {
                            pResult = false;
                        }
                    }
                    else if (2 == orderinfo.Type)//������
                    {
                        int check = FieldVale.ToString().IndexOf(orderinfo.Key);
                        if (-1 < check)
                        {
                            pResult = false;
                        }
                        else
                        {
                            pResult = true;
                        }
                    }
                    else if (3 == orderinfo.Type)//����
                    {
                        if (FieldVale.ToString() == orderinfo.Key)
                        {
                            pResult = true;
                        }
                        else
                        {
                            pResult = false;
                        }
                    }
                    else if (4 == orderinfo.Type)//������
                    {
                        if (FieldVale.ToString() != orderinfo.Key)
                        {
                            pResult = true;
                        }
                        else
                        {
                            pResult = false;
                        }
                    }

                    if (0 == orderinfo.Flag)//��ֹ
                    {
                        pResult = pResult.Equals(false);
                    }

                    return pResult;
                }
            }

            return true;
        }
        #endregion

        #region �����ֶ�
        private List<OrderInfo> pOrderInfo;
        private object FieldVale;
        private object Field;
        #endregion
    }

    /// <summary>
    /// ��ȡ��������Ȩ����
    /// </summary>
    public class GetOrder_Num
    {
        #region ���캯��
        public GetOrder_Num(string strAnalyseConn, int int_groupid, int int_verifyid, string str_Message)
        {
            GroupID = int_groupid;
            VerifyID = int_verifyid;
            strMessage = str_Message;

            pADOUtils = new MSSQLOperate(strAnalyseConn);

            //pADOUtils = ps
        }
        #endregion

        #region ����
        /// <summary>
        /// �û����������¼��
        /// </summary>
        /// <returns></returns>
        public int Banned_Order()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_BANNEDORDER";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }
                paramCache[0].Value = GroupID;
                paramCache[1].Value = VerifyID;
                paramCache[2].Value = strMessage;

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;//�û����������¼��
            }

            return num;
        }

        /// <summary>
        /// �û�����Ȩ�޼�¼��
        /// </summary>
        /// <returns></returns>
        public int Order_Count()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_ORDER";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }
                paramCache[0].Value = GroupID;
                paramCache[1].Value = VerifyID;
                paramCache[2].Value = strMessage;

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;
            }

            return num;
        }

        /// <summary>
        /// �û�������������
        /// </summary>
        /// <returns></returns>
        public int Banned_Template()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_BANNEDORDER_TEMPLATE";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iTemplateName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }

                paramCache[0].Value = GroupID;
                paramCache[1].Value = VerifyID;
                paramCache[2].Value = strMessage.Split('_')[0].ToString();

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;
            }

            return num;
        }

        /// <summary>
        /// �û�������Ȩ����
        /// </summary>
        /// <returns></returns>
        public int Template_Count()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_ORDER_TEMPLATE";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iTemplateName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }

                paramCache[0].Value = GroupID;
                paramCache[1].Value = VerifyID;
                paramCache[2].Value = strMessage.Split('_')[0].ToString();

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;//
            }

            return num;
        }

        /// <summary>
        /// �û������������
        /// </summary>
        /// <returns></returns>
        public int Banned_Order_Group()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_GROUP_BANNEDORDER";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }

                paramCache[0].Value = GroupID;
                paramCache[1].Value = strMessage;

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;
            }

            return num;
        }

        /// <summary>
        /// �û�������Ȩ����
        /// </summary>
        /// <returns></returns>
        public int Order_Count_Group()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_GROUP_ORDER";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iOrderName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }

                paramCache[0].Value = GroupID;
                paramCache[1].Value = strMessage;

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;
            }

            return num;
        }

        /// <summary>
        /// �û�������������¼��
        /// </summary>
        /// <returns></returns>
        public int Banned_Template_Group()
        {
            int num = 0;
            if (pADOUtils.Connect(false))
            {
                strProcParams = "SP_GET_COMMON_GROUP_BANNEDORDER_TEMPLATE";
                paramCache = DataUtilities.GetParameters(strProcParams);

                if (paramCache == null)
                {
                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@iTemplateName",SqlDbType.VarChar,200)
											   };
                    DataUtilities.SetParameters(strProcParams, paramCache);
                }
                paramCache[0].Value = GroupID;
                paramCache[1].Value = strMessage.Split('_')[0].ToString();

                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                pADOUtils.GetResult(RecordStyle.DATASET);
                pDataSet = (DataSet)pADOUtils.RecordData;
                num = pADOUtils.AffectRow;
            }
            return num;
        }
        #endregion

        #region ˽���ֶ�
        private string strProcParams = null;
        private DbParameter[] paramCache;
        private int GroupID;
        private int VerifyID;
        private string strMessage;
        MSSQLOperate pADOUtils;
        DataSet pDataSet = null;
        #endregion
    }
}