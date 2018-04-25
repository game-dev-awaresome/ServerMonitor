using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Windows.Network
{
    #region ISocketService
    /// <summary>
    /// Socket服务接口
    /// </summary>
    public interface ISocketService
    {
        void OnConnected(SocketEventArgs pEventArgs);
        void OnDisconnected(SocketEventArgs pEventArgs);
        void OnException(SocketEventArgs pEventArgs);

        void OnSent(MessageEventArgs pEventArgs);
        void OnReceived(MessageEventArgs pEventArgs);
    }
    #endregion

    #region ISocketConnection
    #region BaseConnection
    /// <summary>
    /// Socket 连接基本接口
    /// </summary>
    public interface ISocketConnection
    {
        #region 方法定义
        /// <summary>
        /// 断开连接
        /// </summary>
        void OnDisconnect();

        /// <summary>
        /// 接收数据
        /// </summary>
        void OnRecive();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="pBuffer"></param>
        void OnSend(byte[] pBuffer);

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <returns></returns>
        IClientConnection ClientConnection();

        /// <summary>
        /// 服务端连接
        /// </summary>
        /// <returns></returns>
        IServerConnection ServerConnection();
        #endregion

        #region 属性定义
        /// <summary>
        /// Socket会话ID(GUID).
        /// </summary>
        string Session
        {
            get;
        }

        /// <summary>
        /// Socket句柄
        /// </summary>
        IntPtr SocketHandle
        {
            get;
        }

        /// <summary>
        /// Socket地址
        /// </summary>
        IPEndPoint SocketEndPoint
        {
            get;
        }

        CreatorManage CurrentCreator
        {
            get;
        }

        ServiceManage CurrentService
        {
            get;
        }
        #endregion
    }
    #endregion

    #region ClientConnection
    /// <summary>
    /// Socket 客户端扩展接口
    /// </summary>
    public interface IClientConnection : ISocketConnection
    {
        void ReConnect();
    }
    #endregion

    #region ServerConnection
    /// <summary>
    /// Socket 服务端扩展接口
    /// </summary>
    public interface IServerConnection : ISocketConnection
    {
        void SendTo(byte[] pBuffer, bool bAllConnection);
        void SendTo(ISocketConnection pConnectionManage, byte[] pBuffer);

        ISocketConnection[] GetConnection();
        ISocketConnection GetConnection(string strSession);
    }
    #endregion
    #endregion
}
