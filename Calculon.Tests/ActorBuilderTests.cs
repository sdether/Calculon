/*
 * Calculon 
 * Copyright (C) 2011 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Droog.Calculon.Framework;
using Moq;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorBuilderTests {
        private MockBackstage _backstage;

        [SetUp]
        public void Setup() {
            _backstage = new MockBackstage();
        }

        [TearDown]
        public void Teardown() {
        }

        ActorBuilder<TActor, ActorBuilderTests> AddActor<TActor>() {
            return new ActorBuilder<TActor, ActorBuilderTests>(this, _backstage);
        }

        [Test]
        public void Can_build_actor_with_no_parameter_ctor() {
            AddActor<NoDependencyActor>().Build();
            var actor = _backstage.Get<NoDependencyActor>();
            Assert.IsNotNull(actor);
        }
        [Test]
        public void Can_build_actor_with_address_dependency() {
            AddActor<AddressDependencyActor>().WithId("foo").Build();
            var actor = _backstage.Get<AddressDependencyActor>();
            Assert.IsNotNull(actor);
            Assert.AreEqual("foo", actor.Address.Id);
        }

        [Test]
        public void Can_build_actor_requiring_expression_transport_with_manual_builder() {
            AddActor<ExpressionTransportActor>().BuildWithExpressionTransport(t => new ExpressionTransportActor(t));
            var actor = _backstage.Get<ExpressionTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_expression_transport() {
            AddActor<ExpressionTransportActor>().Build();
            var actor = _backstage.Get<ExpressionTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Can_build_actor_requiring_expression_transport_and_meta_with_manual_builder() {
            AddActor<ExpressionTransportActorWithAddress>().WithId("foo").BuildWithExpressionTransport((t, m) => new ExpressionTransportActorWithAddress(t, m));
            var actor = _backstage.Get<ExpressionTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(ExpressionTransportActorWithAddress), actor.Address.Type);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_expression_transport_and_meta() {
            AddActor<ExpressionTransportActorWithAddress>().WithId("foo").Build();
            var actor = _backstage.Get<ExpressionTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(ExpressionTransportActorWithAddress), actor.Address.Type);
        }

        [Test]
        public void Can_build_actor_requiring_message_transport_with_manual_builder() {
            AddActor<MessageTransportActor>().BuildWithMessageTransport(t => new MessageTransportActor(t));
            var actor = _backstage.Get<MessageTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_message_transport() {
            AddActor<MessageTransportActor>().Build();
            var actor = _backstage.Get<MessageTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Can_build_actor_requiring_message_transport_and_meta_with_manual_builder() {
            AddActor<MessageTransportActorWithAddress>().WithId("foo").BuildWithMessageTransport((t, m) => new MessageTransportActorWithAddress(t, m));
            var actor = _backstage.Get<MessageTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(MessageTransportActorWithAddress), actor.Address.Type);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_message_transport_and_meta() {
            AddActor<MessageTransportActorWithAddress>().WithId("foo").Build();
            var actor = _backstage.Get<MessageTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(MessageTransportActorWithAddress), actor.Address.Type);
        }

        [Test]
        public void Can_build_actor_requiring_combined_transport_with_manual_builder() {
            AddActor<CombinedTransportActor>().BuildWithCombinedTransport(t => new CombinedTransportActor(t));
            var actor = _backstage.Get<CombinedTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_combined_transport() {
            AddActor<CombinedTransportActor>().Build();
            var actor = _backstage.Get<CombinedTransportActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Can_build_actor_requiring_combined_transport_and_meta_with_manual_builder() {
            AddActor<CombinedTransportActorWithAddress>().WithId("foo").BuildWithCombinedTransport((t, m) => new CombinedTransportActorWithAddress(t, m));
            var actor = _backstage.Get<CombinedTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(CombinedTransportActorWithAddress), actor.Address.Type);
        }

        [Test]
        public void Address_and_transport_are_injected_for_actor_requiring_combined_transport_and_meta() {
            AddActor<CombinedTransportActorWithAddress>().WithId("foo").Build();
            var actor = _backstage.Get<CombinedTransportActorWithAddress>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.IsNotNull(actor.Address);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(CombinedTransportActorWithAddress), actor.Address.Type);
        }

        public class NoDependencyActor { }
        public class AddressDependencyActor {
            public ActorAddress Address { get; private set; }

            public AddressDependencyActor(ActorAddress address) {
                Address = address;
            }
        }

        public class ExpressionTransportActor {
            public IExpressionTransport Transport { get; private set; }

            public ExpressionTransportActor(IExpressionTransport expressionTransport) {
                Transport = expressionTransport;
            }
        }

        public class ExpressionTransportActorWithAddress {
            public IExpressionTransport Transport { get; private set; }
            public ActorAddress Address { get; private set; }

            public ExpressionTransportActorWithAddress(IExpressionTransport transport, ActorAddress address) {
                Transport = transport;
                Address = address;
            }
        }

        public class MessageTransportActor {
            public IMessageTransport Transport { get; private set; }

            public MessageTransportActor(IMessageTransport transport) {
                Transport = transport;
            }
        }

        public class MessageTransportActorWithAddress {
            public IMessageTransport Transport { get; private set; }
            public ActorAddress Address { get; private set; }

            public MessageTransportActorWithAddress(IMessageTransport transport, ActorAddress address) {
                Transport = transport;
                Address = address;
            }
        }

        public class CombinedTransportActor {
            public ICombinedTransport Transport { get; private set; }

            public CombinedTransportActor(ICombinedTransport transport) {
                Transport = transport;
            }
        }

        public class CombinedTransportActorWithAddress {
            public ICombinedTransport Transport { get; private set; }
            public ActorAddress Address { get; private set; }

            public CombinedTransportActorWithAddress(ICombinedTransport transport, ActorAddress address) {
                Transport = transport;
                Address = address;
            }
        }

        public class MockBackstage : IBackstage {
            private object _actor;
            public void AddActor<TActor>(TActor actor, ActorAddress address, int parallelism) {
                _actor = actor;
            }

            public IExpressionTransport CreateExpressionTransport(ActorAddress address) {
                return new Mock<IExpressionTransport>().Object;
            }

            public IMessageTransport CreateMessageTransport(ActorAddress address) {
                return new Mock<IMessageTransport>().Object;
            }

            public ICombinedTransport CreateCombinedTransport(ActorAddress address) {
                return new Mock<ICombinedTransport>().Object;
            }

            public T Get<T>() {
                return (T)_actor;
            }
        }
    }
}
