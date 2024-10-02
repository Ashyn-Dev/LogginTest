using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace ChatAppWCFService
{
    public class CorrelationIdBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(CorrelationIdBehavior);

        protected override object CreateBehavior()
        {
            return new CorrelationIdBehavior();
        }
    }
}
