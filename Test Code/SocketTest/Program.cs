using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Net;

using System.Global;
using Windows.Network;
using Windows.Service;
using System.DataCenter;
using System.NotesCore;
using Domino;
using System.DataPacket;
using System.Define;
using SMSCore;
using MySql.Data.MySqlClient;

namespace SocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃                           ※服务器状态监控※                             ┃");
            Console.WriteLine("┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫");
            Console.WriteLine("┃                           Ver : 3.0 Release                              ┃");
            Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
                                   
            #region 读配置文件
            string strPath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            IniFile mIniFile = new IniFile(strPath + @"\Scheme\Scheme.INI");

            //片断 [SERVER]
            string strServer = mIniFile.ReadValue("SERVER", "Address", "127.0.0.1");
            string strPort = mIniFile.ReadValue("SERVER", "Port", "4020");
            string strMaxClient = mIniFile.ReadValue("SERVER", "MaxClient", "10");
            string strBufferSize = mIniFile.ReadValue("SERVER", "Buffer", "4096");
            string strInterval = mIniFile.ReadValue("SERVER", "Interval", "15000");
            //Console.WriteLine(strInterval.ToString());
            #endregion

            #region 重置用户信息
            strAnalyse = @"Server=192.168.24.132\sqlexpress;Database=gameReport;uid=sa; pwd=1234;";
            MSSQLOperate pResetVerify = new MSSQLOperate(strAnalyse);
            pResetVerify.Connect(false);
            pResetVerify.ExecuteQuery("UPDATE Member_Verify SET Verify_Status = 0, Verify_Session = 'N/A'");
            pResetVerify.GetResult(RecordStyle.NONE);
            #endregion            

            //pNotesSession.Initialize("12341234");
            bool b_Control = false;

            #region 收取Notes信件
            double TimeInterval = 0;
            try
            {
                TimeInterval = Convert.ToDouble(strInterval);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            System.Timers.Timer t = new System.Timers.Timer(TimeInterval);
            t.Elapsed += new System.Timers.ElapsedEventHandler(CheckMail);
            t.AutoReset = true;

            if (TimeInterval != 0)
            {
                t.Enabled = b_Control;
            } 
            #endregion

            #region 移动Notes信件
            System.Timers.Timer tt = new System.Timers.Timer(TimeInterval + 500);
            tt.Elapsed += new System.Timers.ElapsedEventHandler(MoveMail);
            tt.AutoReset = true;

            if (TimeInterval != 0)
            {
                tt.Enabled = b_Control;
            }
            #endregion

            #region 实时监控
            pRealTime = new System.Timers.Timer(10000);
            pRealTime.Elapsed += new System.Timers.ElapsedEventHandler(RealtimeWarning);
            pRealTime.AutoReset = true;
            pRealTime.Enabled = true;
            #endregion

            #region 数据库连接监控
            Thread tConnectTime = new Thread(new ThreadStart(ConnectTime));
            tConnectTime.Start();
            #endregion

            #region 定时发送AU日志
            //pSendAuLog = new System.Timers.Timer(604800000);
            //pSendAuLog.Elapsed += new System.Timers.ElapsedEventHandler(SendAuLog);
            //pSendAuLog.AutoReset = true;
            //pSendAuLog.Enabled = b_Control;
            #endregion

            IPEndPoint pServerPoint = new IPEndPoint(0, int.Parse(strPort));
            SocketEvent pEvent = new SocketEvent(ServerEvent);
            pServer = new AsyncService(NetServiceType.HOST, pEvent);
            pServer.StarService(pServerPoint, CompressionType.DEFLATE, 1024);

            while (true)
            {
                Thread.Sleep(0);
                #region
                string[] strInput = Console.ReadLine().Trim().Split(" ".ToCharArray(), 2);

                if (strInput.Length > 0)
                {
                    switch (strInput[0])
                    {
                        case "/R":
                            pResetVerify = new MSSQLOperate(strAnalyse);
                            pResetVerify.Connect(false);
                            pResetVerify.ExecuteQuery("UPDATE Member_Verify SET Verify_Status = 0, Verify_Sign = 'N/A', Verify_Session = 'N/A' WHERE Verify_Nick = '" + strInput[1] + "'");
                            pResetVerify.GetResult(RecordStyle.NONE);

                            if (pResetVerify.AffectRow > 0)
                            {
                                Console.WriteLine("成功重置" + strInput[1] + "的登录信息!");
                            }
                            else
                            {
                                Console.WriteLine("重置" + strInput[1] + "的登录信息失败!");
                            }
                            break;
                        case "/C":
                            pResetVerify = new MSSQLOperate(strAnalyse);
                            pResetVerify.Connect(false);
                            pResetVerify.ExecuteQuery("UPDATE Member_Verify SET Verify_Status = 1, Verify_Session = 'N/A' WHERE Verify_Nick = '" + strInput[1] + "'");
                            pResetVerify.GetResult(RecordStyle.NONE);

                            if (pResetVerify.AffectRow > 0)
                            {
                                Console.WriteLine("成功锁定" + strInput[1] + "的登录信息!");
                            }
                            else
                            {
                                Console.WriteLine("锁定" + strInput[1] + "的登录信息失败!");
                            }
                            break;
                        case "reset":
                            Console.WriteLine("Please Input /R [NickName]");
                            break;
                        case "restart":
                            pServer.StopService();
                            Thread.Sleep(500);
                            pServer.StarService(pServerPoint, CompressionType.DEFLATE, 1024);
                            Console.WriteLine("Server Restarted");
                            break;
                        case "stop":
                            pServer.StopService();
                            return;
                        case "clear":
                            Console.Clear();
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("无效指令");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                    }
                }
                #endregion
            }
        }

        #region 方法
        static void ConnectTime()
        {
            pConnectTime = new System.Timers.Timer(300000);
            pConnectTime.Elapsed += new System.Timers.ElapsedEventHandler(ConnecttimeWarning);
            pConnectTime.AutoReset = true;
            pConnectTime.Enabled = false;
        }

        static void ConnecttimeWarning(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer pp = (System.Timers.Timer)source;
            pp.Enabled = false;
            string[] _strgame = GetConnect().Split(",".ToCharArray());

            for (int m = 0; m < _strgame.Length; m++)
            {
                int game_id = int.Parse(_strgame[m]);

                #region 电话列表
                string str_sendsql = "SELECT Mobile_Phone FROM SMS_Mobile WHERE Mobile_Game =" + game_id;
                MSSQLOperate pSms = new MSSQLOperate(strAnalyse);
                pSms.Connect(false);
                pSms.ExecuteQuery(str_sendsql);
                pSms.GetResult(RecordStyle.DATASET);
                DataSet psDataSet = (DataSet)pSms.RecordData;
                #endregion

                string str_sql = "SELECT Server_Internet,Access_User,Access_Pwd,Server_DBtype,Game_Name,Server_Name FROM Game_Server,Server_Access,Info_Game WHERE Server_DBtype <> 127 AND Server_Game = " + game_id + " AND Access_Server = Server_ID AND Game_ID=Server_Game AND Access_Sort = 14";
                MSSQLOperate pConnect = new MSSQLOperate(strAnalyse);
                pConnect.Connect(false);
                pConnect.ExecuteQuery(str_sql);
                pConnect.GetResult(RecordStyle.DATASET);
                DataSet pDataSet = (DataSet)pConnect.RecordData;

                if (pConnect.AffectRow > 0)
                {
                    for (int i = 0; i < pDataSet.Tables[0].Rows.Count; i++)
                    {
                        bool pResult = true;
                        string str_user = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[i][1].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));
                        string str_pwd = System.Text.Encoding.UTF8.GetString(Declassified.Decrypt(System.Convert.FromBase64String(pDataSet.Tables[0].Rows[i][2].ToString()), System.Text.Encoding.UTF8.GetBytes("1234567890abcdef")));

                        if ("2" == pDataSet.Tables[0].Rows[i][3].ToString())//
                        {
                            #region SQL SERVER
                            string str_conncet = "Connect Timeout=300000;Server=" + pDataSet.Tables[0].Rows[i][0].ToString() + ";Database=master;uid=" + str_user + "; pwd=" + str_pwd;

                            MSSQLOperate pSqlConnect = new MSSQLOperate(str_conncet);
                            try
                            {
                                if (!pSqlConnect.Connect(false))
                                {
                                    //连接失败
                                    pResult = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                //连接失败
                                pResult = false;
                            }
                            #endregion
                        }
                        else if ("3" == pDataSet.Tables[0].Rows[i][3].ToString())//
                        {
                            #region MYSQL
                            string str_conncet = "Connect Timeout=300000;Server=" + pDataSet.Tables[0].Rows[i][0].ToString() + ";uid=" + str_user + "; pwd=" + str_pwd;

                            MysqlUtils pMysqlConnect = new MysqlUtils(str_conncet);
                            try
                            {
                                if (!pMysqlConnect.Connected())
                                {
                                    //连接失败
                                    pResult = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                //连接失败
                                pResult = false;
                            }
                            #endregion
                        }

                        #region 发送短信
                        if (!pResult)//连接失败，发送短信
                        {
                            if (pSms.AffectRow > 0)
                            {
                                for (int j = 0; j < pSms.AffectRow; j++)
                                {
                                    try
                                    {
                                        WebSMS pWebSMSSend = new WebSMS();
                                        string str_send = "游戏‘" + pDataSet.Tables[0].Rows[i][4].ToString() + "’，服务器‘" + pDataSet.Tables[0].Rows[i][5].ToString() + "’连接失败";
                                        byte[] pResultBuffer = pWebSMSSend.sendSMS(psDataSet.Tables[0].Rows[j][0].ToString(), str_send);
                                        int iResult = Encoding.Default.GetString(pResultBuffer) == "OK" ? 1 : 0;
                                    }
                                    catch
                                    { }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            pp.Enabled = true;
        }

        static void SendAuLog(object source, System.Timers.ElapsedEventArgs e)
        {
            NotesUtils pNotesUtils = null;
            try
            {
                string str_sql = "SELECT * FROM Au_Log";
                MSSQLOperate pSendLog = new MSSQLOperate(strAnalyse);
                pSendLog.Connect(false);
                pSendLog.ExecuteQuery(str_sql);
                pSendLog.GetResult(RecordStyle.DATASET);
                DataSet pSet = (DataSet)pSendLog.RecordData;

                string _str = null;

                if (pSendLog.AffectRow > 0)
                {
                    for (int i = 0; i < pSendLog.AffectRow; i++)
                    {
                        for (int j = 0; j < pSet.Tables[0].Columns.Count; j++)
                        {
                            _str += pSet.Tables[0].Rows[i][j].ToString();
                            _str += "\a\t\a\t";
                        }
                        _str += "\r\n";
                    }


                    //pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\费亚平.nsf");
                    pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\netadmin.nsf");

                    if (pNotesUtils.OpenDataBase("黄恒泰", "12341234"))
                    {
                        pNotesUtils.SendMailInfo("", "", "Au日志", _str);
                    }
                }
            }
            catch
            { }
            finally
            {
                if (pNotesUtils != null)
                {
                    pNotesUtils.Dispose();
                }
            }
        }

        static void MoveMail(object source, System.Timers.ElapsedEventArgs e)
        {
            NotesUtils pNotesUtils = null;
            Console.WriteLine("Notes Move!");
            System.Timers.Timer pp = (System.Timers.Timer)source;
            pp.Enabled = false;

            try
            {
                //pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\费亚平.nsf");
                pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\netadmin.nsf");

                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                {
                    if (pNotesUtils.MoveMailInfo())
                    {
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Notes Move Exception : " + pNotesUtils.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Notes Connect Exception : " + pNotesUtils.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Notes Save Exception : " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");
            }
            finally
            {
                if (pNotesUtils != null)
                {
                    pNotesUtils.Dispose();
                }
            }

            pp.Enabled = true;
        }

        static void CheckMail(object source, System.Timers.ElapsedEventArgs e)
        {
            NotesUtils pNotesUtils = null;
            Console.WriteLine("Notes Check:");
            System.Timers.Timer pp = (System.Timers.Timer)source;
            pp.Enabled = false;

            try
            {
                //pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\费亚平.nsf");
                pNotesUtils = new NotesUtils(pNotesSession, "mail9you/runstar", "mail\\netadmin.nsf");

                if (pNotesUtils.OpenDataBase("netadmin", "12341234"))
                {
                    if (pNotesUtils.GetMailInfo())
                    {
                        MSSQLOperate pSaveMail = new MSSQLOperate(strAnalyse);

                        CustomDataCollection pMailStruct = (CustomDataCollection)pNotesUtils.Records;
                        CustomData[,] pMailInfo = pMailStruct[-1];

                        for (int i = 0; i < pMailStruct.RowCount; i++)
                        {
                            pSaveMail.Connect(false);
                            string strProcParams = "SP_PUT_NOTESCONTENT";
                            DbParameter[] paramCache = DataUtilities.GetParameters(strProcParams);

                            #region
                            if (paramCache == null)
                            {
                                paramCache = new SqlParameter[]{
												   new SqlParameter("@iFailure",SqlDbType.Int, 4),
												   new SqlParameter("@iCategory",SqlDbType.Int, 4),
												   new SqlParameter("@strUID",SqlDbType.VarChar,100),
												   new SqlParameter("@strPUID",SqlDbType.VarChar,100),
												   new SqlParameter("@strSubject",SqlDbType.VarChar,100),
												   new SqlParameter("@dtPost",SqlDbType.DateTime),
												   new SqlParameter("@strSender",SqlDbType.VarChar,50),
												   new SqlParameter("@strRecive",SqlDbType.Text),
												   new SqlParameter("@strContent",SqlDbType.Text),
												   new SqlParameter("@strCount",SqlDbType.VarChar, 100),
												   new SqlParameter("@iView",SqlDbType.Int, 4),
                                                   new SqlParameter("@iGroup",SqlDbType.Int,4)
											   };
                                DataUtilities.SetParameters(strProcParams, paramCache);
                            }

                            paramCache[0].Value = 0;
                            paramCache[1].Value = 0;
                            paramCache[2].Value = pMailInfo[i, 0].Content;
                            paramCache[3].Value = pMailInfo[i, 1].Content;
                            paramCache[4].Value = pMailInfo[i, 2].Content;
                            paramCache[5].Value = pMailInfo[i, 4].Content;
                            paramCache[6].Value = pMailInfo[i, 3].Content;
                            paramCache[7].Value = pMailInfo[i, 5].Content;
                            paramCache[8].Value = pMailInfo[i, 6].Content;
                            paramCache[9].Value = pMailInfo[i, 7].Content;
                            paramCache[10].Value = 0;
                            paramCache[11].Value = 2;
                            #endregion

                            pSaveMail.ExecuteQuery(false, strProcParams, paramCache);
                            pSaveMail.GetResult(RecordStyle.NONE);
                        }

                        #region
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Notes Recive Count : " + pMailStruct.RowCount);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("");
                        #endregion
                    }
                    else
                    {
                        #region
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Notes Recive Exception : " + pNotesUtils.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("");
                        #endregion
                    }
                }
                else
                {
                    #region
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Notes Connect Exception : " + pNotesUtils.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Notes Save Exception : " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");
                #endregion
            }
            finally
            {
                if (pNotesUtils != null)
                {
                    pNotesUtils.Dispose();
                }
            }

            pp.Enabled = true;

        }

        static void RealtimeWarning(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer pp = (System.Timers.Timer)source;
            pp.Enabled = false;

            MSSQLOperate pRealtimeCheck = new MSSQLOperate(strAccess);//(strAccess);
            pRealtimeCheck.Connect(false);
            pRealtimeCheck.ExecuteQuery("EXEC SP_GET_REALTIMECONTENT");// ("EXEC SP_GET_REALTIMECONTENT");
            pRealtimeCheck.GetResult(RecordStyle.DATASET);
            DataSet pDataSet = (DataSet)pRealtimeCheck.RecordData;

            if (null != pDataSet && pDataSet.Tables[0].Rows.Count > 0)
            {
                CustomDataCollection mReturnBody = new CustomDataCollection(StructType.CUSTOMDATA);

                foreach (DataTable pDataTable in pDataSet.Tables)
                {
                    for (int i = 0; i < pDataTable.Rows.Count; i++)
                    {
                        uint usFirstTag = 0x08000001;

                        for (int j = 0; j < pDataTable.Columns.Count; j++)
                        {
                            mReturnBody.Add((DataField)usFirstTag, DefineUtilities.ToDataFormat(pDataTable.Columns[j].DataType.Name), pDataTable.Rows[i][j]);

                            usFirstTag++;
                        }
                        mReturnBody.AddRows();
                        
                    }
                }

                SocketPacket mSocketPacket = new SocketPacket(TaskService.INSTANT_MONITOR_SEND, mReturnBody);

                byte[] pSendBuffer = mSocketPacket.CoalitionInfo();

                if (null != pServer.SocketConnected)
                {
                    pServer.SocketConnected.ServerConnection().SendTo(pSendBuffer, true);
                    pServer.SocketConnected.OnRecive();

                    //pRealtimeCheck.ExecuteQuery("EXEC SP_SET_VIEW");
                    //pRealtimeCheck.GetResult(RecordStyle.NONE);
                }
            }

            pp.Enabled = true;
        }

        static void ServerEvent(object pObject)
        {
            if (pObject.GetType() == typeof(SocketEventArgs))
            {
                SocketEventArgs pSocketEvent = (SocketEventArgs)pObject;
                #region
                if (pSocketEvent.Link)
                {
                    Console.Write("※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※");
                    Console.WriteLine("Client [{0}] Connected!", pSocketEvent.Connection.SocketEndPoint.Address.ToString());
                }
                else
                {
                    string strProcParams = "SP_VERIFY_LOGOUT";
                    DbParameter[] paramCache = DataUtilities.GetParameters(strProcParams);

                    if (paramCache == null)
                    {
                        paramCache = new SqlParameter[]{
												   new SqlParameter("@strSession",SqlDbType.VarChar,50)
											   };
                        DataUtilities.SetParameters(strProcParams, paramCache);
                    }

                    paramCache[0].Value = pSocketEvent.Connection.Session;

                    MSSQLOperate pLogout = new MSSQLOperate(strAnalyse);
                    pLogout.Connect(false);
                    pLogout.ExecuteQuery(false, strProcParams, paramCache);
                    pLogout.GetResult(RecordStyle.NONE);

                    Console.WriteLine("Client [{0}, {1}] Disconnected!", pSocketEvent.Connection.SocketEndPoint.Address.ToString(), pSocketEvent.Connection.Session);

                    if (null != pSocketEvent.SocketException)
                    {
                        Console.WriteLine("Socket Exception : {0}", pSocketEvent.SocketException.Message);
                    }

                    Console.Write("※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※");
                }
                #endregion
                ((SocketEventArgs)pObject).Connection.OnRecive();                
            }
            else if (pObject.GetType() == typeof(MessageEventArgs))
            {
                DateTime tStartTime = DateTime.Now;

                TaskParser pTaskParser = new TaskParser(pNotesSession, (MessageEventArgs)pObject, strAnalyse, strOnline, strAccess);

                if (!pTaskParser.Parser())
                {
                    Console.WriteLine(pTaskParser.Error);
                }

                pTaskParser.Dispose();

                #region 发送/保存消息
                /*TaskService pMessageID = pTaskParser.SocketMessage;

                MSSQLOperate pMessageADO = new MSSQLOperate(strMessage);

                if (pMessageADO.Connect(false))
                {
                    string strProcParams = "SP_SEND_INSTANTMESSAGE";
                    DbParameter[] paramCache = DataUtilities.GetParameters(strProcParams);

                    if (paramCache == null)
                    {
                        paramCache = new SqlParameter[]{
                                                   new SqlParameter("@strCommand",SqlDbType.VarChar, 50),
                                                   new SqlParameter("@strSession",SqlDbType.VarChar, 50)
                                               };
                        DataUtilities.SetParameters(strProcParams, paramCache);
                    }

                    paramCache[0].Value = pMessageID.ToString();
                    paramCache[1].Value = ((MessageEventArgs)pObject).Connection.Session;

                    pMessageADO.ExecuteQuery(false, strProcParams, paramCache);
                    pMessageADO.GetResult(RecordStyle.DATASET);
                    DataSet pMessageData = (DataSet)pMessageADO.RecordData;

                    if (pMessageADO.AffectRow > 0)
                    {
                        for (int i = 0; i < pMessageData.Tables[0].Rows.Count; i++)
                        {
                            if (pMessageData.Tables[0].Rows[i][1].ToString() != "")
                            {
                                //离线消息
                                if (pMessageData.Tables[0].Rows[i][3].ToString() == "N/A")
                                {
                                    string strInput = @"INSERT INTO Message_Content(Content_Message, Content_Sender, Content_Recive, Content_Desc)
                                        VALUES(" + pMessageData.Tables[0].Rows[i][0].ToString() + "," + pMessageData.Tables[0].Rows[i][1].ToString()
                                                    + "," + pMessageData.Tables[0].Rows[i][2].ToString();

                                    pMessageADO.ExecuteQuery(strInput);
                                    pMessageADO.GetResult(RecordStyle.NONE);
                                }
                                //及时消息
                                else
                                {                                    
                                    CustomDataCollection mSendContent = new CustomDataCollection(StructType.CUSTOMDATA);

                                    mSendContent.Add("用户：" + pMessageData.Tables[0].Rows[i][4].ToString() + "触发了命令：" + pMessageData.Tables[0].Rows[i][5].ToString() + "内容为：");

                                    SocketPacket pSourcePacket = new SocketPacket(pMessageID, mSendContent);
                                    byte[] pSendBuffer = pSourcePacket.CoalitionInfo();

                                    //byte[] pSendBuffer = mSocketDate.setSocketData(CEnum.ServiceKey.INSTANT_CONTENT_RECIVE, CEnum.Msg_Category.INSTANT, mSendContent).bMsgBuffer;

                                    //((MessageEventArgs)pObject).SocketConnection.OnSend(pMessageData.Tables[0].Rows[i][3].ToString(), pSendBuffer);
                                }
                            }
                        }
                    }
                    pMessageADO.DisConnected();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format("      DB Connect Error！"));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();
                }*/
                #endregion

                ((MessageEventArgs)pObject).Connection.OnRecive();               

                //Print Message
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("      Total Times : {0}", DateTime.Now.Subtract(tStartTime)));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }
        }

        static string GetDBConfig(string strSection)
        {
            string strResult = String.Empty;

            string strPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            IniFile mIniFile = new IniFile(strPath + @"\Scheme\Scheme.INI");

            string strVerify = mIniFile.ReadValue(strSection, "Verify", "WINDOWS");
            string strAddress = mIniFile.ReadValue(strSection, "Address", "127.0.0.1");
            string strDBPort = mIniFile.ReadValue(strSection, "Port", "1433");
            string strDBName = mIniFile.ReadValue(strSection, "DBName", "Analyse");
            string strDBUser = mIniFile.ReadValue(strSection, "DBUser", "Analyse");
            string strDBPwd = mIniFile.ReadValue(strSection, "DBPwd", "");

            //设置数据库连接字符串
            switch (strVerify)
            {
                case "WINDOWS":
                    strResult = "Connect Timeout=3000;Integrated Security=SSPI;Persist Security Info=False;Data Source=127.0.0.1;Initial Catalog=" + strDBName;
                    break;
                case "SQLSERVER":
                    strResult = "Connect Timeout=3000;Server=" + strAddress + "," + strDBPort + ";Database=" + strDBName + ";uid=" + strDBUser + "; pwd=" + strDBPwd + ";";
                    break;
            }

            return strResult;
        }

        static string GetConnect()
        {
            string strPath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            IniFile GameIniFile = new IniFile(strPath + @"\Scheme\Connect.INI");

            //片断[GAME]
            string _strGame = GameIniFile.ReadValue("GAME", "GAMEID", "1");

            return _strGame;
        }
        #endregion

        #region 变量定义
        static string strAnalyse = GetDBConfig("ANALYSE");
        static string strOnline = GetDBConfig("ONLINENUM");
        static string strAccess = GetDBConfig("REPORT");
        static string strMessage = GetDBConfig("ANALYSE");

        static NotesSessionClass pNotesSession = new NotesSessionClass();
        static System.Timers.Timer pRealTime;
        static AsyncService pServer;
        static System.Timers.Timer pConnectTime;
        //static System.Timers.Timer pSendAuLog;
        #endregion

    }
}
