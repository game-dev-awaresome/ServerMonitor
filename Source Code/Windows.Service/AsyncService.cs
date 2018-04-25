using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using System.Global;
using Windows.Network;

namespace Windows.Service
{
    /// <summary>
    /// 异步服务
    /// </summary>
    public class AsyncService : SocketService
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public AsyncService()
        {
            this._pSocketEvent = null;
            this.iMaxClient = 1000;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pEvent">事件</param>
        public AsyncService(NetServiceType eNetService, SocketEvent pEvent)
        {
            this._eNetService = eNetService;
            this._pSocketEvent = pEvent;
            this.iMaxClient = 1000;
        }

        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region 方法定义
        //public void StarService(CompressionType eCompression, int iMaxClient, int iPort, int iSocketBuffer)
        public void StarService(IPEndPoint pEndPoint, CompressionType eCompression, int iSocketBuffer)
        {
            //IPEndPoint pServerPoint = new IPEndPoint(0, iPort);

            //pSocketServer = new SocketServer(this, eCompression, iSocketBuffer, 1, 1);
            //pSocketServer.AddListener(pServerPoint, iMaxClient);

            //pSocketServer.ServiceStart();

            switch (this._eNetService)
            {
                case NetServiceType.HOST:
                    pSocketServer = new SocketServer(this, eCompression, iSocketBuffer, this.iCheckInterval, this.iCheckTimeOut);
                    pSocketServer.AddListener(pEndPoint, this.iMaxClient);

                    pSocketServer.ServiceStart();
                    break;
                case NetServiceType.CLIENT:
                    pSocketClient = new AsyncClient(this, eCompression, iSocketBuffer, this.iCheckInterval, this.iCheckTimeOut);
                    pSocketClient.AddConnector(pEndPoint);

                    pSocketClient.ServiceStart();
                    break;
            }
        }

        private void ServiceEvent(object pObject)
        {
            if (this._pSocketEvent != null)
            {
                this._pSocketEvent.Invoke(pObject);
            }
        }

        public void StopService()
        {
            //pSocketServer.ServiceStop();
            switch (this._eNetService)
            {
                case NetServiceType.HOST:
                    pSocketServer.ServiceStop();
                    break;
                case NetServiceType.CLIENT:
                    pSocketClient.ServiceStop();
                    break;
            }

            this.ServiceEvent("Disconnected!");
        }
        #endregion

        #region 重载方法
        public override void OnConnected(SocketEventArgs pEventArgs)
        {
            this.SocketConnect = pEventArgs.Connection;
            this.ServiceEvent(pEventArgs);
        }

        public override void OnDisconnected(SocketEventArgs pEventArgs)
        {
            this.ServiceEvent(pEventArgs);
        }

        public override void OnException(SocketEventArgs pEventArgs)
        {
            this.ServiceEvent(pEventArgs);
        }

        public override void OnReceived(MessageEventArgs pEventArgs)
        {
            this.ServiceEvent(pEventArgs);
        }

        public override void OnSent(MessageEventArgs pEventArgs)
        {
            pEventArgs.Connection.OnRecive();
        }
        #endregion

        #region
        public ISocketConnection SocketConnected
        {
            get
            {
                return this.SocketConnect;
            }
        } 
        #endregion

        #region 属性定义
        /// <summary>
        /// 最大客户端数
        /// </summary>
        public int MaxClient
        {
            get
            {
                return this.iMaxClient;
            }
            set
            {
                this.iMaxClient = value;
            }
        }

        public int CheckInterval
        {
            get
            {
                return this.iCheckInterval;
            }
            set
            {
                this.iCheckInterval = value;
            }
        }

        public int CheckTimeOut
        {
            get
            {
                return this.iCheckTimeOut;
            }
            set
            {
                this.iCheckTimeOut = value;
            }
        }
        #endregion

        #region 变量定义
        private SocketEvent _pSocketEvent;
        private SocketServer pSocketServer;
        private ISocketConnection SocketConnect;

        private NetServiceType _eNetService;
        private int iMaxClient;
        private int iCheckInterval;
        private int iCheckTimeOut;
        private AsyncClient pSocketClient;
        #endregion
    }
}
