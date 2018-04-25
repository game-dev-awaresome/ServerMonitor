using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using System.Global;
using System.Define;

namespace System.DataPacket
{
    public class SocketPacket : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="pBuffer"></param>
        public SocketPacket(byte[] pBuffer)
        {
            this._pBuffer = pBuffer;

            this.strMessage = string.Empty;
        }

        /// <summary>
        /// ���캯�� -- ��װ����
        /// </summary>
        /// <param name="eCategory"></param>
        /// <param name="eService"></param>
        /// <param name="pSocketData"></param>
        public SocketPacket(TaskService eService, CustomDataCollection pSocketData)
        {
            this._eService = eService;
            this._pSocketData = pSocketData;

            this.eCategory = DefineUtilities.ToTaskCategory(eService);
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
                this._pSocketData.Clear();
            }

            base.Free(bDispodedByUser);
        }
        #endregion
        
        #region ��������
        public bool ExtractInfo()
        {
            bool bExtractResult = false;

            try
            {
                //Head Info
                PacketHead pPacketHead = new PacketHead(this._pBuffer);
                pPacketHead.ExtractInfo();

                this.iPacketID = pPacketHead.CurrentID;
                this.eCategory = pPacketHead.Category;
                this._eService = pPacketHead.Service;

                //Row Index
                int iRowIndex = PacketConstant.MAX_PACKETHEAD_SIZE;

                //Data Info
                while (true)
                {
                    //Data Length
                    byte[] pLengthBuffer = new byte[PacketConstant.MAX_DATALENGTH_SIZE];
                    Array.Copy(this._pBuffer, iRowIndex, pLengthBuffer, 0, PacketConstant.MAX_DATALENGTH_SIZE);
                    int iDataLength = BitConverter.ToInt32(pLengthBuffer, 0);

                    //Data Content
                    byte[] pDataBuffer = new byte[iDataLength];
                    Array.Copy(this._pBuffer, iRowIndex + PacketConstant.MAX_DATALENGTH_SIZE, pDataBuffer, 0, iDataLength);

                    if (this._pSocketData == null)
                    {
                        this._pSocketData = new CustomDataCollection(pDataBuffer);
                    }
                    else
                    {
                        this._pSocketData.Add(pDataBuffer);
                    }

                    //Row Index
                    iRowIndex += pDataBuffer.Length + PacketConstant.MAX_DATALENGTH_SIZE;

                    //Row Separbarte
                    if ((this._pBuffer[iRowIndex] == 0xFF) && (this._pBuffer[iRowIndex + 1] == 0xFF))
                    //if (this._pBuffer[iRowIndex] == 0xFF)
                    {
                        this._pSocketData.AddRows();

                        iRowIndex += PacketConstant.MAX_SEPARATELENGTH_SIZE;
                    }

                    //Break While
                    if (iRowIndex == this._pBuffer.Length)
                    {
                        break;
                    }
                }

                bExtractResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = String.Format(
                                    "Extract Exception\r\n\a\tSource �� {0}\r\n\a\tMessage �� {1}\r\n\a\tInnerMessage �� {2}",
                                    ex.Source, ex.Message, (null == ex.InnerException) ? "N/A" : ex.InnerException.Message
                                        );

                bExtractResult = false;
            }

            return bExtractResult;
        }

        public byte[] CoalitionInfo()
        {
            //PacketHead
            PacketHead pPacketHead = new PacketHead(this.eCategory, this._eService);
            byte[] pHeadBuffer = pPacketHead.CoalitionInfo();

            //Result
            if (PacketConstant.MAX_PACKETHEAD_SIZE != pHeadBuffer.Length)
            {
                throw new ArgumentException("��ͷ�����붨�岻���ϣ�");
            }

            //PacketData
            byte[] pDataBuffer = this._pSocketData.CoalitionInfo(0);

            byte[] pResultBuffer = new byte[pHeadBuffer.Length + pDataBuffer.Length];
            Array.Copy(pHeadBuffer, 0, pResultBuffer, 0, PacketConstant.MAX_PACKETHEAD_SIZE);
            Array.Copy(pDataBuffer, 0, pResultBuffer, PacketConstant.MAX_PACKETHEAD_SIZE, pDataBuffer.Length);

            return pResultBuffer;
        }
        #endregion

        #region ���Զ���
        public int Row
        {
            get
            {
                return this._pSocketData.RowCount;
            }
        }

        public int Column
        {
            get
            {
                return this._pSocketData.FieldCount;
            }
        }

        public string Message
        {
            get
            {
                return this.strMessage;
            }
        }

        public CustomDataCollection Content
        {
            get
            {
                return this._pSocketData;
            }
        }

        public TaskService eService
        {
            get
            {
                return this._eService;
            }
        }

        public int PacketID
        {
            get
            {
                return this.iPacketID;
            }
        }
        #endregion

        #region ��������
        private byte[] _pBuffer;
        private TaskService _eService;
        private CustomDataCollection _pSocketData;

        private int iPacketID;
        private string strMessage;
        private TaskCategory eCategory;
        #endregion

        #region PacketHead��
        /// <summary>
        /// ��ͷ
        /// </summary>
        internal class PacketHead
        {
            #region ����/��������
            /// <summary>
            /// ���캯�� -- ��������
            /// </summary>
            /// <param name="pBuffer">������</param>
            public PacketHead(byte[] pBuffer)
            {
                if ((null == pBuffer) || (pBuffer.Length < PacketConstant.MAX_PACKETHEAD_SIZE))
                {
                    throw new ArgumentException("��ͷ���ݴ���");
                }

                this._pBuffer = pBuffer;
            }

            /// <summary>
            /// ���캯�� -- ��װ����
            /// </summary>
            /// <param name="eCategory">�������</param>
            /// <param name="eService">�������</param>
            public PacketHead(TaskCategory eCategory, TaskService eService)
            {
                this._eCategory = eCategory;
                this._eService = eService;

                this.iPacketID = PacketID.Instance().CurrentID;
            }
            #endregion

            #region ��������
            /// <summary>
            /// ��������
            /// </summary>
            public void ExtractInfo()
            {
                byte[] pBuffer = null;

                //������
                pBuffer = new byte[PacketConstant.MAX_PACKETSERIAL_SIZE];
                Array.Copy(this._pBuffer, 0, pBuffer, 0, PacketConstant.MAX_PACKETSERIAL_SIZE);
                this.iPacketID = BitConverter.ToInt32(pBuffer, 0);

                //�������
                pBuffer = new byte[PacketConstant.MAX_PACKETCATEGORY_SIZE];
                Array.Copy(this._pBuffer, PacketConstant.MAX_PACKETSERIAL_SIZE, pBuffer, 0, PacketConstant.MAX_PACKETCATEGORY_SIZE);
                this._eCategory = (TaskCategory)pBuffer[0];

                //��������
                pBuffer = new byte[PacketConstant.MAX_PACKETSERVICE_SIZE];
                Array.Copy(this._pBuffer, PacketConstant.MAX_PACKETSERIAL_SIZE + PacketConstant.MAX_PACKETCATEGORY_SIZE, pBuffer, 0, PacketConstant.MAX_PACKETSERVICE_SIZE);
                this._eService = (TaskService)BitConverter.ToInt32(pBuffer, 0);
            }

            /// <summary>
            /// ��װ����
            /// </summary>
            /// <returns></returns>
            public byte[] CoalitionInfo()
            {
                byte[] pBuffer = null;
                byte[] pResultBuffer = new byte[PacketConstant.MAX_PACKETHEAD_SIZE];

                //������
                pBuffer = BitConverter.GetBytes(this.iPacketID);
                Array.Copy(pBuffer, 0, pResultBuffer, 0, PacketConstant.MAX_PACKETSERIAL_SIZE);

                //�������
                pBuffer = BitConverter.GetBytes((byte)this._eCategory);
                Array.Copy(pBuffer, 0, pResultBuffer, PacketConstant.MAX_PACKETSERIAL_SIZE, PacketConstant.MAX_PACKETCATEGORY_SIZE);

                //��������
                pBuffer = BitConverter.GetBytes((uint)this._eService);
                Array.Copy(pBuffer, 0, pResultBuffer, PacketConstant.MAX_PACKETSERIAL_SIZE + PacketConstant.MAX_PACKETCATEGORY_SIZE, PacketConstant.MAX_PACKETSERVICE_SIZE);

                return pResultBuffer;
            }
            #endregion

            #region ���Զ���
            /// <summary>
            /// ���ݰ�ID
            /// </summary>
            public int CurrentID
            {
                get
                {
                    return this.iPacketID;
                }
            }

            /// <summary>
            /// �������
            /// </summary>
            [Description("�������"), DefaultValue(typeof(TaskCategory), "MESSAGE")]
            public TaskCategory Category
            {
                get
                {
                    return this._eCategory;
                }
            }

            /// <summary>
            /// ��������
            /// </summary>
            [Description("��������"), DefaultValue(typeof(TaskService), "MESSAGE")]
            public TaskService Service
            {
                get
                {
                    return this._eService;
                }
            }
            #endregion

            #region ��������
            private int iPacketID;
            private byte[] _pBuffer;
            private TaskCategory _eCategory;
            private TaskService _eService;
            #endregion

            #region PacketID��
            /// <summary>
            /// ���ݰ�ID
            /// </summary>
            internal class PacketID
            {
                #region ����/��������
                /// <summary>
                /// ���캯��
                /// </summary>
                public PacketID()
                {
                    this._iPacketID = 1;
                }

                /// <summary>
                /// ���캯��
                /// </summary>
                public PacketID(int iID)
                {
                    if (iID == 0)
                    {
                        iID = 1;
                    }

                    this._iPacketID = iID;
                }
                #endregion

                #region ��������
                /// <summary>
                /// ��̬����
                /// </summary>
                /// <returns>���ݰ�ID</returns>
                public static PacketID Instance()
                {
                    if (pSingleton == null)
                    {
                        pSingleton = new PacketID();
                    }

                    return pSingleton;
                }
                #endregion

                #region ���Զ���
                /// <summary>
                /// ��ǰPacketID
                /// </summary>
                public int CurrentID
                {
                    get
                    {
                        return (this._iPacketID == int.MaxValue) ? 1 : this._iPacketID++;
                    }
                    set
                    {
                        this._iPacketID = (value == 0) ? 1 : value;
                    }
                }
                #endregion

                #region ��������
                private static PacketID pSingleton = null;

                private int _iPacketID;
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
