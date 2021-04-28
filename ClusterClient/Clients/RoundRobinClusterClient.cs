using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClusterClient.AdditionalClasses;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var requests = CreateRequests(
                GetRandomAddressesSequence(), query).ToArray();
            var oneRequestTimeout = CalculateOneTimeout(timeout, requests.Length);
            var remainingTimeout = timeout;

            RequestResult completedRequest = null;
            for (int i = 0; i < requests.Length; i++)
            {
                completedRequest = await SendRequestAsync(requests[i], oneRequestTimeout);
                
                if (completedRequest.Status == RequestStatus.Success)
                    return completedRequest.ReceivedData;
                
                if (completedRequest.Status == RequestStatus.BadResponse)
                {
                    remainingTimeout -= completedRequest.Duration;
                    oneRequestTimeout = remainingTimeout / (requests.Length - i - 1);
                }
            }

            ThrowTimeoutExceptionIfItExceed(completedRequest);
            throw new WebException("Bad response");
        }

        protected IEnumerable<string> GetRandomAddressesSequence()
        {
            return ReplicaAddresses; // TODO delete
            
            var random = new Random();
            return ReplicaAddresses.OrderBy(x => random.Next()); 
        }

        protected TimeSpan CalculateOneTimeout(
            TimeSpan totalTimeout, int addressesCount, TimeSpan? lastDuration = null)
        {
            lastDuration ??= TimeSpan.Zero;
            var remainingTimeout = totalTimeout - (TimeSpan) lastDuration;
            return remainingTimeout / addressesCount;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}