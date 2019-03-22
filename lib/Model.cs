using System;
using Newtonsoft.Json;

namespace Iot.Model
{
    public enum TelemetryType
    {
        CLIMATE
    }

    public class Configuration
    {
        public string EventHubEndpoint { get; set; }
        public string Hub { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string StorageContainer { get; set; }

        public string StorageConnectionString
        {
            get { return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey); }
        }

        public Configuration()
        {
            Hub = Environment.GetEnvironmentVariable("HUB");
            EventHubEndpoint = Environment.GetEnvironmentVariable("EVENT_HUB_ENDPOINT");
            StorageAccountName = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME");
            StorageAccountKey = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY"); ;
            StorageContainer = (Environment.GetEnvironmentVariable("STORAGE_CONTAINER") != null) ? Environment.GetEnvironmentVariable("STORAGE_CONTAINER") : "eph";
        }

        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class Climate
    {
        public string DeviceId { get; set; } = Environment.GetEnvironmentVariable("DEVICE");
        public double WindSpeed { get; set; }
        public double Humidity { get; set; }
        public long TimeStamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long Count { get; set; }

        public Climate()
        {
            Random rand = new Random();
            WindSpeed = 10 + rand.NextDouble() * 4;
            Humidity = 60 + rand.NextDouble() * 20;
        }

        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
