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
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using Context;
    using Contracts;
    using Magnum.Reflection;
    using MassTransit.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serialization;


    public class SanitizedRoutingSlip :
        RoutingSlip
    {
        readonly IConsumeContext<RoutingSlip> _context;
        readonly IConsumeContext<RoutingSlip> _jsonContext;
        readonly JToken _messageToken;
        readonly JToken _variablesToken;

        public SanitizedRoutingSlip(IConsumeContext<RoutingSlip> context)
        {
            _context = context;

            using (var ms = new MemoryStream())
            {
                context.BaseContext.CopyBodyTo(ms);

                ReceiveContext receiveContext = ReceiveContext.FromBodyStream(ms, false);

                if (string.Compare(context.ContentType, "application/vnd.masstransit+json",
                    StringComparison.OrdinalIgnoreCase) == 0)
                    _jsonContext = TranslateJsonBody(receiveContext);
                else if (string.Compare(context.ContentType, "application/vnd.masstransit+xml",
                    StringComparison.OrdinalIgnoreCase) == 0)
                    _jsonContext = TranslateXmlBody(receiveContext);
                else
                    throw new InvalidOperationException("Only JSON and XML messages can be scheduled");
            }

            IConsumeContext<JToken> messageTokenContext;
            if (!_jsonContext.TryGetContext(out messageTokenContext))
                throw new InvalidOperationException("Unable to retrieve JSON token");

            _messageToken = messageTokenContext.Message;

            RoutingSlip routingSlip = _jsonContext.Message;

            TrackingNumber = routingSlip.TrackingNumber;

            _variablesToken = _messageToken["variables"];

            Variables = routingSlip.Variables ?? JsonConvert.DeserializeObject<IDictionary<string, object>>("{}");

            if (routingSlip.Itinerary == null)
                Itinerary = new List<Activity>();
            else
                Itinerary = routingSlip.Itinerary.Select(x => (Activity)new SanitizedActivity(x, Variables)).ToList();

            if (routingSlip.ActivityLogs == null)
                ActivityLogs = new List<ActivityLog>();
            else
            {
                ActivityLogs =
                    routingSlip.ActivityLogs.Select(x => (ActivityLog)new SanitizedActivityLog(x, Variables)).ToList();
            }

            if (routingSlip.ActivityExceptions == null)
                ActivityExceptions = new List<ActivityException>();
            else
            {
                ActivityExceptions = routingSlip.ActivityExceptions
                                                .Select(x => (ActivityException)new SanitizedActivityException(x))
                                                .ToList();
            }

            ActivityExceptions = routingSlip.ActivityExceptions ?? new List<ActivityException>();
        }


        public Guid TrackingNumber { get; private set; }
        public IList<Activity> Itinerary { get; private set; }
        public IList<ActivityLog> ActivityLogs { get; private set; }
        public IDictionary<string, object> Variables { get; private set; }
        public IList<ActivityException> ActivityExceptions { get; private set; }


        public T GetActivityArguments<T>()
        {
            JToken itineraryToken = _messageToken["itinerary"];

            JToken activityToken = itineraryToken is JArray ? itineraryToken[0] : itineraryToken;

            JToken token = activityToken["arguments"].Merge(_variablesToken);
            if (token.Type == JTokenType.Null)
                token = new JObject();

            using (var jsonReader = new JTokenReader(token))
            {
                return (T)JsonMessageSerializer.Deserializer.Deserialize(jsonReader, typeof(T));
            }
        }

        public T GetActivityLog<T>()
        {
            JToken activityLogsToken = _messageToken["activityLogs"];

            JToken activityLogToken;
            if (activityLogsToken is JArray)
            {
                var logsToken = activityLogsToken as JArray;
                activityLogToken = activityLogsToken[logsToken.Count - 1];
            }
            else
                activityLogToken = activityLogsToken;

            JToken token = activityLogToken["results"].Merge(_variablesToken);
            if (token.Type == JTokenType.Null)
                token = new JObject();

            using (var jsonReader = new JTokenReader(token))
            {
                return (T)JsonMessageSerializer.Deserializer.Deserialize(jsonReader, typeof(T));
            }
        }

        static IConsumeContext<RoutingSlip> TranslateJsonBody(IReceiveContext context)
        {
            var serializer = new JsonMessageSerializer();

            serializer.Deserialize(context);

            IConsumeContext<RoutingSlip> routingSlipContext;
            if (context.TryGetContext(out routingSlipContext))
                return routingSlipContext;

            throw new InvalidOperationException("Unable to reprocess message as RoutingSlip");
        }

        static IConsumeContext<RoutingSlip> TranslateXmlBody(IReceiveContext context)
        {
            var serializer = new XmlMessageSerializer();

            serializer.Deserialize(context);

            IConsumeContext<RoutingSlip> routingSlipContext;
            if (context.TryGetContext(out routingSlipContext))
                return routingSlipContext;

            throw new InvalidOperationException("Unable to reprocess message as RoutingSlip");
        }


        class SanitizedActivity :
            Activity
        {
            public SanitizedActivity(Activity activity, IDictionary<string, object> variables)
            {
                if (string.IsNullOrEmpty(activity.Name))
                    throw new SerializationException("An Activity Name is required");
                if (activity.ExecuteAddress == null)
                    throw new SerializationException("An Activity ExecuteAddress is required");

                Name = activity.Name;
                ExecuteAddress = activity.ExecuteAddress;
                Arguments = activity.Arguments ?? JsonConvert.DeserializeObject<IDictionary<string, object>>("{}");
            }

            public string Name { get; private set; }
            public Uri ExecuteAddress { get; private set; }
            public IDictionary<string, object> Arguments { get; private set; }
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


        class SanitizedActivityLog :
            ActivityLog
        {
            public SanitizedActivityLog(ActivityLog activityLog, IDictionary<string, object> variables)
            {
                if (string.IsNullOrEmpty(activityLog.Name))
                    throw new SerializationException("An ActivityLog Name is required");
                if (activityLog.CompensateAddress == null)
                    throw new SerializationException("An ActivityLog CompensateAddress is required");

                ActivityTrackingNumber = activityLog.ActivityTrackingNumber;
                Name = activityLog.Name;
                CompensateAddress = activityLog.CompensateAddress;
                Results = activityLog.Results ?? JsonConvert.DeserializeObject<IDictionary<string, object>>("{}");
            }

            public Guid ActivityTrackingNumber { get; private set; }
            public string Name { get; private set; }
            public Uri CompensateAddress { get; private set; }
            public IDictionary<string, object> Results { get; private set; }
        }
    }
}