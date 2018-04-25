using System;
using System.Collections.Generic;
using System.Text;

using System.Global;
using System.Define;
using System.ComponentModel;

namespace System.DataPacket
{
    public class FilePacket : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数 -- 解析数据
        /// </summary>
        /// <param name="pBuffer"></param>
        public FilePacket(byte[] pBuffer)
        {
            this._pBuffer = pBuffer;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// 构造函数 -- 封装数据
        /// </summary>
        /// <param name="eService"></param>
        /// <param name="pBuffer"></param>
        public FilePacket(TaskService eService, byte[] pBuffer)
        {
            this._eService = eService;
            this._pBuffer = pBuffer;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// 构造函数 -- 封装数据
        /// </summary>
        /// <param name="eService"></param>
        /// <param name="pFiletData"></param>
        public FilePacket(TaskService eService, CustomDataCollection pFiletData)
        {
            if ((null == pFiletData) || (pFiletData.Count == 0))
            {
                throw new ArgumentException("文件数据错误！");
            }

            this._eService = eService;
            this._pRecordCollection = pFiletData;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// 继承方法
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                this._pRecordCollection.Clear();
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        #endregion

        #region 属性定义
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return this.pFileHead.CreateTime;
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        [Description("插入数据"), DefaultValue(typeof(bool), "false")]
        public bool Append
        {
            get
            {
                return this.bAppendData;
            }
            set
            {
                this.bAppendData = value;
            }
        }

        /// <summary>
        /// 数据体偏移量
        /// </summary>
        [Description("数据体偏移量"), DefaultValue(typeof(uint), "0")]
        public uint DataOffSet
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
        /// 修改时间/服务器时间
        /// </summary>
        public DateTime ModifyTime
        {
            get
            {
                return this.pFileHead.ModifyTime;
            }
            set
            {
                this.pFileHead.ModifyTime = value;
            }
        }
        #endregion

        #region 变量定义
        private byte[] _pBuffer;
        private TaskService _eService;
        private CustomDataCollection _pRecordCollection;

        private bool bAppendData;
        private uint iOffSet;
        private string strMessage;
        private PacketHead pFileHead;
        #endregion

        #region PacketHead类
        /// <summary>
        /// 文件头
        /// </summary>
        internal class PacketHead
        {
            #region 构造/析构函数
            /// <summary>
            /// 构造函数 -- 更新数据
            /// </summary>
            public PacketHead() { }

            /// <summary>
            /// 构造函数 -- 解析数据
            /// </summary>
            /// <param name="pBuffer"></param>
            public PacketHead(byte[] pBuffer)
            {
                this._pDataBuffer = pBuffer;
            }

            /// <summary>
            /// 构造函数 -- 封装数据
            /// </summary>
            /// <param name="eService">数据类别</param>
            public PacketHead(TaskService eService)
            {
                this._eService = eService;
            }
            #endregion            

            #region 方法定义
            /// <summary>
            /// 解析数据
            /// </summary>
            public void ExtractInfo()
            {
                byte[] pElementBuffer = null;

                //创建时间
                pElementBuffer = new byte[PacketConstant.MAX_CREATETIME_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE, pElementBuffer, 0, PacketConstant.MAX_CREATETIME_SIZE);
                this.lCreateTicks = BitConverter.ToInt64(pElementBuffer, 0);

                //服务对象
                pElementBuffer = new byte[PacketConstant.MAX_SERVICES_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE, pElementBuffer, 0, PacketConstant.MAX_SERVICES_SIZE);
                this._eService = (TaskService)BitConverter.ToUInt32(pElementBuffer, 0);

                //修改时间/服务器时间
                pElementBuffer = new byte[PacketConstant.MAX_MODIFYTIME_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE - PacketConstant.MAX_MODIFYTIME_SIZE, pElementBuffer, 0, PacketConstant.MAX_MODIFYTIME_SIZE);
                this.lServerTicks = BitConverter.ToInt64(pElementBuffer, 0);

                //CRC验证串
                pCRCBuffer = new byte[PacketConstant.MAX_DATACRC_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, pCRCBuffer, 0, PacketConstant.MAX_DATACRC_SIZE);

                //数据块大小倍数/数据索引位置
                pElementBuffer = new byte[PacketConstant.MAX_DATABLOCK_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE, pElementBuffer, 0, PacketConstant.MAX_DATABLOCK_SIZE);
                this.iOffset = BitConverter.ToUInt16(pElementBuffer, 0);
            }

            /// <summary>
            /// 封装数据
            /// </summary>
            /// <returns></returns>
            public byte[] CoalitionInfo(FileType eCategory)
            {
                byte[] pElementBuffer = null;
                byte[] pResultBuffer = new byte[PacketConstant.MAX_FILEHEAD_SIZE];

                //文件类型
                pElementBuffer = Encoding.Default.GetBytes(eCategory.ToString());
                Array.Copy(pElementBuffer, 0, pResultBuffer, 0, PacketConstant.MAX_MARK_SIZE);

                //文件说明
                Array.Copy(PacketConstant.FILE_TELTAG_CONTENT, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE, PacketConstant.FILE_TELTAG_CONTENT.Length);

                //创建时间
                pElementBuffer = BitConverter.GetBytes(DateTime.Now.Ticks);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE, PacketConstant.MAX_CREATETIME_SIZE);

                //文件内容
                pElementBuffer = BitConverter.GetBytes((uint)this._eService);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE, PacketConstant.MAX_SERVICES_SIZE);

                //修改时间/服务器时间
                pElementBuffer = BitConverter.GetBytes(this.lServerTicks);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE + PacketConstant.MAX_SERVICES_SIZE, PacketConstant.MAX_MODIFYTIME_SIZE);

                //数据验证
                Array.Copy(this.pCRCBuffer, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, PacketConstant.MAX_DATACRC_SIZE);

                //数据块倍数/索引位置
                pElementBuffer = BitConverter.GetBytes(this.iOffset);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE, PacketConstant.MAX_DATABLOCK_SIZE);

                //结束标志
                Array.Copy(PacketConstant.FILE_OVERTAG_CONTENT, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE, PacketConstant.MAX_OVERTAG_SIZE);

                return pResultBuffer;
            }

