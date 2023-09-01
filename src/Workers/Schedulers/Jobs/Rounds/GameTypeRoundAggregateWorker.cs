using System.Threading.Tasks;
using Hangfire;
using TGS.Core.Models.Messages;
using TGS.Workers.Schedulers.Hangfire.Services;

namespace TGS.Workers.Schedulers.Jobs.Rounds;

[Queue(Consts.Queue.Report)]
public sealed class GameTypeRoundAggregateWorker : BaseWork<TgsResponse>
{
    private readonly IGameTypeRoundAggregateWorkerService _worker;

    public GameTypeRoundAggregateWorker(
        IGameTypeRoundAggregateWorkerService worker)
    {
        _worker = worker;
    }

    public override Task<TgsResponse> ExecuteAsync(object context) => _worker.ExecutionAsync();
}