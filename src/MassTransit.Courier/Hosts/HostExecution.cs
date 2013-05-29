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
    using Magnum.Reflection;


    public class HostExecution<TArguments> :
        Execution<TArguments>
        where TArguments : class
    {
        readonly Activity _activity;
        readonly Uri _compensationAddress;
        readonly IConsumeContext<RoutingSlip> _context;
        readonly RoutingSlip _routingSlip;

        public HostExecution(IConsumeContext<RoutingSlip> context, Uri compensationAddress)
        {
            _context = context;
            _compensationAddress = compensationAddress;

            _routingSlip = Sanitize(context.Message);
            if (_routingSlip.Itinerary.Count == 0)
                throw new ArgumentException("The routingSlip must contain at least one activity");

            _activity = _routingSlip.Itinerary[0];

            Arguments = GetActivityArguments(_activity, _routingSlip.Variables);
        }

        public TArguments Arguments { get; private set; }

        public Guid TrackingNumber
        {
            get { return _routingSlip.TrackingNumber; }
        }

        public ExecutionResult Completed()
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder();

            return Complete(builder.Build(), NewId.NextGuid(), RoutingSlipBuilder.NoArguments);
        }

        public ExecutionResult Completed<TLog>(TLog log)
            where TLog : class
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);

            return Complete(builder.Build(), activityLog.ActivityTrackingNumber, activityLog.Results);
        }

        public ExecutionResult Completed<TLog>(TLog log, object values)
            where TLog : class
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);
            builder.SetVariables(values);

            return Complete(builder.Build(), activityLog.ActivityTrackingNumber, activityLog.Results);
        }

        public ExecutionResult Completed<TLog>(TLog log, IDictionary<string, string> values)
            where TLog : class
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);
            builder.SetVariables(values);

            return Complete(builder.Build(), activityLog.ActivityTrackingNumber, activityLog.Results);
        }

        public ExecutionResult Faulted()
        {
            return Faulted(new RoutingSlipException("The routing slip execution failed"));
        }

        public ExecutionResult Faulted(Exception exception)
        {
            if (_routingSlip.IsRunning())
                return new CompensateResult(_context, _routingSlip, _activity, exception);

            return new FaultResult(_context.Bus, _routingSlip.TrackingNumber, _activity, exception);
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder<TLog>(TLog log, out ActivityLog activityLog)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder();
            activityLog = builder.AddActivityLog(_activity.Name, _compensationAddress, log);

            return builder;
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder()
        {
            return new RoutingSlipBuilder(_routingSlip.TrackingNumber, _routingSlip.Itinerary.Skip(1),
                _routingSlip.ActivityLogs, _routingSlip.Variables, _routingSlip.ActivityExceptions);
        }

        ExecutionResult Complete(RoutingSlip routingSlip, Guid activityTrackingNumber,
            IDictionary<string, string> results)
        {
            if (routingSlip.RanToCompletion())
            {
                return new RanToCompletionResult(_context.Bus, routingSlip, _activity.Name, activityTrackingNumber,
                    results);
            }

            return new NextActivityResult(_context, routingSlip, _activity.Name, activityTrackingNumber, results);
        }

        static RoutingSlip Sanitize(RoutingSlip message)
        {
            return new SanitizedRoutingSlip(message);
        }

        static TArguments GetActivityArguments(Activity activity, IEnumerable<KeyValuePair<string, string>> variables)
        {
            IDictionary<string, object> initializer = variables.ToDictionary(x => x.Key, x => (object)x.Value);
            foreach (var argument in activity.Arguments)
                initializer[argument.Key] = argument.Value;

            return InterfaceImplementationExtensions.InitializeProxy<TArguments>(initializer);
        }
    }
}