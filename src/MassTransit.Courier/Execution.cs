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
    using System.Collections.Generic;


    public interface Execution<out TArguments>
        where TArguments : class
    {
        TArguments Arguments { get; }

        /// <summary>
        /// The tracking number for this routing slip
        /// </summary>
        Guid TrackingNumber { get; }

        /// <summary>
        /// The tracking number for this activity within the routing slip
        /// </summary>
        Guid ActivityTrackingNumber { get; }

        /// <summary>
        /// The service bus instance where the activity is hosted
        /// </summary>
        IServiceBus Bus { get; }

        /// <summary>
        /// Completes the execution, without passing a compensating log entry
        /// </summary>
        /// <returns></returns>
        ExecutionResult Completed();

        /// <summary>
        /// Completes the activity, passing a compensation log entry
        /// </summary>
        /// <typeparam name="TLog"></typeparam>
        /// <param name="log"></param>
        /// <returns></returns>
        ExecutionResult Completed<TLog>(TLog log)
            where TLog : class;

        /// <summary>
        /// Completes the activity, passing a compensation log entry and additional variables to set on 
        /// the routing slip
        /// </summary>
        /// <typeparam name="TLog"></typeparam>
        /// <param name="log"></param>
        /// <param name="values">An anonymous object of values to add/set as variables on the routing slip</param>
        /// <returns></returns>
        ExecutionResult Completed<TLog>(TLog log, object values)
            where TLog : class;

        /// <summary>
        /// Completes the activity, passing a compensation log entry and additional variables to set on 
        /// the routing slip
        /// </summary>
        /// <typeparam name="TLog"></typeparam>
        /// <param name="log"></param>
        /// <param name="values">An dictionary of values to add/set as variables on the routing slip</param>
        /// <returns></returns>
        ExecutionResult Completed<TLog>(TLog log, IDictionary<string, object> values)
            where TLog : class;

        /// <summary>
        /// The activity Faulted for an unknown reason, but compensation should be triggered
        /// </summary>
        /// <returns></returns>
        ExecutionResult Faulted();

        /// <summary>
        /// The activity Faulted, and compensation should be triggered
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        ExecutionResult Faulted(Exception exception);
    }
}