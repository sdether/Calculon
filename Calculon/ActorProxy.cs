namespace Droog.Calculon {
    public class ActorProxy<TActor> {
        public readonly ActorRef Ref;
        public readonly TActor Proxy;

        public ActorProxy(ActorRef actorRef, TActor proxy) {
            Ref = actorRef;
            Proxy = proxy;
        }
    }
}