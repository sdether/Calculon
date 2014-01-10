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
using System.Reflection;
using System.Text;

namespace Droog.Calculon.Backstage.Messages {
    public abstract class Message {

        // TODO: This can be cached
        public static string GetMessageNameFromMethodInfo(MethodInfo methodInfo) {
            var sb = new StringBuilder();
            sb.Append(methodInfo.Name);
            sb.Append("(");
            foreach(var param in methodInfo.GetParameters()) {
                sb.Append(param.ParameterType.Name);
                sb.Append(",");
            }
            sb.Append(")");
            return sb.ToString();
        }

        public readonly Guid Id;
        public readonly ActorRef Sender;
        public readonly ActorRef Receiver;
        public readonly string Name;

        protected Message(string name, ActorRef sender, ActorRef receiver, Guid id = default(Guid)) {
            Id = id;
            if(Id == default(Guid)) {
                Id = Guid.NewGuid();
            }
            Sender = sender;
            Name = name;
            Receiver = receiver;
        }

        protected Message(SystemMessageNames name, ActorRef sender, ActorRef receiver, Guid id = default(Guid)) {
            Id = id;
            if(Id == default(Guid)) {
                Id = Guid.NewGuid();
            }
            Sender = sender;
            Name = name.ToString();
            Receiver = receiver;
        }

        public virtual bool IsFatalFault { get { return false; } }

        public override string ToString() {
            return string.Format("{0}:{1}:{2}", Name, Receiver, Sender);
        }

        public Message Wrap(ActorRef sender, ActorRef receiver) {
            return new WrappedMessage(sender, receiver, this);
        }

        public virtual Message Unwrap() {
            return this;
        }
    }
}