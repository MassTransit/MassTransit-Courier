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
namespace MassTransit.Courier.MongoDbIntegration
{
    using System;
    using Consumers;
    using Documents;
    using Events;
    using Logging;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;


    public class UpsertEventPersister :
        EventPersister
    {
        static readonly ILog _log = Logger.Get<RoutingSlipCompletedConsumer>();

        readonly MongoCollection _collection;

        public UpsertEventPersister(MongoCollection collection)
        {
            _collection = collection;
        }

        public void Persist<T>(Guid trackingNumber, T @event) where T : RoutingSlipEventDocument
        {
            IMongoQuery upsertQuery = Query<RoutingSlipDocument>.EQ(x => x.TrackingNumber, trackingNumber);

            WriteConcernResult result = _collection.Update(upsertQuery,
                Update<RoutingSlipDocument>.AddToSet(x => x.Events, @event),
                UpdateFlags.Upsert);

            if (result.Ok == false || result.DocumentsAffected != 1)
                throw new SaveEventException(trackingNumber, "Failed to save the event");

            // if this was the first save, this will be false
            if (result.UpdatedExisting == false)
            {
                if (_log.IsDebugEnabled)
                    _log.DebugFormat("{0} {1}", trackingNumber, typeof(T).Name);
            }
        }
    }
}