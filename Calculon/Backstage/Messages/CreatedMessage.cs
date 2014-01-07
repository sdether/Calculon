namespace Droog.Calculon.Backstage.Messages {
    public class CreatedMessage : Message {
        public CreatedMessage(ActorRef child, ActorRef parent) : base(SystemMessageNames.Created, child, parent) { }
    }
}