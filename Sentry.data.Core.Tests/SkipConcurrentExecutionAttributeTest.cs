using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Reflection;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SkipConcurrentExecutionAttributeTest
    {
        [TestMethod]
        public void OnPerforming_Creates_DistrbutedLock()
        {
            MethodInfo methodInfo = typeof(IJobService).GetMethod("GetApacheLivyBatchStatusAsync", new Type[] { typeof(int), typeof(int) });
            Mock<IStorageConnection> storageConnection = new Mock<IStorageConnection>();
            Mock<IJobCancellationToken> mockCancellationTocken = new Mock<IJobCancellationToken>();
            
            object[] argArray = new object[2];
            argArray[0] = 123;
            argArray[1] = 999;
            Job job_1 = new Job(methodInfo, argArray);
            BackgroundJob backgroundJob_1 = new BackgroundJob("1", job_1, DateTime.Now);

            PerformContext performContext_1 = new PerformContext(null,storageConnection.Object, backgroundJob_1, mockCancellationTocken.Object);

            PerformingContext performingContext_1 = new PerformingContext(performContext_1);
            SkipConcurrentExecutionAttribute attribute = new SkipConcurrentExecutionAttribute(1);
            attribute.OnPerforming(performingContext_1);

            Assert.IsTrue(performingContext_1.Items.ContainsKey("DistributedLock"));
            storageConnection.Verify(v => v.AcquireDistributedLock(It.IsAny<string>(), It.IsAny<TimeSpan>()),Times.Once);

        }
        [TestMethod]
        public void OnPerforming_Concurrent_Is_Cancelled()
        {
            MethodInfo methodInfo = typeof(IJobService).GetMethod("GetApacheLivyBatchStatusAsync", new Type[] { typeof(int), typeof(int) });
            Mock<IStorageConnection> storageConnection = new Mock<IStorageConnection>();
            storageConnection.Setup(s => s.AcquireDistributedLock(It.IsAny<string>(), It.IsAny<TimeSpan>())).Throws(new Exception("Failed to aquire lock"));
            Mock<IJobCancellationToken> mockCancellationTocken = new Mock<IJobCancellationToken>();

            object[] argArray = new object[2];
            argArray[0] = 123;
            argArray[1] = 999;
            Job job_1 = new Job(methodInfo, argArray);
            BackgroundJob backgroundJob_1 = new BackgroundJob("1", job_1, DateTime.Now);
            PerformContext performContext_1 = new PerformContext(null, storageConnection.Object, backgroundJob_1, mockCancellationTocken.Object);
            PerformingContext performingContext_1 = new PerformingContext(performContext_1);

            SkipConcurrentExecutionAttribute attribute = new SkipConcurrentExecutionAttribute(30);
            attribute.OnPerforming(performingContext_1);

            Assert.IsTrue(performingContext_1.Canceled, "concurrent job was not cancelled");
            
        }

        [TestMethod]
        public void GetResource()
        {
            MethodInfo methodInfo = typeof(IJobService).GetMethod("GetApacheLivyBatchStatusAsync", new Type[] { typeof(int), typeof(int) });
            
            object[] argArray = new object[2];
            argArray[0] = 123;
            argArray[1] = 999;
            Job mockJob = new Job(methodInfo, argArray);

            SkipConcurrentExecutionAttribute attribute = new SkipConcurrentExecutionAttribute(10);

            string result = attribute.GetResource(mockJob);

            Assert.AreEqual("Sentry.data.Core.IJobService.GetApacheLivyBatchStatusAsync.123.999", result);
        }
    }

    

}
