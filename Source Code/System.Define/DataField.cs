using System;
using System.Collections.Generic;
using System.Text;

namespace System.Define
{
    /// <summary>
    /// �����ֶ�
    /// </summary>
    public enum DataField : uint
    {
        #region ϵͳĬ��
        //////////////////////////////���� �� �� Ϣ��//////////////////////////////
        Record_ID = 0x00000001,       //PK
        Record_Parent = 0x00000002,       //FPK
        Record_Layer = 0x00000003,       //FK
        Record_Sort = 0x00000004,       //Sign OR FPK
        Record_Kind = 0x00000005,
        Record_Desc = 0x00000006,       //Description

        //////////////////////////////��ģ �� �� Ϣ��//////////////////////////////
        Module_Name = 0x02000001,
        Module_Class = 0x02000002,
        Module_Version = 0x02000003,
        ////////////////////////////���� �� �� �� Ϣ��/////////////////////////////
        Group_Name = 0x03000001,
        //////////////////////////////���� Ա �� Ϣ��//////////////////////////////
        Member_Name = 0x04000001,
        Member_Addr = 0x04000002,
        Member_Tel = 0x04000003,
        Member_Mobile = 0x04000004,
        Member_IM = 0x04000005,
        //////////////////////////////���� ֤ �� Ϣ��//////////////////////////////        
        Verify_Nick = 0x05000001,
        Verify_Sign = 0x05000002,
        Verify_Pwd = 0x05000003,
        Verify_Term = 0x05000004,
        Verify_Status = 0x05000005,
        Verify_Module = 0x05000006,
        Verify_Group = 0x05000007,  //������,�û�������id
        Verify_Desc = 0x05000008,  //������,�û��ʺ�����
        Verify_ID = 0x05000009,
        //////////////////////////////��֧ �� �� Ϣ��//////////////////////////////
        Game_Name = 0x06000001,
        Game_Manager = 0x06000002,
        Server_Name = 0x06000003,
        Server_Intranet = 0x06000004,
        Server_Internet = 0x06000005,
        Sort_Name = 0x06000006,
        Sort_Key = 0x06000007,
        Sort_Analyse = 0x06000008,
        Sort_Backup = 0x06000009,
        Sort_View = 0x0600000A,
        Criterion_Normal = 0x0600000B,
        Criterion_Warning = 0x0600000C,
        Criterion_Graveness = 0x0600000D,
        Criterion_Desc = 0x0600000E,
        Game_DB = 0x0600000F,
        Game_Port = 0x06000010,
        Server_Position = 0x06000011,
        Server_Configuration = 0x06000012,
        Server_Tel = 0x06000013,
        Server_House = 0x06000014,
        Server_Status = 0x06000015,
        //--------------------------����
        Layer_Count = 0x06000016,//��Ϸ������
        Server_Count = 0x06000017,//��Ϸ��������
        Server_DBtype = 0x06000018,
        //--------------------------
        //////////////////////////////���� �� �� Ϣ��//////////////////////////////
        Report_Table = 0x07000001,
        Report_Write = 0x07000002,
        Report_Source = 0x07000003,
        Report_Date = 0x07000004,
        Report_User = 0x07000005,
        Report_Value = 0x07000006,
        Report_Content = 0x07000007,
        Report_View = 0x07000008,
        Report_Member = 0x07000009,
        Report_Key = 0x0700000A,
        Report_Disposal = 0x0700000B,
        Report_Level = 0x0700000C,
        Report_Condition = 0x0700000D,
        Report_Get = 0x0700000E,
        Report_Result = 0x0700000F,
        Notify_Sort = 0x07000010,
        Notify_Count = 0x07000011,
        Leach_Game = 0x07000012,
        Leach_Log = 0x07000013,
        Leach_Data = 0x07000014,

