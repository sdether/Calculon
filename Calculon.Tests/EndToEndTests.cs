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
using System.Threading;
using NUnit.Framework;
using MindTouch.Extensions.Time;

namespace Droog.Calculon.Tests {

    [TestFixture]
    public class EndToEndTests {
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
        public void Fire_and_forget() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            _origin.FireAndForget("foo", "bar");
            Assert.IsTrue(actor.Trigger.WaitOne(10.Seconds()));
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Fire_and_forget_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            _origin.FireAndForget("foo");
            Assert.IsTrue(actor.Trigger.WaitOne(10.Seconds()));
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        [Test]
        public void Send_and_block() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            _origin.SendAndBlock("foo", "bar");
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Send_and_block_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            _origin.SendAndBlock("foo");
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        [Test]
        public void Send_and_receive_message() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            var msg = _origin.SendAndReceive("foo","bar");
            Assert.AreEqual("bar", msg);
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Send_and_receive_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<Recipient>().WithId("foo").BuildWithExpressionTransport(t => actor = new Recipient());
            _stage.AddActor<Recipient>().WithId("baz").BuildWithExpressionTransport(t => actorx = new Recipient());
            var from = _origin.SendAndReceive("foo");
            Assert.AreEqual(_origin.Address.Id, from);
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        public class Origin {
            private readonly IExpressionTransport _transport;
            private readonly ActorAddress _address;

            public Origin(IExpressionTransport transport, ActorAddress address) {
                _transport = transport;
                _address = address;
            }

            public ActorAddress Address { get { return _address; } }

            public void FireAndForget(string id, string msg) {
                _transport.For<Recipient>(id).Send(x => x.Receive(msg));
            }

            public void FireAndForget(string id) {
                _transport.For<Recipient>(id).Send((x, m) => x.Receive(m));
            }

            public void SendAndBlock(string id, string msg) {
                _transport.For<Recipient>(id).SendAndReceive(x => x.Receive(msg)).Wait();
            }

            public void SendAndBlock(string id) {
                _transport.For<Recipient>(id).SendAndReceive((x, m) => x.Receive(m)).Wait();
            }

            public string SendAndReceive(string id, string echo) {
                return _transport.For<Recipient>(id).SendAndReceive(x => x.Echo(echo)).Wait();
            }

            public string SendAndReceive(string id) {
                return _transport.For<Recipient>(id).SendAndReceive((x, m) => x.Echo(m)).Wait();
            }
        }

        public class Recipient {

            public string Msg;
            public string From;
            public ManualResetEvent Trigger = new ManualResetEvent(false);

            public void Receive(string msg) {
                Msg = msg;
                Trigger.Set();
            }

            public void Receive(MessageMeta meta) {
                From = meta.Sender.Id;
                Trigger.Set();
            }

            public string Echo(string msg) {
                return Msg = msg;
            }
            public string Echo(MessageMeta meta) {
                return From = meta.Sender.Id;
            }
        }
    }
}
