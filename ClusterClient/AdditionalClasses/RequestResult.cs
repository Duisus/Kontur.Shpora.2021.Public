using System;

namespace ClusterClient.AdditionalClasses
{
    public class RequestResult<T>  // TODO delete generic? It is like transport level, generic = byte or string
    {
        public T Result { get; }
        public TimeSpan Duration { get; }
        public RequestStatus Status { get; }

        public RequestResult(T result, TimeSpan duration)
        {
            Result = result;
            Duration = duration;
            Status = RequestStatus.Success;
        }
        
        public RequestResult(RequestStatus status, TimeSpan duration)
        {
            Status = status;
            Duration = duration;
        }
    }
}