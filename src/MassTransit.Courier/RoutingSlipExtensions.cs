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
    using System.Linq;
    using Contracts;
    using InternalMessages;


    public static class RoutingSlipExtensions
    {
        /// <summary>
        /// Returns true if there are no remaining activities to be executed
        /// </summary>
        /// <param name="routingSlip"></param>
        /// <returns></returns>
        public static bool RanToCompletion(this RoutingSlip routingSlip)
        {
            return routingSlip.Itinerary == null || routingSlip.Itinerary.Count == 0;
        }

        /// <summary>
        /// Returns true if at least one activity log is present, signifying that activities have
        /// been executed with compensation logs
        /// </summary>
        /// <param name="routingSlip"></param>
        /// <returns></returns>
        public static bool IsRunning(this RoutingSlip routingSlip)
        {
            return routingSlip.ActivityLogs != null && routingSlip.ActivityLogs.Count > 0;
        }

        public static Uri GetNextExecuteAddress(this RoutingSlip routingSlip)
        {
            Activity activity = routingSlip.Itinerary.First();

            return activity.ExecuteAddress;
        }

        public static Uri GetNextCompensateAddress(this RoutingSlip routingSlip)
        {
            ActivityLog activity = routingSlip.ActivityLogs.Last();

            return activity.CompensateAddress;
        }

        public static void Execute(this IServiceBus bus, RoutingSlip routingSlip)
        {
            if (routingSlip.RanToCompletion())
            {
                bus.Publish<RoutingSlipCompleted>(new RoutingSlipCompletedMessage(routingSlip.TrackingNumber,
                    DateTime.UtcNow, routingSlip.Variables));
            }
            else
            {
                IEndpoint endpoint = bus.GetEndpoint(routingSlip.GetNextExecuteAddress());

                endpoint.Send(routingSlip, x => x.SetSourceAddress(bus.Endpoint.Address.Uri));
            }
        }
    }
}