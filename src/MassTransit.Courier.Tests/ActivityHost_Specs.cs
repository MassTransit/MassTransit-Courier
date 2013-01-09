namespace RelayHealth.MessageRouting.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Hosts;
    using MassTransit;
    using MassTransit.Testing;
    using NUnit.Framework;


    [TestFixture]
    public class An_ActivityHost
    {
        [Test]
        public void Should_register_the_proper_consumer()
        {
            Assert.IsNotEmpty(_bus.HasSubscription<RoutingSlip>(), "Consumer subscription not registered");
        }

        IServiceBus _bus;
        ExecuteActivityHost<TestActivity, TestArguments> _host;

        [TestFixtureSetUp]
        public void Setup()
        {
            var compensateAddress = new Uri("loopback://localhost/my_compensation");

            _host = new ExecuteActivityHost<TestActivity, TestArguments>(compensateAddress, 
                _  => new TestActivity());

            _bus = ServiceBusFactory.New(x =>
                {
                    x.ReceiveFrom("loopback://localhost/test_queue");

                    x.Subscribe(s => s.Instance(_host));
                });
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _bus.Dispose();
        }
    }


    [TestFixture]
    public class An_execute_activity_consumer
    {
        [Test]
        public void Should_publish_the_completed_event()
        {
            Assert.IsTrue(_test.Published.Any<RoutingSlipCompleted>());
        }

        [Test]
        public void Should_register_the_proper_consumer()
        {
            Assert.IsNotEmpty(_test.Scenario.InputBus.HasSubscription<RoutingSlip>(),
                "Consumer subscription not registered");
        }

        ConsumerTest<BusTestScenario, ExecuteActivityHost<TestActivity, TestArguments>> _test;

        [TestFixtureSetUp]
        public void Setup()
        {
            _test = TestFactory.ForConsumer<ExecuteActivityHost<TestActivity, TestArguments>>()
                               .InSingleBusScenario()
                               .New(x =>
                                   {
                                       x.ConstructUsing(
                                           () =>
                                               {
                                                   var compensateAddress = new Uri("loopback://localhost/mt_server");

                                                   return new ExecuteActivityHost<TestActivity, TestArguments>(compensateAddress,
                                                       _ => new TestActivity());

                                               });

                                       var message = new MessageRoutingSlip(Guid.NewGuid());
                                       message.AddActivity("test", new Uri("loopback://localhost/mt_client"), new
                                           {
                                               Value = "Hello",
                                           });

                                       x.Send<RoutingSlip>(message);
                                   });

            _test.Execute();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _test.Dispose();
        }
    }
}