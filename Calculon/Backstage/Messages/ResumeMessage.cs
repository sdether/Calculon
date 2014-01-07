namespace Droog.Calculon.Backstage.Messages {
    public class ResumeMessage : Message {
        public ResumeMessage(ActorRef sender, ActorRef receiver) : base(SystemMessageNames.Resume, sender, receiver) { }
    }
}