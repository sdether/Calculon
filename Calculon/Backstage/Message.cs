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
        public readonly string Contract;
        public readonly object[] Args;
        public readonly MessageType Type;
        public readonly Type Response;

        public Message(Guid id, ActorRef sender, string contract, MessageType type, Type responseType, object[] args) {
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