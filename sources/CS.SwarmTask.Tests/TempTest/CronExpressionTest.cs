using System;
using CS.Diagnostics;
using CS.TaskScheduling;
using NUnit.Framework;

namespace CS.SwarmTask.Tests.TempTest
{
    [TestFixture]
    public class CronExpressionTest
    {
        [Test]
        public void Test()
        {
            var cronExp = new CronExpression("0 */1 * * * ? ");//从0秒开始每5秒执行一次
            var sum = cronExp.GetExpressionSummary();
            var now = SystemTime.Now();
            var  runTime = cronExp.GetNextValidTimeAfter(now); //Meta.Execution.LastSucceedRun.Value.ToUniversalTime()
            Console.WriteLine($"Now:{now};NextTime:{(runTime)}[{sum}]");


            //Assert.IsTrue(runTime.Value == now.AddMilliseconds(5));
        }
    }
}