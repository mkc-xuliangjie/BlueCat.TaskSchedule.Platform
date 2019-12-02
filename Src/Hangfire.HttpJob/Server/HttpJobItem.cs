﻿using Newtonsoft.Json;

namespace Hangfire.HttpJob.Server
{
    public class HttpJobItem
    {
        public HttpJobItem()
        {
            Method = "Post";
            ContentType = "application/json";
            Timeout = 20000;
            DelayFromMinutes = 15;
        }
        /// <summary>
        /// 请求Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object Data { get; set; }

        public string ContentType { get; set; }

        public int Timeout { get; set; }

        public int DelayFromMinutes { get; set; }
        public string Corn { get; set; }
        public string JobName { get; set; }
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        public string BasicUserName { get; set; }
        public string BasicPassword { get; set; }
        public bool IsRetry { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    /// <summary>
    /// 周期性任务
    /// </summary>
    public class RecurringJobItem
    {
        public string Url { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object Data { get; set; }

        public string ContentType { get; set; }

        public int Timeout { get; set; }
        public string Corn { get; set; }
        public string JobName { get; set; }
        public string BasicUserName { get; set; }
        public string BasicPassword { get; set; }

        public string QueueName { get; set; }

        public bool IsRetry { get; set; }
    }
}
