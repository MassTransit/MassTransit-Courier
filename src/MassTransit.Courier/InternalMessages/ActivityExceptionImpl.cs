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
namespace MassTransit.Courier.InternalMessages
{
    using System;
    using System.Diagnostics;
    using Contracts;


    class ActivityExceptionImpl :
        ActivityException
    {
        public ActivityExceptionImpl(string name, Uri hostAddress, Guid activityTrackingNumber, DateTime timestamp,
            Exception exception)
        {
            ActivityTrackingNumber = activityTrackingNumber;
            Timestamp = timestamp;
            Name = name;
            HostAddress = hostAddress;
            MachineName = Environment.MachineName;
            Process process = Process.GetCurrentProcess();
            ProcessName = process.ProcessName;
            ProcessId = process.Id;
            ExceptionInfo = new ExceptionInfoImpl(exception);
        }

        public Guid ActivityTrackingNumber { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Name { get; private set; }
        public Uri HostAddress { get; private set; }
        public string MachineName { get; private set; }
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }
        public ExceptionInfo ExceptionInfo { get; private set; }
    }
}