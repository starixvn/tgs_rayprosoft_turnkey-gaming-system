using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using TGS.Core.DataServices.Tgs.Extensions;
using TGS.Core.Infrastructures.ApiClient;
using TGS.Core.Infrastructures.Mail;
using TGS.Core.Infrastructures.Transactions;
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.Operations.Extensions;
using TGS.Workers.Schedulers.Hangfire;
using TGS.Workers.Schedulers.Hangfire.Filters;
using TGS.Workers.Schedulers.Jobs;
using TGS.Workers.Schedulers.Jobs.Agents;
using TGS.Workers.Schedulers.Jobs.Players;
using TGS.Workers.Schedulers.Jobs.Promotion;
using TGS.Workers.Schedulers.Jobs.Reports;
using TGS.Workers.Schedulers.Jobs.Rounds;
using TGS.Workers.Schedulers.Services;

namespace TGS.Workers.Schedulers;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(IApplicationBuilder app)
    {
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
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTgsRepository(_configuration, "Tgs")
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
            .AddHangfireScheduler(
                _configuration,
                jobProvider: new JobProvider(),
                typesToUseManagementPage: GetModuleTypes())
            .AddMail(_configuration)
            .AddProfitRebateClient(_configuration);
        services
            .AddRedis((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.ConnectionString = config.GetConnectionString("RedisConfigurationString");
            });

        services
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

        services.AddHangfireServer((sp, options) =>
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

        services.AddControllers();

        services.AddHttpClient<IEmailClient, EmailClient>((sp, c) =>
        {
            var url = _configuration.GetSection("Schedule:Email");
            c.BaseAddress = new Uri(url.Value!);
        });

        services.AddGameProviderScheduleLockRepository();

        services
            .AddHealthChecks();

        // BO services
        services.AddTransient<IBrandGameProviderService, BrandGameProviderService>();

        // job
        services.AddTransient<IPlayerAggregateReportWorkerService, PlayerAggregateReportWorkerService>();
        services.AddTransient<IRoundAggregateWorkerService, RoundAggregateWorkerService>();
        services.AddTransient<IPlayerRebateWorkerService, PlayerRebateWorkerService>();
        services.AddTransient<IAgentDividendWorkerService, AgentDividendWorkerService>();
        services.AddTransient<IPromotionSettlementDailyWorkerService, PromotionSettlementDailyWorkerService>();
        services.AddTransient<IPromotionSettlementWeeklyWorkerService, PromotionSettlementWeeklyWorkerService>();
        services.AddTransient<IPromotionSettlementMonthlyWorkerService, PromotionSettlementMonthlyWorkerService>();
        services.AddTransient<IFillPlayerBucketMinutelyWorkerService, FillPlayerBucketMinutelyWorkerService>();
        services.AddTransient<IPlayerAllAgentRebateWorkerService, PlayerAllAgentRebateWorkerService>();
        services.AddTransient<IPromotionSettleDailyWorkerService, PromotionSettleDailyWorkerService>();
        services.AddTransient<IPromotionSendRebateDailyWeeklyWorkerService, PromotionSendRebateDailyWeeklyWorkerService>();
        services.AddTransient<ISystemClock, SystemClock>();
    }

    private static Type[] GetModuleTypes() => new[]
    {
        typeof(AggregateJobManagement),
        typeof(RebateAndDividendJobManagement),
        typeof(PromotionJobManagement),
        typeof(PromotionRebateJobManagement)
    };
}