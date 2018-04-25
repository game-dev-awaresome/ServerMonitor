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
    #region ���ݱ���
    public static class EncodeUtilise
    {
        /// <summary>
        /// �Գ��㷨����
        /// </summary>
        /// <param name="eEncryptType">��������</param>
        /// <param name="iKeySize">Key��С</param>
        /// <param name="iBlockSize">Block��С</param>
        /// <returns>�Գ��㷨����</returns>
        public static SymmetricAlgorithm CreateSymmetricAlgoritm(EncryptType eEncryptType, int iKeySize, int iBlockSize)
        {
            SymmetricAlgorithm pResult = null;

            #region ���� Key ��IV ֵ
            switch (eEncryptType)
            {
                #region ���������׼�㷨
                case EncryptType.RIJNDAEL:
                    pResult = new RijndaelManaged();
                    break;
                #endregion
                #region �������ݼ��ܱ�׼�㷨
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
        /// ���뷽�� -- �ԳƱ���
        /// </summary>
        /// <param name="pSymmetricAlgorithm">�Գ��㷨����</param>
        /// <param name="pBuffer">������</param>
        /// <returns>������</returns>
        public static byte[] EncryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                //���ܴ���
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    pCryptoStream.Write(pBuffer, 0, pBuffer.Length);
                    pCryptoStream.FlushFinalBlock();

                    return pMemoryStream.ToArray();
                }//end using CryptoStream
            }//end using MemoryStream
        }

        /// <summary>
        /// ���뷽�� -- �ԳƱ���
        /// </summary>
        /// <param name="pSymmetricAlgorithm">�Գ��㷨����</param>
        /// <param name="pKey">Key</param>
        /// <param name="pBlock">Block</param>
        /// <param name="pBuffer">������</param>
        /// <returns>������</returns>
        public static byte[] EncryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pKey, byte[] pBlock, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream())
            {
                //���ܴ���
                using (CryptoStream pCryptoStream = new CryptoStream(pMemoryStream, pSymmetricAlgorithm.CreateEncryptor(pKey, pBlock), CryptoStreamMode.Write))
                {
                    pCryptoStream.Write(pBuffer, 0, pBuffer.Length);
                    pCryptoStream.FlushFinalBlock();

                    return pMemoryStream.ToArray();
                }//end using CryptoStream
            }//end using MemoryStream
        }

        /// <summary>
        /// ���뷽�� -- �ԳƱ���
        /// </summary>
        /// <param name="pSymmetricAlgorithm">�Գ��㷨����</param>
        /// <param name="pPaddingMode">��䷽ʽ</param>
        /// <param name="pBuffer">������</param>
        /// <returns>������</returns>
        public static byte[] DecryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pBuffer)
        {
            using (MemoryStream pMemoryStream = new MemoryStream(pBuffer))
            {
                //���ܴ���
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
        /// ���뷽�� -- �ԳƱ���
        /// </summary>
        /// <param name="pSymmetricAlgorithm">�Գ��㷨����</param>
        /// <param name="pKey">Key</param>
        /// <param name="pBlock">Block</param>
        /// <param name="pBuffer">������</param>
        /// <returns>������</returns>
        public static byte[] DecryptData(SymmetricAlgorithm pSymmetricAlgorithm, byte[] pKey, byte[] pBlock, byte[] pBuffer)
        {
            byte[] pResultBuffer = new byte[pBuffer.Length];

            using (MemoryStream pMemoryStream = new MemoryStream(pBuffer))
            {
                //���ܴ���
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

    #region ����ѹ��/��ѹ��
    public static class CompressionUtilise
    {
        /// <summary>
        /// ѹ������
        /// </summary>
        /// <param name="eCompressionType">ѹ������</param>
        /// <param name="pBuffer">������</param>
        /// <returns>ѹ�����</returns>
        public static byte[] CompressionData(CompressionType eCompressionType, byte[] pBuffer)
        {
            //�ж�ѹ����ʽ
            if (CompressionType.NONE == eCompressionType)
            {
                return pBuffer;
            }
            else
            {
                using (MemoryStream pMemoryStream = new MemoryStream())
                {
                    //����ѹ�����ݶ���
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
        /// ��ѹ������
        /// </summary>
        /// <param name="eCompressionType">ѹ������</param>
        /// <param name="pBuffer">������</param>
        /// <returns>��ѹ�����</returns>
        public static byte[] DeCompressionData(CompressionType eCompressionType, byte[] pBuffer)
        {
            //�ж�ѹ����ʽ
            if (CompressionType.NONE == eCompressionType)
            {
                return pBuffer;
            }
            else
            {
                //ȡԭ���ݵĳ���
                long iLength = BitConverter.ToInt64(pBuffer, pBuffer.Length - BUFFER_SIZE);

                //����ѹ�����ݶ���
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

                //��ȡ��ѹ����
                ArrayList pByteList = new ArrayList();

                for (int i = 0; i < iLength; i++)
                {
                    pByteList.Add(pBinaryReader.ReadByte());
                }

                pResult = (byte[])pByteList.ToArray(typeof(byte));
                pByteList.Clear();

                //�رն���
                pBinaryReader.BaseStream.Close();
                pBinaryReader.Close();

                //���ؽ��
                return pResult;
            }
        }

        private const int BUFFER_SIZE = 8;
    }
    #endregion

    #region ����ת��
    public static class DataUtilise
    {
        /// <summary>
        /// ���ݶ���ת��Ϊ������
        /// </summary>
        /// <param name="eFormat">���ݸ�ʽ</param>
        /// <param name="pContent">���ݶ���</param>
        /// <returns>������</returns>
        public static byte[] DataToBuffer(DataFormat eFormat, object pContent)
        {
            byte[] pResultBuffer = null;

            #region ����ת��
            Type pType = pContent.GetType();

            switch (eFormat)
            {
                #region NONE ת��
                case DataFormat.NONE:
                    if (pType == typeof(byte[]))
                    {
                        pResultBuffer = (byte[])pContent;
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�byte[]��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region BOOLEAN ת��
                case DataFormat.BOOLEAN:
                    if (pType == typeof(bool))
                    {
                        pResultBuffer = BitConverter.GetBytes((bool)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�boolean��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region SBYTE��BYTEת��
                case DataFormat.BYTE:
                    if (pType == typeof(sbyte) || pType == typeof(byte))
                    {
                        pResultBuffer = new byte[] { (byte)pContent };
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�sbyte��byte��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region USHORT��UINT��ULONGת��
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
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�ushort��uint��ulong��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region SHORT��INT��LONGת��
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
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�short��int��long��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region FLOAT, DOUBLE ת��
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
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�float��double��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region DATETIME ת��
                case DataFormat.DATETIME:
                    if (pType == typeof(DateTime))
                    {
                        pResultBuffer = BitConverter.GetBytes(((DateTime)pContent).Ticks);
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�DateTime��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region STRING ת��
                case DataFormat.STRING:
                    if (pType == typeof(string))
                    {
                        pResultBuffer = Encoding.Default.GetBytes((string)pContent);
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�string��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region DECIMALת��
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
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�decimal��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region GUIDת��
                case DataFormat.GUID:
                    if (pType == typeof(Guid))
                    {
                        pResultBuffer = ((Guid)pContent).ToByteArray();
                    }
                    else
                    {
                        throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�guid��, ʵ�����ͣ�" + pType.ToString() + "��");
                    }
                    break;
                #endregion
                #region VARIANTת��
                case DataFormat.VARIANT:
                #endregion
                #region Ĭ������ת��
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
        /// ������ת��Ϊ����
        /// </summary>
        /// <param name="eFormat">���ݸ�ʽ</param>
        /// <param name="pBuffer">������</param>
        /// <returns>���ݶ���</returns>
        public static object BufferToDate(DataFormat eFormat, byte[] pBuffer)
        {
            object pResultObject = null;

            #region ����ת��
            switch (eFormat)
            {
                #region NONE ת��
                case DataFormat.NONE:
                    pResultObject = pBuffer;
                    break;
                #endregion
                #region BOOLEAN ת��
                case DataFormat.BOOLEAN:
                    pResultObject = BitConverter.ToBoolean(pBuffer, 0);
                    break;
                #endregion
                #region SBYTE��BYTE ת��
                case DataFormat.BYTE:
                    pResultObject = pBuffer[0];
                    break;
                #endregion
                #region USHORT��UINT��ULONG ת��
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
                #region SHORT��INT��LONG ת��
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
                #region FLOAT, DOUBLE ת��
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
                #region DATETIME ת��
                case DataFormat.DATETIME:
                    pResultObject = new DateTime(BitConverter.ToInt64(pBuffer, 0));
                    break;
                #endregion
                #region STRING ת��
                case DataFormat.STRING:
                    pResultObject = Encoding.Default.GetString(pBuffer);
                    break;
                #endregion
                #region DECIMALת��
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
                #region GUIDת��
                case DataFormat.GUID:
                    pResultObject = new Guid(pBuffer);
                    break;
                #endregion
                #region VARIANTת��
                case DataFormat.VARIANT:
                #endregion
                #region Ĭ������ת��
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
