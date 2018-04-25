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
    /// ���������
    /// </summary>
    public class PlugInCollection : MarshalByRefObject, IList, ICollection, IEnumerable
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        public PlugInCollection()
        {
            this.pPlugInList = new ArrayList();
        }
        #endregion

        #region ��������
        /// <summary>
        /// ����ģ����Ϣ
        /// </summary>
        /// <param name="strLibrary">ģ�����</param>
        /// <param name="pType">ģ������</param>
        /// <returns>��ţ�-1Ϊ����ʧ��</returns>
        public int Add(string strLibrary, Type pType)
        {
            object[] oAttributes = pType.GetCustomAttributes(typeof(PlugInAttribute), true);

            if (oAttributes.Length == 1)
            {
                //ģ����Ϣ
                PlugIn eCategory = ((PlugInAttribute)oAttributes[0]).Category;
                string strName = ((PlugInAttribute)oAttributes[0]).Name;
                string strClass = ((PlugInAttribute)oAttributes[0]).Class;
                string strDepiction = ((PlugInAttribute)oAttributes[0]).Depiction;

                PlugInInfo pPluginInfo = new PlugInInfo(eCategory, strName, strLibrary, strDepiction);

                //������Ϣ
                MethodInfo pMethodInfo = pType.GetMethod(strClass, BindingFlags.Instance | BindingFlags.Public);

                ArrayList arrParamNames = new ArrayList();

                foreach (ParameterInfo pParameterInfo in pMethodInfo.GetParameters())
                {
                    arrParamNames.Add(pParameterInfo.Name);
                }

                //����ģ����Ϣ����ֵ
                pPluginInfo.Function = String.Format("{0} {1}.{2}({3})", pMethodInfo.ReturnType.Name, pType.Name, pMethodInfo.Name, String.Join(", ", (string[])arrParamNames.ToArray(typeof(string))));
                pPluginInfo.Method = pMethodInfo;
                pPluginInfo.Entrance = Activator.CreateInstance(pType);

                return this.pPlugInList.Add(pPluginInfo);
            }

            return -1;
        }

        /// <summary>
        /// ��ȡ�������
        /// </summary>
        /// <param name="strModuleName">ģ������</param>
        /// <returns>���</returns>
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
        /// �ϲ�ģ����Ϣ
        /// </summary>
        /// <param name="pPluginCollection">ģ�鼯��</param>
        public void Merge(PlugInCollection pPluginCollection)
        {
            foreach (PlugInInfo pPlugin in pPluginCollection)
            {
                this.pPlugInList.Add(pPlugin);
            }
        }

        /// <summary>
        /// �Ƴ�ָ������ģ����Ϣ
        /// </summary>
        /// <param name="strLibrary">�������</param>
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

        #region ���Զ���
        /// <summary>
        /// ƥ��ģ����Ϣ
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
                        throw new ArgumentException("���ܸ���ģ����Ϣ��");
                    }

                    this.pPlugInList[(int)parameter] = value;
                }
                else
                {
                    if (!(parameter is string))
                    {
                        throw new ArgumentException("����ƥ��ģ�����ƣ�");
                    }

                    int iCurrentIndex = 0;

                    foreach (PlugInInfo pPlugin in this.pPlugInList)
                    {
                        if (pPlugin.Name == (string)parameter)
                        {
                            if (iIndex != -1 && iIndex != iCurrentIndex)
                            {
                                throw new ArgumentException("���ܸ���ģ����Ϣ��");
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

        #region �ֶζ���
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
    /// �����Ϣ��
    /// </summary>
    [Serializable]
    public class PlugInInfo : MarshalByRefObject
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="eCategory">ģ�����</param>
        /// <param name="strName">ģ������</param>
        /// <param name="strLibrary">ģ������</param>
        /// <param name="strDepiction">ģ��˵��</param>
        internal PlugInInfo(PlugIn eCategory, string strName, string strLibrary, string strDepiction)
        {
            this._eCategory = eCategory;
            this._strName = strName;
            this._strLibrary = strLibrary;
            this._strDepiction = strDepiction;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ����ģ��
        /// </summary>
        /// <param name="pParams">ģ�����</param>
        /// <returns>ģ�鷵��ֵ</returns>
        public object ModuleInvoke(params object[] pParams)
        {
            return this.pMethodInfo.Invoke(this.pModuleEntrance, pParams);
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ģ�����
        /// </summary>
        public PlugIn Category
        {
            get
            {
                return this._eCategory;
            }
        }

        /// <summary>
        /// ģ������
        /// </summary>
        public string Name
        {
            get
            {
                return this._strName;
            }
        }

        /// <summary>
        /// ģ������
        /// </summary>
        public string Library
        {
            get
            {
                return this._strLibrary;
            }
        }

        /// <summary>
        /// ģ��˵��
        /// </summary>
        public string Depiction
        {
            get
            {
                return this._strDepiction;
            }
        }

        /// <summary>
        /// ��������
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
        /// ģ���������
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
        /// ģ�鷽��
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

        #region �ֶζ���
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
