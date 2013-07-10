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
namespace MassTransit.Courier.Hosts
{
    using System;
    using Contracts;
    using Logging;


    public class ExecuteActivityHost<TActivity, TArguments> :
        Consumes<RoutingSlip>.Context
        where TActivity : ExecuteActivity<TArguments>
        where TArguments : class
    {
        readonly ExecuteActivityFactory<TArguments> _activityFactory;
        readonly Uri _compensateAddress;
        readonly ILog _log = Logger.Get<ExecuteActivityHost<TActivity, TArguments>>();

        public ExecuteActivityHost(Uri compensateAddress, ExecuteActivityFactory<TArguments> activityFactory)
        {
            if (compensateAddress == null)
                throw new ArgumentNullException("compensateAddress");
            if (activityFactory == null)
                throw new ArgumentNullException("activityFactory");

            _compensateAddress = compensateAddress;
            _activityFactory = activityFactory;
        }

        public void Consume(IConsumeContext<RoutingSlip> context)
        {
            var execution = new HostExecution<TArguments>(context, _compensateAddress);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Host: {0} Executing: {1}", context.Bus.Endpoint.Address, execution.TrackingNumber);

            try
            {
                ExecutionResult result = ExecuteActivity(execution);

                result.Evaluate();
            }
            catch (Exception ex)
            {
                _log.Error("The activity threw an unexpected exception", ex);
            }
        }

        ExecutionResult ExecuteActivity(HostExecution<TArguments> execution)
        {
            try
            {
                return _activityFactory.ExecuteActivity(execution);
            }
            catch (Exception ex)
            {
                return execution.Faulted(ex);
            }
        }
    }
}