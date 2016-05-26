using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CS.Diagnostics;
using CS.Http;

namespace CS.SwarmTask.Jobs.Ddns
{
    public class IpHelper
    {
        private static readonly ITracer SysLog = Logger.GetSysLog(typeof(IpHelper));
        private static readonly Regex RegIp = new Regex(@".*?(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}).*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 旧的，老IP
        /// </summary>
        public string OldIp { get; set; }
        /// <summary>
        /// 新的，获取到的IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// IP已改变
        /// </summary>
        public bool IpChanged => OldIp != Ip;

        public IpHelper(string oldip)
        {
            OldIp = oldip;
        }

        public string GetIp()
        {
            try
            {
                Ip = GetIpFromDnsPod();
            }
            catch
            {
                try
                {
                    Ip = GetIpFrom3322();
                }
                catch (Exception ex)
                {
                    SysLog.Error($"获取IP地址时异常", ex);
                }
            }
            return Ip;
        }

        public static string GetIpFromDnsPod()
        {
            var tcpClient = new TcpClient();
            string @string;
            try
            {
                tcpClient.Connect("ns1.dnspod.net", 6666);
                byte[] array = new byte[512];
                int count = tcpClient.GetStream().Read(array, 0, 512);
                @string = Encoding.ASCII.GetString(array, 0, count);
                //SysLog.Debug($"从DNSPod的NS1获得IP地址:{@string}");
            }
            finally
            {
                tcpClient.Close();
            }
            return @string;
        }

        public static string GetIpFrom3322()
        {
            return GetIpByWeb("http://members.3322.org/dyndns/getip", RegIp, "3322.org");
        }

        private static string GetIpByWeb(string url, Regex reg, string domainName)
        {
            var html = HttpHelper.Get(url);
            var matchCollection = reg.Matches(html);
            foreach (Match match in matchCollection)
            {
                return match.Groups[1].Value?.Trim();
            }
            SysLog.Error($"从{domainName}获取IP地址失败。");
            return null;
        }
    }
}