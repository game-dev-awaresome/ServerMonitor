using System;
using System.IO;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;


using SMSCore;
using System.Global;
using System.Define;
using System.DataPacket;
using System.DataCenter;
using Windows.Network;
using System.NotesCore;
using Domino;

namespace SocketTest
{
    public class TaskParser : CustomClass
    {
        #region 构造函数
        public TaskParser(NotesSessionClass pNotesSession, MessageEventArgs pSocketMessage, string strConnectAnalyse, string strConnectOnlinenum, string strAccessConnect)
        {
            this._pSocketMessage = pSocketMessage;
            this.strAnalyseConn = strConnectAnalyse;
            this.strOnlineNumConn = strConnectOnlinenum;
            this.strReportConn = strAccessConnect;
            this.strError = String.Empty;

            pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\netadmin.nsf");
            this.pNotesSession = pNotesSession;
        }        
        #endregion

        public bool Parser()
        {
            #region 变量定义
            bool bFailure = false;
            bool bResult = false;
            DataSet pDataSet = null;
            byte[] pSendBuffer = null;
            byte[] pReciveBuffer = this._pSocketMessage.SocketBuffer;
            CustomDataCollection pSendContent = new CustomDataCollection(StructType.CUSTOMDATA);
            string[] strPopedomCount = null;            

            SocketPacket mSocketPacket = new SocketPacket(pReciveBuffer);
            mSocketPacket.ExtractInfo();
            TaskService mTaskService = mSocketPacket.eService;
            CustomData[,] mReciceContent = mSocketPacket.Content[-1];
            SocketPacket pSourcePacket;
            this.eSocketMessage = mTaskService;
            strMessage = mTaskService.ToString();

            string _result = "SUCCEED";


            //SQL PLUS
            DataBase dt_Data;//数据库类型
            string sqlplus_Conn = null;//数据库连接字符串
            int iTimeOut = 60;//超时时间
            DataService ps = null;
            string sqlplus_ip = null;//数据库ip
            string sqlplus_database = null;//数据库名
            string sqlplus_user = null;//数据库用户名
            string sqlplus_pwd = null;//数据库口令

            //数据库连接
            MSSQLOperate pADOUtils = new MSSQLOperate(this.strAnalyseConn);            
            #endregion

            #region 调试信息 -- 接收的内容
            Console.WriteLine("      Message ID       : {0}", mSocketPacket.PacketID);
            Console.WriteLine("      Message Command  : {0}", mTaskService);
            Console.WriteLine("      Message Date     : {0}", System.DateTime.Now.ToString()/*mSocketDate.pPacket.m_Head.m_dtMsgDateTime.ToString()*/);
            Console.WriteLine("      Client Session     : {0}", this._pSocketMessage.Connection.Session);
            #endregion

            #region 命令权限判断
            int mRecice_length = mReciceContent.GetLength(1);//传进的参数数目

            if (mRecice_length != 1 && mTaskService != TaskService.COMMON_CONNECT && mTaskService != TaskService.COMMON_VERIFY && mTaskService != TaskService.SYSTEM_UPDATE_CHECK_GET && mTaskService != TaskService.SYSTEM_UPDATE_DOWN_GET && mTaskService != TaskService.SYSTEM_UPDATE_PUT_GET)
            {
                this.int_groupid = int.Parse(mReciceContent[0, mRecice_length - 2].Content.ToString());
                this.int_verifyid = int.Parse(mReciceContent[0, mRecice_length - 1].Content.ToString());
            }

            if (mTaskService != TaskService.COMMON_CONNECT || mTaskService != TaskService.COMMON_VERIFY || mTaskService != TaskService.COMMON_DISCONNECT)
            {
                GetOrder_Num Order_Num = new GetOrder_Num(strAnalyseConn, int_groupid, int_verifyid, strMessage);

                int Banned_Order = Order_Num.Banned_Order();//用户禁用命令记录数
                int Order_Count = Order_Num.Order_Count();//用户命令权限记录数
                int Banned_Template = Order_Num.Banned_Template();//用户禁用命令组数
                int Banned_Order_Group = Order_Num.Banned_Order_Group();//用户组禁用命令数
                int Banned_Template_Group = Order_Num.Banned_Template_Group();//用户组禁用命令组记录数

                #region 判断命令
                if (0 != Banned_Order)//用户命令被禁止，命令禁用
                {
                    #region
                    pSendContent.Add("该命令被禁止使用");
                    pSendContent.AddRows();

                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                    pSendBuffer = pSourcePacket.CoalitionInfo();
                    this._pSocketMessage.Connection.OnSend(pSendBuffer);

                    bResult = true;
                    return bResult;
                    #endregion
                }
                else//未找出用户命令禁用记录，查看是否设置了用户命令权限
                {
                    if (0 == Order_Count)//未设置用户命令权限，查看用户所在组的权限
                    {
                        if (0 != Banned_Template)//用户的命令组被禁用，命令禁用
                        {
                            #region
                            pSendContent.Add("该命令被禁止使用");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            this._pSocketMessage.Connection.OnSend(pSendBuffer);

                            bResult = true;
                            return bResult;
                            #endregion
                        }
                        else//未找出用户命令组禁用记录
                        {
                            if (0 != Banned_Order_Group)//用户组的命令被禁用，命令禁用
                            {
                                #region
                                pSendContent.Add("该命令被禁止使用");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                this._pSocketMessage.Connection.OnSend(pSendBuffer);

                                bResult = true;
                                return bResult;
                                #endregion
                            }
                            else//未找出用户组命令禁用记录
                            {
                                if (0 != Banned_Template_Group)//用户组的命令组被禁用，命令禁用
                                {
                                    #region
                                    pSendContent.Add("该命令被禁止使用");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    this._pSocketMessage.Connection.OnSend(pSendBuffer);

                                    bResult = true;
                                    return bResult;
                                    #endregion
                                }
                                else//用户组的命令组未被禁用，命令可用
                                {
                                }
                            }
                        }
                    }
                    else//设置了用户命令的权限，命令可用
                    {
                    }
                }
                #endregion
            }

            pCommonOrder = new CommonOrder(int_groupid, int_verifyid, mTaskService, strAnalyseConn);//获取该命令的权限

            #endregion

            if (mTaskService.ToString().IndexOf("SQLPLUS_AU", 5) > -1)
            {
                #region 解析消息
                switch (mTaskService)
                {
                    #region 获取用户有权限的大区
                    case TaskService.TOOL_SQLPLUS_AU_SERVER_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetServer;//.Replace("$verify", mReciceContent[0, 2].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 >= pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("暂无数据");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            }
                            else
                            {
                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);                            
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion
                    #region 获取数据库访问用户口令列表
                    case TaskService.TOOL_SQLPLUS_AU_ACCESS_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetPopedom.Replace("$access_server", mReciceContent[0, 2].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 >= pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("暂无数据");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            }
                            else
                            {
                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion

                    #region 获取玩家等级和Exp
                    case TaskService.TOOL_SQLPLUS_AU_EXP_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 2].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 4].Content.ToString();
                            str_aupwd = mReciceContent[0, 5].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");
                        pMysqlUtils = new MysqlUtils(strMysqlConn);

                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetExpLevel.Replace("$userid", mReciceContent[0, 6].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                string Str_Sql = Constant.StrRealyLevel.Replace("$Exp", pDataSet.Tables[0].Rows[0][1].ToString());
                                pMysqlUtils.ExecuteQuery(Str_Sql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                DataSet p_DataSet = (DataSet)pMysqlUtils.Data;

                                pDataSet.Tables[0].Columns.Add("RealLevel", System.Type.GetType("System.Int32"), p_DataSet.Tables[0].Rows[0][0].ToString());

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("未查找到该用户信息");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        break;
                    #endregion
                    #region 修改玩家等级和Exp
                    case TaskService.TOOL_SQLPLUS_AU_LEVELEXP_UPDATE:
                        if (pADOUtils.Connect(false))
                        {
                            #region AU日志-已注释
                            /*strProcParams = "SP_PUT_AULOG";
                            paramCache = DataUtilities.GetParameters(strProcParams);

                            if (paramCache == null)
                            {
                                paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                DataUtilities.SetParameters(strProcParams, paramCache);
                            }

                            paramCache[0].Value = this.int_verifyid;
                            paramCache[1].Value = mTaskService.ToString();
                            paramCache[2].Value = 1;
                            paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                            paramCache[4].Value = mReciceContent[0, 5].Content.ToString();
                            paramCache[5].Value = _result;

                            pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                            pADOUtils.GetResult(RecordStyle.NONE);*/
                            #endregion

                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;
                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");
                        pMysqlUtils = new MysqlUtils(strMysqlConn);
                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuUpdateExp.Replace("$level", mReciceContent[0, 4].Content.ToString());
                                StrSql = StrSql.Replace("$userid", mReciceContent[0, 5].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.NONE);
                                                                
                                if (0 < pMysqlUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion.

                    #region 玩家性别信息获取
                    case TaskService.TOOL_SQLPLUS_AU_GENDER_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 2].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 4].Content.ToString();
                            str_aupwd = mReciceContent[0, 5].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");
                        pMysqlUtils = new MysqlUtils(strMysqlConn);

                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetGender.Replace("$userid", mReciceContent[0, 6].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("未查找到该用户信息");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }

                        break;
                    #endregion
                    #region 玩家性别异常处理
                    case TaskService.TOOL_SQLPLUS_AU_GENDER_UPDATE:
                        if (pADOUtils.Connect(false))
                        {
                            #region AU日志-已注释
                            /*strProcParams = "SP_PUT_AULOG";
                            paramCache = DataUtilities.GetParameters(strProcParams);

                            if (paramCache == null)
                            {
                                paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                DataUtilities.SetParameters(strProcParams, paramCache);
                            }

                            paramCache[0].Value = this.int_verifyid;
                            paramCache[1].Value = mTaskService.ToString();
                            paramCache[2].Value = 1;
                            paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                            paramCache[4].Value = mReciceContent[0, 5].Content.ToString();
                            paramCache[5].Value = _result;

                            pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                            pADOUtils.GetResult(RecordStyle.NONE);*/
                            #endregion

                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "itemdb");

                        string _gendervalue = "0";
                        string _gendrresult = "男性";
                        if ("F" == mReciceContent[0, 4].Content.ToString())
                        {
                            _gendervalue = "100";
                            _gendrresult = "女性";
                        }

                        pMysqlUtils = new MysqlUtils(strMysqlConn);
                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string strsql = Constant.StrAuUpdateGender.Replace("$value", _gendervalue);
                                strsql = strsql.Replace("$usersn", mReciceContent[0, 5].Content.ToString());
                                pMysqlUtils.ExecuteQuery(strsql);
                                pMysqlUtils.GetResult(RecordStyle.NONE);

                                if (0 < pMysqlUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据表丢失
                                pSendContent.Add("数据表丢失！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);                                
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion

                    #region 明星初始化卡异常处理
                    case TaskService.TOOL_SQLPLUS_AU_PARTYCARD_UPDATE:
                        #region Audition信息获取
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }
                        #endregion
                        #region Audition操作
                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");
                        pMysqlUtils = new MysqlUtils(strMysqlConn);

                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetGender.Replace("$userid", mReciceContent[0, 4].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("未查找到该用户信息");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                StrSql = "DELETE FROM battleuser WHERE usersn=" + pDataSet.Tables[0].Rows[0][0].ToString();
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.NONE);

                                if (0 >= pMysqlUtils.AffectRow)
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            break;
                            #endregion
                        }
                        #endregion
                        #region Item信息获取
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }
                        #endregion
                        #region Item操作
                        sqlplus_Conn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        sqlplus_Conn = sqlplus_Conn.Replace("$uid", str_auuser);
                        sqlplus_Conn = sqlplus_Conn.Replace("$pwd", str_aupwd);
                        sqlplus_Conn = sqlplus_Conn.Replace("$database", "itemdb");

                        MysqlUtils _MysqlUtils = new MysqlUtils(sqlplus_Conn);
                        if (_MysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetGender.Replace("$userid", mReciceContent[0, 4].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("未查找到该用户信息");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                string strsql = "delete from battleparty_inititem_use_info where usersn=" + pDataSet.Tables[0].Rows[0][0].ToString();
                                _MysqlUtils.ExecuteQuery(strsql);
                                _MysqlUtils.GetResult(RecordStyle.NONE);

                                if (0 < pMysqlUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }
                                else
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            #endregion
                        }
                        #endregion

                        #region AU日志-已注释
                        /*if (pADOUtils.Connect(false))
                        {
                            strProcParams = "SP_PUT_AULOG";
                            paramCache = DataUtilities.GetParameters(strProcParams);

                            if (paramCache == null)
                            {
                                paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                DataUtilities.SetParameters(strProcParams, paramCache);
                            }

                            paramCache[0].Value = this.int_verifyid;
                            paramCache[1].Value = mTaskService.ToString();
                            paramCache[2].Value = 1;
                            paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                            paramCache[4].Value = mReciceContent[0, 4].Content.ToString();
                            paramCache[5].Value = _result;

                            pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                            pADOUtils.GetResult(RecordStyle.NONE);
                        }*/
                        #endregion
                        break;
                    #endregion

                    #region 玩家离婚信息、解除情侣关系信息
                    case TaskService.TOOL_SQLPLUS_AU_MARRIAGEINFO_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 2].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 4].Content.ToString();
                            str_aupwd = mReciceContent[0, 5].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");

                        pMysqlUtils = new MysqlUtils(strMysqlConn);
                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetGender.Replace("$userid", mReciceContent[0, 6].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("未查找到该用户信息");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }
                                else
                                {
                                    string c_value = "malesn";
                                    if ("F" == pDataSet.Tables[0].Rows[0][1].ToString())
                                    {
                                        c_value = "femalesn";
                                    }

                                    string strsql = Constant.StrAuGetCouple.Replace("$_value", c_value);
                                    strsql = strsql.Replace("$value", pDataSet.Tables[0].Rows[0][0].ToString());
                                    pMysqlUtils.ExecuteQuery(strsql);
                                    pMysqlUtils.GetResult(RecordStyle.DATASET);
                                    DataSet mySet = (DataSet)pMysqlUtils.Data;

                                    if (0 == mySet.Tables[0].Rows.Count)
                                    {
                                        if (null == pMysqlUtils.Message)
                                        {
                                            pSendContent.Add("暂无数据！");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }

                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(mySet, false));
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        break;
                    #endregion
                    #region Wedding
                    case TaskService.TOOL_SQLPLUS_AU_WEDDING_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 2].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 4].Content.ToString();
                            str_aupwd = mReciceContent[0, 5].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }

                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "Audition");

                        pMysqlUtils = new MysqlUtils(strMysqlConn);
                        if (pMysqlUtils.Connected())
                        {
                            try
                            {
                                string StrSql = Constant.StrAuGetGender.Replace("$userid", mReciceContent[0, 6].Content.ToString());
                                pMysqlUtils.ExecuteQuery(StrSql);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pMysqlUtils.Data;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("未查找到该用户信息");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }
                                else
                                {
                                    string w_value = "malesn";
                                    if ("F" == pDataSet.Tables[0].Rows[0][1].ToString())
                                    {
                                        w_value = "femalesn";
                                    }

                                    string strsql = Constant.StrAuGetWedding.Replace("$_value", w_value);
                                    strsql = strsql.Replace("$value", pDataSet.Tables[0].Rows[0][0].ToString());
                                    pMysqlUtils.ExecuteQuery(strsql);
                                    pMysqlUtils.GetResult(RecordStyle.DATASET);
                                    DataSet mySet = (DataSet)pMysqlUtils.Data;

                                    if (0 == mySet.Tables[0].Rows.Count)
                                    {
                                        if (null == pMysqlUtils.Message)
                                        {
                                            pSendContent.Add("暂无数据！");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }

                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(mySet, false));
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                #region 数据不存在
                                pSendContent.Add("数据不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        break;
                    #endregion

                    #region 添加婚礼券
                    case TaskService.TOOL_SQLPLUS_AU_MARRIAGECARD_ADD:
                        #region 获取ITEM数据库服务器口令和密码
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                DataSet cDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == cDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(cDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(cDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }
                        #endregion

                        #region
                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "itemdb");

                        pMysqlUtils = new MysqlUtils(strMysqlConn);

                        if (pMysqlUtils.Connected())
                        {
                            if ("0" == mReciceContent[0, 5].Content.ToString())
                            {
                                #region 添加或修改道具
                                #region 检查该用户是否拥有该道具
                                string StrCheck = Constant.StrAuCheckItem.Replace("$ItemTable_Name", "avatar_inventory_items");
                                StrCheck = StrCheck.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                StrCheck = StrCheck.Replace("$ItemID", mReciceContent[0, 10].Content.ToString());

                                pMysqlUtils.ExecuteQuery(StrCheck);
                                pMysqlUtils.GetResult(RecordStyle.DATASET);
                                DataSet pCheckSet = (DataSet)pMysqlUtils.Data;
                                if (0 < pCheckSet.Tables[0].Rows.Count)
                                {
                                    if ("0" == mReciceContent[0, 9].Content.ToString())
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pCheckSet, false));
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                        break;
                                    }
                                }
                                #endregion

                                string Str_ItemSql = string.Empty;
                                if ("0" == mReciceContent[0, 9].Content.ToString())//插入新道具
                                {
                                    #region 插入道具
                                    Str_ItemSql = Constant.StrAuSendCard.Replace("$ItemTable_Name", "avatar_inventory_items");
                                    Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 10].Content.ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ExpiredType", mReciceContent[0, 6].Content.ToString());

                                    double day_count = 0;
                                    switch (mReciceContent[0, 6].Content.ToString())
                                    {
                                        case "0":
                                            day_count = 0;
                                            break;
                                        case "1":
                                            day_count = 7;
                                            break;
                                        case "2":
                                            day_count = 30;
                                            break;
                                        case "3":
                                            day_count = 365;
                                            break;
                                        case "4":
                                            day_count = 999;
                                            break;
                                        default:
                                            day_count = 0;
                                            break;
                                    }
                                    Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                    #endregion
                                }
                                else
                                {
                                    #region 更新道具的过期期限
                                    Str_ItemSql = Constant.StrAuUpdateItem.Replace("$ItemTable_Name", "avatar_inventory_items");
                                    Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 10].Content.ToString());

                                    DateTime Update_Time = Convert.ToDateTime(pCheckSet.Tables[0].Rows[0]["ExpiredDate"].ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", Update_Time.AddDays(Convert.ToDouble(mReciceContent[0, 7].Content.ToString())).ToString());
                                    #endregion
                                }

