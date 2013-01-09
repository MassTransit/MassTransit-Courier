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
namespace MassTransit.Courier.Tests.Testing
{
    using System;
    using BusConfigurators;
    using Subscriptions.Coordinator;


    public interface ActivityTestContext :
        IDisposable
    {
        string Name { get; }

        Uri ExecuteUri { get; }
        Uri CompensateUri { get; }

        IServiceBus ExecuteBus { get; }
        IServiceBus CompensateBus { get; }

        void ConfigureServiceBus(ServiceBusConfigurator configurator);
    }


    public class ActivityTestContext<T, TArguments, TLog> :
        ActivityTestContext
        where T : Activity<TArguments, TLog>
        where TArguments : class
        where TLog : class
    {
        Func<T> _activityFactory;
        SubscriptionLoopback _compensateLoopback;
        SubscriptionLoopback _executeLoopback;

        public ActivityTestContext(Uri baseUri, Func<T> activityFactory)
        {
            _activityFactory = activityFactory;

            Name = GetActivityName();

            ExecuteUri = BuildQueueUri(baseUri, "execute");
            CompensateUri = BuildQueueUri(baseUri, "compensate");

            ExecuteBus = ServiceBusFactory.New(ConfigureExecuteBus);
            CompensateBus = ServiceBusFactory.New(ConfigureCompensateBus);
        }

        static string GetActivityName()
        {
            var name = typeof(T).Name;
            if (name.EndsWith("Activity"))
                name = name.Substring(0, name.Length - "Activity".Length);
            return name;
        }

        Uri BuildQueueUri(Uri baseUri, string prefix)
        {
            return new Uri(baseUri, string.Format("{0}_{1}", prefix, typeof(T).Name.ToLowerInvariant()));
        }

        public IServiceBus ExecuteBus { get; protected set; }
        public IServiceBus CompensateBus { get; protected set; }

        public string Name { get; private set; }
        public Uri ExecuteUri { get; private set; }
        public Uri CompensateUri { get; private set; }

        public void ConfigureServiceBus(ServiceBusConfigurator configurator)
        {
            configurator.AddSubscriptionObserver((bus, coordinator) =>
            {
                var loopback = new SubscriptionLoopback(bus, coordinator);
                loopback.SetTargetCoordinator(_executeLoopback.Router);
                return loopback;
            });
            configurator.AddSubscriptionObserver((bus, coordinator) =>
            {
                var loopback = new SubscriptionLoopback(bus, coordinator);
                loopback.SetTargetCoordinator(_compensateLoopback.Router);
                return loopback;
            });
        }

        public void Dispose()
        {
            ExecuteBus.Dispose();
            CompensateBus.Dispose();
        }

        protected virtual void ConfigureExecuteBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(ExecuteUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _executeLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _executeLoopback;
                });

            configurator.Subscribe(
                s => s.ExecuteActivityHost<T, TArguments>(CompensateUri, _activityFactory));
        }

        protected virtual void ConfigureCompensateBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(CompensateUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _compensateLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _compensateLoopback;
                });

            configurator.Subscribe(s => s.CompensateActivityHost<T, TLog>(_activityFactory));
        }
    }
}