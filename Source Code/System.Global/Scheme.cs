using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Global
{
    /// <summary>
    /// �����ļ���/д����
    /// </summary>
    #region �����ļ���INI
    /// <summary>
    /// ��д INI �ļ�
    /// </summary>
    public class IniFile : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="strPath">�����ļ�·��</param>
        public IniFile(string strPath)
        {
            this._strPath = strPath;
        }

        /// <summary>
        /// ���غ��� -- �ͷ���Դ
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

        #region ��������
        /// <summary>
        /// ��ȡָ��������
        /// </summary>
        /// <param name="strSection">Ƭ������</param>
        /// <param name="strKey">������</param>
        /// <param name="strDefault">Ĭ��ֵ</param>
        /// <returns>������</returns>
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
        /// д��ָ��������
        /// </summary>
        /// <param name="strSection">Ƭ������</param>
        /// <param name="strKey">������</param>
        /// <param name="strValue">������</param>
        /// <returns>�������</returns>
        public long WriteValue(string strSection, string strKey, string strValue)
        {
            return WritePrivateProfileString(strSection, strKey, strValue, this._strPath);
        }
        #endregion

        #region ��������
        /// <summary>
        /// INI�ļ�·��
        /// </summary>
        private string _strPath;
        #endregion

        #region ���� Win32 API ����
        /// <summary>
        /// ��ȡ����ֵ
        /// </summary>
        /// <param name="strSection">Ƭ������</param>
        /// <param name="strKey">����</param>
        /// <param name="strDefault">ȱʡֵ</param>
        /// <param name="stVal">����ֵ</param>
        /// <param name="iSize">����</param>
        /// <param name="strPath">�ļ�·��</param>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string strSection, string strKey, string strDefault, StringBuilder stVal, int iSize, string strPath);

        /// <summary>
        /// д������ֵ
        /// </summary>
        /// <param name="strSection">Ƭ������</param>
        /// <param name="strKey">����</param>
        /// <param name="stVal">����ֵ</param>
        /// <param name="strPath">�ļ�·��</param>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string strSection, string strKey, string strVal, string strPath);
        #endregion
    }
    #endregion

    #region �����ļ���XML
    #endregion
}
