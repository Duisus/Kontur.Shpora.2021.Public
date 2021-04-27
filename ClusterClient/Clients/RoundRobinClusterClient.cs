using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
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
            var requests = (await CreateRequestsAsync(
                GetRandomAddressesSequence(), query)).ToArray();
            var oneRequestTimeout = CalculateOneTimeout(timeout, requests.Length);

            RequestResult<string> result = null;
            for (int i = 0; i < requests.Length; i++)
            {
                result = await SendRequestAsync(requests[i], oneRequestTimeout);
                if (result.Status == RequestStatus.Success)
                    return result.Result;
                if (result.Status == RequestStatus.BadResponse)
                {
                    timeout -= result.Duration;
                    oneRequestTimeout = timeout / (requests.Length - i - 1);
                }
            }
            
            ThrowTimeoutExceptionIfItExceed(result);
            throw new WebException("Bad response");
        }

        protected string[] GetRandomAddressesSequence()
        {
            return ReplicaAddresses; // TODO REALIZE!!!
        }

        protected TimeSpan CalculateOneTimeout(
            TimeSpan totalTimeout, int addressesCount, TimeSpan? lastDuration = null)
        {
            lastDuration ??= TimeSpan.Zero;
            var remainingTimeout = totalTimeout - (TimeSpan)lastDuration;
            return remainingTimeout / addressesCount;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
