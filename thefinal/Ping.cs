using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace thefinal
{
    class Ping
    {

        /// <summary>
        /// 域名转换为IP地址
        /// </summary>
        /// <param name="hostname">域名或IP地址</param>
        /// <returns>IP地址</returns>
        public string Hostname2ip(string hostname)
        {
            try
            {
                IPAddress ip;
                if (IPAddress.TryParse(hostname, out ip))
                    return ip.ToString();
                else
                    return Dns.GetHostEntry(hostname).AddressList[0].ToString();
            }
            catch (Exception)
            {
                throw new Exception("IP Address Error");
            }
        }
    }
}
