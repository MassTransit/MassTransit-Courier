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


    public interface Compensation<out TLog>
        where TLog : class
    {
        TLog Log { get; }

        /// <summary>
        /// The tracking number for this routing slip
        /// </summary>
        Guid TrackingNumber { get; }

        /// <summary>
        /// The compensation was successful
        /// </summary>
        /// <returns></returns>
        CompensationResult Compensated();

        /// <summary>
        /// The compensation failed
        /// </summary>
        /// <returns></returns>
        CompensationResult Failed();

        /// <summary>
        /// The compensation failed with the specified exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        CompensationResult Failed(Exception exception);
    }
}