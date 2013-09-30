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
    using InternalMessages;


    class CompensateResult :
        ExecutionResult
    {
        readonly Activity _activity;
        readonly Guid _activityTrackingNumber;
        readonly IServiceBus _bus;
        readonly IConsumeContext _context;
        readonly Exception _exception;
        readonly RoutingSlip _routingSlip;
        readonly DateTime _timestamp;

        public CompensateResult(IConsumeContext context, RoutingSlip routingSlip, Activity activity,
            Guid activityTrackingNumber, Exception exception)
        {
            _timestamp = DateTime.UtcNow;

            _context = context;
            _bus = context.Bus;
            _routingSlip = routingSlip;
            _activity = activity;
            _exception = exception;
            _activityTrackingNumber = activityTrackingNumber;
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public void Evaluate()
        {
            var activityFaultedMessage = new RoutingSlipActivityFaultedMessage(_routingSlip.TrackingNumber, _timestamp, _activity.Name, _activityTrackingNumber, _exception);
            _bus.Publish<RoutingSlipActivityFaulted>(activityFaultedMessage);

            IEndpoint endpoint = _bus.GetEndpoint(_routingSlip.GetNextCompensateAddress());

            RoutingSlip routingSlip = CreateFaultedRoutingSlip(_activity.Name, _bus.Endpoint.Address.Uri, _exception);
            endpoint.Forward(_context, routingSlip);
        }

        RoutingSlip CreateFaultedRoutingSlip(string activityName, Uri hostAddress, Exception exception)
        {
            var builder = new RoutingSlipBuilder(_routingSlip.TrackingNumber, _routingSlip.Itinerary,
                _routingSlip.ActivityLogs, _routingSlip.Variables, _routingSlip.ActivityExceptions);
            builder.AddActivityException(activityName, hostAddress, _activityTrackingNumber, _timestamp, exception);

            return builder.Build();
        }
    }
}