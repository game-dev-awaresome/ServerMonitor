using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Global
{
    /// <summary>
    /// 配置文件读/写函数
    /// </summary>
    #region 配置文件：INI
    /// <summary>
    /// 读写 INI 文件
    /// </summary>
    public class IniFile : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strPath">配置文件路径</param>
        public IniFile(string strPath)
        {
            this._strPath = strPath;
        }

        /// <summary>
        /// 重载函数 -- 释放资源
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                this._strPath = String.Empty;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 读取指定键变量
        /// </summary>
        /// <param name="strSection">片断名称</param>
        /// <param name="strKey">键名称</param>
        /// <param name="strDefault">默认值</param>
        /// <returns>键变量</returns>
        public string ReadValue(string strSection, string strKey, string strDefault)
        {
            StringBuilder strResult = new StringBuilder(255);
            int iResult = GetPrivateProfileString(strSection, strKey, strDefault, strResult, 255, this._strPath);

            if (iResult > 0)
            {
                return strResult.ToString();
            }
            else
            {
                return strDefault;
            }
        }

        /// <summary>
        /// 写入指定键变量
        /// </summary>
        /// <param name="strSection">片断名称</param>
        /// <param name="strKey">键名称</param>
        /// <param name="strValue">键变量</param>
        /// <returns>操作结果</returns>
        public long WriteValue(string strSection, string strKey, string strValue)
        {
            return WritePrivateProfileString(strSection, strKey, strValue, this._strPath);
        }
        #endregion

        #region 变量定义
        /// <summary>
        /// INI文件路径
        /// </summary>
        private string _strPath;
        #endregion

        #region 引入 Win32 API 函数
        /// <summary>
        /// 读取配置值
        /// </summary>
        /// <param name="strSection">片断名称</param>
        /// <param name="strKey">键名</param>
        /// <param name="strDefault">缺省值</param>
        /// <param name="stVal">配置值</param>
        /// <param name="iSize">长度</param>
        /// <param name="strPath">文件路径</param>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string strSection, string strKey, string strDefault, StringBuilder stVal, int iSize, string strPath);

        /// <summary>
        /// 写入配置值
        /// </summary>
        /// <param name="strSection">片断名称</param>
        /// <param name="strKey">键名</param>
        /// <param name="stVal">配置值</param>
        /// <param name="strPath">文件路径</param>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string strSection, string strKey, string strVal, string strPath);
        #endregion
    }
    #endregion

    #region 配置文件：XML
    #endregion
}
