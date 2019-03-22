using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Iot
{
    public class SimpleEventProcessor : IEventProcessor
    {
        private bool insights = false;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private Stopwatch checkpointStopWatch;
        private TelemetryClient telemetryClient;

        public SimpleEventProcessor()
        {
            // Setup the logger
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")))
            {
                insights = true;
                this.telemetryClient = new TelemetryClient();
                this.telemetryClient.Context.Device.Id = Environment.GetEnvironmentVariable("DEVICE");
                this.telemetryClient.TrackEvent("IoTDeviceSimulator started");
                this.telemetryClient.GetMetric("SimulatorCount").TrackValue(1);
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Log.Info($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();

            return Task.CompletedTask;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Log.Info($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            this.checkpointStopWatch.Stop();

            return context.CheckpointAsync();
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Log.Info($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            if (insights) telemetryClient.TrackTrace(String.Format($"Error on Partition: {context.PartitionId}, Error: {error.Message}"));

            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            // Process the received data for futher processing keep it simple fast and reliable.
            try
            {
                foreach (EventData message in messages)
                {
                    var logContext = $"--- Partition {context.Lease.PartitionId}, Owner: {context.Lease.Owner}, Offset: {message.Body.Offset} --- {DateTime.Now.ToString()}";
                    Log.Debug(logContext);
                    if (insights) telemetryClient.GetMetric("EventMsgProcessed").TrackValue(1);

                    string data = $"Received Message: {Encoding.UTF8.GetString(message.Body.Array)}";
                    Log.Info(data);
                }

                if(this.checkpointStopWatch.Elapsed > TimeSpan.FromSeconds(5))
                {
                    if (insights) telemetryClient.GetMetric("CheckPoint").TrackValue(1);
                    lock (this)
                    {
                        this.checkpointStopWatch.Restart();
                        return context.CheckpointAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                if (insights) telemetryClient.TrackTrace(ex.Message);
            }

            return Task.FromResult<object>(null);
        }
    }
}
