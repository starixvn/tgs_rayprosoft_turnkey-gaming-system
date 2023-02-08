using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Promotion.Clients;
using Hangfire.Server;
using System.Collections.Generic;
using System;
using System.Text.Json;
using TGS.Services.Operations.Queries;
using Tgs.Promotion.Clients.Models.PromotionBonus;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettlementDailyWorkerService
{
    Task<TgsResponse> ExecutionAsync(int timeSpanMinutes);
    Task<TgsResponse> ManualExecutionAsync(DateOnly date, int timeSpanMinutes);
}

public class PromotionSettlementDailyWorkerService : IPromotionSettlementDailyWorkerService
{
    private readonly IPromotionBonusClient _client;

    public PromotionSettlementDailyWorkerService(
        IPromotionBonusClient client)
    {
        _client = client;
    }

    async Task<TgsResponse> IPromotionSettlementDailyWorkerService.ExecutionAsync(int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataDailyAsync(new (timeSpanMinutes)).ConfigureAwait(false);
        return new() { ErrorMessage = response.Message };
    }

    async Task<TgsResponse> IPromotionSettlementDailyWorkerService.ManualExecutionAsync(DateOnly date, int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataDailyAsync(new (timeSpanMinutes, date)).ConfigureAwait(false);
        return new() {ErrorMessage = response.Message};
    }
}