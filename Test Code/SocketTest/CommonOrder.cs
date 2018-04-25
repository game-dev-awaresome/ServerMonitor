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
    /// 获取指定的命令权限
    /// </summary>
    public class CommonOrder
    {
        #region 构造函数
        public CommonOrder(int groupid, int verifyid, TaskService ordername, string strConnectAnalyse)
        {
            this._GroupId = groupid;
            this._VerifyId = verifyid;
            this._OrderName = ordername;

            this.strAnalyseConn = strConnectAnalyse;
        }
        #endregion

        #region 方法
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
                    int Order_Count = pADOUtils.AffectRow;//用户命令权限记录数

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
                    int Group_Orders = pADOUtils.AffectRow;//用户组命令权限数  
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

        #region 定义字段
        private int _GroupId;//用户所在组id
        private int _VerifyId;//用户帐号id
        private TaskService _OrderName;//命令名
        private string strProcParams = null;
        private DbParameter[] paramCache;

        private string strAnalyseConn;
        DataSet pDataSet = new DataSet();
        private List<OrderInfo> arrOrderUser = new List<OrderInfo>();
        #endregion
    }

    /// <summary>
    /// 命令权限
    /// </summary>
    public class OrderInfo
    {
        #region 构造函数
        public OrderInfo(string fieldname, int type, string key, int flag)
        {
            this._Field_Name = fieldname;
            this._Type = type;
            this._Key = key;
            this._Flag = flag;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 字段的Tag_Name
        /// </summary>
        public string Field_Name
        {
            get
            {
                return this._Field_Name;
            }
        }

        /// <summary>
        /// 字段的判断类型
        /// </summary>
        public int Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// 字段关键字
        /// </summary>
        public string Key
        {
            get
            {
                return this._Key;
            }
        }

        /// <summary>
        /// 允许还是禁止
        /// </summary>
        public int Flag
        {
            get
            {
                return this._Flag;
            }
        }
        #endregion

        #region  定义字段
        private string _Field_Name;
        private int _Type;
        private string _Key;
        private int _Flag;
        #endregion
    }

    /// <summary>
    /// 验证字段
    /// </summary>
    public class CheckOrder
    {
        #region 构造函数
        public CheckOrder(List<OrderInfo> porderinfo, object fieldvalue, object field)
        {
            this.pOrderInfo = porderinfo;
            this.FieldVale = fieldvalue;
            this.Field = field;
        }
        #endregion

        #region 方法
        public bool Check_Result()
        {
            foreach (OrderInfo orderinfo in pOrderInfo)
            {
                if (orderinfo.Flag == 0 && orderinfo.Field_Name == "N/A")//该命令被禁止，不需要匹配字符串。
                {
                    return false;
                }
                if (orderinfo.Field_Name == Field.ToString())//当限制的字段名与所判断的字段相同时，进行判断
                {
                    bool pResult = false;
                    if (1 == orderinfo.Type)//包含
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
                    else if (2 == orderinfo.Type)//不包含
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
                    else if (3 == orderinfo.Type)//等于
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
                    else if (4 == orderinfo.Type)//不等于
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

                    if (0 == orderinfo.Flag)//禁止
                    {
                        pResult = pResult.Equals(false);
                    }

                    return pResult;
                }
            }

            return true;
        }
        #endregion

        #region 定义字段
        private List<OrderInfo> pOrderInfo;
        private object FieldVale;
        private object Field;
        #endregion
    }

    /// <summary>
    /// 获取各种命令权限数
    /// </summary>
    public class GetOrder_Num
    {
        #region 构造函数
        public GetOrder_Num(string strAnalyseConn, int int_groupid, int int_verifyid, string str_Message)
        {
            GroupID = int_groupid;
            VerifyID = int_verifyid;
            strMessage = str_Message;

            pADOUtils = new MSSQLOperate(strAnalyseConn);

            //pADOUtils = ps
        }
        #endregion

        #region 方法
        /// <summary>
        /// 用户禁用命令记录数
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
                num = pADOUtils.AffectRow;//用户禁用命令记录数
            }

            return num;
        }

        /// <summary>
        /// 用户命令权限记录数
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
        /// 用户禁用命令组数
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
        /// 用户命令组权限数
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
        /// 用户组禁用命令数
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
        /// 用户组命令权限数
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
        /// 用户组禁用命令组记录数
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

        #region 私有字段
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