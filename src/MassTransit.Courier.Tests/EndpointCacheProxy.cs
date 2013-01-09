﻿// Copyright 2007-2013 Chris Patterson
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
    using Diagnostics.Introspection;


    public class EndpointCacheProxy :
        IEndpointCache
    {
        readonly IEndpointCache _endpointCache;

        public EndpointCacheProxy(IEndpointCache endpointCache)
        {
            _endpointCache = endpointCache;
        }

        public void Dispose()
        {
            // we don't dispose, since we're in testing
        }

        public IEndpoint GetEndpoint(Uri uri)
        {
            return _endpointCache.GetEndpoint(uri);
        }

        public void Inspect(DiagnosticsProbe probe)
        {
            _endpointCache.Inspect(probe);
        }
    }
}