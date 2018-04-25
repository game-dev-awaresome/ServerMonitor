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
    /// 插件加载类定义
    /// </summary>
    public class PlugInLoad : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
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
        /// 继承方法 Free
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

        #region 方法定义
        /// <summary>
        /// 加载指定目录下的模块
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
        /// 加载指定的模块
        /// </summary>
        /// <param name="strLibrary">模块名称</param>
        public void Load(string strLibrary)
        {
            this.pPluginCollection.Merge(this.LoadModule(strLibrary));
        }

        /// <summary>
        /// 卸载所有模块
        /// </summary>
        public void UnLoad()
        {
            this.Free(true);
        }

        /// <summary>
        /// 卸载指定模块
        /// </summary>
        /// <param name="strLibrary">模块类库</param>
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
        /// 获取指定的权限集信息
        /// </summary>
        /// <param name="strPermission">策略组名称</param>
        /// <param name="strPermission">权限集名称</param>
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
        /// 设置程序域的权限
        /// </summary>
        /// <param name="pAppDomain">指定的程序域</param>
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
        /// 创建程序域
        /// </summary>
        /// <param name="strDomain">程序域名称</param>
        /// <returns>唯一域</returns>
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
        /// 加载模块信息
        /// </summary>
        /// <param name="strLibrary">类库名称</param>
        /// <returns>模块列表</returns>
        private PlugInCollection LoadModule(string strLibrary)
        {
            if (this.CreateDomain(strLibrary))
            {
                return this.pRemoteLoader.LoadAssembly(this._strPath, this._strSuffix, strLibrary);
            }
            else
            {
                throw new ArgumentException("创建应用程序域失败！");
            }
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 模块列表
        /// </summary>
        public PlugInCollection Modules
        {
            get
            {
                return this.pPluginCollection;
            }
        }
        #endregion

        #region 字段定义
        private string _strPath;
        private string _strSuffix;

        private AppDomain pAppDomain;
        private Hashtable pDomainTable;
        private PlugInCollection pPluginCollection;
        private RemoteLoader pRemoteLoader;
        #endregion

        #region RemoteLoader类
        internal class RemoteLoader : MarshalByRefObject
        {
            #region 方法定义
            /// <summary>
            /// 反射加载方法
            /// </summary>
            /// <param name="strPath">路径</param>
            /// <param name="strSuffix">扩展名</param>
            /// <param name="strLibrary">模块名称</param>
            /// <returns>模块列表</returns>
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
