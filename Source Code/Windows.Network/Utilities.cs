using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Global;

namespace Windows.Network
{
    /// <summary>
    /// Socket ���ݻص���
    /// </summary>
    internal class SocketDataCallback
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="pConnectionManage"></param>
        /// <param name="pBuffer"></param>
        public SocketDataCallback(ConnectionManage pConnection, object pSocketBuffer)
        {
            this._pConnection = pConnection;
            this._pSocketBuffer = pSocketBuffer;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// Socket ����
        /// </summary>
        public object SocketBuffer
        {
            get
            {
                return this._pSocketBuffer;
            }
        }

        /// <summary>
        /// ���ӹ���
        /// </summary>
        public ConnectionManage Connection
        {
            get
            {
                return this._pConnection;
            }
        }
        #endregion

        #region �ֶζ���
        private object _pSocketBuffer;
        private ConnectionManage _pConnection;
        #endregion
    }

    /// <summary>
    /// Socket ���ݻ�����
    /// </summary>
    internal class SocketBuffer
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="iLength"></param>
        public SocketBuffer(int iLength)
        {
            this.iOffSet = 0;
            this._pRawBuffer = null;
            this._pContentBuffer = null;

            if (iLength > 0)
            {
                this._pContentBuffer = new byte[iLength];
            }
        }

        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="pRawBuffer"></param>
        /// <param name="pContentBuffer"></param>
        public SocketBuffer(byte[] pRawBuffer, byte[] pContentBuffer)
        {
            this._pRawBuffer = pRawBuffer;
            this._pContentBuffer = pContentBuffer;
            this.iOffSet = 0;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��䱨ͷ
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <returns></returns>
        public static SocketBuffer GetPacketBuffer(CompressionType eCompression, ref byte[] pBuffer)
        {
            byte[] pCompressionBuffer = CompressionUtilise.CompressionData(eCompression, pBuffer);

            SocketHead pPacketHead = new SocketHead(eCompression, pCompressionBuffer);
            byte[] pHeadBuffer = pPacketHead.CoalitionInfo();

            byte[] pSendBuffer = new byte[SocketConstant.MAX_SOCKETHEAD_SIZE + pCompressionBuffer.Length];
            Buffer.BlockCopy(pHeadBuffer, 0, pSendBuffer, 0, pHeadBuffer.Length);
            Buffer.BlockCopy(pCompressionBuffer, 0, pSendBuffer, SocketConstant.MAX_SOCKETHEAD_SIZE, pCompressionBuffer.Length);

            return new SocketBuffer(pBuffer, pSendBuffer);
        }

        /// <summary>
        /// ���������С
        /// </summary>
        /// <param name="iLength">���ݳ���</param>
        public void Resize(int iLength)
        {
            Array.Resize(ref this._pContentBuffer, iLength);
        }

        /// <summary>
        /// ��ȡBuffer����
        /// </summary>
        /// <param name="iHeadSize"></param>
        /// <param name="iBodySize"></param>
        /// <returns></returns>
        public byte[] GetRawBuffer(int iHeadSize, int iMessageSize)
        {
            //��ȡ���
            byte[] pResult = new byte[iMessageSize - iHeadSize];
            Buffer.BlockCopy(this._pContentBuffer, iHeadSize, pResult, 0, pResult.Length);

            //����Buffer��С
            byte[] pBuffer = new byte[this._pContentBuffer.Length - iMessageSize];
            Buffer.BlockCopy(this._pContentBuffer, iHeadSize, pBuffer, 0, pBuffer.Length);

            //����������Ϣ
            this._pContentBuffer = pBuffer;
            this.iOffSet = this.iOffSet - iMessageSize;

            return pResult;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ����ƫ����
        /// </summary>
        public int OffSet
        {
            get
            {
                return this.iOffSet;
            }
            set
            {
                this.iOffSet = value;
            }
        }

        /// <summary>
        /// ����ʣ����
        /// </summary>
        public int Remaining
        {
            get
            {
                return this._pContentBuffer.Length - this.iOffSet;
            }
        }

        /// <summary>
        /// ��ǰ��С
        /// </summary>
        public int CurrentSize
        {
            get
            {
                return this._pContentBuffer.Length;
            }
        }

        /// <summary>
        /// �������С -- δ֪����
        /// </summary>
        public int BodySize
        {
            get
            {
                return this.iBodySize;
            }
            set
            {
                this.iBodySize = value;
            }
        }

        /// <summary>
        /// CRCֵ
        /// </summary>
        public byte[] CRCBuffer
        {
            get
            {
                return this.pCRCBuffer;
            }
            set
            {
                this.pCRCBuffer = value;
            }
        }

        /// <summary>
        /// �ڴ滺��
        /// </summary>
        public byte[] RawBuffer
        {
            get
            {
                return this._pRawBuffer;
            }
            set
            {
                this._pRawBuffer = value;
            }
        }

        /// <summary>
        /// ��ǰֵ
        /// </summary>
        public byte[] ContentBuffer
        {
            get
            {
                return this._pContentBuffer;
            }
            set
            {
                this._pContentBuffer = value;
            }
        }

        /// <summary>
        /// ѹ������
        /// </summary>
        public CompressionType Compression
        {
            get
            {
                return this._eCompression;
            }
            set
            {
                this._eCompression = value;
            }
        }
        #endregion

        #region �ֶζ���
        private byte[] _pRawBuffer;
        private byte[] _pContentBuffer;
        private CompressionType _eCompression;

        private int iOffSet;
        private int iBodySize;
        private byte[] pCRCBuffer;
        #endregion
    }

    /// <summary>
    /// SocketHead ��
    /// </summary>
    internal class SocketHead
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="pBuffer">������</param>
        public SocketHead(byte[] pBuffer)
        {
            this._pBuffer = pBuffer;
            this._eCompression = CompressionType.NONE;
        }

        /// <summary>
        /// ���캯�� -- ��װ����
        /// </summary>
        /// <param name="eCompression"></param>
        /// <param name="pBuffer"></param>
        public SocketHead(CompressionType eCompression, byte[] pBuffer)
        {
            this._eCompression = eCompression;
            this._pBuffer = pBuffer;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��װ����
        /// </summary>
        /// <returns>������</returns>
        public byte[] CoalitionInfo()
        {
            //ѹ����ʽ
            byte[] pCompressionSize = BitConverter.GetBytes((byte)this._eCompression);

            //�����峤��
            byte[] pBodySize = BitConverter.GetBytes(this._pBuffer.Length);

            //CRC����
            CRC32 pCRCUtilise = new CRC32();
            pCRCUtilise.ComputeHash(this._pBuffer, 0, this._pBuffer.Length);
            byte[] pCRCValue = pCRCUtilise.Value;

            //Socket Head Buffer
            byte[] pResultBuffer = new byte[SocketConstant.MAX_SOCKETHEAD_SIZE];
            Buffer.BlockCopy(pCompressionSize, 0, pResultBuffer, 0, pCompressionSize.Length);
            Buffer.BlockCopy(pBodySize, 0, pResultBuffer, SocketConstant.MAX_SOCKETCOMPRESS_SIZE, pBodySize.Length);
            Buffer.BlockCopy(pCRCValue, 0, pResultBuffer, SocketConstant.MAX_SOCKETHEAD_SIZE - SocketConstant.MAX_SOCKETCRC_SIZE, pCRCValue.Length);

            return pResultBuffer;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void ExtractInfo()
        {
            //�ж�����������
            if ((null == this._pBuffer) || (this._pBuffer.Length < SocketConstant.MAX_SOCKETHEAD_SIZE))
            {
                throw new Exception("Head Err!");
            }

            byte[] pBuffer = null;

            //ѹ����ʽ
            pBuffer = new byte[SocketConstant.MAX_SOCKETCOMPRESS_SIZE];
            Buffer.BlockCopy(this._pBuffer, 0, pBuffer, 0, SocketConstant.MAX_SOCKETCOMPRESS_SIZE);
            this._eCompression = (CompressionType)pBuffer[0];

            //�����峤��
            pBuffer = new byte[SocketConstant.MAX_SOCKETBODY_SIZE];
            Buffer.BlockCopy(this._pBuffer, SocketConstant.MAX_SOCKETCOMPRESS_SIZE, pBuffer, 0, SocketConstant.MAX_SOCKETBODY_SIZE);
            this.iBodyLength = BitConverter.ToInt32(pBuffer, 0);

            //CRCֵ
            pBuffer = new byte[SocketConstant.MAX_SOCKETCRC_SIZE];
            Buffer.BlockCopy(this._pBuffer, SocketConstant.MAX_SOCKETHEAD_SIZE - SocketConstant.MAX_SOCKETCRC_SIZE, pBuffer, 0, SocketConstant.MAX_SOCKETCRC_SIZE);
            this.pCRCValue = pBuffer;
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ѹ����ʽ
        /// </summary>
        public CompressionType Compression
        {
            get
            {
                return this._eCompression;
            }
        }

        /// <summary>
        /// ���ݰ�����
        /// </summary>
        public int BodyLength
        {
            get
            {
                return this.iBodyLength;
            }
        }

        /// <summary>
        /// CRC����ֵ
        /// </summary>
        public byte[] CRCValue
        {
            get
            {
                return this.pCRCValue;
            }
        }
        #endregion

        #region �ֶζ���
        private int iBodyLength;
        private byte[] pCRCValue;
        private byte[] _pBuffer;
        private CompressionType _eCompression;
        #endregion
    }

    /// <summary>
    /// �߳��ӳ���
    /// </summary>
    internal class ThreadLoop
    {
        public static void LoopSleep(ref int iLoopIndex)
        {
            if (Environment.ProcessorCount == 1 || (++iLoopIndex % (Environment.ProcessorCount * 50)) == 0)
            {
                //----- Single-core!
                Thread.Sleep(5);
            }
            else
            {
                //----- Multi-core / HT!
                Thread.SpinWait(20);
            }
        }
    }
}
