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

[ManagementPage("返水活動手動Schedule", Consts.Queue.Schedule)]
public class PromotionRebateJobManagement
{
    private readonly IPromotionSettleDailyWorkerService _promotionSettleDailyWorkerService;
    private readonly IPromotionSendRebateDailyWeeklyWorkerService _promotionSendRebateDailyWeeklyWorkerService;
    public PromotionRebateJobManagement(
        IPromotionSettleDailyWorkerService promotionSettleDailyWorkerService,
        IPromotionSendRebateDailyWeeklyWorkerService promotionSendRebateDailyWeeklyWorkerService)
    {
        _promotionSettleDailyWorkerService = promotionSettleDailyWorkerService;
        _promotionSendRebateDailyWeeklyWorkerService = promotionSendRebateDailyWeeklyWorkerService;
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("promotion settle daily")]
    [Description("每日結算(未派發)")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(90)]
    public Task PromotionSettleDailyAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "依照時區做結算")]
        string date,
        [DisplayData("指定utc小時",
            "",
            "+08時區整點 = 16")]
        int hour = 16,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        return _promotionSettleDailyWorkerService.ManualExecutionAsync(DateOnly.Parse(date), hour);
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("promotion send daily and weekly")]
    [Description("每日每周反水派發")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(90)]
    public Task PromotionSendDailyWeeklyAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "依照時區做結算")]
        string date,
        [DisplayData("指定utc小時",
            "",
            "+08時區整點 = 16")]
        int hour = 16,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        return _promotionSendRebateDailyWeeklyWorkerService.ManualExecutionAsync(DateOnly.Parse(date), hour);
    }
}