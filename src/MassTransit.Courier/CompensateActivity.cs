﻿// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    public interface CompensateActivity<in TLog>
        where TLog : class
    {
        /// <summary>
        /// Compensate the activity and return the remaining compensation items
        /// </summary>
        /// <param name="routingSlip">The routing slip being unrolled</param>
        /// <param name="log">The activity log that was saved by this activity when it was executed</param>
        /// <returns></returns>
        CompensationResult Compensate(Compensation<TLog> compensation);
    }
}