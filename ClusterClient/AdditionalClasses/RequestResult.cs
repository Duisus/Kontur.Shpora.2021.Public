using System;

namespace ClusterClient.AdditionalClasses
{
    public class RequestResult
    {
        public Uri Uri { get; }
        public string ReceivedData { get; }
        public TimeSpan Duration { get; }
        public RequestStatus Status { get; }

        private RequestResult(
            string receivedData, TimeSpan duration, RequestStatus status, Uri uri)
        {
            ReceivedData = receivedData;
            Duration = duration;
            Status = status;
            Uri = uri;
        }

        public RequestResult(string receivedData, TimeSpan duration, Uri uri)
            : this(receivedData, duration, RequestStatus.Success, uri)
        {
        }

        public RequestResult(RequestStatus status, TimeSpan duration, Uri uri) 
            : this(default, duration, status, uri)
        {
        }
    }
}