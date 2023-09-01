using System;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using TGS.Core.DataServices.Tgs.Extensions;
using TGS.Core.Infrastructures.ApiClient;
using TGS.Core.Infrastructures.Mail;
using TGS.Core.Infrastructures.Transactions;
using TGS.Core.Infrastructures.WebApiServices;
using Tgs.Game.Type.Round.Aggregate.Client.DependencyInjection;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.Operations.Extensions;
using TGS.Workers.Schedulers;
using TGS.Workers.Schedulers.Hangfire;
using TGS.Workers.Schedulers.Hangfire.Filters;
using TGS.Workers.Schedulers.Jobs;
using TGS.Workers.Schedulers.Jobs.Agents;
using TGS.Workers.Schedulers.Jobs.Players;
using TGS.Workers.Schedulers.Jobs.Promotion;
using TGS.Workers.Schedulers.Jobs.Reports;
using TGS.Workers.Schedulers.Jobs.Rounds;
using TGS.Workers.Schedulers.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, conifg) =>
    {
        var nodeIp = conifg.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
            options.Endpoint = new Uri($"http://{nodeIp}:4317");

        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services
    .AddProfileViewer()
    .AddOpenTelemetry()
    .ConfigureResource(_ => ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .WithMetrics(meterProviderBuilder => meterProviderBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/health")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !new[] { "html", "ico", "png", "jpg" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
    .AddTgsRepository(builder.Configuration, "Tgs")
    .AddWebApiServices()
    .AddTgsLog()
    .AddSystemJsonSerializer()
    .AddSecurity()
    .AddTransactions()
    .AddApiClient()
    .AddGameApiClients()
    .AddOperations()
    .AddReportAggregateClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = config.GetValue<Uri>("Schedule:ReportAggregate");
    })
    .AddPromotionClientServices((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        options.Url = config.GetValue<Uri>("Schedule:Promotion");
    })
    .AddPromotionRebateClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        options.Url = config.GetValue<Uri>("Schedule:PromotionRebate");
    })
    .AddGameTypeRoundAggregateClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        options.Url = config.GetValue<Uri>("Schedule:GameTypeRoundAggregate");
    })
    .AddHangfireScheduler(
        builder.Configuration,
        jobProvider: new JobProvider(),
        typesToUseManagementPage: GetModuleTypes())
    .AddMail(builder.Configuration)
    .AddProfitRebateClient(builder.Configuration);
builder.Services
    .AddRedis((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.ConnectionString = config.GetConnectionString("RedisConfigurationString");
    });

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.Url = config.GetConnectionString("Nats")!;
            });
    });

builder.Services.AddHangfireServer((sp, options) =>
{
    var setting = sp.GetRequiredService<HangfireSettings>();

    options.Queues = new[]
    {
                Consts.Queue.Schedule,
                Consts.Queue.GameProvider,
                Consts.Queue.Report,
                Consts.Queue.ArrearsEmail
            };

    options.SchedulePollingInterval = TimeSpan.FromSeconds(setting.Schedule.SchedulePollingInterval);
});

builder.Services.AddControllers();

builder.Services.AddHttpClient<IEmailClient, EmailClient>((sp, c) =>
{
    var url = builder.Configuration.GetSection("Schedule:Email");
    c.BaseAddress = new Uri(url.Value!);
});

builder.Services.AddGameProviderScheduleLockRepository();

builder.Services
    .AddHealthChecks();

// BO services
builder.Services.AddTransient<IBrandGameProviderService, BrandGameProviderService>()
    .AddTransient<IPlayerAggregateReportWorkerService, PlayerAggregateReportWorkerService>()
    .AddTransient<IRoundAggregateWorkerService, RoundAggregateWorkerService>()
    .AddTransient<IGameTypeRoundAggregateWorkerService, GameTypeRoundAggregateWorkerService>()
    .AddTransient<IPlayerRebateWorkerService, PlayerRebateWorkerService>()
    .AddTransient<IAgentDividendWorkerService, AgentDividendWorkerService>()
    .AddTransient<IPromotionSettlementDailyWorkerService, PromotionSettlementDailyWorkerService>()
    .AddTransient<IPromotionSettlementWeeklyWorkerService, PromotionSettlementWeeklyWorkerService>()
    .AddTransient<IPromotionSettlementMonthlyWorkerService, PromotionSettlementMonthlyWorkerService>()
    .AddTransient<IFillPlayerBucketMinutelyWorkerService, FillPlayerBucketMinutelyWorkerService>()
    .AddTransient<IPlayerAllAgentRebateWorkerService, PlayerAllAgentRebateWorkerService>()
    .AddTransient<IPromotionSettleDailyWorkerService, PromotionSettleDailyWorkerService>()
    .AddTransient<IPromotionSendRebateDailyWeeklyWorkerService, PromotionSendRebateDailyWeeklyWorkerService>()
    .AddTransient<ISystemClock, SystemClock>();

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseHangfireScheduler();

app.UseRouting();

app.UseHttpMetrics();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");

    endpoints.MapMetrics();

    endpoints.MapHangfireDashboard("/hangfire",
        new DashboardOptions
        {
            Authorization = new[]
            {
                new IgnoreHangfireAuthorizationFilter()
            }
        });
});

app.Run();

static Type[] GetModuleTypes() => new[]
{
    typeof(AggregateJobManagement),
    typeof(RebateAndDividendJobManagement),
    typeof(PromotionJobManagement),
    typeof(PromotionRebateJobManagement)
};