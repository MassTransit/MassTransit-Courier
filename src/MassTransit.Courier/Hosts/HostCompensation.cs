// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Exceptions;
    using Extensions;
    using Magnum.Reflection;


    public class HostCompensation<TLog> :
        Compensation<TLog>
        where TLog : class
    {
        readonly IConsumeContext<RoutingSlip> _context;
        readonly RoutingSlip _routingSlip;

        public HostCompensation(IConsumeContext<RoutingSlip> context)
        {
            _context = context;
            _routingSlip = Sanitize(context.Message);

            Log = GetActivityLog(_routingSlip);
        }

        public TLog Log { get; private set; }

        public CompensationResult Compensated()
        {
            var routingSlip = new MessageRoutingSlip(_routingSlip.TrackingNumber, _routingSlip.Activities,
                _routingSlip.ActivityLogs.SkipLast(), _routingSlip.Variables);

            return Compensated(routingSlip);
        }

        public CompensationResult Failed()
        {
            return Failed(new RoutingSlipException("The routing slip compensation failed"));
        }

        public CompensationResult Failed(Exception exception)
        {
            // the exception is thrown so MT will move the routing slip into the error queue
            throw exception;
        }

        CompensationResult Compensated(RoutingSlip routingSlip)
        {
            if (routingSlip.IsRunning())
            {
                IEndpoint endpoint = _context.Bus.GetEndpoint(routingSlip.GetLastCompensateAddress());

                endpoint.Forward(_context, routingSlip);

                return new CompensatedResult();
            }

            _context.Bus.Publish<RoutingSlipFaulted>(new
                {
                    routingSlip.TrackingNumber,
                    Timestamp = DateTime.UtcNow,
                });

            return new FaultedResult();
        }


        static RoutingSlip Sanitize(RoutingSlip message)
        {
            return new SanitizedRoutingSlip(message);
        }

        static TLog GetActivityLog(RoutingSlip routingSlip)
        {
            ActivityLog activityLog = routingSlip.ActivityLogs.Last();

            IDictionary<string, string> argumentDictionary = activityLog.Results ?? new Dictionary<string, string>();
            IDictionary<string, object> initializer = argumentDictionary.ToDictionary(x => x.Key, x => (object)x.Value);

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