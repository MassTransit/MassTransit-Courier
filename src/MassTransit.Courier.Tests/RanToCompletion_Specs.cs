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
    using System.Diagnostics;
    using System.Threading;
    using BusConfigurators;
    using Contracts;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class When_an_activity_runs_to_completion :
        ActivityTestFixture
    {
        [Test]
        public void Should_publish_the_completed_event()
        {
            var handled = new ManualResetEvent(false);

            LocalBus.SubscribeHandler<RoutingSlipCompleted>(message => { handled.Set(); });

            Assert.IsTrue(WaitForSubscription<RoutingSlipCompleted>());

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();

            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new
                {
                    Value = "Hello",
                });
            LocalBus.Execute(builder.Build());

            Assert.IsTrue(handled.WaitOne(Debugger.IsAttached ? 5.Minutes() : 30.Seconds()));
        }

        Uri _localUri;
        IServiceBus LocalBus { get; set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            _localUri = new Uri(BaseUri, "local");

            AddActivityContext<TestActivity, TestArguments, TestLog>(() => new TestActivity());

            LocalBus = CreateServiceBus(ConfigureLocalBus);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            LocalBus.Dispose();
        }

        protected virtual void ConfigureLocalBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(_localUri);
        }
    }
}