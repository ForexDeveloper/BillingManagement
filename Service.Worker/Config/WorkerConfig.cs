using System;

namespace Service.Worker.Config
{
    public class WorkerConfig
    {
        public TimeSpan IntervalBetweenProcessRequest { get; set; } = new(0,0,1);
    }
}
