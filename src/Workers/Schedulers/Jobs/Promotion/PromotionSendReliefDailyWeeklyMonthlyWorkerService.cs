using System.Threading.Tasks;
using System;
using Tgs.Promotion.ReliefMoney.Client;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSendReliefDailyWeeklyMonthlyWorkerService
{
    Task ManualExecutionAsync(DateOnly date, int hour);
}

public class PromotionSendReliefDailyWeeklyMonthlyWorkerService : IPromotionSendReliefDailyWeeklyMonthlyWorkerService
{
    private readonly IPromotionReliefClient _client;

    public PromotionSendReliefDailyWeeklyMonthlyWorkerService(
        IPromotionReliefClient client)
    {
        _client = client;
    }

    Task IPromotionSendReliefDailyWeeklyMonthlyWorkerService.ManualExecutionAsync(DateOnly date, int hour)
    {
        return _client.SendBonusAsync(date, hour);
    }
}