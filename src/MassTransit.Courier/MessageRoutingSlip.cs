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
namespace MassTransit.Courier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Magnum.Reflection;


    public class MessageRoutingSlip :
        RoutingSlip
    {
        public MessageRoutingSlip(Guid trackingNumber)
        {
            TrackingNumber = trackingNumber;
            Activities = new List<Activity>();
            ActivityLogs = new List<ActivityLog>();
            Variables = new Dictionary<string, string>();
        }

        public MessageRoutingSlip(Guid trackingNumber, IEnumerable<Activity> activities,
            IEnumerable<ActivityLog> activityLogs,
            IDictionary<string, string> variables)
        {
            TrackingNumber = trackingNumber;
            Activities = activities.ToList();
            ActivityLogs = activityLogs.ToList();
            Variables = variables ?? new Dictionary<string, string>();
        }


        public Guid TrackingNumber { get; private set; }
        public IList<Activity> Activities { get; private set; }
        public IList<ActivityLog> ActivityLogs { get; private set; }
        public IDictionary<string, string> Variables { get; private set; }

        public void AddActivity(string name, Uri executeAddress, object arguments)
        {
            Activity activity = new ActivityImpl(name, executeAddress, GetObjectAsDictionary(arguments));
            Activities.Add(activity);
        }

        public void AddActivityLog(string name, Uri compensateAddress, object results)
        {
            ActivityLog activity = new ActivityLogImpl(name, compensateAddress, GetObjectAsDictionary(results));
            ActivityLogs.Add(activity);
        }

        IDictionary<string, string> GetObjectAsDictionary(object values)
        {
            IDictionary<string, object> dictionary = Statics.Converter.Convert(values);

            return dictionary.ToDictionary(x => x.Key, x => x.Value.ToString());
        }


        class ActivityImpl :
            Activity
        {
            public ActivityImpl(string name, Uri executeAddress, IDictionary<string, string> arguments)
            {
                Name = name;
                ExecuteAddress = executeAddress;
                Arguments = arguments;
            }

            public string Name { get; private set; }
            public Uri ExecuteAddress { get; private set; }
            public IDictionary<string, string> Arguments { get; private set; }
        }


        class ActivityLogImpl :
            ActivityLog
        {
            public ActivityLogImpl(string name, Uri compensateAddress, IDictionary<string, string> results)
            {
                Name = name;
                CompensateAddress = compensateAddress;
                Results = results;
            }

            public string Name { get; private set; }
            public Uri CompensateAddress { get; private set; }
            public IDictionary<string, string> Results { get; private set; }
        }


        static class Statics
        {
            internal static readonly AnonymousObjectDictionaryConverter Converter =
                new AnonymousObjectDictionaryConverter();

            /// <summary>
            /// Forces lazy load of all static fields in a thread-safe way.
            /// The static initializer will not be executed until a property or method in that class
            /// has been executed for the first time.
            /// </summary>
            static Statics()
            {
            }
        }
    }
}