using System;
using System.Collections.Generic;
using System.Text;

namespace SocketTest
{
    public class Constant
    {
        public const string StrAuConnect = @"Connect Timeout=3000;Server=$serverip;Database=$database;uid=$uid; pwd=$pwd";


        public const string StrAuGetServer = @"SELECT Server_ID,Server_Name,Server_Internet FROM Game_Server WHERE Server_Layer = 0  
                                               AND Server_Game = (SELECT Game_ID FROM Info_Game WHERE Game_Name like '¾¢ÎèÍÅ%') 
                                               AND Server_ID IN (SELECT Popedom_Tag FROM Common_Popedom WHERE Popedom_Sort > 6)"; //AND Popedom_Verify = $verify

        public const string StrAuGet_Audition = @"SELECT top 1 Server_ID,Server_Name,Server_Internet FROM Game_Server WHERE Server_DBtype = 3 AND Server_Layer = $server_layer
                                                AND Server_Name like '%-Audition%'";
        public const string StrAuGet_ItemDB = @"SELECT top 1 Server_ID,Server_Name,Server_Internet FROM Game_Server WHERE Server_DBtype = 3 AND Server_Layer = $server_layer
                                                AND (Server_Name like '%-itemdb%' OR Server_Name like '%&itemdb%')";

        public const string StrAuGetPwd = @"SELECT Access_User,Access_Pwd FROM Server_Access WHERE Access_Server = $access_server AND Access_Sort = 14";

        public const string StrAuGetPopedom = @"SELECT Distinct(Access_User),Access_Pwd FROM Server_Access WHERE Access_Sort > 5 AND Access_Server 
                                                IN (SELECT Server_ID FROM Game_Server WHERE Server_Layer = $access_server)";





        public const string StrAuGetExpLevel = @"SELECT UserSN,Exp,Level FROM UserInfo as a WHERE UserID ='$userid'";
        //,(SELECT level FROM LevelInfo WHERE Exp < a.Exp ORDER BY level DESC LIMIT 1) AS RealLevel

        public const string StrRealyLevel = @"SELECT level AS RealLevel From LevelInfo Where Exp < $Exp ORDER BY level DESC LIMIT 1";

        public const string StrAuUpdateExp = @"UPDATE UserInfo SET Level = '$level' WHERE UserID = '$userid'";

        public const string StrAuGetGender = @"SELECT UserSN,UserGender,UserNick FROM UserInfo WHERE UserID = '$userid'";

        public const string StrAuUpdateGender = @"UPDATE avatar_default SET CurrentType='$value',OriginalType='$value' WHERE usersn='$usersn'";

        public const string StrAuGetCouple = @"SELECT MaleSN,FemaleSN,CoupleDate,SeparateDate,SeparateSN FROM couple_log WHERE $_value = '$value'";
        public const string StrAuGetWedding = @"SELECT MaleSN,FemaleSN,WeddingDate,DivorceDate,DivorceSN FROM wedding_log WHERE $_value = '$value'";

        public const string StrAuGetItem = @"SELECT Item_ID,Item_Name,Item_Price,Item_Sex FROM Prors_Item";

        public const string StrAuUpdateCard = @"UPDATE avatar_inventory_items SET UseItem = 1 WHERE UserSN = '$UserSN' AND ItemID = '$ItemID'";

        public const string StrAUGetItemTable = @"SELECT * FROM v3_avatar_table WHERE V_Props_ID = '$V_Props_ID'";

        public const string StrAuCheckItem = @"SELECT '±ê¼Ç' as UserNick, ItemID as Item_ID,ExpiredDate FROM $ItemTable_Name WHERE UserSN = '$UserSN' AND ItemID = '$ItemID'";

        public const string StrAuSendItem = @"INSERT INTO $ItemTable_Name (UserSN,BuyNick,ItemID,DuplicationCount,ExpiredType,ExpiredDate)
                                              VALUES('$UserSN','9YOUADMIN','$ItemID',1,'$ExpiredType','$ExpiredDate')";

        public const string StrAuUpdateItem = @"UPDATE $ItemTable_Name SET ExpiredDate = '$ExpiredDate' WHERE UserSN = '$UserSN' AND ItemID = '$ItemID'";

        public const string StrAuSendCard = @"INSERT INTO $ItemTable_Name (UserSN,BuyNick,ItemID,DuplicationCount,ExpiredType,ExpiredDate,UseItem,RemainCount)
                                              VALUES('$UserSN','9YOUADMIN','$ItemID',1,'$ExpiredType','$ExpiredDate',1,3)";
    }
}
