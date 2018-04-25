using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace SMSCore
{
    public class WebSMS
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WebSMS() { }

        public byte[] sendSMS(string strMobile, string strContent)
        {
            return this.sendSMS(this.buildBody(strMobile, strContent));
        }

        public byte[] sendSMS(byte[] pBuffer)
        {
            byte[] mResult = null;
            WebResponse mWebResponse = null;

            HttpWebRequest mWebRequest =
                    (HttpWebRequest)WebRequest.Create(Constant.strServerAddr + Constant.strReceivePage);

            mWebRequest.ContentType = Constant.strContentType;
            mWebRequest.Method = Constant.strReceiveMethod;
            mWebRequest.KeepAlive = false;
            mWebRequest.ContentLength = pBuffer.Length;

            try
            {
                mWebRequest.GetRequestStream().Write(pBuffer, 0, pBuffer.Length);
                mWebResponse = mWebRequest.GetResponse();

                mResult = new byte[mWebResponse.ContentLength];
                mWebResponse.GetResponseStream().Read(mResult, 0, mResult.Length);
                mWebResponse.GetResponseStream().Close();
            }
            catch (WebException e)
            {
                mResult = System.Text.Encoding.Default.GetBytes(e.Message);
            }

            return mResult;
        }

        public byte[] buildBody(string strMobile, string strContent)
        {
            byte[] mResult = null;
            string strMessage = Constant.strMessageHead + Constant.strMessageBody;

            strMessage = strMessage.Replace("[DATE]", dtSendTime.ToString("yyyyMMdd"));
            strMessage = strMessage.Replace("[TIME]", dtSendTime.ToString("HHmmss"));
            strMessage = strMessage.Replace("[MOBILE]", strMobile);
            strMessage = strMessage.Replace("[MESSAGE]", strContent);

            mResult = System.Text.Encoding.Default.GetBytes(strMessage);
            return mResult;
        }

        #region 私有函数
        private DateTime dtSendTime = DateTime.Now;
        #endregion
    }
}
