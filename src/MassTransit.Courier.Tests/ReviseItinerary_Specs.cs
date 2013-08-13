// Copyright 2007-2013 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Courier.Tests
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class When_an_itinerary_is_revised :
        ActivityTestFixture
    {
        [Test]
        public void Should_immediately_complete_an_empty_list()
        {
            var completed = new TaskCompletionSource<RoutingSlipCompleted>();
            var reviseActivityCompleted = new TaskCompletionSource<RoutingSlipActivityCompleted>();
            var testActivityCompleted = new TaskCompletionSource<RoutingSlipActivityCompleted>();

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            ActivityTestContext reviseActivity = GetActivityContext<ReviseToEmptyItineraryActivity>();

            LocalBus.SubscribeHandler<RoutingSlipCompleted>(completed.SetResult);
            LocalBus.SubscribeHandler<RoutingSlipActivityCompleted>(msg =>
                {
                    if (msg.ActivityName.Equals(testActivity.Name))
                        testActivityCompleted.SetResult(msg);
                    if (msg.ActivityName.Equals(reviseActivity.Name))
                        reviseActivityCompleted.SetResult(msg);
                });

            Assert.IsTrue(WaitForSubscription<RoutingSlipCompleted>());
            Assert.IsTrue(WaitForSubscription<RoutingSlipActivityCompleted>());

            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddActivity(reviseActivity.Name, reviseActivity.ExecuteUri, new
            {
                Value = "Time to remove any remaining items!",
            });
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new
                {
                    Value = "Hello",
                });
            LocalBus.Execute(builder.Build());

            Assert.IsTrue(completed.Task.Wait(TestTimeout), "RoutingSlip did not complete");
            Assert.IsTrue(reviseActivityCompleted.Task.Wait(TestTimeout), "Revise Activity did not complete");

            Assert.IsFalse(testActivityCompleted.Task.Wait(3.Seconds()), "Test Activity should not have completed");
        }

        [Test]
        public void Should_complete_the_additional_item()
        {
            var completed = new TaskCompletionSource<RoutingSlipCompleted>();
            var reviseActivityCompleted = new TaskCompletionSource<RoutingSlipActivityCompleted>();
            var testActivityCompleted = new TaskCompletionSource<RoutingSlipActivityCompleted>();

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            ActivityTestContext reviseActivity = GetActivityContext<ReviseItineraryActivity>();

            LocalBus.SubscribeHandler<RoutingSlipCompleted>(completed.SetResult);
            LocalBus.SubscribeHandler<RoutingSlipActivityCompleted>(msg =>
                {
                    if (msg.ActivityName.Equals(testActivity.Name))
                        testActivityCompleted.SetResult(msg);
                    if (msg.ActivityName.Equals(reviseActivity.Name))
                        reviseActivityCompleted.SetResult(msg);
                });

            Assert.IsTrue(WaitForSubscription<RoutingSlipCompleted>());
            Assert.IsTrue(WaitForSubscription<RoutingSlipActivityCompleted>());

            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddActivity(reviseActivity.Name, reviseActivity.ExecuteUri, new
            {
                Value = "Time to add a new item!",
            });
            LocalBus.Execute(builder.Build());

            Assert.IsTrue(completed.Task.Wait(TestTimeout), "RoutingSlip did not complete");
            Assert.IsTrue(reviseActivityCompleted.Task.Wait(TestTimeout), "Revise Activity did not complete");
            Assert.IsTrue(testActivityCompleted.Task.Wait(3.Seconds()), "TestActivity did not complete");
        }


        protected override void SetupActivities()
        {
            AddActivityContext<TestActivity, TestArguments, TestLog>(() => new TestActivity());
            AddActivityContext<ReviseToEmptyItineraryActivity, TestArguments, TestLog>(
                () => new ReviseToEmptyItineraryActivity());

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            AddActivityContext<ReviseItineraryActivity, TestArguments, TestLog>(
                () => new ReviseItineraryActivity(x => x.AddActivity(testActivity.Name, testActivity.ExecuteUri, new { Value = "Added"})));
        }
    }
}