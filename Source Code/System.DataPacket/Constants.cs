using System;
using System.Collections.Generic;
using System.Text;

namespace System.DataPacket
{
    /// <summary>
    /// PacketCore ��������
    /// </summary>
    internal class PacketConstant
    {
        #region Custom Data����
        /// <summary>
        /// �ֶ�����
        /// </summary>
        public const int MAX_FIELDNAME_SIZE = 4;
        /// <summary>
        /// �ֶθ�ʽ
        /// </summary>
        public const int MAX_FIELDFORMAT_SIZE = 1;
        /// <summary>
        /// ����ͷ����
        /// </summary>
        public const int MAX_DATAHEAD_SIZE = MAX_FIELDNAME_SIZE + MAX_FIELDFORMAT_SIZE;
        #endregion

        #region Data Body����
        /// <summary>
        /// �����н������
        /// </summary>
        public static byte[] ROW_SEPARATE_CONTENT = new byte[] { 0xFF,0xFF };
        /// <summary>
        /// ���ݳ���
        /// </summary>
        public const int MAX_DATALENGTH_SIZE = 4;
        /// <summary>
        /// �зָ�
        /// </summary>separate length
        public const int MAX_SEPARATELENGTH_SIZE = 2;
        #endregion

        #region Socket Packet Head����
        /// <summary>
        /// ���ID
        /// </summary>
        public const int MAX_PACKETSERIAL_SIZE = 4;
        /// <summary>
        /// �������
        /// </summary>
        public const int MAX_PACKETCATEGORY_SIZE = 1;
        /// <summary>
        /// �������
        /// </summary>
        public const int MAX_PACKETSERVICE_SIZE = 4;
        /// <summary>
        /// ���ݰ�ͷ�ܳ���
        /// </summary>
        public const int MAX_PACKETHEAD_SIZE = MAX_PACKETSERIAL_SIZE + MAX_PACKETCATEGORY_SIZE + MAX_PACKETSERVICE_SIZE;
        #endregion

        #region File Packet Head����
        /// <summary>
        /// �ļ�ͷ�����������
        /// </summary>
        public static byte[] FILE_OVERTAG_CONTENT = new byte[] { 0x0D, 0x0A };
        /// <summary>
        /// �ļ�˵��/��������
        /// </summary>
        /// <remarks>��󳤶Ȳ��ܳ���33���ַ�</remarks>
        public static byte[] FILE_TELTAG_CONTENT = new byte[] { 0x00, 0x00 };
        /// <summary>
        /// ���ݿ��С����
        /// </summary>
        public const int FILE_DATABLOCK_SIZE = 512;
        /// <summary>
        /// �ļ����
        /// </summary>
        public const int MAX_MARK_SIZE = 3;
        /// <summary>
        /// �ļ�˵��
        /// </summary>
        public const int MAX_TELTAG_SIZE = 33;
        /// <summary>
        /// �ļ�����ʱ��
        /// </summary>
        public const int MAX_CREATETIME_SIZE = 8;
        /// <summary>
        /// �������
        /// </summary>
        public const int MAX_SERVICES_SIZE = 4;
        /// <summary>
        /// ���ݸ���ʱ��/������ʱ��
        /// </summary>
        public const int MAX_MODIFYTIME_SIZE = 8;
        /// <summary>
        /// ������֤����/����ͷƫ��λ
        /// </summary>
        public const int MAX_DATACRC_SIZE = 4;
        /// <summary>
        /// ���ݿ��С����/��������ƫ��λ
        /// </summary>
        public const int MAX_DATABLOCK_SIZE = 4;
        /// <summary>
        /// �ļ�ͷ�������
        /// </summary>
        public const int MAX_OVERTAG_SIZE = 2;
        /// <summary>
        /// �ļ�ͷ����
        /// </summary>
        public const int MAX_FILEHEAD_SIZE = MAX_MARK_SIZE + MAX_TELTAG_SIZE
                                + MAX_CREATETIME_SIZE + MAX_SERVICES_SIZE
                                + MAX_MODIFYTIME_SIZE + MAX_DATACRC_SIZE + MAX_DATABLOCK_SIZE
                                + MAX_OVERTAG_SIZE;
        #endregion
    }
}
