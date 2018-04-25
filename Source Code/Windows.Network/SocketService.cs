using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Global;

namespace Windows.Network
{
    #region SocketService 抽象定义
    public abstract class SocketService : ISocketService
    {
        #region ISocketService Members

        public virtual void OnConnected(SocketEventArgs pEventArgs) { }
        public virtual void OnDisconnected(SocketEventArgs pEventArgs) { }
        public virtual void OnException(SocketEventArgs pEventArgs) { }
        public virtual void OnSent(MessageEventArgs pEventArgs) { }
        public virtual void OnReceived(MessageEventArgs pEventArgs) { }

        #endregion
    }
    #endregion

    #region Socket Listen Service
    #region SocketServer定义
    public class SocketServer : ServiceManage
    {
        #region 构造/析构函数
        public SocketServer(ISocketService SocketService, CompressionType eCompression)
            : base(SocketService, eCompression, 4096, 0, 0) { }

        public SocketServer(ISocketService SocketService, CompressionType eCompression, int iSocketBuffer)
            : base(SocketService, eCompression, iSocketBuffer, 0, 0) { }

        public SocketServer(ISocketService SocketService, CompressionType eCompression, int iSocketBuffer, int iIdleCheckInterval, int iIdleTimeOut)
            : base(SocketService, eCompression, iSocketBuffer, iIdleCheckInterval, iIdleTimeOut) { }
        #endregion

        #region 方法定义
        public void AddListener(int iListenPort)
        {
            IPEndPoint pLocalPoint = new IPEndPoint(0, iListenPort);

            this.AddListener(pLocalPoint, 5, 2);
        }

        public void AddListener(int iListenPort, int iMaxConnection)
        {
            IPEndPoint pLocalPoint = new IPEndPoint(0, iListenPort);

            this.AddListener(pLocalPoint, iMaxConnection, 2);
        }

        public void AddListener(int iListenPort, int iMaxConnection, int iAcceptThread)
        {
            IPEndPoint pLocalPoint = new IPEndPoint(0, iListenPort);

            this.AddListener(pLocalPoint, iMaxConnection, iAcceptThread);
        }

        public void AddListener(IPEndPoint pEndPoint)
        {
            this.AddListener(pEndPoint, 5, 2);
        }

        public void AddListener(IPEndPoint pEndPoint, int iMaxConnection)
        {
            this.AddListener(pEndPoint, iMaxConnection, 2);
        }

        public void AddListener(IPEndPoint pEndPoint, int iMaxConnection, int iAcceptThread)
        {
            if (!this.Disposed)
            {
                this.AddCreator(new SocketListener(this, pEndPoint, iMaxConnection, iAcceptThread));
            }
        }
        #endregion

        #region 重载方法
        /// <summary>
        /// 停止服务
        /// </summary>
        public override void ServiceStop()
        {
            if (!Disposed)
            {
                this.StopCreators();
                this.StopConnections();

            }

            base.ServiceStop();
        }

        internal override void ReConnect(ClientConnect pConnection) { }

        internal override void SendTo(ServerConnect pServerConnect, byte[] pBuffer, bool bAllConnection)
        {
            if (!this.Disposed)
            {
                ConnectionManage[] pConnectionItmes = this.GetConnections();

                if (pConnectionItmes != null)
                {
                    int iLoopSleep = 0;

                    foreach (ConnectionManage pConnection in pConnectionItmes)
                    {
                        if (this.Disposed)
                        {
                            break;
                        }

                        try
                        {
                            if (bAllConnection || pServerConnect != pConnection)
                            {
                                byte[] pSendBuffer = new byte[pBuffer.Length];
                                Buffer.BlockCopy(pBuffer, 0, pSendBuffer, 0, pBuffer.Length);

                                this.SendTo(pConnection, pSendBuffer);
                            }
                        }
                        finally
                        {
                            ThreadLoop.LoopSleep(ref iLoopSleep);
                        }//end try...finally...
                    }//end foreach(...)
                }//end if(...)
            }//end if(...)
        }

        internal override void SendTo(ConnectionManage pConnectionManage, byte[] pBuffer)
        {
            if (!this.Disposed)
            {
                //this.SendTo(pConnectionManage, pBuffer);
                this.SocketSend(pConnectionManage, pBuffer);
            }
        }

        internal override ConnectionManage[] GetConnection()
        {
            return this.GetConnections();
        }

        internal override ConnectionManage GetConnection(string strSession)
        {
            ConnectionManage pConnectionManage = null;

            if (!Disposed)
            {
                pConnectionManage = this.GetConnections(strSession);
            }

            return pConnectionManage;
        }
        #endregion
    }
    #endregion

    #region SocketListener 定义
    internal class SocketListener : CreatorManage
    {
        #region 构造/析构函数
        public SocketListener(ServiceManage pService, IPEndPoint pEndPoint, int iMaxConnection, int iAcceptThread)
            : base(pService, pEndPoint)
        {
            this._iMaxConnection = iMaxConnection;
            this._iAcceptThread = iAcceptThread;
        }

