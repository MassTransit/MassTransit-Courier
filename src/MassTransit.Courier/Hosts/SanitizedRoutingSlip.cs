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


    public class SanitizedRoutingSlip :
        RoutingSlip
    {
        public SanitizedRoutingSlip(RoutingSlip routingSlip)
        {
            TrackingNumber = routingSlip.TrackingNumber;
            if (routingSlip.Activities == null)
                Activities = new List<Activity>();
            else
                Activities = routingSlip.Activities.Select(x => (Activity)new SanitizedActivity(x)).ToList();

            if (routingSlip.ActivityLogs == null)
                ActivityLogs = new List<ActivityLog>();
            else
                ActivityLogs = routingSlip.ActivityLogs.Select(x => (ActivityLog)new SanitizedActivityLog(x)).ToList();

            Variables = routingSlip.Variables ?? new Dictionary<string, string>();
        }

        public Guid TrackingNumber { get; private set; }
        public IList<Activity> Activities { get; private set; }
        public IList<ActivityLog> ActivityLogs { get; private set; }
        public IDictionary<string, string> Variables { get; private set; }


        class SanitizedActivity :
            Activity
        {
            public SanitizedActivity(Activity activity)
            {
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
                Name = activityLog.Name;
                CompensateAddress = activityLog.CompensateAddress;
                Results = activityLog.Results ?? new Dictionary<string, string>();
            }

            public string Name { get; private set; }
            public Uri CompensateAddress { get; private set; }
            public IDictionary<string, string> Results { get; private set; }
        }
    }
}