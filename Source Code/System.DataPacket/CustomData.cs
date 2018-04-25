using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Global;
using System.Define;

namespace System.DataPacket
{
    /// <summary>
    /// �Զ������ݽṹ������
    /// </summary>
    public class CustomDataCollection : CollectionBase
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��װ����
        /// </summary>
        public CustomDataCollection(StructType DataType)
        {
            this._DataType = DataType;

            this.iRowCount = 0;
            this.iFieldCount = 0;
            this.iCurrentFieldCount = 0;

            this.bExtractData = false;
        }

        /// <summary>
        /// ���캯�� -- �������
        /// </summary>
        /// <param name="pBuffer">������</param>
        internal CustomDataCollection(byte[] pBuffer)
        {
            this.InnerList.Add(pBuffer);

            this.iCurrentFieldCount++;

            this.iRowCount = 0;
            this.iFieldCount = 0;            
            this.bExtractData = true;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ���Ĭ���ַ�������
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public int Add(object pValue)
        {
            //if (pValue.GetType() != typeof(string))
            //{
            //    throw new ArgumentException("�������Ͳ���!");
            //}

            //this.iCurrentFieldCount++;

            //CustomData pDataValue = new CustomData(DataField.MESSAGE, DataFormat.STRING, pValue);
            //return this.InnerList.Add(pDataValue);

            this.iCurrentFieldCount++;

            switch (this._DataType)
            {
                case StructType.CUSTOMDATA:
                    CustomData pDataValue = new CustomData(DataField.MESSAGE, DataFormat.STRING, pValue);
                    return this.InnerList.Add(pDataValue);
                case StructType.SERIALDATA:
                    return this.InnerList.Add(pValue);
                case StructType.BINARYDATA:
                    return this.InnerList.Add(pValue);
                case StructType.LOGDATA:
                    return this.InnerList.Add(pValue);
                default:
                    throw new ArgumentException("ָ�ɵ����ݸ�ʽ����");
            }
        }

        /// <summary>
        /// ���ָ���ֶεĶ�������
        /// </summary>
        /// <param name="eDataField"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public int Add(DataField eDataField, object pValue)
        {
            if (pValue.GetType() != typeof(Byte[]))
            {
                throw new ArgumentException("�������Ͳ���!");
            }

            this.iCurrentFieldCount++;

            CustomData pDataValue = new CustomData(eDataField, DataFormat.NONE, pValue);
            return this.InnerList.Add(pDataValue);
        }

        /// <summary>
        /// ���ָ���ֶΡ�ָ����ʽ�ı���
        /// </summary>
        /// <param name="eDataField"></param>
        /// <param name="eDataFormat"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public int Add(DataField eDataField, DataFormat eDataFormat, object pValue)
        {
            this.iCurrentFieldCount++;

            CustomData pDataValue = new CustomData(eDataField, eDataFormat, pValue);
            return this.InnerList.Add(pDataValue);
        }

        /// <summary>
        /// �����µ�������
        /// </summary>
        /// <returns>��Ч��������</returns>
        public int AddRows()
        {
            this.iFieldCount = this.iCurrentFieldCount;
            this.iCurrentFieldCount = 0;

            return this.iRowCount++;
        }

        /// <summary>
        /// ��ȡ��ǩ����ֵ
        /// </summary>
        /// <param name="eFieldName"></param>
        /// <returns></returns>
        public int IndexOf(DataField eFieldName)
        {
            int iResultIndex = -1;

            for (int i = 0; i < base.InnerList.Count; i++)
            {
                if (this.bExtractData)
                {
                    byte[] pFieldBuffer = new byte[PacketConstant.MAX_FIELDNAME_SIZE];

                    Array.Copy((byte[])(this.InnerList[i]), 0, pFieldBuffer, 0, PacketConstant.MAX_FIELDNAME_SIZE);

                    if ((uint)eFieldName == BitConverter.ToUInt32(pFieldBuffer, 0))
                    {
                        iResultIndex = i;
                        break;
                    }
                }
                else
                {
                    if (((CustomData)this.InnerList[i]).Field == eFieldName)
                    {
                        iResultIndex = i;
                        break;
                    }
                }
            }

            return iResultIndex;
        }

        /// <summary>
        /// ��װ����
        /// </summary>
        /// <param name="iBlockSize">���ݿ��С</param>
        /// <returns>������</returns>
        public byte[] CoalitionInfo(int iBlockSize)
        {
            //���ü��
            if (this.bExtractData)
            {
                throw new ArgumentException("�������ô���");
            }

            if (this.iFieldCount * this.iRowCount != this.InnerList.Count)
            {
                throw new ArgumentException("Ԫ�ظ�������������ݾ���");
            }

            //��װ����
            int iBlockLoop = 0;
            int iCurrentLength = 0;
            byte[] pResultBuffer = new byte[0];

            for (int i = 0; i < this.InnerList.Count; i++)
            {
                //�������                
                int iIndex = pResultBuffer.Length;                                      //��ǰλ��
                byte[] pElementBuffer = ((CustomData)base.InnerList[i]).DataToBuffer(); //������
                byte[] pLengthBuffer = BitConverter.GetBytes(pElementBuffer.Length);    //���ݳ���

                //if (pElementBuffer.Length == 511)
                //{
                //    string abb = "1234";
                //}

                Array.Resize(ref pResultBuffer, iIndex + PacketConstant.MAX_DATALENGTH_SIZE + pElementBuffer.Length);//���������С
                Array.Copy(pLengthBuffer, 0, pResultBuffer, iIndex, pLengthBuffer.Length);//���ó���
                Array.Copy(pElementBuffer, 0, pResultBuffer, iIndex + PacketConstant.MAX_DATALENGTH_SIZE, pElementBuffer.Length);//��������

                if ((this.iFieldCount > 0) && ((i + 1) % this.iFieldCount == 0))
                {
                    //��������־
                    Array.Resize(ref pResultBuffer, pResultBuffer.Length + PacketConstant.MAX_SEPARATELENGTH_SIZE);
                    Array.Copy(PacketConstant.ROW_SEPARATE_CONTENT, 0, pResultBuffer, pResultBuffer.Length - PacketConstant.MAX_SEPARATELENGTH_SIZE, PacketConstant.MAX_SEPARATELENGTH_SIZE);

                    //��������ݿ��С
                    if (iBlockSize > 0)
                    {
                        if (pResultBuffer.Length - iCurrentLength * iBlockLoop >= iBlockSize)
                        {
                            throw new ArgumentException("Ԥ�����ݿ鳤��̫С��", "���ݿ���С����Ӧ��Ϊ��" + (pResultBuffer.Length + PacketConstant.MAX_SEPARATELENGTH_SIZE));
                        }
                        else
                        {
                            iBlockLoop++;
                            iCurrentLength = iBlockSize;

                            Array.Resize(ref pResultBuffer, iBlockSize * iBlockLoop);
                        }
                    }
                }
            }

            return pResultBuffer;
        }

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="pBuffer">��Ч��������</param>
        /// <returns></returns>
        /// <remarks>����������</remarks>
        internal int Add(byte[] pBuffer)
        {
            this.iCurrentFieldCount++;

            return this.InnerList.Add(pBuffer);
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// ������
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.iRowCount;
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        public int FieldCount
        {
            get
            {
                return this.iFieldCount;
            }
        }

        /// <summary>
        /// �����ֶ�
        /// </summary>
        public DataField[] Fields
        {
            get
            {
                if (this.iFieldCount > 0)
                {

                    DataField[] pFieldArray = new DataField[this.iFieldCount];

                    for (int i = 0; i < this.iFieldCount; i++)
                    {
                        if (this.bExtractData)
                        {
                            CustomData pFieldInfo = new CustomData((byte[])this.InnerList[i]);

                            pFieldArray[i] = pFieldInfo.Field;
                        }
                        else
                        {
                            pFieldArray[i] = ((CustomData)this.InnerList[i]).Field;
                        }
                    }

                    return pFieldArray;
                }
                else
                {
                    throw new ArgumentException("�����ֶ�Ϊ�գ�");
                }
            }
        }

        /// <summary>
        /// ƥ������
        /// </summary>
        /// <param name="parameter">������ʽ��-1����ͨ���飻-2���������飻...</param>
        /// <returns>��������</returns>
        public CustomData[,] this[object p]
        {
            get
            {
                if ((this.InnerList.Count > 0) && (this.iFieldCount > 0) && (this.InnerList.Count / this.iRowCount == this.iFieldCount))
                {
                    int iFieldIndex = 0;
                    CustomData[,] pDataArray = new CustomData[this.iRowCount, this.iFieldCount];

                    for (int i = 0; i < this.iRowCount; i++)
                    {
                        for (int j = 0; j < this.iFieldCount; j++)
                        {
                            if (this.bExtractData)
                            {
                                pDataArray[i, j] = new CustomData((byte[])this.InnerList[iFieldIndex]);
                            }
                            else
                            {
                                pDataArray[i, j] = (CustomData)this.InnerList[iFieldIndex];
                            }

                            iFieldIndex++;
                        }
                    }

                    return pDataArray;
                }
                else
                {
                    return new CustomData[,] {
                                    { new CustomData(DataField.MESSAGE, DataFormat.STRING, "ϵͳ��Ϣ��") },
                                    { new CustomData(DataField.MESSAGE, DataFormat.STRING, "��¼��Ϊ�գ�") }
                                };
                }
            }
        }
        #endregion

        #region ��������
        private StructType _DataType;
        private bool bExtractData;
        private int iRowCount;
        private int iFieldCount;
        private int iCurrentFieldCount;
        #endregion
    }

    /// <summary>
    /// �Զ������ݽṹ��
    /// </summary>
    public class CustomData
    {
        #region ����/��������
        /// <summary>
        /// ���캯�� -- ��������
        /// </summary>
        /// <param name="pBuffer">������</param>
        internal CustomData(byte[] pBuffer)
        {
            this._pContent = pBuffer;

            this.BufferToData();
        }

        /// <summary>
        /// ���캯�� -- ��װ����
        /// </summary>
        /// <param name="eField">�ֶα�ǩ</param>
        /// <param name="eFormat">��ʽ��ǩ</param>
        /// <param name="pContent">����</param>
        internal CustomData(DataField eField, DataFormat eFormat, object pContent)
        {
            this._eField = eField;
            this._eFormat = eFormat;
            this._pContent = pContent;
        }

        /// <summary>
        /// ���� ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string strReturn = String.Format(
                                    "Data Info : \r\n\a\tField Name : {0}\r\n\a\tField Format : {1}\r\n\a\tField Content : {2}",
                                    this.Field, this.Format, this.Content
                                            );
            return strReturn;
        }
        #endregion

