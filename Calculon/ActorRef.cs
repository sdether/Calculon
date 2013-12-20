using System;
using System.Linq;

namespace Droog.Calculon {

    public class ActorProxy<TActor> {
        public readonly ActorRef Ref;
        public readonly TActor Proxy;

        public ActorProxy(ActorRef actorRef, TActor proxy) {
            Ref = actorRef;
            Proxy = proxy;
        }
    }
    public class ActorRef {

        public static ActorRef Parse(string address) {
            string[] path;
            if(address.StartsWith("..")) {
                path = address.Split('/');
                return new ActorRef(path, isRelative: true);
            }
            if(address.StartsWith("/")) {
                path = address.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                return new ActorRef(path);
            }
            var parts = address.Split(new[] { ":" }, 2, StringSplitOptions.None);
            path = parts[1].Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            return new ActorRef(path, scheme: parts[0]);
        }

        public readonly string Scheme;
        public readonly string[] Path;
        public readonly bool IsRelative;

        private ActorRef(string[] path, bool isRelative = false, string scheme = "calculon") {
            Scheme = scheme ?? "calculon";
            Path = path;
            IsRelative = isRelative;
        }

        public string LocalName { get { return Path.Last(); } }
        public ActorRef At(string name) {
            return new ActorRef(Path.Concat(new[] { name }).ToArray(), IsRelative, Scheme);
        }

        public ActorRef AsChildOf(ActorRef parent) {
            if(!IsRelative) {
                throw new Exception("Address isn't relative");
            }
            return new ActorRef(parent.Path.Concat(Path.Skip(1)).ToArray(), parent.IsRelative, parent.Scheme);
        }

        public ActorRef RelativeTo(ActorRef parent) {
            return null;
        }

        public override string ToString() {
            if(IsRelative) {
                return string.Join("/", Path);
            }
            return Scheme + ":" + string.Join("/", Path);
        }
    }
}