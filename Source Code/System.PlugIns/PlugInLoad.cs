using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

using System.Global;

namespace System.PlugIns
{
    /// <summary>
    /// ��������ඨ��
    /// </summary>
    public class PlugInLoad : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strSuffix"></param>
        public PlugInLoad(string strPath, string strSuffix)
        {
            this._strPath = strPath;
            this._strSuffix = strSuffix;

            this.pDomainTable = new Hashtable();
            this.pPluginCollection = new PlugInCollection();
        }

        /// <summary>
        /// �̳з��� Free
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                foreach (object pObject in this.pDomainTable.Keys)
                {
                    AppDomain.Unload((AppDomain)this.pDomainTable[pObject]);
                }

                this.pDomainTable.Clear();
                this.pPluginCollection.Clear();
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region ��������
        /// <summary>
        /// ����ָ��Ŀ¼�µ�ģ��
        /// </summary>
        public void Load()
        {
            DirectoryInfo pDirectoryInfo = new DirectoryInfo(this._strPath);

            foreach (FileInfo pFileInfo in pDirectoryInfo.GetFiles("*." + this._strSuffix))
            {
                string strLibrary = Path.GetFileNameWithoutExtension(pFileInfo.FullName);

                this.pPluginCollection.Merge(this.LoadModule(strLibrary));
            }
        }

        /// <summary>
        /// ����ָ����ģ��
        /// </summary>
        /// <param name="strLibrary">ģ������</param>
        public void Load(string strLibrary)
        {
            this.pPluginCollection.Merge(this.LoadModule(strLibrary));
        }

        /// <summary>
        /// ж������ģ��
        /// </summary>
        public void UnLoad()
        {
            this.Free(true);
        }

        /// <summary>
        /// ж��ָ��ģ��
        /// </summary>
        /// <param name="strLibrary">ģ�����</param>
        public void UnLoad(string strLibrary)
        {
            if (this.pDomainTable.ContainsKey(strLibrary))
            {
                this.pPluginCollection.RemoveRange(strLibrary);

                AppDomain.Unload((AppDomain)this.pDomainTable[strLibrary]);
            }

            this.pDomainTable.Remove(strLibrary);
        }

        /// <summary>
        /// ��ȡָ����Ȩ�޼���Ϣ
        /// </summary>
        /// <param name="strPermission">����������</param>
        /// <param name="strPermission">Ȩ�޼�����</param>
        /// <returns></returns>
        private NamedPermissionSet FindNamedPermissionSet(string strPolicy, string strPermission)
        {
            IEnumerator PolicyEnumerator = SecurityManager.PolicyHierarchy();

            while (PolicyEnumerator.MoveNext())
            {
                PolicyLevel pPolicyLevel = (PolicyLevel)PolicyEnumerator.Current;

                if (pPolicyLevel.Label == strPolicy)
                {
                    IList PermissionList = pPolicyLevel.NamedPermissionSets;
                    IEnumerator PermissionEnumerator = PermissionList.GetEnumerator();

                    while (PermissionEnumerator.MoveNext())
                    {
                        if (((NamedPermissionSet)PermissionEnumerator.Current).Name == strPermission)
                        {
                            return ((NamedPermissionSet)PermissionEnumerator.Current);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ���ó������Ȩ��
        /// </summary>
        /// <param name="pAppDomain">ָ���ĳ�����</param>
        private void SetAppDomainPolicy(AppDomain pAppDomain)
        {
            PolicyLevel pLevel = PolicyLevel.CreateAppDomainLevel();

            PermissionSet pPermission = new PermissionSet(PermissionState.None);
            pPermission.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            UnionCodeGroup pRootGroup = new UnionCodeGroup(new AllMembershipCondition(),
                                    new PolicyStatement(pPermission, PolicyStatementAttribute.Nothing));

            NamedPermissionSet pPermissionSet = FindNamedPermissionSet("Machine", "Everything");

            UnionCodeGroup pChileGroup = new UnionCodeGroup(
                                    new ZoneMembershipCondition(SecurityZone.MyComputer),
                                    new PolicyStatement(pPermissionSet, PolicyStatementAttribute.Nothing));

            pChileGroup.Name = "Virtual Intranet";
            pRootGroup.AddChild(pChileGroup);
            pLevel.RootCodeGroup = pRootGroup;
            pAppDomain.SetAppDomainPolicy(pLevel);
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="strDomain">����������</param>
        /// <returns>Ψһ��</returns>
        private bool CreateDomain(string strDomain)
        {
            BindingFlags eBindingFlags = BindingFlags.CreateInstance |
                BindingFlags.Instance | BindingFlags.Public;

            AppDomainSetup pDomainSetup = new AppDomainSetup();

            pDomainSetup.ApplicationName = "Module Center";
            pDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            pDomainSetup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            pDomainSetup.ShadowCopyFiles = "true";
            pDomainSetup.ShadowCopyDirectories = this._strPath;

            this.pAppDomain = AppDomain.CreateDomain(strDomain, null, pDomainSetup);
            this.SetAppDomainPolicy(this.pAppDomain);

            if (!this.pDomainTable.ContainsKey(strDomain))
            {
                this.pDomainTable.Add(strDomain, this.pAppDomain);

                this.pRemoteLoader = (RemoteLoader)this.pAppDomain.CreateInstanceAndUnwrap(
                                    "ModuleCore",
                                    "ModuleCore.RemoteLoader",
                                    true, eBindingFlags, null, null, null, null, null);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ����ģ����Ϣ
        /// </summary>
        /// <param name="strLibrary">�������</param>
        /// <returns>ģ���б�</returns>
        private PlugInCollection LoadModule(string strLibrary)
        {
            if (this.CreateDomain(strLibrary))
            {
                return this.pRemoteLoader.LoadAssembly(this._strPath, this._strSuffix, strLibrary);
            }
            else
            {
                throw new ArgumentException("����Ӧ�ó�����ʧ�ܣ�");
            }
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ģ���б�
        /// </summary>
        public PlugInCollection Modules
        {
            get
            {
                return this.pPluginCollection;
            }
        }
        #endregion

        #region �ֶζ���
        private string _strPath;
        private string _strSuffix;

        private AppDomain pAppDomain;
        private Hashtable pDomainTable;
        private PlugInCollection pPluginCollection;
        private RemoteLoader pRemoteLoader;
        #endregion

        #region RemoteLoader��
        internal class RemoteLoader : MarshalByRefObject
        {
            #region ��������
            /// <summary>
            /// ������ط���
            /// </summary>
            /// <param name="strPath">·��</param>
            /// <param name="strSuffix">��չ��</param>
            /// <param name="strLibrary">ģ������</param>
            /// <returns>ģ���б�</returns>
            public PlugInCollection LoadAssembly(string strPath, string strSuffix, string strLibrary)
            {
                PlugInCollection pPluginCollection = new PlugInCollection();

                Assembly pAssembly = Assembly.LoadFile(strPath + "\\" + strLibrary + "." + strSuffix);

                foreach (Type pType in pAssembly.GetTypes())
                {
                    pPluginCollection.Add(strLibrary, pType);
                }

                return pPluginCollection;
            }
            #endregion
        }
        #endregion
    }
}
