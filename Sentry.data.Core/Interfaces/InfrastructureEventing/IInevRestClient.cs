//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."
#pragma warning disable 8073 // Disable "CS8073 The result of the expression is always 'false' since a value of type 'T' is never equal to 'null' of type 'T?'"
#pragma warning disable 3016 // Disable "CS3016 Arrays as attribute arguments is not CLS-compliant"
#pragma warning disable 8603 // Disable "CS8603 Possible null reference return"

namespace Sentry.data.Core.Interfaces.InfrastructureEventing
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial interface IInevRestClient
    {
        /// <summary>
        /// Shuts down all consumers for a consumer group.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> ShutDownConsumersUsingPUTAsync(string groupId);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Shuts down all consumers for a consumer group.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> ShutDownConsumersUsingPUTAsync(string groupId, System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// Resets offsets to the beginning for given topic and group id.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="topic">topic</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> ResetGroupUsingPUTAsync(string groupId, string topic);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Resets offsets to the beginning for given topic and group id.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="topic">topic</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> ResetGroupUsingPUTAsync(string groupId, string topic, System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// List all topics in the consumer's cluster.
        /// </summary>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<TopicResponse> ListUsingGETAsync();

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// List all topics in the consumer's cluster.
        /// </summary>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<TopicResponse> ListUsingGETAsync(System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// Posts a message to a topic.
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<string> PublishUsingPOSTAsync(Message message);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Posts a message to a topic.
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<string> PublishUsingPOSTAsync(Message message, System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// Polls for messages within a topic given a group and client ID.
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="groupId">groupId</param>
        /// <param name="clientId">clientId</param>
        /// <param name="records">records</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<MessageResponse> ConsumeGroupClientUsingGETAsync(string topic, string groupId, string clientId, int records);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Polls for messages within a topic given a group and client ID.
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="groupId">groupId</param>
        /// <param name="clientId">clientId</param>
        /// <param name="records">records</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<MessageResponse> ConsumeGroupClientUsingGETAsync(string topic, string groupId, string clientId, int records, System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// Polls for messages within a topic given a group ID
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="groupId">groupId</param>
        /// <param name="records">records</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<MessageResponse> ConsumeGroupUsingGETAsync(string topic, string groupId, int records);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Polls for messages within a topic given a group ID
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="groupId">groupId</param>
        /// <param name="records">records</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<MessageResponse> ConsumeGroupUsingGETAsync(string topic, string groupId, int records, System.Threading.CancellationToken cancellationToken);

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class Message
    {
        [Newtonsoft.Json.JsonProperty("details", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.IDictionary<string, string> Details { get; set; }

        [Newtonsoft.Json.JsonProperty("eventType", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string EventType { get; set; }

        [Newtonsoft.Json.JsonProperty("key", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Key { get; set; }

        [Newtonsoft.Json.JsonProperty("messageSource", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string MessageSource { get; set; }

        [Newtonsoft.Json.JsonProperty("saidKey", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string SaidKey { get; set; }

        [Newtonsoft.Json.JsonProperty("topic", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Topic { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class MessageResponse
    {
        [Newtonsoft.Json.JsonProperty("messages", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Message> Messages { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class TopicResponse
    {
        [Newtonsoft.Json.JsonProperty("topics", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> Topics { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class FileResponse : System.IDisposable
    {
        private System.IDisposable _client;
        private System.IDisposable _response;

        public int StatusCode { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public System.IO.Stream Stream { get; private set; }

        public bool IsPartial
        {
            get { return StatusCode == 206; }
        }

        public FileResponse(int statusCode, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.IO.Stream stream, System.IDisposable client, System.IDisposable response)
        {
            StatusCode = statusCode;
            Headers = headers;
            Stream = stream;
            _client = client;
            _response = response;
        }

        public void Dispose()
        {
            Stream.Dispose();
            if (_response != null)
                _response.Dispose();
            if (_client != null)
                _client.Dispose();
        }
    }


    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

#pragma warning restore 1591
#pragma warning restore 1573
#pragma warning restore  472
#pragma warning restore  114
#pragma warning restore  108
#pragma warning restore 3016
#pragma warning restore 8603