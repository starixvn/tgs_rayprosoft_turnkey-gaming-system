using System;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Report.Aggregate.Client;

namespace TGS.Workers.Schedulers.Jobs.Rounds;

public interface IFillPlayerBucketMinutelyWorkerService
{
    Task<TgsResponse> ExecutionAsync();

    Task<TgsResponse> ManualExecutionAsync(string date, int hour, int fromMinute, int toMinute,
        bool fillPlayerBucket = true);
}

public class FillPlayerBucketMinutelyWorkerService : IFillPlayerBucketMinutelyWorkerService
{
    private readonly IReportAggregateClient _client;

    public FillPlayerBucketMinutelyWorkerService(
        IReportAggregateClient client)
        => _client = client;

    async Task<TgsResponse> IFillPlayerBucketMinutelyWorkerService.ExecutionAsync()
    {
        var startDate = DateTimeOffset.UtcNow.AddMinutes(-3);
        var response = await _client.FillPlayerBucketMinutelyAsync(new(startDate));
        return new() { ErrorMessage = response.Message };
    }

    async Task<TgsResponse> IFillPlayerBucketMinutelyWorkerService.ManualExecutionAsync(string date, int hour,
        int fromMinute, int toMinute, bool fillPlayerBucket)
    {
        var response =
            await _client.ManualFillPlayerBucketMinutelyAsync(new(date,
                hour,
                fromMinute,
                toMinute,
                fillPlayerBucket));
        return new() { ErrorMessage = response.Message };
    }
}