using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

using System.Global;

namespace System.Define
{
    /// <summary>
    /// 结构体数据类型
    /// </summary>
    public enum StructType : byte
    {
        CUSTOMDATA = 0xAA,
        SERIALDATA = 0xBB,
        BINARYDATA = 0xCC,
        LOGDATA = 0xDD
    }

    /// <summary>
    /// 序列化数据结构体
    /// </summary>
    [Serializable]
    public struct SerialData
    {
        #region 构造/析构函数
        public SerialData(DataField eName, DataFormat eType, object pValue)
        {
            this._eFieldName = eName;
            this._eFieldType = eType;
            this._pFieldValue = pValue;
        }
        #endregion

        #region 方法定义
        public static SerialData ToStruct(byte[] pBuffer)
        {
            SerialData pResultStruct;

            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                pMemoryStream.Write(pBuffer, 0, pBuffer.Length);
                pMemoryStream.Position = 0;

                BinaryFormatter pBinaryFormatter = new BinaryFormatter();

                pResultStruct = (SerialData)pBinaryFormatter.Deserialize(pMemoryStream);
            }

            return pResultStruct;
        }

        public byte[] ToBuffer()
        {
            byte[] pResultBuffer;

            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                BinaryFormatter pBinaryFormatter = new BinaryFormatter();
                pBinaryFormatter.Serialize(pMemoryStream, this);

                pResultBuffer = pMemoryStream.ToArray();
            }

            return pResultBuffer;
        }
        #endregion

        #region 属性定义
        public DataField FieldName
        {
            get
            {
                return this._eFieldName;
            }
        }

        public DataFormat FieldType
        {
            get
            {
                return this._eFieldType;
            }
        }

        public object FieldValue
        {
            get
            {
                return this._pFieldValue;
            }
        }
        #endregion

        #region 变量定义
        private DataField _eFieldName;
        private DataFormat _eFieldType;
        private object _pFieldValue;
        #endregion
    }

    /// <summary>
    /// 二进制数据结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BinaryData
    {
        #region 构造/析构函数
        public BinaryData(DataField eName, DataFormat eType, byte[] pValue)
        {
            this.FieldName = eName;
            this.FieldType = eType;
            this.FieldValue = pValue;
        }
        #endregion

        #region 方法定义
        public static BinaryData ToStruct(byte[] pBuffer)
        {
            GCHandle pHandle = GCHandle.Alloc(pBuffer, GCHandleType.Pinned);
            BinaryData pResultStruct = (BinaryData)Marshal.PtrToStructure(pHandle.AddrOfPinnedObject(), typeof(BinaryData));
            pHandle.Free();

            pResultStruct.FieldValue = new byte[pBuffer.LongLength - StructSize.Size + VALUE_DEFAULT_SIZE];
            Array.Copy(pBuffer, StructSize.Size - VALUE_DEFAULT_SIZE, pResultStruct.FieldValue, 0, pResultStruct.FieldValue.LongLength);

            return pResultStruct;
        }

        public byte[] ToBuffer()
        {
            byte[] pResultBuffer = new byte[this.FieldValue.LongLength + StructSize.Size - VALUE_DEFAULT_SIZE];

            GCHandle pHandle = GCHandle.Alloc(pResultBuffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, pHandle.AddrOfPinnedObject(), false);
            pHandle.Free();

            Array.Copy(this.FieldValue, 0, pResultBuffer, StructSize.Size - VALUE_DEFAULT_SIZE, this.FieldValue.LongLength);

            return pResultBuffer;
        }
        #endregion

        #region 变量定义
        public DataField FieldName;
        public DataFormat FieldType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VALUE_DEFAULT_SIZE)]
        public byte[] FieldValue;
        #endregion

        #region 常量定义
        private const int VALUE_DEFAULT_SIZE = 1;
        #endregion
    }

    /// <summary>
    /// 自定义数据结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LogData
    {
        #region 构造/析构函数
        public LogData(string strField, DataFormat eType, byte[] pValue)
        {
            this.FieldName = strField;
            this.FieldType = eType;
            this.FieldValue = pValue;
        }
        #endregion

        #region 方法定义
        public static LogData BufferToData(byte[] pBuffer)
        {
            GCHandle pHandle = GCHandle.Alloc(pBuffer, GCHandleType.Pinned);
            LogData pResultStruct = (LogData)Marshal.PtrToStructure(pHandle.AddrOfPinnedObject(), typeof(LogData));
            pHandle.Free();

            pResultStruct.FieldValue = new byte[pBuffer.LongLength - StructSize.Size + VALUE_DEFAULT_SIZE];
            Array.Copy(pBuffer, StructSize.Size - VALUE_DEFAULT_SIZE, pResultStruct.FieldValue, 0, pResultStruct.FieldValue.LongLength);

            return pResultStruct;
        }

        public byte[] DataToBuffer()
        {
            byte[] pResultBuffer = new byte[this.FieldValue.LongLength + StructSize.Size - VALUE_DEFAULT_SIZE];

            GCHandle pHandle = GCHandle.Alloc(pResultBuffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, pHandle.AddrOfPinnedObject(), false);
            pHandle.Free();

            Array.Copy(this.FieldValue, 0, pResultBuffer, StructSize.Size - VALUE_DEFAULT_SIZE, this.FieldValue.LongLength);

            return pResultBuffer;
        }
        #endregion

        #region 变量定义
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FIELD_DEFAULT_SIZE)]
        public string FieldName;
        public DataFormat FieldType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VALUE_DEFAULT_SIZE)]
        public byte[] FieldValue;
        #endregion

        #region 常量定义
        private const int FIELD_DEFAULT_SIZE = 1;
        private const int VALUE_DEFAULT_SIZE = 1;
        #endregion
    }

    /// <summary>
    /// 默认结构体大小
    /// </summary>
    internal sealed class StructSize
    {
        #region 构造函数
        static StructSize()
        {
            iSize = Marshal.SizeOf(typeof(BinaryData));
        }
        #endregion

        #region 属性定义
        public static int Size
        {
            get
            {
                return iSize;
            }
        }
        #endregion

        #region 变量定义
        private static int iSize;
        #endregion
    }
}
