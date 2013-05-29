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


    class FaultResult :
        ExecutionResult
    {
        readonly Activity _activity;
        readonly IServiceBus _bus;
        readonly Exception _exception;
        readonly Guid _trackingNumber;

        public FaultResult(IServiceBus bus, Guid trackingNumber, Activity activity, Exception exception)
        {
            _bus = bus;
            _trackingNumber = trackingNumber;
            _activity = activity;
            _exception = exception;
        }

        public void Evaluate()
        {
            var activityFaulted = new RoutingSlipActivityFaultedMessage(_trackingNumber, _activity.Name, _exception);
            _bus.Publish(activityFaulted);

            var activityExceptionInfo = new ActivityExceptionImpl(_activity.Name, _bus.Endpoint.Address.Uri, _exception);

            var routingSlipFaulted = new RoutingSlipFaultedMessage(_trackingNumber, activityExceptionInfo);
            _bus.Publish(routingSlipFaulted);
        }


        class ActivityExceptionImpl :
            ActivityException
        {
            public ActivityExceptionImpl(string name, Uri hostAddress, Exception exception)
            {
                Name = name;
                HostAddress = hostAddress;
                ExceptionInfo = new ExceptionInfoImpl(exception);
            }

            public string Name { get; private set; }
            public Uri HostAddress { get; private set; }
            public ExceptionInfo ExceptionInfo { get; private set; }
        }
    }
}