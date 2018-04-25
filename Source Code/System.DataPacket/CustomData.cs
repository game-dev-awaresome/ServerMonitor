using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Global;
using System.Define;

namespace System.DataPacket
{
    /// <summary>
    /// 自定义数据结构集合类
    /// </summary>
    public class CustomDataCollection : CollectionBase
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数 -- 封装数据
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
        /// 构造函数 -- 拆封数据
        /// </summary>
        /// <param name="pBuffer">数据流</param>
        internal CustomDataCollection(byte[] pBuffer)
        {
            this.InnerList.Add(pBuffer);

            this.iCurrentFieldCount++;

            this.iRowCount = 0;
            this.iFieldCount = 0;            
            this.bExtractData = true;
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 添加默认字符串变量
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public int Add(object pValue)
        {
            //if (pValue.GetType() != typeof(string))
            //{
            //    throw new ArgumentException("数据类型不符!");
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
                    throw new ArgumentException("指派的数据格式错误！");
            }
        }

        /// <summary>
        /// 添加指定字段的二进制流
        /// </summary>
        /// <param name="eDataField"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public int Add(DataField eDataField, object pValue)
        {
            if (pValue.GetType() != typeof(Byte[]))
            {
                throw new ArgumentException("数据类型不符!");
            }

            this.iCurrentFieldCount++;

            CustomData pDataValue = new CustomData(eDataField, DataFormat.NONE, pValue);
            return this.InnerList.Add(pDataValue);
        }

        /// <summary>
        /// 添加指定字段、指定格式的变量
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
        /// 增加新的数据行
        /// </summary>
        /// <returns>有效的数据行</returns>
        public int AddRows()
        {
            this.iFieldCount = this.iCurrentFieldCount;
            this.iCurrentFieldCount = 0;

            return this.iRowCount++;
        }

