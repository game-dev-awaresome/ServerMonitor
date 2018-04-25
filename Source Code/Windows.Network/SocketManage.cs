using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Global;

namespace Windows.Network
{
    #region ServiceManage
    /// <summary>
    /// Socket 服务管理类 - *****
    /// </summary>
    public abstract class ServiceManage : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServiceManage(ISocketService SocketService, CompressionType eCompression, int iBufferSize, int iIdleCheckInterval, int iIdleTimeOut)
        {
            this._SocketService = SocketService;
            this._eCompression = eCompression;
            this._iBufferSize = iBufferSize;
            this._iIdleCheckInterval = iIdleCheckInterval;
            this._iIdleTimeOut = iIdleTimeOut;

            this.bActive = false;
            this.pSyncLock = new object();
            this.pSocketConnectionsLock = new ReaderWriterLock();

            this.arrCreators = new List<CreatorManage>();
            this.arrConnections = new Dictionary<string, ConnectionManage>();

            this.pCreatorsResetEvent = new ManualResetEvent(false);
            this.pConnectionsResetEvent = new ManualResetEvent(false);
            this.pThreadsWaitResetEvent = new ManualResetEvent(false);

            if (this._iIdleCheckInterval > 0 && this._iIdleTimeOut > 0)
            {
                this.pIdleCheckTimer = new Timer(new TimerCallback(CheckSocketConnections));
            }
        }

        /// <summary>
        /// 继承方法
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                if (this.pIdleCheckTimer != null)
                {
                    this.pIdleCheckTimer.Dispose();
                    this.pIdleCheckTimer = null;
                }

                if (this.pCreatorsResetEvent != null)
                {
                    this.pCreatorsResetEvent.Set();
                    this.pCreatorsResetEvent.Close();
                    this.pCreatorsResetEvent = null;
                }

                if (this.pConnectionsResetEvent != null)
                {
                    this.pConnectionsResetEvent.Set();
                    this.pConnectionsResetEvent.Close();
                    this.pConnectionsResetEvent = null;
                }

                if (this.pThreadsWaitResetEvent != null)
                {
                    this.pThreadsWaitResetEvent.Set();
                    this.pThreadsWaitResetEvent.Close();
                    this.pThreadsWaitResetEvent = null;
                }

                if (this.arrConnections != null)
                {
                    this.arrConnections.Clear();
                    this.arrConnections = null;
                }

                if (this.arrConnections != null)
                {
                    this.arrConnections.Clear();
                    this.arrConnections = null;
                }

                this._SocketService = null;
                this.pSyncLock = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        #region Timer方法
        private void CheckSocketConnections(Object stateInfo)
        {
            if (!this.Disposed)
            {
                //Console.WriteLine(1000);
                this.pIdleCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

                try
                {
                    ConnectionManage[] pConnectionItems = this.GetConnections();

                    if (pConnectionItems != null)
                    {
                        int iLoopSleep = 101;

                        foreach (ConnectionManage pConnection in pConnectionItems)
                        {
                            if (this.Disposed)
                            {
                                break;
                            }

                            try
                            {
                                if (pConnection != null)
                                {
                                    if (DateTime.Now > (pConnection.LastAction.AddMilliseconds(this._iIdleTimeOut)))
                                    {
                                        pConnection.OnDisconnect();
                                    }
                                }
                            }
                            finally
                            {
                                ThreadLoop.LoopSleep(ref iLoopSleep);
                            }
                        }
                    }
                }
                finally
                {
                    if (!Disposed)
                    {
                        this.pIdleCheckTimer.Change(this._iIdleCheckInterval, this._iIdleCheckInterval);
                    }
                }
            }
        }
        #endregion

        #region 启动/中止服务方法
        /// <summary>
        /// 启动服务
        /// </summary>
        public void ServiceStart()
        {
            if (!this.Disposed)
            {
                int iLoopSleep = 0;

                foreach (CreatorManage pCreator in this.arrCreators)
                {
                    pCreator.CreatorStart();

                    ThreadLoop.LoopSleep(ref iLoopSleep);
                }

                if (this.pIdleCheckTimer != null)
                {
                    this.pIdleCheckTimer.Change(this._iIdleTimeOut, this._iIdleTimeOut);
                }

                this.Active = true;
            }
        }

        /// <summary>
        /// 中止服务
        /// </summary>
        public virtual void ServiceStop()
        {
            this.Active = false;
            this.Dispose();
        }
        #endregion

