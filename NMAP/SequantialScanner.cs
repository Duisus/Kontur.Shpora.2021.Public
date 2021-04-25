using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
    public class SequentialScanner : IPScanner
    {
        protected virtual ILog log => LogManager.GetLogger(typeof(SequentialScanner));

        public virtual Task Scan(IPAddress[] ipAddrs, int[] ports)
        {
            return Task.WhenAll(ipAddrs.Select(ip => ProcessIp(ip, ports)));
        }

        private async Task ProcessIp(IPAddress ipAddr, int[] ports)
        {
            var status = await PingAddr(ipAddr);

            if (status != IPStatus.Success)
                return;

            await Task.WhenAll(ports.Select(port => CheckPort(ipAddr, port)));
        }

        protected async Task<IPStatus> PingAddr(IPAddress ipAddr, int timeout = 3000)
        {
            log.Info($"Pinging {ipAddr}");
            using(var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(ipAddr, timeout);
                var status = reply.Status;
                log.Info($"Pinged {ipAddr}: {status}");
                return status;
            }
        }

        protected async Task CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
        {
            using(var tcpClient = new TcpClient())
            {
                log.Info($"Checking {ipAddr}:{port}");

                var connectTask = await tcpClient.ConnectWithTimeoutAsync(ipAddr, port, timeout);
                PortStatus portStatus;
                switch(connectTask.Status)
                {
                    case TaskStatus.RanToCompletion:
                        portStatus = PortStatus.OPEN;
                        break;
                    case TaskStatus.Faulted:
                        portStatus = PortStatus.CLOSED;
                        break;
                    default:
                        portStatus = PortStatus.FILTERED;
                        break;
                }
                log.Info($"Checked {ipAddr}:{port} - {portStatus}");
            }
        }
    }
}