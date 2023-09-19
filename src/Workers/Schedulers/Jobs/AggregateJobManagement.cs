using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management.Metadata;
using Hangfire.Dashboard.Management.Support;
using Hangfire.Server;
using TGS.Core.Models.Messages;
using TGS.Workers.Schedulers.Jobs.Reports;
using TGS.Workers.Schedulers.Jobs.Rounds;

namespace TGS.Workers.Schedulers.Jobs;

[ManagementPage("手動aggregate", Consts.Queue.Schedule)]
public class AggregateJobManagement
{
    private readonly IFillPlayerBucketMinutelyWorkerService _fillPlayerBucketMinutelyWorker;
    private readonly IPlayerAggregateReportWorkerService _playerAggregateWorker;
    private readonly IRoundAggregateWorkerService _roundAggregateWorker;
    private readonly IGameTypeRoundAggregateWorkerService _gameTypeRoundAggregateWorker;

    public AggregateJobManagement(IRoundAggregateWorkerService roundAggregateWorker,
        IPlayerAggregateReportWorkerService playerAggregateWorker,
        IFillPlayerBucketMinutelyWorkerService fillPlayerBucketMinutelyWorker,
        IGameTypeRoundAggregateWorkerService gameTypeRoundAggregateWorker)
    {
        _roundAggregateWorker = roundAggregateWorker;
        _playerAggregateWorker = playerAggregateWorker;
        _fillPlayerBucketMinutelyWorker = fillPlayerBucketMinutelyWorker;
        _gameTypeRoundAggregateWorker = gameTypeRoundAggregateWorker;
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("set round game setting")]
    [Description("回寫Round的有效投注比例")]
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> SetRoundGameSettingAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTime = "",
        [DisplayData("指定小時",
            "",
            "0~23, 13 = 13~14")]
        int hour = 0,
        [DisplayData("指定開始分",
            "",
            "0~59")]
        int fromMinute = 0,
        [DisplayData("指定結束分",
            "",
            "0~59, 0~50 = 從0分到51分內")]
        int toMinute = 1,
        [DisplayData("是否填水桶",
            "true")]
        bool fillPlayerBucket = true,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        if (DateTimeOffset.TryParse($"{dateTime} +8:00", out _))
        {
            return await _fillPlayerBucketMinutelyWorker.ManualExecutionAsync(dateTime, hour, fromMinute, toMinute,
                fillPlayerBucket);
        }

        return new() {ErrorMessage = "DateTime invalided"};
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("create round aggregate")]
    [Description("新增局集合指定的時間")]
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> UpdateRoundAggregateAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTime = "",
        [DisplayData("指定小時",
            "",
            "0~23, 13 = 13~14")]
        int hour = 0,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        if (DateTimeOffset.TryParse($"{dateTime} +8:00", out _))
        {
            return await _roundAggregateWorker.ManualExecutionAsync(dateTime, hour);
        }

        return new() {ErrorMessage = "DateTime invalided"};
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("create player aggregate")]
    [Description("新增局集合指定的時間")]
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> UpdatePlayerAggregateAsync(
        [DisplayData("指定日期",
            "2021/02/18",
            "Default offset +08:00")]
        string dateTime = "",
        [DisplayData("指定小時",
            "",
            "0~23, 13 = 13~14")]
        int hour = 0,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        if (DateTimeOffset.TryParse($"{dateTime} +8:00", out _))
        {
            return await _playerAggregateWorker.ManualExecutionAsync(dateTime, hour);
        }

        return new() {ErrorMessage = "DateTime invalided"};
    }

    [Queue(Consts.Queue.Schedule)]
    [Job]
    [DisplayName("create game type round aggregate")]
    [Description("新增15分鐘局集合指定的時間")]
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(90)]
    public async Task<TgsResponse> AddGameTypeRoundAggregateAsync(
        [DisplayData("指定時間",
            "2021/02/18 00:15:00",
            "utc time",
            "2023/08/17 00:15:00")]
        string dateTime = "",
        [DisplayData("指定往前幾節",
            "",
            "例5: 依指定時間執行往前推5節時間段")]
        int count = 0,
        PerformContext context = null)
    {
        context.WriteLine($"Start time:{DateTime.Now}");

        if (!DateTimeOffset.TryParse(dateTime, out _))
        {
            return new() { ErrorMessage = "DateTime invalided" };
        }

        var endDate = DateTimeOffset.Parse($"{dateTime}Z").UtcDateTime;
        return await _gameTypeRoundAggregateWorker.ManualExecutionAsync(endDate, count);
    }

}