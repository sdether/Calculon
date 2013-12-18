namespace Droog.Calculon {

    public interface IActor {
        IScene Scene { get; set; }
        IActorRef Parent { get; set; }
        IActorRef Sender { get; set; }
        void Shutdown();
    }

    public abstract class AActor: IActor {
        public IScene Scene { get; set; }
        public IActorRef Parent { get; set; }
        public IActorRef Sender { get; set; }
        public void Shutdown() {
            throw new System.NotImplementedException();
        }
    }
}