using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox {

    public interface IAdder {
        Task<int> Add(int a, int b);
        void FireAndForget(string msg);
    }

    public interface IStage {
        IActorRef Find(string address);
        IActorRef GetRef(object actor);
        T Create<T>(string address) where T : class;
        T Get<T>(string address) where T : class;
        T Get<T>(IActorRef actorRef) where T : class;
        Task<TResult> Send<TActor, TResult>(string address, Expression<Func<TActor, Task<TResult>>> expression);
    }

    public interface IScene : IStage {
        Task<T> Return<T>(T value);
        Task<T> ReturnLater<T>();
        void Shutdown(IActorRef actorRef);
        void Shutdown(object actor);

    }

    public interface IActorRef {
        string Name { get; }
        string Address { get; }
        Type Type { get; }
        void Shutdown();
    }

    public interface IActor {
        IScene Scene { get; set; }
        IActorRef Parent { get; set; }
        IActorRef Sender { get; set; }
        void Start();
        void Shutdown();
    }

    public interface IActor<T> : IActor {
        T Self { get; set; }
    }


    public class Coordinator : IActor<Coordinator> {

        public void TheLongWay() {
            var t = Scene.Send<IAdder, int>("adder", addr => addr.Add(1, 2));
        }
        public Task<int> Add(int a, int b, int c) {
            var addr = Scene.Create<IAdder>("adder");
            var x = addr.Add(a, b)
                .ContinueWith(t1 => {
                    addr.Add(t1.Result, c)
                        .ContinueWith(t2 => {
                            Scene.Return(t2.Result);
                            Scene.Shutdown(addr);
                        });
                });
            return Scene.ReturnLater<int>();
        }

        public void Fire() {
            Scene.Get<IAdder>("adder")
                .FireAndForget("foo");
        }

        public IScene Scene { get; set; }
        public IActorRef Parent { get; set; }
        public IActorRef Sender { get; set; }
        public Coordinator Self { get; set; }

        public void Start() {
            Self.Add(1, 2, 3);
        }

        public void Shutdown() { }
    }

    public class Stage : IStage {

        private class Actor {
            public IActorRef Ref;
            public Mailbox Mailbox;
            public IActor Instance;
            public object Proxy;
        }

        Dictionary<string, Actor> _actors = new Dictionary<string, Actor>();

        public IActorRef Find(string address) {
            Actor actor;
            return !_actors.TryGetValue(address, out actor)
                ? new ActorRef(this, address, typeof(Void))
                : actor.Ref;
        }

        public IActorRef GetRef(object actor) {
            throw new NotImplementedException();
        }

        public T Create<T>(string address) where T : class {
            var actor = _actors[address] = new Actor {
                Ref = new ActorRef(this, address, typeof(T)),
                Instance = Activator.CreateInstance<T>() as IActor,
                Mailbox = new Mailbox()
            };
            return actor.Proxy as T;
        }

        public T Get<T>(string address) {
            throw new NotImplementedException();
        }

        public T Get<T>(IActorRef actorRef) {
            throw new NotImplementedException();
        }

        public Task<TResult> Send<TActor, TResult>(string address, Expression<Func<TActor, Task<TResult>>> expression) {
            throw new NotImplementedException();
        }
    }

    public class Mailbox { }

    public abstract class MailboxMsg<TActor,TResult> {
        public abstract Task<TResult> Execute(TActor actor);
    }

    public class ExpressionMsg<TActor, TResult> : MailboxMsg<TActor,TResult> {
        private readonly Expression<Func<TActor, Task<TResult>>> _expression;

        public ExpressionMsg(Expression<Func<TActor, Task<TResult>>> expression) {
            _expression = expression;
        }

        public override Task<TResult> Execute(TActor actor) {
            return _expression.Compile()(actor);
        }
    }

    public class ResultMsg<TActor,TResult> : MailboxMsg<TActor,TResult> {

        public class 

        public override Task<TResult> Execute(TActor actor) {
            throw new NotImplementedException();
        }
    }

    public class ActorRef : IActorRef {
        private readonly Stage _stage;
        private readonly string _address;
        private readonly Type _type;

        public ActorRef(Stage stage, string address, Type type) {
            _stage = stage;
            _address = address;
            _type = type;
        }

        public string Name { get; private set; }
        public string Address { get; private set; }
        public Type Type { get; private set; }
        public void Shutdown() {
            throw new NotImplementedException();
        }
    }
}
