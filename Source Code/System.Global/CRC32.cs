using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    /// <summary>
    /// 循环冗余码校验
    /// </summary>
    public class CRC32 : CustomClass
    {
        #region 构造/析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public CRC32()
        {
            this.pCRCHash = new byte[CRC32.iMaxSize];
            this.uiSeek = CRC32.uiAllOnes;
            this.pCRCTable = this.BuildCRCTable(CRC32.uiPolynomial);
        }

        /// <summary>
        /// 重载函数 -- 获取哈西代码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 重载函数 -- 比较方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (typeof(byte[]) == typeof(object))
            {
                for (int i = 0; i < ((byte[])obj).Length; i++)
                {
                    if (((byte[])obj)[i] == this.Value[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                throw new ArgumentException("参数预期类型错误", "预期类型［byte[]］, 实际类型［" + typeof(object).ToString() + "］");
            }

            return true;
        }

        /// <summary>
        /// 重载函数 -- 释放资源
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                this.pCRCHash = null;
                this.pCRCTable = null;
            }

            base.Free(bDispodedByUser);
        }
        #endregion

        #region 方法定义
        /// <summary>
        /// 生成CRC
        /// </summary>
        /// <param name="pBuffer">数据流</param>
        /// <param name="iOffset">起始位</param>
        /// <param name="iSize">长度</param>
        public void ComputeHash(byte[] pBuffer, int iOffset, int iSize)
        {
            for (int i = iOffset; i < iSize; i++)
            {
                ulong ulIndex = (this.uiSeek & 0xFF) ^ pBuffer[i];
                this.uiSeek >>= 8;
                this.uiSeek ^= this.pCRCTable[ulIndex];
            }

            this.HashFinal();
        }

        /// <summary>
        /// CRC计算
        /// </summary>
        protected void HashFinal()
        {
            ulong ulResult = this.uiSeek ^ CRC32.uiAllOnes;
            Array.Copy(BitConverter.GetBytes(ulResult), 0, this.pCRCHash, 0, this.pCRCHash.Length);
        }

        /// <summary>
        /// 创建CRC数据表
        /// </summary>
        /// <param name="uPolynomial"></param>
        /// <returns></returns>
        protected uint[] BuildCRCTable(uint uiPolynomial)
        {
            uint uiSingle;
            uint[] pCRCTable = new uint[256];

            for (int i = 0; i < 256; i++)
            {
                uiSingle = (uint)i;

                for (int j = 8; j > 0; j--)
                {
                    if ((uiSingle & 1) == 1)
                    {
                        uiSingle = (uiSingle >> 1) ^ uiPolynomial;
                    }
                    else
                    {
                        uiSingle >>= 1;
                    }
                }

                pCRCTable[i] = uiSingle;
            }

            return pCRCTable;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxSize
        {
            get
            {
                return CRC32.iMaxSize;
            }
        }

        /// <summary>
        /// 校验值
        /// </summary>
        public byte[] Value
        {
            get
            {
                return this.pCRCHash;
            }
            set
            {
                this.pCRCHash = value;
            }
        }
        #endregion

        #region 变量定义
        protected const int iMaxSize = 4;
        protected const uint uiAllOnes = 0xFFFFFFFF;
        protected const uint uiPolynomial = 0x04C11DB7;

        private uint uiSeek;
        private uint[] pCRCTable;
        private byte[] pCRCHash;
        #endregion
    }
}
