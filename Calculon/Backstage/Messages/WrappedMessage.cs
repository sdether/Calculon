namespace Droog.Calculon.Backstage.Messages {
    public class WrappedMessage : Message {

        public readonly Message Wrapped;
        public WrappedMessage(ActorRef sender, ActorRef receiver, Message wrapped)
            : base(wrapped.Name, sender, receiver) {
            Wrapped = wrapped;
        }

        public override Message Unwrap() {
            return Wrapped.Unwrap();
        }
    }
}