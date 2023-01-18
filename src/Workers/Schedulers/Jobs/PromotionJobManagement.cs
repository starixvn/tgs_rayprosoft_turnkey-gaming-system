using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management.Metadata;
using Hangfire.Dashboard.Management.Support;
using Hangfire.Server;
using TGS.Core.Models.Messages;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Workers.Schedulers.Jobs;

[ManagementPage("活動手動Schedule", Consts.Queue.Schedule)]
public class PromotionJobManagement
{
    private readonly IPromotionSettlementDailyWorkerService _promotionSettlementDailyWorker;
    private readonly IPromotionSettlementWeeklyWorkerService _promotionSettlementWeeklyWorker;

    public PromotionJobManagement(
        IPromotionSettlementDailyWorkerService promotionSettlementDailyWorker,
        IPromotionSettlementWeeklyWorkerService promotionSettlementWeeklyWorker)
    {
        _promotionSettlementDailyWorker = promotionSettlementDailyWorker;
        _promotionSettlementWeeklyWorker = promotionSettlementWeeklyWorker;
    }


    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("send promotion daily")]
    [Description("發送每日活動")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> SendPromotionDailyAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTime,
        [DisplayData("指定時區",
            "",
            "+08:00 = 480")]
        int timeSpanMinutes = 480,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        return await _promotionSettlementDailyWorker.ManualExecutionAsync(DateOnly.Parse(dateTime), timeSpanMinutes);
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("send promotion weekly")]
    [Description("發送每週活動")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> SendPromotionWeeklyAsync(
        [DisplayData("指定from日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTimeFrom,
        [DisplayData("指定to日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTimeTo,
        [DisplayData("指定時區",
            "",
            "+08:00 = 480")]
        int timeSpanMinutes = 480,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        return await _promotionSettlementWeeklyWorker.ManualExecutionAsync(DateOnly.Parse(dateTimeFrom), DateOnly.Parse(dateTimeTo), timeSpanMinutes);
    }

}