        //////////////////////////////���� ־ �� Ϣ��//////////////////////////////
        Log_Remark01 = 0x08000001,
        Log_Remark02 = 0x08000002,
        Log_Remark03 = 0x08000003,
        Log_Remark04 = 0x08000004,
        Log_Remark05 = 0x08000005,
        Log_Remark06 = 0x08000006,
        Log_Remark07 = 0x08000007,
        Log_Remark08 = 0x08000008,
        Log_Remark09 = 0x08000009,
        Log_Remark10 = 0x0800000A,
        Log_Remark11 = 0x0800000B,
        Log_Remark12 = 0x0800000C,
        Log_Remark13 = 0x0800000D,
        Log_Remark14 = 0x0800000E,
        Log_Remark15 = 0x0800000F,
        Log_Remark16 = 0x08000010,
        Log_Remark17 = 0x08000011,
        Log_Remark18 = 0x08000012,
        Log_Remark19 = 0x08000013,
        Log_Remark20 = 0x08000014,
        Log_Remark21 = 0x08000015,
        Log_Remark22 = 0x08000016,
        Log_Remark23 = 0x08000017,
        Log_Remark24 = 0x08000018,
        Log_Remark25 = 0x08000019,
        Log_Remark26 = 0x0800001A,
        Log_Remark27 = 0x0800001B,
        Log_Remark28 = 0x0800001C,
        Log_Remark29 = 0x0800001D,
        Log_Remark30 = 0x0800001E,
        Log_Remark31 = 0x0800001F,
        Log_Remark32 = 0x08000020,
        Log_Remark33 = 0x08000021,
        Log_Remark34 = 0x08000022,
        Log_Remark35 = 0x08000023,
        Log_Remark36 = 0x08000024,
        Log_Remark37 = 0x08000025,
        Log_Remark38 = 0x08000026,
        Log_Remark39 = 0x08000027,
        Log_Remark40 = 0x08000028,
        Log_Remark41 = 0x08000029,
        Log_Remark42 = 0x0800002A,
        Log_Remark43 = 0x0800002B,
        Log_Remark44 = 0x0800002C,
        Log_Remark45 = 0x0800002D,
        Log_Remark46 = 0x0800002E,
        Log_Remark47 = 0x0800002F,
        Log_Remark48 = 0x08000030,
        Log_Remark49 = 0x08000031,
        Log_Remark50 = 0x08000032,
        Log_Remark51 = 0x08000033,
        Log_Remark52 = 0x08000034,
        Log_Remark53 = 0x08000035,
        Log_Remark54 = 0x08000036,
        Log_Remark55 = 0x08000037,
        Log_Remark56 = 0x08000038,
        Log_Remark57 = 0x08000039,
        Log_Remark58 = 0x0800003A,
        Log_Remark59 = 0x0800003B,
        Log_Remark60 = 0x0800003C,
        Log_Remark61 = 0x0800003D,
        Log_Remark62 = 0x0800003E,
        Log_Remark63 = 0x0800003F,

