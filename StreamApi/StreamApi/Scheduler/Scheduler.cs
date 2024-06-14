using System.Reflection;
using Quartz;
using StreamApi.Scheduler.Jobs;

namespace StreamApi.Scheduler;

public static class Scheduler
{
    public static void AddJobs(this IServiceCollection services)
    {
        services.AddTransient<SplitFilesToStreamJob>();
    }

    public static void AddScheduler(this IServiceCollection services)
    {
        services.AddJobs();

        services.Configure<QuartzOptions>(options =>
        {
            options.Scheduling.IgnoreDuplicates = true; // default: false
            options.Scheduling.OverWriteExistingData = true; // default: true
        });

        //See examples here https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#di-aware-job-factories
        services.AddQuartz(q =>
        {
            #region Config

            q.SchedulerId = Assembly.GetCallingAssembly()?.GetName()?.Name ?? "Stream.Api";
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });
            q.UseTimeZoneConverter();

            #endregion

            var splitFilesToStreamJob = new JobKey(nameof(SplitFilesToStreamJob));
            q.AddJob<SplitFilesToStreamJob>(splitFilesToStreamJob, j => j
                    .StoreDurably()
                    .WithIdentity(nameof(SplitFilesToStreamJob))
                )
                .UseInMemoryStore();

            //Runs once at startup
            q.AddTrigger(trigger => trigger
                .ForJob(splitFilesToStreamJob)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1))
                .StartNow());

            //Runs every time by the time defined below
            //q.AddTrigger(trigger => trigger
            //    .ForJob(splitFilesToStreamJob)
            //    .WithDailyTimeIntervalSchedule(x => x.WithInterval(30, IntervalUnit.Minute))
            //    .StartNow());
        });

        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });
    }
}