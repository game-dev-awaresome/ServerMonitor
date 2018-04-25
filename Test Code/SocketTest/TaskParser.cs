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
        #region ���캯��
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
            #region ��������
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
            DataBase dt_Data;//���ݿ�����
            string sqlplus_Conn = null;//���ݿ������ַ���
            int iTimeOut = 60;//��ʱʱ��
            DataService ps = null;
            string sqlplus_ip = null;//���ݿ�ip
            string sqlplus_database = null;//���ݿ���
            string sqlplus_user = null;//���ݿ��û���
            string sqlplus_pwd = null;//���ݿ����

            //���ݿ�����
            MSSQLOperate pADOUtils = new MSSQLOperate(this.strAnalyseConn);            
            #endregion

            #region ������Ϣ -- ���յ�����
            Console.WriteLine("      Message ID       : {0}", mSocketPacket.PacketID);
            Console.WriteLine("      Message Command  : {0}", mTaskService);
            Console.WriteLine("      Message Date     : {0}", System.DateTime.Now.ToString()/*mSocketDate.pPacket.m_Head.m_dtMsgDateTime.ToString()*/);
            Console.WriteLine("      Client Session     : {0}", this._pSocketMessage.Connection.Session);
            #endregion

            #region ����Ȩ���ж�
            int mRecice_length = mReciceContent.GetLength(1);//�����Ĳ�����Ŀ

            if (mRecice_length != 1 && mTaskService != TaskService.COMMON_CONNECT && mTaskService != TaskService.COMMON_VERIFY && mTaskService != TaskService.SYSTEM_UPDATE_CHECK_GET && mTaskService != TaskService.SYSTEM_UPDATE_DOWN_GET && mTaskService != TaskService.SYSTEM_UPDATE_PUT_GET)
            {
                this.int_groupid = int.Parse(mReciceContent[0, mRecice_length - 2].Content.ToString());
                this.int_verifyid = int.Parse(mReciceContent[0, mRecice_length - 1].Content.ToString());
            }

            if (mTaskService != TaskService.COMMON_CONNECT || mTaskService != TaskService.COMMON_VERIFY || mTaskService != TaskService.COMMON_DISCONNECT)
            {
                GetOrder_Num Order_Num = new GetOrder_Num(strAnalyseConn, int_groupid, int_verifyid, strMessage);

                int Banned_Order = Order_Num.Banned_Order();//�û����������¼��
                int Order_Count = Order_Num.Order_Count();//�û�����Ȩ�޼�¼��
                int Banned_Template = Order_Num.Banned_Template();//�û�������������
                int Banned_Order_Group = Order_Num.Banned_Order_Group();//�û������������
                int Banned_Template_Group = Order_Num.Banned_Template_Group();//�û�������������¼��

                #region �ж�����
                if (0 != Banned_Order)//�û������ֹ���������
                {
                    #region
                    pSendContent.Add("�������ֹʹ��");
                    pSendContent.AddRows();

                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                    pSendBuffer = pSourcePacket.CoalitionInfo();
                    this._pSocketMessage.Connection.OnSend(pSendBuffer);

                    bResult = true;
                    return bResult;
                    #endregion
                }
                else//δ�ҳ��û�������ü�¼���鿴�Ƿ��������û�����Ȩ��
                {
                    if (0 == Order_Count)//δ�����û�����Ȩ�ޣ��鿴�û��������Ȩ��
                    {
                        if (0 != Banned_Template)//�û��������鱻���ã��������
                        {
                            #region
                            pSendContent.Add("�������ֹʹ��");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            this._pSocketMessage.Connection.OnSend(pSendBuffer);

                            bResult = true;
                            return bResult;
                            #endregion
                        }
                        else//δ�ҳ��û���������ü�¼
                        {
                            if (0 != Banned_Order_Group)//�û����������ã��������
                            {
                                #region
                                pSendContent.Add("�������ֹʹ��");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                this._pSocketMessage.Connection.OnSend(pSendBuffer);

                                bResult = true;
                                return bResult;
                                #endregion
                            }
                            else//δ�ҳ��û���������ü�¼
                            {
                                if (0 != Banned_Template_Group)//�û���������鱻���ã��������
                                {
                                    #region
                                    pSendContent.Add("�������ֹʹ��");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    this._pSocketMessage.Connection.OnSend(pSendBuffer);

                                    bResult = true;
                                    return bResult;
                                    #endregion
                                }
                                else//�û����������δ�����ã��������
                                {
                                }
                            }
                        }
                    }
                    else//�������û������Ȩ�ޣ��������
                    {
                    }
                }
                #endregion
            }

            pCommonOrder = new CommonOrder(int_groupid, int_verifyid, mTaskService, strAnalyseConn);//��ȡ�������Ȩ��

            #endregion

            if (mTaskService.ToString().IndexOf("SQLPLUS_AU", 5) > -1)
            {
                #region ������Ϣ
                switch (mTaskService)
                {
                    #region ��ȡ�û���Ȩ�޵Ĵ���
                    case TaskService.TOOL_SQLPLUS_AU_SERVER_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetServer;//.Replace("$verify", mReciceContent[0, 2].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 >= pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("��������");
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
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);                            
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion
                    #region ��ȡ���ݿ�����û������б�
                    case TaskService.TOOL_SQLPLUS_AU_ACCESS_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetPopedom.Replace("$access_server", mReciceContent[0, 2].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 >= pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("��������");
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
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion

                    #region ��ȡ��ҵȼ���Exp
                    case TaskService.TOOL_SQLPLUS_AU_EXP_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δ����");
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
                                    pSendContent.Add("δ���ҵ����û���Ϣ");
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
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        break;
                    #endregion
                    #region �޸���ҵȼ���Exp
                    case TaskService.TOOL_SQLPLUS_AU_LEVELEXP_UPDATE:
                        if (pADOUtils.Connect(false))
                        {
                            #region AU��־-��ע��
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
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion.

                    #region ����Ա���Ϣ��ȡ
                    case TaskService.TOOL_SQLPLUS_AU_GENDER_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δ����");
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
                                        pSendContent.Add("δ���ҵ����û���Ϣ");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
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
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }

                        break;
                    #endregion
                    #region ����Ա��쳣����
                    case TaskService.TOOL_SQLPLUS_AU_GENDER_UPDATE:
                        if (pADOUtils.Connect(false))
                        {
                            #region AU��־-��ע��
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
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                        string _gendrresult = "����";
                        if ("F" == mReciceContent[0, 4].Content.ToString())
                        {
                            _gendervalue = "100";
                            _gendrresult = "Ů��";
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
                                #region ���ݱ�ʧ
                                pSendContent.Add("���ݱ�ʧ��");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);                                
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion

                    #region ���ǳ�ʼ�����쳣����
                    case TaskService.TOOL_SQLPLUS_AU_PARTYCARD_UPDATE:
                        #region Audition��Ϣ��ȡ
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                        #region Audition����
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
                                    pSendContent.Add("δ���ҵ����û���Ϣ");
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
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            break;
                            #endregion
                        }
                        #endregion
                        #region Item��Ϣ��ȡ
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                        #region Item����
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
                                    pSendContent.Add("δ���ҵ����û���Ϣ");
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
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }

                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                            }
                            catch (Exception ex)
                            {
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            #endregion
                        }
                        #endregion

                        #region AU��־-��ע��
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

                    #region ��������Ϣ��������¹�ϵ��Ϣ
                    case TaskService.TOOL_SQLPLUS_AU_MARRIAGEINFO_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_Audition.Replace("$server_layer", mReciceContent[0, 3].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                                        pSendContent.Add("δ���ҵ����û���Ϣ");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
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
                                            pSendContent.Add("�������ݣ�");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
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
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δδ����");
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
                                        pSendContent.Add("δ���ҵ����û���Ϣ");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    else
                                    {
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
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
                                            pSendContent.Add("�������ݣ�");
                                            pSendContent.AddRows();

                                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        }
                                        else
                                        {
                                            pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
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
                                #region ���ݲ�����
                                pSendContent.Add("���ݲ����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion
                            }
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        break;
                    #endregion

                    #region ��ӻ���ȯ
                    case TaskService.TOOL_SQLPLUS_AU_MARRIAGECARD_ADD:
                        #region ��ȡITEM���ݿ���������������
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δ����");
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
                                #region ��ӻ��޸ĵ���
                                #region �����û��Ƿ�ӵ�иõ���
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
                                if ("0" == mReciceContent[0, 9].Content.ToString())//�����µ���
                                {
                                    #region �������
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
                                    #region ���µ��ߵĹ�������
                                    Str_ItemSql = Constant.StrAuUpdateItem.Replace("$ItemTable_Name", "avatar_inventory_items");
                                    Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 10].Content.ToString());

                                    DateTime Update_Time = Convert.ToDateTime(pCheckSet.Tables[0].Rows[0]["ExpiredDate"].ToString());
                                    Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", Update_Time.AddDays(Convert.ToDouble(mReciceContent[0, 7].Content.ToString())).ToString());
                                    #endregion
                                }

                                #region ִ�в���
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
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = pMysqlUtils.Message;
                                    }
                                }
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion

                                #region AU��־
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
                                #region ���õ���
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
                                        pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                        _result = pMysqlUtils.Message;
                                    }
                                }
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                #endregion

                                #region AU��־-��ע��
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
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        #endregion
                        break;
                    #endregion

                    #region ��ȡ�����б�
                    case TaskService.TOOL_SQLPLUS_AU_ITEM_GET:
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGetItem;
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            pDataSet = (DataSet)pADOUtils.RecordData;

                            if (0 == pDataSet.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("��������");
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
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                            #endregion
                        }
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        break;
                    #endregion
                    #region ���ŵ���
                    case TaskService.TOOL_SQLPLUS_AU_SENDITEM_PUT:
                        #region ���������õ����Ƿ����
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
                                pSendContent.Add("�õ���ID�����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                            else if (0 == ItemTable_Set.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�õ���ID�����ڣ�");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                pSendBuffer = pSourcePacket.CoalitionInfo(); 
                                break;
                            }
                            ItemTable_Name = ItemTable_Set.Tables[0].Rows[0][3].ToString();
                        }
                        else
                        {
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            #endregion
                        }
                        #endregion

                        #region �жϵ����Ա�
                        if (("F" == mReciceContent[0, 10].Content.ToString() && "1" == ItemTable_Set.Tables[0].Rows[0][6].ToString()) || ("M" == mReciceContent[0, 10].Content.ToString() && "2" == ItemTable_Set.Tables[0].Rows[0][6].ToString()))
                        {
                            pSendContent.Add("�������û��Ա�ƥ�䣡");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo(); 
                            break;
                        }
                        #endregion

                        #region ��ȡITEM���ݿ���������������
                        if (pADOUtils.Connect(false))
                        {
                            string strsql = Constant.StrAuGet_ItemDB.Replace("$server_layer", mReciceContent[0, 1].Content.ToString());
                            pADOUtils.ExecuteQuery(strsql);
                            pADOUtils.GetResult(RecordStyle.DATASET);
                            auDataset = (DataSet)pADOUtils.RecordData;

                            if (0 == auDataset.Tables[0].Rows.Count)
                            {
                                pSendContent.Add("�������������򲻷���");
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
                                    pSendContent.Add("�÷�����������δ����");
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
                            #region �����û��Ƿ�ӵ�иõ���
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
                            if ("0" == mReciceContent[0, 9].Content.ToString())//�����µ���
                            {
                                #region �������
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
                            else//���µ��ߵĹ�������
                            {
                                #region ���µ��ߵĹ�������
                                Str_ItemSql = Constant.StrAuUpdateItem.Replace("$ItemTable_Name", ItemTable_Name);
                                Str_ItemSql = Str_ItemSql.Replace("$UserSN", mReciceContent[0, 8].Content.ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ItemID", mReciceContent[0, 5].Content.ToString());

                                DateTime Update_Time = Convert.ToDateTime(pCheckSet.Tables[0].Rows[0]["ExpiredDate"].ToString());
                                Str_ItemSql = Str_ItemSql.Replace("$ExpiredDate", Update_Time.AddDays(Convert.ToDouble(mReciceContent[0, 7].Content.ToString())).ToString());
                                #endregion
                            }

                            #region ִ�в���
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
                                    pSendContent.Add("���ݿ�ִ���쳣��" + pMysqlUtils.Message);
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                                    _result = pMysqlUtils.Message;
                                }
                            }
                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion

                            #region AU��־-��ע��
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
                            #region δ�������ݿ�
                            pSendContent.Add("���ݿ�����ʧ�ܣ�");
                            pSendContent.AddRows();

                            pSourcePacket = new SocketPacket(mTaskService, pSendContent);

                            pSendBuffer = pSourcePacket.CoalitionInfo();
                            #endregion
                        }
                        #endregion
                        break;
                    #endregion
                    default:
                        throw new Exception("δ֪���");
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
                    else//sql��䲻�Ϸ�
                    {
                        pSendContent.Add("sql��䲻����Լ������!");
                        pSendContent.AddRows();
                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                        pSendBuffer = pSourcePacket.CoalitionInfo();
                        this._pSocketMessage.Connection.OnSend(pSendBuffer);
                    }                    
                }
                else
                {
                    #region δ�������ݿ�
                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                    #region �����߼�
                    try
                    {
                        #region ��������
                        switch (mTaskService)
                        {
                            #region �û�����
                            case TaskService.COMMON_CONNECT:
                                pSendContent.Add("PASS");
                                pSendContent.AddRows();

                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            #endregion
                            #region �û���֤
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
                            #region �û�����
                            case TaskService.COMMON_DISCONNECT:
                                break;
                            #endregion

                            #region ϵͳ���ݹ���ģ��
                            #region ��ȡĿ¼���ڵ�
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
                                    pSendContent.Add("��������");
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

                            #region ���ģ����Ϣ Finished!
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
                            #region ��ȡģ����Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ɾ��ָ��ģ����Ϣ Finished!
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
                            #region ����ָ��ģ����Ϣ Finished!
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

                            #region ��Ӳɼ������Ϣ Finished!
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
                            #region ��ȡ�ɼ�����б� Finished!
                            case TaskService.SYSTEM_CRITERION_SORT_LIST:
                                strProcParams = "SP_GET_SORTINFO";

                                pADOUtils.ExecuteQuery(strProcParams);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("��������");
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
                            #region ����ָ���ɼ������Ϣ Finished!
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
                            #region ɾ��ָ���ɼ����� Finished!
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

                            #region ��ӷ������ɼ���� Finished!
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
                            #region ��ȡÿ̨�������ɼ���׼�б� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ���������ɼ���׼��Ϣ Finished!
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
                            #region ɾ��ָ���������ɼ���׼��Ϣ Finished!
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

                            #region ����SMS
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
                                            pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "���ͳɹ������ݱ���ʧ�ܣ�");
                                            pSendContent.AddRows();
                                            //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                            //pFailureStruct[0].oFiledsTypes = "String";
                                            //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                            //pFailureStruct[1].oFieldsName = "Sender_Content";
                                            //pFailureStruct[1].oFiledsTypes = "String";
                                            //pFailureStruct[1].oFieldValues = "���ͳɹ������ݱ���ʧ�ܣ�";
                                        }
                                        else
                                        {
                                            pSendContent.Add(DataField.LINKER_MOBILE, DataFormat.STRING, arrReciveMobile[i]);
                                            pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "���ͳɹ������ݱ���ɹ���");
                                            pSendContent.AddRows();
                                            //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                            //pFailureStruct[0].oFiledsTypes = "String";
                                            //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                            //pFailureStruct[1].oFieldsName = "Sender_Content";
                                            //pFailureStruct[1].oFiledsTypes = "String";
                                            //pFailureStruct[1].oFieldValues = "���ͳɹ������ݱ���ɹ���";
                                        }
                                    }
                                    else
                                    {
                                        pSendContent.Add(DataField.LINKER_MOBILE, DataFormat.STRING, arrReciveMobile[i]);
                                        pSendContent.Add(DataField.LINKER_CONTENT, DataFormat.STRING, "����ʧ�ܣ�����δ���棡");
                                        pSendContent.AddRows();
                                        //pFailureStruct[0].oFieldsName = "Sender_Mobile";
                                        //pFailureStruct[0].oFiledsTypes = "String";
                                        //pFailureStruct[0].oFieldValues = arrReciveMobile[i];

                                        //pFailureStruct[1].oFieldsName = "Sender_Content";
                                        //pFailureStruct[1].oFiledsTypes = "String";
                                        //pFailureStruct[1].oFieldValues = "����ʧ�ܣ�����δ���棡";
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
                                    pSendContent.Add("��������");
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

                            #region ����Ȩ��
                            #region �����ȡ����ģ����Ϣ
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡ�����Ӧ�������ֶ�
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
                                    pSendContent.Add("��������");
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
                            #region ��������Ȩ��
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
                            #region �޸�����Ȩ��
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
                            #region ɾ������Ȩ��
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
                            #region ��ȡ����������Ȩ���б�
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
                                    pSendContent.Add("��������");
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

                            #region ϵͳ��־
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
                                pSendContent.Add("���������ʹ�ã�");
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
                                    pSendContent.Add("��������");
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
                            #region ϵͳģ�岿��
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
                                    pSendContent.Add("��������");
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
                            #region �ļ����
                            case TaskService.SYSTEM_UPDATE_CHECK_GET:
                                bool bUpdate = false;
                                int iVersionCount = 0;
                                string strFileName = String.Empty;

                                //���Clientû�е��ļ�
                                DirectoryInfo pDirectory = new DirectoryInfo("./update/");

                                //��������ļ�λ��
                                int iAppPathLength = pDirectory.FullName.Length;

                                //�����ļ�����
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

                                //ȷ���汾������ĸ���
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

                                //�����ļ���Ҫ���µ�ʱ��
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

                                    //������Ҫ���µ��ļ���Ϣ
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

                                    //ö�ٸ�Ŀ¼�ļ�
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

                                    //ö����Ŀ¼�ļ�
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
                                    pSendContent.Add("����Ҫ���µ��ļ���");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region �ļ�����
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

                                    pSendContent.Add("�޷�������Ҫ���µ��ļ���");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #endregion

                            #region �û���Ϣ����
                            #region ��ӹ�������Ϣ Finished!
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
                            #region ��ȡ��������Ϣ Finished!
                            case TaskService.MEMBER_GROUP_LIST:
                                strProcParams = "SP_GET_GROUPINFO";

                                pADOUtils.ExecuteQuery(strProcParams);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pADOUtils.AffectRow)
                                {
                                    pSendContent.Add("��������");
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
                            #region ����ָ����������Ϣ Finished!
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
                            #region ɾ��ָ����������Ϣ Finished!
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

                            #region �����Ա��Ϣ Finished!
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
                            #region ��ȡ��Ա��Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ����Ա��Ϣ Finished!
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
                            #region ɾ��ָ����Ա��Ϣ Finished!
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

                            #region ����ʺ���Ϣ Finished!
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
                            #region ��ȡ�ʺ���Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ���ʺ���Ϣ Finished!
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
                            #region ɾ��ָ���ʺ���Ϣ Finished!
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

                            #region ���ָ���ʺ�0Ȩ����Ϣ Finished!
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
                            #region ��ȡָ���û�Ȩ��0��Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ���û�Ȩ��0��Ϣ Finished!
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
                            #region ɾ��ָ���û�Ȩ��0��Ϣ No Used!
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

                            #region ��ȡָ��Ȩ����ϸ��Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ��������Ϣ Finished!
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

                            #region ��ȡ�û��ʺ�Ȩ��Ϊ1��Ȩ����Ϣ
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
                                    pSendContent.Add("��������");
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
                            #region ����û��ʺ�Ȩ��Ϊ1��Ȩ����Ϣ Finished!
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
                            #region �޸��û�����������Ȩ�� Finished
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
                            #region ɾ���û�����������Ȩ�� -- ��֪���Ƿ�ʹ����
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

                            #region ��ȡ�û��в���Ȩ�޵���Ϸ�б�
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡָ����Ϸ���û��в���Ȩ�޵ķ������б�
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
                                    pSendContent.Add("��������");
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

                            #region ���ʿ���
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
                                    pSendContent.Add("��������");
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
                            #region ���ʿ���Ȩ��
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
                                    pSendContent.Add("��������");
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
                            #region �����û�Ȩ�ޡ����Զ���
                            case TaskService.MEMBER_SERVERACCESSBYUSER_SORT_UPDATE:
                                bFailure = false;
                                string[] str_verify = mReciceContent[0, 0].Content.ToString().Split("|".ToCharArray());//��Ա
                                string[] str_server = mReciceContent[0, 1].Content.ToString().Split("|".ToCharArray());//������id
                                string[] str_sort = mReciceContent[0, 2].Content.ToString().Replace("999", "-1").Split("|".ToCharArray());//Ȩ��

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
                            #region �����û�Ȩ�ޡ��������
                            case TaskService.MEMBER_SERVERACCESSBYUSER_SORT_UPDATE_KIND:
                                string[] Str_Verify = mReciceContent[0, 0].Content.ToString().Split("|".ToCharArray());//��Ա
                                string[] Str_Game = mReciceContent[0, 1].Content.ToString().Split("|".ToCharArray());//��Ϸid
                                string[] Str_Kind = mReciceContent[0, 2].Content.ToString().Split("|".ToCharArray());//����ϵͳ
                                string[] Str_ServerType = mReciceContent[0, 3].Content.ToString().Split("|".ToCharArray());//����������
                                string[] Str_Sort = mReciceContent[0, 4].Content.ToString().Replace("999", "-1").Split("|".ToCharArray());//Ȩ��

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
                                    pSendContent.Add("��������");
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
                            #region ɾ��ָ���û���Ȩ��
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
                            #region �û����������ѯ
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
                                    pSendContent.Add("��������");
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

                            #region ����û�mac��
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
                            #region ��ȡָ����Ա��Ϸ�б�
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
                                    pSendContent.Add("��������");
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

                            #region ��������Ϣ�ռ�
                            #region �����Ϸ��Ϣ Finished!
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
                            #region ��ȡ��Ϸ�б� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ����Ϸ��Ϣ Finished!
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
                            #region ɾ��ָ����Ϸ��Ϣ Finished!
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

                            #region ��ӷ�������Ϣ Finished!
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
                            #region ��ȡ�������б� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ����ָ����������Ϣ Finished!
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
                            #region ɾ��ָ����������Ϣ Finished!
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

                            #region IP���뼰�����
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
                            #region �ɼ���ֵ�¹���
                            //��ȡ�вɼ���ֵ�Ŀ��ƻ��б�
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
                                    pSendContent.Add("��������");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else
                                {
                                    pSourcePacket = new SocketPacket(mTaskService, buildTLV(pDataSet, false));
                                }

                                pSendBuffer = pSourcePacket.CoalitionInfo();
                                break;
                            //ͬһ���ͷ���������ֵ����ͬ��
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
                            #region ��IP��ַ���з�������ѯ
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
                                    pSendContent.Add("��������");
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
                            #region ��������ϲ�ѯ
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
                                    pSendContent.Add("��������");
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
                            #region ��Ϸ����������ͳ��
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡĳ���ƻ��·�������Ϣ
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
                                    pSendContent.Add("��Ӧ������Ϣ��δ���");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    if (0 == pADOUtils.AffectRow)
                                    {
                                        pSendContent.Add("��Ӧ������Ϣ��δ���");
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
                            #region ��IPList�ļ����������޸�
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
                            #region ����������
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
                            #region ��ʾ�ʺſɿ�ip
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
                                    pSendContent.Add("��������");
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

                            #region ������ά���ƻ�������ͳ��
                            #region Notes����
                            #region NOTES��ϵ�˻�ȡ
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
                            #region NOTES��Ϣ��ȡ
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
                                    pSendContent.Add("��������");
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
                            #region NOTES��Ϣ����
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
                            #region NOTES��Ϣת��
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
                            #region NOTES��Ϣ������ȡ
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
                                        pSendContent.Add("���ʼ�δ�ҵ��������ʼ���ɾ����");
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
                            #region NOTES��Ϣ�鿴�޸�
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
                            #region NOTES��Ϣɾ��
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
                            #region ����ͳ�Ʋ���
                            #region ����ͳ������
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
                                    pSendContent.Add("��������");
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("��������");
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
                            #region ����ͳ��--��ģ�����Ϊ����
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
                                    pSendContent.Add("��������");
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
                            #region ϵͳά������...
                            #region ά����Ϣ
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("��������");
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
                                    pSendContent.Add("��������");
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
                            #region ά������
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
                                    pSendContent.Add("�����쳣");
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
                                    pSendContent.Add("��������");
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

                            #region ��ȡ�������
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
                                    pSendContent.Add("��������");
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
                            #region ��Ӽ������ ------------------------
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
                            #region ���¼������ --------------------------
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
                            #region ɾ��������� --------------------------
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

                            #region ϵͳ����
                            #region ִ�й̶���SQL���
                            case TaskService.TOOL_RUNSQL_SELECT:
                                string StrSql = "SELECT (SELECT Server_Internet FROM Game_Server WHERE Server_ID=" + mReciceContent[0, 0].Content.ToString() + ") AS Server_Internet,Access_User,Access_Pwd FROM Server_Access WHERE Access_Sort=14 AND Access_Server=" + mReciceContent[0, 0].Content.ToString();
                                pADOUtils.ExecuteQuery(StrSql);
                                pADOUtils.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pADOUtils.RecordData;

                                if (0 == pDataSet.Tables[0].Rows.Count)
                                {
                                    pSendContent.Add("��ѡ����������ݿ���������в�����");
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
                                        pSendContent.Add("ִ�н���");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    pSendContent.Add("�����ݿ⣬��ϸ���ݣ�" + ex.Message);
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                break;
                            #endregion
                            #region �ļ�����
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

                            #region ������״̬��Ϣ
                            #region Ӳ��ά��
                            #region ��ȡӲ��ά����Ϣ ��Ϸ������������������ʼʱ�䣬״̬��ά����
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
                                    pSendContent.Add("��������");
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
                            #region ���Ӳ��ά����Ϣ
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
                            #region �޸�Ӳ����Ϣ״̬
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
                            #region 222��ֲ
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
                                    pSendContent.Add("��������");
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
                                    pSendContent.Add("��������");
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
                                    pSendContent.Add("��������");
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
                            #region �鿴����
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
                                        pSendContent.Add("��������");
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
                                        pSendContent.Add("��������");
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
                            #region ��ȡ����֪ͨ�б� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡ�����б� Finished!
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
                                    pSendContent.Add("������̫������С��ѯ��Χ��");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("�������ݣ�");
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
                            #region ��ȡָ��������ϸ��Ϣ Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡ������־�б� Finished!
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
                                    pSendContent.Add("������̫������С��ѯ��Χ��");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                }
                                else if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("�������ݣ�");
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
                            #region ������漰��־�б� Finished!
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
                            #region ���洦����Ϣ Finished!
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
                            #region ��ô������� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ��ü���� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡ������Ϣ���� Finished! ---
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
                                        pSendContent.Add("��������");
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
                                            pSendContent.Add("��������");
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
                                        pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                        pSendContent.AddRows();

                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                        pSendBuffer = pSourcePacket.CoalitionInfo();
                                    }
                                }

                                break;
                            #endregion
                            #region ��ȡ����������� Finished!
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
                                    pSendContent.Add("��������");
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
                            #region �鿴�������ϸ��Ϣ Finished!
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

                            #region DEBUG�ύ
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
                                    pSendContent.Add("��������");
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

                            #region ��ʱ��Ϣ����...
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
                                    pSendContent.Add("��������");
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
                                pSendContent.Add("���������ʹ�ã�");
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
                                pSendContent.Add("���������ʹ�ã�");
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
                                    pSendContent.Add("��������");
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
                            #region ��ʱ��Ϣ����
                            #region �����Ϣ����
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
                            #region ��ȡ��Ϣ����
                            case TaskService.INSTANT_LEACH_GET:
                                strProcParams = "SP_GET_LEACHMESSAGE";
                                pOnlinenum = new MSSQLOperate(this.strReportConn);
                                pOnlinenum.Connect(false);
                                pOnlinenum.ExecuteQuery(strProcParams);
                                pOnlinenum.GetResult(RecordStyle.DATASET);
                                pDataSet = (DataSet)pOnlinenum.RecordData;

                                if (0 == pOnlinenum.AffectRow)
                                {
                                    pSendContent.Add("��������");
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
                            #region ɾ����Ϣ����
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
                            #region ��ȡ��Ϸ����������������Ȩ�޵��б�
                            #region ��Ϸ
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
                                    pSendContent.Add("��������");
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
                            #region ����
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
                                    pSendContent.Add("��������");
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
                            #region ������
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
                                    pSendContent.Add("��������");
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
                            #region Ȩ��
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
                                    pSendContent.Add("��������");
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
                            #region ��ȡ���ݿ��б�
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
                                        pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ��ȡ���ݿ�ṹ������ͼ�����������洢���̵��б�
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion

                            #region ��
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region ��ͼ
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region �洢����
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion
                            #region ����
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
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
                                                pSendContent.Add("��������");
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
                                                pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion

                            #region ��ȡ���ݿ���������
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
                                        pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ��ȡ�������������
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
                                        pSendContent.Add("��������");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion

                            #region �޸ı�ṹ
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
                                            case AlterTableStyle.ADDCOLUMN://����ֶ�
                                                Update_Sql += ps.Alter_Table_AddColumn(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.DROPOBJECT://ɾ������
                                                Update_Sql += ps.Alter_Table_Drop(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ALTERCOLUMN://�޸��ֶ�
                                                Update_Sql += ps.Alter_Table_Alter(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDPRIMARYKEY://�����������������
                                                Update_Sql += ps.Alter_Table_AddPrimaryKey(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDFOREIGNKEY://����������������
                                                Update_Sql += ps.Alter_Table_AddForeignKey(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDDEFAULT://�������������Ĭ��ֵ
                                                Update_Sql += ps.Alter_Table_AddDefault(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDCHECK://�������������CheckԼ��
                                                Update_Sql += ps.Alter_Table_AddCheck(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDUNIQUE://�������������Ψһ��Լ��
                                                Update_Sql += ps.Alter_Table_AddUnique(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.ADDEXTEND://�����չ����
                                                Update_Sql += ps.Alter_Table_AddExtend(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.MODEXTEND://�޸���չ����
                                                Update_Sql += ps.Alter_Table_ModExtend(mReciceContent, i) + ";";
                                                break;
                                            case AlterTableStyle.DELEXTEND://ɾ����չ����
                                                Update_Sql += ps.Alter_Table_DelExtend(mReciceContent, i) + ";";
                                                break;
                                            default:
                                                Update_Sql += "";
                                                break;
                                            //throw new Exception("δ֪���");
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ִ����䣬�޷��ؽ����
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
                                            pSendContent.Add("����:" + ps.Message);
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
                                        pSendContent.Add("�쳣:" + ex.Message);
                                        pSendContent.AddRows();
                                    }

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ������
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
                                            pSendContent.Add("����:" + ps.Message);
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ��ѯ��
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

                                        if (string.Empty != ps.Message)//��䲻����Լ������
                                        {
                                            pSendContent.Add("����:" + ps.Message);
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
                                                pSendContent.Add("��������");
                                                pSendContent.AddRows();
                                                pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                            }
                                        }
                                    }
                                    else//sql��䲻�Ϸ�
                                    {
                                        pSendContent.Add("sql��䲻����Լ������!");
                                        pSendContent.AddRows();
                                        pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    }
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                }
                                else
                                {
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #region ɾ������
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
                                            pSendContent.Add("����:" + ps.Message);
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
                                    #region δ�������ݿ�
                                    pSendContent.Add("���ݿ�����ʧ�ܣ�");
                                    pSendContent.AddRows();

                                    pSourcePacket = new SocketPacket(mTaskService, pSendContent);
                                    pSendBuffer = pSourcePacket.CoalitionInfo();
                                    #endregion
                                }
                                break;
                            #endregion
                            #endregion

                            #region ��Ϣģ��/�ļ��ϴ�
                            case TaskService.SYSTEM_UPDATE_PUT_GET:
                            default:
                                throw new Exception("δ֪���");
                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                    #region �쳣����
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

        #region ������Ϣ
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

        #region ˽���ֶ�
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

        private int int_groupid;//�û�������id
        private int int_verifyid;//�û��ʺ�id
        private CommonOrder pCommonOrder;

        private MSSQLOperate pOnlinenum = null;

        private MysqlUtils pMysqlUtils = null;
        private string str_auuser = null;
        private string str_aupwd = null;
        private string strMysqlConn;
        private DataSet auDataset;
        #endregion

        #region ˽�з���
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
            //����ҳ��
            int pageCount = tRecord.Tables[0].Rows.Count % iPageSize;

            if (pageCount > 0)
            {
                pageCount = tRecord.Tables[0].Rows.Count / iPageSize + 1;
            }
            else
            {
                pageCount = tRecord.Tables[0].Rows.Count / iPageSize;
            }

            //ȷ����ǰ��¼��
            if ((iCurrentPage - 1) * iPageSize + iPageSize > tRecord.Tables[0].Rows.Count)
            {
                iCurentCount = tRecord.Tables[0].Rows.Count - (iCurrentPage - 1) * iPageSize;
            }
            else
            {
                iCurentCount = iPageSize;
            }

            #region ����
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
        /// �滻�ַ������÷�����Ҫ���¶���
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
