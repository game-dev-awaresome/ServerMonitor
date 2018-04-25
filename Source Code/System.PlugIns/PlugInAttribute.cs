using System;
using System.Collections.Generic;
using System.Text;

using System.Global;

namespace System.PlugIns
{
    /// <summary>
    /// �������
    /// </summary>
    /// <remarks>
    /// ����ϵͳ���������,��֤�ò������Ψһ�� 
    ///</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PlugInAttribute : Attribute
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="eCategory">������</param>
        /// <param name="strName">�������</param>
        /// <param name="strClass">�������</param>
        /// <param name="strDepiction">���˵��</param>
        public PlugInAttribute(PlugIn eCategory, string strName, string strClass, string strDepiction)
        {
            this._eCategory = eCategory;
            this._strName = strName;
            this._strClass = strClass;
            this._strDepiction = strDepiction;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ������
        /// </summary>
        public PlugIn Category
        {
            get
            {
                return this._eCategory;
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        public string Name
        {
            get
            {
                return this._strName;
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        public string Class
        {
            get
            {
                return this._strClass;
            }
        }

        /// <summary>
        /// ���˵��
        /// </summary>
        public string Depiction
        {
            get
            {
                return this._strDepiction;
            }
        }
        #endregion

        #region ��������
        /// <summary>
        /// ������
        /// </summary>
        private PlugIn _eCategory;
        /// <summary>
        /// �������
        /// </summary>
        private string _strName;
        /// <summary>
        /// �������
        /// </summary>
        private string _strClass;
        /// <summary>
        /// ���˵��
        /// </summary>
        private string _strDepiction;
        #endregion
    }
}
