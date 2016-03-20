using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*ICMP数据包类*/
namespace thefinal
{
    class IcmpPacket
    {
        public Byte Type;    // 类型:回显请求(8),应答(0)
        public Byte SubCode;    // 编码
        public UInt16 CheckSum;   // 校验码
        public UInt16 Identifier;      // 标识符
        public UInt16 SequenceNumber;     // 序列号
        public Byte[] Data;
    }
}
