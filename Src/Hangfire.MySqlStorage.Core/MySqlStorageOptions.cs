using System;
using System.Data;

namespace Hangfire.MySqlStorage.Core
{
    public  class MySqlStorageOptions
    {
        private TimeSpan _queuePollInterval;

        public MySqlStorageOptions()
        {
            TransactionIsolationLevel = IsolationLevel.ReadCommitted;//ʵ����뼶��Ĭ��Ϊ��ȡ���ύ
            QueuePollInterval = TimeSpan.FromSeconds(15);//���м��Ƶ�ʣ��뼶������Ҫ���ö̵㣬һ�������������Ĭ��ʱ��
            JobExpirationCheckInterval = TimeSpan.FromHours(1);//��ҵ���ڼ������������ڼ�¼����Ĭ��ֵΪ1Сʱ
            CountersAggregateInterval = TimeSpan.FromMinutes(5);//�ۺϼ������ļ����Ĭ��Ϊ5����
            PrepareSchemaIfNecessary = true;//����true������Զ�������
            DashboardJobListLimit = 50000;//�Ǳ�����ҵ�б�չʾ��������
            TransactionTimeout = TimeSpan.FromMinutes(1);//����ʱʱ�䣬Ĭ��һ����
            InvisibilityTimeout = TimeSpan.FromMinutes(30);
        }

        public IsolationLevel? TransactionIsolationLevel { get; set; }

        /// <summary>
        /// ���м��Ƶ�ʣ��뼶������Ҫ���ö̵㣬һ�������������Ĭ��ʱ��
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
        /// //����true������Զ������� 
        /// </summary>
        public bool PrepareSchemaIfNecessary { get; set; }

        /// <summary>
        /// ��ҵ���ڼ������������ڼ�¼����Ĭ��ֵΪ1Сʱ
        /// </summary>
        public TimeSpan JobExpirationCheckInterval { get; set; }

        /// <summary>
        /// �ۺϼ������ļ����Ĭ��Ϊ5����
        /// </summary>
        public TimeSpan CountersAggregateInterval { get; set; }

        /// <summary>
        /// //�Ǳ�����ҵ�б�չʾ��������
        /// </summary>
        public int? DashboardJobListLimit { get; set; }

        /// <summary>
        /// /����ʱʱ�䣬Ĭ��һ����
        /// </summary>
        public TimeSpan TransactionTimeout { get; set; }
        [Obsolete("Does not make sense anymore. Background jobs re-queued instantly even after ungraceful shutdown now. Will be removed in 2.0.0.")]
        public TimeSpan InvisibilityTimeout { get; set; }

        public string TablePrefix { get; set; } = "Hangfire";
    }
}