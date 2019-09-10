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
using Newtonsoft.Json;

namespace Iot {
    public class SimpleEventProcessor : IEventProcessor {
        private bool insights = false;
        private static readonly ILog Log = LogManager.GetLogger (typeof (Program));
        private Stopwatch checkpointStopWatch;
        private TelemetryClient telemetryClient;

        public SimpleEventProcessor () {
            // Setup the logger
            var logRepository = LogManager.GetRepository (Assembly.GetEntryAssembly ());
            XmlConfigurator.Configure (logRepository, new FileInfo ("log4net.config"));

            if (!string.IsNullOrEmpty (Environment.GetEnvironmentVariable ("APPINSIGHTS_INSTRUMENTATIONKEY"))) {
                insights = true;
                this.telemetryClient = new TelemetryClient ();
            }
        }

        public Task OpenAsync (PartitionContext context) {
            Log.Info ($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");

            this.checkpointStopWatch = new Stopwatch ();
            this.checkpointStopWatch.Start ();

            return Task.CompletedTask;
        }

        public Task CloseAsync (PartitionContext context, CloseReason reason) {
            Log.Info ($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            this.checkpointStopWatch.Stop ();

            return context.CheckpointAsync ();
        }

        public Task ProcessErrorAsync (PartitionContext context, Exception error) {
            Log.Info ($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            if (insights) telemetryClient.TrackTrace (String.Format ($"Error on Partition: {context.PartitionId}, Error: {error.Message}"));

            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync (PartitionContext context, IEnumerable<EventData> messages) {

            int count = 0;

            // Process the received data for futher processing keep it simple fast and reliable.
            try {
                foreach (EventData message in messages) {

                    // var logContext = $"--- Partition {context.Lease.PartitionId}, Owner: {context.Lease.Owner}, Offset: {message.Body.Offset} --- {DateTime.Now.ToString()}";
                    // Log.Info(logContext);

                    // if(insights) {
                    //    Metric messageRead = telemetryClient.GetMetric("EventMsgProcessed", "Partition");
                    //    messageRead.TrackValue(1, $"Partition${context.Lease.PartitionId}");
                    // }
                    ++count;
                    if (insights) {

                        // IoT Hub specific
                        if (message.SystemProperties.ContainsKey ("iothub-connection-device-id")) {
                            var deviceId = message.SystemProperties["iothub-connection-device-id"].ToString ();
                            telemetryClient.GetMetric ("DeviceEventMsgProcessed", "Device").TrackValue (1, deviceId);

                            //if (message.SystemProperties.ContainsKey ("iothub-connection-module-id")) {
                            //    var moduleId = message.SystemProperties["iothub-connection-module-id"].ToString ();
                            //    telemetryClient.GetMetric ("EventMsgProcessed", "Module").TrackValue (1, deviceId + "/" + moduleId);
                            //}
                        }

                    }

                    string data = $"{Encoding.UTF8.GetString(message.Body.Array)}";

                    var logData = JsonConvert.SerializeObject (new { Partition = context.Lease.PartitionId, Size = data.Length });

                    Log.Info ($"{DateTime.Now.ToString()} {logData}");
                    Log.Debug ($"{data} {Environment.NewLine}");
                }

                if (insights)
                    // Tracking by partitionId
                    telemetryClient.GetMetric ("PartitionEventMsgProcessed", "Partition").TrackValue (count, context.Lease.PartitionId);

                if (this.checkpointStopWatch.Elapsed > TimeSpan.FromSeconds (5)) {
                    if (insights) telemetryClient.GetMetric ("CheckPoint").TrackValue (1);
                    lock (this) {
                        this.checkpointStopWatch.Restart ();
                        return context.CheckpointAsync ();
                    }
                }
            } catch (Exception ex) {
                Log.Error (ex.Message);
                if (insights) telemetryClient.TrackTrace (ex.Message);
            }

            return Task.FromResult<object> (null);
        }
    }
}
