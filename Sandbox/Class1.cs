using System.Threading.Tasks;
using Droog.Calculon;

namespace Sandbox {

    public interface IAdder {
        Task<int> Add(int a, int b);
        void FireAndForget(string msg);
    }


    public class Coordinator : IActor {

        public Task<int> Add(int a, int b, int c) {
            var addr = Scene.Create<IAdder>("adder");
            var completion = new TaskCompletionSource<int>();
            var x = addr.Add(a, b)
                .ContinueWith(t1 => {
                    addr.Add(t1.Result, c)
                        .ContinueWith(t2 => {
                            completion.SetResult(t2.Result+c);
                            Scene.Shutdown(addr);
                        });
                });
            return completion.Task;
        }

        public void Fire() {
            Scene.Get<IAdder>("adder")
                .FireAndForget("foo");
        }

        public void Foo() {
            var t = Scene.Get<IAdder>("abc")
                .Add(1, 2);
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

   
}
