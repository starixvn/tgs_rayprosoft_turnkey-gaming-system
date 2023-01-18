using System;

namespace TGS.Workers.Schedulers.Jobs.Promotion
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
    public class SystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
