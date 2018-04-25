using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Global;

namespace Windows.Network
{
    /// <summary>
    /// Socket 数据回调类
    /// </summary>
    internal class SocketDataCallback
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pConnectionManage"></param>
        /// <param name="pBuffer"></param>
        public SocketDataCallback(ConnectionManage pConnection, object pSocketBuffer)
        {
            this._pConnection = pConnection;
            this._pSocketBuffer = pSocketBuffer;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// Socket 数据
        /// </summary>
        public object SocketBuffer
        {
            get
            {
                return this._pSocketBuffer;
            }
        }

        /// <summary>
        /// 连接管理
        /// </summary>
        public ConnectionManage Connection
        {
            get
            {
                return this._pConnection;
            }
        }
        #endregion

        #region 字段定义
        private object _pSocketBuffer;
        private ConnectionManage _pConnection;
        #endregion
    }

    /// <summary>
    /// Socket 数据缓冲类
    /// </summary>
    internal class SocketBuffer
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数 -- 接收数据
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
        /// 构造函数 -- 发送数据
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

        #region 方法定义
        /// <summary>
        /// 填充报头
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
        /// 设置数组大小
        /// </summary>
        /// <param name="iLength">数据长度</param>
        public void Resize(int iLength)
        {
            Array.Resize(ref this._pContentBuffer, iLength);
        }

        /// <summary>
        /// 读取Buffer数据
        /// </summary>
        /// <param name="iHeadSize"></param>
        /// <param name="iBodySize"></param>
        /// <returns></returns>
        public byte[] GetRawBuffer(int iHeadSize, int iMessageSize)
        {
            //获取结果
            byte[] pResult = new byte[iMessageSize - iHeadSize];
            Buffer.BlockCopy(this._pContentBuffer, iHeadSize, pResult, 0, pResult.Length);

            //调整Buffer大小
            byte[] pBuffer = new byte[this._pContentBuffer.Length - iMessageSize];
            Buffer.BlockCopy(this._pContentBuffer, iHeadSize, pBuffer, 0, pBuffer.Length);

            //更新属性信息
            this._pContentBuffer = pBuffer;
            this.iOffSet = this.iOffSet - iMessageSize;

            return pResult;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 数据偏移量
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
        /// 数据剩余量
        /// </summary>
        public int Remaining
        {
            get
            {
                return this._pContentBuffer.Length - this.iOffSet;
            }
        }

        /// <summary>
        /// 当前大小
        /// </summary>
        public int CurrentSize
        {
            get
            {
                return this._pContentBuffer.Length;
            }
        }

        /// <summary>
        /// 数据体大小 -- 未知作用
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
        /// CRC值
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
        /// 内存缓冲
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
        /// 当前值
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
        /// 压缩方法
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

        #region 字段定义
        private byte[] _pRawBuffer;
        private byte[] _pContentBuffer;
        private CompressionType _eCompression;

        private int iOffSet;
        private int iBodySize;
        private byte[] pCRCBuffer;
        #endregion
    }

    /// <summary>
    /// SocketHead 类
    /// </summary>
    internal class SocketHead
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数 -- 解析数据
        /// </summary>
        /// <param name="pBuffer">数据流</param>
        public SocketHead(byte[] pBuffer)
        {
            this._pBuffer = pBuffer;
            this._eCompression = CompressionType.NONE;
        }

        /// <summary>
        /// 构造函数 -- 封装数据
        /// </summary>
        /// <param name="eCompression"></param>
        /// <param name="pBuffer"></param>
        public SocketHead(CompressionType eCompression, byte[] pBuffer)
        {
            this._eCompression = eCompression;
            this._pBuffer = pBuffer;
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 封装数据
        /// </summary>
        /// <returns>数据流</returns>
        public byte[] CoalitionInfo()
        {
            //压缩方式
            byte[] pCompressionSize = BitConverter.GetBytes((byte)this._eCompression);

            //数据体长度
            byte[] pBodySize = BitConverter.GetBytes(this._pBuffer.Length);

            //CRC计算
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
        /// 解析数据
        /// </summary>
        public void ExtractInfo()
        {
            //判断数据完整性
            if ((null == this._pBuffer) || (this._pBuffer.Length < SocketConstant.MAX_SOCKETHEAD_SIZE))
            {
                throw new Exception("Head Err!");
            }

            byte[] pBuffer = null;

            //压缩方式
            pBuffer = new byte[SocketConstant.MAX_SOCKETCOMPRESS_SIZE];
            Buffer.BlockCopy(this._pBuffer, 0, pBuffer, 0, SocketConstant.MAX_SOCKETCOMPRESS_SIZE);
            this._eCompression = (CompressionType)pBuffer[0];

            //数据体长度
            pBuffer = new byte[SocketConstant.MAX_SOCKETBODY_SIZE];
            Buffer.BlockCopy(this._pBuffer, SocketConstant.MAX_SOCKETCOMPRESS_SIZE, pBuffer, 0, SocketConstant.MAX_SOCKETBODY_SIZE);
            this.iBodyLength = BitConverter.ToInt32(pBuffer, 0);

            //CRC值
            pBuffer = new byte[SocketConstant.MAX_SOCKETCRC_SIZE];
            Buffer.BlockCopy(this._pBuffer, SocketConstant.MAX_SOCKETHEAD_SIZE - SocketConstant.MAX_SOCKETCRC_SIZE, pBuffer, 0, SocketConstant.MAX_SOCKETCRC_SIZE);
            this.pCRCValue = pBuffer;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 压缩方式
        /// </summary>
        public CompressionType Compression
        {
            get
            {
                return this._eCompression;
            }
        }

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int BodyLength
        {
            get
            {
                return this.iBodyLength;
            }
        }

        /// <summary>
        /// CRC检验值
        /// </summary>
        public byte[] CRCValue
        {
            get
            {
                return this.pCRCValue;
            }
        }
        #endregion

        #region 字段定义
        private int iBodyLength;
        private byte[] pCRCValue;
        private byte[] _pBuffer;
        private CompressionType _eCompression;
        #endregion
    }

    /// <summary>
    /// 线程延迟类
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
