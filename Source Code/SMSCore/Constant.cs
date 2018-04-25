using System;
using System.Collections.Generic;
using System.Text;

namespace SMSCore
{
    internal class Constant
    {
        public const string strServerAddr = "http://61.152.150.202";
        public const string strReceivePage = "/handlesms/sms.do";
        public const string strReceiveMethod = "POST";
        public const string strContentType = "application/x-www-form-urlencoded";
        public const string strLanguage = "GB2312";

        public const string strMessageHead = "<?xml version=\"1.0\" encoding=\"GB2312\"?>";
        public const string strMessageBody = @"
            <SBMP_MO_MESSAGE>
            <CONNECT_ID>4234</CONNECT_ID>
            <MO_MESSAGE_ID>4138QYDX4234Q0000</MO_MESSAGE_ID>

            <RECEIVE_DATE>[DATE]</RECEIVE_DATE>
            <RECEIVE_TIME>[TIME]</RECEIVE_TIME>

            <GATEWAY_ID>10</GATEWAY_ID>
            <VALID>1</VALID>
            <CITY_CODE>021</CITY_CODE>
            <CITY_NAME>上海</CITY_NAME>
            <STATE_CODE>021</STATE_CODE>
            <STATE_NAME>上海</STATE_NAME>
            <TP_PID>0</TP_PID>
            <TP_UDHI>0</TP_UDHI>

            <MSISDN>[MOBILE]</MSISDN>

            <MESSAGE_TYPE>0</MESSAGE_TYPE>

            <MESSAGE>[MESSAGE]</MESSAGE>

            <LONG_CODE>8</LONG_CODE>
            <SERVICE_CODE>9588</SERVICE_CODE>
            </SBMP_MO_MESSAGE>
            ";
    }
}
