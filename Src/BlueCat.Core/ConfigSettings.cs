using BlueCat.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlueCat.Core
{
    public class ConfigSettings
    {
        private static readonly Lazy<ConfigSettings> _instance = new Lazy<ConfigSettings>(() => new ConfigSettings());

        public static ConfigSettings Instance => _instance.Value;

        /// <summary>
        /// 站点地址,本地配置使用，此配置来自config文件，非Apollo
        /// </summary>
        public string URL => ConfigurationManager.GetAppSetting("Hangfire.Server.ServiceAddress");
        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServiceAddress => ConfigurationManager.GetAppSetting("Server.ServiceAddress");

        /// <summary>
        /// 站点地址
        /// </summary>
        public string AppWebSite => ConfigurationManager.GetAppSetting("Hangfire.Server.Website");

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LoginUser => ConfigurationManager.GetAppSetting("Hangfire.Login.User");

        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd => ConfigurationManager.GetAppSetting("Hangfire.Login.Pwd");

        /// <summary>
        /// 使用redis
        /// </summary>
        public bool UseRedis => ConfigurationManager.GetAppSettingBool("Hangfire.UseRedis");
        /// <summary>
        /// 使用mysql
        /// </summary>
        public bool UseMySql => ConfigurationManager.GetAppSettingBool("Hangfire.UseMySql");
        /// <summary>
        /// 使用sqlserver
        /// </summary>
        public bool UseSqlSerVer => ConfigurationManager.GetAppSettingBool("Hangfire.UseSqlServer");

        /// <summary>
        /// sqlserver数据库连接
        /// </summary>
        public string HangfireSqlserverConnectionString => ConfigurationManager.GetAppSetting("Hangfire.SqlServer");

        /// <summary>
        /// 使用mysql连接
        /// </summary>
        public string HangfireMysqlConnectionString => ConfigurationManager.GetAppSetting("Hangfire.Mysql");

        /// <summary>
        ///  使用redis连接
        /// </summary>
        public string HangfireRedisConnectionString => ConfigurationManager.GetAppSetting("Hangfire.Redis");

        /// <summary>
        /// 是否使用apollo配置中心
        /// </summary>
        public bool UseApollo => ConfigurationManager.GetAppSettingBool("Hangfire.UseApollo");
        /// <summary>
        /// 健康检查api地址
        /// </summary>
        public List<HealthCheckInfo> HostServers => ConfigurationManager.GetObjSetting<List<HealthCheckInfo>>("HealthChecks-UI:CheckUrls");
        /// <summary>
        /// 已完成作业过期时间(过期后会被自动删除)
        /// </summary>
        public int AutomaticDelete => ConfigurationManager.GetAppSettingInt("AutomaticDelete");

        #region 邮件相关配置
        /// <summary>
        /// SMTP地址
        /// </summary>
        public string SMTPServerAddress => ConfigurationManager.GetAppSetting("SMTPConfig:SMTPServerAddress");
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SMTPPort => ConfigurationManager.GetAppSettingInt("SMTPConfig:SMTPPort");
        /// <summary>
        /// 校验密码
        /// </summary>
        public string SMTPPwd => ConfigurationManager.GetAppSetting("SMTPConfig:SMTPPwd");
        /// <summary>
        /// 发送者邮箱
        /// </summary>
        public string SendMailAddress => ConfigurationManager.GetAppSetting("SMTPConfig:SendMailAddress");
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string SMTPSubject => ConfigurationManager.GetAppSetting("SMTPConfig:SMTPSubject");


        /// <summary>
        /// 接收者邮箱
        /// </summary>
        private List<Emails> SendList { get; } = new List<Emails>();


        /// <summary>
        /// 接收者邮箱
        /// </summary>
        public string SendMailJson => ConfigurationManager.GetAppSetting("SMTPConfig:SendToMailList");

        /// <summary>
        /// 接收者邮箱
        /// </summary>
        public List<string> SendMailList => ConfigurationManager.GetObjSetting<List<string>>("SMTPConfig:SendToMailList");
        /// <summary>
        /// 使用后台进程
        /// </summary>
        public bool UseBackWorker => ConfigurationManager.GetAppSettingBool("UseBackWorker");
        /// <summary>
        /// 是否使用邮件通知，此配置来自config文件，非Apollo
        /// </summary>
        public bool UseEmail => ConfigurationManager.GetAppSettingBool("Hangfire.UseEmail");

        public string BackWorker => ConfigurationManager.GetAppSetting("BackWorker");
        #endregion

        public HealthCheckInfo healthCheckInfo = new HealthCheckInfo();
    }

    /// <summary>
    ///API 健康检查
    /// </summary>
    public class HealthCheckInfo
    {
        /// <summary>
        /// 接口url地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// http方法
        /// </summary>
        public string HttpMethod { get; set; }
    }

    public class BackWorker
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string UrL { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 方法类型
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 执行频率/秒
        /// </summary>
        public int Internal { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 请求参数类型
        /// </summary>
        public string ContentType { get; set; }
    }

    public class Emails
    {
        public string Email { get; set; }
    }
}
