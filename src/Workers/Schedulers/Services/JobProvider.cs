using System;
using System.Runtime.InteropServices;
using Hangfire;
using TGS.Workers.Schedulers.Hangfire.Services;
using TGS.Workers.Schedulers.Jobs;
using TGS.Workers.Schedulers.Jobs.Agents;
using TGS.Workers.Schedulers.Jobs.ArrearsEmail;
using TGS.Workers.Schedulers.Jobs.Players;
using TGS.Workers.Schedulers.Jobs.Promotion;
using TGS.Workers.Schedulers.Jobs.Reports;
using TGS.Workers.Schedulers.Jobs.Rounds;

namespace TGS.Workers.Schedulers.Services;

public sealed class JobProvider : BaseJobProvider
{
    private static readonly string _TimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "China Standard Time"
        : "Asia/Shanghai";

    public JobProvider() :
        base(
            new JobDescription(new("500BD4EC-7B4A-4661-A00B-7C7BDC565CB4"),
                Cron.Minutely(),
                null,
                typeof(BrandGameProviderScheduleWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("321E2BEE-7777-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Daily(12, 00),
                null,
                typeof(ArrearsEmailWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("321E2BEE-7751-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Daily(0, 30),
                null,
                typeof(AgentDividendWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("123E2BEE-7751-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Daily(10, 15),
                null,
                typeof(PlayerRebateWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("123E2BEE-7710-4D82-8CA7-BB1F1B83C4E0"),
                "32 0,1 * * *",
                null,
                typeof(PromotionSettlementDailyWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("123E2BEE-7720-4D82-8CA7-BB1F1B83C4E0"),
                "33 0,1 * * 1",
                null,
                typeof(PromotionSettlementWeeklyWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("123E2BEE-7730-4D82-8CA7-BB1F1B83C4E0"),
                "34 0,1 1 * *",
                null,
                typeof(PromotionSettlementMonthlyWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("123E2BEE-7751-4D82-8CA7-BB1F1B83C4E9"),
                Cron.Daily(0, 35),
                null,
                typeof(PlayerAllAgentRebateWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("788E2BEE-7751-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Minutely(),
                null,
                typeof(FillPlayerBucketMinutelyWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("888E2BEE-7751-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Hourly(1),
                null,
                typeof(RoundAggregateWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("999E2BEE-7751-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Hourly(10),
                null,
                typeof(PlayerAggregateReportWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId)),
            new JobDescription(new("321E2BEE-0028-4D82-8CA7-BB1F1B83C4E0"),
                Cron.Monthly(1, 0, 0),
                null,
                typeof(PlatformCreditBalanceWorker),
                TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneId))
        )

    {
    }
}