using System.Threading.Tasks;
using System;
using Tgs.Promotion.ReliefMoney.Client;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettleReliefDailyWorkerService
{
    Task ManualExecutionAsync(DateOnly date, int hour);
}

public class PromotionSettleReliefDailyWorkerService : IPromotionSettleReliefDailyWorkerService
{
    private readonly IPromotionReliefClient _client;

    public PromotionSettleReliefDailyWorkerService(
        IPromotionReliefClient client)
    {
        _client = client;
    }

    Task IPromotionSettleReliefDailyWorkerService.ManualExecutionAsync(DateOnly date, int hour)
    {
        return _client.SettleAsync(date, hour);
    }
}