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

namespace Droog.Calculon.Backstage {
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

        protected Message(Guid id, string name, ActorRef sender, ActorRef receiver) {
            Id = id;
            Sender = sender;
            Name = name;
            Receiver = receiver;
        }

        public virtual bool IsFault { get { return false; } }

        public override string ToString() {
            return string.Format("{0}:{1}:{2}", Name, Receiver, Sender);
        }
    }

    public class FailureMessage : Message {
        public static readonly string GlobalName = "Fault";
        public readonly Message Cause;
        public readonly Exception Exception;

        public FailureMessage(Exception exception, Message cause)
            : base(cause.Id, GlobalName, cause.Receiver, cause.Sender) {
            Exception = exception;
            Cause = cause;
        }

        public override bool IsFault { get { return true; } }
    }

    public class ResponseMessage : Message {
        public static readonly string GlobalName = "Response";
        public readonly Message Cause;
        public readonly object Response;

        public ResponseMessage(object response, Message cause)
            : base(cause.Id, GlobalName, cause.Receiver, cause.Sender) {
            Response = response;
            Cause = cause;
        }
    }

    public class TellMessage : Message {
        public readonly object[] Args;

        public TellMessage(string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(Guid.NewGuid(), name, sender, receiver) {
            Args = args;
        }
    }


    public class NotificationMessage : Message {
        public readonly object[] Args;

        public NotificationMessage(Guid id, string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(id, name, sender, receiver) {
            Args = args;
        }
    }

    public class AskMessage : Message {
        public readonly object[] Args;

        public AskMessage(Guid id, string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(id, name, sender, receiver) {
            Args = args;
        }
    }
}