                                #region 执行操作
                                pMysqlUtils.ExecuteQuery(Str_ItemSql);
                                pMysqlUtils.GetResult(RecordStyle.NONE);

                                if (0 < pMysqlUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    _result = "SUCCEED";
                                }
                                else
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = "FAILURE";
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = pMysqlUtils.Message;
                                    }
                                }
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion

                                #region AU日志
                                strProcParams = "SP_PUT_AULOG";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = this.int_verifyid;
                                paramCache[1].Value = mTaskService.ToString();
                                paramCache[2].Value = mReciceContent[0, 10].Content.ToString();
                                paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                                paramCache[4].Value = mReciceContent[0, 4].Content.ToString();
                                paramCache[5].Value = _result;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);
                                #endregion
                                #endregion
                            }
                            else
                            {
                                #region 重置道具
                                string Str_ItemSql = Constant.StrAuUpdateCard.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 10].Content.ToString());

                                pMysqlUtils.ExecuteQuery(Str_ItemSql);
                                pMysqlUtils.GetResult(RecordStyle.NONE);

                                if (0 < pMysqlUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    _result = "SUCCEED";
                                }
                                else
                                {
                                    if (null == pMysqlUtils.Message)
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = "FAILURE";
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = pMysqlUtils.Message;
                                    }
                                }
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion

                                #region AU日志-已注释
                                /*strProcParams = "SP_PUT_AULOG";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = this.int_verifyid;
                                paramCache[1].Value = mTaskService.ToString();
                                paramCache[2].Value = mReciceContent[0, 10].Content.ToString();
                                paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                                paramCache[4].Value = mReciceContent[0, 4].Content.ToString();
                                paramCache[5].Value = _result;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);*/
                                #endregion
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        #endregion
                        break;
                    #endregion

                    #region 获取道具列表
                    case TaskService.TOOL_SQLPLUS_AU_ITEM_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetItem;
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 == pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("暂无数据");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            }
                            else
                            {
                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                            }
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion
                    #region 发放道具
                    case TaskService.TOOL_SQLPLUS_AU_SENDITEM_PUT:
                        #region 检查服务器该道具是否存在
                        DataSet ItemTable_Set = new DataSet();
                        string ItemTable_Name = string.Empty;
                        if (pADOUtils.Connect(false))
                        {
                            string Str_GetTableSql = Constant.StrAUGetItemTable.Replace("$V_Props_ID", mReciceContent[0, 5].Content.ToString());
                            pADOUtils.ExecuteQuery(Str_GetTableSql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            ItemTable_Set = (DataSet)pADOUtils.RecordData;

                            if (null == ItemTable_Set)
                            {
                                pSendContent.Add("该道具ID不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                            else if (0 == ItemTable_Set.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("该道具ID不存在！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                            ItemTable_Name = ItemTable_Set.Tables[0].Rows[0][3].ToString();
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            #endregion
                        }
                        #endregion

                        #region 判断道具性别
                        if (("F" == mReciceContent[0, 10].Content.ToString() && "1" == ItemTable_Set.Tables[0].Rows[0][6].ToString()) || ("M" == mReciceContent[0, 10].Content.ToString() && "2" == ItemTable_Set.Tables[0].Rows[0][6].ToString()))
                        {
                            pSendContent.Add("道具与用户性别不匹配！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            break;
                        }
                        #endregion

                        #region 获取ITEM数据库服务器口令和密码
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("服务器命名规则不符合");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                        }

                        if ("0" == mReciceContent[0, 0].Content.ToString())
                        {
                            str_auuser = mReciceContent[0, 2].Content.ToString();
                            str_aupwd = mReciceContent[0, 3].Content.ToString();
                        }
                        else
                        {
                            if (pADOUtils.Connect(false))
                            {
                                string strsql = Constant.StrAuGetPwd.Replace("$access_server", auDataset.Tables[0].Rows[0][0].ToString());
                                pADOUtils.ExecuteQuery(strsql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                DataSet cDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == cDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("该服务器口令尚未设置");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }

                                str_auuser = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(cDataSet.Tables[0].Rows[0][0].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                str_aupwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(cDataSet.Tables[0].Rows[0][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                            }
                        }
                        #endregion

                        #region
                        strMysqlConn = Constant.StrAuConnect.Replace("$serverip", auDataset.Tables[0].Rows[0][2].ToString());
                        strMysqlConn = strMysqlConn.Replace("$uid", str_auuser);
                        strMysqlConn = strMysqlConn.Replace("$pwd", str_aupwd);
                        strMysqlConn = strMysqlConn.Replace("$database", "itemdb");

                        pMysqlUtils = new MysqlUtils(strMysqlConn);

                        if (pMysqlUtils.Connected())
                        {
                            #region 检查该用户是否拥有该道具
                            string StrCheck = Constant.StrAuCheckItem.Replace("$ItemTable_Name", ItemTable_Name);
                            StrCheck = StrCheck.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                            StrCheck = StrCheck.Replace("$ItemID", mReciceContent[0, 5].Content.ToString());

                            pMysqlUtils.ExecuteQuery(StrCheck);
                            pMysqlUtils.GetResult(RecordStyle.DATASET);
                            DataSet pCheckSet = (DataSet)pMysqlUtils.Data;
                            if (0 < pCheckSet.Tables[0].Rows.Count)
                            {
                                if ("0" == mReciceContent[0, 9].Content.ToString())
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pCheckSet, false));
                                    pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                    break;
                                }
                            }
                            #endregion

                            string Str_ItemSql = string.Empty;
                            if ("0" == mReciceContent[0, 9].Content.ToString())//插入新道具
                            {
                                #region 插入道具
                                Str_ItemSql = Constant.StrAuSendItem.Replace("$ItemTable_Name", ItemTable_Name);
                                Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 5].Content.ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ExpiredType", mReciceContent[0, 6].Content.ToString());

                                double day_count = 0;
                                switch (mReciceContent[0, 6].Content.ToString())
                                {
                                    case "0":
                                        day_count = 0;
                                        Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                        break;
                                    case "1":
                                        day_count = 7;
                                        Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                        break;
                                    case "2":
                                        day_count = 30;
                                        Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                        break;
                                    case "3":
                                        day_count = 365;
                                        Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                        break;
                                    case "4":
                                        //day_count = 999;
                                        Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", "2090-1-1 00:00:00");
                                        break;
                                    default:
                                        day_count = 0;
                                        break;
                                }
                                //Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", DateTime.Now.AddDays(day_count).ToString());
                                #endregion
                            }
                            else//更新道具的过期期限
                            {
                                #region 更新道具的过期期限
                                Str_ItemSql = Constant.StrAuUpdateItem.Replace("$ItemTable_Name", ItemTable_Name);
                                Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 5].Content.ToString());

                                DateTime Update_Time = Convert.ToDateTime(pCheckSet.Tables[0].Rows[0]["ExpiredDate"].ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", Update_Time.AddDays(Convert.ToDouble(mReciceContent[0, 7].Content.ToString())).ToString());
                                #endregion
                            }

                            #region 执行操作
                            pMysqlUtils.ExecuteQuery(Str_ItemSql);
                            pMysqlUtils.GetResult(RecordStyle.NONE);

                            if (0 < pMysqlUtils.AffectRow)
                            {
                                pSendContent.Add("SUCCEED");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                _result = "SUCCEED";
                            }
                            else
                            {
                                if (null == pMysqlUtils.Message)
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    _result = "FAILURE";
                                }
                                else
                                {
                                    pSendContent.Add("数据库执行异常：" + pMysqlUtils.Message);
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    _result = pMysqlUtils.Message;
                                }
                            }
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion

                            #region AU日志-已注释
                            /*strProcParams = "SP_PUT_AULOG";
                            paramCache = DataUtilities.GetParameters(strProcParams);

                            if (paramCache == null)
                            {
                                paramCache = new SqlParameter[]{
                                                   new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Order",SqlDbType.VarChar,50),
                                                   new SqlParameter("@Item_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_ID",SqlDbType.Int,4),
                                                   new SqlParameter("@User_ID",SqlDbType.VarChar,200),
                                                   new SqlParameter("@Result",SqlDbType.VarChar,200)
                                               };
                                DataUtilities.SetParameters(strProcParams, paramCache);
                            }

                            paramCache[0].Value = this.int_verifyid;
                            paramCache[1].Value = mTaskService.ToString();
                            paramCache[2].Value = mReciceContent[0, 5].Content.ToString();
                            paramCache[3].Value = mReciceContent[0, 1].Content.ToString();
                            paramCache[4].Value = mReciceContent[0, 4].Content.ToString();
                            paramCache[5].Value = _result;

                            pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                            pADOUtils.GetResult(RecordStyle.NONE);*/
                            #endregion
                        }
                        else
                        {
                            #region 未连接数据库
                            pSendContent.Add("数据库连接失败！");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        #endregion
                        break;
                    #endregion
                    default:
                        throw new Exception("未知命令！");
                }
                #endregion

                pADOUtils.DisConnected();

                this._pSocketMessage.Connection.OnSend(pSendBuffer);
            }
            #region
            /*else if (mTaskService.ToString() == "TOOL_SQLPLUS_TRANSACT")
            {
                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                //sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);
                sqlplus_Conn = "Connect Timeout=3000;Server=192.168.24.224,1433;Database=Analyse_Text;Uid=sa;pwd=9you;Asynchronous Processing=true";

                ps = new DataService(dt_Data, sqlplus_Conn, _pSocketMessage.Connection,mTaskService);
                if (ps.OnConnect(false))
                {
                    ps.DataCenter.ExecuteTimeOut = iTimeOut;

                    if (DataSqlConn.Check_Sql(mReciceContent[0, 7].Content.ToString()))
                    {
                        string str = "select * from info_game";
                        //ps.OnQuery(mReciceContent[0, 7].Content.ToString());
                        ps.OnQuery(str);
                        //ps.DataCenter.GetResult(RecordStyle.NONE);
                        ps.DataCenter.GetResult(RecordStyle.REALTIME);
                        if ((null != ps.Message) && "" != ps.Message)
                        {
                            pSendContent.Add(ps.Message);
                            pSendContent.AddRows();
                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            this._pSocketMessage.Connection.OnSend(pSendBuffer);
                        }
                    }
                    else//sql语句不合法
                    {
                        pSendContent.Add("sql语句不符合约束条件!");
                        pSendContent.AddRows();
                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        this._pSocketMessage.Connection.OnSend(pSendBuffer);
                    }                    
                }
                else
                {
                    #region 未连接数据库
                    pSendContent.Add("数据库连接失败！");
                    pSendContent.AddRows();

                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                    pSendBuffer = pSourcePacket.CoalitionInfo();

                    this._pSocketMessage.Connection.OnSend(pSendBuffer);
                    #endregion
                }
            }*/
            #endregion
            else
            {
                if (pADOUtils.Connect(false))
                {
                    #region 服务逻辑
                    try
                    {
                        #region 解析数据
                        switch (mTaskService)
                        {
                            #region 用户连接
                            case TaskService.COMMON_CONNECT:
                                pSendContent.Add("PASS");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 用户认证
                            case TaskService.COMMON_VERIFY:
                                strProcParams = "SP_VERIFY_LOGIN";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strNick",SqlDbType.VarChar,50),
												   new SqlParameter("@strPwd",SqlDbType.VarChar,50),
												   new SqlParameter("@strMAC",SqlDbType.VarChar,50),
												   new SqlParameter("@strSession",SqlDbType.VarChar,50),
                                                   
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = this._pSocketMessage.Connection.Session;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;
                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 用户断线
                            case TaskService.COMMON_DISCONNECT:
                                break;
                            #endregion

                            #region 系统数据管理模块
                            #region 获取目录树节点
                            case TaskService.SYSTEM_TREENODE_GET:
                                strProcParams = "SP_GET_TREEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@verify",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加模块信息 Finished!
                            case TaskService.SYSTEM_MODULE_PUT:
                                strProcParams = "SP_PUT_MODULEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strClass",SqlDbType.VarChar,50),
												   new SqlParameter("@strDesc",SqlDbType.VarChar,100)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取模块信息 Finished!
                            case TaskService.SYSTEM_MODULE_LIST:
                                strProcParams = "SP_GET_MODULEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定模块信息 Finished!
                            case TaskService.SYSTEM_MODULE_DELETE:
                                strProcParams = "SP_DELETE_MODULEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定模块信息 Finished!
                            case TaskService.SYSTEM_MODULE_UPDATE:
                                strProcParams = "SP_UPDATE_MODULEINFO";
                                strContent = "Module_Sort = '%s', Module_Name = '%s', Module_Class = '%s', Module_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加采集类别信息 Finished!
                            case TaskService.SYSTEM_CRITERION_SORT_PUT:
                                strProcParams = "SP_PUT_SORTINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strkey",SqlDbType.VarChar,50),
												   new SqlParameter("@strAnalyse",SqlDbType.VarChar,50),
												   new SqlParameter("@strBackup",SqlDbType.VarChar,50),
												   new SqlParameter("@strView",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取采集类别列表 Finished!
                            case TaskService.SYSTEM_CRITERION_SORT_LIST:
                                strProcParams = "SP_GET_SORTINFO";

                                pADOUtils.ExecuteQuery(strProcParams);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定采集类别信息 Finished!
                            case TaskService.SYSTEM_CRITERION_SORT_UPDATE:
                                strProcParams = "SP_UPDATE_SORTINFO";
                                strContent = "Sort_Kind = '%s', Sort_Name = '%s', Sort_Key = '%s', Sort_Analyse = '%s', Sort_Backup = '%s', Sort_View = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定采集类型 Finished!
                            case TaskService.SYSTEM_CRITERION_SORT_DELETE:
                                strProcParams = "SP_DELETE_SORTINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加服务器采集类别 Finished!
                            case TaskService.SYSTEM_CRITERION_SERVER_PUT:
                                strProcParams = "SP_PUT_CRITERIONINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@fNormal",SqlDbType.Float,8),
												   new SqlParameter("@fWarning",SqlDbType.Float,8),
												   new SqlParameter("@fGraveness",SqlDbType.Float,8),
												   new SqlParameter("@strDesc",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取每台服务器采集标准列表 Finished!
                            case TaskService.SYSTEM_CRITERION_SERVER_LIST:
                                strProcParams = "SP_GET_CRITERIONINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定服务器采集标准信息 Finished!
                            case TaskService.SYSTEM_CRITERION_SERVER_UPDATE:
                                strProcParams = "SP_UPDATE_CRITERIONINFO";
                                strContent = "Criterion_Server = '%s',  Criterion_Sort= '%s', Criterion_Normal = '%s', Criterion_Warning = '%s', Criterion_Graveness = '%s', Criterion_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定服务器采集标准信息 Finished!
                            case TaskService.SYSTEM_CRITERION_SERVER_DELETE:
                                strProcParams = "SP_DELETE_CRITERIONINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 发送SMS
                            #region
                            case TaskService.SYSTEM_SMSCONTENT_PUT:
                                //ArrayList pFailureList = new ArrayList();
                                WebSMS pWebSMSSend = new WebSMS();

                                string[] arrReciveMobile = mReciceContent[0, 3].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < arrReciveMobile.Length; i++)
                                {
                                    //GlobalStruct[] pFailureStruct = new GlobalStruct[2];

                                    byte[] pResultBuffer = pWebSMSSend.sendSMS(arrReciveMobile[i], mReciceContent[0, 4].Content.ToString());
                                    int iResult = Encoding.Default.GetString(pResultBuffer) == "OK" ? 1 : 0;

                                    if (iResult == 1)
                                    {
                                        strProcParams = "SP_PUT_SMSCONTENT";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iFailure",SqlDbType.Int, 4),
												   new SqlParameter("@iRelay",SqlDbType.Int, 4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@strMobile",SqlDbType.VarChar,50),
												   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = mReciceContent[0, 0].Content;
                                        paramCache[1].Value = mReciceContent[0, 1].Content;
                                        paramCache[2].Value = mReciceContent[0, 2].Content;
                                        paramCache[3].Value = arrReciveMobile[i];
                                        paramCache[4].Value = mReciceContent[0, 4].Content;

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.DATASET);
                                        pDataSet = (DataSet)pADOUtils.RecordData;

                                        if (0 > pADOUtils.AffectRow)
                                        {
                                            pSendContent.Add(DataField.LINKER_MOBILE, DataFormat.STRING, arrReciveMobile[i]);
                                            pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "发送成功，数据保存失败！");
                                            pSendContent.AddRows();
                                            //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                            //pFailureStruct[0].oFiledsTypes = "String";
                                            //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                            //pFailureStruct[1].oFieldsName = "Sender_Content";
                                            //pFailureStruct[1].oFiledsTypes = "String";
                                            //pFailureStruct[1].oFieldValues = "发送成功，数据保存失败！";
                                        }
                                        else
                                        {
                                            pSendContent.Add(DataField.LINKER_MOBILE, DataFormat.STRING, arrReciveMobile[i]);
                                            pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "发送成功，数据保存成功！");
                                            pSendContent.AddRows();
                                            //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                            //pFailureStruct[0].oFiledsTypes = "String";
                                            //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                            //pFailureStruct[1].oFieldsName = "Sender_Content";
                                            //pFailureStruct[1].oFiledsTypes = "String";
                                            //pFailureStruct[1].oFieldValues = "发送成功，数据保存成功！";
                                        }
                                    }
                                    else
                                    {
                                        pSendContent.Add(DataField.LINKER_MOBILE, DataFormat.STRING, arrReciveMobile[i]);
                                        pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "发送失败，数据未保存！");
                                        pSendContent.AddRows();
                                        //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                        //pFailureStruct[0].oFiledsTypes = "String";
                                        //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                        //pFailureStruct[1].oFieldsName = "Sender_Content";
                                        //pFailureStruct[1].oFiledsTypes = "String";
                                        //pFailureStruct[1].oFieldValues = "发送失败，数据未保存！";
                                    }
                                    //pFailureList.Add(pFailureStruct);
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                //pSendBuffer = mSocketDate.setSocketData(EnumConvert.getResponseKey(mTagID), mSocketDate.pPacket.m_Head.eCategory, buildTLV(pFailureList.ToArray()), iCurren).bMsgBuffer;
                                //pFailureList.Clear();
                                break;
                            #endregion
                            #region
                            case TaskService.SYSTEM_SMSLINKER_PUT:
                                strProcParams = "SP_PUT_SMSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strMobile",SqlDbType.VarChar,15),
												   new SqlParameter("@strContent",SqlDbType.VarChar,100),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region
                            case TaskService.SYSTEM_SMSLINKER_UPDATE:
                                strProcParams = "SP_UPDATE_SMSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strMobile",SqlDbType.VarChar,15),
												   new SqlParameter("@strContent",SqlDbType.VarChar,100),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region
                            case TaskService.SYSTEM_SMSLINKER_DELETE:
                                strProcParams = "SP_DELETE_SMSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region
                            case TaskService.SYSTEM_SMSLINKER_LIST:
                                strProcParams = "SP_GET_SMSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strMobile",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 命令权限
                            #region 按组获取命令模板信息
                            case TaskService.MEMBER_TEMPLATEINFO_GET_GROUP:
                                strProcParams = "SP_GET_TEMPLATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@iLayer",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取命令对应的限制字段
                            case TaskService.SYSTEM_COMPETENCE_ORDERFIELD_GET:
                                strProcParams = "SP_GET_ORDERFIELD";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@oName",SqlDbType.VarChar,100)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 插入命令权限
                            case TaskService.SYSTEM_COMPETENCE_ORDER_PUT:
                                strProcParams = "SP_PUT_COMMONORDER";

                                for (int i = 0; i < mReciceContent[0, 0].Content.ToString().Split('|').Length; i++)
                                {
                                    paramCache = DataUtilities.GetParameters(strProcParams);
                                    if (null == paramCache)
                                    {
                                        paramCache = new SqlParameter[]{
                                                        new SqlParameter("@iGroup",SqlDbType.Int,4),
                                                        new SqlParameter("@iVerify",SqlDbType.Int,4),
                                                        new SqlParameter("@oTemplate",SqlDbType.VarChar,150),
                                                        new SqlParameter("@oName",SqlDbType.VarChar,150),
                                                        new SqlParameter("@oField",SqlDbType.VarChar,150),
                                                        new SqlParameter("@oKey",SqlDbType.VarChar,500),
                                                        new SqlParameter("@oType",SqlDbType.Int,4),
                                                        new SqlParameter("@oFlag",SqlDbType.Int,4),
                                                        new SqlParameter("@oDesc",SqlDbType.VarChar,500)
                                };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = mReciceContent[0, 0].Content.ToString().Split('|')[i].ToString();
                                    paramCache[1].Value = mReciceContent[0, 1].Content.ToString().Split('|')[i].ToString();
                                    paramCache[2].Value = mReciceContent[0, 2].Content.ToString().Split('|')[i].ToString();
                                    paramCache[3].Value = mReciceContent[0, 3].Content.ToString().Split('|')[i].ToString();
                                    paramCache[4].Value = mReciceContent[0, 4].Content.ToString().Split('|')[i].ToString();
                                    paramCache[5].Value = mReciceContent[0, 5].Content.ToString().Split('|')[i].ToString();
                                    paramCache[6].Value = mReciceContent[0, 6].Content.ToString().Split('|')[i].ToString();
                                    paramCache[7].Value = mReciceContent[0, 7].Content.ToString().Split('|')[i].ToString();
                                    paramCache[8].Value = mReciceContent[0, 8].Content.ToString().Split('|')[i].ToString();

                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);

                                    if (0 > pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();

                                        bFailure = true;
                                        break;
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 修改命令权限
                            case TaskService.SYSTEM_COMPETENCE_ORDER_UPDATE:
                                strProcParams = "SP_UPDATE_COMMONORDER";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@id",SqlDbType.Int,4),
                                                        new SqlParameter("@oType",SqlDbType.Int,4),
                                                        new SqlParameter("@oKey",SqlDbType.VarChar,500)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.AffectRow == 1)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除命令权限
                            case TaskService.SYSTEM_COMPETENCE_ORDER_DELETE:
                                strProcParams = "SP_DELETE_COMMONORDER";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@id",SqlDbType.Int,4)                                                        
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.AffectRow == 1)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取已设置命令权限列表
                            case TaskService.SYSTEM_COMPETENCE_ORDER_LIST:
                                strProcParams = "SP_GET_COMMONORDER";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@iGroup",SqlDbType.Int,4),
                                                        new SqlParameter("@iVerify",SqlDbType.Int,4),
                                                        new SqlParameter("@oTemplate",SqlDbType.VarChar,150),
                                                        new SqlParameter("@oName",SqlDbType.VarChar,150)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 系统日志
                            case TaskService.SYSTEM_LOG_CONTENT_PUT:
                                strProcParams = "SP_PUT_ONLINENUMLOG";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@strGame",SqlDbType.VarChar,50),
												   new SqlParameter("@strArea",SqlDbType.VarChar,50),
												   new SqlParameter("@strChannel",SqlDbType.VarChar,50),
												   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.SYSTEM_RECORD_TRUNCATE:
                                pSendContent.Add("该命令不允许使用！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.SYSTEM_LOG_LIST:
                                int iAffectRow = 0;
                                if (mReciceContent[0, 2].Content.ToString() == "0")
                                {
                                    strProcParams = "SP_GET_OPERATIONLOG";

                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (paramCache == null)
                                    {
                                        paramCache = new SqlParameter[]{
												   new SqlParameter("@dtBegin",SqlDbType.DateTime),
												   new SqlParameter("@dtEnd",SqlDbType.DateTime),
												   new SqlParameter("@strVerifyName",SqlDbType.VarChar, 50),
												   new SqlParameter("@strAction",SqlDbType.VarChar, 100)
											   };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = mReciceContent[0, 3].Content;
                                    paramCache[1].Value = mReciceContent[0, 4].Content;
                                    paramCache[2].Value = mReciceContent[0, 5].Content;
                                    paramCache[3].Value = mReciceContent[0, 6].Content;

                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.DATASET);
                                    pDataSet = (DataSet)pADOUtils.RecordData;

                                    iAffectRow = pADOUtils.AffectRow;
                                }
                                else
                                {
                                    strProcParams = "SP_GET_ONLINENUMLOG";

                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (paramCache == null)
                                    {
                                        paramCache = new SqlParameter[]{
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@dBegin",SqlDbType.DateTime),
												   new SqlParameter("@dEnd",SqlDbType.DateTime),
												   new SqlParameter("@strGame",SqlDbType.VarChar, 50),
												   new SqlParameter("@strArea",SqlDbType.VarChar, 50),
												   new SqlParameter("@strChannel",SqlDbType.VarChar, 50)
											   };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = mReciceContent[0, 3].Content;
                                    paramCache[1].Value = mReciceContent[0, 4].Content;
                                    paramCache[2].Value = mReciceContent[0, 5].Content;
                                    paramCache[3].Value = mReciceContent[0, 6].Content;
                                    paramCache[4].Value = mReciceContent[0, 7].Content;
                                    paramCache[5].Value = mReciceContent[0, 8].Content;

                                    pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                    pOnlinenum.Connect(false);
                                    pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                    pOnlinenum.GetResult(RecordStyle.DATASET);
                                    pDataSet = (DataSet)pOnlinenum.RecordData;

                                    iAffectRow = pOnlinenum.AffectRow;
                                }

                                if (iAffectRow == 0)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 系统模板部分
                            case TaskService.SYSTEM_TEMPLATE_INFO_PUT:
                                strProcParams = "SP_PUT_SERVICETEMPLATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iLayer",SqlDbType.Int, 4),
												   new SqlParameter("@iGame",SqlDbType.Int, 4),
												   new SqlParameter("@iCategory",SqlDbType.Int, 4),
												   new SqlParameter("@strTitle",SqlDbType.VarChar,50),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.SYSTEM_TEMPLATE_INFO_UPDATE:
                                strProcParams = "SP_UPDATE_SERVICETEMPLATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int, 4),
												   new SqlParameter("@iGame",SqlDbType.Int, 4),
												   new SqlParameter("@iCategory",SqlDbType.Int, 4),
												   new SqlParameter("@strTitle",SqlDbType.VarChar,50),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.SYSTEM_TEMPLATE_INFO_DELETE:
                                strProcParams = "SP_DELETE_SERVICETEMPLATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.SYSTEM_TEMPLATE_INFO_LIST:
                                strProcParams = "SP_GET_SERVICETEMPLATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iCategory",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 文件检查
                            case TaskService.SYSTEM_UPDATE_CHECK_GET:
                                bool bUpdate = false;
                                int iVersionCount = 0;
                                string strFileName = String.Empty;

                                //检查Client没有的文件
                                DirectoryInfo pDirectory = new DirectoryInfo("./update/");

                                //计算更新文件位置
                                int iAppPathLength = pDirectory.FullName.Length;

                                //计算文件总数
                                int iFileCount = 0;
                                foreach (DirectoryInfo pSubDirectory in pDirectory.GetDirectories())
                                {
                                    foreach (FileInfo pFileInfo in pSubDirectory.GetFiles())
                                    {
                                        if (pFileInfo.Extension.ToUpper() == ".DLL" || pFileInfo.Extension.ToUpper() == ".EXE")
                                        {
                                            iFileCount++;
                                        }
                                    }
                                }

                                iFileCount += pDirectory.GetFiles().Length;

                                if (mReciceContent.GetLength(0) < iFileCount)
                                {
                                    iVersionCount = iFileCount - mReciceContent.GetLength(0);
                                }

                                //确定版本不相符的个数
                                for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                {
                                    if (File.Exists("./update/" + mReciceContent[i, 0].Content.ToString()))
                                    {
                                        FileVersionInfo pVersion = FileVersionInfo.GetVersionInfo("./update/" + mReciceContent[i, 0].Content.ToString());

                                        if (pVersion.FileVersion != mReciceContent[i, 1].Content.ToString())
                                        {
                                            iVersionCount++;
                                            bUpdate = true;
                                        }
                                    }
                                }

                                if ((!bUpdate) && iVersionCount > 0)
                                {
                                    bUpdate = true;
                                }

                                //当有文件需要更新的时候
                                DirectoryInfo pGuideDirectory = new DirectoryInfo("./update/guide/");
                                DirectoryInfo pIniDirectory = new DirectoryInfo("./update/ini/");

                                if (bUpdate)
                                {
                                    foreach (FileInfo pFileInfo in pGuideDirectory.GetFiles())
                                    {
                                        iVersionCount++;
                                    }

                                    foreach (FileInfo pFileInfo in pIniDirectory.GetFiles())
                                    {
                                        iVersionCount++;
                                    }
                                }

                                //CEnum.Message_Body[,] mSendContents;

                                if (iVersionCount > 0)
                                {
                                    FileInfo pFileInfos = null;

                                    int iFileIndex = 0;
                                    List<string> arrFile = new List<string>();

                                    //mSendContents = new CEnum.Message_Body[iVersionCount, 2];

                                    //构造需要更新的文件信息
                                    for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                    {
                                        if (File.Exists("./update/" + mReciceContent[i, 0].Content.ToString()))
                                        {
                                            FileVersionInfo pVersion = FileVersionInfo.GetVersionInfo("./update/" + mReciceContent[i, 0].Content.ToString());

                                            if (pVersion.FileVersion != mReciceContent[i, 1].Content.ToString())
                                            {
                                                pFileInfos = new FileInfo("./update/" + mReciceContent[i, 0].Content.ToString());

                                                pSendContent.Add(DataField.UPDATE_NAME, DataFormat.STRING, mReciceContent[i, 0].Content);
                                                pSendContent.Add(DataField.UPDATE_STATE, DataFormat.SIGNED, pFileInfos.Length);
                                                pSendContent.AddRows();

                                                //mSendContents[iFileIndex, 0].eFormat = CEnum.TagFormat.TLV_STRING;
                                                //mSendContents[iFileIndex, 0].eName = CEnum.TagName.UPDATE_NAME;
                                                //mSendContents[iFileIndex, 0].Content = mReciceContent[i, 0].Content;

                                                //mSendContents[iFileIndex, 1].eFormat = CEnum.TagFormat.TLV_INTEGER;
                                                //mSendContents[iFileIndex, 1].eName = CEnum.TagName.UPDATE_STATE;
                                                //mSendContents[iFileIndex, 1].Content = pFileInfos.Length;

                                                iFileIndex++;
                                            }

                                            arrFile.Add(mReciceContent[i, 0].Content.ToString().ToUpper());
                                        }
                                    }

                                    if (bUpdate)
                                    {
                                        foreach (FileInfo pFileInfo in pGuideDirectory.GetFiles())
                                        {
                                            pSendContent.Add(DataField.UPDATE_NAME, DataFormat.STRING, "./guide/" + pFileInfo.Name);
                                            pSendContent.Add(DataField.UPDATE_STATE, DataFormat.SIGNED, pFileInfos.Length);
                                            pSendContent.AddRows();

                                            //mSendContents[iFileIndex, 0].eFormat = CEnum.TagFormat.TLV_STRING;
                                            //mSendContents[iFileIndex, 0].eName = CEnum.TagName.UPDATE_NAME;
                                            //mSendContents[iFileIndex, 0].Content = "./guide/" + pFileInfo.Name;

                                            //mSendContents[iFileIndex, 1].eFormat = CEnum.TagFormat.TLV_INTEGER;
                                            //mSendContents[iFileIndex, 1].eName = CEnum.TagName.UPDATE_STATE;
                                            //mSendContents[iFileIndex, 1].Content = pFileInfo.Length;

                                            iFileIndex++;
                                        }

                                        foreach (FileInfo pFileInfo in pIniDirectory.GetFiles())
                                        {
                                            pSendContent.Add(DataField.UPDATE_NAME, DataFormat.STRING, "./ini/" + pFileInfo.Name);
                                            pSendContent.Add(DataField.UPDATE_STATE, DataFormat.SIGNED, pFileInfos.Length);
                                            pSendContent.AddRows();

                                            //mSendContents[iFileIndex, 0].eFormat = CEnum.TagFormat.TLV_STRING;
                                            //mSendContents[iFileIndex, 0].eName = CEnum.TagName.UPDATE_NAME;
                                            //mSendContents[iFileIndex, 0].Content = "./ini/" + pFileInfo.Name;

                                            //mSendContents[iFileIndex, 1].eFormat = CEnum.TagFormat.TLV_INTEGER;
                                            //mSendContents[iFileIndex, 1].eName = CEnum.TagName.UPDATE_STATE;
                                            //mSendContents[iFileIndex, 1].Content = pFileInfo.Length;

                                            iFileIndex++;
                                        }
                                    }

                                    //枚举根目录文件
                                    foreach (FileInfo pFileInfo in pDirectory.GetFiles())
                                    {
                                        if (pFileInfo.Extension.ToUpper() == ".DLL" || pFileInfo.Extension.ToUpper() == ".EXE")
                                        {
                                            strFileName = pFileInfo.FullName.Substring(iAppPathLength, pFileInfo.FullName.Length - iAppPathLength).ToUpper();

                                            if (!arrFile.Remove("\\" + strFileName))
                                            {
                                                pSendContent.Add(DataField.UPDATE_NAME, DataFormat.STRING, strFileName);
                                                pSendContent.Add(DataField.UPDATE_STATE, DataFormat.SIGNED, pFileInfos.Length);
                                                pSendContent.AddRows();

                                                //mSendContents[iFileIndex, 0].eFormat = CEnum.TagFormat.TLV_STRING;
                                                //mSendContents[iFileIndex, 0].eName = CEnum.TagName.UPDATE_NAME;
                                                //mSendContents[iFileIndex, 0].Content = strFileName;

                                                //mSendContents[iFileIndex, 1].eFormat = CEnum.TagFormat.TLV_INTEGER;
                                                //mSendContents[iFileIndex, 1].eName = CEnum.TagName.UPDATE_STATE;
                                                //mSendContents[iFileIndex, 1].Content = pFileInfo.Length;

                                                iFileIndex++;
                                            }
                                        }
                                    }

                                    //枚举子目录文件
                                    foreach (DirectoryInfo pSubDirectory in pDirectory.GetDirectories())
                                    {
                                        foreach (FileInfo pFileInfo in pSubDirectory.GetFiles())
                                        {
                                            if (pFileInfo.Extension.ToUpper() == ".DLL" || pFileInfo.Extension.ToUpper() == ".EXE")
                                            {
                                                strFileName = pFileInfo.FullName.Substring(iAppPathLength, pFileInfo.FullName.Length - iAppPathLength).ToUpper();

                                                if (!arrFile.Remove("\\" + strFileName))
                                                {
                                                    pSendContent.Add(DataField.UPDATE_NAME, DataFormat.STRING, strFileName);
                                                    pSendContent.Add(DataField.UPDATE_STATE, DataFormat.SIGNED, pFileInfos.Length);
                                                    pSendContent.AddRows();

                                                    //mSendContents[iFileIndex, 0].eFormat = CEnum.TagFormat.TLV_STRING;
                                                    //mSendContents[iFileIndex, 0].eName = CEnum.TagName.UPDATE_NAME;
                                                    //mSendContents[iFileIndex, 0].Content = strFileName;

                                                    //mSendContents[iFileIndex, 1].eFormat = CEnum.TagFormat.TLV_INTEGER;
                                                    //mSendContents[iFileIndex, 1].eName = CEnum.TagName.UPDATE_STATE;
                                                    //mSendContents[iFileIndex, 1].Content = pFileInfo.Length;

                                                    iFileIndex++;
                                                }
                                            }
                                        }
                                    }

                                    //pSendBuffer = mSocketDate.setSocketData(EnumConvert.getResponseKey(mTagID), mSocketDate.pPacket.m_Head.eCategory, mSendContents, iCurren).bMsgBuffer;
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    pSendContent.Add("无需要更新的文件！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 文件下载
                            case TaskService.SYSTEM_UPDATE_DOWN_GET:
                                try
                                {
                                    FileStream pFileStream = null;

                                    if (File.Exists(".\\update" + mReciceContent[0, 0].Content.ToString()))
                                    {
                                        pFileStream = new FileStream(".\\update" + mReciceContent[0, 0].Content.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read);
                                    }
                                    else
                                    {
                                        pFileStream = new FileStream(".\\update\\" + mReciceContent[0, 0].Content.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read);
                                    }
                                    byte[] pFileContent = new byte[int.Parse(mReciceContent[0, 1].Content.ToString())];
                                    pFileStream.Position = int.Parse(mReciceContent[0, 2].Content.ToString());
                                    pFileStream.Read(pFileContent, 0, pFileContent.Length);
                                    pFileStream.Close();

                                    pSendBuffer = pFileContent;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);

                                    pSendContent.Add("无法下载需要更新的文件！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #endregion

                            #region 用户信息管理
                            #region 添加工作组信息 Finished!
                            case TaskService.MEMBER_GROUP_PUT:
                                strProcParams = "SP_PUT_GROUPINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
                                                   new SqlParameter("@strDesc",SqlDbType.VarChar,100)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取工作组信息 Finished!
                            case TaskService.MEMBER_GROUP_LIST:
                                strProcParams = "SP_GET_GROUPINFO";

                                pADOUtils.ExecuteQuery(strProcParams);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定工作组信息 Finished!
                            case TaskService.MEMBER_GROUP_UPDATE:
                                strProcParams = "SP_UPDATE_GROUPINFO";
                                strContent = "Group_Name = '%s', Group_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定工作组信息 Finished!
                            case TaskService.MEMBER_GROUP_DELETE:
                                strProcParams = "SP_DELETE_GROUPINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加人员信息 Finished!
                            case TaskService.MEMBER_USER_PUT:
                                strProcParams = "SP_PUT_MEMBERINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGroup",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strAddr",SqlDbType.VarChar,50),
												   new SqlParameter("@strTel",SqlDbType.VarChar,50),
												   new SqlParameter("@strMobile",SqlDbType.VarChar,50),
												   new SqlParameter("@strIM",SqlDbType.VarChar,100),
                                                   new SqlParameter("@strDesc",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取人员信息 Finished!
                            case TaskService.MEMBER_USER_LIST:
                                strProcParams = "SP_GET_MEMBERINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定人员信息 Finished!
                            case TaskService.MEMBER_USER_UPDATE:
                                strProcParams = "SP_UPDATE_MEMBERINFO";
                                strContent = "Member_Group = '%s', Member_Name = '%s', Member_Addr = '%s', Member_Tel = '%s', Member_Mobile = '%s', Member_IM = '%s', Member_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定人员信息 Finished!
                            case TaskService.MEMBER_USER_DELETE:
                                strProcParams = "SP_DELETE_MEMBERINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加帐号信息 Finished!
                            case TaskService.MEMBER_ACCOUNT_PUT:
                                strProcParams = "SP_PUT_VERIFYINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iMember",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.VarChar,50),
												   new SqlParameter("@strNick",SqlDbType.VarChar,50),
												   new SqlParameter("@strPwd",SqlDbType.VarChar,50),
                                                   new SqlParameter("@iTerm",SqlDbType.SmallDateTime,15),
                                                   new SqlParameter("@iDesc",SqlDbType.VarChar,200)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取帐号信息 Finished!
                            case TaskService.MEMBER_ACCOUNT_LIST:
                                strProcParams = "SP_GET_VERIFYINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]
                                {
                                    new SqlParameter("iGroup",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定帐号信息 Finished!
                            case TaskService.MEMBER_ACCOUNT_UPDATE:
                                strProcParams = "SP_UPDATE_VERIFYINFO";
                                //strContent = "Verify_Sort = '%s', Verify_Nick = '%s', Verify_Term = '%s', Verify_Status = '%s', Verify_Pwd = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   //new SqlParameter("@strContent",SqlDbType.VarChar,500),
                                                   new SqlParameter("@iSort",SqlDbType.Int,4),
                                                   new SqlParameter("@iNick",SqlDbType.VarChar,500),
                                                   new SqlParameter("@iTerm",SqlDbType.SmallDateTime,15),
                                                   new SqlParameter("@iStatus",SqlDbType.Int,4),
                                                   new SqlParameter("@iPwd",SqlDbType.VarChar,500),
                                                   new SqlParameter("@iDesc",SqlDbType.VarChar,200)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                //paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定帐号信息 Finished!
                            case TaskService.MEMBER_ACCOUNT_DELETE:
                                strProcParams = "SP_DELETE_VERIFYINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加指定帐号0权限信息 Finished!
                            case TaskService.MEMBER_POPEDOM_PUT:
                                bFailure = false;

                                strProcParams = "SP_DELETE_POPEDOMINFO";

                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };

                                paramCache[0].Value = 0;
                                paramCache[1].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                strPopedomCount = mReciceContent[0, 1].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strPopedomCount.Length; i++)
                                {
                                    if (strPopedomCount[i] != "N/A" && strPopedomCount[i] != "0" && strPopedomCount[i].Length != 0)
                                    {
                                        strProcParams = "SP_PUT_POPEDOMINFO";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iTag",SqlDbType.Int,4)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = 0;
                                        paramCache[1].Value = mReciceContent[0, 0].Content;
                                        paramCache[2].Value = strPopedomCount[i];

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow < 0)
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (-1 < pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 获取指定用户权限0信息 Finished!
                            case TaskService.MEMBER_POPEDOM_LIST:
                                strProcParams = "SP_GET_POPEDOMINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = 0;
                                paramCache[1].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定用户权限0信息 Finished!
                            case TaskService.MEMBER_POPEDOM_UPDATE:
                                bFailure = false;

                                strProcParams = "SP_DELETE_POPEDOMINFO";

                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = 0;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                strPopedomCount = mReciceContent[0, 1].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strPopedomCount.Length; i++)
                                {
                                    if (strPopedomCount[i] != "N/A" && strPopedomCount[i] != "0" && strPopedomCount[i].Length != 0)
                                    {
                                        strProcParams = "SP_UPDATE_POPEDOMINFO";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iTag",SqlDbType.Int,4)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = 0;
                                        paramCache[1].Value = mReciceContent[0, 0].Content;
                                        paramCache[2].Value = strPopedomCount[i];

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow < 0)
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (-1 < pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 删除指定用户权限0信息 No Used!
                            case TaskService.MEMBER_POPEDOM_DELETE:
                                strProcParams = "SP_DELETE_POPEDOMINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 获取指定权限详细信息 Finished!
                            case TaskService.MEMBER_POPEDOM_DETAIL:
                                strProcParams = "SP_GET_POPEDOMDETAIL";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = 0;
                                paramCache[1].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定口令信息 Finished!
                            case TaskService.MEMBER_PASSWORD_UPDATE:
                                strProcParams = "SP_UPDATE_VERIFYPWD";
                                strContent = "Verify_Pwd = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 获取用户帐号权限为1的权限信息
                            case TaskService.MEMBER_SERVER_GET:
                                strProcParams = "SP_GET_POPEDOMINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = 1;
                                paramCache[1].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 添加用户帐号权限为1的权限信息 Finished!
                            case TaskService.MEMBER_SERVER_PUT:
                                bFailure = false;

                                strProcParams = "SP_DELETE_POPEDOMINFO";

                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };

                                paramCache[0].Value = 1;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                strPopedomCount = mReciceContent[0, 2].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strPopedomCount.Length; i++)
                                {
                                    if (strPopedomCount[i] != "N/A" && strPopedomCount[i] != "0" && strPopedomCount[i].Length != 0)
                                    {
                                        strProcParams = "SP_PUT_POPEDOMINFO";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iTag",SqlDbType.Int,4)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = 1;
                                        paramCache[1].Value = mReciceContent[0, 1].Content;
                                        paramCache[2].Value = strPopedomCount[i];

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow < 0)
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (-1 < pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 修改用户服务器操作权限 Finished
                            case TaskService.MEMBER_SERVER_UPDATE:
                                bFailure = false;

                                strProcParams = "SP_DELETE_POPEDOMINFO";

                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };

                                paramCache[0].Value = mReciceContent[0, 1].Content;
                                paramCache[1].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                strPopedomCount = mReciceContent[0, 2].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strPopedomCount.Length; i++)
                                {
                                    if (strPopedomCount[i] != "N/A" && strPopedomCount[i] != "0" && strPopedomCount[i].Length != 0)
                                    {
                                        strProcParams = "SP_UPDATE_POPEDOMINFO";

                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iTag",SqlDbType.Int, 4)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = mReciceContent[0, 0].Content;
                                        paramCache[1].Value = mReciceContent[0, 1].Content;
                                        paramCache[2].Value = strPopedomCount[i];

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow < 0)
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (-1 < pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 删除用户服务器操作权限 -- 不知道是否使用着
                            case TaskService.MEMBER_SERVER_DELETE:
                                strProcParams = "SP_DELETE_POPEDOMINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 获取用户有操作权限的游戏列表
                            case TaskService.MEMBER_GAME_POPEDOM:
                                strProcParams = "SP_GET_POPEDOMGAME";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iMember",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取指定游戏下用户有操作权限的服务器列表
                            case TaskService.MEMBER_SERVER_POPEDOM:
                                strProcParams = "SP_GET_POPEDOMSERVER";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iMember",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 访问口令
                            case TaskService.MEMBER_SERVERACCESS_PUT:
                                strProcParams = "SP_SERVERACCESS_PUT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@strUser",SqlDbType.VarChar,50),
												   new SqlParameter("@strPwd",SqlDbType.VarChar,50),
                                                   new SqlParameter("@strDesc",SqlDbType.Text),
                                                   new SqlParameter("@iUser",SqlDbType.Int, 4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (0 < pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MEMBER_SERVERACCESS_UPDATE:
                                strProcParams = "SP_SERVERACCESS_UPDATE";
                                strContent = "Access_Server = '%s', Access_Sort = '%s', Access_User = '%s', Access_Pwd = '%s', Access_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MEMBER_SERVERACCESS_DELETE:
                                strProcParams = "SP_SERVERACCESS_DELETE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MEMBER_SERVERACCESS_LIST:
                                strProcParams = "SP_SERVERACCESS_GET";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndex",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iUser",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 访问口令权限
                            #region
                            case TaskService.MEMBER_SERVERACCESSSORT_GET:
                                strProcParams = "SP_SERVERACCESSSORT_GET";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iUser",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4)                                                    
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MEMBER_SERVERACCESSBYUSER_PUT:
                                strProcParams = "SP_SERVERACCESSBYUSER_PUT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.Text),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新用户权限——自定义
                            case TaskService.MEMBER_SERVERACCESSBYUSER_SORT_UPDATE:
                                bFailure = false;
                                string[] str_verify = mReciceContent[0, 0].Content.ToString().Split("|".ToCharArray());//人员
                                string[] str_server = mReciceContent[0, 1].Content.ToString().Split("|".ToCharArray());//服务器id
                                string[] str_sort = mReciceContent[0, 2].Content.ToString().Replace("999", "-1").Split("|".ToCharArray());//权限

                                for (int i = 0; i < str_verify.Length; i++)
                                {
                                    strProcParams = "SP_DELETE_POPEDOMINFO";
                                    paramCache = new SqlParameter[]{
                                                    new SqlParameter("@iKey",SqlDbType.Int,4),
                                                    new SqlParameter("@iSort",SqlDbType.Int,4)
                                };

                                    paramCache[0].Value = str_verify[i].ToString();
                                    paramCache[1].Value = -1;
                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);
                                }

                                for (int i = 0; i < str_verify.Length; i++)
                                {
                                    for (int j = 0; j < str_server.Length; j++)
                                    {
                                        for (int m = 0; m < str_sort.Length; m++)
                                        {
                                            strProcParams = "SP_SERVERACCESSBYUSER_UPDATE";
                                            paramCache = DataUtilities.GetParameters(strProcParams);

                                            if (paramCache == null)
                                            {
                                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iPopedom",SqlDbType.Int, 4),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                                DataUtilities.SetParameters(strProcParams, paramCache);
                                            }

                                            paramCache[0].Value = int.Parse(str_sort[m].ToString()) + 2;
                                            paramCache[1].Value = str_verify[i].ToString();
                                            paramCache[2].Value = str_server[j].ToString();
                                            paramCache[3].Value = "N/A";

                                            pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                            pADOUtils.GetResult(RecordStyle.NONE);

                                            if (pADOUtils.AffectRow < 0)
                                            {
                                                pSendContent.Add("FAILURE");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                                bFailure = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 更新用户权限——按类别
                            case TaskService.MEMBER_SERVERACCESSBYUSER_SORT_UPDATE_KIND:
                                string[] Str_Verify = mReciceContent[0, 0].Content.ToString().Split("|".ToCharArray());//人员
                                string[] Str_Game = mReciceContent[0, 1].Content.ToString().Split("|".ToCharArray());//游戏id
                                string[] Str_Kind = mReciceContent[0, 2].Content.ToString().Split("|".ToCharArray());//操作系统
                                string[] Str_ServerType = mReciceContent[0, 3].Content.ToString().Split("|".ToCharArray());//服务器类型
                                string[] Str_Sort = mReciceContent[0, 4].Content.ToString().Replace("999", "-1").Split("|".ToCharArray());//权限

                                for (int i = 0; i < Str_Verify.Length; i++)
                                {
                                    strProcParams = "SP_DELETE_POPEDOMINFO";

                                    paramCache = new SqlParameter[]{
                                                    new SqlParameter("@iKey",SqlDbType.Int,4),
                                                    new SqlParameter("@iSort",SqlDbType.Int,4)
                            };

                                    paramCache[0].Value = Str_Verify[i].ToString();
                                    paramCache[1].Value = -1;
                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);
                                }

                                for (int i = 0; i < Str_Verify.Length; i++)
                                {
                                    for (int j = 0; j < Str_Game.Length; j++)
                                    {
                                        for (int p = 0; p < Str_Kind.Length; p++)
                                        {
                                            for (int m = 0; m < Str_ServerType.Length; m++)
                                            {
                                                for (int n = 0; n < Str_Sort.Length; n++)
                                                {
                                                    strProcParams = "SP_SERVERACCESSBYUSER_UPDATE_KIND";
                                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                                    if (paramCache == null)
                                                    {
                                                        paramCache = new SqlParameter[]{
                                                    new SqlParameter("@iVerify",SqlDbType.Int,4),
                                                    new SqlParameter("@iGame",SqlDbType.Int,4),
                                                    new SqlParameter("@iKind",SqlDbType.Int,4),
                                                    new SqlParameter("@iSort",SqlDbType.Int,4),
                                                    new SqlParameter("@cSort",SqlDbType.Int,4)
                                                };
                                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                                    }

                                                    paramCache[0].Value = Str_Verify[i].ToString();
                                                    paramCache[1].Value = Str_Game[j].ToString();
                                                    paramCache[2].Value = Str_Kind[p].ToString();
                                                    paramCache[3].Value = Str_ServerType[m].ToString();
                                                    paramCache[4].Value = int.Parse(Str_Sort[n].ToString()) + 2;


                                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                                    pADOUtils.GetResult(RecordStyle.NONE);

                                                    if (pADOUtils.AffectRow < 0)
                                                    {
                                                        pSendContent.Add("FAILURE");
                                                        pSendContent.AddRows();

                                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                                        pSendBuffer = pSourcePacket.CoalitionInfo();

                                                        bFailure = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.MEMBER_SERVERACCESSBYUSER_UPDATE:
                                bFailure = false;

                                strProcParams = "SP_DELETE_POPEDOMINFO";

                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };

                                paramCache[0].Value = mReciceContent[0, 1].Content;
                                paramCache[1].Value = int.Parse(mReciceContent[0, 0].Content.ToString()) + 2;
                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                strPopedomCount = mReciceContent[0, 2].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strPopedomCount.Length; i++)
                                {
                                    if (strPopedomCount[i] != "N/A" && strPopedomCount[i] != "0" && strPopedomCount[i].Length != 0)
                                    {
                                        strProcParams = "SP_SERVERACCESSBYUSER_UPDATE";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iVerify",SqlDbType.Int,4),
												   new SqlParameter("@iPopedom",SqlDbType.Int, 4),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = int.Parse(mReciceContent[0, 0].Content.ToString()) + 2;
                                        paramCache[1].Value = mReciceContent[0, 1].Content;
                                        paramCache[2].Value = strPopedomCount[i];
                                        paramCache[3].Value = "N/A";

                                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow < 0)
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }

                                try
                                {
                                    //pSendBuffer = mSocketDate.setSocketData(EnumConvert.getResponseKey(mTagID), mSocketDate.pPacket.m_Head.eCategory, mSendContent).bMsgBuffer;

                                    strProcParams = "SP_SERVERACCESSBYUSER_SELECT_INSERT_SORT";

                                    paramCache = new SqlParameter[]{
                                                  new SqlParameter("@iVerify",SqlDbType.Int,4)
                                                };

                                    paramCache[0].Value = mReciceContent[0, 1].Content;
                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);
                                }
                                catch
                                {
                                }

                                break;
                            #endregion
                            #region
                            case TaskService.MEMBER_SERVERACCESSBYUSER_DELETE:
                                strProcParams = "SP_SERVERACCESSBYUSER_DELETE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;



                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MEMBER_SERVERACCESSBYUSER_LIST:
                                strProcParams = "SP_SERVERACCESSBYUSER_GET";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iUser",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;


                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 删除指定用户的权限
                            case TaskService.MEMBER_USERPOPEDOM_DELETE:
                                strProcParams = "SP_DELETE_USERPOPEDOM";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                    new SqlParameter("@iUser",SqlDbType.Int,4),
                                    new SqlParameter("@iGame",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 <= pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 用户口令密码查询
                            case TaskService.MEMBER_SERVERACCESSBYUSER_SELECT:
                                strProcParams = "SP_GET_SERVERACCESSBYUSER";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                    new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                    new SqlParameter("@Game_ID",SqlDbType.Int,4)
                                };
                                    DataUtilities.GetParameters(strProcParams);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region
                                    strProcParams = "SP_GET_SERVERACCESSBYUSER_COUNT";
                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (null == paramCache)
                                    {
                                        paramCache = new SqlParameter[]{
                                    new SqlParameter("@Verify_ID",SqlDbType.Int,4),
                                    new SqlParameter("@Game_ID",SqlDbType.Int,4)
                                    };
                                        DataUtilities.GetParameters(strProcParams);
                                    }

                                    paramCache[0].Value = mReciceContent[0, 2].Content;
                                    paramCache[1].Value = mReciceContent[0, 3].Content;

                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.DATASET);
                                    DataSet pDataSet_Count = (DataSet)pADOUtils.RecordData;
                                    #endregion
                                    #region
                                    int iDataSet_Count = int.Parse(pDataSet_Count.Tables[0].Rows[0][0].ToString());

                                    DataSet pResult = new DataSet();
                                    pResult.Tables.Add();
                                    pResult.Tables[0].Columns.Add("Failure_Area");
                                    pResult.Tables[0].Columns.Add("Server_Name");
                                    pResult.Tables[0].Columns.Add("Server_Internet");

                                    for (int i = 1; i <= iDataSet_Count; i++)
                                    {
                                        string str_user = "User" + i.ToString();
                                        string str_pwd = "Pwd" + i.ToString();
                                        pResult.Tables[0].Columns.Add(str_user);
                                        pResult.Tables[0].Columns.Add(str_pwd);
                                    }

                                    iDataSet_Count = (iDataSet_Count * 2) + 3;
                                    while (0 != pDataSet.Tables[0].Rows.Count)
                                    {
                                        object[] pData = new object[iDataSet_Count];

                                        pData[0] = pDataSet.Tables[0].Rows[0][0].ToString();
                                        pData[1] = pDataSet.Tables[0].Rows[0][1].ToString();
                                        pData[2] = pDataSet.Tables[0].Rows[0][2].ToString();

                                        int cba = int.Parse(pDataSet.Tables[0].Rows[0][6].ToString());
                                        cba = (cba * 2) + 3;
                                        pData[cba] = pDataSet.Tables[0].Rows[0][3].ToString();
                                        pData[cba + 1] = pDataSet.Tables[0].Rows[0][4].ToString();

                                        string a = pDataSet.Tables[0].Rows[0][7].ToString();
                                        for (int i = 1; i < pDataSet.Tables[0].Rows.Count; i++)
                                        {
                                            if (a == pDataSet.Tables[0].Rows[i][7].ToString())
                                            {
                                                int abc = int.Parse(pDataSet.Tables[0].Rows[i][6].ToString());
                                                abc = (abc * 2) + 3;
                                                pData[abc] = pDataSet.Tables[0].Rows[i][3].ToString();
                                                pData[abc + 1] = pDataSet.Tables[0].Rows[i][4].ToString();
                                                i--;
                                                pDataSet.Tables[0].Rows.Remove(pDataSet.Tables[0].Rows[i]);
                                            }
                                        }

                                        pResult.Tables[0].Rows.Add(pData);
                                        pDataSet.Tables[0].Rows.Remove(pDataSet.Tables[0].Rows[0]);
                                    }
                                    #endregion

                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    //pSendBuffer = mSocketDate.setSocketData(EnumConvert.getResponseKey(mTagID), mSocketDate.pPacket.m_Head.eCategory, buildTLV(pResult, Int32.Parse(mReciceContent[0, 0].Content.ToString()), Int32.Parse(mReciceContent[0, 1].Content.ToString()), false), iCurren).bMsgBuffer;
                                }
                                break;
                            #endregion

                            #region 清除用户mac码
                            case TaskService.MEMBER_VERIFY_UPDATE:
                                strProcParams = "SP_UPDATE_VERIFY";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                    new SqlParameter("@iKey",SqlDbType.Int,4),
                                                    new SqlParameter("@strContent",SqlDbType.VarChar,500)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = "Verify_Sign='N/A',Verify_Status=0,Verify_Session='N/A'";

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取指定人员游戏列表
                            case TaskService.MEMBER_GAMEOFUSER_GET:
                                strProcParams = "SP_GET_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 服务器信息收集
                            #region 添加游戏信息 Finished!
                            case TaskService.SERVERINFO_GAME_PUT:
                                strProcParams = "SP_PUT_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strManager",SqlDbType.VarChar,50),
                                                   new SqlParameter("@strDesc",SqlDbType.VarChar,50),
												   new SqlParameter("@strDB",SqlDbType.VarChar,50),
                                                   new SqlParameter("@strPort",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取游戏列表 Finished!
                            case TaskService.SERVERINFO_GAME_LIST:
                                strProcParams = "SP_GET_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = 0;
                                paramCache[1].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定游戏信息 Finished!
                            case TaskService.SERVERINFO_GAME_UPDATE:
                                strProcParams = "SP_UPDATE_GAMEINFO";
                                strContent = "Game_Name = '%s', Game_Manager = '%s', Game_Desc = '%s', Game_DB = '%s', Game_Port = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定游戏信息 Finished!
                            case TaskService.SERVERINFO_GAME_DELETE:
                                strProcParams = "SP_DELETE_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 添加服务器信息 Finished!
                            case TaskService.SERVERINFO_SERVER_PUT:
                                strProcParams = "SP_PUT_SERVERINFONEW";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strIntranet",SqlDbType.VarChar,50),
												   new SqlParameter("@strInternet",SqlDbType.VarChar,50),
												   new SqlParameter("@strPosition",SqlDbType.Text),
												   new SqlParameter("@strHouse",SqlDbType.Text),
												   new SqlParameter("@strTel",SqlDbType.Text),
												   new SqlParameter("@strConfiguration",SqlDbType.Text),
                                                   new SqlParameter("@strDesc",SqlDbType.Text),
                                                   new SqlParameter("@iUserID",SqlDbType.Int,4),
                                                   new SqlParameter("@iStatus",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4),
                                                   new SqlParameter("@iDbtype",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;
                                paramCache[11].Value = mReciceContent[0, 11].Content;
                                paramCache[12].Value = mReciceContent[0, 12].Content;
                                paramCache[13].Value = mReciceContent[0, 13].Content;
                                paramCache[14].Value = mReciceContent[0, 14].Content;
                                paramCache[15].Value = mReciceContent[0, 15].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (0 < pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取服务器列表 Finished!
                            case TaskService.SERVERINFO_SERVER_LIST:
                                strProcParams = "SP_GET_SERVERINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iUser",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;
                                paramCache[4].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新指定服务器信息 Finished!
                            case TaskService.SERVERINFO_SERVER_UPDATE:
                                strProcParams = "SP_UPDATE_SERVERINFO";
                                strContent = "Server_Game = '%s', Server_Layer = '%s', Server_Sort = '%s', Server_Kind = '%s', Server_Name = '%s', Server_Intranet = '%s', Server_Internet = '%s', Server_Position = '%s', Server_House = '%s', Server_Tel = '%s', Server_Configuration = '%s', Server_Desc = '%s', Server_Status = '%s', Server_DBtype = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除指定服务器信息 Finished!
                            case TaskService.SERVERINFO_SERVER_DELETE:
                                strProcParams = "SP_DELETE_SERVERINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region IP导入及口令导入
                            #region
                            case TaskService.SERVERINFO_SERVERC_IMPORT:
                                bFailure = false;

                                for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                {
                                    strProcParams = "SP_PUT_SERVERINFONEW";
                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (paramCache == null)
                                    {
                                        paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strIntranet",SqlDbType.VarChar,50),
												   new SqlParameter("@strInternet",SqlDbType.VarChar,50),
												   new SqlParameter("@strPosition",SqlDbType.Text),
												   new SqlParameter("@strHouse",SqlDbType.Text),
												   new SqlParameter("@strTel",SqlDbType.Text),
												   new SqlParameter("@strConfiguration",SqlDbType.Text),
                                                   new SqlParameter("@strDesc",SqlDbType.Text),
                                                   new SqlParameter("@iUserID",SqlDbType.Int,4),
                                                   new SqlParameter("@iStatus",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4),
                                                   new SqlParameter("@iDbtype",SqlDbType.Int,4)
											   };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = mReciceContent[i, 0].Content;
                                    paramCache[1].Value = mReciceContent[i, 1].Content;
                                    paramCache[2].Value = mReciceContent[i, 2].Content;
                                    paramCache[3].Value = mReciceContent[i, 3].Content;
                                    paramCache[4].Value = mReciceContent[i, 4].Content;
                                    paramCache[5].Value = mReciceContent[i, 5].Content;
                                    paramCache[6].Value = mReciceContent[i, 6].Content;
                                    paramCache[7].Value = mReciceContent[i, 7].Content;
                                    paramCache[8].Value = mReciceContent[i, 8].Content;
                                    paramCache[9].Value = mReciceContent[i, 9].Content;
                                    paramCache[10].Value = mReciceContent[i, 10].Content;
                                    paramCache[11].Value = mReciceContent[i, 11].Content;
                                    paramCache[12].Value = mReciceContent[i, 12].Content;
                                    paramCache[13].Value = mReciceContent[i, 13].Content;
                                    paramCache[14].Value = mReciceContent[0, 14].Content;
                                    paramCache[15].Value = mReciceContent[0, 15].Content;

                                    pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);

                                    if (pADOUtils.AffectRow == -1 || pADOUtils.ProcReturn.ToString() == "-1")
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();

                                        bFailure = true;
                                        break;
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                pDataSet = null;
                                break;
                            #endregion
                            #region
                            case TaskService.SERVERINFO_SERVERD_IMPORT:
                                bFailure = false;

                                for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                {
                                    if (mReciceContent[i, 2].Content.ToString() == "N/A" || mReciceContent[i, 3].Content.ToString() == "N/A")
                                    { }
                                    else
                                    {
                                        strProcParams = "SP_SERVERINFO_IMPORT";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strLayer",SqlDbType.VarChar,50),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strIntranet",SqlDbType.VarChar,50),
												   new SqlParameter("@strInternet",SqlDbType.VarChar,50),
												   new SqlParameter("@strPosition",SqlDbType.Text),
												   new SqlParameter("@strHouse",SqlDbType.Text),
												   new SqlParameter("@strTel",SqlDbType.Text),
												   new SqlParameter("@strConfiguration",SqlDbType.Text),
                                                   new SqlParameter("@strDesc",SqlDbType.Text),
                                                   new SqlParameter("@iUserID",SqlDbType.Int,4),
                                                   new SqlParameter("@iStatus",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4),
                                                   new SqlParameter("@iDbtype",SqlDbType.Int,4)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = mReciceContent[i, 0].Content;
                                        paramCache[1].Value = mReciceContent[i, 1].Content;
                                        paramCache[2].Value = mReciceContent[i, 2].Content;
                                        paramCache[3].Value = mReciceContent[i, 3].Content;
                                        paramCache[4].Value = mReciceContent[i, 4].Content;
                                        paramCache[5].Value = mReciceContent[i, 5].Content;
                                        paramCache[6].Value = mReciceContent[i, 6].Content;
                                        paramCache[7].Value = mReciceContent[i, 7].Content;
                                        paramCache[8].Value = mReciceContent[i, 8].Content;
                                        paramCache[9].Value = mReciceContent[i, 9].Content;
                                        paramCache[10].Value = mReciceContent[i, 10].Content;
                                        paramCache[11].Value = mReciceContent[i, 11].Content;
                                        paramCache[12].Value = mReciceContent[i, 12].Content;
                                        paramCache[13].Value = mReciceContent[i, 13].Content;
                                        paramCache[14].Value = mReciceContent[i, 14].Content;
                                        paramCache[15].Value = mReciceContent[0, 15].Content;


                                        pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);

                                        if (pADOUtils.AffectRow == -1 || pADOUtils.ProcReturn.ToString() == "-1")
                                        {
                                            pSendContent.Add("FAILURE");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            pSendBuffer = pSourcePacket.CoalitionInfo();

                                            bFailure = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.SERVERINFO_SERVERP_IMPORT:
                                bFailure = false;

                                for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                {
                                    strProcParams = "SP_SERVERACCESS_IMPORT";
                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (paramCache == null)
                                    {
                                        paramCache = new SqlParameter[]{
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strServer",SqlDbType.VarChar,50),
												   new SqlParameter("@strAddr",SqlDbType.VarChar,50),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@strUser",SqlDbType.VarChar,50),
												   new SqlParameter("@strPwd",SqlDbType.VarChar,50),
												   new SqlParameter("@strDesc",SqlDbType.Text),
												   new SqlParameter("@iUser",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = mReciceContent[i, 0].Content;
                                    paramCache[1].Value = mReciceContent[i, 1].Content;
                                    paramCache[2].Value = mReciceContent[i, 2].Content;
                                    paramCache[3].Value = mReciceContent[i, 3].Content;
                                    paramCache[4].Value = mReciceContent[i, 4].Content;
                                    paramCache[5].Value = mReciceContent[i, 5].Content;
                                    paramCache[6].Value = mReciceContent[i, 6].Content;
                                    paramCache[7].Value = mReciceContent[i, 7].Content;
                                    paramCache[8].Value = mReciceContent[i, 8].Content;
                                    paramCache[9].Value = mReciceContent[i, 9].Content;

                                    pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);

                                    if (pADOUtils.AffectRow == -1 || pADOUtils.ProcReturn.ToString() == "-1")
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();

                                        bFailure = true;

                                        break;
                                    }
                                }

                                if (!bFailure)
                                {
                                    if (pADOUtils.AffectRow > -1)
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #endregion
                            #region 采集阈值新功能
                            //获取有采集阈值的控制机列表
                            case TaskService.SERVERINFO_SERVER_CRITERION_LIST:
                                strProcParams = "SP_GET_GAMESERVER_SERVERCRITERION";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                    new SqlParameter("@iKey",SqlDbType.Int,4),
                                                    new SqlParameter("@iGroup",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            //同一类型服务器的阈值进行同步
                            case TaskService.SERVERINFO_SERVER_CRITERION_PUT:
                                strProcParams = "SP_PUT_GAMESERVER_SERVERCRITERION";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                    new SqlParameter("@serverid",SqlDbType.Int,4),
                                                    new SqlParameter("@type",SqlDbType.Int,4),
                                                    new SqlParameter("@iGroup",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 2].Content;
                                paramCache[2].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 以IP地址进行服务器查询
                            case TaskService.SERVERINFO_SERVER_GETBYIP:
                                strProcParams = "SP_GET_SERVERINFOBYIP";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iArea",SqlDbType.Int,4),
												   new SqlParameter("@strIP",SqlDbType.VarChar,20),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 服务器组合查询
                            case TaskService.SERVERINFO_SERVER_PORTINFO_LIST:
                                strProcParams = "SP_GET_SERVERPORTINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);
                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]
                                {
                                    new SqlParameter("@Key",SqlDbType.VarChar,5000),
                                    new SqlParameter("@Verify",SqlDbType.Int,4),
                                    new SqlParameter("@iGroup",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 3].Content;
                                paramCache[1].Value = mReciceContent[0, 2].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 >= pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 游戏所属服务器统计
                            case TaskService.SERVERINFO_GET_SERVER_COUNT:
                                strProcParams = "SP_GET_SERVERCOUNT";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                    new SqlParameter("@Game_ID",SqlDbType.Int,4),
                                    new SqlParameter("@Layer_ID",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取某控制机下服务器信息
                            case TaskService.SERVERINFO_GAMESERVER_LIST:
                                strProcParams = "SP_GET_SERVERACCESSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                        new SqlParameter("@server_id",SqlDbType.Int,4),
                                                        new SqlParameter("@iGroup",SqlDbType.Int,4)
                                                        
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (null == pDataSet)
                                {
                                    pSendContent.Add("相应口令信息尚未添加");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    if (0 == pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("相应口令信息尚未添加");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 对IPList文件进行批量修改
                            case TaskService.SERVERINFO_GAMESERVER_IPLIST_UPDATE:
                                strProcParams = "SP_UPDATE_GAMESERVER_IPLIST";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                                new SqlParameter("@iServer",SqlDbType.Int,4),
                                                new SqlParameter("@check_Kind",SqlDbType.Int,4),
                                                new SqlParameter("@check_House",SqlDbType.Int,4),
                                                new SqlParameter("@str_House",SqlDbType.VarChar,5000),
                                                new SqlParameter("@check_Tel",SqlDbType.Int,4),
                                                new SqlParameter("@str_Tel",SqlDbType.VarChar,5000),
                                                new SqlParameter("@check_Position",SqlDbType.Int,4),
                                                new SqlParameter("@str_Position",SqlDbType.VarChar,5000),
                                                new SqlParameter("@check_Configuration",SqlDbType.Int,4),
                                                new SqlParameter("@str_Configuration",SqlDbType.VarChar,5000),
                                                new SqlParameter("@check_Desc",SqlDbType.Int,4),
                                                new SqlParameter("@str_Desc",SqlDbType.VarChar,5000),
                                                new SqlParameter("@check_Status",SqlDbType.Int,4),
                                                new SqlParameter("@str_Status",SqlDbType.VarChar,5000),

                                                new SqlParameter("@check_Sort1",SqlDbType.Int,4),
                                                new SqlParameter("@str_user1",SqlDbType.VarChar,500),
                                                new SqlParameter("@str_pwd1",SqlDbType.VarChar,500),
                                                new SqlParameter("@check_Sort2",SqlDbType.Int,4),
                                                new SqlParameter("@str_user2",SqlDbType.VarChar,500),
                                                new SqlParameter("@str_pwd2",SqlDbType.VarChar,500),
                                                new SqlParameter("@check_Sort3",SqlDbType.Int,4),
                                                new SqlParameter("@str_user3",SqlDbType.VarChar,500),
                                                new SqlParameter("@str_pwd3",SqlDbType.VarChar,500),
                                                new SqlParameter("@check_Sort4",SqlDbType.Int,4),
                                                new SqlParameter("@str_user4",SqlDbType.VarChar,500),
                                                new SqlParameter("@str_pwd4",SqlDbType.VarChar,500),
                                                new SqlParameter("@check_Sort5",SqlDbType.Int,4),
                                                new SqlParameter("@str_user5",SqlDbType.VarChar,500),
                                                new SqlParameter("@str_pwd5",SqlDbType.VarChar,500),

                                                new SqlParameter("@iGroup",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;
                                paramCache[11].Value = mReciceContent[0, 11].Content;
                                paramCache[12].Value = mReciceContent[0, 12].Content;
                                paramCache[13].Value = mReciceContent[0, 13].Content;
                                paramCache[14].Value = mReciceContent[0, 14].Content;

                                paramCache[15].Value = mReciceContent[0, 15].Content;
                                paramCache[16].Value = mReciceContent[0, 16].Content;
                                paramCache[17].Value = mReciceContent[0, 17].Content;
                                paramCache[18].Value = mReciceContent[0, 18].Content;
                                paramCache[19].Value = mReciceContent[0, 19].Content;
                                paramCache[20].Value = mReciceContent[0, 20].Content;
                                paramCache[21].Value = mReciceContent[0, 21].Content;
                                paramCache[22].Value = mReciceContent[0, 22].Content;
                                paramCache[23].Value = mReciceContent[0, 23].Content;
                                paramCache[24].Value = mReciceContent[0, 24].Content;
                                paramCache[25].Value = mReciceContent[0, 25].Content;
                                paramCache[26].Value = mReciceContent[0, 26].Content;
                                paramCache[27].Value = mReciceContent[0, 27].Content;
                                paramCache[28].Value = mReciceContent[0, 28].Content;
                                paramCache[29].Value = mReciceContent[0, 29].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (0 < pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 服务器提升
                            case TaskService.SERVERINFO_SERVERCONTROL_UPDATE:
                                strProcParams = "SP_SERVERCONTROL_UPDATE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iGameID",SqlDbType.Int),
												   new SqlParameter("@strServerIP",SqlDbType.VarChar,15),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 <= pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 显示帐号可看ip
                            case TaskService.SERVERINFO_SERVERIP_LIST:
                                strProcParams = "SP_GET_SERVERIP";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]{
                                    new SqlParameter("@Verify",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 服务器维护计划及故障统计
                            #region Notes部分
                            #region NOTES联系人获取
                            case TaskService.MAINTENANCE_LINKER_LIST:
                                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                                {
                                    if (pNotesUtils.GetLinkerInfo("Groups"))
                                    {
                                        pSendContent = (CustomDataCollection)pNotesUtils.Records;
                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                    else
                                    {
                                        pSendContent.Add(pNotesUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }
                                else
                                {
                                    pSendContent.Add(pNotesUtils.Message);
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region NOTES信息获取
                            case TaskService.MAINTENANCE_CONTENT_LIST:
                                strProcParams = "SP_GET_NOTESCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@dtBegin",SqlDbType.VarChar,500),
												   new SqlParameter("@dtEnd",SqlDbType.VarChar,500),
												   new SqlParameter("@iFailure",SqlDbType.Int,4),
												   new SqlParameter("@iStatus",SqlDbType.Int,4),
									               new SqlParameter("@iGroup",SqlDbType.Int,4)		   
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;
                                paramCache[4].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region NOTES信息发送
                            case TaskService.MAINTENANCE_CONTENT_SEND:
                                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                                {
                                    string[] arrSupervisors = mReciceContent[0, 5].Content.ToString().Split(";".ToCharArray());
                                    string[] arrSendSecret = mReciceContent[0, 6].Content.ToString().Split(";".ToCharArray());

                                    if (pNotesUtils.SendMailInfo(arrSupervisors, arrSendSecret, mReciceContent[0, 3].Content, mReciceContent[0, 7].Content.ToString()))
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add(pNotesUtils.Message);
                                        pSendContent.AddRows();
                                    }
                                }
                                else
                                {
                                    pSendContent.Add(pNotesUtils.Message);
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region NOTES信息转发
                            case TaskService.MAINTENANCE_CONTENT_REPLAY:
                                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                                {
                                    string[] arrSupervisors = mReciceContent[0, 5].Content.ToString().Split(";".ToCharArray());
                                    string[] arrSendSecret = mReciceContent[0, 6].Content.ToString().Split(";".ToCharArray());

                                    if (pNotesUtils.RelayMailInfo(arrSupervisors, arrSendSecret, mReciceContent[0, 2].Content.ToString(), mReciceContent[0, 7].Content.ToString()))
                                    {
                                        pSendContent.Add("SUCCEED");
                                        pSendContent.AddRows();
                                    }
                                    else
                                    {
                                        pSendContent.Add(pNotesUtils.Message);
                                        pSendContent.AddRows();
                                    }
                                }
                                else
                                {
                                    pSendContent.Add(pNotesUtils.Message);
                                    pSendContent.AddRows();
                                }
                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                break;
                            #endregion
                            #region NOTES信息附件获取
                            case TaskService.MAINTENANCE_ATTACHMENT_LIST:
                                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                                {
                                    if (pNotesUtils.GetMailAttachment(mReciceContent[0, 0].Content.ToString()))
                                    {
                                        pSendContent = (CustomDataCollection)pNotesUtils.Records;
                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                    else
                                    {
                                        pSendContent.Add("该邮件未找到，可能邮件已删除！");
                                        pSendContent.AddRows();
                                    }
                                }
                                else
                                {
                                    pSendContent.Add(pNotesUtils.Message);
                                    pSendContent.AddRows();
                                }
                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region NOTES信息查看修改
                            case TaskService.MAINTENANCE_CONTENT_UPDATE:
                                string[] arrNotesID = mReciceContent[0, 0].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < arrNotesID.Length; i++)
                                {
                                    strProcParams = "SP_UPDATE_NOTESCONTENT";
                                    paramCache = DataUtilities.GetParameters(strProcParams);

                                    if (paramCache == null)
                                    {
                                        paramCache = new SqlParameter[]{
                                                   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iFailure",SqlDbType.Int,4)
                                               };
                                        DataUtilities.SetParameters(strProcParams, paramCache);
                                    }

                                    paramCache[0].Value = arrNotesID[i];
                                    paramCache[1].Value = mReciceContent[0, 1].Content;

                                    pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.NONE);
                                }

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region NOTES信息删除
                            case TaskService.MAINTENANCE_CONTENT_DELETE:
                                strProcParams = "SP_DELETE_NOTESCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 故障统计部分
                            #region 错误统计流程
                            #region
                            case TaskService.MAINTENANCE_INFO_PUT:
                                strProcParams = "SP_PUT_SERVERFAILURE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iNotes",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iArea",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iRevice",SqlDbType.Int,4),
												   new SqlParameter("@dtBegin",SqlDbType.DateTime),
												   new SqlParameter("@dtEnd",SqlDbType.DateTime),
												   new SqlParameter("@strPostName",SqlDbType.VarChar, 50),
												   new SqlParameter("@strError",SqlDbType.Text),
												   new SqlParameter("@strResult",SqlDbType.Text),
												   new SqlParameter("@strMessage",SqlDbType.Text),
                                                   new SqlParameter("@iStatus",SqlDbType.Int)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;
                                paramCache[11].Value = mReciceContent[0, 11].Content;
                                paramCache[12].Value = mReciceContent[0, 12].Content;
                                paramCache[13].Value = mReciceContent[0, 13].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_INFO_UPDATE:
                                strProcParams = "SP_UPDATE_SERVERFAILURE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strPostName",SqlDbType.VarChar, 50),
												   new SqlParameter("@dtBegin",SqlDbType.DateTime),
												   new SqlParameter("@strError",SqlDbType.Text),
												   new SqlParameter("@strResult",SqlDbType.Text),
												   new SqlParameter("@iStatus",SqlDbType.Int, 4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_INFO_DELETE:
                                strProcParams = "SP_DELETE_SERVERFAILURE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_INFO_LIST:
                                strProcParams = "SP_GET_SERVERFAILURE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGame",SqlDbType.Int,4),
                                                   new SqlParameter("@iArea",SqlDbType.Int,4),
                                                   new SqlParameter("@iTemplate",SqlDbType.Int,4),
                                                   new SqlParameter("@dBegin",SqlDbType.DateTime),
                                                   new SqlParameter("@dEnd",SqlDbType.DateTime),
                                                   new SqlParameter("@iUser",SqlDbType.Int,4),
                                                   new SqlParameter("@iStatus",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
                                               };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;
                                paramCache[4].Value = mReciceContent[0, 6].Content;
                                paramCache[5].Value = mReciceContent[0, 7].Content;
                                paramCache[6].Value = mReciceContent[0, 8].Content;
                                paramCache[7].Value = mReciceContent[0, 9].Content;
                                paramCache[8].Value = mReciceContent[0, 10].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 >= pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region
                            case TaskService.MAINTENANCE_REPLAY_PUT:
                                strProcParams = "SP_PUT_FAILURERELAY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iNotes",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iFailure",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iRecive",SqlDbType.Int,4),
												   new SqlParameter("@strReciveName",SqlDbType.VarChar, 50),
												   new SqlParameter("@strMessage",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_REPLAY_UPDATE:
                                strProcParams = "SP_UPDATE_FAILURERELAY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iNotes",SqlDbType.Int,4),
												   new SqlParameter("@iLayer",SqlDbType.Int,4),
												   new SqlParameter("@iFailure",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@dtEnd",SqlDbType.DateTime),
												   new SqlParameter("@strPostName",SqlDbType.VarChar, 50),
												   new SqlParameter("@strMessage",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_REPLAY_DELETE:
                                strProcParams = "SP_PUT_FAILURERELAY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_REPLAY_LIST:
                                strProcParams = "SP_GET_FAILURERELAY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iRecive",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (1 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 错误统计--以模板类别为条件
                            case TaskService.MAINTENANCE_COUNT_BYTEMPLATEGET:
                                strProcParams = "SP_GET_ERRORCOUNT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndexs",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@strServer",SqlDbType.VarChar,1000),
												   new SqlParameter("@iUser",SqlDbType.Int,4),
												   new SqlParameter("@dBegin",SqlDbType.DateTime,20),
												   new SqlParameter("@dEnd",SqlDbType.DateTime,20),
												   new SqlParameter("@strArea",SqlDbType.VarChar,50),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.VarChar,50),
									               new SqlParameter("@iGroup",SqlDbType.Int,4)			   
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 系统维护部分...
                            #region 维护信息
                            case TaskService.MAINTENANCE_HANDLING_INFO_PUT:
                                strProcParams = "SP_PUT_SERVERHANDLING";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@dtDate",SqlDbType.DateTime),
												   new SqlParameter("@strArea",SqlDbType.Text),
												   new SqlParameter("@strFlow",SqlDbType.Text),
												   new SqlParameter("@strDesc",SqlDbType.Text),
												   new SqlParameter("@iStatus",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_INFO_UPDATE:
                                strProcParams = "SP_UPDATE_SERVERHANDLING";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@dtDate",SqlDbType.DateTime),
												   new SqlParameter("@strArea",SqlDbType.Text),
												   new SqlParameter("@strFlow",SqlDbType.Text),
												   new SqlParameter("@strDesc",SqlDbType.Text),
												   new SqlParameter("@iStatus",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_INFO_DELETE:
                                strProcParams = "SP_DELETE_SERVERHANDLING";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_INFO_LIST:
                                strProcParams = "SP_GET_SERVERHANDLING";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndexs",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@dBegin",SqlDbType.DateTime),
												   new SqlParameter("@dEnd",SqlDbType.DateTime),
												   new SqlParameter("@iPost",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 >= pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 维护内容
                            case TaskService.MAINTENANCE_HANDLING_CONTENT_PUT:
                                strProcParams = "SP_PUT_HANDLINGCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iHandling",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strProplem",SqlDbType.Text),
												   new SqlParameter("@strDesc",SqlDbType.Text),
												   new SqlParameter("@iStatus",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_CONTENT_UPDATE:
                                strProcParams = "SP_UPDATE_HANDLINGCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@iTemplate",SqlDbType.Int,4),
												   new SqlParameter("@strProplem",SqlDbType.Text),
												   new SqlParameter("@strDesc",SqlDbType.Text),
												   new SqlParameter("@iStatus",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 > pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("数据异常");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_CONTENT_DELETE:
                                strProcParams = "SP_DELETE_HANDLINGCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iPost",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.MAINTENANCE_HANDLING_CONTENT_LIST:
                                strProcParams = "SP_GET_HANDLINGCONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iHandling",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 获取检查条件
                            case TaskService.MAINTENANCE_CONDITION_LIST:
                                strProcParams = "SP_GET_CHECKCONDITION";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@group",SqlDbType.Int,4)                                                  
                                               };

                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 添加检查条件 ------------------------
                            case TaskService.MAINTENANCE_CONDITION_PUT:
                                strProcParams = "SP_PUT_CHECKCONDITION";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@strName",SqlDbType.VarChar,50),
												   new SqlParameter("@strTag",SqlDbType.Text),
                                                   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 更新检查条件 --------------------------
                            case TaskService.MAINTENANCE_CONDITION_UPDATE:
                                strProcParams = "SP_UPDATE_CHECKCONDITION";
                                strContent = "Condition_Kind = '%s', Condition_Sort = '%s', Condition_Server = '%s', Condition_Name = '%s', Condition_Tag = '%s', Condition_Desc = '%s'";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.VarChar,5000)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = strReplace("%s", strContent, mReciceContent);

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除检查条件 --------------------------
                            case TaskService.MAINTENANCE_CONDITION_DELETE:
                                strProcParams = "SP_DELETE_CHECKCONDITION";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 系统工具
                            #region 执行固定的SQL语句
                            case TaskService.TOOL_RUNSQL_SELECT:
                                string StrSql = "SELECT (SELECT Server_Internet FROM Game_Server WHERE Server_ID=" + mReciceContent[0, 0].Content.ToString() + ") AS Server_Internet,Access_User,Access_Pwd FROM Server_Access WHERE Access_Sort=14 AND Access_Server=" + mReciceContent[0, 0].Content.ToString();
                                pADOUtils.ExecuteQuery(StrSql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("请选择大区的数据库服务器进行操作！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                try
                                {
                                    string Str_User = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0]["Access_User"].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));

                                    string Str_Pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[0]["Access_Pwd"].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));

                                    string Str_Connect = "Connect Timeout=3000;Server=" + pDataSet.Tables[0].Rows[0]["Server_Internet"].ToString() + ",1433;Database=SDO;uid=" + Str_User + "; pwd=" + Str_Pwd + ";";
                                    MSSQLOperate pUnkown = new MSSQLOperate(Str_Connect);
                                    pUnkown.ExecuteTimeOut = int.Parse(mReciceContent[0, 1].Content.ToString());
                                    pUnkown.Connect(false);
                                    pUnkown.ExecuteQuery("EXEC SP_CLEAR_LOGINTABLE " + mReciceContent[0, 2].Content.ToString() + ",'" + mReciceContent[0, 3].Content.ToString() + "'");
                                    pUnkown.GetResult(RecordStyle.NONE);

                                    if (null != pUnkown.Message)
                                    {
                                        pSendContent.Add(pUnkown.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();

                                        break;
                                    }
                                    else
                                    {
                                        pSendContent.Add("执行结束");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    pSendContent.Add("无数据库，详细内容：" + ex.Message);
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 文件解密
                            case TaskService.TOOL_FILE_DECLASSIFIED:
                                byte[] data = (byte[])mReciceContent[0, 0].Content;

                                string Str_Sql = "SELECT Pwd,Key_Pwd1,Key_Pwd2 FROM Declassified_Key";
                                pADOUtils.ExecuteQuery(Str_Sql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;
                                DataRow Key_Row = pDataSet.Tables[0].Rows[0];

                                SymmetricAlgorithm sf = EncodeUtilise.CreateSymmetricAlgoritm(System.Global.EncryptType.RIJNDAEL, 256, 256);

                                byte[] rgbSalt = Encoding.ASCII.GetBytes(Key_Row["Pwd"].ToString().Length.ToString());
                                PasswordDeriveBytes bytes = new PasswordDeriveBytes(Key_Row["Pwd"].ToString(), rgbSalt);

                                byte[] ps1 = bytes.GetBytes(int.Parse(Key_Row["Key_Pwd1"].ToString()));
                                byte[] ps2 = bytes.GetBytes(int.Parse(Key_Row["Key_Pwd2"].ToString()));

                                byte[] ssdd = EncodeUtilise.DecryptData(sf, ps1, ps2, data);

                                string result_data = string.Empty;
                                switch (mReciceContent[0, 1].Content.ToString())
                                {
                                    case "0":
                                        result_data = Encoding.Unicode.GetString(ssdd, 0, ssdd.Length - 1);
                                        break;
                                    case "1":
                                        result_data = Encoding.UTF7.GetString(ssdd, 0, ssdd.Length - 1);
                                        break;
                                    case "2":
                                        result_data = Encoding.UTF8.GetString(ssdd, 0, ssdd.Length - 1);
                                        break;
                                    case "3":
                                        result_data = Encoding.UTF32.GetString(ssdd, 0, ssdd.Length - 1);
                                        break;
                                    default:
                                        result_data = Encoding.Default.GetString(ssdd, 0, ssdd.Length - 1);
                                        break;
                                }

                                pSendContent.Add(result_data);
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region 服务器状态信息
                            #region 硬件维护
                            #region 获取硬件维护信息 游戏，大区，服务器，开始时间，状态，维护人
                            case TaskService.SERVERSTATE_HWS_INFO_GET:
                                strProcParams = "SP_GET_HSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);
                                if (null == paramCache)
                                {
                                    paramCache = new SqlParameter[]
                                {
                                    new SqlParameter("@iGame",SqlDbType.Int,4),
                                    new SqlParameter("@iArea",SqlDbType.Int,4),
                                    new SqlParameter("@iServer",SqlDbType.Int,4),
                                    new SqlParameter("@startTime",SqlDbType.DateTime),
                                    new SqlParameter("@iStatus",SqlDbType.Int,4),
                                    new SqlParameter("@iMaker",SqlDbType.Int,4),
                                    new SqlParameter("@endTime",SqlDbType.DateTime)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 5].Content;
                                paramCache[4].Value = mReciceContent[0, 6].Content;
                                paramCache[5].Value = mReciceContent[0, 7].Content;
                                paramCache[6].Value = mReciceContent[0, 8].Content;


                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 添加硬件维护信息
                            case TaskService.SERVERSTATE_HWS_INFO_ADD:
                                strProcParams = "SP_PUT_HSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iServer",SqlDbType.Int,4),
												   new SqlParameter("@strDemand",SqlDbType.Text),
												   new SqlParameter("@strResult",SqlDbType.Text),
                                                   new SqlParameter("@strMakers",SqlDbType.Int,4),
                                                   new SqlParameter("@strAlliance",SqlDbType.Text),
                                                   new SqlParameter("@startTime",SqlDbType.DateTime),
                                                   new SqlParameter("@iArea",SqlDbType.Int,4),
                                                   new SqlParameter("@iGame",SqlDbType.Int,4)
                                };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 修改硬件信息状态
                            case TaskService.SERVERSTATE_HWS_INFO_UPDATE:
                                strProcParams = "SP_UPDATE_HSINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iID",SqlDbType.Int,4),
												   new SqlParameter("@strDemand",SqlDbType.Text),
												   new SqlParameter("@strResult",SqlDbType.Text),
                                                   new SqlParameter("@endTime",SqlDbType.DateTime)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 222移植
                            case TaskService.SERVERSTATE_ONLINENUM_GET:
                                strProcParams = "SP_GET_ONLINENUM";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strGame",SqlDbType.VarChar, 50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            case TaskService.SERVERSTATE_HISTORYNUM_GET:
                                strProcParams = "SP_GET_HISTORYNUM";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strGame",SqlDbType.VarChar, 50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            case TaskService.SERVERSTATE_OSERVER_GET:
                                strProcParams = "SP_GET_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            case TaskService.SERVERSTATE_OSERVER_ADD:
                                strProcParams = "SP_ADD_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@gamename",SqlDbType.VarChar,50),
												   new SqlParameter("@tablename",SqlDbType.VarChar,50),
												   new SqlParameter("@baktable",SqlDbType.VarChar,50),
												   new SqlParameter("@daynum",SqlDbType.VarChar,50),
												   new SqlParameter("@hisnum",SqlDbType.VarChar,50),
												   new SqlParameter("@numstable",SqlDbType.VarChar,50),
												   new SqlParameter("@type",SqlDbType.VarChar,50),
												   new SqlParameter("@dbname",SqlDbType.VarChar,50),
												   new SqlParameter("@username",SqlDbType.VarChar,50),
												   new SqlParameter("@password",SqlDbType.VarChar,50),
												   new SqlParameter("@strsql",SqlDbType.VarChar,5000),
												   new SqlParameter("@rscount",SqlDbType.VarChar,50),
												   new SqlParameter("@totb",SqlDbType.VarChar,50),
												   new SqlParameter("@ipcount",SqlDbType.VarChar,50),
												   new SqlParameter("@status",SqlDbType.VarChar,50),
												   new SqlParameter("@gamezonename",SqlDbType.VarChar,50),
												   new SqlParameter("@ipaddress",SqlDbType.VarChar,50),
												   new SqlParameter("@ipstatus",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;
                                paramCache[11].Value = mReciceContent[0, 11].Content;
                                paramCache[12].Value = mReciceContent[0, 12].Content;
                                paramCache[13].Value = mReciceContent[0, 13].Content;
                                paramCache[14].Value = mReciceContent[0, 14].Content;
                                paramCache[15].Value = mReciceContent[0, 15].Content;
                                paramCache[16].Value = mReciceContent[0, 16].Content;
                                paramCache[17].Value = mReciceContent[0, 17].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            case TaskService.SERVERSTATE_OSERVER_UPDATE:
                                strProcParams = "SP_UPDATE_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@gamenid",SqlDbType.Int,4),
												   new SqlParameter("@ipid",SqlDbType.Int,4),
												   new SqlParameter("@gamename",SqlDbType.VarChar,50),
												   new SqlParameter("@tablename",SqlDbType.VarChar,50),
												   new SqlParameter("@baktable",SqlDbType.VarChar,50),
												   new SqlParameter("@daynum",SqlDbType.VarChar,50),
												   new SqlParameter("@hisnum",SqlDbType.VarChar,50),
												   new SqlParameter("@numstable",SqlDbType.VarChar,50),
												   new SqlParameter("@type",SqlDbType.VarChar,50),
												   new SqlParameter("@dbname",SqlDbType.VarChar,50),
												   new SqlParameter("@username",SqlDbType.VarChar,50),
												   new SqlParameter("@password",SqlDbType.VarChar,50),
												   new SqlParameter("@strsql",SqlDbType.VarChar,5000),
												   new SqlParameter("@rscount",SqlDbType.VarChar,50),
												   new SqlParameter("@totb",SqlDbType.VarChar,50),
												   new SqlParameter("@ipcount",SqlDbType.VarChar,50),
												   new SqlParameter("@status",SqlDbType.VarChar,50),
												   new SqlParameter("@gamezonename",SqlDbType.VarChar,50),
												   new SqlParameter("@ipaddress",SqlDbType.VarChar,50),
												   new SqlParameter("@ipstatus",SqlDbType.VarChar,50)       
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;
                                paramCache[9].Value = mReciceContent[0, 9].Content;
                                paramCache[10].Value = mReciceContent[0, 10].Content;
                                paramCache[11].Value = mReciceContent[0, 11].Content;
                                paramCache[12].Value = mReciceContent[0, 12].Content;
                                paramCache[13].Value = mReciceContent[0, 13].Content;
                                paramCache[14].Value = mReciceContent[0, 14].Content;
                                paramCache[15].Value = mReciceContent[0, 15].Content;
                                paramCache[16].Value = mReciceContent[0, 16].Content;
                                paramCache[17].Value = mReciceContent[0, 17].Content;
                                paramCache[18].Value = mReciceContent[0, 18].Content;
                                paramCache[19].Value = mReciceContent[0, 19].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            case TaskService.SERVERSTATE_OSERVER_DELETE:
                                strProcParams = "SP_DELETE_GAMEINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@gameid",SqlDbType.Int,4),
												   new SqlParameter("@ipid",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pOnlinenum = new MSSQLOperate(this.strOnlineNumConn);
                                pOnlinenum.Connect(false);

                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();

                                pOnlinenum.DisConnected();
                                break;
                            #endregion
                            #endregion

                            #region Report
                            #region 查看警告
                            case TaskService.REPORT_SHOW_LIST:
                                strProcParams = "SP_GET_NOTIFY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@iLevel",SqlDbType.Int,4),
                                                   new SqlParameter("@iUser",SqlDbType.Int,4),
                                                   new SqlParameter("@Kind",SqlDbType.Int,4),
                                                   new SqlParameter("@Count",SqlDbType.Int,4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
                                               };

                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;
                                paramCache[2].Value = mReciceContent[0, 4].Content;
                                paramCache[3].Value = mReciceContent[0, 1].Content;
                                paramCache[4].Value = mReciceContent[0, 5].Content;

                                if (2 != int.Parse(mReciceContent[0, 4].Content.ToString()))
                                {
                                    pOnlinenum = new MSSQLOperate(this.strReportConn);
                                    pOnlinenum.Connect(false);
                                    pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                    pOnlinenum.GetResult(RecordStyle.DATASET);
                                    pDataSet = (DataSet)pOnlinenum.RecordData;

                                    if (0 == pOnlinenum.AffectRow)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.DATASET);
                                    pDataSet = (DataSet)pADOUtils.RecordData;

                                    if (0 == pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region 获取报告通知列表 Finished!
                            case TaskService.REPORT_NOTIFY_LIST:
                                strProcParams = "SP_VIEW_NOTIFY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strServer",SqlDbType.VarChar,5000),
												   new SqlParameter("@iUser",SqlDbType.Int,4)
											   };

                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取报告列表 Finished!
                            case TaskService.REPORT_REPORT_LIST:
                                strProcParams = "SP_VIEW_REPORT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndex",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strTable",SqlDbType.VarChar,50),
												   new SqlParameter("@dBegin",SqlDbType.SmallDateTime,10),
												   new SqlParameter("@dEnd",SqlDbType.VarChar,50),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strServer",SqlDbType.VarChar,5000),
												   new SqlParameter("@iUser",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;
                                paramCache[8].Value = mReciceContent[0, 8].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 > pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("数据量太大，请缩小查询范围！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取指定报告详细信息 Finished!
                            case TaskService.REPORT_DETAIL_GET:
                                strProcParams = "SP_VIEW_DETAIL";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@iSort",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取报告日志列表 Finished!
                            case TaskService.REPORT_LOG_LIST:
                                strProcParams = "SP_VIEW_LOG";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndex",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@strTable",SqlDbType.VarChar,50),
												   new SqlParameter("@dBegin",SqlDbType.SmallDateTime,10),
												   new SqlParameter("@dEnd",SqlDbType.SmallDateTime,10),
												   new SqlParameter("@strServer",SqlDbType.VarChar,5000),
												   new SqlParameter("@iUser",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 > pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("数据量太大，请缩小查询范围！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 清除报告及日志列表 Finished!
                            case TaskService.REPORT_CLEAR_LIST:
                                strProcParams = "SP_CLEAR_RECORD";

                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strTable",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                pSendContent.Add("SUCCEED");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 保存处理信息 Finished!
                            case TaskService.REPORT_NOTIFY_DISPOSAL:
                                strProcParams = "SP_DISPOSAL_REPORT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strMember",SqlDbType.VarChar,50),
												   new SqlParameter("@iReportKey",SqlDbType.Int,4),
												   new SqlParameter("@iBackupKey",SqlDbType.Int,4),
												   new SqlParameter("@strReportTable",SqlDbType.VarChar,50),
												   new SqlParameter("@strBackupTable",SqlDbType.VarChar,50),
												   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (0 < pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获得处理内容 Finished!
                            case TaskService.REPORT_DISPOSAL_DETAIL:
                                strProcParams = "SP_DISPOSAL_DETAIL";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strTable",SqlDbType.VarChar,50)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获得检查结果 Finished!
                            case TaskService.REPORT_CHECK_RESULT:
                                strProcParams = "SP_VIEW_CHECK";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@PageIndex",SqlDbType.Int,4),
												   new SqlParameter("@PageSize",SqlDbType.Int,4),
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@dBegin",SqlDbType.SmallDateTime,20),
												   new SqlParameter("@dEnd",SqlDbType.SmallDateTime,20),
												   new SqlParameter("@iGame",SqlDbType.Int,4),
												   new SqlParameter("@strServer",SqlDbType.VarChar,5000),
												   new SqlParameter("@iUser",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;
                                paramCache[5].Value = mReciceContent[0, 5].Content;
                                paramCache[6].Value = mReciceContent[0, 6].Content;
                                paramCache[7].Value = mReciceContent[0, 7].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取各类消息个数 Finished! ---
                            case TaskService.REPORT_COUNT_LIST:
                                int iCountAffectRow = 0;
                                strProcParams = "SP_NOTIFY_COUNT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iUser",SqlDbType.Int,4),
												   new SqlParameter("@strServer",SqlDbType.VarChar,7700)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 3].Content;
                                paramCache[1].Value = mReciceContent[0, 4].Content;

                                if (mReciceContent[0, 2].Content.ToString() == "0")
                                {
                                    pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                    pADOUtils.GetResult(RecordStyle.DATASET);
                                    pDataSet = (DataSet)pADOUtils.RecordData;

                                    iCountAffectRow = pADOUtils.AffectRow;

                                    if (0 >= iCountAffectRow)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    pOnlinenum = new MSSQLOperate(this.strReportConn);

                                    if (pOnlinenum.Connect(false))
                                    {
                                        pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                        pOnlinenum.GetResult(RecordStyle.DATASET);
                                        pDataSet = (DataSet)pOnlinenum.RecordData;

                                        iCountAffectRow = pOnlinenum.AffectRow;

                                        if (0 >= iCountAffectRow)
                                        {
                                            pSendContent.Add("暂无数据");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                        }

                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                    else
                                    {
                                        pSendContent.Add("数据库连接失败！");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }

                                break;
                            #endregion
                            #region 获取批处理报告类别 Finished!
                            case TaskService.REPORT_DISPOSAL_CATEGORY:
                                strProcParams = "SP_GET_CATEGORY";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 查看处理的详细信息 Finished!
                            case TaskService.REPORT_BATCH_DISPOSAL:
                                strProcParams = "SP_DISPOSAL_BATCH";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iServer",SqlDbType.Int,4),								                				   
												   new SqlParameter("@iSort",SqlDbType.Int,4),
												   new SqlParameter("@iKind",SqlDbType.Int,4),
												   new SqlParameter("@strCategory",SqlDbType.VarChar,50),
												   new SqlParameter("@strValue",SqlDbType.VarChar,500)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;
                                paramCache[4].Value = mReciceContent[0, 4].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);


                                pSendContent.Add("SUCCEED");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region DEBUG提交
                            case TaskService.DEBUG_INFO_PUT:
                                strProcParams = "SP_ADD_DEBUGINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@strTitle",SqlDbType.VarChar,50),
												   new SqlParameter("@strModule",SqlDbType.VarChar,50),
												   new SqlParameter("@strContent",SqlDbType.Text,50),
												   new SqlParameter("@strPost",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.DEBUG_INFO_LIST:
                                strProcParams = "SP_GET_DEBUGINFO";

                                pADOUtils.ExecuteQuery(strProcParams);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.DEBUG_INFO_UPDATE:
                                strProcParams = "SP_UPDATE_DEBUGINFO";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4),
												   new SqlParameter("@strContent",SqlDbType.Text)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (1 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion

                            #region 即时消息部分...
                            case TaskService.INSTANT_RECIVE_ADD:
                                string[] strCommand = mReciceContent[0, 0].Content.ToString().Split(",".ToCharArray());

                                for (int i = 0; i < strCommand.Length; i++)
                                {
                                    if (strCommand[i].Length > 0)
                                    {
                                        strProcParams = "SP_PUT_INSTANTMESSAGE";
                                        paramCache = DataUtilities.GetParameters(strProcParams);

                                        if (paramCache == null)
                                        {
                                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iCommand",SqlDbType.Int,4),
												   new SqlParameter("@iRevice",SqlDbType.Int,4),
												   new SqlParameter("@strDesc",SqlDbType.Text)
											   };
                                            DataUtilities.SetParameters(strProcParams, paramCache);
                                        }

                                        paramCache[0].Value = int.Parse(strCommand[i]);
                                        paramCache[1].Value = mReciceContent[0, 1].Content;
                                        paramCache[2].Value = mReciceContent[0, 2].Content;

                                        pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                        pADOUtils.GetResult(RecordStyle.NONE);
                                    }
                                }

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_RECIVE_UPDATE:
                                strProcParams = "SP_UPDATE_INSTANTMESSAGE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
                                                   new SqlParameter("@iKey",SqlDbType.Int,4),
                                                   new SqlParameter("@iCommand",SqlDbType.Int,4),
                                                   new SqlParameter("@iRevice",SqlDbType.Int,4),
                                                   new SqlParameter("@strDesc",SqlDbType.Text)
                                               };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;
                                paramCache[2].Value = mReciceContent[0, 2].Content;
                                paramCache[3].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_RECIVE_DELETE:
                                strProcParams = "SP_DELETE_INSTANTMESSAGE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_RECIVE_GET:
                                strProcParams = "SP_GET_INSTANTMESSAGE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iCommand",SqlDbType.Int,4),
												   new SqlParameter("@iRevice",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_CONTENT_ADD:
                                pSendContent.Add("该命令不允许使用！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_CONTENT_UPDATE:
                                strProcParams = "SP_UPDATE_MESSAGECONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pADOUtils.ExecuteQuery(true, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.NONE);

                                if (pADOUtils.ProcReturn.ToString() == "0")
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_CONTENT_DELETE:
                                pSendContent.Add("该命令不允许使用！");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            case TaskService.INSTANT_CONTENT_GET:
                                strProcParams = "SP_GET_MESSAGECONTENT";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@iRevice",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 即时消息过滤
                            #region 添加消息过滤
                            case TaskService.INSTANT_LEACH_PUT:
                                if (mReciceContent[0, 0].Content == mReciceContent[0, 1].Content)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    break;
                                }

                                strProcParams = "SP_PUT_LEACHMESSAGE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Game_ID",SqlDbType.Int,4),
												   new SqlParameter("@Log",SqlDbType.VarChar,200)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;
                                paramCache[1].Value = mReciceContent[0, 1].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 获取消息过滤
                            case TaskService.INSTANT_LEACH_GET:
                                strProcParams = "SP_GET_LEACHMESSAGE";
                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(strProcParams);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 删除消息过滤
                            case TaskService.INSTANT_LEACH_DELETE:
                                strProcParams = "SP_DELETE_LEACHMESSAGE";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Leach_ID",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 0].Content;

                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(false, strProcParams, paramCache);
                                pOnlinenum.GetResult(RecordStyle.NONE);

                                if (1 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("SUCCEED");
                                    pSendContent.AddRows();
                                }
                                else
                                {
                                    pSendContent.Add("FAILURE");
                                    pSendContent.AddRows();
                                }

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion

                            #region SQL PLUS
                            #region 获取游戏、大区、服务器、权限等列表
                            #region 游戏
                            case TaskService.TOOL_SQLPLUS_SORT_GAME_GET:
                                strProcParams = "SP_DBSERVER_GAME_LIST";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Verify_id",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 大区
                            case TaskService.TOOL_SQLPLUS_SORT_AREA_GET:
                                strProcParams = "SP_DBSERVER_AREA_LIST";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Verify_id",SqlDbType.Int,4),
                                                   new SqlParameter("@Game_id",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 服务器
                            case TaskService.TOOL_SQLPLUS_SORT_SERVER_GET:
                                strProcParams = "SP_DBSERVER_SERVER_LIST";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Verify_id",SqlDbType.Int,4),
                                                   new SqlParameter("@Area_id",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region 权限
                            case TaskService.TOOL_SQLPLUS_SORT_GET:
                                strProcParams = "SP_DBSERVER_LIST";
                                paramCache = DataUtilities.GetParameters(strProcParams);

                                if (paramCache == null)
                                {
                                    paramCache = new SqlParameter[]{
												   new SqlParameter("@Verify_id",SqlDbType.Int,4),
                                                   new SqlParameter("@Server_id",SqlDbType.Int,4)
											   };
                                    DataUtilities.SetParameters(strProcParams, paramCache);
                                }

                                paramCache[0].Value = mReciceContent[0, 2].Content;
                                paramCache[1].Value = mReciceContent[0, 3].Content;

                                pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("暂无数据");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #endregion
                            #region 获取数据库列表
                            case TaskService.TOOL_SQLPLUS_DATABASE_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                
                                switch (dt_Data)
                                {
                                    case DataBase.ODBC:
                                        break;
                                    case DataBase.OLEDB:
                                        break;
                                    case DataBase.MSSQL:
                                        sqlplus_database = "master";
                                        break;
                                    case DataBase.MYSQL:
                                        sqlplus_database = "test";
                                        break;
                                    case DataBase.ORACLE:
                                        break;
                                    case DataBase.DB2:
                                    case DataBase.FILE:
                                        break;
                                }
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 4].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase();
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }
                                    DataTable dt = (DataTable)ps.DataCenter.RecordData;

                                    if (0 == dt.Rows.Count)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(dt, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 获取数据库结构－表、视图、触发器、存储过程等列表
                            case TaskService.TOOL_SQLPLUS_DATABASE_STRUCTURE_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Structure(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }
                                    //ps.Message;

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }                                    
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion

                            #region 表
                            #region
                            case TaskService.TOOL_SQLPLUS_TABLE_LIST_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Table_List(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }       
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.TOOL_SQLPLUS_TABLE_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Table(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region 视图
                            #region
                            case TaskService.TOOL_SQLPLUS_VIEW_LIST_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_View_List(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.TOOL_SQLPLUS_VIEW_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_View(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region 存储过程
                            #region
                            case TaskService.TOOL_SQLPLUS_PORC_LIST_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Proc_List(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.TOOL_SQLPLUS_PORC_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Proc(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region 函数
                            #region
                            case TaskService.TOOL_SQLPLUS_FUNCATION_LIST_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Function_List(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region
                            case TaskService.TOOL_SQLPLUS_FUNCATION_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetDatabase_Function(mReciceContent);
                                    if ("" != ps.Message)
                                    {
                                        throw new Exception(ps.Message);
                                    }

                                    switch (dt_Data)
                                    {
                                        case DataBase.MSSQL:
                                            pDataSet = (DataSet)ps.DataCenter.RecordData;

                                            if (0 == pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                        case DataBase.MYSQL:
                                            CustomDataCollection Receive_Custom = (CustomDataCollection)ps.DataCenter.RecordData;

                                            if (0 == Receive_Custom.RowCount)
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();

                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                            else
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, Receive_Custom);
                                            }
                                            pSendBuffer = pSourcePacket.CoalitionInfo();
                                            break;
                                    }
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion

                            #region 获取数据库数据类型
                            case TaskService.TOOL_SQLPLUS_DATATYPE_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetData_Type();
                                    pDataSet = (DataSet)ps.DataCenter.RecordData;

                                    if (0 == pDataSet.Tables[0].Rows.Count)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 获取二进制排序规则
                            case TaskService.TOOL_SQLPLUS_COLLATION_GET:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    ps.GetCollation();
                                    pDataSet = (DataSet)ps.DataCenter.RecordData;

                                    if (0 == pDataSet.Tables[0].Rows.Count)
                                    {
                                        pSendContent.Add("暂无数据");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion

                            #region 修改表结构
                            case TaskService.TOOL_SQLPLUS_TABLE_ALTER:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 0].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 1].Content.ToString();
                                sqlplus_database = mReciceContent[0, 2].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 3].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 4].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    string Update_Sql = string.Empty;

                                    for (int i = 0; i < mReciceContent.GetLength(0); i++)
                                    {
                                        AlterTableStyle at_Style = (AlterTableStyle)int.Parse(mReciceContent[i, 7].Content.ToString());
                                        switch (at_Style)
                                        {
                                            case AlterTableStyle.ADDCOLUMN://添加字段
                                                Update_Sql += ps.Alter_Table_AddColumn(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.DROPOBJECT://删除操作
                                                Update_Sql += ps.Alter_Table_Drop(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ALTERCOLUMN://修改字段
                                                Update_Sql += ps.Alter_Table_Alter(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDPRIMARYKEY://在现有列中添加主键
                                                Update_Sql += ps.Alter_Table_AddPrimaryKey(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDFOREIGNKEY://在现有列中添加外键
                                                Update_Sql += ps.Alter_Table_AddForeignKey(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDDEFAULT://在现有列中添加默认值
                                                Update_Sql += ps.Alter_Table_AddDefault(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDCHECK://在现有列中添加Check约束
                                                Update_Sql += ps.Alter_Table_AddCheck(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDUNIQUE://在现有列中添加唯一性约束
                                                Update_Sql += ps.Alter_Table_AddUnique(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDEXTEND://添加扩展属性
                                                Update_Sql += ps.Alter_Table_AddExtend(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.MODEXTEND://修改扩展属性
                                                Update_Sql += ps.Alter_Table_ModExtend(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.DELEXTEND://删除扩展属性
                                                Update_Sql += ps.Alter_Table_DelExtend(mReciceContent, i) + ";";
                                                break;
                                            default:
                                                Update_Sql += "";
                                                break;
                                            //throw new Exception("未知命令！");
                                        }
                                    }
                                    try
                                    {
                                        ps.OnQuery(Update_Sql);
                                        ps.DataCenter.GetResult(RecordStyle.NONE);
                                        if (string.Empty != ps.Message)
                                        {
                                            pSendContent.Add(ps.Message);
                                            pSendContent.AddRows();
                                        }
                                        else
                                        {
                                            pSendContent.Add("SUCCEED");
                                            pSendContent.AddRows();
                                        }
                                    }
                                    catch
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 执行语句，无返回结果集
                            case TaskService.TOOL_SQLPLUS_EXECUTE:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 0].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 1].Content.ToString();
                                sqlplus_database = mReciceContent[0, 2].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 3].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 4].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);
                                
                                ps = new DataService(dt_Data,sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    try
                                    {
                                        ps.OnQuery(mReciceContent[0,5].Content.ToString());
                                        ps.DataCenter.GetResult(RecordStyle.NONE);

                                        if (string.Empty != ps.Message)
                                        {
                                            pSendContent.Add("错误:" + ps.Message);
                                            pSendContent.AddRows();
                                        }
                                        else
                                        {
                                            pSendContent.Add("SUCCEED");
                                            pSendContent.AddRows();
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        pSendContent.Add("异常:" + ex.Message);
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 重命名
                            case TaskService.TOOL_SQLPLUS_RENAME:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 0].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 1].Content.ToString();
                                sqlplus_database = mReciceContent[0, 2].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 3].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 4].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    string Rename_sql = ps.Rename(mReciceContent);
                                    try
                                    {
                                        ps.OnQuery(Rename_sql);
                                        ps.DataCenter.GetResult(RecordStyle.NONE);

                                        if (string.Empty != ps.Message)
                                        {
                                            pSendContent.Add("错误:" + ps.Message);
                                            pSendContent.AddRows();
                                        }
                                        else
                                        {
                                            pSendContent.Add("SUCCEED");
                                            pSendContent.AddRows();
                                        }
                                    }
                                    catch
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 查询器
                            case TaskService.TOOL_SQLPLUS_TRANSACT:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 2].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 3].Content.ToString();
                                sqlplus_database = mReciceContent[0, 4].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 5].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 6].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn, _pSocketMessage.Connection,mTaskService);
                                if (ps.OnConnect(false))
                                {
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;

                                    if (DataSqlConn.Check_Sql(mReciceContent[0, 7].Content.ToString()))
                                    {
                                        ps.OnQuery(mReciceContent[0, 7].Content.ToString());
                                        ps.DataCenter.GetResult(RecordStyle.DATASET);

                                        pDataSet = (DataSet)ps.DataCenter.RecordData;

                                        if (string.Empty != ps.Message)//语句不符合约束条件
                                        {
                                            pSendContent.Add("错误:" + ps.Message);
                                            pSendContent.AddRows();
                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            if (0 < pDataSet.Tables[0].Rows.Count)
                                            {
                                                pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, int.Parse(mReciceContent[0, 8].Content.ToString()), int.Parse(mReciceContent[0, 9].Content.ToString())));
                                            }
                                            else
                                            {
                                                pSendContent.Add("暂无数据");
                                                pSendContent.AddRows();
                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                        }
                                    }
                                    else//sql语句不合法
                                    {
                                        pSendContent.Add("sql语句不符合约束条件!");
                                        pSendContent.AddRows();
                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region 删除命令
                            case TaskService.TOOL_SQLPLUS_DROP:
                                dt_Data = (DataBase)int.Parse(mReciceContent[0, 0].Content.ToString());
                                sqlplus_ip = mReciceContent[0, 1].Content.ToString();
                                sqlplus_database = mReciceContent[0, 2].Content.ToString();
                                sqlplus_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 3].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(mReciceContent[0, 4].Content.ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                                sqlplus_Conn = DataSqlConn.strConn(dt_Data, sqlplus_ip, sqlplus_database, sqlplus_user, sqlplus_pwd);

                                ps = new DataService(dt_Data, sqlplus_Conn);
                                if (ps.OnConnect(false))
                                {
                                    string Delete_Sql = ps.Drop_Object(mReciceContent);
                                    ps.DataCenter.ExecuteTimeOut = iTimeOut;
                                    try
                                    {
                                        ps.DataCenter.ExecuteQuery(Delete_Sql);
                                        ps.DataCenter.GetResult(RecordStyle.NONE);

                                        if (string.Empty != ps.Message)
                                        {
                                            pSendContent.Add("错误:" + ps.Message);
                                            pSendContent.AddRows();
                                        }
                                        else
                                        {
                                            pSendContent.Add("SUCCEED");
                                            pSendContent.AddRows();
                                        }
                                    }
                                    catch
                                    {
                                        pSendContent.Add("FAILURE");
                                        pSendContent.AddRows();
                                    }
                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region 未连接数据库
                                    pSendContent.Add("数据库连接失败！");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion

                            #region 消息模板/文件上传
                            case TaskService.SYSTEM_UPDATE_PUT_GET:
                            default:
                                throw new Exception("未知命令！");
                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                    #region 异常处理
                    catch (Exception ex)
                    {
                        try
                        {
                            Console.WriteLine(ex.Message + "-" + ex.Source + "-" + ex.InnerException.Message);
                        }
                        catch
                        {
                            Console.WriteLine(ex.Message);
                        }

                        this.strError = ex.Message;

                        strProcParams = "SP_UPDATE_OPERATIONSTATUS";
                        paramCache = DataUtilities.GetParameters(strProcParams);

                        if (paramCache == null)
                        {
                            paramCache = new SqlParameter[]{
												   new SqlParameter("@iKey",SqlDbType.Int)
											   };
                            DataUtilities.SetParameters(strProcParams, paramCache);
                        }

                        paramCache[0].Value = this.iLogID;

                        pADOUtils.ExecuteQuery(false, strProcParams, paramCache);
                        pADOUtils.GetResult(RecordStyle.NONE);
                        this.iLogID = 0;

                        pSendContent.Add(ex.Message);
                        pSendContent.AddRows();

                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                    }
                    #endregion
                }

                pADOUtils.DisConnected();

                this._pSocketMessage.Connection.OnSend(pSendBuffer);
            }            

            //Console.WriteLine("---------" + mTaskService.ToString());

            return bResult;
        }

        #region 属性信息
        public string Error
        {
            get
            {
                return this.strError;
            }
        }

        public TaskService SocketMessage
        {
            get
            {
                return this.eSocketMessage;
            }
        }
        #endregion

        #region 私有字段
        private MessageEventArgs _pSocketMessage;
        private string strMessage;
        private string strError;
        private string strAnalyseConn;
        private string strOnlineNumConn;
        private string strReportConn;
        private int iLogID;
        private NotesUtils pNotesUtils;
        private TaskService eSocketMessage;
        private NotesSessionClass pNotesSession = new NotesSessionClass();

        private string strProcParams = null;
        private DbParameter[] paramCache;
        private string strContent = null;

        private int int_groupid;//用户所在组id
        private int int_verifyid;//用户帐号id
        private CommonOrder pCommonOrder;

        private MSSQLOperate pOnlinenum = null;

        private MysqlUtils pMysqlUtils = null;
        private string str_auuser = null;
        private string str_aupwd = null;
        private string strMysqlConn;
        private DataSet auDataset;
        #endregion

        #region 私有方法
        private CustomDataCollection buildTLV(DataSet tRecord, bool bLog)
        {
            CustomDataCollection mReturnBody = new CustomDataCollection(StructType.CUSTOMDATA);

            List<OrderInfo> arrOrderUser = pCommonOrder.GetOrder();

            if (tRecord.Tables.Count > 1)
            {
                foreach (DataTable pDataTable in tRecord.Tables)
                {
                    if (bLog)
                    {
                        uint usFirstTag = 0x08000001;

                        for (int i = 0; i < pDataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < pDataTable.Columns.Count; j++)
                            {
                                mReturnBody.Add((DataField)usFirstTag, DefineUtilities.ToDataFormat(pDataTable.Columns[j].DataType.Name), pDataTable.Rows[i][j]);
                            }
                            mReturnBody.AddRows();
                            usFirstTag++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < pDataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < pDataTable.Columns.Count; j++)
                            {
                                object obj_check_field = DefineUtilities.ToDataField(pDataTable.Columns[j].ColumnName);
                                object obj_check_key = pDataTable.Rows[i][j];
                                CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                                if (pCheck_Order.Check_Result())
                                {
                                    mReturnBody.Add(DefineUtilities.ToDataField(pDataTable.Columns[j].ColumnName), DefineUtilities.ToDataFormat(pDataTable.Columns[j].DataType.Name), pDataTable.Rows[i][j]);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            mReturnBody.AddRows();
                        }
                    }
                }
            }
            else
            {
                if (bLog)
                {
                    uint usFirstTag = 0x08000001;

                    for (int i = 0; i < tRecord.Tables[0].Rows.Count; i++)
                    {
                        for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
                        {
                            mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName), DefineUtilities.ToDataFormat(tRecord.Tables[0].Columns[j].DataType.Name), tRecord.Tables[0].Rows[i][j]);
                        }
                        mReturnBody.AddRows();
                        usFirstTag++;
                    }                    
                }
                else
                {
                    for (int i = 0; i < tRecord.Tables[0].Rows.Count; i++)
                    {
                        for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
                        {
                            object obj_check_field = DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName);
                            object obj_check_key = tRecord.Tables[0].Rows[i][j];
                            CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                            if (pCheck_Order.Check_Result())
                            {
                                mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName), DefineUtilities.ToDataFormat(tRecord.Tables[0].Columns[j].DataType.Name), tRecord.Tables[0].Rows[i][j]);
                            }
                            else
                            {
                                break;
                            }
                        }
                        mReturnBody.AddRows();
                    }
                }
            }

            return mReturnBody;
        }

        private CustomDataCollection buildTLV(DataSet tRecord)
        {
            CustomDataCollection mReturnBody = new CustomDataCollection(StructType.CUSTOMDATA);

            List<OrderInfo> arrOrderUser = pCommonOrder.GetOrder();

            for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
            {
                object obj_check_field = DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName);
                object obj_check_key = tRecord.Tables[0].Columns[j];
                CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                if (pCheck_Order.Check_Result())
                {
                    mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ToString()), DefineUtilities.ToDataFormat("String"), tRecord.Tables[0].Columns[j].ToString());
                }
                else
                {
                    break;
                }
            }
            mReturnBody.AddRows();

            for (int i = 0; i < tRecord.Tables[0].Rows.Count; i++)
            {
                for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
                {
                    object obj_check_field = DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName);
                    object obj_check_key = tRecord.Tables[0].Rows[i][j];
                    CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                    if (pCheck_Order.Check_Result())
                    {
                        mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName), DefineUtilities.ToDataFormat(tRecord.Tables[0].Columns[j].DataType.Name), tRecord.Tables[0].Rows[i][j]);
                    }
                    else
                    {
                        break;
                    }
                }
                mReturnBody.AddRows();
            }

            return mReturnBody;
        }

        private CustomDataCollection buildTLV(DataSet tRecord, int iCurrentPage, int iPageSize)
        {
            CustomDataCollection mReturnBody = new CustomDataCollection(StructType.CUSTOMDATA);
            List<OrderInfo> arrOrderUser = pCommonOrder.GetOrder();

            int iCurentCount = 0;
            //计算页数
            int pageCount = tRecord.Tables[0].Rows.Count % iPageSize;

            if (pageCount > 0)
            {
                pageCount = tRecord.Tables[0].Rows.Count / iPageSize + 1;
            }
            else
            {
                pageCount = tRecord.Tables[0].Rows.Count / iPageSize;
            }

            //确定当前记录数
            if ((iCurrentPage - 1) * iPageSize + iPageSize > tRecord.Tables[0].Rows.Count)
            {
                iCurentCount = tRecord.Tables[0].Rows.Count - (iCurrentPage - 1) * iPageSize;
            }
            else
            {
                iCurentCount = iPageSize;
            }

            #region 列名
            for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
            {
                object obj_check_field = DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName);
                object obj_check_key = tRecord.Tables[0].Columns[j];
                CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                if (pCheck_Order.Check_Result())
                {
                    mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ToString()), DefineUtilities.ToDataFormat("String"), tRecord.Tables[0].Columns[j].ToString());
                }
                else
                {
                    break;
                }
            }
            mReturnBody.Add(DataField.Page_Count, DataFormat.SIGNED, pageCount);
            mReturnBody.AddRows();
            #endregion

            for (int i = (iCurrentPage - 1) * iPageSize; i < (iCurrentPage - 1) * iPageSize + iCurentCount; i++)
            {
                if (i > tRecord.Tables[0].Rows.Count - 1)
                {
                    break;
                }

                uint usFirstTag = 0x08000001;

                for (int j = 0; j < tRecord.Tables[0].Columns.Count; j++)
                {
                    object obj_check_field = DefineUtilities.ToDataField(tRecord.Tables[0].Columns[j].ColumnName);
                    object obj_check_key = tRecord.Tables[0].Rows[i][j];
                    CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                    if (pCheck_Order.Check_Result())
                    {
                        mReturnBody.Add((DataField)usFirstTag, DefineUtilities.ToDataFormat(tRecord.Tables[0].Columns[j].DataType.Name), tRecord.Tables[0].Rows[i][j]);
                    }
                    else
                    {
                        break;
                    }

                    usFirstTag++;
                }

                mReturnBody.Add(DataField.Page_Count, DataFormat.SIGNED, pageCount);
                mReturnBody.AddRows();
            }

            return mReturnBody;
        }

        private CustomDataCollection buildTLV(DataTable tRecord, bool bLog)
        {
            CustomDataCollection mReturnBody = new CustomDataCollection(StructType.CUSTOMDATA);

            List<OrderInfo> arrOrderUser = pCommonOrder.GetOrder();

            if (bLog)
            {
                uint usFirstTag = 0x08000001;

                for (int i = 0; i < tRecord.Rows.Count; i++)
                {
                    for (int j = 0; j < tRecord.Columns.Count; j++)
                    {
                        mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Columns[j].ColumnName), DefineUtilities.ToDataFormat(tRecord.Columns[j].DataType.Name), tRecord.Rows[i][j]);
                    }
                    mReturnBody.AddRows();
                    usFirstTag++;
                }
            }
            else
            {
                for (int i = 0; i < tRecord.Rows.Count; i++)
                {
                    for (int j = 0; j < tRecord.Columns.Count; j++)
                    {
                        object obj_check_field = DefineUtilities.ToDataField(tRecord.Columns[j].ColumnName);
                        object obj_check_key = tRecord.Rows[i][j];
                        CheckOrder pCheck_Order = new CheckOrder(arrOrderUser, obj_check_key, obj_check_field);

                        if (pCheck_Order.Check_Result())
                        {
                            mReturnBody.Add(DefineUtilities.ToDataField(tRecord.Columns[j].ColumnName), DefineUtilities.ToDataFormat(tRecord.Columns[j].DataType.Name), tRecord.Rows[i][j]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    mReturnBody.AddRows();
                }
            }


            return mReturnBody;
        }

        /// <summary>
        /// 替换字符串，该方法需要重新定义
        /// </summary>
        /// <param name="strSplit"></param>
        /// <param name="strOld"></param>
        /// <param name="arrContent"></param>
        /// <returns></returns>
        private string strReplace(string strSplit, string strOld, CustomData[,] arrContent)
        {
            int iIndex = 1;
            string strReturn = strOld;

            while (true)
            {
                int iCount = strReturn.IndexOf(strSplit);

                if (iCount == -1)
                    break;

                strReturn = strReturn.Remove(iCount, strSplit.Length);
                strReturn = strReturn.Insert(iCount, arrContent[0, iIndex].Content.ToString());

                iIndex++;
            }

            return strReturn;
        }

        #endregion
    }     
}
