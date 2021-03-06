﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Context
{
    using System;
    using System.Threading.Tasks;


    /// <summary>
    /// The context for an outbox instance as part of consume context. Used to signal the completion of
    /// the consume, and store any Task factories that should be created.
    /// </summary>
    public interface OutboxContext
    {
        /// <summary>
        /// Returns an awaitable task that is completed when it is clear to send messages
        /// </summary>
        Task ClearToSend { get; }

        /// <summary>
        /// Adds a method to be invoked once the outbox is ready to be sent
        /// </summary>
        /// <param name="method"></param>
        void Add(Func<Task> method);
    }
}