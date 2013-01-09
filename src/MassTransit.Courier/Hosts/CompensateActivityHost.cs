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
    using System.Linq;
    using Contracts;
    using Magnum.Reflection;


    public class CompensateActivityHost<TController, TLog> :
        Consumes<RoutingSlip>.Context
        where TController : CompensateActivity<TLog>
        where TLog : class
    {
        readonly Func<TLog, TController> _controllerFactory;

        public CompensateActivityHost(Func<TLog, TController> controllerFactory)
        {
            _controllerFactory = controllerFactory;
        }

        public void Consume(IConsumeContext<RoutingSlip> context)
        {
            var compensation = new HostCompensation<TLog>(context);

            try
            {
                TController controller = _controllerFactory(compensation.Log);

                CompensationResult result = controller.Compensate(compensation);
            }
            catch (Exception ex)
            {
                compensation.Failed(ex);
            }
        }

        static TLog GetActivityLogResults(ActivityLog activityLog)
        {
            IDictionary<string, string> dictionary = activityLog.Results ?? new Dictionary<string, string>();
            IDictionary<string, object> initializer = dictionary.ToDictionary(x => x.Key, x => (object)x.Value);

            return InterfaceImplementationExtensions.InitializeProxy<TLog>(initializer);
        }
    }
}