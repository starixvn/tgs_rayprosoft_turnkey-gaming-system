using System;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Fill.In.Player.Bucket.Client;

namespace TGS.Workers.Schedulers.Jobs.Rounds;

public interface IFillPlayerBucketMinutelyWorkerService
{
    Task<TgsResponse> ExecutionAsync();

    Task<TgsResponse> ManualExecutionAsync(DateTime startDate);
}

public class FillPlayerBucketMinutelyWorkerService : IFillPlayerBucketMinutelyWorkerService
{
    private readonly IFillInPlayerBucketClient _client;

    public FillPlayerBucketMinutelyWorkerService(
        IFillInPlayerBucketClient client)
        => _client = client;

    async Task<TgsResponse> IFillPlayerBucketMinutelyWorkerService.ExecutionAsync()
    {
        var startDate = DateTimeOffset.UtcNow.AddMinutes(-3);

        var start = new DateTimeOffset(startDate.Year, startDate.Month, startDate.Day,
            startDate.Hour,
            startDate.Minute, 0, new(0,0,0));

        await _client.FillPlayerBucketMinutelyAsync(new(start.UtcDateTime));

        return default;
    }

    async Task<TgsResponse> IFillPlayerBucketMinutelyWorkerService.ManualExecutionAsync(DateTime startDate)
    {
        await _client.FillPlayerBucketMinutelyAsync(new(startDate));

        return default;
    }
}