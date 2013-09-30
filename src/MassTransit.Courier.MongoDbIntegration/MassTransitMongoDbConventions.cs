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
    using System.Linq.Expressions;
    using Documents;
    using Events;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;


    public class MassTransitMongoDbConventions
    {
        readonly ConventionPack _convention;
        readonly ConventionFilter _filter;

        public MassTransitMongoDbConventions(ConventionFilter filter = default(ConventionFilter))
        {
            _filter = filter ?? IsMassTransitClass;
            _convention = new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfDefaultConvention(true),
                    new MemberDefaultValueConvention(typeof(Guid), Guid.Empty),
                };

            ConventionRegistry.Register("MassTransitConventions", _convention, type => _filter(type));

            RegisterClass<RoutingSlipDocument>(x => x.TrackingNumber);
            RegisterClass<ExceptionInfoDocument>();
            RegisterClass<ActivityExceptionDocument>();
            RegisterClass<RoutingSlipEventDocument>();
            RegisterClass<RoutingSlipActivityCompensatedDocument>();
            RegisterClass<RoutingSlipActivityCompensationFailedDocument>();
            RegisterClass<RoutingSlipActivityCompletedDocument>();
            RegisterClass<RoutingSlipActivityFaultedDocument>();
            RegisterClass<RoutingSlipCompensationFailedDocument>();
            RegisterClass<RoutingSlipCompletedDocument>();
            RegisterClass<RoutingSlipFaultedDocument>();
        }

        static bool IsMassTransitClass(Type type)
        {
            return type.FullName.StartsWith("MassTransit");
        }

        public void RegisterClass<T>(Expression<Func<T, Guid>> id)
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
                return;

            BsonClassMap.RegisterClassMap<T>(x =>
                {
                    x.AutoMap();
                    x.SetIdMember(x.GetMemberMap(id));
                });
        }

        public void RegisterClass<T>()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
                return;

            BsonClassMap.RegisterClassMap<T>(x =>
                {
                    x.AutoMap();
                    x.SetDiscriminatorIsRequired(true);

                    string typeName = typeof(T).Name;
                    if (typeName.EndsWith("Document"))
                        x.SetDiscriminator(typeName.Substring(0, typeName.Length - "Document".Length));
                });
        }
    }
}