using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;

namespace SimulatedDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "Ying-S1Hub.azure-devices.net";
        static string deviceKey = "tiESM34G/OWJ97eDXcyRwrPhpo5hgrJIasInnEWL1EU=";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            ReceiveC2dAsync();
            SendToBlobAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    deviceId = "myFirstDevice",
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                string levelValue;

                //This method randomly adds the property "level": "critical" to messages sent by the device, 
                //which simulates a message that requires immediate action by the solution back-end. 
                //The device app passes this information in the message properties, instead of in the message body, 
                //so that IoT Hub can route the message to the proper message destination.
                if (rand.NextDouble() > 0.7)
                {
                    messageString = "This is a critical message";
                    levelValue = "critical";
                }
                else
                {
                    levelValue = "normal";
                }

                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", levelValue);

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));

                foreach (var key in receivedMessage.Properties.Keys)
                {
                    Console.WriteLine("Received message key: {0}, value {1} ", key, receivedMessage.Properties[key]);
                }
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }
        private static async void SendToBlobAsync()
        {
            string fileName = "image.jpg";
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(fileName, FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(fileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }
    }
}
