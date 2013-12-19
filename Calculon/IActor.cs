namespace Droog.Calculon {

    public interface IActor {
        IScene Scene { get; set; }
        ActorRef Self { get; set; }
        ActorRef Parent { get; set; }
        void Shutdown();
    }

    public abstract class AActor: IActor {
        public IScene Scene { get; set; }
        public ActorRef Self { get; set; }
        public ActorRef Parent { get; set; }
        public ActorRef Sender { get; set; }
        public void Shutdown() {
            throw new System.NotImplementedException();
        }
    }
}