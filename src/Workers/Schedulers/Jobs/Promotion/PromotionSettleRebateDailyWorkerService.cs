using System.Threading.Tasks;
using System;
using Tgs.Promotion.RebateService.Client;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettleRebateDailyWorkerService
{
    Task ManualExecutionAsync(DateOnly date, int hour);
}

public class PromotionSettleRebateDailyWorkerService : IPromotionSettleRebateDailyWorkerService
{
    private readonly IPromotionRebateClient _client;

    public PromotionSettleRebateDailyWorkerService(
        IPromotionRebateClient client)
    {
        _client = client;
    }

    Task IPromotionSettleRebateDailyWorkerService.ManualExecutionAsync(DateOnly date, int hour)
    {
        return _client.SettleAsync(date, hour);
    }
}