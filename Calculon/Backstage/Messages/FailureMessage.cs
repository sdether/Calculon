using System;

namespace Droog.Calculon.Backstage.Messages {
    public class FailureMessage : Message {
        public readonly Message Cause;
        private readonly bool _fatal;
        public readonly Exception Exception;

        public FailureMessage(Exception exception, Message cause, bool fatal = false)
            : base(SystemMessageNames.Fault, cause.Receiver, cause.Sender, cause.Id) {
            Exception = exception;
            Cause = cause;
            _fatal = fatal;
        }

        public override bool IsFatalFault { get { return _fatal; } }
    }
}