using System;

namespace Droog.Calculon {
    public class ActorRef {
        public readonly string Name;
        public readonly Type Type;

        public ActorRef(string name, Type type) {
            Name = name;
            Type = type;
        }
    }
}