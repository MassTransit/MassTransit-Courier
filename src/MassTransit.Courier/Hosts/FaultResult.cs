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


    class FaultResult :
        ExecutionResult
    {
        readonly Activity _activity;
        readonly Guid _activityTrackingNumber;
        readonly IServiceBus _bus;
        readonly Exception _exception;
        readonly DateTime _timestamp;
        readonly Guid _trackingNumber;
        readonly IDictionary<string, object> _variables;

        public FaultResult(IServiceBus bus, Guid trackingNumber, Activity activity, Guid activityTrackingNumber,
            Exception exception, IDictionary<string, object> variables)
        {
            _timestamp = DateTime.UtcNow;
            _bus = bus;
            _trackingNumber = trackingNumber;
            _activity = activity;
            _activityTrackingNumber = activityTrackingNumber;
            _exception = exception;
            _variables = variables;
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public void Evaluate()
        {
            var activityFaulted = new RoutingSlipActivityFaultedMessage(_trackingNumber, _timestamp, _activity.Name,
                _activityTrackingNumber, _exception, _variables, _activity.Arguments);
            _bus.Publish<RoutingSlipActivityFaulted>(activityFaulted);

            var activityExceptionInfo = new ActivityExceptionImpl(_activity.Name, _bus.Endpoint.Address.Uri,
                _activityTrackingNumber, _timestamp, _exception);

            var routingSlipFaulted = new RoutingSlipFaultedMessage(_trackingNumber, _timestamp, activityExceptionInfo);
            _bus.Publish<RoutingSlipFaulted>(routingSlipFaulted);
        }
    }
}