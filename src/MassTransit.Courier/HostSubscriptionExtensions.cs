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
namespace MassTransit.Courier
{
    using System;
    using Hosts;
    using SubscriptionConfigurators;


    public static class HostSubscriptionExtensions
    {
        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TController, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress, Func<TController> controllerFactory)
            where TController : ExecuteActivity<TArguments>
            where TArguments : class
        {
            return ExecuteActivityHost<TController, TArguments>(configurator, compensateAddress,
                _ => controllerFactory());
        }

        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TController, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress, Func<TArguments, TController> controllerFactory)
            where TController : ExecuteActivity<TArguments>
            where TArguments : class
        {
            var host = new ExecuteActivityHost<TController, TArguments>(compensateAddress, controllerFactory);

            return configurator.Instance(host);
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TController, TLog>(
            this SubscriptionBusServiceConfigurator configurator, Func<TController> controllerFactory)
            where TController : CompensateActivity<TLog>
            where TLog : class
        {
            return CompensateActivityHost<TController, TLog>(configurator, _ => controllerFactory());
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TController, TLog>(
            this SubscriptionBusServiceConfigurator configurator, Func<TLog, TController> controllerFactory)
            where TController : CompensateActivity<TLog>
            where TLog : class
        {
            var host = new CompensateActivityHost<TController, TLog>(controllerFactory);

            return configurator.Instance(host);
        }
    }
}