using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.Calculon.Framework {
    public class DispatchChain : IDispatcher {

        private IList<IDispatcherLink> _dispatchers = new List<IDispatcherLink>();

        public void Dispatch<TData>(Message<TData> message) {
            (from d in _dispatchers where d.Dispatch(message) select d).FirstOrDefault();
        }

        public void Dispatch<TRecipient>(ExpressionMessage<TRecipient> message) {
            (from d in _dispatchers where d.Dispatch(message) select d).FirstOrDefault();
        }
    }
}
