using System;

namespace TGS.Workers.Schedulers.Jobs.Promotion
{
    public interface IDateTimeOffsetExtension
    {
        DateTimeOffset UtcNow { get; }
    }
    public class DateTimeOffsetExtension : IDateTimeOffsetExtension
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
