﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.WebJobs.ServiceBusIntegration
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBusTransport.Contexts;
    using GreenPipes;


    public class CollectorSendEndpointContextSource :
        ISource<SendEndpointContext>
    {
        readonly SendEndpointContext _context;

        public CollectorSendEndpointContextSource(SendEndpointContext context)
        {
            _context = context;
        }

        public Task Send(IPipe<SendEndpointContext> pipe, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sharedContext = new SharedSendEndpointContext(_context, cancellationToken);

            return pipe.Send(sharedContext);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("binderSource");
        }
    }
}