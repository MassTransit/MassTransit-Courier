namespace MassTransit.Courier.MongoDbIntegration.Tests
{
    using System;
    using Consumers;
    using Contracts;
    using Documents;
    using Events;
    using MongoDB.Driver;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class When_a_complete_routing_slip_is_completed :
        MongoDbTestFixture
    {
        [Test]
        public void Should_process_the_event()
        {
            Assert.IsTrue(_completedTest.Received.Any<RoutingSlipCompleted>());
        }

        [Test]
        public void Should_process_the_prepare_activity_completed_event()
        {
            Assert.IsTrue(
                _activityTest.Received.Any<RoutingSlipActivityCompleted>((c, m) => m.ActivityName.Equals("Prepare")));
        }

        [Test]
        public void Should_process_the_send_activity_completed_event()
        {
            Assert.IsTrue(
                _activityTest.Received.Any<RoutingSlipActivityCompleted>((c, m) => m.ActivityName.Equals("Send")));
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _collection = Database.GetCollection<RoutingSlipDocument>(EventCollectionName);
            _trackingNumber = NewId.NextGuid();
            EventPersister persister = new RoutingSlipEventPersister(_collection);

            Console.WriteLine("Tracking Number: {0}", _trackingNumber);

            _completedTest = TestFactory.ForConsumer<RoutingSlipCompletedConsumer>()
                                        .New(x =>
                                        {
                                            x.ConstructUsing(() => new RoutingSlipCompletedConsumer(persister));

                                            x.Publish(new RoutingSlipCompletedEvent(_trackingNumber, DateTime.UtcNow));
                                        });


            _activityTest = TestFactory.ForConsumer<RoutingSlipActivityCompletedConsumer>()
                                       .New(x =>
                                       {
                                           x.ConstructUsing(
                                               () => new RoutingSlipActivityCompletedConsumer(persister));

                                           x.Publish(new RoutingSlipActivityCompletedEvent(_trackingNumber,
                                               "Prepare",
                                               NewId.NextGuid(), DateTime.UtcNow));
                                           x.Publish(new RoutingSlipActivityCompletedEvent(_trackingNumber, "Send",
                                               NewId.NextGuid(), DateTime.UtcNow));
                                       });

            _completedTest.Execute();
            _activityTest.Execute();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _completedTest.Dispose();
            _activityTest.Dispose();
        }


        MongoCollection<RoutingSlipDocument> _collection;
        ConsumerTest<BusTestScenario, RoutingSlipCompletedConsumer> _completedTest;
        Guid _trackingNumber;
        ConsumerTest<BusTestScenario, RoutingSlipActivityCompletedConsumer> _activityTest;
    }
}
