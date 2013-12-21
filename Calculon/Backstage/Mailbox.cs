using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class Mailbox<TActor> : IMailbox<TActor> where TActor : class {

        private static readonly ProxyGenerator PROXY_GENERATOR = new ProxyGenerator();
        private const int MAX_PROCESSNG_SIZE = 100;

        private readonly ActorRef _parent;
        private readonly IBackstage _backstage;
        private readonly Func<TActor> _builder;
        private readonly ActorRef _actorRef;
        private readonly Queue<Message> _queue = new Queue<Message>();
        private readonly Dictionary<Guid, MessageResponse> _pendingResponses = new Dictionary<Guid, MessageResponse>();
        private readonly Dictionary<string, Action<Message, TActor>> _handlers = new Dictionary<string, Action<Message, TActor>>();
        private readonly MethodInfo _buildTaskofTHandler;
        private readonly TActor _instance;
        private bool _processing = false;

        public Mailbox(ActorRef parent, string name, IBackstage backstage, Func<TActor> builder) {
            _parent = parent;
            _backstage = backstage;
            _builder = builder;
            _actorRef = (parent ?? ActorRef.Parse("/")).At(name);
            _instance = _builder();
            _buildTaskofTHandler = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "BuildTaskHandler" && x.IsGenericMethod);
            var actor = _instance as IActor;
            actor.Context = new ActorContext(_backstage, Ref, _parent);
            PopulatedMessageHandlers();
        }

        private void PopulatedMessageHandlers() {
            foreach(var methodInfo in typeof(TActor).GetMethods()) {
                var returnType = methodInfo.ReturnType;
                Action<Message, TActor> handler;
                if(returnType == typeof(void)) {
                    handler = BuildVoidHandler(methodInfo);
                } else if(returnType == typeof(Task)) {
                    handler = BuildTaskHandler(methodInfo);
                } else if(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                    var taskType = returnType.GetGenericArguments().First();
                    var generic = _buildTaskofTHandler.MakeGenericMethod(taskType);
                    handler = generic.Invoke(this, new object[] { methodInfo }) as Action<Message, TActor>;
                } else {
                    continue;
                }
                var signature = Message.GetContractFromMethodInfo(methodInfo);
                _handlers[signature] = handler;
            }
            _handlers[MessageType.Response.ToString()] = BuildResponseHandler();
            _handlers[MessageType.Fault.ToString()] = BuildFaultHandler();
        }

        private Action<Message, TActor> BuildFaultHandler() {
            return (msg, actor) => {
                var response = GetPendingResponse(msg.Id);
                response.Fault(msg.Args[0] as Exception);
            };
        }

        private MessageResponse GetPendingResponse(Guid id) {
            lock(_pendingResponses) {
                var response = _pendingResponses[id];
                _pendingResponses.Remove(id);
                return response;
            }
        }

        private Action<Message, TActor> BuildResponseHandler() {
            return (msg, actor) => {
                var response = GetPendingResponse(msg.Id);
                response.Complete(msg.Args[0]);
            };
        }

        private Action<Message, TActor> BuildTaskHandler(MethodInfo methodInfo) {
            return (msg, actor) => {
                var mailbox = _backstage.GetMailbox(msg.Sender);
                try {
                    (methodInfo.Invoke(actor, msg.Args) as Task).ContinueWith(t => mailbox.Enqueue(
                        t.IsFaulted
                            ? new Message(msg.Id, Ref, msg.Contract, MessageType.Fault, null, new object[] { t.Exception })
                            : new Message(msg.Id, Ref, msg.Contract, MessageType.Response, null, new object[] { null })
                        )
                    );
                } catch(TargetInvocationException e) {
                    mailbox.Enqueue(new Message(msg.Id, Ref, msg.Contract, MessageType.Fault, null, new[] { e.InnerException }));
                }
            };
        }

        private Action<Message, TActor> BuildTaskHandler<TResult>(MethodInfo methodInfo) {
            return (msg, actor) => {
                var mailbox = _backstage.GetMailbox(msg.Sender);
                try {
                    (methodInfo.Invoke(actor, msg.Args) as Task<TResult>).ContinueWith(t => mailbox.Enqueue(
                        t.IsFaulted
                            ? new Message(msg.Id, Ref, msg.Contract, MessageType.Fault, typeof(TResult), new object[] { t.Exception })
                            : new Message(msg.Id, Ref, msg.Contract, MessageType.Response, typeof(TResult), new object[] { t.Result })
                        )
                    );
                } catch(TargetInvocationException e) {
                    mailbox.Enqueue(new Message(msg.Id, Ref, msg.Contract, MessageType.Fault, typeof(TResult), new[] { e.InnerException }));
                }
            };
        }

        private Action<Message, TActor> BuildVoidHandler(MethodInfo methodInfo) {
            return (msg, actor) => methodInfo.Invoke(actor, msg.Args);
        }

        public ActorRef Ref { get { return _actorRef; } }

        public bool IsMailboxFor<TActor1>() {
            return typeof(TActor).IsAssignableFrom(typeof(TActor1));
        }

        public IMailbox<TActor1> As<TActor1>() where TActor1 : class {
            return this as IMailbox<TActor1>;
        }

        public MessageResponse CreatePendingResponse(Type type) {
            var response = new MessageResponse(type);
            lock(_pendingResponses) {
                _pendingResponses.Add(response.Id, response);
            }
            return response;
        }

        public TActor BuildProxy(IMailbox sender) {
            return PROXY_GENERATOR.CreateInterfaceProxyWithoutTarget<TActor>(new ActorProxyInterceptor<TActor>(sender, this));
        }

        public void Enqueue(Message msg) {
            lock(_queue) {
                _queue.Enqueue(msg);
                if(_processing) {
                    return;
                }
                _processing = true;
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void Dequeue(object state) {
            List<Message> manyMsg = null;
            Message singleMsg = null;
            var size = 0;
            lock(_queue) {
                size = _queue.Count;
                switch(size) {
                case 0:
                    _processing = false;
                    return;
                case 1:
                    singleMsg = _queue.Dequeue();
                    break;
                default:
                    manyMsg = new List<Message>();
                    for(var i = 0; i < Math.Min(size, MAX_PROCESSNG_SIZE); i++) {
                        manyMsg.Add(_queue.Dequeue());
                    }
                    break;
                }
            }
            if(singleMsg != null) {
                ExecuteMessage(singleMsg);
            } else {
                foreach(var msg in manyMsg) {
                    ExecuteMessage(msg);
                }
                if(size > 10) {
                    ThreadPool.QueueUserWorkItem(Dequeue);
                    return;
                }
            }
            lock(_queue) {
                if(!_queue.Any()) {
                    _processing = false;
                    return;
                }
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void ExecuteMessage(Message msg) {
            var actor = _instance as IActor;
            actor.Sender = msg.Sender;
            _handlers[msg.Signature](msg, _instance);
        }
    }
}