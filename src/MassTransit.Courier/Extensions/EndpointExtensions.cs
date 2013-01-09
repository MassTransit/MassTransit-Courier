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
namespace MassTransit.Courier.Extensions
{
    public static class EndpointExtensions
    {
        public static void Forward<T>(this IEndpoint endpoint, IConsumeContext context, T message)
        {
            endpoint.Send(message, x =>
                {
                    x.SetSourceAddress(context.SourceAddress);
                    x.SetFaultAddress(context.FaultAddress);
                    if (context.ExpirationTime.HasValue)
                        x.SetExpirationTime(context.ExpirationTime.Value);
                    x.SetNetwork(context.Network);
                    x.SetRequestId(context.RequestId);
                    x.SetConversationId(context.ConversationId);
                    foreach (var header in context.Headers)
                        x.SetHeader(header.Key, header.Value);
                });
        }
    }
}