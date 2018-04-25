using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Service
{
    public delegate void SocketEvent(object pObject);

    public enum NetServiceType : sbyte
    {
        HOST = 0x0A,
        CLIENT = 0x0B
    }
}
