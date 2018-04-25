using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Windows.Network
{
    #region ISocketService
    /// <summary>
    /// Socket����ӿ�
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
    /// Socket ���ӻ����ӿ�
    /// </summary>
    public interface ISocketConnection
    {
        #region ��������
        /// <summary>
        /// �Ͽ�����
        /// </summary>
        void OnDisconnect();

        /// <summary>
        /// ��������
        /// </summary>
        void OnRecive();

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="pBuffer"></param>
        void OnSend(byte[] pBuffer);

        /// <summary>
        /// �ͻ�������
        /// </summary>
        /// <returns></returns>
        IClientConnection ClientConnection();

        /// <summary>
        /// ���������
        /// </summary>
        /// <returns></returns>
        IServerConnection ServerConnection();
        #endregion

        #region ���Զ���
        /// <summary>
        /// Socket�ỰID(GUID).
        /// </summary>
        string Session
        {
            get;
        }

        /// <summary>
        /// Socket���
        /// </summary>
        IntPtr SocketHandle
        {
            get;
        }

        /// <summary>
        /// Socket��ַ
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
    /// Socket �ͻ�����չ�ӿ�
    /// </summary>
    public interface IClientConnection : ISocketConnection
    {
        void ReConnect();
    }
    #endregion

    #region ServerConnection
    /// <summary>
    /// Socket �������չ�ӿ�
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
