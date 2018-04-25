using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    /// <summary>
    /// �Զ������
    /// </summary>
    public abstract class CustomClass : IDisposable
    {
        #region ����/��������
        /// <summary>
        /// ��������
        /// </summary>
        ~CustomClass()
        {
            Free(false);
        }
        #endregion

        #region ��������
        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        /// <param name="bDispodedByUser">�û���Ԥ����/��</param>
        protected virtual void Free(bool bDispodedByUser) { }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ������
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

        #region ��������
        /// <summary>
        /// �ͷű��
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
