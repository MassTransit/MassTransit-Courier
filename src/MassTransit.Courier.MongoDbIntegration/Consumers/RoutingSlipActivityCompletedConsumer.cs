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
namespace MassTransit.Courier.MongoDbIntegration.Consumers
{
    using Contracts;
    using Events;


    public class RoutingSlipActivityCompletedConsumer :
        Consumes<RoutingSlipActivityCompleted>.Context
    {
        readonly EventPersister _persister;

        public RoutingSlipActivityCompletedConsumer(EventPersister persister)
        {
            _persister = persister;
        }

        public void Consume(IConsumeContext<RoutingSlipActivityCompleted> context)
        {
            var @event = new RoutingSlipActivityCompletedDocument(context.Message);

            _persister.Persist(context.Message.TrackingNumber, @event);
        }
    }
}