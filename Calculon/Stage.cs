using System;
using Droog.Calculon.Backstage;

namespace Droog.Calculon {
    public class Stage : IStage {

        private readonly IBackstage _backstage;

        public Stage() {
            _backstage = new Backstage.Backstage();
        }

        public TActor CreateAndGet<TActor>(string name = null) where TActor : class {
            return _backstage.CreateAndGet<TActor>(_backstage.RootRef, _backstage.RootRef, name);
        }

        public TActor Get<TActor>(string name) where TActor : class {
            return Get<TActor>(new ActorRef(name, typeof(TActor)));
        }

        public TActor Get<TActor>(ActorRef actorRef) where TActor : class {
            return _backstage.Get<TActor>(_backstage.RootRef, actorRef);
        }


    }
}