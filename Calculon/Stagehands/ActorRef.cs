using System;

namespace Droog.Calculon {
    public class ActorRef : IActorRef {
        private readonly string _name;

        public ActorRef(string name) {
            _name = name;
        }

        public string Name { get { return _name; } }
    }
}