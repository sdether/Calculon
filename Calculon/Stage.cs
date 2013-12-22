using System;
using Droog.Calculon.Backstage;

namespace Droog.Calculon {
    public class Stage : IStage {

        private readonly IBackstage _backstage;

        public Stage() {
            _backstage = new Backstage.Backstage();
        }

        public ActorProxy<TActor> Find<TActor>(ActorRef actorRef) where TActor : class {
            return _backstage.Find<TActor>(_backstage.RootRef, actorRef);
        }

        public ActorProxy<TActor> Create<TActor>(string name = null, Func<TActor> builder = null) where TActor : class {
            return _backstage.Create<TActor>(_backstage.RootRef, _backstage.RootRef, name: name, builder: builder);
        }
    }
}