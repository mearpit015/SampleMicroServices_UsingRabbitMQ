using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace MSConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                //UserName = "guest",
               // Password = "guest",
               // VirtualHost = "/",
                //factory.AmqpUriSslProtocols = Protocols.DefaultProtocol();
                //HostName = "0.0.0.0:5672",//"192.168.0.12";
                HostName = "localhost",
               // Port = 15672,
                //RequestedHeartbeat = TimeSpan.FromSeconds(150),
                //AutomaticRecoveryEnabled = true
            };
            var connection = factory.CreateConnection();


            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "food",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "food",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }
    }
}
