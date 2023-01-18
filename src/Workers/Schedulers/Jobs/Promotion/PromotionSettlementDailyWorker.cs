using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using TGS.Core.Models.Messages;
using TGS.Services.Operations.Queries;
using TGS.Workers.Schedulers.Hangfire.Services;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

[Queue(Consts.Queue.Report)]
public sealed class PromotionSettlementDailyWorker : BaseWork<TgsResponse>
{
    private readonly IPromotionSettlementDailyWorkerService _worker;
    private readonly ISettingQueries _settingQueries;
    private readonly ISystemClock _clock;

    public PromotionSettlementDailyWorker(
        IPromotionSettlementDailyWorkerService worker,
        ISettingQueries settingQueries,
        ISystemClock clock)
    {
        _worker = worker;
        _settingQueries = settingQueries;
        _clock = clock;
    }

    public override async Task<TgsResponse> ExecuteAsync(object context)
    {
        var value = await _settingQueries.GetSettingValueByKeyAsync("PromotionDailyTimeSpanMinutes");

        var timeSpanMinutes = JsonSerializer.Deserialize<Dictionary<int, int?>>(value);

        var hour = _clock.UtcNow.Hour;

        if (timeSpanMinutes?.TryGetValue(hour, out var time) == null)
        {
            return new TgsResponse { ErrorMessage = "can't find defined timeSpanMinutes." };
        }

        if (time != null)
        {
            return await _worker.ExecutionAsync(time.Value);
        }

        return new TgsResponse { ErrorMessage = "can't find defined timeSpanMinutes." };
    }
}