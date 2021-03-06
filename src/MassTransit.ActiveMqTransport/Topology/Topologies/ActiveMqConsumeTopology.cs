// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ActiveMqTransport.Topology.Topologies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Builders;
    using GreenPipes;
    using MassTransit.Topology;
    using MassTransit.Topology.Topologies;
    using NewIdFormatters;
    using Specifications;
    using Util;


    public class ActiveMqConsumeTopology :
        ConsumeTopology,
        IActiveMqConsumeTopologyConfigurator
    {
        static readonly INewIdFormatter _formatter = new ZBase32Formatter();
        readonly IMessageTopology _messageTopology;
        readonly IActiveMqPublishTopology _publishTopology;
        readonly IList<IActiveMqConsumeTopologySpecification> _specifications;

        public ActiveMqConsumeTopology(IMessageTopology messageTopology, IActiveMqPublishTopology publishTopology)
        {
            _messageTopology = messageTopology;
            _publishTopology = publishTopology;

            _specifications = new List<IActiveMqConsumeTopologySpecification>();
        }

        IRabbitMqMessageConsumeTopology<T> IRabbitMqConsumeTopology.GetMessageTopology<T>()
        {
            return base.GetMessageTopology<T>() as IRabbitMqMessageConsumeTopologyConfigurator<T>;
        }

        public void AddSpecification(IActiveMqConsumeTopologySpecification specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(specification);
        }

        IRabbitMqMessageConsumeTopologyConfigurator<T> IActiveMqConsumeTopologyConfigurator.GetMessageTopology<T>()
        {
            return base.GetMessageTopology<T>() as IRabbitMqMessageConsumeTopologyConfigurator<T>;
        }

        public void Apply(IReceiveEndpointBrokerTopologyBuilder builder)
        {
            foreach (var specification in _specifications)
                specification.Apply(builder);

            ForEach<IRabbitMqMessageConsumeTopologyConfigurator>(x => x.Apply(builder));
        }

        public void Bind(string exchangeName, Action<ITopicBindingConfigurator> configure = null)
        {
            var specification = new TopicBindingConsumeTopologySpecification(exchangeName);

            configure?.Invoke(specification);

            _specifications.Add(specification);

            _specifications.Add(specification);
        }

        public string CreateTemporaryQueueName(string prefix)
        {
            var sb = new StringBuilder(prefix);

            var host = HostMetadataCache.Host;

            foreach (var c in host.MachineName)
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c == '.' || c == '_' || c == '-' || c == ':')
                    sb.Append(c);

            sb.Append('-');
            foreach (var c in host.ProcessName)
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c == '.' || c == '_' || c == '-' || c == ':')
                    sb.Append(c);

            sb.Append('-');
            sb.Append(NewId.Next().ToString(_formatter));

            return sb.ToString();
        }

        public override IEnumerable<ValidationResult> Validate()
        {
            return base.Validate().Concat(_specifications.SelectMany(x => x.Validate()));
        }

        protected override IMessageConsumeTopologyConfigurator CreateMessageTopology<T>(Type type)
        {
            var messageTopology = new RabbitMqMessageConsumeTopology<T>(_messageTopology.GetMessageTopology<T>(), _publishTopology.GetMessageTopology<T>());

            OnMessageTopologyCreated(messageTopology);

            return messageTopology;
        }
    }
}