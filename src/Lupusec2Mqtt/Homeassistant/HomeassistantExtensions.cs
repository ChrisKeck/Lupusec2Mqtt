using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Lupusec2Mqtt.Homeassistant
{
    public static class HomeassistantExtensions
    {
        public static IConfigurationBuilder AddHomeassistantConfig(this IConfigurationBuilder builder,
            bool logging = false)
        {
            string path = @"/data/options.json";
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_HOMEASSISTANT__CONFIG")))
            {
                path = Environment.GetEnvironmentVariable("ASPNETCORE_HOMEASSISTANT__CONFIG");
            }

            if (logging)
            {
                Console.WriteLine($"Homeassistant config file path is {path}");
            }

            if (File.Exists(path))
            {
                return builder.Add(new HomeassistantConfigurationSource(path));
            }

            if (path != null)
            {
                if (logging)
                {
                    Console.WriteLine($"Homeassistant config file not found at {path}");
                }
            }

            return builder;
        }

        public static string GetLupusecUrl(this IConfiguration configuration)
        {
            return configuration["Lupusec:Url"] ?? "http://192.168.0.10";
        }

        public static string GetMqttUrl(this IConfiguration configuration)
        {
            return configuration["Mqtt:Server"] ?? "localhost";
        }

        public static int GetMqttPort(this IConfiguration configuration)
        {
            return configuration.GetValue("Mqtt:Port", 1883);
        }


        public static string GetMqttUser(this IConfiguration configuration)
        {
            return configuration["Mqtt:Login"] ?? "";
        }

        public static string GetMqttSecret(this IConfiguration configuration)
        {
            return configuration["Mqtt:Password"] ?? "";
        }

        public static string GetLupusecUser(this IConfiguration configuration)
        {
            return configuration["Lupusec:Login"] ?? "expert56";
        }

        public static string GetLupusecSecret(this IConfiguration configuration)
        {
            return configuration["Lupusec:Password"] ?? "Auditts:";
        }

        public static string GetBasicAuth(this IConfiguration configuration)
        {
            return Convert.ToBase64String(
                                          Encoding.ASCII.GetBytes($"{configuration.GetLupusecUser()}:{configuration.GetLupusecSecret()}"));
        }
    }
}