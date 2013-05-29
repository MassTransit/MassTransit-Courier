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
    using System.Linq;
    using Contracts;
    using Extensions;
    using InternalMessages;
    using Magnum.Reflection;


    public class HostCompensation<TLog> :
        Compensation<TLog>
        where TLog : class
    {
        readonly ActivityLog _activityLog;
        readonly IConsumeContext<RoutingSlip> _context;
        readonly RoutingSlip _routingSlip;

        public HostCompensation(IConsumeContext<RoutingSlip> context)
        {
            _context = context;

            _routingSlip = Sanitize(context.Message);
            if (_routingSlip.ActivityLogs.Count == 0)
                throw new ArgumentException("The routingSlip must contain at least one activity log");

            _activityLog = _routingSlip.ActivityLogs.Last();

            Log = GetActivityLog(_activityLog, _routingSlip.Variables);
        }

        public TLog Log { get; private set; }

        public Guid TrackingNumber
        {
            get { return _routingSlip.TrackingNumber; }
        }

        public CompensationResult Compensated()
        {
            var builder = new RoutingSlipBuilder(_routingSlip.TrackingNumber, _routingSlip.Itinerary,
                _routingSlip.ActivityLogs.SkipLast(), _routingSlip.Variables, _routingSlip.ActivityExceptions);

            return Compensated(builder.Build());
        }

        public CompensationResult Failed()
        {
            return Failed(new RoutingSlipException("The routing slip compensation failed"));
        }

        public CompensationResult Failed(Exception exception)
        {
            var message = new CompensationFailedMessage(_routingSlip.TrackingNumber, _activityLog.ActivityTrackingNumber,
                _activityLog.Name, exception);
            _context.Bus.Publish(message);

            // the exception is thrown so MT will move the routing slip into the error queue
            throw exception;
        }

        CompensationResult Compensated(RoutingSlip routingSlip)
        {
            _context.Bus.Publish(new RoutingSlipActivityCompensatedMessage(_routingSlip.TrackingNumber,
                _activityLog.ActivityTrackingNumber, _activityLog.Name, _activityLog.Results));

            if (routingSlip.IsRunning())
            {
                IEndpoint endpoint = _context.Bus.GetEndpoint(routingSlip.GetNextCompensateAddress());

                endpoint.Forward(_context, routingSlip);

                return new CompensatedResult();
            }

            _context.Bus.Publish(new RoutingSlipFaultedMessage(routingSlip.TrackingNumber,routingSlip.ActivityExceptions));

            return new FaultedResult();
        }

        static RoutingSlip Sanitize(RoutingSlip message)
        {
            return new SanitizedRoutingSlip(message);
        }

        static TLog GetActivityLog(ActivityLog activityLog, IEnumerable<KeyValuePair<string, string>> variables)
        {
            IDictionary<string, object> initializer = variables.ToDictionary(x => x.Key, x => (object)x.Value);
            foreach (var argument in activityLog.Results.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                initializer[argument.Key] = argument.Value;

            return InterfaceImplementationExtensions.InitializeProxy<TLog>(initializer);
        }


        class CompensatedResult :
            CompensationResult
        {
        }


        class FaultedResult :
            CompensationResult
        {
        }
    }
}