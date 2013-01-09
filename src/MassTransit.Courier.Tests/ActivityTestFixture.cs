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
namespace MassTransit.Courier.Tests
{
    using System;
    using Configurators;
    using EndpointConfigurators;
    using Magnum.Extensions;
    using MassTransit.Exceptions;
    using NUnit.Framework;
    using Saga;
    using Transports;


    [TestFixture]
    public abstract class ActivityTestFixture
    {
        [TestFixtureSetUp]
        public void ActivityTextFixtureSetup()
        {
            if (_endpointFactoryConfigurator != null)
            {
                ConfigurationResult result =
                    ConfigurationResultImpl.CompileResults(_endpointFactoryConfigurator.Validate());

                try
                {
                    EndpointFactory = _endpointFactoryConfigurator.CreateEndpointFactory();
                    //	_endpointFactoryConfigurator = null;

                    _endpointCache = new EndpointCache(EndpointFactory);

                    EndpointCache = new EndpointCacheProxy(_endpointCache);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(result, "An exception was thrown during endpoint cache creation",
                        ex);
                }
            }

            ServiceBusFactory.ConfigureDefaultSettings(x =>
                {
                    x.SetEndpointCache(EndpointCache);
                    x.SetConcurrentConsumerLimit(4);
                    x.SetReceiveTimeout(50.Milliseconds());
                    x.EnableAutoStart();
                });
        }

        [TestFixtureTearDown]
        public void ActivityTestFixtureFixtureTeardown()
        {
            _endpointCache.Clear();

            if (EndpointCache != null)
            {
                EndpointCache.Dispose();
                EndpointCache = null;
            }

            ServiceBusFactory.ConfigureDefaultSettings(x => { x.SetEndpointCache(null); });
        }

        readonly EndpointFactoryConfiguratorImpl _endpointFactoryConfigurator;
        EndpointCache _endpointCache;

        protected ActivityTestFixture()
        {
            var defaultSettings = new EndpointFactoryDefaultSettings();

            _endpointFactoryConfigurator = new EndpointFactoryConfiguratorImpl(defaultSettings);
            _endpointFactoryConfigurator.SetPurgeOnStartup(true);
        }

        protected void AddTransport<T>()
            where T : class, ITransportFactory, new()
        {
            _endpointFactoryConfigurator.AddTransportFactory<T>();
        }

        protected IEndpointFactory EndpointFactory { get; private set; }
        protected IEndpointCache EndpointCache { get; set; }

        protected void ConfigureEndpointFactory(Action<EndpointFactoryConfigurator> configure)
        {
            if (_endpointFactoryConfigurator == null)
                throw new ConfigurationException("The endpoint factory configurator has already been executed.");

            configure(_endpointFactoryConfigurator);
        }

        protected static InMemorySagaRepository<TSaga> SetupSagaRepository<TSaga>()
            where TSaga : class, ISaga
        {
            var sagaRepository = new InMemorySagaRepository<TSaga>();

            return sagaRepository;
        }
    }
}