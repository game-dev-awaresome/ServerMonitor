using System;
using System.Collections.Generic;
using System.Text;

namespace System.DataPacket
{
    /// <summary>
    /// PacketCore 常量定义
    /// </summary>
    internal class PacketConstant
    {
        #region Custom Data定义
        /// <summary>
        /// 字段名称
        /// </summary>
        public const int MAX_FIELDNAME_SIZE = 4;
        /// <summary>
        /// 字段格式
        /// </summary>
        public const int MAX_FIELDFORMAT_SIZE = 1;
        /// <summary>
        /// 数据头长度
        /// </summary>
        public const int MAX_DATAHEAD_SIZE = MAX_FIELDNAME_SIZE + MAX_FIELDFORMAT_SIZE;
        #endregion

        #region Data Body定义
        /// <summary>
        /// 数据行结束标记
        /// </summary>
        public static byte[] ROW_SEPARATE_CONTENT = new byte[] { 0xFF,0xFF };
        /// <summary>
        /// 数据长度
        /// </summary>
        public const int MAX_DATALENGTH_SIZE = 4;
        /// <summary>
        /// 行分隔
        /// </summary>separate length
        public const int MAX_SEPARATELENGTH_SIZE = 2;
        #endregion

        #region Socket Packet Head定义
        /// <summary>
        /// 封包ID
        /// </summary>
        public const int MAX_PACKETSERIAL_SIZE = 4;
        /// <summary>
        /// 任务类别
        /// </summary>
        public const int MAX_PACKETCATEGORY_SIZE = 1;
        /// <summary>
        /// 任务键名
        /// </summary>
        public const int MAX_PACKETSERVICE_SIZE = 4;
        /// <summary>
        /// 数据包头总长度
        /// </summary>
        public const int MAX_PACKETHEAD_SIZE = MAX_PACKETSERIAL_SIZE + MAX_PACKETCATEGORY_SIZE + MAX_PACKETSERVICE_SIZE;
        #endregion

        #region File Packet Head定义
        /// <summary>
        /// 文件头结束标记内容
        /// </summary>
        public static byte[] FILE_OVERTAG_CONTENT = new byte[] { 0x0D, 0x0A };
        /// <summary>
        /// 文件说明/描述内容
        /// </summary>
        /// <remarks>最大长度不能超过33个字符</remarks>
        public static byte[] FILE_TELTAG_CONTENT = new byte[] { 0x00, 0x00 };
        /// <summary>
        /// 数据块大小基数
        /// </summary>
        public const int FILE_DATABLOCK_SIZE = 512;
        /// <summary>
        /// 文件标记
        /// </summary>
        public const int MAX_MARK_SIZE = 3;
        /// <summary>
        /// 文件说明
        /// </summary>
        public const int MAX_TELTAG_SIZE = 33;
        /// <summary>
        /// 文件创建时间
        /// </summary>
        public const int MAX_CREATETIME_SIZE = 8;
        /// <summary>
        /// 服务对象
        /// </summary>
        public const int MAX_SERVICES_SIZE = 4;
        /// <summary>
        /// 数据更新时间/服务器时间
        /// </summary>
        public const int MAX_MODIFYTIME_SIZE = 8;
        /// <summary>
        /// 数据验证长度/数据头偏移位
        /// </summary>
        public const int MAX_DATACRC_SIZE = 4;
        /// <summary>
        /// 数据块大小倍数/数据索引偏移位
        /// </summary>
        public const int MAX_DATABLOCK_SIZE = 4;
        /// <summary>
        /// 文件头结束标记
        /// </summary>
        public const int MAX_OVERTAG_SIZE = 2;
        /// <summary>
        /// 文件头长度
        /// </summary>
        public const int MAX_FILEHEAD_SIZE = MAX_MARK_SIZE + MAX_TELTAG_SIZE
                                + MAX_CREATETIME_SIZE + MAX_SERVICES_SIZE
                                + MAX_MODIFYTIME_SIZE + MAX_DATACRC_SIZE + MAX_DATABLOCK_SIZE
                                + MAX_OVERTAG_SIZE;
        #endregion
    }
}
