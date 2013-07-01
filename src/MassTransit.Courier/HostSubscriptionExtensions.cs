﻿// Copyright 2007-2013 Chris Patterson
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
        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TActivity, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress)
            where TActivity : ExecuteActivity<TArguments>, new()
            where TArguments : class
        {
            return ExecuteActivityHost(configurator, compensateAddress,
                DefaultConstructorExecuteActivityFactory<TActivity, TArguments>.ExecuteFactory);
        }

        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TActivity, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress, Func<TActivity> controllerFactory)
            where TActivity : ExecuteActivity<TArguments>
            where TArguments : class
        {
            return ExecuteActivityHost<TActivity, TArguments>(configurator, compensateAddress,
                _ => controllerFactory());
        }

        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TActivity, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress, Func<TArguments, TActivity> controllerFactory)
            where TActivity : ExecuteActivity<TArguments>
            where TArguments : class
        {
            var factory = new FactoryMethodExecuteActivityFactory<TActivity, TArguments>(controllerFactory);
            var host = new ExecuteActivityHost<TActivity, TArguments>(compensateAddress, factory);

            return configurator.Instance(host);
        }

        public static InstanceSubscriptionConfigurator ExecuteActivityHost<TActivity, TArguments>(
            this SubscriptionBusServiceConfigurator configurator,
            Uri compensateAddress, ExecuteActivityFactory<TActivity, TArguments> factory)
            where TActivity : ExecuteActivity<TArguments>
            where TArguments : class
        {
            var host = new ExecuteActivityHost<TActivity, TArguments>(compensateAddress, factory);

            return configurator.Instance(host);
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TActivity, TLog>(
            this SubscriptionBusServiceConfigurator configurator)
            where TActivity : CompensateActivity<TLog>, new()
            where TLog : class
        {
            return CompensateActivityHost(configurator,
                DefaultConstructorCompensateActivityFactory<TActivity, TLog>.CompensateFactory);
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TActivity, TLog>(
            this SubscriptionBusServiceConfigurator configurator, Func<TActivity> controllerFactory)
            where TActivity : CompensateActivity<TLog>
            where TLog : class
        {
            return CompensateActivityHost<TActivity, TLog>(configurator, _ => controllerFactory());
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TActivity, TLog>(
            this SubscriptionBusServiceConfigurator configurator, Func<TLog, TActivity> controllerFactory)
            where TActivity : CompensateActivity<TLog>
            where TLog : class
        {
            var factory = new FactoryMethodCompensateActivityFactory<TActivity, TLog>(controllerFactory);
            var host = new CompensateActivityHost<TActivity, TLog>(factory);

            return configurator.Instance(host);
        }

        public static InstanceSubscriptionConfigurator CompensateActivityHost<TActivity, TLog>(
            this SubscriptionBusServiceConfigurator configurator, CompensateActivityFactory<TActivity, TLog> factory)
            where TActivity : CompensateActivity<TLog>
            where TLog : class
        {
            var host = new CompensateActivityHost<TActivity, TLog>(factory);

            return configurator.Instance(host);
        }
    }
}