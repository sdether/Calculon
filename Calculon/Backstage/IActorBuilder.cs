/* ----------------------------------------------------------------------------
 * Copyright (C) 2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/Calculon
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * ----------------------------------------------------------------------------
 */

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