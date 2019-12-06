using System;
using System.Data;

namespace Hangfire.MySqlStorage.Core
{
    public  class MySqlStorageOptions
    {
        private TimeSpan _queuePollInterval;

        public MySqlStorageOptions()
        {
            TransactionIsolationLevel = IsolationLevel.ReadCommitted;//实物隔离级别，默认为读取已提交
            QueuePollInterval = TimeSpan.FromSeconds(15);//队列检测频率，秒级任务需要配置短点，一般任务可以配置默认时间
            JobExpirationCheckInterval = TimeSpan.FromHours(1);//作业到期检查间隔（管理过期记录）。默认值为1小时
            CountersAggregateInterval = TimeSpan.FromMinutes(5);//聚合计数器的间隔。默认为5分钟
            PrepareSchemaIfNecessary = true;//设置true，则会自动创建表
            DashboardJobListLimit = 50000;//仪表盘作业列表展示条数限制
            TransactionTimeout = TimeSpan.FromMinutes(1);//事务超时时间，默认一分钟
            InvisibilityTimeout = TimeSpan.FromMinutes(30);
        }

        public IsolationLevel? TransactionIsolationLevel { get; set; }

        /// <summary>
        /// 队列检测频率，秒级任务需要配置短点，一般任务可以配置默认时间
        /// </summary>
        public TimeSpan QueuePollInterval
        {
            get { return _queuePollInterval; }
            set
            {
                var message = String.Format(
                    "The QueuePollInterval property value should be positive. Given: {0}.",
                    value);

                if (value == TimeSpan.Zero)
                {
                    throw new ArgumentException(message, "value");
                }
                if (value != value.Duration())
                {
                    throw new ArgumentException(message, "value");
                }

                _queuePollInterval = value;
            }
        }

        /// <summary>
        /// //设置true，则会自动创建表 
        /// </summary>
        public bool PrepareSchemaIfNecessary { get; set; }

        /// <summary>
        /// 作业到期检查间隔（管理过期记录）。默认值为1小时
        /// </summary>
        public TimeSpan JobExpirationCheckInterval { get; set; }

        /// <summary>
        /// 聚合计数器的间隔。默认为5分钟
        /// </summary>
        public TimeSpan CountersAggregateInterval { get; set; }

        /// <summary>
        /// //仪表盘作业列表展示条数限制
        /// </summary>
        public int? DashboardJobListLimit { get; set; }

        /// <summary>
        /// /事务超时时间，默认一分钟
        /// </summary>
        public TimeSpan TransactionTimeout { get; set; }
        [Obsolete("Does not make sense anymore. Background jobs re-queued instantly even after ungraceful shutdown now. Will be removed in 2.0.0.")]
        public TimeSpan InvisibilityTimeout { get; set; }

        public string TablePrefix { get; set; } = "Hangfire";
    }
}