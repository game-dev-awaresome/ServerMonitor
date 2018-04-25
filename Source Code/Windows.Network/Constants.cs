using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Network
{
    /// <summary>
    /// Socket ��������
    /// </summary>
    internal static class SocketConstant
    {
        /// <summary>
        /// �汾�ż���Ȩ
        /// </summary>
        public const string strVersion = "1.0";
        public const string strCopyRight = "Copyright (C) QQ_QQ WorkShop 2006";

        /// <summary>
        /// Socket Head����
        /// </summary>
        public const int MAX_SOCKETCOMPRESS_SIZE = 1;
        public const int MAX_SOCKETBODY_SIZE = 4;
        public const int MAX_SOCKETCRC_SIZE = 4;
        public const int MAX_SOCKETHEAD_SIZE = MAX_SOCKETCOMPRESS_SIZE + MAX_SOCKETBODY_SIZE + MAX_SOCKETCRC_SIZE;

        /// <summary>
        /// �߳���ض���
        /// </summary>
        public const int DEF_RESETEVENT_TIMEOUT = 1000;
    }
}
