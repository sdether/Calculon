using System;
using System.Linq;
using System.Reflection;
using Droog.Calculon.Messages;
using MindTouch.Collections;

namespace Droog.Calculon.Framework {
    public class PatternMatchingMailbox : IMailbox {
        private readonly ActorAddress _address;
        private readonly IPatternMatchingActor _recipient;
        private readonly ProcessingQueue<IMessage> _queue;
        private readonly Func<MessageMeta, bool> _criteria;
        private readonly MethodInfo _receiveMethod;
        private bool _isAlive = true;

        public PatternMatchingMailbox(ActorAddress address, IPatternMatchingActor recipient, int parallelism) {
            _address = address;
            _recipient = recipient;
            _queue = new ProcessingQueue<IMessage>(Dispatch, parallelism);
            _criteria = _recipient.AcceptCriteria.Compile();

            // TODO: this needs to make sure it finds at least one, should accept any method with parameters assignable from IMessage
            // and should build a dynamic method for faster invocation.
            _receiveMethod = (from method in recipient.GetType().GetMethods()
                              where method.Name == "Receive"
                              let parameters = method.GetParameters()
                              where parameters.Length == 1 && typeof(IMessage) == parameters[0].ParameterType
                              select method).First();
        }

        private void Dispatch(IMessage message) {
            _receiveMethod.Invoke(_recipient, new object[] { message });
        }

        public ActorAddress Recipient { get { return _address; } }

        public bool CanAccept(MessageMeta meta) {
            return _criteria(meta);
        }

        public bool Accept(IMessage message) {
            if(message is ShutdownMessage) {
                var disposable = _recipient as IDisposable;
                if(disposable != null) {
                    disposable.Dispose();
                }
                _isAlive = false;
                return true;
            }
            if(_criteria(message.Meta)) {
                _queue.TryEnqueue(message);
                return true;
            }
            return false;
        }

        public bool IsAlive { get { return _isAlive; } }

        public void Dispose() {
            _isAlive = false;
        }
    }
}