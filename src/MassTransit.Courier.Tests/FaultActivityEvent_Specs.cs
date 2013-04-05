namespace MassTransit.Courier.Tests
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using NUnit.Framework;
    using Testing;


    [TestFixture]
    public class Executing_a_faulting_routing_slip_with_compensating_activities :
        ActivityTestFixture
    {
        [Test]
        public void Should_receive_the_first_routing_slip_activity_completed_event()
        {
            RoutingSlipActivityCompleted activityCompleted = _firstActivityCompleted.Task.Result;

            Assert.AreEqual(_routingSlip.TrackingNumber, activityCompleted.TrackingNumber);
        }

        [Test]
        public void Should_receive_the_first_routing_slip_activity_compensated_event()
        {
            RoutingSlipActivityCompensated activityCompensated = _firstActivityCompensated.Task.Result;

            Assert.AreEqual(_routingSlip.TrackingNumber, activityCompensated.TrackingNumber);
        }

        [Test]
        public void Should_include_the_compensated_activity_log()
        {
            RoutingSlipActivityCompensated activityCompensated = _firstActivityCompensated.Task.Result;

            Assert.AreEqual("Hello", activityCompensated.Results["OriginalValue"]);
        }

        [Test]
        public void Should_match_the_completed_and_compensated_identifiers()
        {
            RoutingSlipActivityCompleted activityCompleted = _firstActivityCompleted.Task.Result;
            RoutingSlipActivityCompensated activityCompensated = _firstActivityCompensated.Task.Result;

            Assert.AreEqual(activityCompleted.ActivityTrackingNumber, activityCompensated.ActivityTrackingNumber);
        }

        [Test]
        public void Should_receive_the_routing_slip_faulted_event()
        {
            RoutingSlipFaulted faulted = _faulted.Task.Result;

            Assert.AreEqual(_routingSlip.TrackingNumber, faulted.TrackingNumber);
        }

        TaskCompletionSource<RoutingSlipFaulted> _faulted;
        TaskCompletionSource<RoutingSlipActivityCompleted> _firstActivityCompleted;
        TaskCompletionSource<RoutingSlipActivityCompensated> _firstActivityCompensated;
        RoutingSlip _routingSlip;

        protected override void SetupActivities()
        {
            AddActivityContext<TestActivity, TestArguments, TestLog>(() => new TestActivity());
            AddActivityContext<SecondTestActivity, TestArguments, TestLog>(() => new SecondTestActivity());
            AddActivityContext<FaultyActivity, FaultyArguments, FaultyLog>(() => new FaultyActivity());
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _faulted = new TaskCompletionSource<RoutingSlipFaulted>(TestCancellationToken);
            _firstActivityCompleted = new TaskCompletionSource<RoutingSlipActivityCompleted>(TestCancellationToken);
            _firstActivityCompensated = new TaskCompletionSource<RoutingSlipActivityCompensated>(TestCancellationToken);

            LocalBus.SubscribeHandler<RoutingSlipFaulted>(x => _faulted.SetResult(x));
            Assert.IsTrue(WaitForSubscription<RoutingSlipFaulted>());

            LocalBus.SubscribeHandler<RoutingSlipActivityCompleted>(x =>
            {
                if (x.ActivityName.Equals("Test"))
                    _firstActivityCompleted.SetResult(x);
            });
            Assert.IsTrue(WaitForSubscription<RoutingSlipActivityCompleted>());

            LocalBus.SubscribeHandler<RoutingSlipActivityCompensated>(x =>
            {
                if (x.ActivityName.Equals("Test"))
                    _firstActivityCompensated.SetResult(x);
            });
            Assert.IsTrue(WaitForSubscription<RoutingSlipActivityCompensated>());

            var builder = new RoutingSlipBuilder(Guid.NewGuid());

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new
            {
                Value = "Hello",
            });

            testActivity = GetActivityContext<SecondTestActivity>();
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri);
            testActivity = GetActivityContext<FaultyActivity>();
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri);

            builder.AddVariable("Variable", "Knife");

            _routingSlip = builder.Build();

            LocalBus.Execute(_routingSlip);
        }
    }

}
