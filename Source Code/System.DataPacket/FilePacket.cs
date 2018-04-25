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
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="pBuffer"></param>
        public FilePacket(byte[] pBuffer)
        {
            this._pBuffer = pBuffer;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// ���캯�� -- ��װ����
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
        /// ���캯�� -- ��װ����
        /// </summary>
        /// <param name="eService"></param>
        /// <param name="pFiletData"></param>
        public FilePacket(TaskService eService, CustomDataCollection pFiletData)
        {
            if ((null == pFiletData) || (pFiletData.Count == 0))
            {
                throw new ArgumentException("�ļ����ݴ���");
            }

            this._eService = eService;
            this._pRecordCollection = pFiletData;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// �̳з���
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

        #region ��������
        #endregion

        #region ���Զ���
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return this.pFileHead.CreateTime;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        [Description("��������"), DefaultValue(typeof(bool), "false")]
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
        /// ������ƫ����
        /// </summary>
        [Description("������ƫ����"), DefaultValue(typeof(uint), "0")]
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
        /// �޸�ʱ��/������ʱ��
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

        #region ��������
        private byte[] _pBuffer;
        private TaskService _eService;
        private CustomDataCollection _pRecordCollection;

        private bool bAppendData;
        private uint iOffSet;
        private string strMessage;
        private PacketHead pFileHead;
        #endregion

        #region PacketHead��
        /// <summary>
        /// �ļ�ͷ
        /// </summary>
        internal class PacketHead
        {
            #region ����/��������
            /// <summary>
            /// ���캯�� -- ��������
            /// </summary>
            public PacketHead() { }

            /// <summary>
            /// ���캯�� -- ��������
            /// </summary>
            /// <param name="pBuffer"></param>
            public PacketHead(byte[] pBuffer)
            {
                this._pDataBuffer = pBuffer;
            }

            /// <summary>
            /// ���캯�� -- ��װ����
            /// </summary>
            /// <param name="eService">�������</param>
            public PacketHead(TaskService eService)
            {
                this._eService = eService;
            }
            #endregion            

            #region ��������
            /// <summary>
            /// ��������
            /// </summary>
            public void ExtractInfo()
            {
                byte[] pElementBuffer = null;

                //����ʱ��
                pElementBuffer = new byte[PacketConstant.MAX_CREATETIME_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE, pElementBuffer, 0, PacketConstant.MAX_CREATETIME_SIZE);
                this.lCreateTicks = BitConverter.ToInt64(pElementBuffer, 0);

                //�������
                pElementBuffer = new byte[PacketConstant.MAX_SERVICES_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE, pElementBuffer, 0, PacketConstant.MAX_SERVICES_SIZE);
                this._eService = (TaskService)BitConverter.ToUInt32(pElementBuffer, 0);

                //�޸�ʱ��/������ʱ��
                pElementBuffer = new byte[PacketConstant.MAX_MODIFYTIME_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE - PacketConstant.MAX_MODIFYTIME_SIZE, pElementBuffer, 0, PacketConstant.MAX_MODIFYTIME_SIZE);
                this.lServerTicks = BitConverter.ToInt64(pElementBuffer, 0);

                //CRC��֤��
                pCRCBuffer = new byte[PacketConstant.MAX_DATACRC_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, pCRCBuffer, 0, PacketConstant.MAX_DATACRC_SIZE);

                //���ݿ��С����/��������λ��
                pElementBuffer = new byte[PacketConstant.MAX_DATABLOCK_SIZE];
                Array.Copy(this._pDataBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_DATABLOCK_SIZE, pElementBuffer, 0, PacketConstant.MAX_DATABLOCK_SIZE);
                this.iOffset = BitConverter.ToUInt16(pElementBuffer, 0);
            }

            /// <summary>
            /// ��װ����
            /// </summary>
            /// <returns></returns>
            public byte[] CoalitionInfo(FileType eCategory)
            {
                byte[] pElementBuffer = null;
                byte[] pResultBuffer = new byte[PacketConstant.MAX_FILEHEAD_SIZE];

                //�ļ�����
                pElementBuffer = Encoding.Default.GetBytes(eCategory.ToString());
                Array.Copy(pElementBuffer, 0, pResultBuffer, 0, PacketConstant.MAX_MARK_SIZE);

                //�ļ�˵��
                Array.Copy(PacketConstant.FILE_TELTAG_CONTENT, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE, PacketConstant.FILE_TELTAG_CONTENT.Length);

                //����ʱ��
                pElementBuffer = BitConverter.GetBytes(DateTime.Now.Ticks);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE, PacketConstant.MAX_CREATETIME_SIZE);

                //�ļ�����
                pElementBuffer = BitConverter.GetBytes((uint)this._eService);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE, PacketConstant.MAX_SERVICES_SIZE);

                //�޸�ʱ��/������ʱ��
                pElementBuffer = BitConverter.GetBytes(this.lServerTicks);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE + PacketConstant.MAX_SERVICES_SIZE, PacketConstant.MAX_MODIFYTIME_SIZE);

                //������֤
                Array.Copy(this.pCRCBuffer, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, PacketConstant.MAX_DATACRC_SIZE);

                //���ݿ鱶��/����λ��
                pElementBuffer = BitConverter.GetBytes(this.iOffset);
                Array.Copy(pElementBuffer, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE, PacketConstant.MAX_DATABLOCK_SIZE);

                //������־
                Array.Copy(PacketConstant.FILE_OVERTAG_CONTENT, 0, pResultBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE, PacketConstant.MAX_OVERTAG_SIZE);

                return pResultBuffer;
            }

            /// <summary>
            /// ��������
            /// </summary>
            /// <param name="pBuffer">ԭ�ļ���Ϣ��</param>
            public void UpdateInfo(ref byte[] pSourceBuffer)
            {
                byte[] pModifyBuffer = null;

                //�޸�ʱ��
                pModifyBuffer = BitConverter.GetBytes(this.lServerTicks);
                Array.Copy(pModifyBuffer, 0, pSourceBuffer, PacketConstant.MAX_MARK_SIZE + PacketConstant.MAX_TELTAG_SIZE + PacketConstant.MAX_CREATETIME_SIZE + PacketConstant.MAX_SERVICES_SIZE, PacketConstant.MAX_MODIFYTIME_SIZE);

                //������֤
                Array.Copy(this.pCRCBuffer, 0, pSourceBuffer, PacketConstant.MAX_FILEHEAD_SIZE - PacketConstant.MAX_OVERTAG_SIZE - PacketConstant.MAX_DATABLOCK_SIZE - PacketConstant.MAX_DATACRC_SIZE, PacketConstant.MAX_DATACRC_SIZE);
            }
            #endregion

            #region ���Զ���
            /// <summary>
            /// ����ʱ��
            /// </summary>
            public DateTime CreateTime
            {
                get
                {
                    return new DateTime(this.lCreateTicks);
                }
            }

            /// <summary>
            /// �ļ�����
            /// </summary>
            public TaskService Service
            {
                get
                {
                    return this._eService;
                }
            }

            /// <summary>
            /// ���ݿ��С����/��������λ��
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
            /// CRC��֤��
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
            /// �޸�ʱ��/������ʱ��
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

            #region ��������
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
