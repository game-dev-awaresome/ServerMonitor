using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Reflection;

using System.Global;

namespace System.PlugIns
{
    /// <summary>
    /// 插件集合类
    /// </summary>
    public class PlugInCollection : MarshalByRefObject, IList, ICollection, IEnumerable
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public PlugInCollection()
        {
            this.pPlugInList = new ArrayList();
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 加入模块信息
        /// </summary>
        /// <param name="strLibrary">模块类库</param>
        /// <param name="pType">模块类型</param>
        /// <returns>序号：-1为加入失败</returns>
        public int Add(string strLibrary, Type pType)
        {
            object[] oAttributes = pType.GetCustomAttributes(typeof(PlugInAttribute), true);

            if (oAttributes.Length == 1)
            {
                //模块信息
                PlugIn eCategory = ((PlugInAttribute)oAttributes[0]).Category;
                string strName = ((PlugInAttribute)oAttributes[0]).Name;
                string strClass = ((PlugInAttribute)oAttributes[0]).Class;
                string strDepiction = ((PlugInAttribute)oAttributes[0]).Depiction;

                PlugInInfo pPluginInfo = new PlugInInfo(eCategory, strName, strLibrary, strDepiction);

                //参数信息
                MethodInfo pMethodInfo = pType.GetMethod(strClass, BindingFlags.Instance | BindingFlags.Public);

                ArrayList arrParamNames = new ArrayList();

                foreach (ParameterInfo pParameterInfo in pMethodInfo.GetParameters())
                {
                    arrParamNames.Add(pParameterInfo.Name);
                }

                //设置模块信息附加值
                pPluginInfo.Function = String.Format("{0} {1}.{2}({3})", pMethodInfo.ReturnType.Name, pType.Name, pMethodInfo.Name, String.Join(", ", (string[])arrParamNames.ToArray(typeof(string))));
                pPluginInfo.Method = pMethodInfo;
                pPluginInfo.Entrance = Activator.CreateInstance(pType);

                return this.pPlugInList.Add(pPluginInfo);
            }

            return -1;
        }

        /// <summary>
        /// 获取索引序号
        /// </summary>
        /// <param name="strModuleName">模块名称</param>
        /// <returns>序号</returns>
        public int GetIndex(string strModuleName)
        {
            int iResultIndex = -1;

            for (int i = 0; i < this.pPlugInList.Count; i++)
            {
                if (String.Compare(this[i].Name, strModuleName, true, CultureInfo.CurrentCulture) == 0)
                {
                    return i;
                }
            }

            return iResultIndex;
        }

        /// <summary>
        /// 合并模块信息
        /// </summary>
        /// <param name="pPluginCollection">模块集合</param>
        public void Merge(PlugInCollection pPluginCollection)
        {
            foreach (PlugInInfo pPlugin in pPluginCollection)
            {
                this.pPlugInList.Add(pPlugin);
            }
        }

        /// <summary>
        /// 移除指定类库的模块信息
        /// </summary>
        /// <param name="strLibrary">类库名称</param>
        public void RemoveRange(string strLibrary)
        {
            foreach (PlugInInfo pPlugin in this.pPlugInList)
            {
                if (String.Compare(pPlugin.Library, strLibrary, true, CultureInfo.CurrentCulture) == 0)
                {
                    this.pPlugInList.Remove(pPlugin);
                }
            }
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 匹配模块信息
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public PlugInInfo this[object parameter]
        {
            get
            {
                if (!(parameter is int))
                {
                    if (parameter is string)
                    {
                        foreach (PlugInInfo pPlugin in this.pPlugInList)
                        {
                            if (pPlugin.Name == (string)parameter)
                            {
                                return pPlugin;
                            }
                        }
                    }

                    return (PlugInInfo)this.pPlugInList[0];
                }

                return (PlugInInfo)this.pPlugInList[(int)parameter];
            }
            set
            {
                int iIndex = this.GetIndex(value.Name);

                if (parameter is int)
                {
                    if (iIndex != -1 && iIndex != (int)parameter)
                    {
                        throw new ArgumentException("不能更新模块信息！");
                    }

                    this.pPlugInList[(int)parameter] = value;
                }
                else
                {
                    if (!(parameter is string))
                    {
                        throw new ArgumentException("不能匹配模块名称！");
                    }

                    int iCurrentIndex = 0;

                    foreach (PlugInInfo pPlugin in this.pPlugInList)
                    {
                        if (pPlugin.Name == (string)parameter)
                        {
                            if (iIndex != -1 && iIndex != iCurrentIndex)
                            {
                                throw new ArgumentException("不能更新模块信息！");
                            }

                            this.pPlugInList[iCurrentIndex] = value;
                            break;
                        }

                        iCurrentIndex++;
                    }
                }
            }
        }
        #endregion

        #region 字段定义
        private ArrayList pPlugInList;
        #endregion

        #region IList Members

        public int Add(object value)
        {
            if (value is PlugInInfo)
            {
                return this.pPlugInList.Add(value);
            }

            return -1;
        }

        public void Clear()
        {
            this.pPlugInList.Clear();
        }

        public bool Contains(object value)
        {
            return this.pPlugInList.Contains(value);
        }

        public int IndexOf(object value)
        {
            return this.pPlugInList.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            if (value is PlugInInfo)
            {
                this.pPlugInList.Insert(index, value);
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this.pPlugInList.IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.pPlugInList.IsReadOnly;
            }
        }

        public void Remove(object value)
        {
            this.pPlugInList.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.pPlugInList.Remove(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this.pPlugInList[index];
            }
            set
            {
                this.pPlugInList[index] = value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            this.pPlugInList.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return this.pPlugInList.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.pPlugInList.IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.pPlugInList.SyncRoot;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return this.pPlugInList.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// 插件信息类
    /// </summary>
    [Serializable]
    public class PlugInInfo : MarshalByRefObject
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eCategory">模块类别</param>
        /// <param name="strName">模块名称</param>
        /// <param name="strLibrary">模块类名</param>
        /// <param name="strDepiction">模块说明</param>
        internal PlugInInfo(PlugIn eCategory, string strName, string strLibrary, string strDepiction)
        {
            this._eCategory = eCategory;
            this._strName = strName;
            this._strLibrary = strLibrary;
            this._strDepiction = strDepiction;
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="pParams">模块参数</param>
        /// <returns>模块返回值</returns>
        public object ModuleInvoke(params object[] pParams)
        {
            return this.pMethodInfo.Invoke(this.pModuleEntrance, pParams);
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 模块类别
        /// </summary>
        public PlugIn Category
        {
            get
            {
                return this._eCategory;
            }
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name
        {
            get
            {
                return this._strName;
            }
        }

        /// <summary>
        /// 模块类名
        /// </summary>
        public string Library
        {
            get
            {
                return this._strLibrary;
            }
        }

        /// <summary>
        /// 模块说明
        /// </summary>
        public string Depiction
        {
            get
            {
                return this._strDepiction;
            }
        }

        /// <summary>
        /// 函数描述
        /// </summary>
        public string Function
        {
            get
            {
                return this.strFunction;
            }
            set
            {
                this.strFunction = value;
            }
        }

        /// <summary>
        /// 模块入口名称
        /// </summary>
        public object Entrance
        {
            get
            {
                return this.pModuleEntrance;
            }
            set
            {
                this.pModuleEntrance = value;
            }
        }

        /// <summary>
        /// 模块方法
        /// </summary>
        public MethodInfo Method
        {
            get
            {
                return this.pMethodInfo;
            }
            set
            {
                this.pMethodInfo = value;
            }
        }
        #endregion

        #region 字段定义
        private PlugIn _eCategory;
        private string _strName;
        private string _strLibrary;
        private string _strDepiction;
        private string strFunction;
        private object pModuleEntrance;
        private MethodInfo pMethodInfo;
        #endregion
    }
}
