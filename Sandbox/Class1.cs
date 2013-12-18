using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Droog.Calculon;

namespace Sandbox {

    public interface IAdder {
        Task<int> Add(int a, int b);
        void FireAndForget(string msg);
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
