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
    /// Socket ��������� - *****
    /// </summary>
    public abstract class ServiceManage : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
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
        /// �̳з���
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

        #region ��������
        #region Timer����
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

        #region ����/��ֹ���񷽷�
        /// <summary>
        /// ��������
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
        /// ��ֹ����
        /// </summary>
        public virtual void ServiceStop()
        {
            this.Active = false;
            this.Dispose();
        }
        #endregion

        #region ������
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
        /// �ر����лỰ
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
        /// �ر���������
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

        #region Socket����
        /// <summary>
        /// ���߷��� - *****
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="DisconnectException"></param>
        internal void SocketDisconnect(ConnectionManage pConnection)
        {
            //�ж϶�����Ч
            if (!this.Disposed)
            {
                SocketEventArgs pSocketEventArgs = new SocketEventArgs(pConnection, false);

                if (pConnection.Active)
                {
                    try
                    {
                        //�жϲ���ϵͳ�汾��
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
        /// ���ݷ��ͷ���
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pBuffer"></param>
        internal void SocketSend(ConnectionManage pConnection, byte[] pBuffer)
        {
            //�ж϶�����Ч
            if (!this.Disposed)
            {
                try
                {
                    //�ж�������Ч
                    if (pConnection.Active)
                    {
                        pConnection.LastAction = DateTime.Now;

                        //�������ͻ�����
                        SocketBuffer pSendBuffer = SocketBuffer.GetPacketBuffer(this._eCompression, ref pBuffer);

                        //Ϊ��ǰ���Ӽ���
                        lock (pConnection.SendQueue)
                        {
                            //�жϷ���״̬
                            if (pConnection.SendState)
                            {
                                //���ڷ�����
                                pConnection.SendQueue.Enqueue(pSendBuffer);
                            }
                            else
                            {
                                //���ķ���״̬
                                pConnection.SendState = true;

                                //����������
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
        /// ���ݽ��շ���
        /// </summary>
        /// <param name="pConnection"></param>
        internal void SocketRecive(ConnectionManage pConnection)
        {
            //�ж϶�����Ч
            if (!this.Disposed)
            {
                try
                {
                    //�ж�������Ч
                    if (pConnection.Active)
                    {
                        //Ϊ��ǰ���Ӽ���
                        lock (pConnection.ReciveLock)
                        {
                            //��ȡ���յ�������
                            if (pConnection.ReciveState)
                            {
                                if (pConnection.ReadCount == 0)
                                {
                                    //�������ջ�����
                                    SocketBuffer pReciveBuffer = new SocketBuffer(this.SocketBufferSize);

                                    if (pConnection.CurrentStream != null)
                                    {
                                        //��SSL Stream��ȡ����
                                        pConnection.CurrentStream.BeginRead(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining,
                                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                                    }
                                    else
                                    {
                                        //��Socket Stream��ȡ����
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

        #region �ص�����
        /// <summary>
        /// ���߻ص�����
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
        /// ���ݷ��ͻص�����
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
        /// ���ݽ��շ���
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

        #region ������
        /// <summary>
        /// ���ߴ��� - *****
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
        /// �������ݴ���
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

                                    //����������
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
        /// �������ݴ��� -- ***
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
                                //��������
                                lock (pConnection.ReciveLock)
                                {
                                    pConnection.ReadCount--;

                                    if (pConnection.ReadCount > 0)
                                    {
                                        SocketBuffer pNextBuffer = new SocketBuffer(this.SocketBufferSize);

                                        if (pConnection.CurrentStream != null)
                                        {
                                            //��SSL Stream��ȡ����
                                            pConnection.CurrentStream.BeginRead(pNextBuffer.ContentBuffer, pNextBuffer.OffSet, pNextBuffer.Remaining,
                                                            new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pNextBuffer));
                                        }
                                        else
                                        {
                                            //��Socket Stream��ȡ����
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
        /// ���ݴ��� - ***
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

            //--- �Ľ��ĵط�
            //��ȡ���򿪹�
            bool bFromBuffer = false;
            bool bFromSocket = false;

            //ȷ���������С�Լ���ȡ����
            do
            {
                //�жϵ�ǰBuffer��С
                if (pReciveBuffer.OffSet > SocketConstant.MAX_SOCKETHEAD_SIZE)
                {
                    //�ж��������Ƿ����
                    if (pReciveBuffer.BodySize == 0)
                    {
                        SocketHead pPacketHead = new SocketHead(pReciveBuffer.ContentBuffer);
                        pPacketHead.ExtractInfo();

                        pReciveBuffer.BodySize = pPacketHead.BodyLength;
                        pReciveBuffer.Compression = pPacketHead.Compression;
                        pReciveBuffer.CRCBuffer = pPacketHead.CRCValue;
                    }//end if pReciveBuffer.BodySize

                    //Socket Packet ��С
                    int iBodySize = pReciveBuffer.BodySize + SocketConstant.MAX_SOCKETHEAD_SIZE;

                    //���ݶ�ȡ����
                    if (iBodySize == pReciveBuffer.OffSet)
                    {
                        pDataBuffer = pReciveBuffer.GetRawBuffer(SocketConstant.MAX_SOCKETHEAD_SIZE, iBodySize);

                        bFromBuffer = false;
                        bFromSocket = false;
                    }
                    else
                    {
                        //��Buffer��ȡ����
                        if (iBodySize < pReciveBuffer.OffSet)
                        {
                            pDataBuffer = pReciveBuffer.GetRawBuffer(SocketConstant.MAX_SOCKETHEAD_SIZE, iBodySize);

                            bFromBuffer = true;
                            bFromSocket = false;

                            this.SocketReceivedEvent(pConnection, pDataBuffer, false);
                        }
                        else
                        {
                            //��Socket��ȡ����
                            if (iBodySize > pReciveBuffer.OffSet)
                            {
                                //����Buffer��С��Socket Packet��С
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

            //��Socket��ȡ����
            if (bFromSocket)
            {
                if (pConnection.Active)
                {
                    if (pConnection.CurrentStream != null)
                    {
                        //��SSL Stream��ȡ����
                        pConnection.CurrentStream.BeginRead(pReciveBuffer.ContentBuffer, pReciveBuffer.OffSet, pReciveBuffer.Remaining,
                                        new AsyncCallback(ReciveCallback), new SocketDataCallback(pConnection, pReciveBuffer));
                    }
                    else
                    {
                        //��Socket Stream��ȡ����
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

        #region Event����
        /// <summary>
        /// �����¼�
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
        /// �����¼�
        /// </summary>
        /// <param name="DisconnectedEvent"></param>
        internal void SocketDisconnectEvent(SocketEventArgs pDisconnectedEvent)
        {
            this._SocketService.OnDisconnected(pDisconnectedEvent);
        }

        /// <summary>
        /// �����¼�
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
        /// �����¼�
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
        /// Socket�쳣
        /// </summary>
        /// <param name="ex">�쳣��Ϣ</param>
        internal void SocketExceptionEvent(ConnectionManage pConnection, Exception ex)
        {
            this.SocketExceptionEvent(pConnection, ex, false);
        }

        /// <summary>
        /// Socket�쳣
        /// </summary>
        /// <param name="ex">�쳣��Ϣ</param>
        internal void SocketExceptionEvent(ConnectionManage pConnection, Exception ex, bool bForceEvent)
        {
            if (bForceEvent || pConnection.Active)
            {
                this._SocketService.OnException(new SocketEventArgs(pConnection, ex));
            }
        }
        #endregion

        #region ���󷽷�
        internal abstract void ReConnect(ClientConnect pConnection);
        internal abstract void SendTo(ServerConnect pServerConnect, byte[] pBuffer, bool bAllConnection);
        internal abstract void SendTo(ConnectionManage pConnectionManage, byte[] pBuffer);
        internal abstract ConnectionManage[] GetConnection();
        internal abstract ConnectionManage GetConnection(string strSession);
        #endregion
        #endregion

        #region ���Զ���
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

        #region �ֶζ���
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
    /// Socket ���ӹ�����
    /// </summary>
    public abstract class ConnectionManage : CustomClass, ISocketConnection
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="pSocket">Socket��</param>
        /// <param name="pManagement">Socket������</param>
        /// <param name="eEncryptType">���ܷ�ʽ</param>
        /// <param name="eCompressionType">ѹ����ʽ</param>
        public ConnectionManage(Socket pSocket, CreatorManage pCreator, ServiceManage pService)
        {
            //��ֵ
            this._pSocket = pSocket;
            this._pCreator = pCreator;
            this._pService = pService;

            //��ʼ������
            //--����״̬
            this.bConnectActive = false;
            this.pConnectActive = new object();
            //--����״̬
            this.iReciveCount = 0;
            this.bReciveState = true;
            this.pReciveLock = new object();
            //--����״̬
            this.bSendState = false;
            this.pSendItems = new Queue<SocketBuffer>();

            //�����ỰID
            this.strSession = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            //�û��ʱ��
            this.pActiveTime = DateTime.Now;
        }

        /// <summary>
        /// �̳з���
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                //�����Ϣ����
                if (this.pSendItems != null)
                {
                    this.pSendItems.Clear();
                    this.pSendItems = null;
                }

                //�ͷ�SSL������
                if (this.pStream != null)
                {
                    this.pStream.Close();
                    this.pStream.Dispose();
                    this.pStream = null;
                }

                //�ͷ�Socket��Դ
                if (this._pSocket != null)
                {
                    this._pSocket.Close();
                    this._pSocket = null;
                }

                //�ͷ�������Դ
                this._pService = null;
                this._pCreator = null;
                this.pReciveLock = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ����״̬��
        /// </summary>
        internal object ActiveLock
        {
            get
            {
                return this.pConnectActive;
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        internal object ReciveLock
        {
            get
            {
                return this.pReciveLock;
            }
        }

        /// <summary>
        /// ��ǰSocket
        /// </summary>
        internal Socket CurrentSocket
        {
            get
            {
                return this._pSocket;
            }
        }

        /// <summary>
        /// ���Ͷ���
        /// </summary>
        internal Queue<SocketBuffer> SendQueue
        {
            get
            {
                return this.pSendItems;
            }
        }

        /// <summary>
        /// ����״̬
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
        /// ����״̬
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
        /// ����״̬
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
        /// ��ȡ����
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
        /// ���һ���¼�����ʱ��
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
        /// ��ǰ��
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

        #region ˽�ж���
        //�������
        private Socket _pSocket;
        private CreatorManage _pCreator;
        private ServiceManage _pService;

        //�ڲ�����
        private string strSession;
        private Stream pStream;

        //����״̬
        private bool bConnectActive;
        private object pConnectActive;
        private DateTime pActiveTime;

        //���ն���
        private bool bReciveState;
        private int iReciveCount;
        private object pReciveLock;

        //���Ͷ���
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

        #region ���󷽷�
        public abstract IClientConnection ClientConnection();
        public abstract IServerConnection ServerConnection();
        #endregion

        #endregion
    }
    #endregion

    #region CreatorManage
    /// <summary>
    /// Socket ����������
    /// </summary>
    public abstract class CreatorManage : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
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

        #region ��������
        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="pState"></param>
        protected virtual void Initialize(object pState)
        {
            //�ж϶�����Ч
            if (!this.Disposed)
            {
                ConnectionManage pConnection = (ConnectionManage)pState;

                this._pService.SocketConnectedEvent(pConnection);
            }
        }
        #endregion

        #region ���󷽷�����
        internal abstract void CreatorStart();
        internal abstract void CreatorStop();
        #endregion

        #region ���Զ���
        /// <summary>
        /// Socket��ַ
        /// </summary>
        public IPEndPoint EndPoint
        {
            get
            {
                return this._pEndPoint;
            }
        }

        /// <summary>
        /// Socket�������
        /// </summary>
        public ServiceManage Service
        {
            get
            {
                return this._pService;
            }
        }
        #endregion

        #region �ֶζ���
        private ServiceManage _pService;
        private IPEndPoint _pEndPoint;
        #endregion
    }
    #endregion
}
