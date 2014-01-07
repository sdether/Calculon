namespace Droog.Calculon.Backstage.Messages {
    public class SuspendMessage : Message {
        public SuspendMessage(ActorRef sender, ActorRef receiver) : base(SystemMessageNames.Suspend, sender, receiver) { }
    }
}