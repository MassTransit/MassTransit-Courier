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
    using Contracts;
    using Internals.Extensions;


    class ExceptionInfoImpl :
        ExceptionInfo
    {
        readonly Exception _exception;

        public ExceptionInfoImpl(Exception exception)
        {
            _exception = exception;
        }

        public string ExceptionTypeName
        {
            get { return _exception.GetType().GetTypeName(); }
        }

        public ExceptionInfo InnerException
        {
            get
            {
                if (_exception.InnerException != null)
                    return new ExceptionInfoImpl(_exception.InnerException);
                return null;
            }
        }

        public string StackTrace
        {
            get { return _exception.StackTrace; }
        }

        public string Message
        {
            get { return _exception.Message; }
        }

        public string Source
        {
            get { return _exception.Source; }
        }
    }
}