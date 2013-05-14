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


    public class CompensateActivityHost<TActivity, TLog> :
        Consumes<RoutingSlip>.Context
        where TActivity : CompensateActivity<TLog>
        where TLog : class
    {
        readonly Func<TLog, TActivity> _activityFactory;
        readonly ILog _log = Logger.Get<CompensateActivityHost<TActivity, TLog>>();

        public CompensateActivityHost(Func<TLog, TActivity> activityFactory)
        {
            _activityFactory = activityFactory;
        }

        public void Consume(IConsumeContext<RoutingSlip> context)
        {
            var compensation = new HostCompensation<TLog>(context);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Host: {0} Compensating: {1}", context.Bus.Endpoint.Address,
                    compensation.TrackingNumber);

            try
            {
                TActivity activity = _activityFactory(compensation.Log);

                CompensationResult result = activity.Compensate(compensation);
            }
            catch (Exception ex)
            {
                compensation.Failed(ex);
            }
        }
    }
}