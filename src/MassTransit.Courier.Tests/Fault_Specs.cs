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
    using System.Linq;
    using System.Threading;
    using BusConfigurators;
    using Contracts;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Subscriptions.Coordinator;
    using Testing;


    [TestFixture]
    public class When_an_activity_faults :
        ActivityTestFixture
    {
        [Test]
        public void Should_run_the_compensation()
        {
            var handled = new ManualResetEvent(false);

            LocalBus.SubscribeHandler<RoutingSlipFaulted>(message => { handled.Set(); });

            Assert.IsTrue(TestCompensateBus.HasSubscription<RoutingSlipFaulted>().Any());

            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddActivity("test", _testExecuteUri, new
                {
                    Value = "Hello",
                });
            builder.AddActivity("fault", _faultExecuteUri, new
                {
                });

            LocalBus.Execute(builder.Build());

            Assert.IsTrue(handled.WaitOne(Debugger.IsAttached ? 5.Minutes() : 30.Seconds()));
        }

        Uri _localUri;
        Uri _faultExecuteUri;
        Uri _testExecuteUri;
        Uri _testCompensateUri;
        SubscriptionLoopback _localToTestCompensateLoopback;
        SubscriptionLoopback _testCompensateLoopback;
        SubscriptionLoopback _testExecuteLoopback;
        SubscriptionLoopback _localToTestExecuteLoopback;
        SubscriptionLoopback _faultExecuteLoopback;
        Uri _faultCompensateUri;
        public IServiceBus LocalBus { get; protected set; }
        public IServiceBus TestExecuteBus { get; protected set; }
        public IServiceBus FaultExecuteBus { get; protected set; }
        public IServiceBus TestCompensateBus { get; protected set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            _localUri = new Uri("loopback://localhost/local");
            _testExecuteUri = new Uri("loopback://localhost/test_execute");
            _testCompensateUri = new Uri("loopback://localhost/test_compensate");
            _faultExecuteUri = new Uri("loopback://localhost/fault_execute");
            _faultCompensateUri = new Uri("loopback://localhost/fault_compensate");

            LocalBus = ServiceBusFactory.New(ConfigureLocalBus);

            TestExecuteBus = ServiceBusFactory.New(ConfigureTestExecuteBus);

            TestCompensateBus = ServiceBusFactory.New(ConfigureTestCompensateBus);

            FaultExecuteBus = ServiceBusFactory.New(ConfigureFaultExecuteBus);

            _localToTestCompensateLoopback.SetTargetCoordinator(_testCompensateLoopback.Router);
            _localToTestExecuteLoopback.SetTargetCoordinator(_testExecuteLoopback.Router);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            FaultExecuteBus.Dispose();
            TestCompensateBus.Dispose();
            TestExecuteBus.Dispose();
            LocalBus.Dispose();
        }

        protected virtual void ConfigureLocalBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(_localUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _localToTestCompensateLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _localToTestCompensateLoopback;
                });
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _localToTestExecuteLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _localToTestExecuteLoopback;
                });
        }

        protected virtual void ConfigureTestExecuteBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(_testExecuteUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _testExecuteLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _testExecuteLoopback;
                });

            configurator.Subscribe(
                s => s.ExecuteActivityHost<TestActivity, TestArguments>(_testCompensateUri, _ => new TestActivity()));
        }

        protected virtual void ConfigureTestCompensateBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(_testCompensateUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _testCompensateLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _testCompensateLoopback;
                });

            configurator.Subscribe(s => s.CompensateActivityHost<TestActivity, TestLog>(_ => new TestActivity()));
        }

        protected virtual void ConfigureFaultExecuteBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(_faultExecuteUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _faultExecuteLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _faultExecuteLoopback;
                });

            configurator.Subscribe(
                s => s.ExecuteActivityHost<FaultyActivity, FaultyArguments>(_faultCompensateUri,
                    _ => new FaultyActivity()));
        }
    }
}