        /// <summary>
        /// 获取标签索引值
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
        /// 封装数据
        /// </summary>
        /// <param name="iBlockSize">数据块大小</param>
        /// <returns>数据流</returns>
        public byte[] CoalitionInfo(int iBlockSize)
        {
            //调用检测
            if (this.bExtractData)
            {
                throw new ArgumentException("方法调用错误！");
            }

            if (this.iFieldCount * this.iRowCount != this.InnerList.Count)
            {
                throw new ArgumentException("元素个数不能组成数据矩阵！");
            }

            //封装数据
            int iBlockLoop = 0;
            int iCurrentLength = 0;
            byte[] pResultBuffer = new byte[0];

            for (int i = 0; i < this.InnerList.Count; i++)
            {
                //填充数据                
                int iIndex = pResultBuffer.Length;                                      //当前位置
                byte[] pElementBuffer = ((CustomData)base.InnerList[i]).DataToBuffer(); //数据流
                byte[] pLengthBuffer = BitConverter.GetBytes(pElementBuffer.Length);    //数据长度

                //if (pElementBuffer.Length == 511)
                //{
                //    string abb = "1234";
                //}

                Array.Resize(ref pResultBuffer, iIndex + PacketConstant.MAX_DATALENGTH_SIZE + pElementBuffer.Length);//更改数组大小
                Array.Copy(pLengthBuffer, 0, pResultBuffer, iIndex, pLengthBuffer.Length);//放置长度
                Array.Copy(pElementBuffer, 0, pResultBuffer, iIndex + PacketConstant.MAX_DATALENGTH_SIZE, pElementBuffer.Length);//放置数据

                if ((this.iFieldCount > 0) && ((i + 1) % this.iFieldCount == 0))
                {
                    //填充结束标志
                    Array.Resize(ref pResultBuffer, pResultBuffer.Length + PacketConstant.MAX_SEPARATELENGTH_SIZE);
                    Array.Copy(PacketConstant.ROW_SEPARATE_CONTENT, 0, pResultBuffer, pResultBuffer.Length - PacketConstant.MAX_SEPARATELENGTH_SIZE, PacketConstant.MAX_SEPARATELENGTH_SIZE);

                    //填充至数据块大小
                    if (iBlockSize > 0)
                    {
                        if (pResultBuffer.Length - iCurrentLength * iBlockLoop >= iBlockSize)
                        {
                            throw new ArgumentException("预设数据块长度太小！", "数据块最小长度应该为：" + (pResultBuffer.Length + PacketConstant.MAX_SEPARATELENGTH_SIZE));
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
        /// 添加数据流
        /// </summary>
        /// <param name="pBuffer">有效的数据流</param>
        /// <returns></returns>
        /// <remarks>解析数据用</remarks>
        internal int Add(byte[] pBuffer)
        {
            this.iCurrentFieldCount++;

            return this.InnerList.Add(pBuffer);
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 数据行
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.iRowCount;
            }
        }

        /// <summary>
        /// 数据列
        /// </summary>
        public int FieldCount
        {
            get
            {
                return this.iFieldCount;
            }
        }

        /// <summary>
        /// 数据字段
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
                    throw new ArgumentException("数据字段为空！");
                }
            }
        }

        /// <summary>
        /// 匹配数据
        /// </summary>
        /// <param name="parameter">数据样式，-1：普通数组；-2：矩阵数组；...</param>
        /// <returns>多行数据</returns>
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
                                    { new CustomData(DataField.MESSAGE, DataFormat.STRING, "系统信息：") },
                                    { new CustomData(DataField.MESSAGE, DataFormat.STRING, "纪录集为空！") }
                                };
                }
            }
        }
        #endregion

        #region 变量定义
        private StructType _DataType;
        private bool bExtractData;
        private int iRowCount;
        private int iFieldCount;
        private int iCurrentFieldCount;
        #endregion
    }

    /// <summary>
    /// 自定义数据结构类
    /// </summary>
    public class CustomData
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数 -- 解析数据
        /// </summary>
        /// <param name="pBuffer">数据流</param>
        internal CustomData(byte[] pBuffer)
        {
            this._pContent = pBuffer;

            this.BufferToData();
        }

        /// <summary>
        /// 构造函数 -- 封装数据
        /// </summary>
        /// <param name="eField">字段标签</param>
        /// <param name="eFormat">格式标签</param>
        /// <param name="pContent">数据</param>
        internal CustomData(DataField eField, DataFormat eFormat, object pContent)
        {
            this._eField = eField;
            this._eFormat = eFormat;
            this._pContent = pContent;
        }

        /// <summary>
        /// 重载 ToString()
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

        #region 方法定义
        /// <summary>
        /// 转换成Buffer
        /// </summary>
        internal byte[] DataToBuffer()
        {
            //字段
            byte[] pFieldBytes = BitConverter.GetBytes((uint)this._eField);
            //格式
            byte[] pFormatBytes = BitConverter.GetBytes((sbyte)this._eFormat);
            //数据
            byte[] pContentBytes;

            if (this._pContent.GetType() == typeof(DBNull))
            {
                pContentBytes = new byte[] { };
            }
            else
            {
                pContentBytes = DataUtilise.DataToBuffer(this._eFormat, this._pContent);
            }

            //结果
            byte[] pResult = new byte[PacketConstant.MAX_DATAHEAD_SIZE + pContentBytes.Length];
            Array.Copy(pFieldBytes, 0, pResult, 0, PacketConstant.MAX_FIELDNAME_SIZE);
            Array.Copy(pFormatBytes, 0, pResult, PacketConstant.MAX_FIELDNAME_SIZE, PacketConstant.MAX_FIELDFORMAT_SIZE);
            Array.Copy(pContentBytes, 0, pResult, PacketConstant.MAX_DATAHEAD_SIZE, pContentBytes.Length);

            return pResult;
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        private void BufferToData()
        {
            try
            {
                //数据流
                byte[] pBuffer = null;

                //解析Data Field
                pBuffer = new byte[PacketConstant.MAX_FIELDNAME_SIZE];
                Array.Copy((byte[])this._pContent, 0, pBuffer, 0, PacketConstant.MAX_FIELDNAME_SIZE);
                this._eField = (DataField)BitConverter.ToUInt32(pBuffer, 0);

                //解析Data Format
                pBuffer = new byte[PacketConstant.MAX_FIELDFORMAT_SIZE];
                Array.Copy((byte[])this._pContent, PacketConstant.MAX_FIELDNAME_SIZE, pBuffer, 0, PacketConstant.MAX_FIELDFORMAT_SIZE);
                this._eFormat = (DataFormat)pBuffer[0];

                //解析Data Content
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
                this._pContent = String.Format("Extract Error ： \r\n\a\t{0}", ex.Message);
            }
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 字段
        /// </summary>
        public DataField Field
        {
            get
            {
                return this._eField;
            }
        }

        /// <summary>
        /// 格式
        /// </summary>
        public DataFormat Format
        {
            get
            {
                return this._eFormat;
            }
        }

        /// <summary>
        /// 内容
        /// </summary>
        public object Content
        {
            get
            {
                return this._pContent;
            }
        }
        #endregion

        #region 变量定义
        private DataField _eField;
        private DataFormat _eFormat;
        private object _pContent;
        #endregion
    }
}
