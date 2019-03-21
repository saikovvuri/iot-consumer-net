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
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        private static EventProcessorHost eventProcessorHost;

        private static async Task MainAsync(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AssemblyLoadContext.Default.Unloading += Default_Unloading;

            // Setup the configuration File
            var config = new Configuration();

            eventProcessorHost = new EventProcessorHost(
                config.Hub,
                PartitionReceiver.DefaultConsumerGroupName,
                config.EventHubEndpoint,
                config.StorageConnectionString,
                config.StorageContainer);

            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            // Console.WriteLine("Receiving. Press ENTER to stop worker.");
            // Console.ReadLine();

            // // Disposes of the Event Processor Host
            // await eventProcessorHost.UnregisterEventProcessorAsync();
        }
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(2000);
                }
            });

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
            _closing.WaitOne();
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            eventProcessorHost.UnregisterEventProcessorAsync().GetAwaiter().GetResult();
            Console.WriteLine("unload");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("process exit");
        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            // eventProcessorHost.UnregisterEventProcessorAsync().GetAwaiter().GetResult();
            Console.WriteLine("Exit");
            _closing.Set();
        }
    }
}
