using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Network
{
    /// <summary>
    /// Socket 常量定义
    /// </summary>
    internal static class SocketConstant
    {
        /// <summary>
        /// 版本号及版权
        /// </summary>
        public const string strVersion = "1.0";
        public const string strCopyRight = "Copyright (C) QQ_QQ WorkShop 2006";

        /// <summary>
        /// Socket Head定义
        /// </summary>
        public const int MAX_SOCKETCOMPRESS_SIZE = 1;
        public const int MAX_SOCKETBODY_SIZE = 4;
        public const int MAX_SOCKETCRC_SIZE = 4;
        public const int MAX_SOCKETHEAD_SIZE = MAX_SOCKETCOMPRESS_SIZE + MAX_SOCKETBODY_SIZE + MAX_SOCKETCRC_SIZE;

        /// <summary>
        /// 线程相关定义
        /// </summary>
        public const int DEF_RESETEVENT_TIMEOUT = 1000;
    }
}
