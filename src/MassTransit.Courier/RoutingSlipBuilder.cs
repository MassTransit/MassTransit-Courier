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
namespace MassTransit.Courier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hosts;
    using Magnum.Reflection;


    public class RoutingSlipBuilder
    {
        public static readonly IDictionary<string, string> NoArguments = new Dictionary<string, string>();

        readonly IList<ActivityLog> _activityLogs;
        readonly IList<Activity> _itinerary;
        readonly Guid _trackingNumber;
        readonly IDictionary<string, string> _variables;

        public RoutingSlipBuilder(Guid trackingNumber)
        {
            _trackingNumber = trackingNumber;
            _itinerary = new List<Activity>();
            _activityLogs = new List<ActivityLog>();
            _variables = new Dictionary<string, string>();
        }

        public RoutingSlipBuilder(Guid trackingNumber, IEnumerable<Activity> activities,
            IEnumerable<ActivityLog> activityLogs, IDictionary<string, string> variables)
        {
            _trackingNumber = trackingNumber;
            _itinerary = activities.ToList();
            _activityLogs = activityLogs.ToList();
            _variables = variables ?? new Dictionary<string, string>();
        }

        public Guid TrackingNumber
        {
            get { return _trackingNumber; }
        }

        public RoutingSlip Build()
        {
            return new RoutingSlipImpl(TrackingNumber, _itinerary, _activityLogs, _variables);
        }

        public void AddActivity(string name, Uri executeAddress)
        {
            Activity activity = new ActivityImpl(name, executeAddress, NoArguments);
            _itinerary.Add(activity);
        }

        public void AddActivity(string name, Uri executeAddress, object arguments)
        {
            Activity activity = new ActivityImpl(name, executeAddress, GetObjectAsDictionary(arguments));
            _itinerary.Add(activity);
        }

        public void AddActivity(string name, Uri executeAddress, IDictionary<string, string> arguments)
        {
            Activity activity = new ActivityImpl(name, executeAddress, arguments);
            _itinerary.Add(activity);
        }

        public ActivityLog AddActivityLog(string name, Uri compensateAddress, object logObject)
        {
            Guid activityTrackingNumber = NewId.NextGuid();
            IDictionary<string, string> results = GetObjectAsDictionary(logObject);

            ActivityLog activityLog = new ActivityLogImpl(activityTrackingNumber, name, compensateAddress, results);
            _activityLogs.Add(activityLog);

            return activityLog;
        }

        public ActivityLog AddActivityLog(string name, Uri compensateAddress, IDictionary<string, string> results)
        {
            Guid activityTrackingNumber = NewId.NextGuid();

            ActivityLog activityLog = new ActivityLogImpl(activityTrackingNumber, name, compensateAddress, results);
            _activityLogs.Add(activityLog);

            return activityLog;
        }

        public void AddVariable(string key, string value)
        {
            _variables.Add(key, value);
        }

        /// <summary>
        /// Sets the value of any existing variables to the value in the anonymous object,
        /// as well as adding any additional variables that did not exist previously.
        /// </summary>
        /// <param name="values"></param>
        public void SetVariables(object values)
        {
            IDictionary<string, string> dictionary = GetObjectAsDictionary(values);

            ApplyDictionaryToVariables(dictionary);
        }

        public void SetVariables(IEnumerable<KeyValuePair<string, string>> values)
        {
            ApplyDictionaryToVariables(values);
        }

        void ApplyDictionaryToVariables(IEnumerable<KeyValuePair<string, string>> logValues)
        {
            foreach (var logValue in logValues)
            {
                if (string.IsNullOrEmpty(logValue.Value))
                    _variables.Remove(logValue.Key);
                else
                    _variables[logValue.Key] = logValue.Value;
            }
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
            public ActivityLogImpl(Guid activityTrackingNumber, string name, Uri compensateAddress,
                IDictionary<string, string> results)
            {
                ActivityTrackingNumber = activityTrackingNumber;
                Name = name;
                CompensateAddress = compensateAddress;
                Results = results;
            }

            public Guid ActivityTrackingNumber { get; private set; }
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