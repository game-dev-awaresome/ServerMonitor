using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    /// <summary>
    /// 自定义基类
    /// </summary>
    public abstract class CustomClass : IDisposable
    {
        #region 构造/析构函数
        /// <summary>
        /// 析构函数
        /// </summary>
        ~CustomClass()
        {
            Free(false);
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="bDispodedByUser">用户干预：是/否</param>
        protected virtual void Free(bool bDispodedByUser) { }
        #endregion

        #region 属性定义
        /// <summary>
        /// 处理标记
        /// </summary>
        protected bool Disposed
        {
            get
            {
                lock (this)
                {
                    return this._bDisposed;
                }
            }
        }
        #endregion

        #region 变量定义
        /// <summary>
        /// 释放标记
        /// </summary>
        private bool _bDisposed = false;
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            lock (this)
            {
                if (this._bDisposed == false)
                {
                    try
                    {
                        Free(true);
                    }
                    finally
                    {
                        this._bDisposed = true;
                        GC.SuppressFinalize(this);
                    }
                }
            }
        }
        #endregion
    }
}
