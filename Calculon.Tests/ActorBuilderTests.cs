using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorBuilderTests {
        private Stage _stage;

        [SetUp]
        public void Setup() {
            _stage = new Stage();
        }

        [TearDown]
        public void Teardown() {
            _stage.Dispose();
        }

        [Test]
        public void Can_build_actor_requiring_expression_transport_with_manual_builder() {
            ExpressionTransportActor actor = null;
            _stage.AddActor<ExpressionTransportActor>().BuildWithExpressionTransport(t => actor = new ExpressionTransportActor(t));
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        [Test]
        public void Meta_and_transport_are_injected_for_actor_requiring_expression_transport() {
            ExpressionTransportActor actor = null;
            _stage.AddActor<ExpressionTransportActor>().Build();
            Assert.IsNotNull(actor);
            Assert.IsNotNull(actor.Transport);
        }

        public class ExpressionTransportActor {
            public IExpressionTransport Transport { get; private set; }

            public ExpressionTransportActor(IExpressionTransport expressionTransport) {
                Transport = expressionTransport;
            }
        }

        public class ExpressionTransportActorWithMeta {

        }

        public class MessageTransportActor {

        }

        public class MessageTransportActorWithMeta {

        }

        public class CombinedTransportActor {

        }
        public class CombinedTransportActorWithMeta {

        }

    }
}
