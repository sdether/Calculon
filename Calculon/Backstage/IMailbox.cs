using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public interface IMailbox<TActor> : IMailbox where TActor: class {
        void EnqueueExpression<TResult>(Guid id, ActorRef sender, Func<TActor, Task<TResult>> expr);
        void EnqueueExpression(Guid id, ActorRef sender, Func<TActor, Task> expr);
        void EnqueueExpression(Guid id, ActorRef sender, Action<TActor> expr);
        void SetInstance(TActor instance);
        TActor BuildProxy(IMailbox sender);
    }

    public interface IMailbox {
        ActorRef Ref { get; }
        void EnqueueResponseMessage<TResult>(Guid id, ActorRef sender, TResult result);
        bool IsMailboxFor<TActor>();
        IMailbox<TActor> As<TActor>() where TActor : class;
        MessageResponse<TResult> CreatePendingResponse<TResult>();
    }
}