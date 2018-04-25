using System;
using System.Collections.Generic;
using System.Text;

using System.Global;

namespace System.Define
{
    /// <summary>
    /// 定义转换工具
    /// </summary>
    public static class DefineUtilities
    {
        public static DataField ToDataField(string strFieldName)
        {
            DataField mDataField = DataField.MESSAGE;

            switch (strFieldName)
            {
                case "id":
                case "Group_ID":
                case "Member_ID":
                case "Verify_NewID":
                case "Module_ID":
                case "Popedom_ID":
                case "Game_ID":
                case "Server_ID":
                case "Sort_ID":
                case "Criterion_ID":
                case "Report_ID":
                case "Condition_ID":
                case "Debug_ID":
                case "Template_ID":
                case "Item_ID":
                case "Content_ID":
                case "Access_ID":
                case "Relay_ID":
                case "Result_ID":
                case "Failure_ID":
                case "Handling_ID":
                case "Message_ID":
                case "Log_ID":
                case "Notes_ID":
                case "Sender_ID":
                case "Order_Field_ID"://--------------新增
                case "Competence_Order_ID"://--------------新增
                case "HS_ID":
                case "Info_ID":
                case "Leach_ID":
                    mDataField = DataField.Record_ID;
                    break;
                case "Verify_ID":
                    mDataField = DataField.Verify_ID;
                    break;
                case "Member_Group":
                case "Verify_Member":
                case "Popedom_Verify":
                case "Server_Game":
                case "Criterion_Sort":
                case "Report_Sort":
                case "Condition_Kind":
                case "Error_Server":
                case "Item_Game":
                case "Content_Item":
                case "Proplem_Content":
                case "Relay_Error":
                case "Access_Server":
                case "Result_Failure":
                case "Relay_Failure":
                case "Message_Command":
                case "Content_Message":
                case "Log_Verify":
                case "Sender_Template":
                case "Notes_Group"://-----------新增
                case "HS_Server":
                    mDataField = DataField.Record_Parent;
                    break;
                case "Module_Sort":
                case "Server_Layer":
                case "Criterion_Server":
                case "Report_Server":
                case "Condition_Server":
                case "Proplem_Server":
                case "Relay_Layer":
                case "Template_Layer":
                    mDataField = DataField.Record_Layer;
                    break;
                case "Record_Sort":
                case "Verify_Sort":
                case "Server_Sort":
                case "Popedom_Sort":
                case "Server_Parent":
                case "Condition_Sort":
                case "Result_Post":
                case "Template_Category":
                case "Message_Recive":
                case "Notes_Category":
                    mDataField = DataField.Record_Sort;
                    break;
                case "Sort_Kind":
                case "Server_Kind":
                    mDataField = DataField.Record_Kind;
                    break;
                case "Group_Desc":
                case "Member_Desc":
                case "Module_Desc":
                case "Popedom_Desc":
                case "Game_Desc":
                case "Server_Desc":
                case "Item_Desc":
                case "Proplem_Desc":
                case "Relay_Desc":
                case "Access_Desc":
                case "Message_Desc":
                case "Handling_Desc":
                case "Content_Desc":
                case "Log_Content":
                case "Sender_Desc":
                    mDataField = DataField.Record_Desc;
                    break;
                case "Access_Sort":
                    mDataField = DataField.ACCESS_SORT;
                    break;
                case "Module_Name":
                    mDataField = DataField.Module_Name;
                    break;
                case "Module_Class":
                    mDataField = DataField.Module_Class;
                    break;
                case "Module_Version":
                    mDataField = DataField.Module_Version;
                    break;
                case "Group_Name":
                    mDataField = DataField.Group_Name;
                    break;
                case "Member_Name":
                    mDataField = DataField.Member_Name;
                    break;
                case "Member_Addr":
                    mDataField = DataField.Member_Addr;
                    break;
                case "Member_Tel":
                    mDataField = DataField.Member_Tel;
                    break;
                case "Member_Mobile":
                    mDataField = DataField.Member_Mobile;
                    break;
                case "Member_IM":
                    mDataField = DataField.Member_IM;
                    break;
                case "Verify_Nick":
                    mDataField = DataField.Verify_Nick;
                    break;
                case "Verify_Sign":
                    mDataField = DataField.Verify_Sign;
                    break;
                case "Verify_Pwd":
                    mDataField = DataField.Verify_Pwd;
                    break;
                case "Verify_Term":
                    mDataField = DataField.Verify_Term;
                    break;
                case "Verify_Status":
                    mDataField = DataField.Verify_Status;
                    break;
                case "Popedom_Tag":
                    mDataField = DataField.Verify_Module;
                    break;
                case "Verify_Group"://---------------------------新增
                    mDataField = DataField.Verify_Group;
                    break;
                case "Verify_Desc"://---------------------------新增
                    mDataField = DataField.Verify_Desc;
                    break;
                case "Game_Name":
                    mDataField = DataField.Game_Name;
                    break;
                case "Game_Manager":
                    mDataField = DataField.Game_Manager;
                    break;
                case "Game_DB":
                    mDataField = DataField.Game_DB;
                    break;
                case "Game_Port":
                    mDataField = DataField.Game_Port;
                    break;
                case "Server_Name":
                    mDataField = DataField.Server_Name;
                    break;
                case "Server_Intranet":
                    mDataField = DataField.Server_Intranet;
                    break;
                case "Server_Internet":
                    mDataField = DataField.Server_Internet;
                    break;
                case "Server_Status":
                    mDataField = DataField.Server_Status;
                    break;
                case "Sort_Name":
                    mDataField = DataField.Sort_Name;
                    break;
                case "Sort_Key":
                    mDataField = DataField.Sort_Key;
                    break;
                case "Sort_Analyse":
                    mDataField = DataField.Sort_Analyse;
                    break;
                case "Sort_Backup":
                    mDataField = DataField.Sort_Backup;
                    break;
                case "Sort_View":
                    mDataField = DataField.Sort_View;
                    break;
                case "Criterion_Normal":
                    mDataField = DataField.Criterion_Normal;
                    break;
                case "Criterion_Warning":
                    mDataField = DataField.Criterion_Warning;
                    break;
                case "Criterion_Graveness":
                    mDataField = DataField.Criterion_Graveness;
                    break;
                case "Report_Table":
                    mDataField = DataField.Report_Table;
                    break;
                case "Report_Write":
                    mDataField = DataField.Report_Write;
                    break;
                case "Report_Source":
                    mDataField = DataField.Report_Source;
                    break;
                case "Report_Date":
                    mDataField = DataField.Report_Date;
                    break;
                case "Report_User":
                    mDataField = DataField.Report_User;
                    break;
                case "Report_Value":
                    mDataField = DataField.Report_Value;
                    break;
                case "Report_Content":
                    mDataField = DataField.Report_Content;
                    break;
                case "Report_View":
                    mDataField = DataField.Report_View;
                    break;
                case "Report_Level":
                    mDataField = DataField.Report_Level;
                    break;
                case "Criterion_Desc":
                    mDataField = DataField.Criterion_Desc;
                    break;
                case "Notify_Sort":
                    mDataField = DataField.Notify_Sort;
                    break;
                case "Notify_Count":
                    mDataField = DataField.Notify_Count;
                    break;
                case "Disposal_Member":
                    mDataField = DataField.Report_Member;
                    break;
                case "Disposal_Write":
                    mDataField = DataField.Report_Write;
                    break;
                case "Disposal_Report":
                    mDataField = DataField.Report_Key;
                    break;
                case "Disposal_Content":
                    mDataField = DataField.Report_Disposal;
                    break;
                case "Condition_Name":
                    mDataField = DataField.Condition_Name;
                    break;
                case "Condition_Tag":
                    mDataField = DataField.Condition_Tag;
                    break;
                case "Condition_Desc":
                    mDataField = DataField.Condition_Desc;
                    break;
                case "descs":
                    mDataField = DataField.ONLINE_DESC;
                    break;
                case "infodate":
                    mDataField = DataField.ONLINE_TIMES;
                    break;
                case "nums":
                    mDataField = DataField.ONLINE_NUM;
                    break;
                case "set_id":
                    mDataField = DataField.ONLINE_KIND;
                    break;
                case "gamename":
                    mDataField = DataField.ONLINE_GAME;
                    break;
                case "tablename":
                    mDataField = DataField.ONLINE_TABLE;
                    break;
                case "daynum":
                    mDataField = DataField.ONLINE_DAYNUM;
                    break;
                case "hisnum":
                    mDataField = DataField.ONLINE_HISNUM;
                    break;
                case "type":
                    mDataField = DataField.ONLINE_TYPE;
                    break;
                case "dbname":
                    mDataField = DataField.ONLINE_DBNAME;
                    break;
                case "username":
                    mDataField = DataField.ONLINE_USER;
                    break;
                case "password":
                    mDataField = DataField.ONLINE_PWD;
                    break;
                case "strsql":
                    mDataField = DataField.ONLINE_SQL;
                    break;
                case "rscount":
                    mDataField = DataField.ONLINE_MAXNUM;
                    break;
                case "totb":
                    mDataField = DataField.ONLINE_TOTB;
                    break;
                case "ipaddress":
                    mDataField = DataField.ONLINE_IPADDR;
                    break;
                case "gamezonename":
                    mDataField = DataField.ONLINE_SERVER;
                    break;
                case "numstable":
                    mDataField = DataField.ONLINE_NUMSTABLE;
                    break;
                case "baktable":
                    mDataField = DataField.ONLINE_BAKTABLE;
                    break;
                case "status":
                    mDataField = DataField.ONLINE_STATE;
                    break;
                case "ipstatus":
                    mDataField = DataField.ONLINE_IPSTATE;
                    break;
                case "Debug_Title":
                    mDataField = DataField.DEBUG_TITLE;
                    break;
                case "Debug_Module":
                    mDataField = DataField.DEBUG_MODULE;
                    break;
                case "Debug_Content":
                    mDataField = DataField.DEBUG_CONTENT;
                    break;
                case "Debug_Post":
                    mDataField = DataField.DEBUG_POST;
                    break;
                case "Debug_Disposal":
                    mDataField = DataField.DEBUG_DISPOSAL;
                    break;
                case "Debug_Status":
                    mDataField = DataField.DEBUG_STATUS;
                    break;
                case "Template_Title":
                    mDataField = DataField.TEMPLATE_TITLE;
                    break;
                case "Template_Desc":
                    mDataField = DataField.TEMPLATE_DESC;
                    break;
                case "Server_Position":
                    mDataField = DataField.Server_Position;
                    break;
                case "Server_Configuration":
                    mDataField = DataField.Server_Configuration;
                    break;
                case "Server_Tel":
                    mDataField = DataField.Server_Tel;
                    break;
                case "Server_House":
                    mDataField = DataField.Server_House;
                    break;
                case "Access_User":
                    mDataField = DataField.ACCESS_USER;
                    break;
                case "Access_Pwd":
                    mDataField = DataField.ACCESS_PWD;
                    break;
                case "Failure_Area":
                    mDataField = DataField.CRASH_AREA;
                    break;
                case "Failure_Template":
                    mDataField = DataField.TEMPLATE_TITLE;
                    break;
                case "Failure_Post":
                    mDataField = DataField.CRASH_POST;
                    break;
                case "Relay_Recive":
                    mDataField = DataField.CRASH_RECIVE;
                    break;
                case "Failure_Time":
                    mDataField = DataField.CRASH_BEGIN;
                    break;
                case "Result_Time":
                    mDataField = DataField.CRASH_END;
                    break;
                case "Failure_Desc":
                    mDataField = DataField.CRASH_ERROR;
                    break;
                case "Result_Desc":
                    mDataField = DataField.CRASH_RESULT;
                    break;
                case "Relay_Message":
                    mDataField = DataField.CRASH_MESSAGE;
                    break;
                case "Failure_Status":
                case "Relay_Status":
                    mDataField = DataField.CRASH_STATUS;
                    break;
                case "Relay_Date":
                    mDataField = DataField.CRASH_RELAYDATE;
                    break;
                case "Handling_Name":
                    mDataField = DataField.HANDLING_NAME;
                    break;
                case "Handling_Area":
                    mDataField = DataField.HANDLING_AREA;
                    break;
                case "Handling_Post":
                    mDataField = DataField.HANDLING_POST;
                    break;
                case "Handling_Date":
                    mDataField = DataField.HANDLING_DATE;
                    break;
                case "Handling_Flow":
                    mDataField = DataField.HANDLING_FLOW;
                    break;
                case "Content_Proplem":
                    mDataField = DataField.HANDLING_PROPLEM;
                    break;
                case "Handling_Status":
                case "Content_Status":
                    mDataField = DataField.HANDLING_STATUS;
                    break;
                case "Content_Date":
                    mDataField = DataField.CONTENT_DATE;
                    break;
                case "Content_Sender":
                    mDataField = DataField.CONTENT_SENDER;
                    break;
                case "Template_Parent":
                    mDataField = DataField.TEMPLATE_PARENT;
                    break;
                case "Log_Date":
                    mDataField = DataField.LOG_DATE;
                    break;
                case "Log_Game":
                    mDataField = DataField.LOG_GAME;
                    break;
                case "Log_Area":
                    mDataField = DataField.LOG_AREA;
                    break;
                case "Log_Channel":
                    mDataField = DataField.LOG_CHANNEL;
                    break;
                case "Log_Session":
                    mDataField = DataField.LOG_SESSION;
                    break;
                case "Log_Action":
                    mDataField = DataField.LOG_ACTION;
                    break;
                case "Log_Source":
                    mDataField = DataField.LOG_SOURCE;
                    break;
                case "Log_Status":
                    mDataField = DataField.LOG_STATUS;
                    break;
                case "Linker_Name":
                case "Sender_Name":
                    mDataField = DataField.LINKER_NAME;
                    break;
                case "Sender_Mobile":
                    mDataField = DataField.LINKER_MOBILE;
                    break;
                case "Sender_Content":
                case "Linker_Content":
                    mDataField = DataField.LINKER_CONTENT;
                    break;
                case "Notes_UID":
                    mDataField = DataField.NOTES_UID;
                    break;
                case "Notes_Subject":
                    mDataField = DataField.NOTES_SUBJECT;
                    break;
                case "Notes_From":
                    mDataField = DataField.NOTES_FROM;
                    break;
                case "Notes_Date":
                    mDataField = DataField.NOTES_DATE;
                    break;
                case "Notes_Supervisors":
                    mDataField = DataField.NOTES_SUPERVISORS;
                    break;
                case "Notes_Content":
                    mDataField = DataField.NOTES_CONTENT;
                    break;
                case "Notes_Attachment":
                    mDataField = DataField.NOTES_ATTACHMENT;
                    break;
                case "Notes_AttachmentCount":
                    mDataField = DataField.NOTES_ATTACHMENTCOUNT;
                    break;
                case "Notes_AttachmentName":
                    mDataField = DataField.NOTES_ATTACHMENTNAME;
                    break;

                case "User1":
                    mDataField = DataField.User1;
                    break;
                case "User2":
                    mDataField = DataField.User2;
                    break;
                case "User3":
                    mDataField = DataField.User3;
                    break;
                case "User4":
                    mDataField = DataField.User4;
                    break;
                case "User5":
                    mDataField = DataField.User5;
                    break;
                case "User6":
                    mDataField = DataField.User6;
                    break;
                case "User7":
                    mDataField = DataField.User7;
                    break;
                case "User8":
                    mDataField = DataField.User8;
                    break;
                case "User9":
                    mDataField = DataField.User9;
                    break;
                case "User10":
                    mDataField = DataField.User10;
                    break;
                case "User11":
                    mDataField = DataField.User11;
                    break;
                case "User12":
                    mDataField = DataField.User12;
                    break;
                case "User13":
                    mDataField = DataField.User13;
                    break;
                case "User14":
                    mDataField = DataField.User14;
                    break;
                case "User15":
                    mDataField = DataField.User15;
                    break;
                case "Pwd1":
                    mDataField = DataField.Pwd1;
                    break;
                case "Pwd2":
                    mDataField = DataField.Pwd2;
                    break;
                case "Pwd3":
                    mDataField = DataField.Pwd3;
                    break;
                case "Pwd4":
                    mDataField = DataField.Pwd4;
                    break;
                case "Pwd5":
                    mDataField = DataField.Pwd5;
                    break;
                case "Pwd6":
                    mDataField = DataField.Pwd6;
                    break;
                case "Pwd7":
                    mDataField = DataField.Pwd7;
                    break;
                case "Pwd8":
                    mDataField = DataField.Pwd8;
                    break;
                case "Pwd9":
                    mDataField = DataField.Pwd9;
                    break;
                case "Pwd10":
                    mDataField = DataField.Pwd10;
                    break;
                case "Pwd11":
                    mDataField = DataField.Pwd11;
                    break;
                case "Pwd12":
                    mDataField = DataField.Pwd12;
                    break;
                case "Pwd13":
                    mDataField = DataField.Pwd13;
                    break;
                case "Pwd14":
                    mDataField = DataField.Pwd14;
                    break;
                case "Pwd15":
                    mDataField = DataField.Pwd15;
                    break;
                case "Relay_Notes":
                    mDataField = DataField.Relay_Notes;
                    break;
                case "Relay_Post":
                    mDataField = DataField.Relay_Post;
                    break;
                case "Result_Content":
                    mDataField = DataField.Result_Content;
                    break;
                case "Result_Write":
                    mDataField = DataField.Result_Write;
                    break;
                case "Relay_ReciveName":
                    mDataField = DataField.Relay_ReciveName;
                    break;
                case "Server_DBtype":
                    mDataField = DataField.Server_DBtype;
                    break;

                //----------------- 新增
                case "Common_Order_GroupID":
                    mDataField = DataField.Common_Group_ID;
                    break;
                case "Common_Order_VerifyID":
                    mDataField = DataField.Common_Verify_ID;
                    break;
                case "Order_Field":
                    mDataField = DataField.ORDER_FIELD;
                    break;
                case "Order_Desc":
                    mDataField = DataField.ORDER_DESC;
                    break;
                case "Competence_Order_VerifyID":
                    mDataField = DataField.Competence_Order_VerifyID;
                    break;
                case "Competence_Order_GroupID":
                    mDataField = DataField.Competence_Order_GroupID;
                    break;
                case "Competence_Order_Template":
                    mDataField = DataField.Competence_Order_Template;
                    break;
                case "Competence_Order_Name":
                    mDataField = DataField.Competence_Order_Name;
                    break;
                case "Competence_Order_Field":
                    mDataField = DataField.Competence_Order_Field;
                    break;
                case "Competence_Order_Key":
                    mDataField = DataField.Competence_Order_Key;
                    break;
                case "Competence_Order_Type":
                    mDataField = DataField.Competence_Order_Type;
                    break;
                case "Competence_Order_Flag":
                    mDataField = DataField.Competence_Order_Flag;
                    break;
                case "Competence_Order_Desc":
                    mDataField = DataField.Competence_Order_Desc;
                    break;
                case "Competence_Order_Name_Desc":
                    mDataField = DataField.Competence_Order_Name_Desc;
                    break;
                case "Competence_Order_Template_Desc":
                    mDataField = DataField.Competence_Order_Template_Desc;
                    break;
                case "Competence_Order_Field_Desc":
                    mDataField = DataField.Competence_Order_Field_Desc;
                    break;
                case "HS_Date":
                    mDataField = DataField.HS_DATE;
                    break;
                case "HS_Demand":
                    mDataField = DataField.HS_DEMAND;
                    break;
                case "HS_Result":
                    mDataField = DataField.HS_RESULT;
                    break;
                case "HS_Status":
                    mDataField = DataField.HS_STATUS;
                    break;
                case "HS_Alliance":
                    mDataField = DataField.HS_ALLIANCE;
                    break;
                case "HS_EndDate":
                    mDataField = DataField.HS_ENDDATE;
                    break;
                case "HS_StartTime":
                    mDataField = DataField.HS_STARTTIME;
                    break;
                case "HS_EndTime":
                    mDataField = DataField.HS_ENDTIME;
                    break;
                //-----------------
                case "Layer_Count":
                    mDataField = DataField.Layer_Count;
                    break;
                case "Server_Count":
                    mDataField = DataField.Server_Count;
                    break;
                //-----------------SQL PLUS            
                case "DB_Name":
                    mDataField = DataField.DB_Name;
                    break;
                case "DB_Game_ID":
                    mDataField = DataField.DB_Game_ID;
                    break;
                case "DB_Layer":
                    mDataField = DataField.DB_Layer;
                    break;
                case "DB_Game_Name":
                    mDataField = DataField.DB_Game_Name;
                    break;
                case "DB_Type":
                    mDataField = DataField.DB_Type;
                    break;
                case "DB_Internet":
                    mDataField = DataField.DB_Internet;
                    break;
                case "DB_Popedom":
                    mDataField = DataField.DB_Popedom;
                    break;
                case "DB_User":
                    mDataField = DataField.DB_User;
                    break;
                case "DB_Pwd":
                    mDataField = DataField.DB_Pwd;
                    break;

                case "NAMES_Layer":
                    mDataField = DataField.NAMES_Layer;
                    break;
                case "NAME_KIND":
                    mDataField = DataField.NAME_KIND;
                    break;
                case "NAME_ID":
                    mDataField = DataField.NAME_ID;
                    break;
                case "CONSTRAINT_TYPE":
                    mDataField = DataField.CONSTRAINT_TYPE;
                    break;
                case "ORDINAL_POSITION":
                    mDataField = DataField.ORDINAL_POSITION;
                    break;
                case "COLUMN_DEFAULT":
                    mDataField = DataField.COLUMN_DEFAULT;
                    break;
                case "IS_NULLABLE":
                    mDataField = DataField.IS_NULLABLE;
                    break;
                case "DATA_TYPE":
                    mDataField = DataField.DATA_TYPE;
                    break;
                case "CHARACTER_MAXIMUM_LENGTH":
                    mDataField = DataField.CHARACTER_MAXIMUM_LENGTH;
                    break;
                case "CHARACTER_OCTET_LENGTH":
                    mDataField = DataField.CHARACTER_OCTET_LENGTH;
                    break;
                case "NUMERIC_PRECISION":
                    mDataField = DataField.NUMERIC_PRECISION;
                    break;
                case "NUMERIC_PRECISION_RADIX":
                    mDataField = DataField.NUMERIC_PRECISION_RADIX;
                    break;
                case "NUMERIC_SCALE":
                    mDataField = DataField.NUMERIC_SCALE;
                    break;
                case "DATETIME_PRECISION":
                    mDataField = DataField.DATETIME_PRECISION;
                    break;
                case "COLLATION_NAME":
                    mDataField = DataField.COLLATION_NAME;
                    break;
                case "CREATE_DATA":
                    mDataField = DataField.CREATE_DATA;
                    break;
                case "ALTER_DATA":
                    mDataField = DataField.ALTER_DATA;
                    break;

                case "IS_IDENTITY":
                    mDataField = DataField.IS_IDENTITY;
                    break;
                case "SEED_VALUE":
                    mDataField = DataField.SEED_VALUE;
                    break;
                case "INCREMENT_VALUE":
                    mDataField = DataField.INCREMENT_VALUE;
                    break;
                case "COLUMN_NAME":
                    mDataField = DataField.COLUMN_NAME;
                    break;
                case "DEFAULT_VALUE":
                    mDataField = DataField.DEFAULT_VALUE;
                    break;
                case "DBTYPE_NAME":
                    mDataField = DataField.DBTYPE_NAME;
                    break;

                case "EXTEND_VALUE":
                    mDataField = DataField.EXTEND_VALUE;
                    break;

                case "Leach_Game":
                    mDataField = DataField.Leach_Game;
                    break;
                case "Leach_Log":
                    mDataField = DataField.Leach_Log;
                    break;
                case "Leach_Data":
                    mDataField = DataField.Leach_Data;
                    break;
                //----------------AU
                case "UserSN":
                    mDataField = DataField.UserSN;
                    break;
                case "Exp":
                    mDataField = DataField.Exp;
                    break;
                case "Level":
                    mDataField = DataField.Level;
                    break;
                case "RealLevel":
                    mDataField = DataField.RealLevel;
                    break;
                case "UserGender":
                    mDataField = DataField.UserGender;
                    break;
                case "Item_Name":
                    mDataField = DataField.Item_Name;
                    break;
                case "Item_Price":
                    mDataField = DataField.Item_Price;
                    break;
                case "Item_Sex":
                    mDataField = DataField.Item_Sex;
                    break;
                case "MaleSN":
                    mDataField = DataField.MaleSN;
                    break;
                case "FemaleSN":
                    mDataField = DataField.FemaleSN;
                    break;
                case "CoupleDate":
                    mDataField = DataField.CoupleDate;
                    break;
                case "SeparateDate":
                    mDataField = DataField.SeparateDate;
                    break;
                case "SeparateSN":
                    mDataField = DataField.SeparateSN;
                    break;
                case "DivorceSN":
                    mDataField = DataField.DivorceSN;
                    break;
                case "WeddingDate":
                    mDataField = DataField.WeddingDate;
                    break;
                case "DivorceDate":
                    mDataField = DataField.DivorceDate;
                    break;
                case "UserNick":
                    mDataField = DataField.UserNick;
                    break;
                case "ExpiredDate":
                    mDataField = DataField.ExpiredDate;
                    break;

                default:
                    mDataField = DataField.MESSAGE;                   
                    break;
            }

            return mDataField;
        }

        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param name="strFieldType"></param>
        /// <returns></returns>
        public static DataFormat ToDataFormat(string strFieldType)
        {
            switch (strFieldType)
            {
                case "Byte[]":
                    return DataFormat.NONE;
                case "Boolean":
                    return DataFormat.BOOLEAN;
                case "DateTime":
                    return DataFormat.DATETIME;
                case "SByte":
                case "Byte":
                    return DataFormat.BYTE;
                case "Guid":
                    return DataFormat.GUID;
                case "String":
                    return DataFormat.STRING;
                case "Decimal":
                    return DataFormat.DECIMAL;
                case "Single":
                case "Double":
                    return DataFormat.PRECISION;
                case "Int16":
                case "Int32":
                case "Int64":
                    return DataFormat.SIGNED;
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return DataFormat.UNSIGNED;
                case "Object":
                    return DataFormat.VARIANT;
                default:
                    return DataFormat.NONE;
            }
        }

        /// <summary>
        /// 任务类别转换
        /// </summary>
        /// <param name="eService"></param>
        /// <returns></returns>
        public static TaskCategory ToTaskCategory(TaskService eService)
        {
            return (TaskCategory)((uint)eService >> 24);
        }
    }
}
