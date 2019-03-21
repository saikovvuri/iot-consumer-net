using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Iot
{
    public class SimpleEventProcessor : IEventProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private Stopwatch checkpointStopWatch;

        public SimpleEventProcessor()
        {
            // Setup the logger
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
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

            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            // Process the received data for futher processing keep it simple fast and reliable.
            try
            {
                foreach (EventData message in messages)
                {
                    var logData = $"{DateTime.Now.ToString()} Message Received: Partition {context.Lease.PartitionId}, Owner: {context.Lease.Owner}, Offset: { message.Body.Offset}";
                    Log.Info(logData);
                    Log.Info(Encoding.UTF8.GetString(message.Body.Array));
                }

                if(this.checkpointStopWatch.Elapsed > TimeSpan.FromSeconds(5))
                {
                    lock (this)
                    {
                        this.checkpointStopWatch.Restart();
                        return context.CheckpointAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling here.
                Log.Error(ex.Message);
            }

            return Task.FromResult<object>(null);
        }
    }
}
