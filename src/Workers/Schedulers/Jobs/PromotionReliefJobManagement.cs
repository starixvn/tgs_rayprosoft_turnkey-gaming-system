using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management.Metadata;
using Hangfire.Dashboard.Management.Support;
using Hangfire.Server;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Workers.Schedulers.Jobs;

[ManagementPage("SOS活動手動Schedule", Consts.Queue.Schedule)]
public class PromotionReliefJobManagement
{
    private readonly IPromotionSettleReliefDailyWorkerService _promotionSettleDailyWorkerService;
    private readonly IPromotionSendReliefDailyWeeklyMonthlyWorkerService _promotionSendReliefDailyWeeklyWorkerService;
    public PromotionReliefJobManagement(
        IPromotionSettleReliefDailyWorkerService promotionSettleDailyWorkerService,
        IPromotionSendReliefDailyWeeklyMonthlyWorkerService promotionSendReliefDailyWeeklyWorkerService)
    {
        _promotionSettleDailyWorkerService = promotionSettleDailyWorkerService;
        _promotionSendReliefDailyWeeklyWorkerService = promotionSendReliefDailyWeeklyWorkerService;
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("promotion relief settle daily")]
    [Description("SOS每日結算(未派發僅計算)")]
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
    [DisplayName("promotion relief send daily and weekly and monthly")]
    [Description("SOS每日每周每月派發")]
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

        return _promotionSendReliefDailyWeeklyWorkerService.ManualExecutionAsync(DateOnly.Parse(date), hour);
    }
}