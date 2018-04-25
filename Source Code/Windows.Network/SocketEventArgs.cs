using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Network
{
    /// <summary>
    /// Socket�¼�
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ����
        /// </summary>
        /// <param name="SocketConnection">���ӽӿ�</param>
        public SocketEventArgs(ISocketConnection SocketConnection)
        {
            this._bLink = true;

            this._SocketConnection = SocketConnection;
            this._SocketException = new Exception();
        }

        /// <summary>
        /// ���캯�� -- ����
        /// </summary>
        /// <param name="SocketConnection">���ӽӿ�</param>
        public SocketEventArgs(ISocketConnection SocketConnection, bool bConnection)
        {
            this._bLink = bConnection;

            this._SocketConnection = SocketConnection;
            this._SocketException = new Exception();
        }

        /// <summary>
        /// ���캯�� -- ����
        /// </summary>
        /// <param name="SocketConnection">���ӽӿ�</param>
        /// <param name="SocketException">�쳣��</param>
        public SocketEventArgs(ISocketConnection SocketConnection, Exception SocketException)
        {
            this._bLink = false;

            this._SocketConnection = SocketConnection;
            this._SocketException = SocketException;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ����״̬
        /// </summary>
        /// <remarks>����/����</remarks>
        public bool Link
        {
            get
            {
                return this._bLink;
            }
        }

        /// <summary>
        /// Socket Connection �ӿ�
        /// </summary>
        public ISocketConnection Connection
        {
            get
            {
                return this._SocketConnection;
            }
        }

        /// <summary>
        /// Socket�쳣
        /// </summary>
        public Exception SocketException
        {
            get
            {
                return this._SocketException;
            }
        }
        #endregion

        #region �ֶζ���
        private bool _bLink;
        private ISocketConnection _SocketConnection;
        private Exception _SocketException;
        #endregion
    }

    /// <summary>
    /// ��Ϣ�¼�
    /// </summary>
    public class MessageEventArgs : SocketEventArgs
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="IConnection">���ӽӿ�</param>
        /// <param name="pBuffer">������</param>
        public MessageEventArgs(ISocketConnection SocketConnection, byte[] pBuffer)
            : base(SocketConnection)
        {
            this._pBuffer = pBuffer;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// Socket״̬
        /// </summary>
        public byte[] SocketBuffer
        {
            get
            {
                return this._pBuffer;
            }
        }
        #endregion

        #region �ֶζ���
        private byte[] _pBuffer;
        #endregion
    }
}
