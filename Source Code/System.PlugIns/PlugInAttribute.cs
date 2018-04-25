using System;
using System.Collections.Generic;
using System.Text;

using System.Global;

namespace System.PlugIns
{
    /// <summary>
    /// 插件属性
    /// </summary>
    /// <remarks>
    /// 建立系统插件的索引,保证该插件具有唯一性 
    ///</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PlugInAttribute : Attribute
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eCategory">插件类别</param>
        /// <param name="strName">插件名称</param>
        /// <param name="strClass">插件类名</param>
        /// <param name="strDepiction">插件说明</param>
        public PlugInAttribute(PlugIn eCategory, string strName, string strClass, string strDepiction)
        {
            this._eCategory = eCategory;
            this._strName = strName;
            this._strClass = strClass;
            this._strDepiction = strDepiction;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 插件类别
        /// </summary>
        public PlugIn Category
        {
            get
            {
                return this._eCategory;
            }
        }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name
        {
            get
            {
                return this._strName;
            }
        }

        /// <summary>
        /// 插件类名
        /// </summary>
        public string Class
        {
            get
            {
                return this._strClass;
            }
        }

        /// <summary>
        /// 插件说明
        /// </summary>
        public string Depiction
        {
            get
            {
                return this._strDepiction;
            }
        }
        #endregion

        #region 变量定义
        /// <summary>
        /// 插件类别
        /// </summary>
        private PlugIn _eCategory;
        /// <summary>
        /// 插件名称
        /// </summary>
        private string _strName;
        /// <summary>
        /// 插件类名
        /// </summary>
        private string _strClass;
        /// <summary>
        /// 插件说明
        /// </summary>
        private string _strDepiction;
        #endregion
    }
}
