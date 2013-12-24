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
    public class Message {

        public static string GetContractFromMethodInfo(MethodInfo methodInfo) {
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
        public readonly string Contract;
        public readonly object[] Args;
        public readonly MessageType Type;
        public readonly Type Response;

        public Message(Guid id, ActorRef sender, ActorRef receiver, string contract, MessageType type, Type responseType, object[] args) {
            Id = id;
            Sender = sender;
            Type = type;
            Contract = contract;
            Response = responseType;
            Args = args;
        }

        public string Signature { get { return (Type == MessageType.Fault || Type == MessageType.Response) ? Type.ToString() : Contract; } }

        public override string ToString() {
            return string.Format("{0}:{1}", Type, Contract);
        }
    }
}