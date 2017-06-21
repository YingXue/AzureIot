using System;
using System.Text;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace ReadCriticalQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Receive critical messages. Ctrl-C to exit.\n");
            var connectionString = "Endpoint=sb://yingtest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Wi7lWQ8f3hHfdOSAO2IjjUSNw1B9KBTkpnN7i+lZEdU=";
            var queueName = "criticalqueue";

            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            client.OnMessage(message =>
            {
                Stream stream = message.GetBody<Stream>();
                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                string s = reader.ReadToEnd();
                Console.WriteLine(String.Format("Message body: {0}", s));
            });

            Console.ReadLine();
        }
    }
}
