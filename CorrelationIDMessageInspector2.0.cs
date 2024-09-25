using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace ChatAppWCFService
{
    public class CorrelationIdMessageInspector : IDispatchMessageInspector
    {
        private const string CorrelationIdHeaderName = "CorrelationId";
        private const string TestValueHeaderName = "TestValue";

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            string correlationId = null;
            string testValue = null;

            // Get the HTTP headers from the incoming request
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object httpRequestMessageObject)
                && httpRequestMessageObject is HttpRequestMessageProperty httpRequest)
            {
                // Extract Correlation ID from HTTP headers
                correlationId = httpRequest.Headers[CorrelationIdHeaderName];
                testValue = httpRequest.Headers[TestValueHeaderName];
                LogHttpHeaders(httpRequest, correlationId, "Incoming");

            }

            if (string.IsNullOrEmpty(correlationId))
            {
                // Generate a new Correlation ID if not present
                correlationId = Guid.NewGuid().ToString();
            }

            // Set the ActivityId in the CorrelationManager
            Trace.CorrelationManager.ActivityId = Guid.Parse(correlationId);

            // Store Correlation ID and TestValue for use in the reply
            OperationContext.Current.Extensions.Add(new CorrelationIdExtension(correlationId, testValue));

            // Log the Correlation ID and headers
            //LogHttpHeaders(httpRequest, correlationId, "Incoming");

            return correlationId; // Can be used as the correlationState
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            string correlationId = correlationState as string;
            string testValue = null;

            if (string.IsNullOrEmpty(correlationId))
            {
                // Attempt to retrieve Correlation ID from OperationContext if not passed via correlationState
                var extension = OperationContext.Current.Extensions.Find<CorrelationIdExtension>();
                correlationId = extension?.CorrelationId ?? Guid.NewGuid().ToString();
                testValue = extension?.TestValue;
            }
            else
            {
                // Retrieve TestValue from OperationContext
                var extension = OperationContext.Current.Extensions.Find<CorrelationIdExtension>();
                testValue = extension?.TestValue;
            }

            // Get or create the HTTP response message property
            if (!reply.Properties.TryGetValue(HttpResponseMessageProperty.Name, out object httpResponseMessageObject)
                || !(httpResponseMessageObject is HttpResponseMessageProperty httpResponse))
            {
                httpResponse = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
            }

            // Add the Correlation ID to the HTTP response headers
            httpResponse.Headers[CorrelationIdHeaderName] = correlationId;

            // Add TestValue header to the HTTP response headers
            httpResponse.Headers[TestValueHeaderName] = testValue ?? "Doesn't exist";

            // Log the Correlation ID and headers
            LogHttpHeaders(httpResponse, correlationId, "Outgoing");
        }

        private void LogHttpHeaders(HttpRequestMessageProperty httpRequest, string correlationId, string direction)
        {
            Trace.WriteLine($"{direction} HTTP Request Headers with Correlation ID: {correlationId}");

            foreach (string headerName in httpRequest.Headers.AllKeys)
            {
                string headerValue = httpRequest.Headers[headerName];
                Trace.WriteLine($"Header: {headerName} = {headerValue}");
            }
        }

        private void LogHttpHeaders(HttpResponseMessageProperty httpResponse, string correlationId, string direction)
        {
            Trace.WriteLine($"{direction} HTTP Response Headers with Correlation ID: {correlationId}");

            foreach (string headerName in httpResponse.Headers.AllKeys)
            {
                string headerValue = httpResponse.Headers[headerName];
                Trace.WriteLine($"Header: {headerName} = {headerValue}");
            }
        }
    }

    // Helper class to store Correlation ID and TestValue in OperationContext
    public class CorrelationIdExtension : IExtension<OperationContext>
    {
        public string CorrelationId { get; private set; }
        public string TestValue { get; private set; }

        public CorrelationIdExtension(string correlationId, string testValue)
        {
            CorrelationId = correlationId;
            TestValue = testValue;
        }

        public void Attach(OperationContext owner) { }

        public void Detach(OperationContext owner) { }
    }
}
