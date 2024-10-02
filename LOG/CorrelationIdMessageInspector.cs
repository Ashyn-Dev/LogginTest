using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using log4net;
using log4net.Config;
using Newtonsoft.Json;

namespace ChatAppWCFService
{
    public class CorrelationIdMessageInspector : IDispatchMessageInspector
    {
        // Initialize the logger for this class
        private static readonly ILog log = LogManager.GetLogger(typeof(CorrelationIdMessageInspector));

        public string CorrelationIdHeaderName { get; private set; } = "CorrelationId";
        public string TestValueHeaderName { get; private set; } = "TestValue";

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            string correlationId = null;
            string testValue = null;

            // Log the request message
            try
            {
                // Create a buffered copy of the message
                MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);
                Message requestCopy = buffer.CreateMessage();

                // Log headers and body
                LogMessage("Request", requestCopy);

                // Replace the original request with a new copy to preserve it
                request = buffer.CreateMessage();
            }
            catch (Exception ex)
            {
                log.Error("Error logging request: " + ex.Message, ex);
            }

            // Extract Correlation ID and TestValue from HTTP headers
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object httpRequestMessageObject) &&
                httpRequestMessageObject is HttpRequestMessageProperty httpRequest)
            {
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
            if (Guid.TryParse(correlationId, out Guid activityId))
            {
                System.Diagnostics.Trace.CorrelationManager.ActivityId = activityId;
            }
            else
            {
                activityId = Guid.NewGuid();
                System.Diagnostics.Trace.CorrelationManager.ActivityId = activityId;
            }

            // Store Correlation ID and TestValue for use in the reply
            var extension = new CorrelationIdExtension(correlationId, testValue);
            OperationContext.Current.Extensions.Add(extension);

            return extension;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            string correlationId = null;
            string testValue = null;

            if (correlationState is CorrelationIdExtension extension)
            {
                correlationId = extension.CorrelationId;
                testValue = extension.TestValue;
            }

            // Get or create the HTTP response message property
            if (!reply.Properties.TryGetValue(HttpResponseMessageProperty.Name, out object httpResponseMessageObject) ||
                !(httpResponseMessageObject is HttpResponseMessageProperty httpResponse))
            {
                httpResponse = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
            }

            // Add headers to the response
            httpResponse.Headers[CorrelationIdHeaderName] = correlationId;
            httpResponse.Headers[TestValueHeaderName] = testValue ?? "Doesn't exist";

            // Log the response message
            try
            {
                MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue);
                Message replyCopy = buffer.CreateMessage();

                LogMessage("Response", replyCopy);

                // Replace the original reply with a new copy to preserve it
                reply = buffer.CreateMessage();
            }
            catch (Exception ex)
            {
                log.Error("Error logging response: " + ex.Message, ex);
            }
        }

        //private void LogMessage(string messageType, Message message)
        //{
        //    try
        //    {
        //        // Dictionary to store HTTP headers
        //        var httpHeaders = new Dictionary<string, string>();

        //        // Extract HTTP headers based on whether the message is a request or response
        //        if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object httpRequestMessageObject) &&
        //            httpRequestMessageObject is HttpRequestMessageProperty httpRequest)             
        //        {
        //            foreach (string headerName in httpRequest.Headers.AllKeys)
        //            {
        //                httpHeaders[headerName] = httpRequest.Headers[headerName];
        //            }
        //        }
        //        else if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out object httpResponseMessageObject) &&
        //                 httpResponseMessageObject is HttpResponseMessageProperty httpResponse)
        //        {
        //            foreach (string headerName in httpResponse.Headers.AllKeys)
        //            {
        //                httpHeaders[headerName] = httpResponse.Headers[headerName];
        //            }
        //        }

        //        // Read the body of the message if available
        //        string body = null;
        //        if (!message.IsEmpty)
        //        {
        //            using (var reader = message.GetReaderAtBodyContents())
        //            {
        //                reader.MoveToContent();
        //                body = reader.ReadOuterXml();
        //            }
        //        }

        //        // Retrieve CorrelationId from headers (already being passed)
        //        httpHeaders.TryGetValue(CorrelationIdHeaderName, out string correlationId);

        //        // Create a unique Request ID (UUID)
        //        string requestId = Guid.NewGuid().ToString();

        //        // Determine LOGTYPE based on message type (you can customize this logic if needed)
        //        string logType = messageType.Equals("Request", StringComparison.OrdinalIgnoreCase) ? "REQUEST" : "RESPONSE";

        //        // Construct the log entry according to the desired structure
        //        var logEntry = new
        //        {
        //            Level = messageType.ToUpper(), // Log level, e.g., INFO, ERROR
        //            TraceID = correlationId,       // Correlation ID (from headers)
        //            RequestID = requestId,         // Unique request ID
        //            TEST = "TEST",                 // Sample TEST field
        //            OTHER = "OTHER",               // Sample OTHER field
        //            MESSAGE = new
        //            {
        //                HttpHeaders = httpHeaders, // Combined headers
        //                Body = body                // The message body
        //            },
        //            SAMPLE = "SAMPLE",             // Sample SAMPLE field
        //            LOGTYPE = logType              // Custom log type (REQUEST or RESPONSE)
        //        };

        //        // Serialize the log entry to JSON (single line)
        //        string jsonLog = JsonConvert.SerializeObject(logEntry, Formatting.None);

        //        // Log the serialized JSON using log4net
        //        log.Info(jsonLog);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle logging of exceptions
        //        log.Error("Error logging message: " + ex.Message, ex);
        //    }
        //}
        private void LogMessage(string messageType, Message message)
{
    try
    {
        // Dictionary to store HTTP headers
        var httpHeaders = new Dictionary<string, string>();

        // Extract HTTP headers based on whether the message is a request or response
        if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object httpRequestMessageObject) &&
            httpRequestMessageObject is HttpRequestMessageProperty httpRequest)
        {
            foreach (string headerName in httpRequest.Headers.AllKeys)
            {
                httpHeaders[headerName] = httpRequest.Headers[headerName];
            }
        }
        else if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out object httpResponseMessageObject) &&
                 httpResponseMessageObject is HttpResponseMessageProperty httpResponse)
        {
            foreach (string headerName in httpResponse.Headers.AllKeys)
            {
                httpHeaders[headerName] = httpResponse.Headers[headerName];
            }
        }

        // Read the body of the message if available
        string body = null;
        if (!message.IsEmpty)
        {
            using (var reader = message.GetReaderAtBodyContents())
            {
                reader.MoveToContent();
                body = reader.ReadOuterXml();
            }
        }

        // Retrieve CorrelationId from headers (already being passed)
        httpHeaders.TryGetValue(CorrelationIdHeaderName, out string correlationId);

        // Create a unique Request ID (UUID)
        string requestId = Guid.NewGuid().ToString();

        // Determine LOGTYPE based on message type (you can customize this logic if needed)
        string logType = messageType.Equals("Request", StringComparison.OrdinalIgnoreCase) ? "REQUEST" : "RESPONSE";

        // Construct the log entry according to the desired structure
        var logEntry = new
        {
            Level = "INFO", // Log level, e.g., INFO, ERROR
            TraceID = correlationId,       // Correlation ID (from headers)
            RequestID = requestId,         // Unique request ID
            TEST = "TEST",                 // Sample TEST field
            OTHER = "OTHER",               // Sample OTHER field
            MESSAGE = new
            {
                HttpHeaders = httpHeaders, // Combined headers
                Body = body                // The message body
            },
            SAMPLE = "SAMPLE",             // Sample SAMPLE field
            LOGTYPE = logType              // Custom log type (REQUEST or RESPONSE)
        };

        // Serialize the log entry to JSON (single line)
        string jsonLog = JsonConvert.SerializeObject(logEntry, Formatting.None);

        // Log the serialized JSON using log4net
        log.Info(jsonLog);
    }
    catch (Exception ex)
    {
        // Handle logging of exceptions
        log.Error("Error logging message: " + ex.Message, ex);
    }
}


        //private void LogMessage(string messageType, Message message)
        //{
        //    try
        //    {
        //        var httpHeaders = new Dictionary<string, string>();

        //        if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object httpRequestMessageObject) &&
        //            httpRequestMessageObject is HttpRequestMessageProperty httpRequest)
        //        {
        //            foreach (string headerName in httpRequest.Headers.AllKeys)
        //            {
        //                httpHeaders[headerName] = httpRequest.Headers[headerName];
        //            }
        //        }
        //        else if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out object httpResponseMessageObject) &&
        //                 httpResponseMessageObject is HttpResponseMessageProperty httpResponse)
        //        {
        //            foreach (string headerName in httpResponse.Headers.AllKeys)
        //            {
        //                httpHeaders[headerName] = httpResponse.Headers[headerName];
        //            }
        //        }

        //        // Read the body
        //        string body = null;
        //        if (!message.IsEmpty)
        //        {
        //            using (var reader = message.GetReaderAtBodyContents())
        //            {
        //                reader.MoveToContent();
        //                body = reader.ReadOuterXml();
        //            }
        //        }

        //        // Get CorrelationId from headers if available
        //        httpHeaders.TryGetValue(CorrelationIdHeaderName, out string correlationId);

        //        // Create the log entry
        //        var logEntry = new
        //        {
        //            Timestamp = DateTime.Now,
        //            MessageType = messageType,
        //            CorrelationId = correlationId,
        //            HttpHeaders = httpHeaders,
        //            Body = body
        //        };

        //        // Serialize to JSON
        //        string json = JsonConvert.SerializeObject(logEntry, Formatting.None);

        //        // Log using log4net
        //        log.Info(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error logging message: " + ex.Message, ex);
        //    }
        //}

        private void LogHttpHeaders(HttpRequestMessageProperty httpRequest, string correlationId, string direction)
        {
            log.Info($"{direction} HTTP Request Headers with Correlation ID: {correlationId}");

            foreach (string headerName in httpRequest.Headers.AllKeys)
            {
                string headerValue = httpRequest.Headers[headerName];
                log.Info($"Header: {headerName} = {headerValue}");
            }
        }
    }

    // Definition of CorrelationIdExtension class
    public class CorrelationIdExtension : IExtension<OperationContext>
    {
        public string CorrelationId { get; }
        public string TestValue { get; }

        public CorrelationIdExtension(string correlationId, string testValue)
        {
            CorrelationId = correlationId;
            TestValue = testValue;
        }

        public void Attach(OperationContext owner) { }
        public void Detach(OperationContext owner) { }
    }
}
