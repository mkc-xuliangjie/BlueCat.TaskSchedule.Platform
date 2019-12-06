using BlueCat.ApiExtensions.Extensions;
using BlueCat.Core;
using BlueCat.Core.Options;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.HttpJob;
using Hangfire.HttpJob.Support;
using Hangfire.MySqlStorage.Core;
using Hangfire.Server;
using HealthChecks.UI.Client;
using JobsServer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;



namespace BlueCat.JobServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            if (ConfigSettings.Instance.UseRedis)
            {
                Redis = ConnectionMultiplexer.Connect(ConfigSettings.Instance.HangfireRedisConnectionString);
            }

        }

        public static ConnectionMultiplexer Redis;

        public static readonly string[] ApiQueues = new[] { "apis", "jobs", "task", "rjob", "pjob", "rejob", "default" };

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

            culture.DateTimeFormat.ShortDatePattern = "yyyy-M-d";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.ShortTimePattern = "HH:mm";
            culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;

            services.AddHttpContextAccessor();

            //健康检查地址添加
            var hostlist = ConfigSettings.Instance.HostServers;
            //添加健康检查地址
            hostlist.ForEach(s =>
            {
                services.AddHealthChecks().AddUrlGroup(new Uri(s.Uri), s.HttpMethod.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, $"{s.Uri}");
            });

            #region 添加Hnagfire 数据库,包括redis,mysql,sqlserver
            services.AddHangfire(config =>
            {
                //使用服务器资源监视
                config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));

                //使用Redis
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseRedis"))
                {
                    //使用redis
                    config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
                    {
                        FetchTimeout = TimeSpan.FromMinutes(5),
                        Prefix = "{Custom}:",
                        //活动服务器超时时间
                        InvisibilityTimeout = TimeSpan.FromHours(1),
                        //任务过期检查频率
                        ExpiryCheckInterval = TimeSpan.FromHours(1),
                        DeletedListSize = 10000,
                        SucceededListSize = 10000
                    });

                }

                //使用Mysq
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseMySql"))
                {
                    //添加 Mysql
                    config.UseStorage(new MySqlStorage(Configuration.GetConnectionString("Hangfire.MySql"), new MySqlStorageOptions() { TablePrefix = "Custom" }));//启用http任务;
                }

                //使用SqlServer
                if (ConfigurationManager.GetAppSettingBool("Hangfire.UseSqlServer"))
                {
                    //添加 SqlServer
                    config.UseSqlServerStorage(ConfigSettings.Instance.HangfireSqlserverConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions()
                    {
                        //每隔一小时检查过期job
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        QueuePollInterval = TimeSpan.FromSeconds(1)
                    });
                }

                config.UseHangfireHttpJob(new HangfireHttpJobOptions()
                {
                    AddHttpJobButtonName = "添加计划任务",
                    AddRecurringJobHttpJobButtonName = "添加定时任务",
                    EditRecurringJobButtonName = "编辑定时任务",
                    PauseJobButtonName = "暂停或开始",
                    DashboardTitle = "XXX公司任务管理",
                    DashboardName = "后台任务管理",
                    DashboardFooter = "XXX公司后台任务管理V1.0.0.0",
                    StartBackgroudJobButtonName = "带参数执行",
                    StopBackgroudJobButtonName = "停止job"
                    //SendToMailList = ConfigSettings.Instance.SendMailList,
                    //SendMailAddress = ConfigSettings.Instance.SendMailAddress,
                    //SMTPServerAddress = ConfigSettings.Instance.SMTPServerAddress,
                    //SMTPPort = ConfigSettings.Instance.SMTPPort,
                    //SMTPPwd = ConfigSettings.Instance.SMTPPwd,
                    //SMTPSubject = ConfigSettings.Instance.SMTPSubject
                }).UseConsole(new ConsoleOptions() { BackgroundColor = "#000079" }).UseDashboardMetric(DashboardMetrics.AwaitingCount)
                  .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                  .UseDashboardMetric(DashboardMetrics.RecurringJobCount)
                  .UseDashboardMetric(DashboardMetrics.RetriesCount)
                  .UseDashboardMetric(DashboardMetrics.FailedCount)
                  .UseDashboardMetric(DashboardMetrics.ServerCount);

            });
            #endregion

            //添加SignalR
            services.AddSignalR();

            //依赖注入
            services.AddSingleton<IServiceProvider, ServiceProvider>();

            //跨域设置
            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowCredentials();
            }));

            services.AddHealthChecksUI();



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region  //本地化

            var supportedCultures = new[]
            {
                new CultureInfo("zh-CN"),
                new CultureInfo("en-US")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("zh-CN"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            #endregion

            #region //队列

            var queues = new[] { "default", "apis", "localjobs" };
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(4),
                SchedulePollingInterval = TimeSpan.FromSeconds(1),//秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
                ShutdownTimeout = TimeSpan.FromMinutes(30),//超时时间
                Queues = ApiQueues,//队列
                WorkerCount = Math.Max(Environment.ProcessorCount, 40)//工作线程数，当前允许的最大线程，默认20
            },
            //服务器资源检测频率
            additionalProcesses: new IBackgroundProcess[] { new ProcessMonitor(checkInterval: TimeSpan.FromSeconds(1)) }//new[] { new SystemMonitor(checkInterval: TimeSpan.FromSeconds(1))}
            );
            #endregion

            #region //后台进程
            if (ConfigSettings.Instance.UseBackWorker)
            {
                //var listprocess = new List<IBackgroundProcess>
                //{
                //    ConfigurationManager.FromJson<BackWorkers>(ConfigSettings.Instance.BackWorker)
                //};
                //app.UseHangfireServer(new BackgroundJobServerOptions()
                //{
                //    ServerName = $"{Environment.MachineName}-BackWorker",
                //    WorkerCount = 20,
                //    Queues = new[] { "test", "api", "demo" }
                //}, additionalProcesses: listprocess);
            }
            #endregion

            #region //启动Hangfire面板

            //启动hangfire面板
            app.UseHangfireDashboard("/job", new DashboardOptions
            {
                AppPath = ConfigSettings.Instance.AppWebSite,//返回时跳转的地址
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {

                    return false;
                },
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,//是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login =ConfigSettings.Instance.LoginUser,//登录账号
                            PasswordClear =  ConfigSettings.Instance.LoginPwd//登录密码
                        }
                    }
                })}
            });

            //只读面板，只能读取不能操作
            app.UseHangfireDashboard("/job-read", new DashboardOptions
            {
                IgnoreAntiforgeryToken = true,
                AppPath = "#",//返回时跳转的地址
                DisplayStorageConnectionString = false,//是否显示数据库连接信息
                IsReadOnlyFunc = Context =>
                {
                    return true;
                },
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,//是否启用ssl验证，即https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "read",
                            PasswordClear = "only"
                        },
                        new BasicAuthAuthorizationUser
                        {
                            Login = "test",
                            PasswordClear = "123456"
                        },
                        new BasicAuthAuthorizationUser
                        {
                            Login = "guest",
                            PasswordClear = "123@123"
                        }
                    }
                })
                }
            });

            #endregion

            #region //重写json报告数据，可用于远程调用获取健康检查结果

            var options = new HealthCheckOptions
            {
                ResponseWriter = async (c, r) =>
                {
                    c.Response.ContentType = "application/json";

                    var result = JsonConvert.SerializeObject(new
                    {
                        status = r.Status.ToString(),
                        errors = r.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
                    });
                    await c.Response.WriteAsync(result);
                }
            };

            #endregion

            #region //健康检查

            app.UseHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecks("/health", options);//获取自定义格式的json数据
            app.UseHealthChecksUI(setup =>
            {
                setup.UIPath = "/hc"; // 健康检查的UI面板地址
                setup.ApiPath = "/hc-api"; // 用于api获取json的检查数据
            });

            #endregion

            #region SignalR

            //跨域支持
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                routes.MapHub<SignalrHubs>("/Hubs");
            });
            app.UseWebSockets();

            #endregion

            //保证在 Mvc 之前调用
            app.UseHttpContextGlobal()
               .UseToolTrace();

            app.UseMvc();
        }
    }
}
