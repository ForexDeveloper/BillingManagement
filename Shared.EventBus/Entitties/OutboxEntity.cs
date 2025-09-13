using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.EventBus.Entitties
{
    public enum PublishStatus
    {
        InProgress = 1,
        Published = 2,
        Failed = 3
    }
    public class OutboxEntity
    {
        public long Id { get; private set; }
        public string EventName { get; private set; }
        public string Message { get; private set; }
        public DateTime CreateDateTime { get; private set; }
        public DateTime EditDateTime { get; private set; }
        public int PublishTryCount { get; private set; }
        public PublishStatus Status { get; private set; }


        public OutboxEntity(string eventName, string message)
        {
            EventName = eventName;
            Message = message;
            Status = PublishStatus.InProgress;
            CreateDateTime = DateTime.Now;
            EditDateTime = DateTime.Now;
            PublishTryCount = 0;
        }
        public void PrepareToPublish()
        {
            EditDateTime = DateTime.Now;
            PublishTryCount++;
        }

        public void GotoProperState(bool publishSuccess)
        {
            if (publishSuccess)
            {
                Status = PublishStatus.Published;
            }
            else if (PublishTryCount > 1000)
            {
                Status = PublishStatus.Failed;
            }
        }
    }


}
