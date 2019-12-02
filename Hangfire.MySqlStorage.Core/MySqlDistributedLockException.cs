using System;

namespace Hangfire.MySqlStorage.Core
{
    public class MySqlDistributedLockException : Exception
    {
        public MySqlDistributedLockException(string message) : base(message)
        {
        }
    }
}
