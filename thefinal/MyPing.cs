using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace thefinal
{
    class MyPing
    {
        const int SOCKET_ERROR = -1;
        const int ICMP_ECHO = 8;
        public string PingHost(string host, ref int spentTime)
        {
            IPHostEntry serverHE, fromHE;
            int nBytes = 0;
            int dwStart = 0, dwStop = 0;
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Icmp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);//send超时值
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);//接收超时
            try
            {
                serverHE = Dns.GetHostEntry(host);
            }
            catch (Exception)
            {
                return "解析主机名失败";
            }
            //用IP地址和端口号构造IPEndPoint对象
            IPEndPoint ipepServer = new IPEndPoint(serverHE.AddressList[0], 0);
            EndPoint epServer = (ipepServer);
            //获得本地计算机的EndPint
            fromHE = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint ipEndPointFrom = new IPEndPoint(fromHE.AddressList[0], 80);
            EndPoint EndPointFrom = (ipEndPointFrom);

            int PacketSize = 0;
            IcmpPacket packet = new IcmpPacket();
            //构建ICMP数据包
            //构建数据报 报头字节 数据设计字节
            packet.Type = ICMP_ECHO;//一个字节
            packet.SubCode = 0;
            packet.CheckSum = UInt16.Parse("0");
            packet.Identifier = UInt16.Parse("45");
            packet.SequenceNumber = UInt16.Parse("0");
            int PingData = 32;  //sizeof(icmppacket-8)
            packet.Data = new Byte[PingData];
            //初始化ICMP包的数据部分，即Packet.data
            for (int i = 0; i < PingData; i++)
            {
                packet.Data[i] = (byte)'*';
            }
            //保存数据报长度
            PacketSize = PingData + 8;
            Byte[] icmp_pkt_buffer = new Byte[PacketSize];
            Int32 Index= 0;
            //调用Serialize方法
            //报文总共的字节数
            //序列化数据报，验证数据包大小
            Index= Serialize(packet, icmp_pkt_buffer, PacketSize, PingData);
            if(Index==-1)
            {
                return "创建包失败";
            }
            //将ICMP数据包转换成UInt16数组
            //获取转化后的数组长度
            Double double_length = Convert.ToDouble(Index);
            Double dtemp = Math.Ceiling(double_length / 2);

            int cksum_buffer_length = Convert.ToInt32(dtemp);//20
            //生成一个字节数组
            UInt16[] cksum_buffer = new UInt16[cksum_buffer_length];
            //初始化UInt16类型数组
            int icmp_header_buffer_index = 0;
            for(int i=0;i<cksum_buffer_length;i++)
            {
                cksum_buffer[i] = BitConverter.ToUInt16(icmp_pkt_buffer, icmp_header_buffer_index);
                icmp_header_buffer_index += 2;
            }
            //获取ICMP数据包的校验码
            //调用checksum，返回检查和
            UInt16 u_cksum = checksum(cksum_buffer, cksum_buffer_length);
            //保存校验码
            packet.CheckSum = u_cksum;
            //再次序列化数据包
            //再次检查报的大小
            Byte[] sendbuf = new Byte[PacketSize];
            Index = Serialize(packet, sendbuf, PacketSize, PingData);
            if(Index==-1)
            {
                return "创建包失败";
            }
            dwStart = System.Environment.TickCount;
            if((nBytes = socket.SendTo(sendbuf, PacketSize, 0, epServer))==SOCKET_ERROR)
            {
                return "无法发送Socket";
            }
            //初始化缓冲区，接收缓冲去
            //大小为ICMP报头+IP报头的大小，共60字节
            Byte[] ReceiveBuffer = new Byte[60];
            nBytes = 0;
            //接收字节流
            bool recd = false;
            int timeout = 0;
            //循环检查目标主机响应时间
            while (!recd)
            {
                try
                {
                    nBytes = socket.ReceiveFrom(ReceiveBuffer, 60, SocketFlags.None, ref EndPointFrom);
                    if(nBytes==SOCKET_ERROR)
                    {
                        return "主机未响应";
                    }
                    else if(nBytes>0)
                    {
                        dwStop = System.Environment.TickCount - dwStart;
                        spentTime = dwStop;
                        return "Reply from  " + epServer.ToString() + "  in " + dwStop + "ms.  Received: " + nBytes + "Bytes  " + "TTL=" + PingTTl(host);
                    }
                }
                catch(SocketException e)
                {
                    return "超时";
                }
            }
            socket.Close();
            return "";
        }
        //序列化数据包
        public static Int32 Serialize(IcmpPacket packet,Byte[] Buffer,Int32 PacketSize,Int32 PingData)
        {
            //取得报文内容，转化为字节数组，然后计算报文的长度
            Int32 cbReturn = 0;
            //数据报结构转化为数组
            int Index = 0;
            Byte[] b_type = new Byte[1];
            b_type[0] = (packet.Type);
            Byte[] b_code = new Byte[1];
            b_code[0] = (packet.SubCode);
            Byte[] b_cksum = BitConverter.GetBytes(packet.CheckSum);
            Byte[] b_id = BitConverter.GetBytes(packet.Identifier);
            Byte[] b_seq = BitConverter.GetBytes(packet.SequenceNumber);

            Array.Copy(b_type, 0, Buffer, Index, b_type.Length);
            Index += b_type.Length;
            Array.Copy(b_code, 0, Buffer, Index, b_code.Length);
            Index += b_code.Length;
            Array.Copy(b_cksum, 0, Buffer, Index, b_cksum.Length);
            Index += b_cksum.Length;
            Array.Copy(b_id, 0, Buffer, Index, b_id.Length);
            Index += b_id.Length;
            Array.Copy(b_seq, 0, Buffer, Index, b_seq.Length);
            Index += b_seq.Length;
            //复制数据
            Array.Copy(packet.Data, 0, Buffer, Index, PingData);
            Index += PingData;
            if (Index != PacketSize)
            {
                cbReturn = -1;
                return cbReturn;
            }
            cbReturn = Index;
            return cbReturn;
        }
        //计算数据包的校验码
        public static UInt16 checksum(UInt16[] buffer,int size)
        {
            Int32 cksum = 0;
            int counter = 0;
            //把ICMP报头的二进制数据以字节为单位累加起来
            while(size>0)
            {
                UInt16 val = buffer[counter];
                cksum += Convert.ToInt32(buffer[counter]);
                counter++;
                size--;
            }
            //弱ICMP报头为奇数个字节，就会剩下最后一个字节，把最后一个字节视为一个*2个字节数据的高字节，
            //这个字节数据的低字节继续累加
            cksum = (cksum >> 16) + (cksum & 0xffff);
            cksum += (cksum >> 16);
            return (UInt16)(~cksum);
        }

        public int PingTTl(string host)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingReply reply = pingSender.Send(host, timeout, buffer, options);
            if(reply.Status==IPStatus.Success)
            {
                return reply.Options.Ttl;
            }
            return -1;
        }
    }
}