        //////////////////////////////���� ־ �� Ϣ��//////////////////////////////
        Condition_Name = 0x08000041,
        Condition_Tag = 0x08000042,
        Condition_Desc = 0x08000043,
        //////////////////////////////���� �� �� Ϣ��//////////////////////////////
        UPDATE_STATE = 0x08000051,
        UPDATE_NAME = 0x08000052,
        UPDATE_VERSION = 0x08000053,
        UPDATE_CONTENT = 0x08000054,
        //////////////////////////////���� �� �� Ϣ��//////////////////////////////
        ONLINE_SERVER = 0x09000001,
        ONLINE_KIND = 0x09000002,
        ONLINE_NUM = 0x09000003,
        ONLINE_TIMES = 0x09000004,
        ONLINE_DESC = 0x09000005,
        ONLINE_TABLE = 0x09000006,
        ONLINE_DAYNUM = 0x09000007,
        ONLINE_HISNUM = 0x09000008,
        ONLINE_TYPE = 0x09000009,
        ONLINE_DBNAME = 0x09000010,
        ONLINE_USER = 0x09000011,
        ONLINE_PWD = 0x09000012,
        ONLINE_SQL = 0x09000013,
        ONLINE_MAXNUM = 0x09000014,
        ONLINE_TOTB = 0x09000015,
        ONLINE_GAME = 0x09000016,
        ONLINE_IPADDR = 0x0900000A,
        ONLINE_NUMSTABLE = 0x0900000B,
        ONLINE_BAKTABLE = 0x0900000C,
        ONLINE_STATE = 0x0900000D,
        ONLINE_IPSTATE = 0x0900000E,
        ////////////////////////////��DEBUG�� �� �� Ϣ��///////////////////////////
        DEBUG_TITLE = 0x10000001,
        DEBUG_MODULE = 0x10000002,
        DEBUG_CONTENT = 0x10000003,
        DEBUG_POST = 0x10000004,
        DEBUG_DISPOSAL = 0x10000005,
        DEBUG_STATUS = 0x10000006,
        //////////////////////////��������ͳ���� �� Ϣ��/////////////////////////
        CRASH_AREA = 0x11000003,
        CRASH_POST = 0x11000004,
        CRASH_RECIVE = 0x11000005,
        CRASH_BEGIN = 0x11000006,
        CRASH_END = 0x11000007,
        CRASH_ERROR = 0x11000008,
        CRASH_RESULT = 0x11000009,
        CRASH_STATUS = 0x11000010,
        CRASH_MESSAGE = 0x11000011,
        CRASH_RELAYDATE = 0x11000012,
        CRASH_POSTNAME = 0x11000013,
        CRASH_RECIVENAME = 0x11000014,
        //////////////////////////////��ģ �� �� Ϣ��//////////////////////////////
        TEMPLATE_PARENT = 0x12000001,
        TEMPLATE_TITLE = 0x12000002,
        TEMPLATE_DESC = 0x12000003,
        ORDER_FIELD = 0x12000004,//---------����
        ORDER_DESC = 0x12000005,//---------����
        //////////////////////////////��ά �� �� Ϣ��//////////////////////////////
        HANDLING_DATE = 0x13000001,
        HANDLING_FLOW = 0x13000002,
        HANDLING_PROPLEM = 0x13000003,
        HANDLING_STATUS = 0x13000004,
        HANDLING_NAME = 0x13000005,
        HANDLING_AREA = 0x13000006,
        HANDLING_POST = 0x13000007,
        HS_DEMAND = 0x13000008,
        HS_RESULT = 0x13000009,
        HS_DATE = 0x13000010,
        HS_STATUS = 0x13000011,
        HS_ALLIANCE = 0x13000012,
        HS_ENDDATE = 0x13000013,
        HS_STARTTIME = 0x13000014,
        HS_ENDTIME = 0x13000015,
        //////////////////////////////���� ʱ �� Ϣ��//////////////////////////////
        CONTENT_SENDER = 0x14000001,
        CONTENT_DATE = 0x14000002,
        CONTENT_VIEW = 0x14000003,
        //////////////////////////////���� �� Ȩ �� �� Ϣ��//////////////////////////////
        ACCESS_USER = 0x14000001,
        ACCESS_PWD = 0x14000002,
        ACCESS_SORT = 0x14000003,
        //////////////////////////////���� ־ �� Ϣ��//////////////////////////////
        LOG_SESSION = 0x15000001,
        LOG_STATUS = 0x15000002,
        LOG_DATE = 0x15000003,
        LOG_GAME = 0x15000004,
        LOG_AREA = 0x15000005,
        LOG_CHANNEL = 0x15000006,
        LOG_ACTION = 0x15000007,
        LOG_SOURCE = 0x15000008,
        LOG_CONTENT = 0x15000009,
        //////////////////////////////��Notes �� ϵ �ˡ�///////////////////////////
        LINKER_NAME = 0x16000001,
        LINKER_CONTENT = 0x16000002,
        LINKER_MOBILE = 0x16000003,
        //////////////////////////////��Notes �� ����//////////////////////////////
        NOTES_UID = 0x17000001,
        NOTES_SUBJECT = 0x17000002,
        NOTES_FROM = 0x17000003,
        NOTES_DATE = 0x17000004,
        NOTES_SUPERVISORS = 0x17000005,
        NOTES_CONTENT = 0x17000006,
        NOTES_ATTACHMENT = 0x17000007,
        NOTES_ATTACHMENTNAME = 0x17000008,
        NOTES_ATTACHMENTCOUNT = 0x17000009,
        //////////////////////////////���� �� Ȩ �ޡ�//////////////////////////////
        Common_Group_ID = 0x18000001,
        Common_Verify_ID = 0x18000002,

