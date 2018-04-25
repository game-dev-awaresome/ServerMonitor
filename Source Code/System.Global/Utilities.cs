using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Global
{
    #region 数据编码
    public static class EncodeUtilise
    {
        /// <summary>
        /// 对称算法基类
        /// </summary>
        /// <param name="eEncryptType">加密类型</param>
        /// <param name="iKeySize">Key大小</param>
        /// <param name="iBlockSize">Block大小</param>
        /// <returns>对称算法基类</returns>
        public static SymmetricAlgorithm CreateSymmetricAlgoritm(EncryptType eEncryptType, int iKeySize, int iBlockSize)
        {
            SymmetricAlgorithm pResult = null;

            #region 生成 Key 、IV 值
            switch (eEncryptType)
            {
                #region 分组密码标准算法
                case EncryptType.RIJNDAEL:
                    pResult = new RijndaelManaged();
                    break;
                #endregion
                #region 三重数据加密标准算法
                case EncryptType.TRIPLEDES:
                    pResult = new TripleDESCryptoServiceProvider();
                    break;
                #endregion
            }

            if (pResult != null)
            {
                //pResult.KeySize = iKeySize;
                //pResult.BlockSize = iBlockSize;
                //pResult.Mode = CipherMode.CBC;
                //pResult.Padding = PaddingMode.ISO10126;
            }
            #endregion

            return pResult;
        }

        /// <summary>
        /// 编码方法 -- 对称编码
        /// </summary>
        /// <param name="pSymmetricAlgorithm">对称算法基类</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>编码结果</returns>
        public static byte[] EncryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                //加密处理
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    pCryptoStream.Write(pBuffer, 0, pBuffer.Length);
                    pCryptoStream.FlushFinalBlock();

                    return pMemoryStream.ToArray();
                }//end using CryptoStream
            }//end using MemoryStream
        }

        /// <summary>
        /// 编码方法 -- 对称编码
        /// </summary>
        /// <param name="pSymmetricAlgorithm">对称算法基类</param>
        /// <param name="pKey">Key</param>
        /// <param name="pBlock">Block</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>编码结果</returns>
        public static byte[] EncryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pKey, byte[] pBlock, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                //加密处理
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateEncryptor(pKey, pBlock), CryptoStreamMode.Write))
                {
                    pCryptoStream.Write(pBuffer, 0, pBuffer.Length);
                    pCryptoStream.FlushFinalBlock();

                    return pMemoryStream.ToArray();
                }//end using CryptoStream
            }//end using MemoryStream
        }

        /// <summary>
        /// 解码方法 -- 对称编码
        /// </summary>
        /// <param name="pSymmetricAlgorithm">对称算法基类</param>
        /// <param name="pPaddingMode">填充方式</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>解码结果</returns>
        public static byte[] DecryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream(pBuffer))
            {
                //解密处理
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                using (BinaryReader pBinaryReader = new BinaryReader(pCryptoStream))
                {
                    pMemoryStream.Position = 0;

                    ArrayList pByteList = new ArrayList();

                    for (int i = 0; i < pBinaryReader.BaseStream.Length; i++)
                    {
                        pByteList.Add(pBinaryReader.ReadByte());
                    }

                    return (byte[])pByteList.ToArray(typeof(byte));
                }//end using BinaryReader
            }//end using MemoryStream
        }

        /// <summary>
        /// 解码方法 -- 对称编码
        /// </summary>
        /// <param name="pSymmetricAlgorithm">对称算法基类</param>
        /// <param name="pKey">Key</param>
        /// <param name="pBlock">Block</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>解码结果</returns>
        public static byte[] DecryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pKey, byte[] pBlock, byte[] pBuffer)
        {
            byte[] pResultBuffer = new byte[pBuffer.Length];

            using (MemoryStream pMemoryStream = new MemoryStream(pBuffer))
            {
                //解密处理
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateDecryptor(pKey, pBlock), CryptoStreamMode.Read))
                using (BinaryReader pBinaryReader = new BinaryReader(pCryptoStream))
                {
                    //pMemoryStream.Position = 0;

                    //ArrayList pByteList = new ArrayList();

                    //for (int i = 0; i < pBinaryReader.BaseStream.Length - 1; i++)
                    //{
                    //    pByteList.Add(pBinaryReader.ReadByte());
                    //}

                    pMemoryStream.Position = 0;
                    pSymmetricAlgorithm.Padding = PaddingMode.None;
                    pBinaryReader.Read(pResultBuffer, 0, pResultBuffer.Length - 1);

                    //return (byte[])pByteList.ToArray(typeof(byte));
                }//end using BinaryReader
            }//end using MemoryStream

            return pResultBuffer;
        }
    }
    #endregion

    #region 数据压缩/解压缩
    public static class CompressionUtilise
    {
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="eCompressionType">压缩方法</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>压缩结果</returns>
        public static byte[] CompressionData(CompressionType eCompressionType, byte[] pBuffer)
        {
            //判断压缩方式
            if (CompressionType.NONE == eCompressionType)
            {
                return pBuffer;
            }
            else
            {
                using (MemoryStream pMemoryStream = new MemoryStream())
                {
                    //创建压缩数据对象
                    byte[] pResult = null;
                    object pCompressionStream = null;

                    switch (eCompressionType)
                    {
                        case CompressionType.DEFLATE:
                            pCompressionStream = new DeflateStream(pMemoryStream, CompressionMode.Compress, true);
                            ((DeflateStream)pCompressionStream).Write(pBuffer, 0, pBuffer.Length);
                            ((DeflateStream)pCompressionStream).Close();
                            break;
                        case CompressionType.GZIP:
                            pCompressionStream = new GZipStream(pMemoryStream, CompressionMode.Compress, true);
                            ((GZipStream)pCompressionStream).Write(pBuffer, 0, pBuffer.Length);
                            ((GZipStream)pCompressionStream).Close();
                            break;
                    }//end switch eCompressionType

                    pResult = new byte[pMemoryStream.Length + BUFFER_SIZE];
                    Array.Copy(pMemoryStream.ToArray(), 0, pResult, 0, pMemoryStream.Length);
                    Array.Copy(BitConverter.GetBytes(pBuffer.LongLength), 0, pResult, pMemoryStream.Length, BUFFER_SIZE);
                    return pResult;
                }//end using MemoryStream
            }
        }

        /// <summary>
        /// 解压缩数据
        /// </summary>
        /// <param name="eCompressionType">压缩方法</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>解压缩结果</returns>
        public static byte[] DeCompressionData(CompressionType eCompressionType, byte[] pBuffer)
        {
            //判断压缩方式
            if (CompressionType.NONE == eCompressionType)
            {
                return pBuffer;
            }
            else
            {
                //取原数据的长度
                long iLength = BitConverter.ToInt64(pBuffer, pBuffer.Length - BUFFER_SIZE);

                //创建压缩数据对象
                byte[] pResult = null;
                object pCompressionStream = null;
                BinaryReader pBinaryReader = null;
                MemoryStream pMemoryStream = new MemoryStream(pBuffer);

                switch (eCompressionType)
                {
                    case CompressionType.DEFLATE:
                        pCompressionStream = new DeflateStream(pMemoryStream, CompressionMode.Decompress, true);
                        pBinaryReader = new BinaryReader((DeflateStream)pCompressionStream);
                        break;
                    case CompressionType.GZIP:
                        pCompressionStream = new GZipStream(pMemoryStream, CompressionMode.Decompress, true);
                        pBinaryReader = new BinaryReader((GZipStream)pCompressionStream);
                        break;
                }//end switch eCompressionType

                //读取解压内容
                ArrayList pByteList = new ArrayList();

                for (int i = 0; i < iLength; i++)
                {
                    pByteList.Add(pBinaryReader.ReadByte());
                }

                pResult = (byte[])pByteList.ToArray(typeof(byte));
                pByteList.Clear();

                //关闭对象
                pBinaryReader.BaseStream.Close();
                pBinaryReader.Close();

                //返回结果
                return pResult;
            }
        }

        private const int BUFFER_SIZE = 8;
    }
    #endregion

    #region 数据转换
    public static class DataUtilise
    {
        /// <summary>
        /// 内容对象转换为数据流
        /// </summary>
        /// <param name="eFormat">数据格式</param>
        /// <param name="pContent">内容对象</param>
        /// <returns>数据流</returns>
        public static byte[] DataToBuffer(DataFormat eFormat, object pContent)
        {
            byte[] pResultBuffer = null;

            #region 数据转换
            Type pType = pContent.GetType();

            switch (eFormat)
            {
                #region NONE 转换
                case DataFormat.NONE:
                    if (pType == typeof(byte[]))
                    {
                        pResultBuffer = (byte[])pContent;
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［byte[]］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region BOOLEAN 转换
                case DataFormat.BOOLEAN:
                    if (pType == typeof(bool))
                    {
                        pResultBuffer = BitConverter.GetBytes((bool)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［boolean］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region SBYTE、BYTE转换
                case DataFormat.BYTE:
                    if (pType == typeof(sbyte) || pType == typeof(byte))
                    {
                        pResultBuffer = new byte[] { (byte)pContent };
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［sbyte、byte］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region USHORT、UINT、ULONG转换
                case DataFormat.UNSIGNED:
                    if (pType == typeof(ushort))
                    {
                        pResultBuffer = BitConverter.GetBytes((ushort)pContent);
                    }
                    else if (pType == typeof(uint))
                    {
                        pResultBuffer = BitConverter.GetBytes((uint)pContent);
                    }
                    else if (pType == typeof(UInt64))
                    {
                        pResultBuffer = BitConverter.GetBytes((UInt64)pContent);
                    }
                    else if (pType == typeof(ulong))
                    {
                        pResultBuffer = BitConverter.GetBytes((ulong)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［ushort、uint、ulong］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region SHORT、INT、LONG转换
                case DataFormat.SIGNED:
                    if (pType == typeof(short))
                    {
                        pResultBuffer = BitConverter.GetBytes((short)pContent);
                    }
                    else if (pType == typeof(int))
                    {
                        pResultBuffer = BitConverter.GetBytes((int)pContent);
                    }
                    else if (pType == typeof(Int64))
                    {
                        pResultBuffer = BitConverter.GetBytes((Int64)pContent);
                    }
                    else if (pType == typeof(long))
                    {
                        pResultBuffer = BitConverter.GetBytes((long)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［short、int、long］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region FLOAT, DOUBLE 转换
                case DataFormat.PRECISION:
                    if (pType == typeof(float))
                    {
                        pResultBuffer = BitConverter.GetBytes((float)pContent);
                    }
                    else if (pType == typeof(double))
                    {
                        pResultBuffer = BitConverter.GetBytes((double)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［float、double］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region DATETIME 转换
                case DataFormat.DATETIME:
                    if (pType == typeof(DateTime))
                    {
                        pResultBuffer = BitConverter.GetBytes(((DateTime)pContent).Ticks);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［DateTime］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region STRING 转换
                case DataFormat.STRING:
                    if (pType == typeof(string))
                    {
                        pResultBuffer = Encoding.Default.GetBytes((string)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［string］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region DECIMAL转换
                case DataFormat.DECIMAL:
                    if (pType == typeof(decimal))
                    {
                        int[] pIntArray = decimal.GetBits((Decimal)pContent);

                        pResultBuffer = new byte[pIntArray.Length * sizeof(int)];

                        for (int i = 0; i < pIntArray.Length; i++)
                        {
                            byte[] pTmpBuffer = BitConverter.GetBytes(pIntArray[i]);
                            Array.Copy(pTmpBuffer, 0, pResultBuffer, i * sizeof(int), pTmpBuffer.Length);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［decimal］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region GUID转换
                case DataFormat.GUID:
                    if (pType == typeof(Guid))
                    {
                        pResultBuffer = ((Guid)pContent).ToByteArray();
                    }
                    else
                    {
                        throw new ArgumentException("参数预期类型错误", "预期类型［guid］, 实际类型［" + pType.ToString() + "］");
                    }
                    break;
                #endregion
                #region VARIANT转换
                case DataFormat.VARIANT:
                #endregion
                #region 默认类型转换
                default:
                    using (MemoryStream pMemoryStream = new MemoryStream())
                    {
                        BinaryFormatter pBinaryFormatter = new BinaryFormatter();
                        pBinaryFormatter.Serialize(pMemoryStream, pContent);

                        pResultBuffer = pMemoryStream.ToArray();
                    }
                    break;
                #endregion
            }
            #endregion

            return pResultBuffer;
        }

        /// <summary>
        /// 数据流转换为对象
        /// </summary>
        /// <param name="eFormat">数据格式</param>
        /// <param name="pBuffer">数据流</param>
        /// <returns>内容对象</returns>
        public static object BufferToDate(DataFormat eFormat, byte[] pBuffer)
        {
            object pResultObject = null;

            #region 数据转换
            switch (eFormat)
            {
                #region NONE 转换
                case DataFormat.NONE:
                    pResultObject = pBuffer;
                    break;
                #endregion
                #region BOOLEAN 转换
                case DataFormat.BOOLEAN:
                    pResultObject = BitConverter.ToBoolean(pBuffer, 0);
                    break;
                #endregion
                #region SBYTE、BYTE 转换
                case DataFormat.BYTE:
                    pResultObject = pBuffer[0];
                    break;
                #endregion
                #region USHORT、UINT、ULONG 转换
                case DataFormat.UNSIGNED:
                    switch (pBuffer.Length)
                    {
                        case 2:
                            pResultObject = BitConverter.ToUInt16(pBuffer, 0);
                            break;
                        case 4:
                            pResultObject = BitConverter.ToUInt32(pBuffer, 0);
                            break;
                        case 8:
                            pResultObject = BitConverter.ToUInt64(pBuffer, 0);
                            break;
                        default:
                            pResultObject = 0;
                            break;
                    }
                    break;
                #endregion
                #region SHORT、INT、LONG 转换
                case DataFormat.SIGNED:
                    switch (pBuffer.Length)
                    {
                        case 2:
                            pResultObject = BitConverter.ToInt16(pBuffer, 0);
                            break;
                        case 4:
                            pResultObject = BitConverter.ToInt32(pBuffer, 0);
                            break;
                        case 8:
                            pResultObject = BitConverter.ToInt64(pBuffer, 0);
                            break;
                        default:
                            pResultObject = 0;
                            break;
                    }
                    break;
                #endregion
                #region FLOAT, DOUBLE 转换
                case DataFormat.PRECISION:
                    switch (pBuffer.Length)
                    {
                        case 4:
                            pResultObject = BitConverter.ToSingle(pBuffer, 0);
                            break;
                        case 8:
                            pResultObject = BitConverter.ToDouble(pBuffer, 0);
                            break;
                        default:
                            pResultObject = 0.00f;
                            break;
                    }
                    break;
                #endregion
                #region DATETIME 转换
                case DataFormat.DATETIME:
                    pResultObject = new DateTime(BitConverter.ToInt64(pBuffer, 0));
                    break;
                #endregion
                #region STRING 转换
                case DataFormat.STRING:
                    pResultObject = Encoding.Default.GetString(pBuffer);
                    break;
                #endregion
                #region DECIMAL转换
                case DataFormat.DECIMAL:
                    int[] pIntArray = new int[pBuffer.Length / sizeof(int)];

                    for (int i = 0; i < pIntArray.Length; i++)
                    {
                        byte[] pTmpBuffer = new byte[sizeof(int)];

                        Array.Copy(pBuffer, i * sizeof(int), pTmpBuffer, 0, sizeof(int));

                        pIntArray[i] = BitConverter.ToInt32(pTmpBuffer, 0);
                    }

                    pResultObject = new decimal(pIntArray);
                    break;
                #endregion
                #region GUID转换
                case DataFormat.GUID:
                    pResultObject = new Guid(pBuffer);
                    break;
                #endregion
                #region VARIANT转换
                case DataFormat.VARIANT:
                #endregion
                #region 默认类型转换
                default:
                    using (MemoryStream pMemoryStream = new MemoryStream())
                    {
                        pMemoryStream.Write(pBuffer, 0, pBuffer.Length);
                        pMemoryStream.Position = 0;

                        BinaryFormatter pBinaryFormatter = new BinaryFormatter();

                        pResultObject = pBinaryFormatter.Deserialize(pMemoryStream);
                    }
                    break;
                #endregion
            }
            #endregion

            return pResultObject;
        }
    }
    #endregion
}
