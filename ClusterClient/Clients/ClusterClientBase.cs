using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.AdditionalClasses;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase
    {
        protected string[] ReplicaAddresses { get; set; }
        protected string QueryParamName { get; set; } = "query";

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
        }

        public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
        protected abstract ILog Log { get; }

        protected static HttpWebRequest CreateRequest(string uriStr)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        protected async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using (var response = await request.GetResponseAsync())
            {
                var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
                Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
                return result;
            }
        }
        
        protected async Task<IEnumerable<HttpWebRequest>> CreateRequestsAsync(
            IEnumerable<string> addresses, string query)
        {
            return await Task.Run(() => addresses.Select(uri =>
                CreateRequest(GetUriWithQuery(uri, query))));
        }
        
        protected async Task<RequestResult<string>> SendRequestAsync(WebRequest request, TimeSpan timeout)
        {
            var timer = Stopwatch.StartNew();
            var requestTask = ProcessRequestAsync(request);
            var timeoutTask = CreateTimeoutTask(timeout);

            await Task.WhenAny(requestTask, timeoutTask);
            timer.Stop();

            if (timeoutTask.IsCompleted)
                return new RequestResult<string>(RequestStatus.TimeoutExceed, timeout);

            return requestTask.IsFaulted
                ? new RequestResult<string>(RequestStatus.BadResponse, timer.Elapsed)
                : new RequestResult<string>(requestTask.Result, timer.Elapsed);
        }

        protected void ThrowTimeoutExceptionIfItExceed(RequestResult<string> requestResult)
        {
            if (requestResult.Status == RequestStatus.TimeoutExceed)
                throw new TimeoutException(
                    $"Timeout in {requestResult.Duration} ms was exceeded");
        }
        
        protected static Task CreateTimeoutTask(TimeSpan timeout)
        {
            return Task.Delay(timeout);
        }

        protected string GetUriWithQuery(string uri, string paramValue)
        {
            return $"{uri}?{QueryParamName}={paramValue}";
        }
    }
}