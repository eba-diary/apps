using Hangfire;
using Hangfire.Storage;
using Moq;
using System;

namespace Sentry.data.Infrastructure.Tests
{
    public class MockJobStorage : JobStorage
    {
        public override IStorageConnection GetConnection()
        {
            Mock<IStorageConnection> storageConnection = new Mock<IStorageConnection>();
            return storageConnection.Object;
        }

        public override IMonitoringApi GetMonitoringApi()
        {
            throw new NotImplementedException();
        }
    }
}
