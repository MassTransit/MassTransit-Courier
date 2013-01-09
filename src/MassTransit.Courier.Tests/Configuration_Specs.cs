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
    using NUnit.Framework;


    [TestFixture]
    public class When_configuring_activity_hosts_with_masstransit
    {
        [Test]
        public void Should_have_a_clean_interface()
        {
        }

        IServiceBus _bus;

        [TestFixtureSetUp]
        public void Setup()
        {
            _bus = ServiceBusFactory.New(x =>
                {
                    var executeUri = new Uri("loopback://localhost/mt_client");
                    var compensateUri = new Uri("loopback://localhost/mt_server");
                    x.ReceiveFrom(executeUri);

                    x.Subscribe(
                        s =>
                            {
                                s.ExecuteActivityHost<TestActivity, TestArguments>(compensateUri,
                                    _ => new TestActivity());
                            });
                });
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _bus.Dispose();
        }
    }
}