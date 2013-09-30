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
namespace MassTransit.Courier.MongoDbIntegration.Tests
{
    using System;
    using Consumers;
    using Contracts;
    using Documents;
    using Events;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using MongoDbIntegration.Events;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class When_a_routing_slip_is_completed :
        MongoDbTestFixture
    {
        [Test]
        public void Should_process_the_event()
        {
            Assert.IsTrue(_test.Received.Any<RoutingSlipCompleted>());
        }

        [Test]
        public void Should_upsert_the_event_into_the_routing_slip()
        {
            Assert.IsTrue(_test.Consumer.Received.Any<RoutingSlipCompleted>(),
                "Message was not received");

            IMongoQuery query =
                Query<RoutingSlipDocument>.EQ(x => x.TrackingNumber,
                    _trackingNumber);
            RoutingSlipDocument routingSlip = _collection.FindOne(query);

            Assert.IsNotNull(routingSlip);
            Assert.IsNotNull(routingSlip.Events);
            Assert.AreEqual(1, routingSlip.Events.Length);
            
            var completed = routingSlip.Events[0] as RoutingSlipCompletedDocument;
            Assert.IsNotNull(completed);
            Assert.IsTrue(completed.Variables.ContainsKey("Client"));
            Assert.AreEqual("27", completed.Variables["Client"]);
            //Assert.AreEqual(received.Timestamp.ToMongoDbDateTime(), read.Timestamp);
        }


        [TestFixtureSetUp]
        public void Setup()
        {
            _collection = Database.GetCollection<RoutingSlipDocument>(EventCollectionName);
            _trackingNumber = NewId.NextGuid();

            EventPersister persister = new UpsertEventPersister(_collection);

            Console.WriteLine("Tracking Number: {0}", _trackingNumber);

            _test = TestFactory.ForConsumer<RoutingSlipCompletedConsumer>()
                               .New(x =>
                                   {
                                       x.ConstructUsing(() => new RoutingSlipCompletedConsumer(persister));
                                       DateTime timestamp = DateTime.UtcNow;
                                       x.Publish(new RoutingSlipCompletedEvent(_trackingNumber, timestamp));
                                   });

            _test.Execute();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _test.Dispose();
        }

        MongoCollection<RoutingSlipDocument> _collection;
        ConsumerTest<BusTestScenario, RoutingSlipCompletedConsumer> _test;
        Guid _trackingNumber;
    }
}