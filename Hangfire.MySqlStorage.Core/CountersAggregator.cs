﻿using System;
using System.Threading;
using Dapper;
using Hangfire.Annotations;
using Hangfire.Logging;
using Hangfire.Server;

namespace Hangfire.MySqlStorage.Core
{
    internal class CountersAggregator : IServerComponent //IBackgroundProcess
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(CountersAggregator));

        private const int NumberOfRecordsInSinglePass = 1000;
        private static readonly TimeSpan DelayBetweenPasses = TimeSpan.FromMilliseconds(500);

        private readonly MySqlStorage _storage;
        private readonly MySqlStorageOptions _options;
        private readonly TimeSpan _interval;

        public CountersAggregator(MySqlStorage storage, MySqlStorageOptions options, TimeSpan interval)
        {
            if (storage == null) throw new ArgumentNullException("storage");

            _storage = storage;
            _options = options;
            _interval = interval;
        }
        // BackgroundProcessContext,CancellationToken
        //public void Execute(BackgroundProcessContext backgroundProcessContext)
        //{
        //    Logger.DebugFormat("Aggregating records in 'Counter' table...");

        //    int removedCount = 0;

        //    do
        //    {
        //        _storage.UseConnection(connection =>
        //        {
        //            removedCount = connection.Execute(
        //                GetAggregationQuery(),
        //                new { now = DateTime.UtcNow, count = NumberOfRecordsInSinglePass });
        //        });

        //        if (removedCount >= NumberOfRecordsInSinglePass)
        //        {
        //            backgroundProcessContext.StoppingToken.WaitHandle.WaitOne(DelayBetweenPasses);
        //            backgroundProcessContext.StoppingToken.ThrowIfCancellationRequested();
        //        }
        //    } while (removedCount >= NumberOfRecordsInSinglePass);

        //    backgroundProcessContext.StoppingToken.WaitHandle.WaitOne(_interval);
        //}

        public void Execute(CancellationToken cancellationToken)
        {
            Logger.DebugFormat("Aggregating records in 'Counter' table...");

            int removedCount = 0;

            do
            {
                _storage.UseConnection(connection =>
                {
                    removedCount = connection.Execute(
                        GetAggregationQuery(),
                        new { now = DateTime.UtcNow, count = NumberOfRecordsInSinglePass });
                });

                if (removedCount >= NumberOfRecordsInSinglePass)
                {
                    cancellationToken.WaitHandle.WaitOne(DelayBetweenPasses);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            } while (removedCount >= NumberOfRecordsInSinglePass);

            cancellationToken.WaitHandle.WaitOne(_interval);
        }

        public override string ToString()
        {
            return GetType().ToString();
        }

        private string GetAggregationQuery()
        {
            return @"
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
START TRANSACTION;

INSERT INTO  " + _options.TablePrefix + @"_AggregatedCounter (`Key`, Value, ExpireAt)
    SELECT `Key`, SUM(Value) as Value, MAX(ExpireAt) AS ExpireAt 
    FROM (
            SELECT `Key`, Value, ExpireAt
            FROM  " + _options.TablePrefix + @"_Counter
            LIMIT @count) tmp
	GROUP BY `Key`
        ON DUPLICATE KEY UPDATE 
            Value = Value + VALUES(Value),
            ExpireAt = GREATEST(ExpireAt,VALUES(ExpireAt));

DELETE FROM `" + _options.TablePrefix + @"_Counter`
LIMIT @count;

COMMIT;";
        }
    }
}
