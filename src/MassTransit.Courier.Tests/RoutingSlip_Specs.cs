// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading;
    using Contracts;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class Sending_a_routing_slip
    {
        [Test]
        public void Should_be_properly_serialized_as_a_message()
        {
            var message = new MessageRoutingSlip(Guid.NewGuid());
            message.AddActivity("test", new Uri("loopback://localhost/mt_client"), new {});

            _bus.Publish<RoutingSlip>(message);

            Assert.IsTrue(_received.WaitOne(8.Seconds()));
        }

        [Test]
        public void Should_register_the_proper_consumer()
        {
            Assert.IsNotEmpty(_bus.HasSubscription<RoutingSlip>(), "Subscription not registered");
        }

        IServiceBus _bus;
        readonly ManualResetEvent _received = new ManualResetEvent(false);

        [TestFixtureSetUp]
        public void Setup()
        {
            _bus = ServiceBusFactory.New(x =>
                {
                    x.ReceiveFrom("loopback://localhost/test_queue");

                    x.Subscribe(s => s.Handler<RoutingSlip>(message => _received.Set()));
                });
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _bus.Dispose();
        }
    }
}