        #region ��������
        /// <summary>
        /// ת����Buffer
        /// </summary>
        internal byte[] DataToBuffer()
        {
            //�ֶ�
            byte[] pFieldBytes = BitConverter.GetBytes((uint)this._eField);
            //��ʽ
            byte[] pFormatBytes = BitConverter.GetBytes((sbyte)this._eFormat);
            //����
            byte[] pContentBytes;

            if (this._pContent.GetType() == typeof(DBNull))
            {
                pContentBytes = new byte[] { };
            }
            else
            {
                pContentBytes = DataUtilise.DataToBuffer(this._eFormat, this._pContent);
            }

            //���
            byte[] pResult = new byte[PacketConstant.MAX_DATAHEAD_SIZE + pContentBytes.Length];
            Array.Copy(pFieldBytes, 0, pResult, 0, PacketConstant.MAX_FIELDNAME_SIZE);
            Array.Copy(pFormatBytes, 0, pResult, PacketConstant.MAX_FIELDNAME_SIZE, PacketConstant.MAX_FIELDFORMAT_SIZE);
            Array.Copy(pContentBytes, 0, pResult, PacketConstant.MAX_DATAHEAD_SIZE, pContentBytes.Length);

            return pResult;
        }

        /// <summary>
        /// ��������
        /// </summary>
        private void BufferToData()
        {
            try
            {
                //������
                byte[] pBuffer = null;

                //����Data Field
                pBuffer = new byte[PacketConstant.MAX_FIELDNAME_SIZE];
                Array.Copy((byte[])this._pContent, 0, pBuffer, 0, PacketConstant.MAX_FIELDNAME_SIZE);
                this._eField = (DataField)BitConverter.ToUInt32(pBuffer, 0);

                //����Data Format
                pBuffer = new byte[PacketConstant.MAX_FIELDFORMAT_SIZE];
                Array.Copy((byte[])this._pContent, PacketConstant.MAX_FIELDNAME_SIZE, pBuffer, 0, PacketConstant.MAX_FIELDFORMAT_SIZE);
                this._eFormat = (DataFormat)pBuffer[0];

                //����Data Content
                pBuffer = new byte[((byte[])this._pContent).Length - PacketConstant.MAX_DATAHEAD_SIZE];
                Array.Copy((byte[])this._pContent, PacketConstant.MAX_DATAHEAD_SIZE, pBuffer, 0, pBuffer.Length);

                if (pBuffer.Length == 0)
                {
                    this._pContent = DBNull.Value;
                }
                else
                {
                    this._pContent = DataUtilise.BufferToDate(this._eFormat, pBuffer);
                }
            }
            catch (Exception ex)
            {
                this._eField = DataField.MESSAGE;
                this._eFormat = DataFormat.STRING;
                this._pContent = String.Format("Extract Error �� \r\n\a\t{0}", ex.Message);
            }
        }
        #endregion

        #region ���Զ���
        /// <summary>
        /// �ֶ�
        /// </summary>
        public DataField Field
        {
            get
            {
                return this._eField;
            }
        }

        /// <summary>
        /// ��ʽ
        /// </summary>
        public DataFormat Format
        {
            get
            {
                return this._eFormat;
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        public object Content
        {
            get
            {
                return this._pContent;
            }
        }
        #endregion

        #region ��������
        private DataField _eField;
        private DataFormat _eFormat;
        private object _pContent;
        #endregion
    }
}
