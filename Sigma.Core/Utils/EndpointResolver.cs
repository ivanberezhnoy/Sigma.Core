using System;
using HotelManager;

namespace Sigma.Core.Utils
{
    public static class EndpointResolver
    {
        public const string ProductionEndpointUrl = "http://192.168.1.152/HotelManager/ws/ws2.1cws";
        public const string DeveloperEndpointUrl = "http://192.168.1.152/ApartmentDeveloper/ws/ws2.1cws";

        public static EndpointType Normalize(EndpointType? endpoint)
        {
            return endpoint ?? EndpointType.Production;
        }

        public static string GetEndpointKey(EndpointType endpoint)
        {
            return endpoint.ToString();
        }

        public static EndpointType ParseEndpointKey(string? endpoint)
        {
            if (Enum.TryParse<EndpointType>(endpoint, true, out var parsed))
            {
                return parsed;
            }

            return EndpointType.Production;
        }

        public static string GetEndpointUrl(EndpointType endpoint)
        {
            return endpoint switch
            {
                EndpointType.Production => ProductionEndpointUrl,
                EndpointType.Developer => DeveloperEndpointUrl,
                _ => ProductionEndpointUrl
            };
        }

        public static EndpointType GetEndpointTypeFromUrl(string url)
        {
            if (string.Equals(url, ProductionEndpointUrl, StringComparison.OrdinalIgnoreCase))
            {
                return EndpointType.Production;
            }

            if (string.Equals(url, DeveloperEndpointUrl, StringComparison.OrdinalIgnoreCase))
            {
                return EndpointType.Developer;
            }

            return EndpointType.Production;
        }

        public static string GetEndpointKey(HotelManagerPortTypeClient session)
        {
            var endpointType = GetEndpointTypeFromUrl(session.Endpoint.Address.Uri.AbsoluteUri);
            return GetEndpointKey(endpointType);
        }
    }
}
