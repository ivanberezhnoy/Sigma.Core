using System.Text.Json.Serialization;

namespace Sigma.Core.Utils
{
    public enum EndpointType
    {
        Production = 0,
        Developer = 1
    }

    public class CredentionalInfo
    {
        public CredentionalInfo(string userName, string? password, EndpointType? endpoint = null)
        {
            UserName = userName;
            Password = password;
            Endpoint = endpoint;
        }

        public string UserName { get; set; }
        public string? Password { get; set; }

        /// <summary>
        /// Target endpoint identifier. If null, the production endpoint is used.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EndpointType? Endpoint { get; set; }
    }
}
