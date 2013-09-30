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
namespace MassTransit.Courier.MongoDbIntegration.Tests
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NUnit.Framework;


    [TestFixture]
    public class MongoDbTestFixture
    {
        protected const string EventCollectionName = "Events";
        protected const string EventDatabaseName = "EventStore";

        protected MongoClient Client;
        protected MongoServer Server;
        protected MongoDatabase Database;
        MassTransitMongoDbConventions _convention;

        [TestFixtureSetUp]
        public void MongoDbTestFixtureSetup()
        {
            var builder = new MongoUrlBuilder
                {
                    DatabaseName = EventDatabaseName,
                    Server = new MongoServerAddress("localhost", 9001),
                    Username = "test",
                    Password = "password",
                    ConnectTimeout = TimeSpan.FromSeconds(30),
                    ConnectionMode = ConnectionMode.Automatic,
                    GuidRepresentation = GuidRepresentation.Standard
                };

            MongoUrl url = builder.ToMongoUrl();

            Client = new MongoClient(url);

            Server = Client.GetServer();

            Database = Server.GetDatabase(EventDatabaseName);

            _convention = new MassTransitMongoDbConventions();
        }
    }
}