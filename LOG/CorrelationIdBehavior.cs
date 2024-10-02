using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ChatAppWCFService
{
    public class CorrelationIdBehavior : IServiceBehavior
    {
        // Remove the parameterized constructor
        // public CorrelationIdBehavior(string logFilePath)
        // {
        //     this.logFilePath = logFilePath;
        // }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // No binding parameters to add
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in dispatcher.Endpoints)
                {
                    // Instantiate the inspector without parameters
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorrelationIdMessageInspector());
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // No validation needed
        }
    }
}