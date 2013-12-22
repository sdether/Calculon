using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Droog.Calculon.Backstage {
    public interface IActorBuilder {
        Func<TActor> GetBuilder<TActor>();
    }

    public class ActorBuilder : IActorBuilder {

        private readonly Dictionary<string, object> _builders = new Dictionary<string, object>();
        private readonly IEnumerable<Type> _types;
 
        public ActorBuilder() {
            var actorType = typeof(IActor);
            _types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(t => t.IsClass && actorType.IsAssignableFrom(t)).ToArray();
        }

        public Func<TActor> GetBuilder<TActor>() {
            object builderObj;
            Func<TActor> builder;
            var type = typeof(TActor);
            var typeName = type.Name;
            if(!_builders.TryGetValue(typeName, out builderObj)) {
                if(type.IsInterface) {
                    var name = typeName.Substring(1);
                    type = _types.FirstOrDefault(x => x.Name == name);
                }
                builder = Expression.Lambda<Func<TActor>>(Expression.New(type)).Compile();
                _builders[typeName] = builder;
            } else {
                builder = builderObj as Func<TActor>;
            }
            return builder;
        }
    }
}