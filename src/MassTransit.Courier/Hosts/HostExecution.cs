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

            return Complete(builder.Build());
        }

        public ExecutionResult Completed<TLog>(TLog log)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log);

            return Complete(builder.Build());
        }

        public ExecutionResult Completed<TLog>(TLog log, object values)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log);
            builder.SetVariables(values);

            return Complete(builder.Build());
        }

        public ExecutionResult Completed<TLog>(TLog log, IDictionary<string, string> values)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder(log);
            builder.SetVariables(values);

            return Complete(builder.Build());
        }

        public ExecutionResult Faulted()
        {
            return Faulted(new RoutingSlipException("The routing slip execution failed"));
        }

        public ExecutionResult Faulted(Exception exception)
        {
            _context.Bus.Publish(new RoutingSlipActivityFaultedMessage(_routingSlip.TrackingNumber, _activity.Name,
                exception));

            if (_routingSlip.IsRunning())
            {
                IEndpoint endpoint = _context.Bus.GetEndpoint(_routingSlip.GetLastCompensateAddress());

                endpoint.Forward(_context, _routingSlip);

                return new CompensateResult();
            }

            _context.Bus.Publish(new RoutingSlipFaultedMessage(_routingSlip.TrackingNumber));

            return new FaultedResult();
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder<TLog>(TLog log)
            where TLog : class
        {
            RoutingSlipBuilder builder = CreateRoutingSlipBuilder();
            builder.AddActivityLog(_activity.Name, _compensationAddress, log);

            return builder;
        }

        RoutingSlipBuilder CreateRoutingSlipBuilder()
        {
            return new RoutingSlipBuilder(_routingSlip.TrackingNumber,
                _routingSlip.Itinerary.Skip(1), _routingSlip.ActivityLogs, _routingSlip.Variables);
        }

        ExecutionResult Complete(RoutingSlip routingSlip)
        {
            if (routingSlip.RanToCompletion())
            {
                _context.Bus.Publish(new RoutingSlipCompletedMessage(routingSlip.TrackingNumber, routingSlip.Variables));

                return new RanToCompletionResult();
            }

            _context.Bus.Publish(new RoutingSlipActivityCompletedMessage(routingSlip.TrackingNumber, _activity.Name));

            IEndpoint endpoint = _context.Bus.GetEndpoint(routingSlip.GetNextExecuteAddress());

            endpoint.Forward(_context, routingSlip);

            return new NextActivityResult();
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


        class CompensateResult :
            ExecutionResult
        {
        }


        class FaultedResult :
            ExecutionResult
        {
        }


        class NextActivityResult :
            ExecutionResult
        {
        }


        class RanToCompletionResult :
            ExecutionResult
        {
        }
    }
}