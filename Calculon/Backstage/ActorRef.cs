using System;

namespace Droog.Calculon.Backstage {
    public class ActorRef : IActorRef {
        private readonly string _name;
        private readonly Type _type;

        public ActorRef(string name, Type type) {
            _name = name;
            _type = type;
        }

        public string Name { get { return _name; } }
        public Type Type { get { return _type; } }
    }
}