        #region 管理方法
        protected void AddCreator(CreatorManage pCreator)
        {
            if (!this.Disposed)
            {
                lock (this.arrCreators)
                {
                    this.arrCreators.Add(pCreator);
                }
            }
        }

        protected void RemoveCreator(CreatorManage pCreator)
        {
            if (!this.Disposed)
            {
                lock (this.arrCreators)
                {
                    this.arrCreators.Remove(pCreator);

                    if (this.arrCreators.Count <= 0)
                    {
                        pCreatorsResetEvent.Set();
                    }
                }
            }
        }

        public CreatorManage[] GetSocketCreators()
        {
            CreatorManage[] pCreatorItems = null;

            if (!this.Disposed)
            {
                lock (this.arrCreators)
                {
                    pCreatorItems = new CreatorManage[this.arrCreators.Count];
                    this.arrCreators.CopyTo(pCreatorItems, 0);
                }
            }

            return pCreatorItems;
        }

        /// <summary>
        /// 关闭所有会话
        /// </summary>
        protected void StopCreators()
        {
            CreatorManage[] pCreators = this.GetSocketCreators();

            if (pCreators != null)
            {
                int iLoopCount = 0;
                this.pCreatorsResetEvent.Reset();

                foreach (CreatorManage pCreator in pCreators)
                {
                    try
                    {
                        pCreator.CreatorStop();
                    }
                    finally
                    {
                        this.RemoveCreator(pCreator);
                        pCreator.Dispose();

                        ThreadLoop.LoopSleep(ref iLoopCount);
                    }
                }

                if (pCreators.Length > 0)
                {
                    this.pCreatorsResetEvent.WaitOne(SocketConstant.DEF_RESETEVENT_TIMEOUT, false);
                }
            }
        }

        internal void AddConnection(ConnectionManage pConnection)
        {
            if (!this.Disposed)
            {
                this.pSocketConnectionsLock.AcquireWriterLock(Timeout.Infinite);

                try
                {
                    this.arrConnections.Add(pConnection.Session, pConnection);
                }
                finally
                {
                    this.pSocketConnectionsLock.ReleaseWriterLock();
                }
            }
        }

        internal void RemoveSocketConnection(ConnectionManage pConnection)
        {
            if (!this.Disposed)
            {
                if (pConnection != null)
                {
                    this.pSocketConnectionsLock.AcquireWriterLock(Timeout.Infinite);

                    try
                    {
                        this.arrConnections.Remove(pConnection.Session);
                    }
                    finally
                    {
                        this.pSocketConnectionsLock.ReleaseWriterLock();

                        if (this.arrConnections.Count <= 0)
                        {
                            this.pConnectionsResetEvent.Set();
                        }

                    }
                }
            }
        }

        internal ConnectionManage[] GetConnections()
        {
            ConnectionManage[] pConnectionItems = null;

            if (!this.Disposed)
            {
                this.pSocketConnectionsLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    pConnectionItems = new ConnectionManage[this.arrConnections.Count];
                    this.arrConnections.Values.CopyTo(pConnectionItems, 0);
                }
                finally
                {
                    this.pSocketConnectionsLock.ReleaseReaderLock();
                }
            }

