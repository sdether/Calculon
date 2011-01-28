using Droog.Calculon.Framework;

namespace Droog.Calculon {
    public interface IDispatcher {
        void Dispatch<TData>(Message<TData> message);
        void Dispatch<TRecipient>(ExpressionMessage<TRecipient> message);
    }
}