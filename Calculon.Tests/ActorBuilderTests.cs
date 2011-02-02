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
            Assert.AreEqual(typeof(AddressDependencyActor), actor.Address.Type);
        }

        [Test]
        public void Can_build_actor_requiring_transport() {
            AddActor<TransportDependencyActor>().Build();
            var actor = _backstage.Get<TransportDependencyActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Can_build_actor_requiring_transport_and_address() {
            AddActor<TransportAndAddressDependencyActor>().WithId("foo").Build();
            var actor = _backstage.Get<TransportAndAddressDependencyActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(TransportAndAddressDependencyActor), actor.Address.Type);
        }

        [Test]
        public void Can_build_actor_requiring_transport_with_manual_builder() {
            AddActor<TransportDependencyActor>().Build(t => new TransportDependencyActor(t));
            var actor = _backstage.Get<TransportDependencyActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Can_build_actor_requiring_transport_and_address_with_manual_builder() {
            AddActor<TransportAndAddressDependencyActor>().WithId("foo").Build((t, a) => new TransportAndAddressDependencyActor(t, a));
            var actor = _backstage.Get<TransportAndAddressDependencyActor>();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
            Assert.AreEqual("foo", actor.Address.Id);
            Assert.AreEqual(typeof(TransportAndAddressDependencyActor), actor.Address.Type);
        }

     
        public class NoDependencyActor { }
        public class AddressDependencyActor {
            public ActorAddress Address { get; private set; }

            public AddressDependencyActor(ActorAddress address) {
                Address = address;
            }
        }

        public class TransportDependencyActor {
            public ITransport Transport { get; private set; }

            public TransportDependencyActor(ITransport transport) {
                Transport = transport;
            }
        }


        public class TransportAndAddressDependencyActor {
            public ITransport Transport { get; private set; }
            public ActorAddress Address { get; private set; }

            public TransportAndAddressDependencyActor(ITransport transport, ActorAddress address) {
                Transport = transport;
                Address = address;
            }
        }

        public class MockBackstage : IBackstage {
            private object _actor;
            public void AddActor<TActor>(TActor actor, ActorAddress address, int parallelism) {
                _actor = actor;
            }

            public ITransport CreateTransport(ActorAddress address) {
                return new Mock<ITransport>().Object;
            }

            public T Get<T>() {
                return (T)_actor;
            }
        }
    }
}
