using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Game.Type.Round.Aggregate.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TGS.Workers.Schedulers.Jobs.Rounds;

public interface IGameTypeRoundAggregateWorkerService
{
    Task<TgsResponse> ExecutionAsync();
    Task<TgsResponse> ManualExecutionAsync(DateTime endDate, int count);
}

public class GameTypeRoundAggregateWorkerService : IGameTypeRoundAggregateWorkerService
{
    private readonly IGameTypeRoundClient _client;

    public GameTypeRoundAggregateWorkerService(
        IGameTypeRoundClient client)
        => _client = client;

    public async Task<TgsResponse> ExecutionAsync()
    {
        var intervals = GenerateReverseTimeIntervals(DateTime.UtcNow, 4);

        foreach (var (_, from, to) in intervals)
        {
            await _client.GenerateGameTypeRoundsAsync(from, to).ConfigureAwait(false);

            await Task.Delay(3000);
        }

        return default;
    }

    public async Task<TgsResponse> ManualExecutionAsync(DateTime endDate, int count)
    {
        var intervals = GenerateReverseTimeIntervals(endDate, count);

        foreach (var (_, from, to) in intervals)
        {
            await _client.GenerateGameTypeRoundsAsync(from, to).ConfigureAwait(false);

            await Task.Delay(3000);
        }

        return default;
    }

    private static IEnumerable<(int number, DateTime from, DateTime to)> GenerateReverseTimeIntervals(DateTime endDate, int count)
    {
        var startDate = endDate;

        startDate = startDate.AddMinutes(-(startDate.Minute % 15)).AddSeconds(-startDate.Second);

        return Enumerable.Range(1, count).Select(count =>
            {
                var nextStartDate = startDate.AddMinutes(-15);
                var toDate = startDate;
                startDate = nextStartDate;

                return (count, nextStartDate, toDate);
            })
            .OrderByDescending(x => x.count)
            .ToArray();
    }
}