        protected override void Free(bool DispodedByUser)
        {
            if (DispodedByUser)
            {
                this.pSocket.Close();
                this.pSocket = null;
            }

            base.Free(DispodedByUser);
        }
        #endregion

        #region 回调方法
        internal void SocketAcceptCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                Socket pAcceptedSocket = null;
                SocketListener pListener = null;
                ConnectionManage pConnection = null;

                try
                {
                    pListener = (SocketListener)ar.AsyncState;
                    pAcceptedSocket = pListener.CurrentSocket.EndAccept(ar);

                    pAcceptedSocket.ReceiveBufferSize = this.Service.SocketBufferSize;
                    pAcceptedSocket.SendBufferSize = this.Service.SocketBufferSize;

                    pConnection = new ServerConnect(pAcceptedSocket, pListener, this.Service);
                    pConnection.Active = true;

                    this.Service.AddConnection(pConnection);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.Initialize), pConnection);
                }
                catch (Exception ex)
                {
                    this.Service.SocketExceptionEvent(pConnection, ex);
                }
                finally
                {
                    pListener.CurrentSocket.BeginAccept(new AsyncCallback(SocketAcceptCallback), pListener);
                }
            }
        }
        #endregion

        #region 重载方法
        internal override void CreatorStart()
        {
            if (!this.Disposed)
            {
                try
                {
                    int iLoopCount = 101;

                    this.pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.pSocket.Bind(this.EndPoint);
                    this.pSocket.Listen(this._iMaxConnection);

                    for (int i = 1; i <= this._iAcceptThread; i++)
                    {
                        this.pSocket.BeginAccept(new AsyncCallback(SocketAcceptCallback), this);

                        ThreadLoop.LoopSleep(ref iLoopCount);
                    }
                }
                catch
                {
                    throw new Exception("网络地址重复绑定，服务中断");
                }
            }
        }

        internal override void CreatorStop()
        {
            this.Dispose();
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 当前Socket
        /// </summary>
        internal Socket CurrentSocket
        {
            get
            {
                return this.pSocket;
            }
        }
        #endregion

        #region 变量定义
        private int _iMaxConnection;
        private int _iAcceptThread;

        private Socket pSocket;
        #endregion
    }
    #endregion

    #region ServerConnection 定义
    internal class ServerConnect : ConnectionManage, IServerConnection
    {
        #region 构造/析构函数
        internal ServerConnect(Socket pSocket, CreatorManage pCreator, ServiceManage pService)
            : base(pSocket, pCreator, pService) { }
        #endregion

        #region ISocketConnection Members

        public override IClientConnection ClientConnection()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override IServerConnection ServerConnection()
        {
            return (this as IServerConnection);
        }

        #endregion

        #region IServerConnection Members

        public void SendTo(byte[] pBuffer, bool bAllConnection)
        {
            this.CurrentService.SendTo(this, pBuffer, bAllConnection);
        }

        public void SendTo(ISocketConnection pConnectionManage, byte[] pBuffer)
        {
            this.CurrentService.SendTo((ConnectionManage)pConnectionManage, pBuffer);
        }

        public ISocketConnection[] GetConnection()
        {
            return this.CurrentService.GetConnection();
        }

        public ISocketConnection GetConnection(string strSession)
        {
            return this.CurrentService.GetConnection(strSession);
        }

        #endregion
    }
    #endregion
    #endregion

    #region Socket Client Service
    #region AsyncClient 定义
    public class AsyncClient : ServiceManage
    {
        #region 构造/析构函数
        public AsyncClient(ISocketService SocketService, CompressionType eCompression)
            : base(SocketService, eCompression, 4096, 0, 0) { }

        public AsyncClient(ISocketService SocketService, CompressionType eCompression, int iSocketBuffer)
            : base(SocketService, eCompression, iSocketBuffer, 0, 0) { }

        public AsyncClient(ISocketService SocketService, CompressionType eCompression, int iSocketBuffer, int iIdleCheckInterval, int iIdleTimeOut)
            : base(SocketService, eCompression, iSocketBuffer, iIdleCheckInterval, iIdleTimeOut) { }
        #endregion

        #region 方法定义
        public SocketConnector AddConnector(IPEndPoint pEndPoint)
        {
            SocketConnector pConnector = null;

            if (!this.Disposed)
            {
                pConnector = new SocketConnector(this, pEndPoint, 0, 0);
                this.AddCreator(pConnector);
            }
            return pConnector;
        }
        #endregion

        #region 继承方法
        public override void ServiceStop()
        {
            if (!this.Disposed)
            {
                this.StopConnections();
                this.StopCreators();
            }

            base.ServiceStop();
        }

        internal override void ReConnect(ClientConnect pConnection)
        {
            if (!this.Disposed)
            {
                SocketConnector pConnector = (SocketConnector)pConnection.CurrentCreator;

                pConnector.ReConnect(true, null, null);
            }
        }

        #region 无效的方法
        internal override void SendTo(ServerConnect pServerConnect, byte[] pBuffer, bool bAllConnection) { }
        internal override void SendTo(ConnectionManage pConnectionManage, byte[] pBuffer) { }

        internal override ConnectionManage[] GetConnection()        
        {
            return null;
        }

        internal override ConnectionManage GetConnection(string strSession)
        {
            return null;
        }
        #endregion
        #endregion
    }
    #endregion

    #region Socket Connector 定义
    public class SocketConnector : CreatorManage
    {
        #region 构造/析构函数
        public SocketConnector(ServiceManage pService, IPEndPoint pIPEndPoint, int iIdleInterval, int iReConnectCount)
            : base(pService, pIPEndPoint)
        {
            this._iIdleInterval = iIdleInterval;
            this._iAttemptCount = iReConnectCount;

            this.iCurrentCount = -1;
            this.pReConnectTimer = new Timer(new TimerCallback(ReConnectCallBack));
        }

        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                if (this.pReConnectTimer != null)
                {
                    this.pReConnectTimer.Dispose();
                    this.pReConnectTimer = null;
                }

                if (this.pSocket != null)
                {
                    this.pSocket.Close();
                    this.pSocket = null;
                }
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        private void Connecting()
        {
            this.pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.pSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            this.pSocket.BeginConnect(this.EndPoint, new AsyncCallback(ConnectCallback), this);
        }

        internal void ReConnect(bool bResetAttempts, ConnectionManage pConnection, Exception pException)
        {
            if (!this.Disposed)
            {
                if (bResetAttempts)
                {
                    this.iCurrentCount = 0;
                }

                if (this._iAttemptCount > 0)
                {
                    if (this.iCurrentCount < this._iAttemptCount)
                    {
                        this.Service.RemoveSocketConnection(pConnection);
                        this.Service.StopConnections(ref pConnection);

                        this.pReConnectTimer.Change(this._iIdleInterval, this._iIdleInterval);
                    }
                    else
                    {
                        this.CreatorStop();
                        this.Service.SocketExceptionEvent(pConnection, new ArgumentException("尝试连接次数已达到最大值：", "尝试连接次数为【" + this._iAttemptCount.ToString() + "】次"), true);
                    }
                }
            }
            else
            {
                if ((pConnection != null) && (pException != null))
                {
                    this.Service.SocketExceptionEvent(pConnection, pException, true);
                }
            }
        }

        internal void ConnectCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ConnectionManage pConnection = null;
                SocketConnector pConnector = null;

                try
                {
                    pConnector = (SocketConnector)ar.AsyncState;

                    pConnection = new ClientConnect(pConnector.CurrentSocket, this, this.Service);

                    pConnector.CurrentSocket.EndConnect(ar);

                    pConnector.CurrentSocket.ReceiveBufferSize = this.Service.SocketBufferSize;
                    pConnector.CurrentSocket.SendBufferSize = this.Service.SocketBufferSize;

                    pConnection.Active = true;

                    this.Service.AddConnection(pConnection);
                    this.Initialize(pConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SocketException)
                    {
                        this.iCurrentCount++;
                        this.ReConnect(false, pConnection, ex);
                    }
                    else
                    {
                        this.Service.SocketExceptionEvent(pConnection, ex);
                    }
                }
            }
        }

        private void ReConnectCallBack(Object stateInfo)
        {
            if (!this.Disposed)
            {
                this.pReConnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.Connecting();
            }
        }
        #endregion

        #region 继承方法
        internal override void CreatorStart()
        {
            if (!this.Disposed)
            {
                this.Connecting();
            }
        }

        internal override void CreatorStop()
        {
            this.Dispose();
        }

        protected override void Initialize(object pState)
        {
            ConnectionManage pConnectionManage = (ConnectionManage)pState;

            base.Initialize(pConnectionManage);
        }
        #endregion

        #region 属性定义
        public int AttemptCount
        {
            get
            {
                return this._iAttemptCount;
            }
            set
            {
                this._iAttemptCount = value;
            }
        }

        public int Interval
        {
            get
            {
                return this._iIdleInterval;
            }
            set
            {
                this._iIdleInterval = value;
            }
        }

        /// <summary>
        /// 当前Socket
        /// </summary>
        internal Socket CurrentSocket
        {
            get
            {
                return this.pSocket;
            }
        }
        #endregion

        #region 字段定义
        private int _iIdleInterval;
        private int _iAttemptCount;

        private int iCurrentCount;
        private Socket pSocket;
        private Timer pReConnectTimer;
        #endregion
    }
    #endregion

    #region ClientConnect 定义
    internal class ClientConnect : ConnectionManage, IClientConnection
    {
        #region 构造/析构函数
        internal ClientConnect(Socket pSocket, CreatorManage pCreator, ServiceManage pService)
            : base(pSocket, pCreator, pService) { }
        #endregion

        #region ClientConnect 定义
        public override IClientConnection ClientConnection()
        {
            return (this as IClientConnection);
        }

        public override IServerConnection ServerConnection()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        #region IClientConnection Members

        public void ReConnect()
        {
            this.CurrentService.ReConnect(this);
        }

        #endregion
    }
    #endregion
    #endregion
}
