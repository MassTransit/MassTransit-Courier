﻿// Copyright 2007-2013 Chris Patterson
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
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;


    class RoutingSlipImpl :
        RoutingSlip
    {
        public RoutingSlipImpl(Guid trackingNumber, IEnumerable<Activity> activities,
            IEnumerable<ActivityLog> activityLogs, IDictionary<string, object> variables,
            IEnumerable<ActivityException> exceptions)
        {
            TrackingNumber = trackingNumber;
            Itinerary = activities.ToList();
            ActivityLogs = activityLogs.ToList();
            Variables = variables ?? new Dictionary<string, object>();
            ActivityExceptions = exceptions.ToList();
        }


        public IList<Activity> Itinerary { get; private set; }
        public Guid TrackingNumber { get; private set; }
        public IList<ActivityLog> ActivityLogs { get; private set; }
        public IDictionary<string, object> Variables { get; private set; }
        public IList<ActivityException> ActivityExceptions { get; private set; }
    }
}