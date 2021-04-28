using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.AdditionalClasses;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : RoundRobinClusterClient
    {
        //todo use EventWaitHandle?
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var requests = CreateRequests(
                GetRandomAddressesSequence(), query).ToArray();
            var oneRequestTimeout = CalculateOneTimeout(timeout, requests.Length);
            var runningRequests = new List<Task<RequestResult>>();
            var remainingTimeout = timeout;

            RequestResult completedRequest = null;
            for (int i = 0; i < requests.Length; i++)
            {
                runningRequests.Add(
                    SendRequestAsync(requests[i], remainingTimeout));
                var timeoutTask = CreateTimeoutTask(oneRequestTimeout);
                var requestTask = Task.WhenAny(runningRequests);
                await Task.WhenAny(requestTask, timeoutTask);

                if (timeoutTask.IsCompleted)
                    continue;

                completedRequest = requestTask.Result.Result;

                if (completedRequest.Status == RequestStatus.Success)
                    return completedRequest.ReceivedData;

                runningRequests.Remove(requestTask.Result);

                if (completedRequest.Status == RequestStatus.BadResponse
                    && completedRequest.Uri == requests[i].RequestUri)
                {
                    remainingTimeout -= completedRequest.Duration;
                    oneRequestTimeout = timeout / (requests.Length - i - 1);
                }
            }

            ThrowTimeoutExceptionIfItExceed(completedRequest);
            throw new WebException("Bad response");
        }

        protected async Task<RequestResult> SmartSendRequestAsync(
            WebRequest request, TimeSpan timeout)
        {
            var a = await SendRequestAsync(request, timeout);
            throw new NotImplementedException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}