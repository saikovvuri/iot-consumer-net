using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Iot.Model;
using log4net;
using log4net.Config;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Iot
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static readonly AutoResetEvent exitEvent = new AutoResetEvent(false);

        private static EventProcessorHost eventProcessorHost;

        private static async Task MainAsync(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            AssemblyLoadContext.Default.Unloading += OnExit;

            // Setup the Options
            var epo = new EventProcessorOptions
            {
                InitialOffsetProvider = (partitionId) => EventPosition.FromEnqueuedTime(DateTime.UtcNow)
            };

            // Setup the configuration File
            var config = new Configuration();

            var consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;
            if (!string.IsNullOrEmpty(config.ConsumerGroupName))
            {
                consumerGroupName = config.ConsumerGroupName;
            }
            else
            {
                consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;
            }

            eventProcessorHost = new EventProcessorHost(
                config.Hub,
                consumerGroupName,
                config.EventHubEndpoint,
                config.StorageConnectionString,
                config.StorageContainer);

            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(epo);
        }
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
            exitEvent.WaitOne();
        }

        private static void OnExit(AssemblyLoadContext obj)
        {
            eventProcessorHost.UnregisterEventProcessorAsync().GetAwaiter().GetResult();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            eventProcessorHost.UnregisterEventProcessorAsync().GetAwaiter().GetResult();
        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            eventProcessorHost.UnregisterEventProcessorAsync().GetAwaiter().GetResult();
            exitEvent.Set();
            Console.WriteLine("Exit Completed");
        }
    }
}
