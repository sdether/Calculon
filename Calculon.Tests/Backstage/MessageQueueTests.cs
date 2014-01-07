using System;
using System.Collections.Generic;
using Droog.Calculon.Backstage;
using Droog.Calculon.Backstage.Messages;
using NUnit.Framework;

namespace Droog.Calculon.Tests.Backstage {

    [TestFixture]
    public class MessageQueueTests {

        [Test]
        public void Returns_null_if_no_item_in_queue() {
            var queue = new MessageQueue();
            Assert.IsNull(queue.Dequeue());
        }

        [Test]
        public void Returns_null_when_no_item_left_in_queued_state() {
            var queue = new MessageQueue();
            queue.Enqueue(new TestMessage());
            queue.Dequeue();
            Assert.IsNull(queue.Dequeue());
        }

        [Test]
        public void Queue_is_FIFO_1() {
            var queue = new MessageQueue();
            var messages = new List<Message>();
            for(var i = 0; i < 10; i++) {
                var msg = new TestMessage();
                messages.Add(msg);
                queue.Enqueue(msg);
            }
            for(var i = 0; i < 10; i++) {
                var item = queue.Dequeue();
                Assert.IsNotNull(item);
                Assert.AreSame(messages[i], item.Message);
            }
        }

        [Test]
        public void Queue_is_FIFO_2() {
            var queue = new MessageQueue();
            var previous = new TestMessage();
            queue.Enqueue(previous);
            for(var i = 0; i < 10; i++) {
                var msg = new TestMessage();
                queue.Enqueue(msg);
                var item = queue.Dequeue();
                Assert.IsNotNull(item);
                Assert.AreSame(previous, item.Message);
                previous = msg;
            }
        }

        [Test]
        public void Compact_clears_queue_with_nothing_pending_left() {
            var queue = new MessageQueue();
            for(var i = 0; i < 10; i++) {
                var msg = new TestMessage();
                queue.Enqueue(msg);
            }
            var stats = queue.GetStats();
            Assert.AreEqual(10, stats.Size, "wrong size after insert");
            Assert.AreEqual(10, stats.Pending, "wrong pending after insert");
            for(var i = 0; i < 10; i++) {
                var item = queue.Dequeue();
                item.Complete();
            }
            stats = queue.GetStats();
            Assert.AreEqual(10, stats.Size, "wrong size after dequeue");
            Assert.AreEqual(0, stats.Pending, "wrong pending after dequeue");
            queue.Compact();
            stats = queue.GetStats();
            Assert.AreEqual(0, stats.Size, "wrong size after compact");
            Assert.AreEqual(0, stats.Pending, "wrong pending after compact");
        }

        [Test]
        public void Compact_cleans_up_queue_with_mixed_pending_items() {
            var queue = new MessageQueue();
            var messages = new Queue<Message>();
            for(var i = 0; i < 10; i++) {
                var msg = new TestMessage();
                messages.Enqueue(msg);
                queue.Enqueue(msg);
            }
            var stats = queue.GetStats();
            Assert.AreEqual(10, stats.Size, "wrong size after insert");
            Assert.AreEqual(10, stats.Pending, "wrong pending after insert");
            for(var i = 0; i < 5; i++) {
                var item = queue.Dequeue();
                item.Complete();
                var msg = messages.Dequeue();
                Assert.AreSame(msg, item.Message, string.Format("wrong message at index {0}", i));
            }
            stats = queue.GetStats();
            Assert.AreEqual(10, stats.Size, "wrong size after dequeue");
            Assert.AreEqual(5, stats.Pending, "wrong pending after dequeue");
            queue.Compact();
            stats = queue.GetStats();
            Assert.AreEqual(5, stats.Size, "wrong size after compact");
            Assert.AreEqual(5, stats.Pending, "wrong pending after compact");
            for(var i = 0; i < 5; i++) {
                var item = queue.Dequeue();
                var msg = messages.Dequeue();
                Assert.AreSame(msg, item.Message, string.Format("wrong message at index {0}", i + 5));
            }
            stats = queue.GetStats();
            Assert.AreEqual(5, stats.Size, "wrong size after compact");
            Assert.AreEqual(0, stats.Pending, "wrong pending after compact");
        }

        private class TestMessage : Message {
            public TestMessage()
                : base("test", null, null, Guid.NewGuid()) { }
        }
    }
}
