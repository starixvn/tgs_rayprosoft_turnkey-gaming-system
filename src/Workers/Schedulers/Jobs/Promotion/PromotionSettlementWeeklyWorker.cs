using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using TGS.Core.Models.Messages;
using TGS.Workers.Schedulers.Hangfire.Services;
using TGS.Services.Operations.Queries;

namespace TGS.Workers.Schedulers.Jobs.Promotion;

[Queue(Consts.Queue.Report)]
public sealed class PromotionSettlementWeeklyWorker : BaseWork<TgsResponse>
{
    private readonly IPromotionSettlementWeeklyWorkerService _worker;
    private readonly ISettingQueries _settingQueries;
    private readonly ISystemClock _clock;

    public PromotionSettlementWeeklyWorker(
        IPromotionSettlementWeeklyWorkerService worker,
        ISettingQueries settingQueries,
        ISystemClock clock)
    {
        _worker = worker;
        _settingQueries = settingQueries;
        _clock = clock;
    }

    public override async Task<TgsResponse> ExecuteAsync(object context)
    {
        var value = await _settingQueries.GetSettingValueByKeyAsync("PromotionWeeklyTimeSpanMinutes");

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