        Competence_Order_ID = 0x18000003,
        Competence_Order_GroupID = 0x18000004,
        Competence_Order_VerifyID = 0x18000005,
        Competence_Order_Template = 0x18000006,
        Competence_Order_Name = 0x18000007,
        Competence_Order_Field = 0x18000008,
        Competence_Order_Key = 0x18000009,
        Competence_Order_Type = 0x1800000A,
        Competence_Order_Flag = 0x1800000B,
        Competence_Order_Desc = 0x1800000C,
        Competence_Order_Template_Desc = 0x1800000D,
        Competence_Order_Name_Desc = 0x1800000E,
        Competence_Order_Field_Desc = 0x1800000F,
        //////////////////////////////��SQL PLUS��//////////////////////////////
        DB_Game_ID = 0x19000001,
        DB_Layer = 0x19000002,
        DB_Game_Name = 0x19000003,
        DB_Type = 0x19000004,
        DB_Internet = 0x19000005,
        DB_Popedom = 0x19000006,
        DB_User = 0x19000007,
        DB_Pwd = 0x19000008,
        NAMES_Layer = 0x19000009,
        NAME_KIND = 0x19000010,
        NAME_ID = 0x19000011,
        CONSTRAINT_TYPE = 0x19000012,
        ORDINAL_POSITION = 0x19000013,
        COLUMN_DEFAULT = 0x19000014,
        IS_NULLABLE = 0x19000015,
        DATA_TYPE = 0x19000016,
        CHARACTER_MAXIMUM_LENGTH = 0x19000017,
        CHARACTER_OCTET_LENGTH = 0x19000018,
        NUMERIC_PRECISION = 0x19000019,
        NUMERIC_PRECISION_RADIX = 0x19000020,
        NUMERIC_SCALE = 0x19000021,
        DATETIME_PRECISION = 0x19000022,
        COLLATION_NAME = 0x19000023,
        CREATE_DATA = 0x19000024,
        ALTER_DATA = 0x19000025,
        DB_Name = 0x19000026,

        IS_IDENTITY = 0x19000027,
        SEED_VALUE = 0x19000028,
        INCREMENT_VALUE = 0x19000029,
        DEFAULT_VALUE = 0x19000030,
        COLUMN_NAME = 0x19000031,

        DBTYPE_NAME = 0x19000032,
        EXTEND_VALUE = 0x19000033,
        //////////////////////////////��AU �� Ϣ��//////////////////////////////
        UserSN = 0x30000001,
        Exp = 0x30000002,
        Level = 0x30000003,
        RealLevel = 0x30000004,
        UserGender = 0x30000005,
        //Item_ID = 0x2006,
        Item_Name = 0x30000006,
        Item_Price = 0x30000007,
        Item_Sex = 0x30000008,
        MaleSN = 0x30000009,
        FemaleSN = 0x30000010,
        CoupleDate = 0x30000011,
        SeparateDate = 0x30000012,
        SeparateSN = 0x30000013,
        DivorceSN = 0x30000014,
        WeddingDate = 0x30000015,
        DivorceDate = 0x30000016,
        UserNick = 0x30000017,
        ExpiredDate = 0x30000018,
        //////////////////////////////��ϵ ͳ �� Ϣ��//////////////////////////////            
        Relay_Notes = 0xFE0000E8,
        Relay_Post = 0xFE0000E9,
        Relay_ReciveName = 0xFE0000EB,

        Result_Write = 0xFE0000ED,
        Result_Content = 0xFE0000EE,

        User1 = 0x20000001,
        User2 = 0x20000002,
        User3 = 0x20000003,
        User4 = 0x20000004,
        User5 = 0x20000005,
        User6 = 0x20000006,
        User7 = 0x20000007,
        User8 = 0x20000008,
        User9 = 0x20000009,
        User10 = 0x2000000A,
        User11 = 0x2000000B,
        User12 = 0x2000000C,
        User13 = 0x2000000D,
        User14 = 0x2000000E,
        User15 = 0x2000000F,

        Pwd1 = 0x20000010,
        Pwd2 = 0x20000011,
        Pwd3 = 0x20000012,
        Pwd4 = 0x20000013,
        Pwd5 = 0x20000014,
        Pwd6 = 0x20000015,
        Pwd7 = 0x20000016,
        Pwd8 = 0x20000017,
        Pwd9 = 0x20000018,
        Pwd10 = 0x20000019,
        Pwd11 = 0x2000001A,
        Pwd12 = 0x2000001B,
        Pwd13 = 0x2000001C,
        Pwd14 = 0x2000001D,
        Pwd15 = 0x2000001E,

        Process_Mode = 0xFFFFFF08,
        Process_Service = 0xFFFFFF09,
        Verify_Message = 0xFFFFFFFA,
        Page_Count = 0xFFFFFFFB,
        Page_Current = 0xFFFFFFFC,
        Page_Size = 0xFFFFFFFD,


        CRCVALUE    = 0xFFFFFFFE,
        MESSAGE     = 0xFFFFFFFF
        #endregion
    }
}