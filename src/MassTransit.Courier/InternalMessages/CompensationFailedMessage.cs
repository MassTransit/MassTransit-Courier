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
namespace MassTransit.Courier.InternalMessages
{
    using System;
    using Contracts;


    class CompensationFailedMessage :
        RoutingSlipActivityCompensationFailed,
        RoutingSlipCompensationFailed
    {
        public CompensationFailedMessage(Guid trackingNumber, string activityName, Guid activityTrackingNumber,
            DateTime timestamp, Exception exception)
        {
            Timestamp = timestamp;

            TrackingNumber = trackingNumber;
            ActivityTrackingNumber = activityTrackingNumber;
            ActivityName = activityName;
            Source = exception.Source;
            Message = exception.Message;
            StackTrace = exception.StackTrace;

            ExceptionInfo = new ExceptionInfoImpl(exception);
        }

        public Guid TrackingNumber { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Guid ActivityTrackingNumber { get; private set; }
        public string ActivityName { get; private set; }
        public string Source { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public ExceptionInfo ExceptionInfo { get; private set; }
    }
}