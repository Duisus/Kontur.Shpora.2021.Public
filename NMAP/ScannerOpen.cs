using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NMAP
{
    public class SequentialScannerOpen
    {
        public virtual Task<TcpClient[]> Scan(IPAddress[] ipAddrs, int port)
        {
            return Task.WhenAll(ipAddrs
                .Select(async ip => await Connect(ip, port))
                .Where(c => c != null));
        }

        protected async Task<TcpClient> Connect(IPAddress ipAddr, int port, int timeout = 3000)
        {
            var tcpClient = new TcpClient();
            
            var connectTask = await tcpClient.ConnectWithTimeoutAsync(ipAddr, port, timeout);

            if (connectTask.Status == TaskStatus.RanToCompletion)
                return tcpClient;
            
            tcpClient.Dispose();
            return null;
        }
    }
}