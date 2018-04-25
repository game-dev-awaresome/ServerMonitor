using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    /// <summary>
    /// ѭ��������У��
    /// </summary>
    public class CRC32 : CustomClass
    {
        #region ����/��������
        /// <summary>
        /// ���캯��
        /// </summary>
        public CRC32()
        {
            this.pCRCHash = new byte[CRC32.iMaxSize];
            this.uiSeek = CRC32.uiAllOnes;
            this.pCRCTable = this.BuildCRCTable(CRC32.uiPolynomial);
        }

        /// <summary>
        /// ���غ��� -- ��ȡ��������
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// ���غ��� -- �ȽϷ���
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
                throw new ArgumentException("����Ԥ�����ʹ���", "Ԥ�����ͣ�byte[]��, ʵ�����ͣ�" + typeof(object).ToString() + "��");
            }

            return true;
        }

        /// <summary>
        /// ���غ��� -- �ͷ���Դ
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

        #region ��������
        /// <summary>
        /// ����CRC
        /// </summary>
        /// <param name="pBuffer">������</param>
        /// <param name="iOffset">��ʼλ</param>
        /// <param name="iSize">����</param>
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
        /// CRC����
        /// </summary>
        protected void HashFinal()
        {
            ulong ulResult = this.uiSeek ^ CRC32.uiAllOnes;
            Array.Copy(BitConverter.GetBytes(ulResult), 0, this.pCRCHash, 0, this.pCRCHash.Length);
        }

        /// <summary>
        /// ����CRC���ݱ�
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

        #region ���Զ���
        /// <summary>
        /// ��󳤶�
        /// </summary>
        public int MaxSize
        {
            get
            {
                return CRC32.iMaxSize;
            }
        }

        /// <summary>
        /// У��ֵ
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

        #region ��������
        protected const int iMaxSize = 4;
        protected const uint uiAllOnes = 0xFFFFFFFF;
        protected const uint uiPolynomial = 0x04C11DB7;

        private uint uiSeek;
        private uint[] pCRCTable;
        private byte[] pCRCHash;
        #endregion
    }
}
