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
    using InternalMessages;
    using Magnum.Reflection;


    /// <summary>
    /// A RoutingSlipBuilder is used to create a routing slip with proper validation that the resulting RoutingSlip
    /// is valid.
    /// </summary>
    public class RoutingSlipBuilder :
        ItineraryBuilder
    {
        public static readonly IDictionary<string, object> NoArguments = new Dictionary<string, object>();
        readonly IList<ActivityException> _activityExceptions;

        readonly IList<ActivityLog> _activityLogs;
        readonly IList<Activity> _itinerary;
        readonly List<Activity> _sourceItinerary;
        readonly Guid _trackingNumber;
        readonly IDictionary<string, object> _variables;

        public RoutingSlipBuilder(Guid trackingNumber)
        {
            _trackingNumber = trackingNumber;
            _itinerary = new List<Activity>();
            _sourceItinerary = new List<Activity>();
            _activityLogs = new List<ActivityLog>();
            _variables = new Dictionary<string, object>();
            _activityExceptions = new List<ActivityException>();
        }

        public RoutingSlipBuilder(Guid trackingNumber, IEnumerable<Activity> activities,
            IEnumerable<ActivityLog> activityLogs, IDictionary<string, object> variables,
            IEnumerable<ActivityException> activityExceptions)
        {
            _trackingNumber = trackingNumber;
            _itinerary = activities.ToList();
            _sourceItinerary = new List<Activity>();
            _activityLogs = activityLogs.ToList();
            _variables = variables ?? new Dictionary<string, object>();
            _activityExceptions = activityExceptions.ToList();
        }

        /// <summary>
        /// The tracking number of the routing slip
        /// </summary>
        public Guid TrackingNumber
        {
            get { return _trackingNumber; }
        }

        /// <summary>
        /// Adds an activity to the routing slip without specifying any arguments
        /// </summary>
        /// <param name="name">The activity name</param>
        /// <param name="executeAddress">The execution address of the activity</param>
        public void AddActivity(string name, Uri executeAddress)
        {
            Activity activity = new ActivityImpl(name, executeAddress, NoArguments);
            _itinerary.Add(activity);
        }

        /// <summary>
        /// Adds an activity to the routing slip specifying activity arguments as an anonymous object
        /// </summary>
        /// <param name="name">The activity name</param>
        /// <param name="executeAddress">The execution address of the activity</param>
        /// <param name="arguments">An anonymous object of properties matching the argument names of the activity</param>
        public void AddActivity(string name, Uri executeAddress, object arguments)
        {
            IDictionary<string, object> argumentsDictionary = GetObjectAsDictionary(arguments);

            AddActivity(name, executeAddress, argumentsDictionary);
        }

        /// <summary>
        /// Adds an activity to the routing slip specifying activity arguments a dictionary
        /// </summary>
        /// <param name="name">The activity name</param>
        /// <param name="executeAddress">The execution address of the activity</param>
        /// <param name="arguments">A dictionary of name/values matching the activity argument properties</param>
        public void AddActivity(string name, Uri executeAddress, IDictionary<string, object> arguments)
        {
            Activity activity = new ActivityImpl(name, executeAddress, arguments);
            _itinerary.Add(activity);
        }

        public void AddVariable(string key, string value)
        {
            _variables.Add(key, value);
        }


        /// <summary>
        /// Sets the value of any existing variables to the value in the anonymous object,
        /// as well as adding any additional variables that did not exist previously.
        /// 
        /// For example, SetVariables(new { IntValue = 27, StringValue = "Hello, World." });
        /// </summary>
        /// <param name="values"></param>
        public void SetVariables(object values)
        {
            IDictionary<string, object> dictionary = GetObjectAsDictionary(values);

            ApplyDictionaryToVariables(dictionary);
        }


        public void SetVariables(IEnumerable<KeyValuePair<string, object>> values)
        {
            ApplyDictionaryToVariables(values);
        }

        public int AddSourceItinerary()
        {
            int count = _sourceItinerary.Count;

            foreach (Activity activity in _sourceItinerary)
                _itinerary.Add(activity);
            _sourceItinerary.Clear();

            return count;
        }

        /// <summary>
        /// Builds the routing slip using the current state of the builder
        /// </summary>
        /// <returns>The RoutingSlip</returns>
        public RoutingSlip Build()
        {
            return new RoutingSlipImpl(TrackingNumber, _itinerary, _activityLogs, _variables, _activityExceptions);
        }

        /// <summary>
        /// Specify the source itinerary for this routing slip builder
        /// </summary>
        /// <param name="sourceItinerary"></param>
        public void SetSourceItinerary(IEnumerable<Activity> sourceItinerary)
        {
            _sourceItinerary.Clear();
            _sourceItinerary.AddRange(sourceItinerary);
        }

        public ActivityLog AddActivityLog(string name, Guid activityTrackingNumber, Uri compensateAddress,
            object logObject)
        {
            IDictionary<string, object> resultsDictionary = GetObjectAsDictionary(logObject);

            return AddActivityLog(name, activityTrackingNumber, compensateAddress, resultsDictionary);
        }

        public ActivityLog AddActivityLog(string name, Guid activityTrackingNumber, Uri compensateAddress,
            IDictionary<string, object> results)
        {
            ActivityLog activityLog = new ActivityLogImpl(activityTrackingNumber, name, compensateAddress, results);
            _activityLogs.Add(activityLog);

            return activityLog;
        }

        /// <summary>
        /// Adds an activity exception to the routing slip
        /// </summary>
        /// <param name="name">The name of the faulted activity</param>
        /// <param name="hostAddress">The host address where the faulted activity executed</param>
        /// <param name="timestamp">The timestamp of the exception</param>
        /// <param name="exception">The exception thrown by the activity</param>
        /// <param name="activityTrackingNumber">The activity tracking number</param>
        /// <returns>The ActivityExceptionInfo that was added</returns>
        public ActivityException AddActivityException(string name, Uri hostAddress, Guid activityTrackingNumber,
            DateTime timestamp, Exception exception)
        {
            ActivityException activityException = new ActivityExceptionImpl(name, hostAddress, activityTrackingNumber,
                timestamp, exception);
            _activityExceptions.Add(activityException);

            return activityException;
        }


        void ApplyDictionaryToVariables(IEnumerable<KeyValuePair<string, object>> logValues)
        {
            foreach (var logValue in logValues)
            {
                if (logValue.Value == null
                    || (logValue.Value is string && string.IsNullOrEmpty((string)logValue.Value)))
                    _variables.Remove(logValue.Key);
                else
                    _variables[logValue.Key] = logValue.Value;
            }
        }


        IDictionary<string, object> GetObjectAsDictionary(object values)
        {
            IDictionary<string, object> dictionary = Statics.Converter.Convert(values);

            return dictionary.ToDictionary(x => x.Key, x => x.Value);
        }


        class ActivityExceptionImpl :
            ActivityException
        {
            public ActivityExceptionImpl(string name, Uri hostAddress, Guid activityTrackingNumber, DateTime timestamp,
                Exception exception)
            {
                ActivityTrackingNumber = activityTrackingNumber;
                Timestamp = timestamp;
                Name = name;
                HostAddress = hostAddress;
                ExceptionInfo = new ExceptionInfoImpl(exception);
            }

            public Guid ActivityTrackingNumber { get; private set; }
            public DateTime Timestamp { get; private set; }
            public string Name { get; private set; }
            public Uri HostAddress { get; private set; }
            public ExceptionInfo ExceptionInfo { get; private set; }
        }


        class ActivityImpl :
            Activity
        {
            public ActivityImpl(string name, Uri executeAddress, IDictionary<string, object> arguments)
            {
                Name = name;
                ExecuteAddress = executeAddress;
                Arguments = arguments;
            }

            public string Name { get; private set; }
            public Uri ExecuteAddress { get; private set; }
            public IDictionary<string, object> Arguments { get; private set; }
        }


        class ActivityLogImpl :
            ActivityLog
        {
            public ActivityLogImpl(Guid activityTrackingNumber, string name, Uri compensateAddress,
                IDictionary<string, object> results)
            {
                ActivityTrackingNumber = activityTrackingNumber;
                Name = name;
                CompensateAddress = compensateAddress;
                Results = results;
            }

            public Guid ActivityTrackingNumber { get; private set; }
            public string Name { get; private set; }
            public Uri CompensateAddress { get; private set; }
            public IDictionary<string, object> Results { get; private set; }
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