            /// <summary>
            /// 更新数据
            /// </summary>
            /// <param name="pBuffer">原文件信息流</param>
            public void UpdateInfo(ref byte[] pSourceBuffer)
            {
                byte[] pModifyBuffer = null;

                //修改时间
                pModifyBuffer = BitConverter.GetBytes(this.lServerTicks);
                Array.Copy(pModifyBuffer, 0, pSourceBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE + PacketConstant.MAX_SERVICES_SIZE, PacketConstant.MAX_MODIFYTIME_SIZE);

                //数据验证
                Array.Copy(this.pCRCBuffer, 0, pSourceBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, PacketConstant.MAX_DATACRC_SIZE);
            }
            #endregion

            #region 属性定义
            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime CreateTime
            {
                get
                {
                    return new DateTime(this.lCreateTicks);
                }
            }

            /// <summary>
            /// 文件内容
            /// </summary>
            public TaskService Service
            {
                get
                {
                    return this._eService;
                }
            }

            /// <summary>
            /// 数据块大小倍数/数据索引位置
            /// </summary>
            public uint OffSet
            {
                get
                {
                    return this.iOffset;
                }
                set
                {
                    this.iOffset = value;
                }
            }

            /// <summary>
            /// CRC验证串
            /// </summary>
            public byte[] CRC
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
            /// 修改时间/服务器时间
            /// </summary>
            public DateTime ModifyTime
            {
                get
                {
                    return new DateTime(this.lServerTicks);
                }
                set
                {
                    this.lServerTicks = value.Ticks;
                }
            }
            #endregion

            #region 变量定义
            private byte[] _pDataBuffer;
            private TaskService _eService;

            private uint iOffset;
            private long lCreateTicks;
            private long lServerTicks;
            private byte[] pCRCBuffer;
            #endregion
        }
        #endregion
    }
}
