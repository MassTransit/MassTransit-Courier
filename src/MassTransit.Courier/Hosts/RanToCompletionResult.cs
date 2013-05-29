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
    using System.Collections.Generic;
    using Contracts;
    using InternalMessages;


    class RanToCompletionResult :
        ExecutionResult
    {
        readonly string _activityName;
        readonly Guid _activityTrackingNumber;
        readonly IServiceBus _bus;
        readonly IDictionary<string, string> _results;
        readonly RoutingSlip _routingSlip;

        public RanToCompletionResult(IServiceBus bus, RoutingSlip routingSlip,
            string activityName, Guid activityTrackingNumber, IDictionary<string, string> results)
        {
            _routingSlip = routingSlip;
            _activityName = activityName;
            _activityTrackingNumber = activityTrackingNumber;
            _results = results;
            _bus = bus;
        }

        public void Evaluate()
        {
            _bus.Publish(new RoutingSlipActivityCompletedMessage(_routingSlip.TrackingNumber,
                _activityTrackingNumber, _activityName, _results, _routingSlip.Variables));

            _bus.Publish(new RoutingSlipCompletedMessage(_routingSlip.TrackingNumber, _routingSlip.Variables));
        }
    }
}