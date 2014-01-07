namespace Droog.Calculon.Backstage.Messages {
    public class ResponseMessage : Message {
        public readonly Message Cause;
        public readonly object Response;

        public ResponseMessage(object response, Message cause)
            : base(SystemMessageNames.Response, cause.Receiver, cause.Sender, cause.Id) {
            Response = response;
            Cause = cause;
        }
    }
}