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


    public class HostExecution<TArguments> :
        Execution<TArguments>
        where TArguments : class
    {
        readonly Activity _activity;
        readonly Guid _activityTrackingNumber;
        readonly TArguments _arguments;
        readonly Uri _compensationAddress;
        readonly IConsumeContext<RoutingSlip> _context;
        readonly SanitizedRoutingSlip _routingSlip;

        public HostExecution(IConsumeContext<RoutingSlip> context, Uri compensationAddress)
        {
            _context = context;
            _compensationAddress = compensationAddress;

            _routingSlip = new SanitizedRoutingSlip(context);
            if (_routingSlip.Itinerary.Count == 0)
                throw new ArgumentException("The routingSlip must contain at least one activity");

            _activityTrackingNumber = NewId.NextGuid();

            _activity = _routingSlip.Itinerary[0];
            _arguments = _routingSlip.GetActivityArguments<TArguments>();
        }

        TArguments Execution<TArguments>.Arguments
        {
            get { return _arguments; }
        }

        Guid Execution<TArguments>.TrackingNumber
        {
            get { return _routingSlip.TrackingNumber; }
        }

        Guid Execution<TArguments>.ActivityTrackingNumber
        {
            get { return _activityTrackingNumber; }
        }

        IServiceBus Execution<TArguments>.Bus
        {
            get { return _context.Bus; }
        }

        ExecutionResult Execution<TArguments>.Completed()
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder();

            return Complete(builder.Build(), RoutingSlipBuilder.NoArguments);
        }

        ExecutionResult Execution<TArguments>.Completed<TLog>(TLog log)
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);

            return Complete(builder.Build(), activityLog.Results);
        }

        ExecutionResult Execution<TArguments>.Completed<TLog>(TLog log, object variables)
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);
            builder.SetVariables(variables);

            return Complete(builder.Build(), activityLog.Results);
        }

        ExecutionResult Execution<TArguments>.Completed<TLog>(TLog log,
            IEnumerable<KeyValuePair<string, object>> variables)
        {
            ActivityLog activityLog;
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log, out activityLog);
            builder.SetVariables(variables);

            return Complete(builder.Build(), activityLog.Results);
        }

        public ExecutionResult ReviseItinerary(Action<ItineraryBuilder> itineraryBuilder)
        {
            RoutingSlipBuilder builder = CreateReviseRoutingSlipBuilder();

            return ReviseItinerary(builder, RoutingSlipBuilder.NoArguments, itineraryBuilder);
        }

        public ExecutionResult ReviseItinerary<TLog>(TLog log, Action<ItineraryBuilder> itineraryBuilder)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateReviseRoutingSlipBuilder();
            ActivityLog activityLog = builder.AddActivityLog(_activity.Name, _activityTrackingNumber,
                _compensationAddress, log);

            return ReviseItinerary(builder, activityLog.Results, itineraryBuilder);
        }

        public ExecutionResult ReviseItinerary<TLog>(TLog log, object variables,
            Action<ItineraryBuilder> buildItinerary)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateReviseRoutingSlipBuilder();
            ActivityLog activityLog = builder.AddActivityLog(_activity.Name, _activityTrackingNumber,
                _compensationAddress, log);
            builder.SetVariables(variables);

            return ReviseItinerary(builder, activityLog.Results, buildItinerary);
        }

        public ExecutionResult ReviseItinerary<TLog>(TLog log, IEnumerable<KeyValuePair<string, object>> variables,
            Action<ItineraryBuilder> buildItinerary)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateReviseRoutingSlipBuilder();
            ActivityLog activityLog = builder.AddActivityLog(_activity.Name, _activityTrackingNumber,
                _compensationAddress, log);
            builder.SetVariables(variables);

            return ReviseItinerary(builder, activityLog.Results, buildItinerary);
        }

        ExecutionResult Execution<TArguments>.Faulted()
        {
            return Faulted(new RoutingSlipException("The routing slip execution failed"));
        }

        ExecutionResult Execution<TArguments>.Faulted(Exception exception)
        {
            return Faulted(exception);
        }

        RoutingSlipBuilder CreateReviseRoutingSlipBuilder()
        {
            return new RoutingSlipBuilder(_routingSlip.TrackingNumber, Enumerable.Empty<Activity>(),
                _routingSlip.ActivityLogs, _routingSlip.Variables, _routingSlip.ActivityExceptions);
        }

        ExecutionResult Faulted(Exception exception)
        {
            if (_routingSlip.IsRunning())
                return new CompensateResult(_context, _routingSlip, _activity, _activityTrackingNumber, exception);

            return new FaultResult(_context.Bus, _routingSlip.TrackingNumber, _activity, _activityTrackingNumber,
                exception);
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder<TLog>(TLog log, out ActivityLog activityLog)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder();
            activityLog = builder.AddActivityLog(_activity.Name, _activityTrackingNumber, _compensationAddress, log);

            return builder;
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder()
        {
            return new RoutingSlipBuilder(_routingSlip.TrackingNumber, _routingSlip.Itinerary.Skip(1),
                _routingSlip.ActivityLogs, _routingSlip.Variables, _routingSlip.ActivityExceptions);
        }

        ExecutionResult Complete(RoutingSlip routingSlip, IDictionary<string, object> results)
        {
            if (routingSlip.RanToCompletion())
            {
                return new RanToCompletionResult(_context.Bus, routingSlip, _activity.Name, _activityTrackingNumber,
                    results);
            }

            return new NextActivityResult(_context, routingSlip, _activity.Name, _activityTrackingNumber, results);
        }

        ExecutionResult ReviseItinerary(RoutingSlipBuilder builder, IDictionary<string, object> results,
            Action<ItineraryBuilder> buildItinerary)
        {
            builder.SetSourceItinerary(_routingSlip.Itinerary.Skip(1));

            buildItinerary(builder);

            RoutingSlip routingSlip = builder.Build();

            if (routingSlip.RanToCompletion())
            {
                return new RanToCompletionResult(_context.Bus, routingSlip, _activity.Name, _activityTrackingNumber,
                    results);
            }

            return new NextActivityResult(_context, routingSlip, _activity.Name, _activityTrackingNumber, results);
        }
    }
}