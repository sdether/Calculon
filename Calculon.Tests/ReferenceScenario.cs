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
using NUnit.Framework;

namespace Droog.Calculon.Tests {

    [TestFixture]
    public class ReferenceScenario {
        private Director _director;
        private Origin _origin;

        [SetUp]
        public void Setup() {
            _director = DirectorBuilder.Build();
            _origin = _director.Create<Origin>("origin");
        }

        [TearDown]
        public void Teardown() {
            _director.Dispose();
        }

        [Test]
        public void Meta_is_injected() {
            var o2 = _director.Create<Origin>("again");
            Assert.AreEqual("again", o2.Meta.From);
            Assert.AreEqual(typeof(Origin), o2.Meta.SenderType);
        }

        [Test]
        public void Echo() {
            var actor = _director.Create<TestActor>("foo");
            var actorx = _director.Create<TestActor>("baz");
            _origin.Echo("foo", "bar");
            Assert.AreEqual("bar", actor.echo);
            Assert.IsNull(actorx.echo);
        }

        [Test]
        public void EchoFrom() {
            var actor = _director.Create<TestActor>("foo");
            var actorx = _director.Create<TestActor>("baz");
            _origin.EchoFrom("foo");
            Assert.AreEqual(_origin.Meta.From, actor.echo);
        }
    }

    public class Origin {
        private readonly IExpressionTransport _transport;
        private readonly MessageMeta _meta;

        public Origin(IExpressionTransport transport, MessageMeta meta) {
            _transport = transport;
            _meta = meta;
        }

        public MessageMeta Meta { get { return _meta; } }

        public string Echo(string id, string echo) {
            return _transport.For<TestActor>(id).SendAndReceive(x => x.Echo(echo)).Wait();
        }

        public string EchoFrom(string id) {
            return _transport.For<TestActor>(id).SendAndReceive((x, m) => x.EchoFrom(m)).Wait();
        }
    }

    public class TestActor {

        public string echo;
        public string from;
        public string Echo(string msg) {
            return echo = msg;
        }
        public string EchoFrom(MessageMeta meta) {
            return from = meta.From;
        }
    }
}
