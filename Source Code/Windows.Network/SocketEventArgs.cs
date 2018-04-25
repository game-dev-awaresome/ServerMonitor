using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Network
{
    /// <summary>
    /// Socket事件
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
        #region 构造/析构定义
        /// <summary>
        /// 构造函数 -- 连接
        /// </summary>
        /// <param name="SocketConnection">连接接口</param>
        public SocketEventArgs(ISocketConnection SocketConnection)
        {
            this._bLink = true;

            this._SocketConnection = SocketConnection;
            this._SocketException = new Exception();
        }

        /// <summary>
        /// 构造函数 -- 断线
        /// </summary>
        /// <param name="SocketConnection">连接接口</param>
        public SocketEventArgs(ISocketConnection SocketConnection, bool bConnection)
        {
            this._bLink = bConnection;

            this._SocketConnection = SocketConnection;
            this._SocketException = new Exception();
        }

        /// <summary>
        /// 构造函数 -- 断线
        /// </summary>
        /// <param name="SocketConnection">连接接口</param>
        /// <param name="SocketException">异常类</param>
        public SocketEventArgs(ISocketConnection SocketConnection, Exception SocketException)
        {
            this._bLink = false;

            this._SocketConnection = SocketConnection;
            this._SocketException = SocketException;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 连接状态
        /// </summary>
        /// <remarks>连线/断线</remarks>
        public bool Link
        {
            get
            {
                return this._bLink;
            }
        }

        /// <summary>
        /// Socket Connection 接口
        /// </summary>
        public ISocketConnection Connection
        {
            get
            {
                return this._SocketConnection;
            }
        }

        /// <summary>
        /// Socket异常
        /// </summary>
        public Exception SocketException
        {
            get
            {
                return this._SocketException;
            }
        }
        #endregion

        #region 字段定义
        private bool _bLink;
        private ISocketConnection _SocketConnection;
        private Exception _SocketException;
        #endregion
    }

    /// <summary>
    /// 消息事件
    /// </summary>
    public class MessageEventArgs : SocketEventArgs
    {
        #region 构造/析构定义
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="IConnection">连接接口</param>
        /// <param name="pBuffer">数据流</param>
        public MessageEventArgs(ISocketConnection SocketConnection, byte[] pBuffer)
            : base(SocketConnection)
        {
            this._pBuffer = pBuffer;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// Socket状态
        /// </summary>
        public byte[] SocketBuffer
        {
            get
            {
                return this._pBuffer;
            }
        }
        #endregion

        #region 字段定义
        private byte[] _pBuffer;
        #endregion
    }
}
