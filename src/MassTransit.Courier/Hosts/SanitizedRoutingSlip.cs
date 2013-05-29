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
    using System.Runtime.Serialization;
    using Contracts;


    public class SanitizedRoutingSlip :
        RoutingSlip
    {
        public SanitizedRoutingSlip(RoutingSlip routingSlip)
        {
            TrackingNumber = routingSlip.TrackingNumber;
            if (routingSlip.Itinerary == null)
                Itinerary = new List<Activity>();
            else
                Itinerary = routingSlip.Itinerary.Select(x => (Activity)new SanitizedActivity(x)).ToList();

            if (routingSlip.ActivityLogs == null)
                ActivityLogs = new List<ActivityLog>();
            else
                ActivityLogs = routingSlip.ActivityLogs.Select(x => (ActivityLog)new SanitizedActivityLog(x)).ToList();

            Variables = routingSlip.Variables ?? new Dictionary<string, string>();

            if (routingSlip.ActivityExceptions == null)
                ActivityExceptions = new List<ActivityException>();
            else
            {
                ActivityExceptions =
                    routingSlip.ActivityExceptions.Select(x => (ActivityException)new SanitizedActivityException(x))
                               .ToList();
            }

            ActivityExceptions = routingSlip.ActivityExceptions ?? new List<ActivityException>();
        }


        public Guid TrackingNumber { get; private set; }
        public IList<Activity> Itinerary { get; private set; }
        public IList<ActivityLog> ActivityLogs { get; private set; }
        public IDictionary<string, string> Variables { get; private set; }
        public IList<ActivityException> ActivityExceptions { get; private set; }


        class SanitizedActivity :
            Activity
        {
            public SanitizedActivity(Activity activity)
            {
                if (string.IsNullOrEmpty(activity.Name))
                    throw new SerializationException("An Activity Name is required");
                if (activity.ExecuteAddress == null)
                    throw new SerializationException("An Activity ExecuteAddress is required");

                Name = activity.Name;
                ExecuteAddress = activity.ExecuteAddress;
                Arguments = activity.Arguments ?? new Dictionary<string, string>();
            }

            public string Name { get; private set; }
            public Uri ExecuteAddress { get; private set; }
            public IDictionary<string, string> Arguments { get; private set; }
        }


        class SanitizedActivityLog :
            ActivityLog
        {
            public SanitizedActivityLog(ActivityLog activityLog)
            {
                if (string.IsNullOrEmpty(activityLog.Name))
                    throw new SerializationException("An ActivityLog Name is required");
                if (activityLog.CompensateAddress == null)
                    throw new SerializationException("An ActivityLog CompensateAddress is required");

                ActivityTrackingNumber = activityLog.ActivityTrackingNumber;
                Name = activityLog.Name;
                CompensateAddress = activityLog.CompensateAddress;
                Results = activityLog.Results ?? new Dictionary<string, string>();
            }

            public Guid ActivityTrackingNumber { get; private set; }
            public string Name { get; private set; }
            public Uri CompensateAddress { get; private set; }
            public IDictionary<string, string> Results { get; private set; }
        }


        class SanitizedActivityException :
            ActivityException
        {
            public SanitizedActivityException(ActivityException activityException)
            {
                if (string.IsNullOrEmpty(activityException.Name))
                    throw new SerializationException("An Activity Name is required");
                if (activityException.HostAddress == null)
                    throw new SerializationException("An Activity HostAddress is required");
                if (activityException.ExceptionInfo == null)
                    throw new SerializationException("An Activity ExceptionInfo is required");

                Name = activityException.Name;
                HostAddress = activityException.HostAddress;
                ExceptionInfo = activityException.ExceptionInfo;
            }

            public string Name { get; private set; }
            public Uri HostAddress { get; private set; }
            public ExceptionInfo ExceptionInfo { get; private set; }
        }
    }
}