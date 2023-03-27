using System.Threading.Tasks;
using System;
using Tgs.Promotion.RebateService.Client;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettleDailyWorkerService
{
    Task ManualExecutionAsync(DateOnly date, int hour);
}

public class PromotionSettleDailyWorkerService : IPromotionSettleDailyWorkerService
{
    private readonly IPromotionRebateClient _client;

    public PromotionSettleDailyWorkerService(
        IPromotionRebateClient client)
    {
        _client = client;
    }

    Task IPromotionSettleDailyWorkerService.ManualExecutionAsync(DateOnly date, int hour)
    {
        return _client.SettleAsync(date, hour);
    }
}