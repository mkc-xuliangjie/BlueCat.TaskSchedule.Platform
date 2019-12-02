using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueCat.Core.Options;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Heartbeat;
using Hangfire.MySqlStorage.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BlueCat.JobServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static ConnectionMultiplexer Redis;

        public static readonly string[] ApiQueues = new[] { "apis", "jobs", "task", "rjob", "pjob", "rejob", "default" };

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHangfire(config => {
                //使用服务器资源监视
                config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));

                //使用Redis
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseRedis"))
                {
                    //使用redis
                    config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
                    {
                        FetchTimeout = TimeSpan.FromMinutes(5),
                        Prefix = "{hangfire}:",
                        //活动服务器超时时间
                        InvisibilityTimeout = TimeSpan.FromHours(1),
                        //任务过期检查频率
                        ExpiryCheckInterval = TimeSpan.FromHours(1),
                        DeletedListSize = 10000,
                        SucceededListSize = 10000
                    })
                    .UseHangfireHttpJob(new HangfireHttpJobOptions()
                    {
                        AddHttpJobButtonName = "添加计划任务",
                        AddRecurringJobHttpJobButtonName = "添加定时任务",
                        EditRecurringJobButtonName = "编辑定时任务",
                        PauseJobButtonName = "暂停或开始",
                        DashboardTitle = "XXX公司任务管理",
                        DashboardName = "后台任务管理",
                        DashboardFooter = "XXX公司后台任务管理V1.0.0.0",
                        SendToMailList = HangfireSettings.Instance.SendMailList,
                        SendMailAddress = HangfireSettings.Instance.SendMailAddress,
                        SMTPServerAddress = HangfireSettings.Instance.SMTPServerAddress,
                        SMTPPort = HangfireSettings.Instance.SMTPPort,
                        SMTPPwd = HangfireSettings.Instance.SMTPPwd,
                        SMTPSubject = HangfireSettings.Instance.SMTPSubject
                    })
                    .UseConsole(new ConsoleOptions()
                    {
                        BackgroundColor = "#000079"
                    })
                    .UseDashboardMetric(DashboardMetrics.AwaitingCount)
                    .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                    .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                    .UseDashboardMetric(DashboardMetrics.RetriesCount)
                    .UseDashboardMetric(DashboardMetrics.FailedCount)
                    .UseDashboardMetric(DashboardMetrics.ServerCount);
                }

                //使用Mysq
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseMySql"))
                {
                    //添加 Mysql
                     config.UseStorage(new MySqlStorage(Configuration.GetConnectionString("Hangfire_MySql"), new MySqlStorageOptions() { TablePrefix = "Custom" }));
                }

                //使用SqlServer
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseSqlServer"))
                {

                }

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireServer();//启动Hangfire服务
            app.UseHangfireDashboard();//启动hangfire面板
            app.UseMvc();
        }
    }
}
