using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClusterClient.AdditionalClasses;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var requests = CreateRequests(ReplicaAddresses, query);

            var result = await TryGetFirstSuccessRequestAsync(  // todo refactor
                requests
                    .Select(request => SendRequestAsync(request, timeout))
                    .ToList());

            if (result == null)
                throw new WebException("Bad response");

            return result.ReceivedData;
        }

        protected async Task<RequestResult> TryGetFirstSuccessRequestAsync(
            List<Task<RequestResult>> tasks)
        {
            Task<RequestResult> completedTask;
            do
            {
                completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
            } while (completedTask.Result.Status != RequestStatus.Success && tasks.Count > 0);

            ThrowTimeoutExceptionIfItExceed(completedTask.Result);

            return completedTask.Result.Status == RequestStatus.Success
                ? completedTask.Result
                : null;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}