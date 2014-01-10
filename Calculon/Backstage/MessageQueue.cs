using System;
using System.Collections.Generic;
using System.Linq;
using Droog.Calculon.Backstage.Messages;

namespace Droog.Calculon.Backstage {
    public class MessageQueue {
        public enum QueueItemStatus {
            Queued,
            InFlight,
            Completed,
            Aborted
        }

        public class QueueStats {
            public readonly int Size;
            public readonly int Pending;

            public QueueStats(int count, int index) {
                Size = count;
                Pending = Size - (index + 1);
            }
        }

        private class InternalQueueItem : QueueItem {
            public InternalQueueItem(Message message) : base(message) { }

            public void StartProcessing() {
                if(_status == QueueItemStatus.InFlight) {
                    throw new InvalidOperationException(string.Format("message '{0}' is already in flight", Message.Id));
                }
                _status = QueueItemStatus.InFlight;
            }
        }

        public class QueueItem {
            public readonly Message Message;
            protected QueueItemStatus _status = QueueItemStatus.Queued;

            public QueueItem(Message message) {
                Message = message;
            }

            public QueueItemStatus Status { get { return _status; } }

            public void Complete() {
                _status = QueueItemStatus.Completed;
            }
            public void Abort() {
                _status = QueueItemStatus.Aborted;
            }
        }

        private readonly List<InternalQueueItem> _queue = new List<InternalQueueItem>();
        private readonly int _collectionThreshold;
        private int _out = -1;

        public MessageQueue(int collectionThreshold = 1000) {
            _collectionThreshold = collectionThreshold;
        }

        public void Enqueue(Message message) {
            lock(_queue) {
                if(_out >= _collectionThreshold) {
                    Compact();
                }
                _queue.Add(new InternalQueueItem(message));
            }
        }

        public void Compact() {
            lock(_queue) {
                if(_out == -1) {
                    return;
                }
                var unfinishedIndex = _queue.FindIndex(0, x => x.Status == QueueItemStatus.InFlight || x.Status == QueueItemStatus.Queued);
                if(unfinishedIndex == -1) {
                    _queue.Clear();
                    _out = -1;
                } else if(unfinishedIndex == 0) {
                    return;
                } else {
                    _queue.RemoveRange(0, unfinishedIndex);
                    _out -= unfinishedIndex;
                }
            }
        }

        public QueueItem Dequeue() {
            lock(_queue) {
                var size = _queue.Count;
                if(size == 0 || _out == size - 1) {
                    return null;
                }
                _out++;
                var item = _queue[_out];
                item.StartProcessing();
                return item;
            }
        }

        public void ResetPending() {
            lock(_queue) {
                Compact();
                for(var i = 0; i < _queue.Count; i++) {
                    if(_queue[i].Status == QueueItemStatus.Queued) {
                        break;
                    }
                    _queue[i] = new InternalQueueItem(_queue[i].Message);
                }
            }
        }

        public QueueStats GetStats() {
            return new QueueStats(_queue.Count, _out);
        }
    }
}