            return pConnectionItems;
        }

        internal ConnectionManage GetConnections(string strSession)
        {

            ConnectionManage pConnectionItem = null;

            if (!this.Disposed)
            {
                this.pSocketConnectionsLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    pConnectionItem = this.arrConnections[strSession];
                }
                finally
                {
                    this.pSocketConnectionsLock.ReleaseReaderLock();
                }

            }

            return pConnectionItem;
        }

        /// <summary>
        /// 关闭所有连接
        /// </summary>
        protected void StopConnections()
        {
            if (!this.Disposed)
            {
                ConnectionManage[] pConnections = this.GetConnections();

                if (pConnections != null)
                {
                    int iLoopSleep = 0;
                    this.pConnectionsResetEvent.Reset();

                    foreach (ConnectionManage pConnection in pConnections)
                    {
                        pConnection.OnDisconnect();
                        ThreadLoop.LoopSleep(ref iLoopSleep);
                    }

                    if (pConnections.Length > 0)
                    {
                        this.pConnectionsResetEvent.WaitOne(SocketConstant.DEF_RESETEVENT_TIMEOUT, false);
                    }
                }
            }
        }

        internal void StopConnections(ref ConnectionManage pConnection)
        {

            if (pConnection != null)
            {
                pConnection.Dispose();
                pConnection = null;
            }

        }
        #endregion

        #region Socket方法
        /// <summary>
        /// 断线方法 - *****
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="DisconnectException"></param>
        internal void SocketDisconnect(ConnectionManage pConnection)
        {
            //判断对象有效
            if (!this.Disposed)
            {
                SocketEventArgs pSocketEventArgs = new SocketEventArgs(pConnection, false);

                if (pConnection.Active)
                {
                    try
                    {
                        //判断操作系统版本号
                        if ((Environment.OSVersion.Version.Major == 5)
                                     && (Environment.OSVersion.Version.Minor >= 1))
                        {
                            //--- NT5 / WinXP and later!
                            pConnection.CurrentSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), pSocketEventArgs);
                        }
                        else
                        {
                            //---- NT5 / Win2000!
                            ThreadPool.QueueUserWorkItem(new WaitCallback(DisconnectProcessing), pSocketEventArgs);
                        }//end Version if...else...
                    }
                    catch (Exception ex)
                    {
                        this.SocketExceptionEvent(pConnection, ex);
                    }
                }
                else
                {
                    this.RemoveSocketConnection(pConnection);
                    this.StopConnections(ref pConnection);
                }
            }//end if         
        }

        /// <summary>
        /// 数据发送方法
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pBuffer"></param>
        internal void SocketSend(ConnectionManage pConnection, byte[] pBuffer)
        {
            //判断对象有效
            if (!this.Disposed)
            {
                try
                {
                    //判断连接有效
                    if (pConnection.Active)
                    {
                        pConnection.LastAction = DateTime.Now;

                        //创建发送缓冲区
                        SocketBuffer pSendBuffer = SocketBuffer.GetPacketBuffer(this._eCompression, ref pBuffer);

                        //为当前连接加锁
                        lock (pConnection.SendQueue)
                        {
                            //判断发送状态
                            if (pConnection.SendState)
                            {
                                //正在发送中
                                pConnection.SendQueue.Enqueue(pSendBuffer);
                            }
                            else
                            {
                                //更改发送状态
                                pConnection.SendState = true;

                                //发送数据流
                                if (pConnection.CurrentStream != null)
                                {
                                    pConnection.CurrentStream.BeginWrite(pSendBuffer.ContentBuffer, pSendBuffer.OffSet, pSendBuffer.Remaining,
                                                    new AsyncCallback(SendCallback), new SocketDataCallback(pConnection, pSendBuffer));
                                }
                                else
                                {
                                    pConnection.CurrentSocket.BeginSend(pSendBuffer.ContentBuffer, pSendBuffer.OffSet, pSendBuffer.Remaining, SocketFlags.None,
                                                    new AsyncCallback(SendCallback), new SocketDataCallback(pConnection, pSendBuffer));
                                }
                            }
                            //end if
                        }//end lock
                    }//end if                    
                }
                catch (Exception ex)
                {
                    this.SocketExceptionEvent(pConnection, ex);
                }//end try...catch...            
            }//end if        
        }

        /// <summary>
        /// 数据接收方法
        /// </summary>
        /// <param name="pConnection"></param>
        internal void SocketRecive(ConnectionManage pConnection)
        {
            //判断对象有效
            if (!this.Disposed)
            {
                try
                {
                    //判断连接有效
                    if (pConnection.Active)
                    {
                        //为当前连接加锁
                        lock (pConnection.ReciveLock)
                        {
                            //读取接收到的数据
                            if (pConnection.ReciveState)
                            {
                                if (pConnection.ReadCount == 0)
                                {
                                    //创建接收缓冲区
                                    SocketBuffer pReciveBuffer = new SocketBuffer(this.SocketBufferSize);

                                    if (pConnection.CurrentStream != null)
                                    {
                                        //从SSL Stream读取数据
                                        pConnection.CurrentStream.BeginRead(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining,
                                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                                    }
                                    else
                                    {
                                        //从Socket Stream读取数据
                                        pConnection.CurrentSocket.BeginReceive(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining, SocketFlags.None,
                                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                                    }//end if
                                }//end if ReadCount

                                pConnection.ReadCount++;
                            }//end if
                        }//end lock                        
                    }
                }
                catch (Exception ex)
                {
                    this.SocketExceptionEvent(pConnection, ex);
                }//end try...catch...
            }// end if
        }
        #endregion

        #region 回调方法
        /// <summary>
        /// 断线回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void DisconnectCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DisconnectProcessing), ar);
            }
        }

        /// <summary>
        /// 数据发送回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendProcessing), ar);
            }
        }

        /// <summary>
        /// 数据接收方法
        /// </summary>
        /// <param name="ar"></param>
        private void ReciveCallback(IAsyncResult ar)
        {
            if (!this.Disposed)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReciveProcessing), ar);
            }
        }
        #endregion

        #region 处理方法
        /// <summary>
        /// 断线处理 - *****
        /// </summary>
        /// <param name="state"></param>
        private void DisconnectProcessing(object state)
        {
            if (!this.Disposed)
            {
                bool bCallDisconnect = false;

                SocketEventArgs pSocketEventArgs = null;
                ConnectionManage pConnection = null;
                IAsyncResult AsyncResult = null;

                try
                {
                    if (state is SocketEventArgs)
                    {
                        //----- NT5 / Win2000!
                        pSocketEventArgs = (SocketEventArgs)state;
                        bCallDisconnect = false;
                    }
                    else
                    {
                        //----- NT5 / WinXP and later!
                        AsyncResult = (IAsyncResult)state;
                        pSocketEventArgs = (SocketEventArgs)AsyncResult.AsyncState;
                        bCallDisconnect = true;
                    }

                    pConnection = (ConnectionManage)pSocketEventArgs.Connection;

                    if (pConnection.Active)
                    {
                        if (bCallDisconnect)
                        {
                            pConnection.CurrentSocket.EndDisconnect(AsyncResult);
                        }

                        lock (pConnection.ActiveLock)
                        {
                            pConnection.Active = false;
                            pConnection.CurrentSocket.Close();
                        }

                        this.SocketDisconnectEvent(pSocketEventArgs);
                    }

                    this.RemoveSocketConnection(pConnection);
                    this.StopConnections(ref pConnection);
                }
                catch (Exception ex)
                {
                    this.SocketExceptionEvent(pConnection, ex);
                }
            }
        }

        /// <summary>
        /// 发送数据处理
        /// </summary>
        /// <param name="state"></param>
        private void SendProcessing(object pSendState)
        {
            if (!this.Disposed)
            {
                bool bCanReadQueue = false;
                SocketBuffer pSendBuffer = null;
                ConnectionManage pConnection = null;

                IAsyncResult AsyncResult = (IAsyncResult)pSendState;

                try
                {
                    SocketDataCallback pCallbackBuffer = (SocketDataCallback)AsyncResult.AsyncState;

                    pConnection = pCallbackBuffer.Connection;
                    pSendBuffer = (SocketBuffer)pCallbackBuffer.SocketBuffer;

                    if (pConnection.Active)
                    {
                        if (pConnection.CurrentStream != null)
                        {
                            pConnection.CurrentStream.EndWrite(AsyncResult);

                            this.SocketSend(pConnection, pSendBuffer.RawBuffer);
                            bCanReadQueue = true;
                        }
                        else
                        {
                            int iWriteBytes = pConnection.CurrentSocket.EndSend(AsyncResult);

                            if (iWriteBytes < pSendBuffer.Remaining)
                            {
                                bCanReadQueue = false;

                                pSendBuffer.OffSet += iWriteBytes;
                                pConnection.CurrentSocket.BeginSend(pSendBuffer.ContentBuffer, pSendBuffer.OffSet, pSendBuffer.Remaining, SocketFlags.None,
                                                new AsyncCallback(SendCallback), pCallbackBuffer);
                            }
                            else
                            {
                                this.SocketSentEvent(pConnection, pSendBuffer.RawBuffer);
                                bCanReadQueue = true;
                            }
                        }//end CurrentStream if...else...

                        if (bCanReadQueue)
                        {
                            pSendBuffer = null;
                            pCallbackBuffer = null;

                            lock (pConnection.SendQueue)
                            {
                                if (pConnection.SendQueue.Count > 0)
                                {
                                    SocketBuffer pQueueBuffer = pConnection.SendQueue.Dequeue();

                                    //发送数据流
                                    if (pConnection.CurrentStream != null)
                                    {
                                        pConnection.CurrentStream.BeginWrite(pQueueBuffer.ContentBuffer, pQueueBuffer.OffSet, pQueueBuffer.Remaining,
                                                        new AsyncCallback(SendCallback), new SocketDataCallback(pConnection, pQueueBuffer));
                                    }
                                    else
                                    {
                                        pConnection.CurrentSocket.BeginSend(pQueueBuffer.ContentBuffer, pQueueBuffer.OffSet, pQueueBuffer.Remaining, SocketFlags.None,
                                                        new AsyncCallback(SendCallback), new SocketDataCallback(pConnection, pQueueBuffer));
                                    }
                                }
                                else
                                {
                                    pConnection.SendState = false;
                                }
                            }//lock SendQueue
                        }//end bCanReadQueue if...else...
                    }//end Active if...else...
                }
                catch (Exception ex)
                {
                    this.SocketExceptionEvent(pConnection, ex);
                }
            }
        }

        /// <summary>
        /// 接收数据处理 -- ***
        /// </summary>
        /// <param name="state"></param>
        private void ReciveProcessing(object state)
        {
            if (!this.Disposed)
            {
                ConnectionManage pConnection = null;

                IAsyncResult AsyncResult = (IAsyncResult)state;

                try
                {
                    SocketDataCallback pCallbackBuffer = (SocketDataCallback)AsyncResult.AsyncState;
                    pConnection = pCallbackBuffer.Connection;

                    if (pConnection.Active)
                    {
                        int iReadBytes = 0;

                        if (pConnection.CurrentStream != null)
                        {
                            iReadBytes = pConnection.CurrentStream.EndRead(AsyncResult);
                        }
                        else
                        {
                            iReadBytes = pConnection.CurrentSocket.EndReceive(AsyncResult);
                        }//end CurrentStream if...else...

                        if (iReadBytes > 0)
                        {
                            bool bReadSocket = false;

                            this.BufferProcessing(pCallbackBuffer, iReadBytes, ref bReadSocket);

                            if (!bReadSocket)
                            {
                                //检索队列
                                lock (pConnection.ReciveLock)
                                {
                                    pConnection.ReadCount--;

                                    if (pConnection.ReadCount > 0)
                                    {
                                        SocketBuffer pNextBuffer = new SocketBuffer(this.SocketBufferSize);

                                        if (pConnection.CurrentStream != null)
                                        {
                                            //从SSL Stream读取数据
                                            pConnection.CurrentStream.BeginRead(pNextBuffer.ContentBuffer, pNextBuffer.OffSet, pNextBuffer.Remaining,
                                                            new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pNextBuffer));
                                        }
                                        else
                                        {
                                            //从Socket Stream读取数据
                                            pConnection.CurrentSocket.BeginReceive(pNextBuffer.ContentBuffer, pNextBuffer.OffSet, pNextBuffer.Remaining, SocketFlags.None,
                                                            new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pNextBuffer));
                                        }//end CurrentStream if...else...
                                    }//end ReadCount if...else...
                                }//end lock
                            }
                        }
                        else
                        {
                            pConnection.OnDisconnect();
                        }//end iReadBytes if...else...
                    }//end Active if...else...
                }
                catch (Exception ex)
                {
                    this.SocketExceptionEvent(pConnection, ex);
                }//end try...catch...
            }
        }

        /// <summary>
        /// 数据处理 - ***
        /// </summary>
        /// <param name="pSocketDataCallback"></param>
        /// <param name="iBytesCount"></param>
        private void BufferProcessing(SocketDataCallback pSocketDataCallback, int iBytesCount, ref bool bReadSocket)
        {
            byte[] pDataBuffer = null;

            ConnectionManage pConnection = pSocketDataCallback.Connection;
            SocketBuffer pReciveBuffer = (SocketBuffer)pSocketDataCallback.SocketBuffer;

            pConnection.LastAction = DateTime.Now;

            //Console.WriteLine(iBytesCount);

            pReciveBuffer.OffSet += iBytesCount;

            //--- 改进的地方
            //读取方向开关
            bool bFromBuffer = false;
            bool bFromSocket = false;

            //确定数据体大小以及读取方向
            do
            {
                //判断当前Buffer大小
                if (pReciveBuffer.OffSet > SocketConstant.MAX_SOCKETHEAD_SIZE)
                {
                    //判断数据体是否填充
                    if (pReciveBuffer.BodySize == 0)
                    {
                        SocketHead pPacketHead = new SocketHead(pReciveBuffer.ContentBuffer);
                        pPacketHead.ExtractInfo();

                        pReciveBuffer.BodySize = pPacketHead.BodyLength;
                        pReciveBuffer.Compression = pPacketHead.Compression;
                        pReciveBuffer.CRCBuffer = pPacketHead.CRCValue;
                    }//end if pReciveBuffer.BodySize

                    //Socket Packet 大小
                    int iBodySize = pReciveBuffer.BodySize + SocketConstant.MAX_SOCKETHEAD_SIZE;

                    //数据读取结束
                    if (iBodySize == pReciveBuffer.OffSet)
                    {
                        pDataBuffer = pReciveBuffer.GetRawBuffer(SocketConstant.MAX_SOCKETHEAD_SIZE, iBodySize);

                        bFromBuffer = false;
                        bFromSocket = false;
                    }
                    else
                    {
                        //从Buffer读取数据
                        if (iBodySize < pReciveBuffer.OffSet)
                        {
                            pDataBuffer = pReciveBuffer.GetRawBuffer(SocketConstant.MAX_SOCKETHEAD_SIZE, iBodySize);

                            bFromBuffer = true;
                            bFromSocket = false;

                            this.SocketReceivedEvent(pConnection, pDataBuffer, false);
                        }
                        else
                        {
                            //从Socket读取数据
                            if (iBodySize > pReciveBuffer.OffSet)
                            {
                                //更改Buffer大小至Socket Packet大小
                                if (iBodySize > pReciveBuffer.CurrentSize)
                                {
                                    pReciveBuffer.Resize(iBodySize);
                                }

                                bFromBuffer = false;
                                bFromSocket = true;
                            }
                        }
                    }//end if (iBodySize == pReciveBuffer.OffSet)
                }
                else
                {
                    if (pReciveBuffer.Remaining < SocketConstant.MAX_SOCKETHEAD_SIZE)
                    {
                        pReciveBuffer.Resize(pReciveBuffer.CurrentSize + SocketConstant.MAX_SOCKETHEAD_SIZE);
                    }

                    bFromBuffer = false;
                    bFromSocket = true;
                }//end if (pReciveBuffer.OffSet > SocketConstant.MAX_SOCKETHEAD_SIZE)
            } while (bFromBuffer);

            //从Socket读取数据
            if (bFromSocket)
            {
                if (pConnection.Active)
                {
                    if (pConnection.CurrentStream != null)
                    {
                        //从SSL Stream读取数据
                        pConnection.CurrentStream.BeginRead(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining,
                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                    }
                    else
                    {
                        //从Socket Stream读取数据
                        pConnection.CurrentSocket.BeginReceive(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining, SocketFlags.None,
                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                    }
                }
            }

            if (null != pDataBuffer)
            {
                pDataBuffer = CompressionUtilise.DeCompressionData(pReciveBuffer.Compression, pDataBuffer);

                this.SocketReceivedEvent(pConnection, pDataBuffer, true);
            }//end pDataBuffer if...else...

            bReadSocket = bFromSocket;
            pReciveBuffer = null;
            pSocketDataCallback = null;
        }
        #endregion

        #region Event方法
        /// <summary>
        /// 连线事件
        /// </summary>
        /// <param name="pConnection"></param>
        internal void SocketConnectedEvent(ConnectionManage pConnection)
        {
            if (pConnection.Active)
            {
                this._SocketService.OnConnected(new SocketEventArgs(pConnection));
            }
        }

        /// <summary>
        /// 断线事件
        /// </summary>
        /// <param name="DisconnectedEvent"></param>
        internal void SocketDisconnectEvent(SocketEventArgs pDisconnectedEvent)
        {
            this._SocketService.OnDisconnected(pDisconnectedEvent);
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pBuffer"></param>
        /// <param name="bCanEnqueue"></param>
        private void SocketReceivedEvent(ConnectionManage pConnection, byte[] pBuffer, bool bCanEnqueue)
        {
            if (pConnection.Active)
            {
                if (!bCanEnqueue)
                {
                    lock (pConnection.ReciveLock)
                    {
                        pConnection.ReciveState = false;
                    }
                }

                this._SocketService.OnReceived(new MessageEventArgs(pConnection, pBuffer));

                if (!bCanEnqueue)
                {
                    lock (pConnection.ReciveLock)
                    {
                        pConnection.ReciveState = true;
                    }
                }
            }
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pBuffer"></param>
        private void SocketSentEvent(ConnectionManage pConnection, byte[] pBuffer)
        {
            if (pConnection.Active)
            {
                this._SocketService.OnSent(new MessageEventArgs(pConnection, pBuffer));
            }
        }

        /// <summary>
        /// Socket异常
        /// </summary>
        /// <param name="ex">异常信息</param>
        internal void SocketExceptionEvent(ConnectionManage pConnection, Exception ex)
        {
            this.SocketExceptionEvent(pConnection, ex, false);
        }

        /// <summary>
        /// Socket异常
        /// </summary>
        /// <param name="ex">异常信息</param>
        internal void SocketExceptionEvent(ConnectionManage pConnection, Exception ex, bool bForceEvent)
        {
            if (bForceEvent || pConnection.Active)
            {
                this._SocketService.OnException(new SocketEventArgs(pConnection, ex));
            }
        }
        #endregion

        #region 抽象方法
        internal abstract void ReConnect(ClientConnect pConnection);
        internal abstract void SendTo(ServerConnect pServerConnect, byte[] pBuffer, bool bAllConnection);
        internal abstract void SendTo(ConnectionManage pConnectionManage, byte[] pBuffer);
        internal abstract ConnectionManage[] GetConnection();
        internal abstract ConnectionManage GetConnection(string strSession);
        #endregion
        #endregion

        #region 属性定义
        public bool Active
        {
            get
            {
                if (Disposed)
                {
                    return false;
                }

                lock (this.pSyncLock)
                {
                    return this.bActive;
                }
            }
            internal set
            {
                lock (this.pSyncLock)
                {
                    this.bActive = value;
                }
            }
        }

        internal int SocketBufferSize
        {
            get
            {
                return this._iBufferSize;
            }
        }

        protected ISocketService SocketService
        {
            get
            {
                return this._SocketService;
            }
        }
        #endregion

        #region 字段定义
        private ISocketService _SocketService;
        private int _iBufferSize;
        private int _iIdleCheckInterval;
        private int _iIdleTimeOut;
        private CompressionType _eCompression;

        private bool bActive;
        private object pSyncLock;
        private Timer pIdleCheckTimer;
        private ManualResetEvent pCreatorsResetEvent;
        private ManualResetEvent pConnectionsResetEvent;
        private ManualResetEvent pThreadsWaitResetEvent;
        private ReaderWriterLock pSocketConnectionsLock;
        private List<CreatorManage> arrCreators;
        private Dictionary<string, ConnectionManage> arrConnections;
        #endregion
    }
    #endregion

    #region ConnectionManage
    /// <summary>
    /// Socket 连接管理类
    /// </summary>
    public abstract class ConnectionManage : CustomClass, ISocketConnection
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pSocket">Socket类</param>
        /// <param name="pManagement">Socket管理类</param>
        /// <param name="eEncryptType">加密方式</param>
        /// <param name="eCompressionType">压缩方式</param>
        public ConnectionManage(Socket pSocket, CreatorManage pCreator, ServiceManage pService)
        {
            //赋值
            this._pSocket = pSocket;
            this._pCreator = pCreator;
            this._pService = pService;

            //初始化变量
            //--连接状态
            this.bConnectActive = false;
            this.pConnectActive = new object();
            //--接收状态
            this.iReciveCount = 0;
            this.bReciveState = true;
            this.pReciveLock = new object();
            //--发送状态
            this.bSendState = false;
            this.pSendItems = new Queue<SocketBuffer>();

            //创建会话ID
            this.strSession = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            //用户活动时间
            this.pActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 继承方法
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                //清空消息队列
                if (this.pSendItems != null)
                {
                    this.pSendItems.Clear();
                    this.pSendItems = null;
                }

                //释放SSL数据流
                if (this.pStream != null)
                {
                    this.pStream.Close();
                    this.pStream.Dispose();
                    this.pStream = null;
                }

                //释放Socket资源
                if (this._pSocket != null)
                {
                    this._pSocket.Close();
                    this._pSocket = null;
                }

                //释放其他资源
                this._pService = null;
                this._pCreator = null;
                this.pReciveLock = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 连接状态锁
        /// </summary>
        internal object ActiveLock
        {
            get
            {
                return this.pConnectActive;
            }
        }

        /// <summary>
        /// 接收锁
        /// </summary>
        internal object ReciveLock
        {
            get
            {
                return this.pReciveLock;
            }
        }

        /// <summary>
        /// 当前Socket
        /// </summary>
        internal Socket CurrentSocket
        {
            get
            {
                return this._pSocket;
            }
        }

        /// <summary>
        /// 发送队列
        /// </summary>
        internal Queue<SocketBuffer> SendQueue
        {
            get
            {
                return this.pSendItems;
            }
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        internal bool Active
        {
            get
            {
                if (this.Disposed)
                {
                    return false;
                }

                lock (this.pConnectActive)
                {
                    return this.bConnectActive;
                }
            }
            set
            {
                lock (this.pConnectActive)
                {
                    this.bConnectActive = value;
                }
            }
        }

        /// <summary>
        /// 接收状态
        /// </summary>
        internal bool ReciveState
        {
            get
            {
                return this.bReciveState;
            }
            set
            {
                this.bReciveState = value;
            }
        }

        /// <summary>
        /// 发送状态
        /// </summary>
        internal bool SendState
        {
            get
            {
                return this.bSendState;
            }
            set
            {
                this.bSendState = value;
            }
        }

        /// <summary>
        /// 读取次数
        /// </summary>
        internal int ReadCount
        {
            get
            {
                return this.iReciveCount;
            }
            set
            {
                this.iReciveCount = value;
            }
        }

        /// <summary>
        /// 最后一次事件触发时间
        /// </summary>
        internal DateTime LastAction
        {
            get
            {
                return this.pActiveTime;
            }
            set
            {
                this.pActiveTime = value;
            }
        }

        /// <summary>
        /// 当前流
        /// </summary>
        internal Stream CurrentStream
        {
            get
            {
                return this.pStream;
            }
            set
            {
                this.pStream = value;
            }
        }
        #endregion

        #region 私有定义
        //传入参数
        private Socket _pSocket;
        private CreatorManage _pCreator;
        private ServiceManage _pService;

        //内部定义
        private string strSession;
        private Stream pStream;

        //连接状态
        private bool bConnectActive;
        private object pConnectActive;
        private DateTime pActiveTime;

        //接收定义
        private bool bReciveState;
        private int iReciveCount;
        private object pReciveLock;

        //发送定义
        private bool bSendState;
        private Queue<SocketBuffer> pSendItems;
        #endregion

        #region ISocketConnection Members

        public void OnDisconnect()
        {
            this._pService.SocketDisconnect(this);
        }

        public void OnRecive()
        {
            this._pService.SocketRecive(this);
        }

        public void OnSend(byte[] pBuffer)
        {
            this._pService.SocketSend(this, pBuffer);
        }

        public string Session
        {
            get
            {
                return this.strSession;
            }
        }

        public IntPtr SocketHandle
        {
            get
            {
                return this._pSocket.Handle;
            }
        }

        public IPEndPoint SocketEndPoint
        {
            get
            {
                return (IPEndPoint)this._pSocket.RemoteEndPoint;
            }
        }

        public CreatorManage CurrentCreator
        {
            get
            {
                return this._pCreator;
            }
        }

        public ServiceManage CurrentService
        {
            get
            {
                return this._pService;
            }
        }

        #region 抽象方法
        public abstract IClientConnection ClientConnection();
        public abstract IServerConnection ServerConnection();
        #endregion

        #endregion
    }
    #endregion

    #region CreatorManage
    /// <summary>
    /// Socket 创建管理类
    /// </summary>
    public abstract class CreatorManage : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pService"></param>
        /// <param name="pEndPoint"></param>
        public CreatorManage(ServiceManage pService, IPEndPoint pEndPoint)
        {
            this._pService = pService;
            this._pEndPoint = pEndPoint;
        }

        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                this._pEndPoint = null;
                this._pService = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="pState"></param>
        protected virtual void Initialize(object pState)
        {
            //判断对象有效
            if (!this.Disposed)
            {
                ConnectionManage pConnection = (ConnectionManage)pState;

                this._pService.SocketConnectedEvent(pConnection);
            }
        }
        #endregion

        #region 抽象方法定义
        internal abstract void CreatorStart();
        internal abstract void CreatorStop();
        #endregion

        #region 属性定义
        /// <summary>
        /// Socket地址
        /// </summary>
        public IPEndPoint EndPoint
        {
            get
            {
                return this._pEndPoint;
            }
        }

        /// <summary>
        /// Socket服务管理
        /// </summary>
        public ServiceManage Service
        {
            get
            {
                return this._pService;
            }
        }
        #endregion

        #region 字段定义
        private ServiceManage _pService;
        private IPEndPoint _pEndPoint;
        #endregion
    }
    #endregion
}
