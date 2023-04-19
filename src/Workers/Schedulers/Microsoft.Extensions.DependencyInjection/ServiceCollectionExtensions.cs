using System;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TGS.Workers.Schedulers;
using TGS.Workers.Schedulers.Hangfire;
using TGS.Workers.Schedulers.Hangfire.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireScheduler(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "Hangfire",
        BaseJobProvider jobProvider = null,
        Type[] typesToUseManagementPage = null)
    {
        services.Configure<HangfireSettings>(configuration.GetSection(configSectionName));

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<HangfireSettings>>().Value);

        services.AddHangfire((sp, option) =>
        {
            var settings = sp.GetRequiredService<HangfireSettings>();
            option.UseRedisStorage(settings.ConnectionString);

            option.UseSimpleAssemblyNameTypeSerializer()
                .UseColouredConsoleLogProvider()
                .UseConsole()
                .UseRecommendedSerializerSettings(s =>
                {
                    s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    s.TypeNameHandling = TypeNameHandling.None;
                    s.NullValueHandling = NullValueHandling.Include;
                    s.DefaultValueHandling = DefaultValueHandling.Include;
                });

            if (typesToUseManagementPage?.Length > 0)
            {
                option.UseManagementPages(p => p.AddJobs(typesToUseManagementPage));
            }
        });

        if (jobProvider == null)
        {
            return services;
        }

        services.AddSingleton<IJobProvider>(jobProvider);

        var jobs = ((IJobProvider)jobProvider).GetJobDescriptions();
        foreach (var job in jobs)
        {
            services.AddTransient(job.Worker);
        }

        return services;
    }

    public static IServiceCollection AddRedis(
                this IServiceCollection service,
        Action<RedisOptions, IServiceProvider> configure)
    {
        service
            .AddOptions<RedisOptions>()
            .Configure(configure)
            .Services
            .AddRedisServices()
            .RegisterRedisConnectionFactory<RedisConnectionFactory>();

        return service;
    }
}
