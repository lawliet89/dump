using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dump.Tasks
{
    [TestFixture]
    public class TaskExtensionsTest
    {
        private const string testString = "foobar123";
        private Func<string> sleep = () =>
        {
            Thread.Sleep(1000);
            return testString;
        };
        private Func<Task, string> sleepTaskAction = Task =>
        {
            Thread.Sleep(1000);
            return testString;
        };

        [Test]
        public void TasksAreContinuedProperly()
        {
            var task = Task.Factory.StartNew(sleep)
                .Then(sleepTaskAction);
            task.Wait();
            Assert.That(task.IsCompleted);
            Assert.AreEqual(testString, task.Result);
        }

        [Test]
        public void ExceptionsArePropogated()
        {
            var task = Task<string>.Factory.StartNew(() =>
            {
                throw new Exception(testString);
            });
            Func<Task<string>, string> continuationFunction = t =>
            {
                Assert.Fail("Code should never be run");
                return testString;
            };
            var continuation = task.Then(continuationFunction)
                .Then(continuationFunction);

            Assert.Throws<AggregateException>(() => continuation.Wait());
            Assert.That(continuation.IsFaulted);
            Assert.AreEqual(testString, continuation.Exception.InnerException.Message);
        }
    }
}
