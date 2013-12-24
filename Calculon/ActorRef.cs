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
using System.Linq;

namespace Droog.Calculon {
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
            var parts = address.Split(new[] { "://" }, 2, StringSplitOptions.None);
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
            return Scheme + "://" + string.Join("/", Path);
        }
    }
}