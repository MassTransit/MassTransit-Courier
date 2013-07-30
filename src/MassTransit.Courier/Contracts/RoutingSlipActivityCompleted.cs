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
namespace MassTransit.Courier.Contracts
{
    using System;
    using System.Collections.Generic;


    public interface RoutingSlipActivityCompleted
    {
        /// <summary>
        /// The tracking number of the routing slip that faulted
        /// </summary>
        Guid TrackingNumber { get; }

        /// <summary>
        /// The date/time when the routing slip compensation was finished
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// The tracking number for completion of the activity
        /// </summary>
        Guid ActivityTrackingNumber { get; }

        /// <summary>
        /// The name of the activity that completed
        /// </summary>
        string ActivityName { get; }

        /// <summary>
        /// The results saved as the ActivityLog by the Activity
        /// </summary>
        IDictionary<string, object> Results { get; }

        /// <summary>
        /// The variables that were present once the routing slip completed, can be used
        /// to capture the output of the slip - real events should likely be used for real
        /// completion items but this is useful for some cases
        /// </summary>
        IDictionary<string, object> Variables { get; }
    }
}