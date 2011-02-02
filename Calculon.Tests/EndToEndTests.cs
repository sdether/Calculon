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
using System.Linq.Expressions;
using System.Threading;
using Droog.Calculon.Framework;
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
            _stage.AddActor<Origin>().WithId("origin").Build((t, m) => _origin = new Origin(t, m));
        }

        [TearDown]
        public void Teardown() {
            _stage.Dispose();
        }

        [Test]
        public void Fire_and_forget() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            _origin.FireAndForget("foo", "bar");
            Assert.IsTrue(actor.Trigger.WaitOne(10.Seconds()));
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Fire_and_forget_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            _origin.FireAndForget("foo");
            Assert.IsTrue(actor.Trigger.WaitOne(10.Seconds()));
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        [Test]
        public void Send_and_block() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            _origin.SendAndBlock("foo", "bar");
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Send_and_block_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            _origin.SendAndBlock("foo");
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        [Test]
        public void Send_and_receive_message() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            var msg = _origin.SendAndReceive("foo", "bar");
            Assert.AreEqual("bar", msg);
            Assert.AreEqual("bar", actor.Msg);
            Assert.IsNull(actorx.Msg);
        }

        [Test]
        public void Send_and_receive_with_meta() {
            Recipient actor = null, actorx = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.AddActor<IRecipient>().WithId("baz").Build(t => actorx = new Recipient());
            var from = _origin.SendAndReceive("foo");
            Assert.AreEqual(_origin.Address.Id, from);
            Assert.AreEqual(_origin.Address.Id, actor.From);
            Assert.IsNull(actorx.From);
        }

        [Test]
        public void Sending_message_to_nonexistent_actor_throws_in_result() {
            var r = _stage.Transport.For<IRecipient>("foo").SendAndReceive(x => x.Echo("hello")).Block();
            Assert.IsTrue(r.HasException);
            Assert.AreEqual(typeof(NoSuchRecipientException), r.Exception.GetType());
        }

        [Test]
        public void Can_spawn_actor_via_messaging() {
            _stage.Transport
                .SendAndReceive<IDirector>(x => x.AddActor<IRecipient>().WithId("foo").Build(t => new Recipient()))
                .Wait();
            var r = _stage.Transport.For<IRecipient>("foo").SendAndReceive(x => x.Echo("hello")).Block();
            Assert.IsFalse(r.HasException);
            Assert.AreEqual("hello", r.Value);
        }

        [Test]
        public void Can_shutdown_actor_via_messaging() {
            Recipient actor = null;
            _stage.AddActor<IRecipient>().WithId("foo").Build(t => actor = new Recipient());
            _stage.Transport.SendAndReceive<IDirector>((x, m) => x.RetireActor(ActorAddress.Create<IRecipient>("foo"), m)).Wait();
            var r = _stage.Transport.For<IRecipient>("foo").SendAndReceive(x => x.Echo("hello")).Block();
            Assert.IsTrue(r.HasException);
            Assert.AreEqual(typeof(NoSuchRecipientException), r.Exception.GetType());
            Assert.IsTrue(actor.IsDisposed);
        }

        [Test]
        public void Can_spawn_actors_on_demand() {
            _stage.AddActor<RecipientFactory>().Build();
            var r = _stage.Transport.For<IRecipient>("foo").SendAndReceive(x => x.Echo("hello")).Block();
            Assert.IsFalse(r.HasException);
            Assert.AreEqual("hello", r.Value);
        }
    }


    public class Origin {
        private readonly ITransport _transport;
        private readonly ActorAddress _address;

        public Origin(ITransport transport, ActorAddress address) {
            _transport = transport;
            _address = address;
        }

        public ActorAddress Address { get { return _address; } }

        public void FireAndForget(string id, string msg) {
            _transport.For<IRecipient>(id).Send(x => x.Receive(msg));
        }

        public void FireAndForget(string id) {
            _transport.For<IRecipient>(id).Send((x, m) => x.Receive(m));
        }

        public void SendAndBlock(string id, string msg) {
            _transport.For<IRecipient>(id).SendAndReceive(x => x.Receive(msg)).Wait();
        }

        public void SendAndBlock(string id) {
            _transport.For<IRecipient>(id).SendAndReceive((x, m) => x.Receive(m)).Wait();
        }

        public string SendAndReceive(string id, string echo) {
            return _transport.For<IRecipient>(id).SendAndReceive(x => x.Echo(echo)).Wait();
        }

        public string SendAndReceive(string id) {
            return _transport.For<IRecipient>(id).SendAndReceive((x, m) => x.Echo(m)).Wait();
        }
    }

    public class RecipientFactory : IPatternMatchingActor {
        private readonly ITransport _transport;

        public RecipientFactory(ITransport transport) {
            _transport = transport;
        }

        public Expression<Func<MessageMeta, bool>> AcceptCriteria {
            get { return m => typeof(IRecipient).IsAssignableFrom(m.Recipient.Type); }
        }

        public void Receive(IMessage message) {
            _transport
                .SendAndReceive<IDirector>(x => x.AddActor<IRecipient>().WithId(message.Meta.Recipient.Id).Build(t => new Recipient()))
                .Wait();
            _transport.Send(message);
        }
    }

    public interface IRecipient {
        void Receive(string msg);
        void Receive(MessageMeta meta);
        string Echo(string msg);
        string Echo(MessageMeta meta);
    }

    public class Recipient : IRecipient, IDisposable {

        public string Msg;
        public string From;
        public ManualResetEvent Trigger = new ManualResetEvent(false);
        public bool IsDisposed;

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

        public void Dispose() {
            IsDisposed = true;
        }
    }
}
