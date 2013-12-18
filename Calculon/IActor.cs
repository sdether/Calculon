namespace Droog.Calculon {
    public interface IActor<T> : IActor {
        T Self { get; set; }
    }

    public interface IActor {
        IScene Scene { get; set; }
        IActorRef Parent { get; set; }
        IActorRef Sender { get; set; }
        void Start();
        void Shutdown();
    }
}