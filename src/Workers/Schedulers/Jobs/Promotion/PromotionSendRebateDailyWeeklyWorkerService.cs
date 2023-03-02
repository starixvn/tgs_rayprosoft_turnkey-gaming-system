using System.Threading.Tasks;
using System;
using Tgs.Promotion.RebateService.Client;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSendRebateDailyWeeklyWorkerService
{
    Task ManualExecutionAsync(DateOnly date, int hour);
}

public class PromotionSendRebateDailyWeeklyWorkerService : IPromotionSendRebateDailyWeeklyWorkerService
{
    private readonly IPromotionRebateClient _client;

    public PromotionSendRebateDailyWeeklyWorkerService(
        IPromotionRebateClient client)
    {
        _client = client;
    }

    Task IPromotionSendRebateDailyWeeklyWorkerService.ManualExecutionAsync(DateOnly date, int hour)
    {
        return _client.SendBonusAsync(date, hour);
    }
}