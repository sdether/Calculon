/*
 * Calculon 
 * Copyright (C) 2011 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Linq.Expressions;
using MindTouch.Tasking;

namespace Droog.Calculon.Framework {
    public abstract class ExpressionMessage<TRecipient> : IMessage {
        private readonly MessageMeta _meta;

        protected ExpressionMessage(ActorAddress sender, ActorAddress recipient) {
            _meta = new MessageMeta(typeof(ExpressionMessage<TRecipient>), sender, recipient);
        }

        public MessageMeta Meta { get { return _meta; } }

        public void Undeliverable() { Throw(new NoSuchRecipientException(Meta)); }

        public abstract void Invoke(TRecipient recipient);
        public abstract void Throw(Exception e);
    }

    public class ExpressionActionMessage<TRecipient> : ExpressionMessage<TRecipient> {

        private readonly Expression<Action<TRecipient>> _expression;
        private readonly Result _result;

        public ExpressionActionMessage(ActorAddress sender, ActorAddress recipient, Expression<Action<TRecipient>> expression, Result result)
            : base(sender, recipient) {
            _expression = expression;
            _result = result;
        }

        public ExpressionActionMessage(ActorAddress sender, ActorAddress recipient, Expression<Action<TRecipient>> expression)
            : base(sender, recipient) {
            _expression = expression;
        }

        public override void Invoke(TRecipient recipient) {
            try {
                _expression.Compile()(recipient);
                if(_result != null) {
                    _result.Return();
                }
            } catch(Exception e) {
                if(_result != null) {
                    _result.Throw(e);
                }
            }
        }

        public override void Throw(Exception e) {
            _result.Throw(e);
        }
    }

    public class ExpressionActionMessageWithMeta<TRecipient> : ExpressionMessage<TRecipient> {

        private readonly Expression<Action<TRecipient, MessageMeta>> _expression;
        private readonly Result _result;

        public ExpressionActionMessageWithMeta(ActorAddress sender, ActorAddress recipient, Expression<Action<TRecipient, MessageMeta>> expression, Result result)
            : base(sender, recipient) {
            _expression = expression;
            _result = result;
        }

        public ExpressionActionMessageWithMeta(ActorAddress sender, ActorAddress recipient, Expression<Action<TRecipient, MessageMeta>> expression)
            : base(sender, recipient) {
            _expression = expression;
        }

        public override void Invoke(TRecipient recipient) {
            try {
                _expression.Compile()(recipient, Meta);
                if(_result != null) {
                    _result.Return();
                }
            } catch(Exception e) {
                if(_result != null) {
                    _result.Throw(e);
                }
            }
        }

        public override void Throw(Exception e) {
            _result.Throw(e);
        }
    }

    public class ExpressionFuncMessage<TRecipient, TResponse> : ExpressionMessage<TRecipient> {

        private readonly Expression<Func<TRecipient, TResponse>> _expression;
        private readonly Result<TResponse> _result;

        public ExpressionFuncMessage(ActorAddress sender, ActorAddress recipient, Expression<Func<TRecipient, TResponse>> expression, Result<TResponse> result)
            : base(sender, recipient) {
            _expression = expression;
            _result = result;
        }

        public override void Invoke(TRecipient recipient) {
            try {
                _result.Return(_expression.Compile()(recipient));
            } catch(Exception e) {
                _result.Throw(e);
            }
        }

        public override void Throw(Exception e) {
            _result.Throw(e);
        }
    }

    public class ExpressionFuncMessageWithMeta<TRecipient, TResponse> : ExpressionMessage<TRecipient> {

        private readonly Expression<Func<TRecipient, MessageMeta, TResponse>> _expression;
        private readonly Result<TResponse> _result;

        public ExpressionFuncMessageWithMeta(ActorAddress sender, ActorAddress recipient, Expression<Func<TRecipient, MessageMeta, TResponse>> expression, Result<TResponse> result)
            : base(sender, recipient) {
            _expression = expression;
            _result = result;
        }

        public override void Invoke(TRecipient recipient) {
            try {
                _result.Return(_expression.Compile()(recipient, Meta));
            } catch(Exception e) {
                _result.Throw(e);
            }
        }

        public override void Throw(Exception e) {
            _result.Throw(e);
        }
    }
}