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
        private Stage _stage;
        private Origin _origin;

        [SetUp]
        public void Setup() {
            _stage = new Stage();
            _stage.AddActor<Origin>().WithId("origin").BuildWithExpressionTransport((t, m) => _origin = new Origin(t, m));
        }

        [TearDown]
        public void Teardown() {
            _stage.Dispose();
        }

        [Test]
        public void Echo() {
            TestActor actor = null, actorx = null;
            _stage.AddActor<TestActor>().WithId("foo").BuildWithExpressionTransport(t => actor = new TestActor());
            _stage.AddActor<TestActor>().WithId("baz").BuildWithExpressionTransport(t => actorx = new TestActor());
            Assert.AreEqual("bar", actor.echo);
            Assert.IsNull(actorx.echo);
        }

        [Test]
        public void EchoFrom() {
            TestActor actor = null, actorx = null;
            _stage.AddActor<TestActor>().WithId("foo").BuildWithExpressionTransport(t => actor = new TestActor());
            _stage.AddActor<TestActor>().WithId("baz").BuildWithExpressionTransport(t => actorx = new TestActor());
            _origin.EchoFrom("foo");
            Assert.AreEqual(_origin.Meta.From, actor.echo);
            Assert.IsNull(actorx.echo);
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
}
