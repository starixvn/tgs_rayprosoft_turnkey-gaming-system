using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Promotion.Clients;
using System;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettlementWeeklyWorkerService
{
    Task<TgsResponse> ExecutionAsync(int timeSpanMinutes);
    Task<TgsResponse> ManualExecutionAsync(DateOnly from, DateOnly to, int timeSpanMinutes);
}

public class PromotionSettlementWeeklyWorkerService : IPromotionSettlementWeeklyWorkerService
{
    private readonly IPromotionBonusClient _client;

    public PromotionSettlementWeeklyWorkerService(
        IPromotionBonusClient client)
    {
        _client = client;
    }

    async Task<TgsResponse> IPromotionSettlementWeeklyWorkerService.ExecutionAsync(int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataWeeklyAsync(new(timeSpanMinutes)).ConfigureAwait(false);
        return new() {ErrorMessage = response.Message};
    }

    async Task<TgsResponse> IPromotionSettlementWeeklyWorkerService.ManualExecutionAsync(DateOnly from,
        DateOnly to, int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataWeeklyAsync(new(timeSpanMinutes, from, to)).ConfigureAwait(false);
        return new() {ErrorMessage = response.Message};
    }
}