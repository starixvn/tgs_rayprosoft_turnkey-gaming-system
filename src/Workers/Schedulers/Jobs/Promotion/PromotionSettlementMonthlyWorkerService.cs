using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Promotion.Clients;
using TGS.Services.Operations.Queries;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

public interface IPromotionSettlementMonthlyWorkerService
{
    Task<TgsResponse> ExecutionAsync(int timeSpanMinutes);
    Task<TgsResponse> ManualExecutionAsync(int timeSpanMinutes);
}

public class PromotionSettlementMonthlyWorkerService : IPromotionSettlementMonthlyWorkerService
{
    private readonly IPromotionBonusClient _client;
    private readonly ISettingQueries _settingQueries;

    public PromotionSettlementMonthlyWorkerService(
        IPromotionBonusClient client,
        ISettingQueries settingQueries)
    {
        _client = client;
        _settingQueries = settingQueries;
    }

    async Task<TgsResponse> IPromotionSettlementMonthlyWorkerService.ExecutionAsync(int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataMonthlyAsync(new(timeSpanMinutes)).ConfigureAwait(false);
        return new() {ErrorMessage = response.Message};
    }

    async Task<TgsResponse> IPromotionSettlementMonthlyWorkerService.ManualExecutionAsync(int timeSpanMinutes)
    {
        var response = await _client.CreatePromotionSettleDataMonthlyAsync(new(timeSpanMinutes)).ConfigureAwait(false);
        return new() {ErrorMessage = response.Message};
    }
}