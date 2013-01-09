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
namespace MassTransit.Courier.Tests
{
    using System;


    public class TestActivity :
        Activity<TestArguments, TestLog>
    {
        public ExecutionResult Execute(Execution<TestArguments> execution)
        {
            TestLog log = new TestLogImpl(execution.Arguments.Value);

            return execution.Completed(log);
        }

        public CompensationResult Compensate(Compensation<TestLog> compensation)
        {
            Console.WriteLine("Compensating Value: {0}", compensation.Log.Value);

            return compensation.Compensated();
        }


        class TestLogImpl :
            TestLog
        {
            public TestLogImpl(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }
        }
    }
}