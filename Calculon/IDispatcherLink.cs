using Droog.Calculon.Framework;

namespace Droog.Calculon {
    public interface IDispatcherLink {
        bool Dispatch<TData>(Message<TData> message);
        bool Dispatch<TRecipient>(ExpressionMessage<TRecipient> message);
    }
}