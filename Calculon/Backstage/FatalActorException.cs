using System;

namespace Droog.Calculon.Backstage {
    public class FatalActorException : Exception {
        public FatalActorException(Exception exception) : base(null, exception) { }
    }
}