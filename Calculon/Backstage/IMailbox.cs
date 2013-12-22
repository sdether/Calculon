using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public interface IMailbox<TActor> : IMailbox where TActor: class {
        TActor BuildProxy(IMailbox sender);
    }

    public interface IMailbox {
        ActorRef Ref { get; }
        bool IsMailboxFor<TActor>();
        IMailbox<TActor> As<TActor>() where TActor : class;
        MessageResponse CreatePendingResponse(Type type);
        void Enqueue(Message msg);
    }
}